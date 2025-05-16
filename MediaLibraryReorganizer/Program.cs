// <copyright file="Program.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
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
            // Setup logging first
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
    }
}