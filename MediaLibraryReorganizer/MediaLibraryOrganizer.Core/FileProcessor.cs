// <copyright file="FileProcessor.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using MetadataExtractor.Formats.QuickTime;
    using Serilog;
    using MDE = MetadataExtractor;
    using TagFile = TagLib.File;
    using MusicTag = TagLib.Tag;

    /// <summary>
    /// Processes individual media files based on their type and metadata.
    /// </summary>
    public class FileProcessor
    {
        private static HashSet<char>? invalidCharsHash;

        private readonly BackupManager backupManager;
        private readonly DirectoryManager directoryManager;
        private readonly List<Exception> errors;
        private readonly OrganizerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessor"/> class.
        /// </summary>
        /// <param name="backupManager">The backup manager to record processed files.</param>
        /// <param name="directoryManager">The directory manager for file system operations.</param>
        /// <param name="errorList">A list to add errors to.</param>
        /// <param name="options">Tracking and duplicate behavior.</param>
        public FileProcessor(BackupManager backupManager, DirectoryManager directoryManager, List<Exception> errorList, OrganizerOptions options)
        {
            this.backupManager = backupManager;
            this.directoryManager = directoryManager;
            this.errors = errorList;
            this.options = options;
        }

        private static HashSet<char> InvalidCharsHash
        {
            get { return invalidCharsHash ??= new HashSet<char>(Path.GetInvalidFileNameChars()); }
        }

        private static string CleanFilename(string fileName, string newValue = "_")
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            char newChar = newValue[0];
            char[] chars = fileName.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (InvalidCharsHash.Contains(c))
                {
                    chars[i] = newChar;
                }
            }

            return new string(chars);
        }

        /// <summary>
        /// Resolves a single folder artist name from TagLib tags (YouTube Music / Takeout often populate only some fields).
        /// </summary>
        private static string GetPrimaryArtist(MusicTag tag)
        {
            string? fromArray(string[] values)
            {
                if (values == null || values.Length == 0)
                {
                    return null;
                }

                foreach (string s in values)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        return s.Trim();
                    }
                }

                return null;
            }

            string? fromJoined(string? joined)
            {
                if (string.IsNullOrWhiteSpace(joined))
                {
                    return null;
                }

                joined = joined.Trim();
                int semi = joined.IndexOf(';');
                if (semi > 0)
                {
                    return joined.Substring(0, semi).Trim();
                }

                return joined;
            }

            return fromArray(tag.AlbumArtists)
                ?? (string.IsNullOrWhiteSpace(tag.FirstAlbumArtist) ? null : tag.FirstAlbumArtist.Trim())
                ?? fromJoined(tag.JoinedAlbumArtists)
                ?? fromArray(tag.Performers)
                ?? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? null : tag.FirstPerformer.Trim())
                ?? fromJoined(tag.JoinedPerformers)
#pragma warning disable CS0618 // Tag.Artists is obsolete; still populated on some exports
                ?? fromArray(tag.Artists)
#pragma warning restore CS0618
                ?? string.Empty;
        }

        /// <summary>
        /// Processes a single file.
        /// </summary>
        /// <param name="fileInfo">The file to process.</param>
        public void ProcessFile(FileInfo fileInfo)
        {
            StaticLog.Enter($"{nameof(this.ProcessFile)} - {fileInfo.FullName}");
            try
            {
                if (fileInfo.IsZip())
                {
                    return;
                }

                string? md5Hex = null;
                if (this.options.TrackManifest)
                {
                    md5Hex = Statics.GetChecksum(fileInfo);
                    if (this.backupManager.IsProcessed(md5Hex))
                    {
                        this.HandleDuplicateSource(fileInfo, md5Hex);
                        return;
                    }
                }

                FileInfo destinationPath = this.GetDestinationFile(fileInfo);
                destinationPath = this.CreateDestinationFile(fileInfo, destinationPath);

                if (this.options.TrackManifest && md5Hex != null)
                {
                    this.backupManager.AddProcessedFile(md5Hex, destinationPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing file {fileInfo?.FullName}", ex);
                if (fileInfo == null)
                {
                    this.errors.Add(ex);
                    return;
                }

                if (this.options.Interactive && this.options.ErrorHandler != null)
                {
                    this.HandleInteractiveError(fileInfo, ex);
                    return;
                }

                this.errors.Add(ex);
                this.ProcessErrorFile(fileInfo);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        private void HandleInteractiveError(FileInfo fileInfo, Exception ex)
        {
            FileErrorDisposition disposition = this.options.ErrorHandler!.Resolve(fileInfo, ex);
            switch (disposition)
            {
                case FileErrorDisposition.Skip:
                    Log.Information("Skipped file after error: {Path}", fileInfo.FullName);
                    break;
                case FileErrorDisposition.ErrorWorkflow:
                    this.ProcessErrorFile(fileInfo);
                    Log.Information("Sent file to SokkaCorp/Errors/ after user choice: {Path}", fileInfo.FullName);
                    break;
                case FileErrorDisposition.Abort:
                    throw new OrganizerAbortedException($"Aborted while processing {fileInfo.FullName}.");
                default:
                    throw new InvalidOperationException($"Unknown file error disposition: {disposition}");
            }
        }

        private void HandleDuplicateSource(FileInfo file, string md5Hex)
        {
            switch (this.options.OnDuplicate)
            {
                case DuplicateDisposition.Skip:
                    Log.Information("Duplicate MD5 skipped (already in manifest): {Path}", file.FullName);
                    break;
                case DuplicateDisposition.Delete:
                    try
                    {
                        file.Delete();
                        Log.Information("Duplicate source deleted: {Path}", file.FullName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Could not delete duplicate source: {Path}", file.FullName);
                        this.errors.Add(ex);
                    }

                    break;
                case DuplicateDisposition.Quarantine:
                    try
                    {
                        DirectoryInfo dupDir = this.directoryManager.GetDuplicatesDirectory();
                        string safeName = $"{md5Hex}_{file.Name}";
                        string dest = Path.Combine(dupDir.FullName, safeName);
                        int n = 0;
                        while (File.Exists(dest))
                        {
                            n++;
                            dest = Path.Combine(dupDir.FullName, $"{md5Hex}_{n}_{file.Name}");
                        }

                        bool dupCopy = this.options.CopyOnly || (file.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                        if (dupCopy)
                        {
                            file.CopyTo(dest);
                        }
                        else
                        {
                            file.MoveTo(dest);
                        }

                        Log.Information("Duplicate quarantined: {Src} -> {Dest}", file.FullName, dest);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Could not quarantine duplicate: {Path}", file.FullName);
                        this.errors.Add(ex);
                    }

                    break;
            }
        }

        private void ProcessErrorFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessErrorFile));
            try
            {
                DirectoryInfo errorDir = this.directoryManager.GetErrorMiscDirectory();
                if (file.IsPhoto())
                {
                    errorDir = this.directoryManager.GetErrorPhotoDirectory();
                }
                else if (file.IsVideo())
                {
                    errorDir = this.directoryManager.GetErrorVideoDirectory();
                }
                else if (file.IsMusic())
                {
                    errorDir = this.directoryManager.GetErrorMusicDirectory();
                }

                string errorFileName = Path.Combine(errorDir.FullName, file.Name);

                if (File.Exists(errorFileName))
                {
                    string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                    string fileExtension = file.Extension;

                    errorFileName = Path.Combine(
                        errorDir.FullName,
                        fileNameWithoutExtension ?? "unknown_file",
                        DateTime.Now.Ticks.ToString(),
                        fileExtension);

                    string? timestampDirectory = Path.GetDirectoryName(errorFileName);
                    if (!string.IsNullOrEmpty(timestampDirectory))
                    {
                        Directory.CreateDirectory(timestampDirectory);
                    }
                }

                file.CopyTo(errorFileName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Trying to copy an error file {Path} to the error directory", file?.FullName);
                this.errors.Add(ex);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessErrorFile));
            }
        }

        private FileInfo GetDestinationFile(FileInfo fileInfo)
        {
            if (fileInfo.IsPhoto())
            {
                return this.ProcessPhoto(fileInfo);
            }

            if (fileInfo.IsVideo())
            {
                return this.ProcessVideo(fileInfo);
            }

            if (fileInfo.IsMusic())
            {
                return this.ProcessMusic(fileInfo);
            }

            return this.ProcessMiscFile(fileInfo);
        }

        private FileInfo ProcessPhoto(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessPhoto));
            try
            {
                DateTime photoDt = PhotoTakenDate.GetBestDate(file);
                return new FileInfo(Path.Combine(this.directoryManager.GetPhotoDirectoryFromDateTime(photoDt).FullName, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessPhoto));
            }
        }

        private FileInfo ProcessVideo(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessVideo));
            try
            {
                TagLib.File? tagfile = null;
                DateTime? videoDt = null;

                try
                {
                    tagfile = TagLib.File.Create(file.FullName);
                }
                catch
                {
                    // TagLib fails on some MOV files; MetadataExtractor used below.
                }

                if (tagfile?.Tag?.DateTagged.HasValue == true)
                {
                    videoDt = tagfile.Tag.DateTagged.Value;
                }
                else if (file.FullName.EndsWith(Constants.FileExtensions.Video.MOV, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        IReadOnlyList<MDE.Directory> tags = MDE.ImageMetadataReader.ReadMetadata(file.FullName);
                        foreach (var tag in tags)
                        {
                            if (tag is QuickTimeMovieHeaderDirectory movie)
                            {
                                if (movie.GetObject(QuickTimeMovieHeaderDirectory.TagCreated) is DateTime dt)
                                {
                                    videoDt = dt;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex) when (ex is MDE.ImageProcessingException or IOException)
                    {
                        Log.Debug(ex, "QuickTime metadata not read for {Path}; using file timestamp.", file.FullName);
                    }
                }

                DateTime finalVideoDt = videoDt ?? file.LastWriteTime;
                string videoDirectory = this.directoryManager.GetVideoDirectoryFromDateTime(finalVideoDt).FullName;
                return new FileInfo(Path.Combine(videoDirectory, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessVideo));
            }
        }

        private FileInfo ProcessMusic(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessMusic));
            try
            {
                using var songDetails = TagFile.Create(file.FullName);
                string artist = GetPrimaryArtist(songDetails.Tag);

                if (string.IsNullOrEmpty(artist))
                {
                    artist = Constants.Music.UnknownArtist;
                    Log.Information($"Artist not found; using \"{Constants.Music.UnknownArtist}\"");
                }

                artist = CleanFilename(artist);

                string? rawAlbum = songDetails.Tag.Album;
                string albumTitle;
                if (string.IsNullOrWhiteSpace(rawAlbum))
                {
                    albumTitle = Constants.Music.UnknownAlbum;
                    Log.Information($"Album not found; using \"{Constants.Music.UnknownAlbum}\"");
                }
                else
                {
                    albumTitle = rawAlbum;
                }

                albumTitle = CleanFilename(albumTitle);

                string? fileNameOnly = Path.GetFileName(file.FullName);
                string cleanedFileName = CleanFilename(fileNameOnly ?? file.Name);

                string fileName = Path.Combine(
                    this.directoryManager.GetProcessedMusicDirectory().FullName,
                    artist,
                    albumTitle,
                    cleanedFileName);

                string? destinationDirectoryName = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(destinationDirectoryName))
                {
                    Directory.CreateDirectory(destinationDirectoryName);
                }

                return new FileInfo(fileName);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessMusic));
            }
        }

        private FileInfo ProcessMiscFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessMiscFile));
            try
            {
                var destinationDirectory = this.directoryManager.GetMiscDirectoryFromDateTime(file.CreationTime);
                string destFileName = Path.Combine(destinationDirectory.FullName, file.Name);
                return new FileInfo(destFileName);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessMiscFile));
            }
        }

        private FileInfo CreateDestinationFile(FileInfo fileInfo, FileInfo destinationPath)
        {
            string uniquePath = UniquifyDestinationPath(destinationPath.FullName);
            destinationPath = new FileInfo(uniquePath);

            Log.Information("Importing {Source} => {Dest}", fileInfo.FullName, destinationPath.FullName);

            string? destDir = Path.GetDirectoryName(destinationPath.FullName);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            bool readOnlySource = (fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            bool useCopy = this.options.CopyOnly || readOnlySource;
            if (useCopy)
            {
                fileInfo.CopyTo(destinationPath.FullName);
            }
            else
            {
                fileInfo.MoveTo(destinationPath.FullName);
            }

            return destinationPath;
        }

        /// <summary>
        /// If <paramref name="destPath"/> already exists, append _1, _2, … before the extension so imports are not skipped.
        /// </summary>
        private static string UniquifyDestinationPath(string destPath)
        {
            if (!File.Exists(destPath))
            {
                return destPath;
            }

            string? dir = Path.GetDirectoryName(destPath);
            string name = Path.GetFileNameWithoutExtension(destPath);
            string ext = Path.GetExtension(destPath);
            int i = 1;
            string candidate;
            do
            {
                candidate = Path.Combine(dir ?? ".", $"{name}_{i}{ext}");
                i++;
            }
            while (File.Exists(candidate));

            return candidate;
        }
    }
}
