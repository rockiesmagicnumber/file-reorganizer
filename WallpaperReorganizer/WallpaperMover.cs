using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using static WallpaperReorganizer.EnumClasses;

namespace WallpaperReorganizer
{
    public class WallpaperMover
    {
        public static double MagicNumber => double.Parse(ConfigurationManager.AppSettings[AppConstants.MagicNumber.ToString()]);
        public static void CreateChildFolders(DirectoryInfo root, List<string> Wallpapers, double NumberOfLevels, double recursionLevel = 1)
        {
            for (int i = 1; i <= MagicNumber; i++)
            {
                var sub = root.CreateSubdirectory(i.ToString());
                if (recursionLevel < NumberOfLevels)
                {
                    CreateChildFolders(sub, Wallpapers, NumberOfLevels, recursionLevel + 1);
                }
            }
        }
        public static void PopulateChildFolders(DirectoryInfo root, List<string> Wallpapers, double NumberOfLevels, double recursionLevel = 1)
        {
            for (int i = 1; i <= MagicNumber; i++)
            {
                var sub = Directory.CreateDirectory(Path.Combine(root.FullName, i.ToString()));
                if (recursionLevel < NumberOfLevels)
                {
                    PopulateChildFolders(sub, Wallpapers, NumberOfLevels, recursionLevel + 1);
                }
                else
                {
                    Populate(sub, ref Wallpapers);
                }
            }
        }

        public static void Populate(DirectoryInfo newFolder, ref List<string> allWallpapers)
        {
            int numFilesToMove = (int)(MagicNumber - newFolder.EnumerateFiles().Count());
            var currentFiles = allWallpapers.TakeLast(numFilesToMove).ToList();
            foreach (var currentFile in currentFiles)
            {
                // move file to new location
                File.Move(currentFile, Path.Combine(newFolder.FullName, Path.GetFileName(currentFile)));
                allWallpapers.Remove(currentFile);
            }
        }
    }
}
