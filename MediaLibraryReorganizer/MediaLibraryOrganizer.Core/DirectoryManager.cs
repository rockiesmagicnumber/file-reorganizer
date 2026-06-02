// <copyright file="DirectoryManager.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Serilog;

    /// <summary>
    /// Manages directory and file system operations for media organization.
    /// </summary>
    public class DirectoryManager
    {
        private readonly DirectoryInfo originalSourceDirectory;
        private readonly DirectoryInfo outputDirectory;
        private readonly List<Exception> errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryManager"/> class.
        /// </summary>
        /// <param name="sourceDirectory">The original source directory.</param>
        /// <param name="outputDirectory">The <c>MediaLibrary</c> root (<c>{output parent}/MediaLibrary</c>): <c>Processed/</c> plus tool artifacts under <c>SokkaCorp/</c>.</param>
        /// <param name="errors">The list to collect errors.</param>
        public DirectoryManager(DirectoryInfo sourceDirectory, DirectoryInfo outputDirectory, List<Exception> errors)
        {
            this.originalSourceDirectory = sourceDirectory;
            this.outputDirectory = outputDirectory;
            this.errors = errors;
        }

        /// <summary>
        /// Gets the original source directory.
        /// </summary>
        public DirectoryInfo OriginalSourceDirectory => this.originalSourceDirectory;

        /// <summary>
        /// Gets the media library root directory (<c>MediaLibrary</c> folder).
        /// </summary>
        public DirectoryInfo OutputDirectory => this.outputDirectory;

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp</c> (logs, manifest, errors, duplicates, unzip scratch).
        /// </summary>
        /// <returns>SokkaCorp workspace directory.</returns>
        public DirectoryInfo GetSokkaCorpDirectory()
        {
            string ret = Path.Combine(this.outputDirectory.FullName, Constants.RuntimeDirectories.SokkaCorpDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/Processed</c>.
        /// </summary>
        /// <returns>Processed root.</returns>
        public DirectoryInfo GetProcessedRootDirectory()
        {
            string ret = Path.Combine(this.outputDirectory.FullName, Constants.RuntimeDirectories.ProcessedDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Duplicates</c> for quarantined duplicate sources.
        /// </summary>
        /// <returns>Duplicates directory.</returns>
        public DirectoryInfo GetDuplicatesDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.DuplicatesDirectoryName);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Unzipped</c> (short-lived ZIP extraction scratch).
        /// </summary>
        /// <returns>The DirectoryInfo for the unzipped files directory.</returns>
        public DirectoryInfo GetUnzippedDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.UnzippedDirectoryName);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Errors/Misc</c>.
        /// </summary>
        /// <returns>The DirectoryInfo for the miscellaneous error directory.</returns>
        public DirectoryInfo GetErrorMiscDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.ErrorDirectoryName, Constants.FolderCategories.MiscFolder);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Errors/Photos</c>.
        /// </summary>
        /// <returns>The DirectoryInfo for the error photos directory.</returns>
        public DirectoryInfo GetErrorPhotoDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.ErrorDirectoryName, Constants.FolderCategories.PhotoFolder);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Errors/Videos</c>.
        /// </summary>
        /// <returns>The DirectoryInfo for the error videos directory.</returns>
        public DirectoryInfo GetErrorVideoDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.ErrorDirectoryName, Constants.FolderCategories.VideoFolder);
        }

        /// <summary>
        /// Gets <c>{MediaLibrary}/SokkaCorp/Errors/Music</c>.
        /// </summary>
        /// <returns>The DirectoryInfo for the error music directory.</returns>
        public DirectoryInfo GetErrorMusicDirectory()
        {
            return this.GetUnderSokkaCorp(Constants.RuntimeDirectories.ErrorDirectoryName, Constants.FolderCategories.MusicFolder);
        }

        /// <summary>
        /// Gets the processed music directory within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the processed music directory.</returns>
        public DirectoryInfo GetProcessedMusicDirectory()
        {
            string musicPath = Path.Combine(this.outputDirectory.FullName, Constants.RuntimeDirectories.ProcessedDirectoryName, Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(musicPath);
        }

        /// <summary>
        /// Gets a photo directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure.</param>
        /// <returns>The directory info for the photo files location.</returns>
        public DirectoryInfo GetPhotoDirectoryFromDateTime(DateTime dt)
        {
            string destDir = Path.Combine(
                this.outputDirectory.FullName,
                Constants.RuntimeDirectories.ProcessedDirectoryName,
                Constants.FolderCategories.PhotoFolder,
                dt.Year.ToString("0000"),
                dt.Month.ToString("00"),
                dt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets a video directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure.</param>
        /// <returns>The directory info for the video files location.</returns>
        public DirectoryInfo GetVideoDirectoryFromDateTime(DateTime dt)
        {
            string destDir = Path.Combine(
                this.outputDirectory.FullName,
                Constants.RuntimeDirectories.ProcessedDirectoryName,
                Constants.FolderCategories.VideoFolder,
                dt.Year.ToString("0000"),
                dt.Month.ToString("00"),
                dt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets a misc directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure.</param>
        /// <returns>The directory info for the misc files location.</returns>
        public DirectoryInfo GetMiscDirectoryFromDateTime(DateTime dt)
        {
            string destDir = Path.Combine(
                this.outputDirectory.FullName,
                Constants.RuntimeDirectories.ProcessedDirectoryName,
                Constants.FolderCategories.MiscFolder,
                dt.Year.ToString("0000"),
                dt.Month.ToString("00"),
                dt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Deletes the unzip scratch tree (<c>{MediaLibrary}/SokkaCorp/Unzipped</c>) after files have been organized.
        /// </summary>
        public void CleanupUnzippedScratch()
        {
            Log.Information("Cleaning up unzip scratch directory.");
            DirectoryInfo unzippedDir = this.GetUnzippedDirectory();
            if (!unzippedDir.Exists)
            {
                return;
            }

            try
            {
                Log.Information("Deleting unzipped directory: {Path}", unzippedDir.FullName);
                Directory.Delete(unzippedDir.FullName, true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting unzipped directory {Path}", unzippedDir.FullName);
                this.errors.Add(ex);
            }
        }

        /// <summary>
        /// Resolves a directory under <c>{MediaLibrary}/SokkaCorp</c>.
        /// </summary>
        /// <param name="segments">Path segments after SokkaCorp.</param>
        /// <returns>Created directory.</returns>
        private DirectoryInfo GetUnderSokkaCorp(params string[] segments)
        {
            string path = this.GetSokkaCorpDirectory().FullName;
            foreach (string segment in segments)
            {
                path = Path.Combine(path, segment);
            }

            return Directory.CreateDirectory(path);
        }
    }
}
