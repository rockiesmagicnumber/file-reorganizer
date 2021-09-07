using System;
using System.Configuration;
using System.IO;
using System.Linq;
using static WallpaperReorganizer.EnumClasses;

namespace WallpaperReorganizer
{
    public class EveryoneUsesThese
    {
        public static void DeleteAllEmptyDirectories(DirectoryInfo rootPath)
        {
            if (rootPath.EnumerateDirectories().Count() > 0)
            {
                foreach (var dir in rootPath.EnumerateDirectories())
                {
                    DeleteAllEmptyDirectories(dir);
                }
            }
            if (rootPath.EnumerateFiles().Count() == 0 && rootPath.EnumerateDirectories().Count() == 0)
            {
                Directory.Delete(rootPath.FullName);
            }
        }

        public static double GetNumberOfLevels(int totalWallpapers)
        {
            return Math.Floor(Math.Log(totalWallpapers, double.Parse(ConfigurationManager.AppSettings[AppConstants.MagicNumber.ToString()])));
        }
    }
}
