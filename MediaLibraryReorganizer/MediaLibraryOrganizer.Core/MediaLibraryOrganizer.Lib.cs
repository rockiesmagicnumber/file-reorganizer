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
        private static readonly string[] SkippedDirectoryNamesExact =
        {
            "$RECYCLE.BIN",
            "__MACOSX",
            "System Volume Information",
            "lost+found",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibraryOrganizer"/> class.
        /// </summary>
        /// <param name="backupManager">The backup manager dependency.</param>
        /// <param name="directoryManager">The directory manager dependency.</param>
        /// <param name="fileProcessor">The file processor dependency.</param>
        /// <param name="zipProcessor">The zip processor dependency.</param>
        /// <param name="errors">The list to collect errors.</param>
        /// <param name="runState">Shared run outcome (abort flag).</param>
        public MediaLibraryOrganizer(BackupManager backupManager, DirectoryManager directoryManager, FileProcessor fileProcessor, ZipProcessor zipProcessor, List<Exception> errors, OrganizerRunState runState)
        {
            this.BackupManager = backupManager;
            this.DirectoryManager = directoryManager;
            this.FileProcessor = fileProcessor;
            this.ZipProcessor = zipProcessor;
            this.Errors = errors;
            this.RunState = runState;
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
        /// Gets the shared run state.
        /// </summary>
        private OrganizerRunState RunState { get; }

        /// <summary>
        /// Organizes files from the source directory into the output directory.
        /// </summary>
        public void OrganizeFiles()
        {
            StaticLog.Enter(nameof(this.OrganizeFiles));
            try
            {
                try
                {
                    this.ProcessDirectoryNode(this.DirectoryManager.OriginalSourceDirectory, 0);
                }
                catch (OrganizerAbortedException ex)
                {
                    Log.Warning("Organization aborted by user: {Message}", ex.Message);
                    this.RunState.Aborted = true;
                }
                finally
                {
                    this.DirectoryManager.CleanupUnzippedScratch();
                }
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
        /// Walks one directory level at a time: non-zip files are imported; each zip is expanded to a short-lived scratch folder, processed depth-first, then removed.
        /// </summary>
        /// <param name="directory">Directory to walk.</param>
        /// <param name="zipNestingDepth">Archive nesting level (increments after each successful outer extraction).</param>
        private void ProcessDirectoryNode(DirectoryInfo directory, int zipNestingDepth)
        {
            StaticLog.Enter($"{nameof(this.ProcessDirectoryNode)} - {directory.FullName}");
            try
            {
                if (this.ShouldSkipDirectory(directory))
                {
                    Log.Debug("Skipping directory {Path}", directory.FullName);
                    return;
                }

                foreach (FileInfo file in directory.EnumerateFiles())
                {
                    if (file.IsZip())
                    {
                        this.ZipProcessor.ExtractProcessAndCleanup(
                            file,
                            zipNestingDepth,
                            extractedRoot => this.ProcessDirectoryNode(extractedRoot, zipNestingDepth + 1));
                    }
                    else
                    {
                        this.FileProcessor.ProcessFile(file);
                    }
                }

                foreach (DirectoryInfo subDir in directory.EnumerateDirectories())
                {
                    this.ProcessDirectoryNode(subDir, zipNestingDepth);
                }
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessDirectoryNode));
            }
        }

        private bool ShouldSkipDirectory(DirectoryInfo directory)
        {
            string name = directory.Name;
            foreach (string candidate in SkippedDirectoryNamesExact)
            {
                if (name.Equals(candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            if (name.StartsWith(".Trash", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return this.IsUnderOutputTree(directory);
        }

        private bool IsUnderOutputTree(DirectoryInfo directory)
        {
            DirectoryInfo outputParent = this.DirectoryManager.OutputDirectory.Parent
                ?? throw new InvalidOperationException("Media library root has no parent directory.");
            string full = NormalizeDirectoryPath(directory.FullName);
            string mediaLibrary = NormalizeDirectoryPath(this.DirectoryManager.OutputDirectory.FullName);
            string original = NormalizeDirectoryPath(
                Path.Combine(outputParent.FullName, Constants.RuntimeDirectories.OriginalDirectoryName));

            return IsSameOrUnder(full, mediaLibrary) || IsSameOrUnder(full, original);
        }

        private static string NormalizeDirectoryPath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool IsSameOrUnder(string candidate, string root)
        {
            if (string.Equals(candidate, root, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string prefix = root + Path.DirectorySeparatorChar;
            return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
