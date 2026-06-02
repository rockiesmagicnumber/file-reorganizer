// <copyright file="Program.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
    using System.IO;
    using CommandLine;
    using Serilog;
    using SokkaCorp.MediaLibraryOrganizer.Lib;

    /// <summary>
    /// Entry point class for the media library organizer.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 for success, 1 for failure.</returns>
        public static int Main(string[] args)
        {
            PrimeOutputDirectoryForLogging(args);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    Statics.GetLogFilePath(),
                    rollingInterval: RollingInterval.Infinite,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                return Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .MapResult(
                        opts =>
                        {
                            try
                            {
                                opts.Execute();
                                return 0;
                            }
                            catch (Exception ex)
                            {
                                Log.Fatal(ex, "An error occurred while executing the command.");
                                return 1;
                            }
                        },
                        errs =>
                        {
                            Log.Error("Invalid command line arguments.");
                            return 1;
                        });
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occurred.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Sets <see cref="Statics.OutputDirectory"/> from <c>-o</c> / <c>--output</c> so the file log lands under the intended library root.
        /// </summary>
        /// <param name="args">Raw command-line arguments.</param>
        private static void PrimeOutputDirectoryForLogging(string[] args)
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a == "-o" || a == "--output")
                {
                    if (i + 1 < args.Length)
                    {
                        dir = args[i + 1];
                    }
                }
                else if (a.StartsWith("--output=", StringComparison.Ordinal))
                {
                    dir = a.Substring("--output=".Length);
                }
            }

            string mediaLibrary = Path.Combine(dir, Constants.RuntimeDirectories.MediaLibraryDirectoryName);
            Statics.OutputDirectory = new DirectoryInfo(mediaLibrary);
        }
    }
}
