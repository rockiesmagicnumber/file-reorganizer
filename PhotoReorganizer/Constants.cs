// <copyright file="Constants.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    public static class Constants
    {
        public const string ErrorDirectoryName = "Errors";
        public const string LogDirectoryName = "Logs";
        public const string ProcessedDirectoryName = "Processed";
        public const string SokkaCorpDirectoryName = "SokkaCorp";

        public static class FileExtensions
        {
            public static class Zip
            {
                public const string ZipFileExtension = ".zip";
            }

            public static class Image
            {
                public const string JPGFileExtension = ".jpg";
                public const string JPEGFileExtension = ".jpeg";
                public const string PNGFileExtension = ".png";
                public const string BMPFileExtension = ".bmp";
                public const string GIFFileExtension = ".gif";
                public const string TIFFFileExtension = ".tiff";
                public const string TIFFileExtension = ".tif";
                public const string WEBPFileExtension = ".webp";
                public const string HEIFFileExtension = ".heif";
                public const string HEICFileExtension = ".heic";
            }
        }
    }
}
