// <copyright file="DirectoryManager.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Serilog; // Needed for logging

    /// <summary>
    /// Manages directory and file system operations for media organization.
    /// </summary>
    public class DirectoryManager
    {
        // Constants for subdirectory names
        private const string UnzippedSubdirectoryName = "Unzipped";
        private const string ErrorsSubdirectoryName = "Errors";
        private const string MiscErrorsSubdirectoryName = "Misc";

        private readonly DirectoryInfo originalSourceDirectory;
        private readonly DirectoryInfo outputDirectory; // Added dependency for clarity
        private readonly List<Exception> errors; // Added error list dependency
        private DirectoryInfo workingSourceDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryManager"/> class.
        /// </summary>
        /// <param name="sourceDirectory">The original source directory.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="errors">The list to collect errors.</param>
        public DirectoryManager(DirectoryInfo sourceDirectory, DirectoryInfo outputDirectory, List<Exception> errors)
        {
            this.originalSourceDirectory = sourceDirectory;
            this.workingSourceDirectory = sourceDirectory; // Initially the same
            this.outputDirectory = outputDirectory; // Store output directory
            this.errors = errors; // Assign the error list
        }

        /// <summary>
        /// Gets the original source directory.
        /// </summary>
        public DirectoryInfo OriginalSourceDirectory => this.originalSourceDirectory;

        /// <summary>
        /// Gets the output directory.
        /// </summary>
        public DirectoryInfo OutputDirectory => this.outputDirectory;

        /// <summary>
        /// Gets the current working source directory.
        /// </summary>
        public DirectoryInfo WorkingSourceDirectory => this.workingSourceDirectory;

        /// <summary>
        /// Gets the directory for unzipped files within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the unzipped files directory.</returns>
        public DirectoryInfo GetUnzippedDirectory()
        {
            string unzippedPath = Path.Combine(this.outputDirectory.FullName, UnzippedSubdirectoryName);
            return Directory.CreateDirectory(unzippedPath);
        }

        /// <summary>
        /// Gets the miscellaneous error directory within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the miscellaneous error directory.</returns>
        public DirectoryInfo GetErrorMiscDirectory()
        {
            string errorPath = Path.Combine(this.outputDirectory.FullName, ErrorsSubdirectoryName, MiscErrorsSubdirectoryName);
            return Directory.CreateDirectory(errorPath);
        }

        /// <summary>
        /// Gets the error photos directory within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the error photos directory.</returns>
        public DirectoryInfo GetErrorPhotoDirectory()
        {
            string errorPath = Path.Combine(this.outputDirectory.FullName, ErrorsSubdirectoryName, Constants.FolderCategories.PhotoFolder);
            return Directory.CreateDirectory(errorPath);
        }

        /// <summary>
        /// Gets the error videos directory within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the error videos directory.</returns>
        public DirectoryInfo GetErrorVideoDirectory()
        {
            string errorPath = Path.Combine(this.outputDirectory.FullName, ErrorsSubdirectoryName, Constants.FolderCategories.VideoFolder);
            return Directory.CreateDirectory(errorPath);
        }

        /// <summary>
        /// Gets the error music directory within the output directory.
        /// </summary>
        /// <returns>The DirectoryInfo for the error music directory.</returns>
        public DirectoryInfo GetErrorMusicDirectory()
        {
            string errorPath = Path.Combine(this.outputDirectory.FullName, ErrorsSubdirectoryName, Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(errorPath);
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
        /// Moves the source directory to the output area for processing if it's read-only.
        /// Creates a working copy.
        /// </summary>
        public void CreateWorkingCopyIfReadOnly()
        {
            // Check if the original source directory is read-only
            if (this.originalSourceDirectory.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                Log.Information("Source directory is read-only, creating a working copy.");

                // it's read-only, let's make a copy of the whole thing so we can manipulate it at will
                // We need a base path for temporary working directories. Let's use a dedicated temp directory.
                // Consider injecting a temp path provider or using Environment.GetTempPath() combined with a unique app folder
                // For now, let's create a temp folder within the system's temp path
                string tempBasePath = Path.Combine(Path.GetTempPath(), "MediaLibraryOrganizerWorking");
                string newSourcePath = Path.Combine(tempBasePath, this.originalSourceDirectory.Name);

                DirectoryInfo newSourceDir = Directory.CreateDirectory(newSourcePath);
                FileInfo[] allFiles = this.originalSourceDirectory.GetFiles("*.*", SearchOption.AllDirectories);

                for (int i = 0; i < allFiles.Length; i++)
                {
                    FileInfo file = allFiles[i];
                    Console.WriteLine("{1} - {0}", file.Name, i);

                    string parentDirectoryFullName = file.Directory?.FullName ?? this.originalSourceDirectory.FullName;
                    string relativePath = Path.GetRelativePath(this.originalSourceDirectory.FullName, parentDirectoryFullName);
                    string newfilePath = Path.Combine(newSourceDir.FullName, relativePath, file.Name);
                    string? newFileDirectoryName = Path.GetDirectoryName(newfilePath);

                    if (!string.IsNullOrEmpty(newFileDirectoryName))
                    {
                        Directory.CreateDirectory(newFileDirectoryName);
                    }

                    try
                    {
                        FileInfo newFileInfo = new FileInfo(newfilePath);
                        if (!newFileInfo.Exists)
                        {
                            file.CopyTo(newfilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error copying file {file.FullName} to {newfilePath}");
                        this.errors.Add(ex); // Report error
                    }
                }

                this.workingSourceDirectory = newSourceDir; // Update working directory to the copy
            }
            else
            {
                Log.Information("Source directory is writable, working directly on the original.");
                this.workingSourceDirectory = this.originalSourceDirectory; // Work directly on the original
            }
        }

        /// <summary>
        /// Removes temporary working directories created during the process,
        /// specifically the working copy (if different from the original source)
        /// and the directory used for unzipped files.
        /// </summary>
        public void CleanupWorkingDirectories()
        {
            Log.Information("Cleaning up temporary working directories.");

            // Cleanup the created working copy if it was necessary (i.e., different from the original source)
            if (this.workingSourceDirectory.FullName != this.originalSourceDirectory.FullName)
            {
                if (this.workingSourceDirectory.Exists)
                {
                    try
                    {
                        Log.Information($"Attempting to delete working directory: {this.workingSourceDirectory.FullName}");
                        Directory.Delete(this.workingSourceDirectory.FullName, true);
                        Log.Information($"Successfully deleted working directory: {this.workingSourceDirectory.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error deleting working directory: {this.workingSourceDirectory.FullName}");
                        this.errors.Add(ex); // Report error
                    }
                }
                else
                {
                    Log.Information($"Working directory {this.workingSourceDirectory.FullName} does not exist, no cleanup needed.");
                }
            }
            else
            {
                Log.Information("Working directory is the same as the original source, no working copy to delete.");
            }

            // Also cleanup the unzipped directory, which is typically within the working source directory
            // Now get the unzipped directory path using the DirectoryManager method
            DirectoryInfo unzippedDir = this.GetUnzippedDirectory();
            if (unzippedDir.Exists)
            {
                try
                {
                    Log.Information($"Attempting to delete unzipped directory: {unzippedDir.FullName}");
                    Directory.Delete(unzippedDir.FullName, true);
                    Log.Information($"Successfully deleted unzipped directory: {unzippedDir.FullName}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error deleting unzipped directory: {unzippedDir.FullName}");
                    this.errors.Add(ex); // Report error
                }
            }
            else
            {
                Log.Information($"Unzipped directory {unzippedDir.FullName} does not exist, no cleanup needed.");
            }
        }
    }
}