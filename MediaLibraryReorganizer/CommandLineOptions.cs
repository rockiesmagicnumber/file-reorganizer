// <copyright file="CommandLineOptions.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CommandLine;
    using Serilog;
    using SokkaCorp.MediaLibraryOrganizer.Lib;

    /// <summary>
    /// Handles command line options for the media library organizer.
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Gets or sets the source directory path.
        /// </summary>
        [Option('s', "source", Required = true, HelpText = "Source directory containing media files to organize.")]
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output directory path.
        /// </summary>
        [Option('o', "output", Required = false, HelpText = "Output directory where organized files will be stored.")]
        public string OutputPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Gets or sets a value indicating whether to refresh the JSON backup.
        /// </summary>
        [Option("refresh-json", Required = false, HelpText = "Refresh the JSON backup by removing entries for files that no longer exist.")]
        public bool RefreshJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to repopulate the JSON backup.
        /// </summary>
        [Option("repopulate-json", Required = false, HelpText = "Repopulate the JSON backup by scanning all files in the processed directory.")]
        public bool RepopulateJson { get; set; }

        /// <summary>
        /// Executes the media library organization based on the provided options.
        /// </summary>
        public void Execute()
        {
            if (string.IsNullOrEmpty(this.SourcePath))
            {
                throw new InvalidOperationException("Source path is required.");
            }

            DirectoryInfo sourceDir = Directory.CreateDirectory(this.SourcePath);
            DirectoryInfo outputDir = Directory.CreateDirectory(this.OutputPath);

            Log.Debug("Source Directory: {executionDirectory}", sourceDir.FullName);
            Log.Information($"Output Directory: {outputDir.FullName}");

            // Create the dependencies
            List<Exception> errors = new List<Exception>(); // Centralized error list
            BackupManager backupManager = new BackupManager(errors);
            DirectoryManager directoryManager = new DirectoryManager(sourceDir, outputDir, errors);

            // FileProcessor needs BackupManager, DirectoryManager and the error list
            FileProcessor fileProcessor = new FileProcessor(backupManager, directoryManager, errors);

            // Create the ZipProcessor dependency, now injecting the DirectoryManager
            ZipProcessor zipProcessor = new ZipProcessor(directoryManager);

            // Instantiate the orchestrator with its dependencies
            MediaLibraryOrganizer organizer = new MediaLibraryOrganizer(backupManager, directoryManager, fileProcessor, zipProcessor, errors);

            if (this.RefreshJson)
            {
                // Call directly on the backupManager instance
                backupManager.PruneJsonBackup();
            }
            else if (this.RepopulateJson)
            {
                // Call directly on the backupManager instance
                backupManager.RepopulateJsonBackup();
            }
            else
            {
                organizer.OrganizeFiles();
            }

            // Optionally log/report errors collected during the process
            if (errors.Any())
            {
                Log.Warning($"Organization finished with {errors.Count} errors.");
                foreach (Exception error in errors)
                {
                    Log.Error(error, "Collected Error");
                }

                if (errors.Any())
                {
                    // Throw a final exception indicating that errors occurred
                    throw new AggregateException($"Organization finished with {errors.Count} errors. See logs for details.", errors);
                }
            }
        }
    }
}