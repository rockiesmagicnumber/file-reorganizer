// <copyright file="Statics.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public static class Statics
    {
        public static IList<string> PhotoExtensions = GetPhotoFileExtensions();
        public static IList<string> VideoExtensions = GetVideoFileExtensions();
        public static IList<string> MusicExtensions = GetMusicFileExtensions();

        public static DirectoryInfo SourceDirectory = null;
        public static DirectoryInfo OutputDirectory = null;

        public static FileInfo GetJsonBackup()
        {
            return new FileInfo(Path.Combine(GetSokkaCorpDirectory().FullName, "jsonBackup.json"));
        }

        public static DirectoryInfo GetDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }
            else
            {
                DateTime fileDt = dt.Value;
                string destDir = Path.Combine(
                    GetProcessedDirectory().FullName,
                    fileDt.Year.ToString("0000"),
                    fileDt.Month.ToString("00"),
                    fileDt.Day.ToString("00"));
                return Directory.CreateDirectory(destDir);
            }
        }

        public static DirectoryInfo GetPhotoDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }
            else
            {
                DateTime fileDt = dt.Value;
                string destDir = Path.Combine(
                    GetProcessedPhotoDirectory().FullName,
                    fileDt.Year.ToString("0000"),
                    fileDt.Month.ToString("00"),
                    fileDt.Day.ToString("00"));
                return Directory.CreateDirectory(destDir);
            }
        }

        public static DirectoryInfo GetVideoDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }
            else
            {
                DateTime fileDt = dt.Value;
                string destDir = Path.Combine(
                    GetProcessedVideoDirectory().FullName,
                    fileDt.Year.ToString("0000"),
                    fileDt.Month.ToString("00"),
                    fileDt.Day.ToString("00"));
                return Directory.CreateDirectory(destDir);
            }
        }

        public static DirectoryInfo GetMiscDirectoryFromDateTime(DateTime? dt)
        {
            if (!dt.HasValue)
            {
                return GetProcessedDirectory();
            }
            else
            {
                DateTime fileDt = dt.Value;
                string destDir = Path.Combine(
                    GetProcessedMiscDirectory().FullName,
                    fileDt.Year.ToString("0000"),
                    fileDt.Month.ToString("00"),
                    fileDt.Day.ToString("00"));
                return Directory.CreateDirectory(destDir);
            }
        }

        // eg ~/MyExternalDrive/SokkaCorp
        public static DirectoryInfo GetSokkaCorpDirectory()
        {
            var ret = Path.Combine(
                OutputDirectory?.FullName ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Constants.RuntimeDirectories.SokkaCorpDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static string GetLogFilePath()
        {
            return Path.Combine(GetLogsDirectory().FullName, Constants.RuntimeFiles.LogFileName);
        }

        /// <summary>
        /// Returns Log Directory |
        /// eg ~/MyExternalDrive/SokkaCorp/Logs
        /// </summary>
        /// <returns><seealso cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetLogsDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.LogDirectoryName);
            return Directory.CreateDirectory(ret);
        }


        /// <summary>
        /// Returns Error Files Directory |
        /// eg ~/MyExternalDrive/SokkaCorp/Errors
        /// </summary>
        /// <returns><seealso cref="DirectoryInfo"/></returns>
        public static DirectoryInfo GetErrorDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.ErrorDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        // eg ~/MyExternalDrive/SokkaCorp/Processed
        public static DirectoryInfo GetProcessedDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.RuntimeDirectories.ProcessedDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetProcessedPhotoDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.PhotoFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetProcessedVideoDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.VideoFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetProcessedMusicDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetProcessedMiscDirectory()
        {
            var ret = Path.Combine(
                GetProcessedDirectory().FullName,
                Constants.FolderCategories.MiscFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetErrorPhotoDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.PhotoFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetErrorVideoDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.VideoFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetErrorMusicDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.MusicFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetErrorMiscDirectory()
        {
            var ret = Path.Combine(
                GetErrorDirectory().FullName,
                Constants.FolderCategories.MiscFolder);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetUnzippedDirectory(MediaLibraryOrganizerOptions options)
        {
            string ret = Path.Combine(
                path1: options.SourceDirectoryInfo.FullName,
                path2: Constants.RuntimeDirectories.UnzippedDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static void UnZip(FileInfo filePath, MediaLibraryOrganizerOptions options)
        {
            ZipFile.ExtractToDirectory(
                sourceArchiveFileName: filePath.FullName,
                destinationDirectoryName: GetUnzippedDirectory(options).FullName,
                overwriteFiles: true);
        }

        public static bool IsPhoto(this FileInfo filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = filePath.Extension.ToLowerInvariant();

            // Check against a list of common photo file extensions
            return PhotoExtensions.Contains(extension);
        }

        public static bool IsZip(this FileInfo filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = filePath.Extension.ToLowerInvariant();

            // Check against a list of common .zips - we're not gonna bother with gzip or whatever right now
            return extension.EndsWith(Constants.FileExtensions.Zip);
        }

        public static bool IsVideo(this FileInfo filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = filePath.Extension.ToLowerInvariant();

            // Check against a list of common video file extensions
            return VideoExtensions.Contains(extension);
        }

        public static bool IsMusic(this FileInfo filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = filePath.Extension.ToLowerInvariant();

            // Check against a list of common video file extensions
            return MusicExtensions.Contains(extension);
        }

        public static string GetChecksum(FileInfo file)
        {
            using var stream = new BufferedStream(file.OpenRead(), 1200000);
            using var sha = MD5.Create();
            byte[] checksum = sha.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        public static IList<string> GetPhotoFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Image).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    ret.Add(field.GetValue(null).ToString());
                }
            }

            return ret;
        }

        public static IList<string> GetVideoFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Video).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    ret.Add(field.GetValue(null).ToString());
                }
            }

            return ret;
        }

        public static IList<string> GetMusicFileExtensions()
        {
            List<string> ret = new List<string>();
            foreach (FieldInfo field in typeof(Constants.FileExtensions.Music).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType == typeof(string))
                {
                    ret.Add(field.GetValue(null).ToString());
                }
            }

            return ret;
        }
    }
}