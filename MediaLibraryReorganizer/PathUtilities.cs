// <copyright file="PathUtilities.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
    using System.IO;

    /// <summary>
    /// Path comparison helpers for CLI safety checks.
    /// </summary>
    internal static class PathUtilities
    {
        /// <summary>
        /// Returns true when two paths refer to the same directory after normalization.
        /// </summary>
        /// <param name="left">First path.</param>
        /// <param name="right">Second path.</param>
        /// <returns>True when equivalent directory paths.</returns>
        public static bool AreSameDirectory(string left, string right)
        {
            string normalizedLeft = NormalizeDirectoryPath(left);
            string normalizedRight = NormalizeDirectoryPath(right);
            return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDirectoryPath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
