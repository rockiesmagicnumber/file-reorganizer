using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using static WallpaperReorganizer.EnumClasses;

namespace WallpaperReorganizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootFolder = new DirectoryInfo(ConfigurationManager.AppSettings[AppConstants.WallpaperFolder.ToString()]);

            if (args[0].ToLower() == "process")
            {
                List<string> allWallpapers = rootFolder.EnumerateFiles().Select(x => x.FullName).ToList();

                while (allWallpapers.Count > 0)
                {
                    new Helpers().Process(rootFolder, ref allWallpapers);
                }
            }

            else if (args[0].ToLower() == "undo")
            {
                Helpers.RestoreFiles(rootFolder.FullName);
            }
        }
    }
}
