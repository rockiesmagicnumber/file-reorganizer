using CommandLine;
using System.IO;
using PhotoLibraryCleaner.Lib;

namespace PhotoLibraryCleaner;

class Program
{
    static void Main(string[] args)
    {
        string executionDirectoryStr = args[0];
        DirectoryInfo executionDirectory;

        if (string.IsNullOrEmpty(executionDirectoryStr))
        {
            executionDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        }
        else if (Directory.Exists(executionDirectoryStr))
        {
            executionDirectory = new DirectoryInfo(executionDirectoryStr);
        }
        else
        {
            throw new ArgumentNullException("Must specify an existing directory", null as Exception);
        }

        bool readOnly = args.Contains("-ro") || args.Contains("--read-only");
        bool deleteDupes = args.Contains("--delete-duplicates");

        var executionOptions = new Options(executionDirectory, readOnly, deleteDupes);
        PhotoReorganizer pr = new PhotoReorganizer(executionOptions);
        var success = pr.OrganizePhotos();
        if (success)
        {
            Console.WriteLine("wooo it worked");
        }
        else
        {
            Console.Error.WriteLine(success.Error);
        }
    }
}

