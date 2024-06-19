// <copyright file="Constants.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    public static class Constants
    {
        public static class RuntimeFiles
        {
            private const string LogDateTimeFormat = "yyyy-MM-ddThh-mm-ss.ffff";

            public static string LogFileName => $"SokkaCorp-{DateTime.Now.ToString(LogDateTimeFormat)}";
        }

        public static class RuntimeDirectories
        {
            public const string ErrorDirectoryName = "Errors";
            public const string LogDirectoryName = "Logs";
            public const string OriginalDirectoryName = "Original";
            public const string ProcessedDirectoryName = "Processed";
            public const string SokkaCorpDirectoryName = "SokkaCorp";
        }

        public static class FolderCategories
        {
            public const string PhotoFolder = "Photos";
            public const string MusicFolder = "Music";
            public const string VideoFolder = "Videos";
            public const string MiscFolder = "Misc";
        }

        public static class FileExtensions
        {
            public const string Zip = ".zip";

            public static class Image
            {
                public const string JPG = ".jpg";
                public const string JPEG = ".jpeg";
                public const string PNG = ".png";
                public const string BMP = ".bmp";
                public const string GIF = ".gif";
                public const string TIFF = ".tiff";
                public const string TIF = ".tif";
                public const string WEBP = ".webp";
                public const string HEIF = ".heif";
                public const string HEIC = ".heic";
            }

            public static class Video
            {
                public const string ThreeG2 = ".3g2";
                public const string ThreeGP = ".3gp";
                public const string AMV = ".amv";
                public const string ASF = ".asf";
                public const string AVI = ".avi";
                public const string DRC = ".drc";
                public const string F4A = ".f4a";
                public const string F4B = ".f4b";
                public const string F4P = ".f4p";
                public const string F4V = ".f4v";
                public const string FLV = ".flv";
                public const string GIF = ".gif";
                public const string GIFV = ".gifv";
                public const string M2TS = ".M2TS";
                public const string M2V = ".m2v";
                public const string M4P = ".m4p";
                public const string M4V = ".m4v";
                public const string MKV = ".mkv";
                public const string MNG = ".mng";
                public const string MOV = ".mov";
                public const string MP2 = ".mp2";
                public const string MP4 = ".mp4";
                public const string MPE = ".mpe";
                public const string MPEG = ".mpeg";
                public const string MPG = ".mpg";
                public const string MPV = ".mpv";
                public const string MTS = ".MTS";
                public const string MXF = ".mxf";
                public const string NSV = ".nsv";
                public const string OGG = ".ogg";
                public const string OGV = ".ogv";
                public const string QT = ".qt";
                public const string RM = ".rm";
                public const string RMVB = ".rmvb";
                public const string ROQ = ".roq";
                public const string SVI = ".svi";
                public const string TS = ".TS";
                public const string VIV = ".viv";
                public const string VOB = ".vob";
                public const string WEBM = ".webm";
                public const string WMV = ".wmv";
                public const string YUV = ".yuv";
            }
        }
    }
}
