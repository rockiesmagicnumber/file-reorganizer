using System.IO;

namespace WallpaperReorganizer
{
    public class UndoEverything
    {
        public static void RestoreAllFilesToRootFolder(string path, DirectoryInfo RootFolder)
        {
            var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                if (!File.Exists(Path.Combine(RootFolder.FullName, Path.GetFileName(file))))
                {
                    File.Move(file, Path.Combine(RootFolder.FullName, Path.GetFileName(file)));
                }
            }
        }
    }
}
