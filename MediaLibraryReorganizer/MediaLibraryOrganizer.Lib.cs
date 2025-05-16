// <copyright file="MediaLibraryOrganizer.Lib.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Serilog;

    /// <summary>
    /// Handles the orchestration of media library organization.
    /// </summary>
    public class MediaLibraryOrganizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibraryOrganizer"/> class.
        /// </summary>
        /// <param name="backupManager">The backup manager dependency.</param>
        /// <param name="directoryManager">The directory manager dependency.</param>
        /// <param name="fileProcessor">The file processor dependency.</param>
        /// <param name="zipProcessor">The zip processor dependency.</param>
        /// <param name="errors">The list to collect errors.</param>
        public MediaLibraryOrganizer(BackupManager backupManager, DirectoryManager directoryManager, FileProcessor fileProcessor, ZipProcessor zipProcessor, List<Exception> errors)
        {
            this.BackupManager = backupManager;
            this.DirectoryManager = directoryManager;
            this.FileProcessor = fileProcessor;
            this.ZipProcessor = zipProcessor;
            this.Errors = errors;
        }

        /// <summary>
        /// Gets the backup manager instance.
        /// </summary>
        public BackupManager BackupManager { get; }

        /// <summary>
        /// Gets the directory manager instance.
        /// </summary>
        public DirectoryManager DirectoryManager { get; }

        /// <summary>
        /// Gets the file processor instance.
        /// </summary>
        public FileProcessor FileProcessor { get; }

        /// <summary>
        /// Gets the zip processor instance.
        /// </summary>
        public ZipProcessor ZipProcessor { get; }

        /// <summary>
        /// Gets or sets the list of errors encountered during processing.
        /// </summary>
        private List<Exception> Errors { get; set; }

        /// <summary>
        /// Organizes files from the source directory into the output directory.
        /// </summary>
        public void OrganizeFiles()
        {
            StaticLog.Enter(nameof(this.OrganizeFiles));
            try
            {
                this.DirectoryManager.CreateWorkingCopyIfReadOnly();
                this.UnzipAllRecursively(this.DirectoryManager.WorkingSourceDirectory);
                this.ProcessDirectory(this.DirectoryManager.WorkingSourceDirectory);
                this.DirectoryManager.CleanupWorkingDirectories();
            }
            catch (Exception ex)
            {
                Log.Error("Error organizing files", ex);
                this.Errors.Add(ex);
                throw;
            }
            finally
            {
                this.BackupManager.WriteJsonBackup();
                StaticLog.Exit(nameof(this.OrganizeFiles));
            }
        }

        /// <summary>
        /// Processes a directory by iterating through files and using the FileProcessor.
        /// </summary>
        /// <param name="directory">The directory to process.</param>
        private void ProcessDirectory(DirectoryInfo directory)
        {
            StaticLog.Enter($"{nameof(this.ProcessDirectory)} - {directory.FullName}");
            try
            {
                FileInfo[] childFiles = directory.GetFiles("*.*", SearchOption.AllDirectories);
                int childFileCount = childFiles.Length;

                int cnt = 0;
                foreach (FileInfo cf in childFiles)
                {
                    cnt++;
                    Log.Information($"Processing {cnt}/{childFileCount}");
                    this.FileProcessor.ProcessFile(cf);
                }
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessDirectory));
            }
        }

        /// <summary>
        /// Unzips all ZIP files in the directory recursively.
        /// </summary>
        /// <param name="directory">The directory containing ZIP files.</param>
        private void UnzipAllRecursively(DirectoryInfo directory)
        {
            StaticLog.Enter(nameof(this.UnzipAllRecursively));
            try
            {
                this.ZipProcessor.ProcessZipsRecursively(directory);
            }
            finally
            {
                StaticLog.Exit(nameof(this.UnzipAllRecursively));
            }
        }
    }
}