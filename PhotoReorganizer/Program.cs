// <copyright file="Program.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner
{
    using System.IO;
    using PhotoLibraryCleaner.Lib;

    public class Program
    {
        public static void Main(string[] args)
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

            bool readOnly = false;// args.Contains("-ro") || args.Contains("--read-only");
            bool deleteDupes = false;// args.Contains("--delete-duplicates");

            PhotoReorganizerOptions executionOptions = new PhotoReorganizerOptions(executionDirectory, readOnly, deleteDupes);
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
}