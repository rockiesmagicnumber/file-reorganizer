// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System.IO;
    using SokkaCorp.MediaLibraryOrganizer.Lib;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    Statics.GetLogFilePath(),
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

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

            Log.Debug("Execution Directory: {executionDirectory}", executionDirectory);

            bool readOnly = false; // args.Contains("-ro") || args.Contains("--read-only");
            bool deleteDupes = false; // args.Contains("--delete-duplicates");
            MediaLibraryOrganizerOptions executionOptions = new MediaLibraryOrganizerOptions(executionDirectory, readOnly, deleteDupes);
            MediaLibraryOrganizer pr = new MediaLibraryOrganizer(executionOptions);
            JobReturn success = pr.OrganizePhotos();
            Log.Information("Success: " + success.Success.ToString());
            Log.Information("\tHandled File Errors: " + success.HandledError.InnerExceptions.Count.ToString());
        }
    }
}