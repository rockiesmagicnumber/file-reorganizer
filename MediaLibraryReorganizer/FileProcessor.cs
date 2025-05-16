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
    using static Statics; // Still needed for GetChecksum and Is* extension methods
    using MDE = MetadataExtractor;
    using TagFile = TagLib.File;

    /// <summary>
    /// Processes individual media files based on their type and metadata.
    /// </summary>
    public class FileProcessor
    {
        private static HashSet<char>? invalidCharsHash;

        // Instance members
        private readonly BackupManager backupManager;
        private readonly DirectoryManager directoryManager;
        private readonly List<Exception> errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessor"/> class.
        /// </summary>
        /// <param name="backupManager">The backup manager to record processed files.</param>
        /// <param name="directoryManager">The directory manager for file system operations.</param>
        /// <param name="errorList">A list to add errors to.</param>
        public FileProcessor(BackupManager backupManager, DirectoryManager directoryManager, List<Exception> errorList)
        {
            this.backupManager = backupManager;
            this.directoryManager = directoryManager;
            this.errors = errorList;
        }

        /// <summary>
        /// Gets the set of invalid filename characters.
        /// </summary>
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
        /// Processes a single file.
        /// </summary>
        /// <param name="fileInfo">The file to process.</param>
        public void ProcessFile(FileInfo fileInfo)
        {
            StaticLog.Enter($"{nameof(this.ProcessFile)} - {fileInfo.FullName}");
            try
            {
                string fileSHAchksum = GetChecksum(fileInfo); // Uses Statics

                // Check if file is already processed using BackupManager
                // Need a method in BackupManager to check existence by checksum
                // if (this.backupManager.IsProcessed(fileSHAchksum)) // Assuming IsProcessed method exists
                // {
                //     Log.Information($"File already processed, skipping | {fileInfo.FullName}");
                //     return;
                // }

                // Determine destination path based on file type and metadata
                FileInfo destinationPath = this.GetDestinationFile(fileInfo);

                // Perform file system operation (create directory and move/copy)
                // This logic should be abstracted or handled by DirectoryManager
                destinationPath = this.CreateDestinationFile(fileInfo, destinationPath); // This method needs access to file operations

                // Add to backup AFTER successful processing and moving
                this.backupManager.AddProcessedFile(fileSHAchksum, destinationPath);
            }
            catch (Exception ex)
            {
                Log.Error($"Error processing file {fileInfo?.FullName}", ex);

                // Add error to the list passed in the constructor
                this.errors.Add(ex);

                // Process error file if it exists
                if (fileInfo != null)
                {
                    this.ProcessErrorFile(fileInfo);
                }
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        /// <summary>
        /// Processes an error file by moving it to the appropriate error directory.
        /// </summary>
        /// <param name="file">The file that encountered an error.</param>
        private void ProcessErrorFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessErrorFile));
            try
            {
                DirectoryInfo errorDir = GetErrorMiscDirectory(); // Uses Statics
                if (file.IsPhoto()) // Uses Statics extension method
                {
                    errorDir = GetErrorPhotoDirectory(); // Uses Statics
                }
                else if (file.IsVideo()) // Uses Statics extension method
                {
                    errorDir = GetErrorVideoDirectory(); // Uses Statics
                }
                else if (file.IsMusic()) // Uses Statics extension method
                {
                    errorDir = GetErrorMusicDirectory(); // Uses Statics
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

                file.CopyTo(errorFileName); // File system operation
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Trying to move an error file {file?.FullName} to the error directory");

                // Add error to the list passed in the constructor
                this.errors.Add(ex);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessErrorFile));
            }
        }

        /// <summary>
        /// Determines the destination file path for a given file.
        /// </summary>
        /// <param name="fileInfo">The file to process.</param>
        /// <returns>The destination file information.</returns>
        private FileInfo GetDestinationFile(FileInfo fileInfo)
        {
            FileInfo destinationPath;
            if (fileInfo.IsPhoto()) // Uses Statics extension method
            {
                destinationPath = this.ProcessPhoto(fileInfo);
            }
            else if (fileInfo.IsVideo()) // Uses Statics extension method
            {
                destinationPath = this.ProcessVideo(fileInfo);
            }
            else if (fileInfo.IsMusic()) // Uses Statics extension method
            {
                destinationPath = this.ProcessMusic(fileInfo);
            }
            else
            {
                destinationPath = this.ProcessMiscFile(fileInfo);
            }

            return destinationPath;
        }

        /// <summary>
        /// Processes a photo file.
        /// </summary>
        /// <param name="file">The photo file to process.</param>
        /// <returns>The destination file information.</returns>
        private FileInfo ProcessPhoto(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessPhoto));
            try
            {
                TagLib.File tagfile = TagLib.File.Create(file.FullName);
                DateTime photoDt;

                photoDt = (tagfile is TagLib.Image.File image && image.ImageTag?.DateTime.HasValue == true)
                    ? image.ImageTag.DateTime.Value
                    : (tagfile.Tag.DateTagged.HasValue
                        ? tagfile.Tag.DateTagged.Value
                        : file.CreationTime);

                // Uses Statics
                return new FileInfo(Path.Combine(GetPhotoDirectoryFromDateTime(photoDt).FullName, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessPhoto));
            }
        }

        /// <summary>
        /// Processes a video file.
        /// </summary>
        /// <param name="file">The video file to process.</param>
        /// <returns>The destination file information.</returns>
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
                    // TagLib fails on .MOV files, we'll handle those separately
                }

                if (tagfile?.Tag?.DateTagged.HasValue == true)
                {
                    videoDt = tagfile.Tag.DateTagged.Value;
                }
                else if (file.FullName.EndsWith(Constants.FileExtensions.Video.MOV, StringComparison.InvariantCultureIgnoreCase))
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

                DateTime finalVideoDt = videoDt ?? file.CreationTime;

                // Uses Statics
                string videoDirectory = GetVideoDirectoryFromDateTime(finalVideoDt).FullName;
                return new FileInfo(Path.Combine(videoDirectory, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessVideo));
            }
        }

        /// <summary>
        /// Processes a music file.
        /// </summary>
        /// <param name="file">The music file to process.</param>
        /// <returns>The destination file information.</returns>
        private FileInfo ProcessMusic(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessMusic));
            try
            {
                var songDetails = TagFile.Create(file.FullName);
                string artist = songDetails.Tag.AlbumArtists.Length > 0
                    ? songDetails.Tag.AlbumArtists[0]
                    : songDetails.Tag.Performers.Length > 0
                        ? songDetails.Tag.Performers[0]
                        : string.Empty;

                if (string.IsNullOrEmpty(artist))
                {
                    artist = Constants.Music.UnknownArtist;
                    Log.Information($"Artist not found; using \"{Constants.Music.UnknownArtist}\"");
                }

                artist = CleanFilename(artist);

                string albumTitle = songDetails.Tag.Album ?? Constants.Music.UnknownAlbum;
                albumTitle = CleanFilename(albumTitle);

                string? fileNameOnly = Path.GetFileName(file.FullName);
                string cleanedFileName = CleanFilename(fileNameOnly ?? file.Name);

                // Uses Statics
                string fileName = Path.Combine(
                    GetProcessedMusicDirectory().FullName,
                    artist,
                    albumTitle,
                    cleanedFileName);

                // File system operation - creating directory
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

        /// <summary>
        /// Processes a miscellaneous file.
        /// </summary>
        /// <param name="file">The file to process.</param>
        /// <returns>The destination file information.</returns>
        private FileInfo ProcessMiscFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessMiscFile));
            try
            {
                // Uses Statics
                var destinationDirectory = GetMiscDirectoryFromDateTime(file.CreationTime);
                string destFileName = Path.Combine(destinationDirectory.FullName, file.Name);
                return new FileInfo(destFileName);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessMiscFile));
            }
        }

        /// <summary>
        /// Creates a destination file from the source file.
        /// Note: This method performs file system operations (move).
        /// Consider moving this logic to a dedicated file system helper or DirectoryManager.
        /// </summary>
        /// <param name="fileInfo">The source file information.</param>
        /// <param name="destinationPath">The destination file information.</param>
        /// <returns>The created destination file information.</returns>
        private FileInfo CreateDestinationFile(FileInfo fileInfo, FileInfo destinationPath)
        {
            Log.Information($"Copying {fileInfo.FullName} => {destinationPath.FullName}");

            if (!destinationPath.Exists)
            {
                // File system operation - moving file
                fileInfo.MoveTo(destinationPath.FullName);
            }

            return destinationPath;
        }
    }
}