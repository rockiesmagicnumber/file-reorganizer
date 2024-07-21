// <copyright file="Constants.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public static class Constants
    {
        public static class ArgumentFlags
        {
            public const string Source = "--source";
            public const string Output = "--output";
            public const string ExcludeDuplicates = "--exclude-duplicates";
        }

        public static class RuntimeFiles
        {
            private const string LogDateTimeFormat = "yyyy-MM-ddThh-mm-ss.ffff";

            public static string LogFileName => $"SokkaCorp-{DateTime.Now.ToString(LogDateTimeFormat)}";
        }

        public static class RuntimeDirectories
        {
            public const string ErrorDirectoryName = "Errors";
            public const string LogDirectoryName = "Logs";
            public const string ProcessedDirectoryName = "Processed";
            public const string SokkaCorpDirectoryName = "SokkaCorp";
            public const string UnzippedDirectoryName = "Unzipped";
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

            public static class Music
            {
                public const string AA = ".aa";
                public const string AAC = ".aac";
                public const string AAX = ".aax";
                public const string ACT = ".act";
                public const string AIFF = ".aiff";
                public const string ALAC = ".alac";
                public const string AMR = ".amr";
                public const string APE = ".ape";
                public const string AU = ".au";
                public const string AWB = ".awb";
                public const string CDA = ".cda";
                public const string DSS = ".dss";
                public const string DVF = ".dvf";
                public const string EightSVX = ".8svx";
                public const string FLAC = ".flac";
                public const string GSM = ".gsm";
                public const string IKLAX = ".iklax";
                public const string IVS = ".ivs";
                public const string M4A = ".m4a";
                public const string M4B = ".m4b";
                public const string M4P = ".m4p";
                public const string MMF = ".mmf";
                public const string MOGG = ".mogg";
                public const string MOVPKG = ".movpkg";
                public const string MP3 = ".mp3";
                public const string MPC = ".mpc";
                public const string MSV = ".msv";
                public const string NMF = ".nmf ";
                public const string OGA = ".oga";
                public const string OGG = ".ogg";
                public const string OPUS = ".opus";
                public const string RA = ".ra";
                public const string RAW = ".raw";
                public const string RF64 = ".rf64";
                public const string RM = ".rm";
                public const string SLN = ".sln";
                public const string TTA = ".tta";
                public const string VOC = ".voc";
                public const string VOX = ".vox";
                public const string WAV = ".wav";
                public const string WEBM = ".webm";
                public const string WMA = ".wma";
                public const string WV = ".wv";
            }
        }
    }
}
