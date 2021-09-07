using System.Configuration;
using System.Collections.Generic;
using System.IO;
using static WallpaperReorganizer.EnumClasses;

namespace WallpaperReorganizer
{
    public class Helpers
    {
        public static DirectoryInfo RootFolder => new DirectoryInfo(ConfigurationManager.AppSettings[AppConstants.WallpaperFolder.ToString()]);

        public void Process(DirectoryInfo rootFolder, ref List<string> allWallpapers)
        {
            var NumberOfLevels = EveryoneUsesThese.GetNumberOfLevels(allWallpapers.Count);

            WallpaperMover.CreateChildFolders(rootFolder, allWallpapers, NumberOfLevels);

            WallpaperMover.PopulateChildFolders(rootFolder, allWallpapers, NumberOfLevels);

            EveryoneUsesThese.DeleteAllEmptyDirectories(rootFolder);
        }

        public static void RestoreFiles(string path)
        {
            UndoEverything.RestoreAllFilesToRootFolder(path, RootFolder);
            EveryoneUsesThese.DeleteAllEmptyDirectories(RootFolder);
        }
    }
}
