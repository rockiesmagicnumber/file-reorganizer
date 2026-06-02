// <copyright file="Statics.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;

    /// <summary>
    /// Shared helpers: logging path root, checksums, extension classification.
    /// Layout paths belong on <see cref="DirectoryManager"/>.
    /// </summary>
    public static class Statics
    {
        private static readonly Lazy<HashSet<string>> PhotoExtensionSet = new Lazy<HashSet<string>>(
            () => BuildExtensionSet(typeof(Constants.FileExtensions.Image)));

        private static readonly Lazy<HashSet<string>> VideoExtensionSet = new Lazy<HashSet<string>>(
            () => BuildExtensionSet(typeof(Constants.FileExtensions.Video)));

        private static readonly Lazy<HashSet<string>> MusicExtensionSet = new Lazy<HashSet<string>>(
            () => BuildExtensionSet(typeof(Constants.FileExtensions.Music)));

        private static readonly Lazy<HashSet<string>> ZipExtensionSet = new Lazy<HashSet<string>>(
            () => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Constants.FileExtensions.Zip });

        /// <summary>
        /// Gets or sets the output directory used only for <see cref="GetLogFilePath"/> before <see cref="DirectoryManager"/> exists.
        /// </summary>
        public static DirectoryInfo? OutputDirectory { get; set; }

        /// <summary>
        /// Gets the full path to the log file under <c>{output}/SokkaCorp/Logs</c>.
        /// </summary>
        /// <returns>Log file path.</returns>
        public static string GetLogFilePath()
        {
            string root = OutputDirectory?.FullName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string logsDir = Path.Combine(
                root,
                Constants.RuntimeDirectories.SokkaCorpDirectoryName,
                Constants.RuntimeDirectories.LogDirectoryName);
            Directory.CreateDirectory(logsDir);
            return Path.Combine(logsDir, Constants.RuntimeFiles.LogFileName);
        }

        /// <summary>
        /// Determines whether the specified file is a photo.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a photo; otherwise, false.</returns>
        public static bool IsPhoto(this FileInfo filePath)
        {
            return ExtensionMatches(filePath.Extension, PhotoExtensionSet.Value);
        }

        /// <summary>
        /// Determines whether the specified file is a ZIP archive.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a ZIP archive; otherwise, false.</returns>
        public static bool IsZip(this FileInfo filePath)
        {
            return ExtensionMatches(filePath.Extension, ZipExtensionSet.Value);
        }

        /// <summary>
        /// Determines whether the specified file is a video.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a video; otherwise, false.</returns>
        public static bool IsVideo(this FileInfo filePath)
        {
            return ExtensionMatches(filePath.Extension, VideoExtensionSet.Value);
        }

        /// <summary>
        /// Determines whether the specified file is a music file.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a music file; otherwise, false.</returns>
        public static bool IsMusic(this FileInfo filePath)
        {
            return ExtensionMatches(filePath.Extension, MusicExtensionSet.Value);
        }

        /// <summary>
        /// Computes the MD5 checksum of a file.
        /// </summary>
        /// <param name="file">The file to compute the checksum for.</param>
        /// <returns>The MD5 checksum as a hexadecimal string.</returns>
        public static string GetChecksum(FileInfo file)
        {
            using var stream = new BufferedStream(file.OpenRead(), 1200000);
            using var md5 = MD5.Create();
            byte[] checksum = md5.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        /// <summary>
        /// Gets the list of supported photo file extensions.
        /// </summary>
        /// <returns>A list of supported photo file extensions.</returns>
        public static IList<string> GetPhotoFileExtensions()
        {
            return PhotoExtensionSet.Value.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Gets the list of supported video file extensions.
        /// </summary>
        /// <returns>A list of supported video file extensions.</returns>
        public static IList<string> GetVideoFileExtensions()
        {
            return VideoExtensionSet.Value.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// Gets the list of supported music file extensions.
        /// </summary>
        /// <returns>A list of supported music file extensions.</returns>
        public static IList<string> GetMusicFileExtensions()
        {
            return MusicExtensionSet.Value.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static bool ExtensionMatches(string extension, HashSet<string> set)
        {
            return !string.IsNullOrEmpty(extension) && set.Contains(extension);
        }

        private static HashSet<string> BuildExtensionSet(Type nestedType)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(string))
                {
                    continue;
                }

                string? value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    set.Add(value);
                }
            }

            return set;
        }
    }
}
