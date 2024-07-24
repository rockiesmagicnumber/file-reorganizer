// <copyright file="Program.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System.IO;
    using SokkaCorp.MediaLibraryOrganizer.Lib;
    using Serilog;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .File(
                    Statics.GetLogFilePath(),
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo
                    .Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            // get args
            DirectoryInfo sourceDirectory = new DirectoryInfo(Assembly.GetEntryAssembly().Location);
            DirectoryInfo outputDirectory = null;
            if (args.Contains(Constants.ArgumentFlags.Source))
            {
                int sourceIndex = Array.IndexOf(args, Constants.ArgumentFlags.Source);
                sourceDirectory = Directory.CreateDirectory(args[sourceIndex + 1]);
            }

            if (args.Contains(Constants.ArgumentFlags.Output))
            {
                int outputIndex = Array.IndexOf(args, Constants.ArgumentFlags.Output);
                outputDirectory = Directory.CreateDirectory(args[outputIndex + 1]);
                Log.Information($"Output Directory: {outputDirectory}");
            }

            Log.Debug("Source Directory: {executionDirectory}", sourceDirectory.FullName);

            bool readOnly = false; // args.Contains("-ro") || args.Contains("--read-only");
            bool excludeDuplicates = args.Contains(Constants.ArgumentFlags.ExcludeDuplicates);
            MediaLibraryOrganizerOptions executionOptions = new MediaLibraryOrganizerOptions(sourceDirectory, readOnly, excludeDuplicates, outputDirectory);
            MediaLibraryOrganizer pr = new MediaLibraryOrganizer(executionOptions);
            JobReturn success;
            if (args.Contains(Constants.ArgumentFlags.RefreshJsonBackup))
            {
                success = pr.PruneJsonBackup();
            }
            else if (args.Contains(Constants.ArgumentFlags.RepopulateJsonBackup))
            {
                success = pr.RepopulateJsonBackup();
            }
            else
            {
                success = pr.OrganizeFiles();
            }

            Log.Information("Success: " + success.Success.ToString());
            Log.Information("\tHandled File Errors: " + success.HandledError?.InnerExceptions?.Count().ToString() ?? "0");
            Log.CloseAndFlush();
        }
    }
}