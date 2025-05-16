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
    using static Statics; // Keep this for StaticLog and GetChecksum for now

    /// <summary>
    /// Handles the processing and extraction of ZIP files in a media library.
    /// </summary>
    public class ZipProcessor
    {
        /// <summary>
        /// The maximum uncompressed size limit for ZIP files (1GB).
        /// </summary>
        private const long MaxUncompressedSize = 1024L * 1024L * 1024L; // 1GB

        /// <summary>
        /// The maximum allowed nesting depth for ZIP files.
        /// </summary>
        private const int MaxDepth = 5;

        /// <summary>
        /// Set of processed ZIP file checksums to avoid reprocessing.
        /// </summary>
        private readonly HashSet<string> processedZips;

        /// <summary>
        /// The directory manager dependency.
        /// </summary>
        private readonly DirectoryManager directoryManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipProcessor"/> class.
        /// </summary>
        /// <param name="directoryManager">The directory manager dependency.</param>
        public ZipProcessor(DirectoryManager directoryManager)
        {
            this.directoryManager = directoryManager;
            this.processedZips = new HashSet<string>();
        }

        /// <summary>
        /// Recursively processes ZIP files in the specified directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory to process.</param>
        public void ProcessZipsRecursively(DirectoryInfo sourceDirectory)
        {
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException(nameof(sourceDirectory));
            }

            StaticLog.Enter(nameof(this.ProcessZipsRecursively));
            try
            {
                this.ProcessZipsInDirectory(sourceDirectory, 0);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessZipsRecursively));
            }
        }

        /// <summary>
        /// Processes ZIP files in the specified directory at the current depth.
        /// </summary>
        /// <param name="directory">The directory to process.</param>
        /// <param name="currentDepth">The current nesting depth.</param>
        private void ProcessZipsInDirectory(DirectoryInfo directory, int currentDepth)
        {
            if (currentDepth >= MaxDepth)
            {
                Log.Warning($"Maximum zip nesting depth ({MaxDepth}) reached at {directory.FullName}. Skipping deeper processing.");
                return;
            }

            IEnumerable<FileInfo> zipFiles = directory.EnumerateFiles("*.zip", SearchOption.AllDirectories);

            foreach (FileInfo zipFile in zipFiles)
            {
                try
                {
                    this.ProcessSingleZip(zipFile, currentDepth);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error processing zip file {zipFile.FullName}");
                    this.MoveToErrorDirectory(zipFile);
                }
            }
        }

        /// <summary>
        /// Processes a single ZIP file.
        /// </summary>
        /// <param name="zipFile">The ZIP file to process.</param>
        /// <param name="currentDepth">The current nesting depth.</param>
        private void ProcessSingleZip(FileInfo zipFile, int currentDepth)
        {
            string zipChecksum = GetChecksum(zipFile);

            if (this.processedZips.Contains(zipChecksum))
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

            string extractPath = this.GetUniqueExtractionPath(zipFile);

            try
            {
                Log.Information($"Extracting {zipFile.FullName} to {extractPath}");
                ZipFile.ExtractToDirectory(zipFile.FullName, extractPath, true);
                this.processedZips.Add(zipChecksum);

                this.ProcessZipsInDirectory(new DirectoryInfo(extractPath), currentDepth + 1);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to extract zip file {zipFile.FullName}");
                this.MoveToErrorDirectory(zipFile);

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
            }
        }

        /// <summary>
        /// Validates a ZIP file for security and size constraints.
        /// </summary>
        /// <param name="zipFile">The ZIP file to validate.</param>
        /// <returns>True if the ZIP file is valid; otherwise, false.</returns>
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

        /// <summary>
        /// Gets a unique extraction path for a ZIP file.
        /// </summary>
        /// <param name="zipFile">The ZIP file.</param>
        /// <returns>A unique path for extraction.</returns>
        private string GetUniqueExtractionPath(FileInfo zipFile)
        {
            string baseDir = this.directoryManager.GetUnzippedDirectory().FullName;
            string uniqueName = $"{Path.GetFileNameWithoutExtension(zipFile.Name)}_{DateTime.Now:yyyyMMddHHmmss}";
            return Path.Combine(baseDir, uniqueName);
        }

        /// <summary>
        /// Moves a problematic ZIP file to the error directory.
        /// </summary>
        /// <param name="zipFile">The ZIP file to move.</param>
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

                zipFile.MoveTo(destPath);
                Log.Information($"Moved problematic zip file to error directory: {destPath}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to move zip file {zipFile.FullName} to error directory");
            }
        }
    }
}