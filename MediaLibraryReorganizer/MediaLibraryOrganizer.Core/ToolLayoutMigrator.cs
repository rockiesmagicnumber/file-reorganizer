// <copyright file="ToolLayoutMigrator.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Serilog;

    /// <summary>
    /// Moves legacy tool folders from the <c>MediaLibrary</c> root into <c>SokkaCorp/</c>.
    /// </summary>
    public static class ToolLayoutMigrator
    {
        private static readonly string[] LegacyToolDirectoryNames =
        {
            Constants.RuntimeDirectories.ErrorDirectoryName,
            Constants.RuntimeDirectories.DuplicatesDirectoryName,
            Constants.RuntimeDirectories.UnzippedDirectoryName,
        };

        /// <summary>
        /// Detects and migrates legacy <c>Errors/</c>, <c>Duplicates/</c>, and <c>Unzipped/</c> siblings of <c>SokkaCorp/</c>.
        /// </summary>
        /// <param name="mediaLibraryRoot"><c>{output parent}/MediaLibrary</c>.</param>
        /// <param name="errors">Collected migration failures.</param>
        public static void MigrateIfNeeded(DirectoryInfo mediaLibraryRoot, List<Exception> errors)
        {
            if (mediaLibraryRoot == null)
            {
                throw new ArgumentNullException(nameof(mediaLibraryRoot));
            }

            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            bool anyLegacy = false;
            foreach (string name in LegacyToolDirectoryNames)
            {
                if (Directory.Exists(Path.Combine(mediaLibraryRoot.FullName, name)))
                {
                    anyLegacy = true;
                    break;
                }
            }

            if (!anyLegacy)
            {
                return;
            }

            Log.Information("Legacy tool layout detected under {Path}; migrating into SokkaCorp/.", mediaLibraryRoot.FullName);
            DirectoryInfo sokkaCorp = Directory.CreateDirectory(
                Path.Combine(mediaLibraryRoot.FullName, Constants.RuntimeDirectories.SokkaCorpDirectoryName));

            foreach (string name in LegacyToolDirectoryNames)
            {
                MigrateLegacyDirectory(mediaLibraryRoot, sokkaCorp, name, errors);
            }
        }

        private static void MigrateLegacyDirectory(
            DirectoryInfo mediaLibraryRoot,
            DirectoryInfo sokkaCorp,
            string directoryName,
            List<Exception> errors)
        {
            string legacyPath = Path.Combine(mediaLibraryRoot.FullName, directoryName);
            if (!Directory.Exists(legacyPath))
            {
                return;
            }

            string targetPath = Path.Combine(sokkaCorp.FullName, directoryName);
            try
            {
                if (!Directory.Exists(targetPath))
                {
                    Log.Information("Moving legacy tool directory {From} -> {To}", legacyPath, targetPath);
                    Directory.Move(legacyPath, targetPath);
                    return;
                }

                Log.Information("Merging legacy tool directory {From} into {To}", legacyPath, targetPath);
                MergeDirectoryContents(new DirectoryInfo(legacyPath), new DirectoryInfo(targetPath), errors);
                TryDeleteEmptyDirectory(legacyPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to migrate legacy tool directory {Path}", legacyPath);
                errors.Add(ex);
            }
        }

        private static void MergeDirectoryContents(DirectoryInfo source, DirectoryInfo target, List<Exception> errors)
        {
            foreach (FileSystemInfo entry in source.EnumerateFileSystemInfos())
            {
                string destPath = Path.Combine(target.FullName, entry.Name);
                try
                {
                    if (entry is FileInfo file)
                    {
                        if (File.Exists(destPath))
                        {
                            Log.Warning("Skipping merge collision for file {Path}", destPath);
                            continue;
                        }

                        file.MoveTo(destPath);
                        Log.Information("Moved file {From} -> {To}", file.FullName, destPath);
                    }
                    else if (entry is DirectoryInfo directory)
                    {
                        if (Directory.Exists(destPath))
                        {
                            MergeDirectoryContents(directory, new DirectoryInfo(destPath), errors);
                            TryDeleteEmptyDirectory(directory.FullName);
                        }
                        else
                        {
                            directory.MoveTo(destPath);
                            Log.Information("Moved directory {From} -> {To}", directory.FullName, destPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to merge {Path} into {Target}", entry.FullName, destPath);
                    errors.Add(ex);
                }
            }
        }

        private static void TryDeleteEmptyDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var directory = new DirectoryInfo(path);
            if (directory.EnumerateFileSystemInfos().GetEnumerator().MoveNext())
            {
                Log.Warning("Legacy directory not empty after merge; leaving {Path}", path);
                return;
            }

            Directory.Delete(path);
            Log.Information("Removed empty legacy directory {Path}", path);
        }
    }
}
