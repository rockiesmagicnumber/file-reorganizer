// <copyright file="Constants.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;

namespace PhotoLibraryCleaner.Lib
{
    public static class Statics
    {
        public static IList<string> PhotoExtensions = GetPhotoFileExtensions();
        public static IList<string> VideoExtensions = GetVideoFileExtensions();

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

        public static DirectoryInfo GetSokkaCorpDirectory()
        {
            var ret = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Constants.SokkaCorpDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetErrorDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.ErrorDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetOriginalDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.OriginalDirectoryName);
            return Directory.CreateDirectory(ret);
        }

        public static DirectoryInfo GetProcessedDirectory()
        {
            var ret = Path.Combine(
                GetSokkaCorpDirectory().FullName,
                Constants.ProcessedDirectoryName);
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

        public static void UnZip(string filePath)
        {
            string dname = Path.GetDirectoryName(filePath) ?? filePath.Replace(Path.GetFileNameWithoutExtension(filePath), string.Empty);
            string newDname = Path.Combine(dname, Path.GetFileNameWithoutExtension(filePath));
            System.IO.Directory.CreateDirectory(newDname);
            ZipFile.ExtractToDirectory(filePath, dname);
        }

        public static bool IsPhoto(this string filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Check against a list of common photo file extensions
            return PhotoExtensions.Contains(extension);
        }

        public static bool IsZip(this string filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Check against a list of common .zips - we're not gonna bother with gzip or whatever right now
            return extension.EndsWith(Constants.FileExtensions.Zip);
        }

        public static bool IsVideo(this string filePath)
        {
            // Get the file extension in lowercase for case-insensitive comparison
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Check against a list of common .zips - we're not gonna bother with gzip or whatever right now
            return extension.EndsWith(Constants.FileExtensions.Zip);
        }

        private static string GetChecksum(string file)
        {
            using var stream = new BufferedStream(File.OpenRead(file), 1200000);
            using var sha = SHA256.Create();
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
    }
}