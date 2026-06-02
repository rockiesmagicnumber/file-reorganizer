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
        [Option('o', "output", Required = false, HelpText = "Output parent: creates MediaLibrary/ (imports, logs, manifest) and Original/ (empty; optional archive).")]
        public string OutputPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Gets or sets a value indicating whether MD5 manifest, duplicate detection, and zip content fingerprinting are enabled.
        /// </summary>
        [Option("track", Required = false, HelpText = "Enable jsonBackup.json manifest and duplicate handling (MD5).")]
        public bool Track { get; set; }

        /// <summary>
        /// Gets or sets duplicate disposition when --track is set: skip | delete | quarantine.
        /// </summary>
        [Option("on-duplicate", Required = false, Default = "skip", HelpText = "With --track: skip | delete | quarantine duplicate sources.")]
        public string OnDuplicate { get; set; } = "skip";

        /// <summary>
        /// Gets or sets a value indicating whether to refresh the JSON backup.
        /// </summary>
        [Option("refresh-json", Required = false, HelpText = "With --track: prune manifest entries for missing files.")]
        public bool RefreshJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to repopulate the JSON backup.
        /// </summary>
        [Option("repopulate-json", Required = false, HelpText = "With --track: rebuild manifest from Processed/ tree.")]
        public bool RepopulateJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to copy files instead of moving them (leave sources in place).
        /// </summary>
        [Option("copy", Required = false, HelpText = "Copy files into the library instead of moving them.")]
        public bool Copy { get; set; }

        /// <summary>
        /// Gets or sets the maximum zip-in-zip nesting depth (outermost archives start at 0). Use 0 for no limit.
        /// </summary>
        [Option("max-zip-depth", Required = false, Default = 5, HelpText = "Stop expanding nested .zip when nesting reaches this index (default 5). Use 0 for no limit.")]
        public int MaxZipDepth { get; set; } = 5;

        /// <summary>
        /// Gets or sets a value indicating whether to prompt on file errors and offer source wipe after flash merges.
        /// </summary>
        [Option("interactive", Required = false, HelpText = "Prompt on file errors (skip | error workflow | abort) and offer to wipe source after successful flash merge.")]
        public bool Interactive { get; set; }

        /// <summary>
        /// Executes the media library organization based on the provided options.
        /// </summary>
        public void Execute()
        {
            if (string.IsNullOrEmpty(this.SourcePath))
            {
                throw new InvalidOperationException("Source path is required.");
            }

            if ((this.RefreshJson || this.RepopulateJson) && !this.Track)
            {
                throw new InvalidOperationException("--refresh-json and --repopulate-json require --track.");
            }

            if (this.MaxZipDepth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.MaxZipDepth), "max-zip-depth must be 0 (unlimited) or a positive integer.");
            }

            DuplicateDisposition dup = ParseDuplicateDisposition(this.OnDuplicate);
            IFileErrorHandler? errorHandler = this.Interactive ? new ConsoleFileErrorHandler() : null;
            var organizerOptions = new OrganizerOptions
            {
                TrackManifest = this.Track,
                OnDuplicate = dup,
                CopyOnly = this.Copy,
                MaxZipNestingDepth = this.MaxZipDepth,
                Interactive = this.Interactive,
                ErrorHandler = errorHandler,
            };

            DirectoryInfo sourceDir = new DirectoryInfo(this.SourcePath);
            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDir.FullName}");
            }

            DirectoryInfo outputParent = Directory.CreateDirectory(this.OutputPath);
            DirectoryInfo mediaLibraryRoot = Directory.CreateDirectory(
                Path.Combine(outputParent.FullName, Constants.RuntimeDirectories.MediaLibraryDirectoryName));
            Directory.CreateDirectory(Path.Combine(outputParent.FullName, Constants.RuntimeDirectories.OriginalDirectoryName));
            Statics.OutputDirectory = mediaLibraryRoot;

            Log.Debug("Source Directory: {Path}", sourceDir.FullName);
            Log.Information("Output parent: {Path}", outputParent.FullName);
            Log.Information("Media library root: {Path}", mediaLibraryRoot.FullName);

            List<Exception> errors = new List<Exception>();
            ToolLayoutMigrator.MigrateIfNeeded(mediaLibraryRoot, errors);
            OrganizerRunState runState = new OrganizerRunState();
            DirectoryManager directoryManager = new DirectoryManager(sourceDir, mediaLibraryRoot, errors);
            BackupManager backupManager = new BackupManager(errors, directoryManager, this.Track);
            FileProcessor fileProcessor = new FileProcessor(backupManager, directoryManager, errors, organizerOptions);
            ZipProcessor zipProcessor = new ZipProcessor(
                directoryManager,
                organizerOptions.FingerprintZipContents,
                organizerOptions.CopyOnly,
                organizerOptions.ZipNestingDepthCutoff);
            MediaLibraryOrganizer organizer = new MediaLibraryOrganizer(
                backupManager,
                directoryManager,
                fileProcessor,
                zipProcessor,
                errors,
                runState);

            if (this.RefreshJson)
            {
                backupManager.PruneJsonBackup();
            }
            else if (this.RepopulateJson)
            {
                backupManager.RepopulateJsonBackup();
            }
            else
            {
                organizer.OrganizeFiles();
            }

            if (runState.Aborted)
            {
                Log.Warning("Organization aborted by user.");
                throw new InvalidOperationException("Organization aborted by user.");
            }

            if (errors.Any())
            {
                Log.Warning("Organization finished with {Count} errors.", errors.Count);
                foreach (Exception error in errors)
                {
                    Log.Error(error, "Collected Error");
                }

                throw new AggregateException($"Organization finished with {errors.Count} errors. See logs for details.", errors);
            }

            if (this.Interactive
                && !this.RefreshJson
                && !this.RepopulateJson
                && !PathUtilities.AreSameDirectory(sourceDir.FullName, outputParent.FullName))
            {
                InteractivePrompts.OfferSourceWipe(sourceDir);
            }
        }

        private static DuplicateDisposition ParseDuplicateDisposition(string raw)
        {
            switch (raw.Trim().ToLowerInvariant())
            {
                case "skip":
                    return DuplicateDisposition.Skip;
                case "delete":
                    return DuplicateDisposition.Delete;
                case "quarantine":
                    return DuplicateDisposition.Quarantine;
                default:
                    throw new ArgumentException($"Invalid --on-duplicate value '{raw}'. Use skip, delete, or quarantine.");
            }
        }
    }
}
