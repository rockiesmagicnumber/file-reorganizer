// <copyright file="PhotoTakenDate.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using MetadataExtractor;

    /// <summary>
    /// Resolves capture date for photos: TagLib, then EXIF-style tags via MetadataExtractor, then last write time.
    /// </summary>
    internal static class PhotoTakenDate
    {
        private static readonly string[] ExifDateFormats =
        {
            "yyyy:MM:dd HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy:MM:dd",
        };

        /// <summary>
        /// Gets the best available date for sorting a photo into year/month/day folders.
        /// </summary>
        /// <param name="file">Image file.</param>
        /// <returns>Local date/time.</returns>
        public static DateTime GetBestDate(FileInfo file)
        {
            if (TryFromTagLib(file.FullName, out DateTime tagLibDt))
            {
                return tagLibDt;
            }

            if (TryFromMetadataExtractor(file.FullName, out DateTime mdeDt))
            {
                return mdeDt;
            }

            return file.LastWriteTime;
        }

        private static bool TryFromTagLib(string path, out DateTime result)
        {
            result = default;
            try
            {
                using TagLib.File tf = TagLib.File.Create(path);
                if (tf is TagLib.Image.File image && image.ImageTag?.DateTime.HasValue == true)
                {
                    result = image.ImageTag.DateTime.Value;
                    return true;
                }

                if (tf.Tag.DateTagged.HasValue)
                {
                    result = tf.Tag.DateTagged.Value;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool TryFromMetadataExtractor(string path, out DateTime result)
        {
            result = default;
            try
            {
                IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(path);
                foreach (MetadataExtractor.Directory directory in directories)
                {
                    foreach (MetadataExtractor.Tag tag in directory.Tags)
                    {
                        if (tag.Name is null || tag.Description is null)
                        {
                            continue;
                        }

                        if (!IsLikelyExifDateTag(tag.Name))
                        {
                            continue;
                        }

                        if (TryParseExifDate(tag.Description, out result))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool IsLikelyExifDateTag(string name)
        {
            return name.Contains("Date/Time Original", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Date/Time Digitized", StringComparison.OrdinalIgnoreCase)
                || name.Equals("Date/Time", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryParseExifDate(string description, out DateTime result)
        {
            if (DateTime.TryParse(description, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result))
            {
                return true;
            }

            foreach (string fmt in ExifDateFormats)
            {
                if (DateTime.TryParseExact(description, fmt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result))
                {
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
