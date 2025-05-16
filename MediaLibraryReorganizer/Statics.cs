// <copyright file="Statics.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography;

    /// <summary>
    /// Provides static utility methods for file and directory operations.
    /// </summary>
    public static class Statics
    {
        /// <summary>
        /// Gets or sets the source directory for media files.
        /// </summary>
        public static DirectoryInfo? SourceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the output directory for processed files.
        /// </summary>
        public static DirectoryInfo? OutputDirectory { get; set; }

        /// <summary>
        /// Gets the JSON backup file location.
        /// </summary>
        /// <returns>The file info for the JSON backup file.</returns>
        public static FileInfo GetJsonBackup()
        {
            return new FileInfo(Path.Combine(GetSokkaCorpDirectory().FullName, "jsonBackup.json"));
        }

        /// <summary>
        /// Gets a directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure. If null, returns the base processed directory.</param>
        /// <returns>A directory info object for the specified date.</returns>
        public static DirectoryInfo GetDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }

            DateTime fileDt = dt.Value;
            string destDir = Path.Combine(
                GetProcessedDirectory().FullName,
                fileDt.Year.ToString("0000"),
                fileDt.Month.ToString("00"),
                fileDt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets a photo directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure. If null, returns the base processed directory.</param>
        /// <returns>A directory info object for the photo files location.</returns>
        public static DirectoryInfo GetPhotoDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }

            DateTime fileDt = dt.Value;
            string destDir = Path.Combine(
                GetProcessedPhotoDirectory().FullName,
                fileDt.Year.ToString("0000"),
                fileDt.Month.ToString("00"),
                fileDt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets a video directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure. If null, returns the base processed directory.</param>
        /// <returns>A directory info object for the video files location.</returns>
        public static DirectoryInfo GetVideoDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }

            DateTime fileDt = dt.Value;
            string destDir = Path.Combine(
                GetProcessedVideoDirectory().FullName,
                fileDt.Year.ToString("0000"),
                fileDt.Month.ToString("00"),
                fileDt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets a misc directory path based on the provided date and time.
        /// </summary>
        /// <param name="dt">The date and time to create the directory structure. If null, returns the base processed directory.</param>
        /// <returns>A directory info object for the misc files location.</returns>
        public static DirectoryInfo GetMiscDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }

            DateTime fileDt = dt.Value;
            string destDir = Path.Combine(
                GetProcessedMiscDirectory().FullName,
                fileDt.Year.ToString("0000"),
                fileDt.Month.ToString("00"),
                fileDt.Day.ToString("00"));
            return Directory.CreateDirectory(destDir);
        }

        /// <summary>
        /// Gets the root SokkaCorp directory (~/MyExternalDrive/SokkaCorp).
        /// </summary>
        /// <returns>The directory info for the SokkaCorp root location.</returns>
        public static DirectoryInfo GetSokkaCorpDirectory()
        {
            var ret = Path.Combine(
                OutputDirectory?.FullName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Constants.RuntimeDirectories.SokkaCorpDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the full path to the log file.
        /// </summary>
        /// <returns>The complete path to the log file.</returns>
        public static string GetLogFilePath()
        {
            return Path.Combine(GetLogsDirectory().FullName, Constants.RuntimeFiles.LogFileName);
        }

        /// <summary>
        /// Gets the logs directory (~/MyExternalDrive/SokkaCorp/Logs).
        /// </summary>
        /// <returns>The directory info for the logs location.</returns>
        public static DirectoryInfo GetLogsDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.LogDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the error files directory (~/MyExternalDrive/SokkaCorp/Errors).
        /// </summary>
        /// <returns>The directory info for the error files location.</returns>
        public static DirectoryInfo GetErrorDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.ErrorDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the processed files directory (~/MyExternalDrive/SokkaCorp/Processed).
        /// </summary>
        /// <returns>The directory info for the processed files location.</returns>
        public static DirectoryInfo GetProcessedDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.ProcessedDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the processed photos directory (~/MyExternalDrive/SokkaCorp/Processed/Photos).
        /// </summary>
        /// <returns>The directory info for the processed photos location.</returns>
        public static DirectoryInfo GetProcessedPhotoDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.PhotoFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the processed videos directory (~/MyExternalDrive/SokkaCorp/Processed/Videos).
        /// </summary>
        /// <returns>The directory info for the processed videos location.</returns>
        public static DirectoryInfo GetProcessedVideoDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.VideoFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the processed music directory (~/MyExternalDrive/SokkaCorp/Processed/Music).
        /// </summary>
        /// <returns>The directory info for the processed music location.</returns>
        public static DirectoryInfo GetProcessedMusicDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the processed misc files directory (~/MyExternalDrive/SokkaCorp/Processed/Misc).
        /// </summary>
        /// <returns>The directory info for the processed misc files location.</returns>
        public static DirectoryInfo GetProcessedMiscDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.MiscFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the error photos directory (~/MyExternalDrive/SokkaCorp/Errors/Photos).
        /// </summary>
        /// <returns>The directory info for the error photos location.</returns>
        public static DirectoryInfo GetErrorPhotoDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.PhotoFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the error videos directory (~/MyExternalDrive/SokkaCorp/Errors/Videos).
        /// </summary>
        /// <returns>The directory info for the error videos location.</returns>
        public static DirectoryInfo GetErrorVideoDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.VideoFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the error music directory (~/MyExternalDrive/SokkaCorp/Errors/Music).
        /// </summary>
        /// <returns>The directory info for the error music location.</returns>
        public static DirectoryInfo GetErrorMusicDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the error misc files directory (~/MyExternalDrive/SokkaCorp/Errors/Misc).
        /// </summary>
        /// <returns>The directory info for the error misc files location.</returns>
        public static DirectoryInfo GetErrorMiscDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.MiscFolder);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Gets the unzipped files directory.
        /// </summary>
        /// <param name="sourceDirectory">The source directory containing the files to process.</param>
        /// <returns>The directory info for the unzipped files location.</returns>
        public static DirectoryInfo GetUnzippedDirectory(DirectoryInfo sourceDirectory)
        {
            string ret = Path.Combine(
                path1: sourceDirectory.FullName,
                path2: Constants.RuntimeDirectories.UnzippedDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        /// <summary>
        /// Determines whether the specified file is a photo.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a photo; otherwise, false.</returns>
        public static bool IsPhoto(this FileInfo filePath)
        {
            string extension = filePath.Extension.ToLowerInvariant();
            return GetPhotoFileExtensions().Contains(extension);
        }

        /// <summary>
        /// Determines whether the specified file is a ZIP archive.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a ZIP archive; otherwise, false.</returns>
        public static bool IsZip(this FileInfo filePath)
        {
            string extension = filePath.Extension.ToLowerInvariant();
            return extension.EndsWith(Constants.FileExtensions.Zip);
        }

        /// <summary>
        /// Determines whether the specified file is a video.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a video; otherwise, false.</returns>
        public static bool IsVideo(this FileInfo filePath)
        {
            string extension = filePath.Extension.ToLowerInvariant();
            return GetVideoFileExtensions().Contains(extension);
        }

        /// <summary>
        /// Determines whether the specified file is a music file.
        /// </summary>
        /// <param name="filePath">The file to check.</param>
        /// <returns>True if the file is a music file; otherwise, false.</returns>
        public static bool IsMusic(this FileInfo filePath)
        {
            string extension = filePath.Extension.ToLowerInvariant();
            return GetMusicFileExtensions().Contains(extension);
        }

        /// <summary>
        /// Computes the MD5 checksum of a file.
        /// </summary>
        /// <param name="file">The file to compute the checksum for.</param>
        /// <returns>The MD5 checksum as a hexadecimal string.</returns>
        public static string GetChecksum(FileInfo file)
        {
            using var stream = new BufferedStream(file.OpenRead(), 1200000);
            using var sha = MD5.Create();
            byte[] checksum = sha.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        /// <summary>
        /// Gets the list of supported photo file extensions.
        /// </summary>
        /// <returns>A list of supported photo file extensions.</returns>
        public static IList<string> GetPhotoFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Image).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    string value = field.GetValue(null)?.ToString() ?? string.Empty;
                    ret.Add(value);
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the list of supported video file extensions.
        /// </summary>
        /// <returns>A list of supported video file extensions.</returns>
        public static IList<string> GetVideoFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Video).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    string value = field.GetValue(null)?.ToString() ?? string.Empty;
                    ret.Add(value);
                }
            }

            return ret;
        }

        /// <summary>
        /// Gets the list of supported music file extensions.
        /// </summary>
        /// <returns>A list of supported music file extensions.</returns>
        public static IList<string> GetMusicFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Music).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    string value = field?.GetValue(null)?.ToString() ?? string.Empty;
                    ret.Add(value);
                }
            }

            return ret;
        }
    }
}