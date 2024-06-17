// <copyright file="Program.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner
{
    using System.IO;
    using System.Security.Cryptography;
    using log4net;
    using log4net.Config;
    using PhotoLibraryCleaner.Lib;

    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            // Initialize Logging
            using (Stream fs = File.OpenRead("log4net.config"))
            {
                XmlConfigurator.Configure(fs);
            }

            // get input argument of the execution directory
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
                Log.Error("Must specify an existing directory");
                throw new ArgumentNullException("Must specify an existing directory", null as Exception);
            }

            Log.DebugFormat("Execution Directory: {0}", executionDirectory);

            bool readOnly = false; // args.Contains("-ro") || args.Contains("--read-only");
            bool deleteDupes = false; // args.Contains("--delete-duplicates");
            PhotoReorganizerOptions executionOptions = new PhotoReorganizerOptions(executionDirectory, readOnly, deleteDupes);
            PhotoReorganizer pr = new PhotoReorganizer(executionOptions);
            JobReturn success = pr.OrganizePhotos();
            Console.WriteLine("Success: " + success.Success.ToString());
            Console.WriteLine("\tHandled File Errors: " + success.HandledError.InnerExceptions.Count.ToString());
        }
    }
}