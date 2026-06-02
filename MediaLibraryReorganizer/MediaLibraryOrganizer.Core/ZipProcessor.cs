// <copyright file="ZipProcessor.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using Serilog;

    /// <summary>
    /// Handles the processing and extraction of ZIP files in a media library.
    /// </summary>
    public class ZipProcessor
    {
        private const long MaxUncompressedSize = 1024L * 1024L * 1024L;

        private readonly HashSet<string> processedZips;
        private readonly DirectoryManager directoryManager;
        private readonly bool fingerprintZipContents;
        private readonly bool copyOnly;
        private readonly int zipNestingDepthCutoff;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipProcessor"/> class.
        /// </summary>
        /// <param name="directoryManager">The directory manager dependency.</param>
        /// <param name="fingerprintZipContents">When true, use MD5 for zip de-dupe; when false, use path/size/time key (no hash).</param>
        /// <param name="copyOnly">When true, copy bad zips to the error folder instead of moving.</param>
        /// <param name="zipNestingDepthCutoff">Skip expansion when current nesting depth is greater than or equal to this value; use <see cref="int.MaxValue"/> for no practical limit.</param>
        public ZipProcessor(DirectoryManager directoryManager, bool fingerprintZipContents, bool copyOnly, int zipNestingDepthCutoff)
        {
            this.directoryManager = directoryManager;
            this.fingerprintZipContents = fingerprintZipContents;
            this.copyOnly = copyOnly;
            this.zipNestingDepthCutoff = zipNestingDepthCutoff;
            this.processedZips = new HashSet<string>();
        }

        /// <summary>
        /// Expands one archive under <c>{MediaLibrary}/SokkaCorp/Unzipped</c>, runs <paramref name="processExtractedRoot"/>, then deletes that scratch folder.
        /// The same archive identity is not expanded twice in one run (<see cref="processedZips"/>). Nested archives are handled by calling this again from inside <paramref name="processExtractedRoot"/>.
        /// </summary>
        /// <param name="zipFile">Archive to expand.</param>
        /// <param name="zipNestingDepth">Number of outer archives already expanded (0 for zips on the source tree).</param>
        /// <param name="processExtractedRoot">Processes files and subfolders under the extraction root (non-recursive for zips — use <see cref="ExtractProcessAndCleanup"/> for nested <c>.zip</c> files).</param>
        public void ExtractProcessAndCleanup(FileInfo zipFile, int zipNestingDepth, Action<DirectoryInfo> processExtractedRoot)
        {
            if (zipFile == null)
            {
                throw new ArgumentNullException(nameof(zipFile));
            }

            if (processExtractedRoot == null)
            {
                throw new ArgumentNullException(nameof(processExtractedRoot));
            }

            if (zipNestingDepth >= this.zipNestingDepthCutoff)
            {
                Log.Warning(
                    "Zip nesting depth limit ({Cap}) reached; skipping \"{Path}\".",
                    this.zipNestingDepthCutoff,
                    zipFile.FullName);
                return;
            }

            StaticLog.Enter(nameof(this.ExtractProcessAndCleanup));
            string? extractPath = null;
            bool extracted = false;
            string zipKey = this.GetZipIdentity(zipFile);
            try
            {
                if (this.processedZips.Contains(zipKey))
                {
                    Log.Information($"Skipping already processed zip file: {zipFile.FullName}");
                    return;
                }

                if (!this.ValidateZipFile(zipFile))
                {
                    Log.Warning($"Zip file validation failed for {zipFile.FullName}");
                    this.MoveToErrorDirectory(zipFile);
                    return;
                }

                extractPath = this.GetUniqueExtractionPath(zipFile);
                Log.Information($"Extracting {zipFile.FullName} to {extractPath}");
                ZipFile.ExtractToDirectory(zipFile.FullName, extractPath, true);
                extracted = true;
                this.processedZips.Add(zipKey);
                processExtractedRoot(new DirectoryInfo(extractPath));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to process zip file {zipFile.FullName}");
                if (extracted)
                {
                    this.processedZips.Remove(zipKey);
                }
                else
                {
                    this.MoveToErrorDirectory(zipFile);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(extractPath) && Directory.Exists(extractPath))
                {
                    try
                    {
                        Directory.Delete(extractPath, true);
                    }
                    catch (Exception delEx)
                    {
                        Log.Error(delEx, "Could not delete extraction directory {Path}", extractPath);
                    }
                }

                StaticLog.Exit(nameof(this.ExtractProcessAndCleanup));
            }
        }

        private string GetZipIdentity(FileInfo zipFile)
        {
            if (this.fingerprintZipContents)
            {
                return Statics.GetChecksum(zipFile);
            }

            return $"{zipFile.FullName}|{zipFile.Length}|{zipFile.LastWriteTimeUtc:o}";
        }

        private bool ValidateZipFile(FileInfo zipFile)
        {
            try
            {
                using ZipArchive archive = ZipFile.OpenRead(zipFile.FullName);
                long totalUncompressedSize = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Contains("..") || Path.IsPathRooted(entry.FullName))
                    {
                        Log.Warning($"Suspicious zip entry detected in {zipFile.FullName}: {entry.FullName}");
                        return false;
                    }

                    totalUncompressedSize += entry.Length;
                    if (totalUncompressedSize > MaxUncompressedSize)
                    {
                        Log.Warning($"Zip file {zipFile.FullName} exceeds maximum uncompressed size limit of {MaxUncompressedSize} bytes");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to validate zip file {zipFile.FullName}");
                return false;
            }
        }

        private string GetUniqueExtractionPath(FileInfo zipFile)
        {
            string baseDir = this.directoryManager.GetUnzippedDirectory().FullName;
            string uniqueName = $"{Path.GetFileNameWithoutExtension(zipFile.Name)}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
            return Path.Combine(baseDir, uniqueName);
        }

        private void MoveToErrorDirectory(FileInfo zipFile)
        {
            try
            {
                DirectoryInfo errorDir = this.directoryManager.GetErrorMiscDirectory();
                string destPath = Path.Combine(errorDir.FullName, zipFile.Name);

                if (File.Exists(destPath))
                {
                    destPath = Path.Combine(
                        errorDir.FullName,
                        $"{Path.GetFileNameWithoutExtension(zipFile.Name)}_{DateTime.Now:yyyyMMddHHmmss}{zipFile.Extension}");
                }

                if (this.copyOnly || (zipFile.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    zipFile.CopyTo(destPath);
                }
                else
                {
                    zipFile.MoveTo(destPath);
                }

                Log.Information($"Moved problematic zip file to error directory: {destPath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to move zip file {zipFile.FullName} to error directory");
            }
        }
    }
}
