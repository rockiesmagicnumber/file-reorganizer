using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IOFile = System.IO.File;
using TagFile = TagLib.File;

namespace MusicReorganizer
{
    class Program
    {
        public const string ArchiveFolder = @"F:\Media\Music_ZIP";
        public const string UnzipFolder = @"F:\Media\Music_ZIP_UNZIP";
        public const string MusicFolder = @"F:\Media\Music";
        public const string UnknownMusicFolder = @"F:\Media\Music\Unknown Files";
        public const string UnzipMusicSubPath = @"F:\Media\Music_ZIP_UNZIP\Takeout\YouTube and YouTube Music\music-uploads";
        public const string UnknownArtist = "Unknown Artist";
        public const string UnknownAlbum = "Unknown Album";
        public const string LogFile = @"E:\Repos\MusicReorganizer\MusicReorganizer\Log.txt";

        static void Main(string[] args)
        {
            Log("Beginning Routine");
            Directory.CreateDirectory(MusicFolder);
            Log($"MusicFolder created at {MusicFolder}");
            EmptyUnzipFolder();
            var AllZipFiles = Directory.GetFiles(ArchiveFolder, "*.zip", SearchOption.AllDirectories);
            for (int i = 0; i < AllZipFiles.Length; i++)
            {
                string zip = AllZipFiles[i];
                UnzipFile(zip);
                var AllUnzippedFiles = Directory.GetFiles(UnzipMusicSubPath);

                foreach (string mp3 in AllUnzippedFiles)
                {
                    try
                    {
                        ProcessFile(mp3);
                    }
                    catch (Exception ex)
                    {
                        MoveToUnknown(mp3);
                        Log(ex.Message, LogType.ERROR);
                    }
                }
                Log($"{Path.GetFileName(zip)} files organized");
                EmptyUnzipFolder();
            }

            // jk it's not an error
            Log("PHEW, that's all of them!", LogType.ERROR);
            Console.ReadLine();
        }

        static void EmptyUnzipFolder()
        {
            foreach (var f in Directory.GetFiles(UnzipFolder, "*.*", SearchOption.AllDirectories))
            {
                IOFile.Delete(f);
            }
            foreach (var d in Directory.GetDirectories(UnzipFolder))
            {
                DeleteAllDirectories(d);
            }
            Log("UnzipFolder emptied");
        }

        static void DeleteAllDirectories(string directoryPath)
        {
            string[] v = Directory.GetDirectories(directoryPath);
            if (v.Any())
            {
                foreach (var d in v)
                {
                    DeleteAllDirectories(d);
                }
            }
            Directory.Delete(directoryPath);
        }

        static void Log(string msg, LogType logType = LogType.INFO)
        {
            string FormattedMsg = $"{Environment.NewLine}{DateTime.Now.ToString()}\t\t{logType.ToString()}\t{msg}";
            if (logType == LogType.ERROR)
            {
                Console.Write(FormattedMsg);
            }
            IOFile.AppendAllText(LogFile, FormattedMsg);
        }

        static void UnzipFile(string zip)
        {
            FastZip z = new FastZip();
            z.ExtractZip(zip, UnzipFolder, string.Empty);
            Log($"File \"{zip}\" unzipped");
        }

        static void ProcessFile(string mp3)
        {
            var songDetails = TagFile.Create(mp3);
            string Artist = songDetails.Tag.AlbumArtists.Length > 0 ?
                songDetails.Tag.AlbumArtists[0] :
                    songDetails.Tag.Artists.Length > 0 ?
                        songDetails.Tag.Artists[0] : string.Empty;

            if (string.IsNullOrEmpty(Artist))
            {
                Artist = UnknownArtist;
                Log($"Artist not found; using \"{UnknownArtist}\"");
            }
            Artist = CleanFilename(Artist);

            string AlbumTitle = songDetails.Tag.Album;
            if (string.IsNullOrEmpty(AlbumTitle))
            {
                AlbumTitle = UnknownAlbum;
                Log($"Album not found; using \"{UnknownAlbum}\"");
            }
            AlbumTitle = CleanFilename(AlbumTitle);

            string newMp3 = Path.Combine(MusicFolder, Artist, AlbumTitle, CleanFilename(Path.GetFileName(mp3)));
            Directory.CreateDirectory(Path.GetDirectoryName(newMp3));
            if (!IOFile.Exists(newMp3))
            {
                Log($"Moving \"{Path.GetFileName(newMp3)}\" to \"{Path.GetDirectoryName(newMp3)}\"");
                IOFile.Move(mp3, newMp3);
            }
            else { Log($"\"{Path.GetFileName(newMp3)}\" already exists in \"{Path.GetDirectoryName(newMp3)}\"; moving to next file"); }
        }

        static void MoveToUnknown(string mp3)
        {
            var badfile = Path.Combine(UnknownMusicFolder, CleanFilename(Path.GetFileName(mp3)));
            if (!IOFile.Exists(badfile))
            {
                Directory.CreateDirectory(UnknownMusicFolder);
                IOFile.Move(mp3, Path.Combine(UnknownMusicFolder, badfile));
                Log($"\"{mp3}\" moved to \"Unknown Music Folder\"");
            }
        }

        //static string CleanFilename(string filename)
        //{
        //    string newFilename = filename;
        //    string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        //    foreach (char c in invalid)
        //    {
        //        newFilename = newFilename.Replace(c.ToString(), "");
        //    }
        //    return newFilename;
        //}

        private static HashSet<char> _invalidCharsHash;
        private static HashSet<char> InvalidCharsHash
        {
            get { return _invalidCharsHash ?? (_invalidCharsHash = new HashSet<char>(Path.GetInvalidFileNameChars())); }
        }

        private static string CleanFilename(string fileName, string newValue = "_")
        {
            char newChar = newValue[0];

            char[] chars = fileName.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (InvalidCharsHash.Contains(c))
                    chars[i] = newChar;
            }

            return new string(chars);
        }

        enum LogType
        {
            INFO = 1,
            ERROR,
            DEBUG
        }
    }
}
