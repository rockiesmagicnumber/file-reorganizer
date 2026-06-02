// <copyright file="InteractivePrompts.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
    using System.IO;
    using Serilog;

    /// <summary>
    /// Post-run interactive prompts (source wipe, etc.).
    /// </summary>
    internal static class InteractivePrompts
    {
        /// <summary>
        /// Offers to delete all contents under the source root after a successful flash merge.
        /// </summary>
        /// <param name="sourceDirectory">Flash drive root that was imported.</param>
        /// <returns>True when the user confirmed and wipe completed.</returns>
        public static bool OfferSourceWipe(DirectoryInfo sourceDirectory)
        {
            Console.WriteLine();
            Console.WriteLine("Migration completed successfully.");
            Console.WriteLine($"Source: {sourceDirectory.FullName}");
            Console.Write("Wipe all files and folders under this source? [y/N]: ");

            string? line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line) || (line.Trim()[0] != 'y' && line.Trim()[0] != 'Y'))
            {
                Log.Information("Source wipe declined by user.");
                return false;
            }

            WipeSourceContents(sourceDirectory);
            Log.Information("Source wipe completed: {Path}", sourceDirectory.FullName);
            Console.WriteLine("Source wiped.");
            return true;
        }

        private static void WipeSourceContents(DirectoryInfo sourceDirectory)
        {
            foreach (FileSystemInfo entry in sourceDirectory.EnumerateFileSystemInfos())
            {
                if (entry is FileInfo file)
                {
                    Log.Information("Deleting file: {Path}", file.FullName);
                    file.Delete();
                }
                else if (entry is DirectoryInfo directory)
                {
                    Log.Information("Deleting directory: {Path}", directory.FullName);
                    directory.Delete(true);
                }
            }
        }
    }
}
