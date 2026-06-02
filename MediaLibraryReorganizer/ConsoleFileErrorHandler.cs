// <copyright file="ConsoleFileErrorHandler.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer
{
    using System;
    using System.IO;
    using SokkaCorp.MediaLibraryOrganizer.Lib;

    /// <summary>
    /// Console prompts for per-file import failures.
    /// </summary>
    public sealed class ConsoleFileErrorHandler : IFileErrorHandler
    {
        private FileErrorDisposition? applyToAll;

        /// <inheritdoc/>
        public FileErrorDisposition Resolve(FileInfo file, Exception exception)
        {
            if (this.applyToAll.HasValue)
            {
                return this.applyToAll.Value;
            }

            Console.WriteLine();
            Console.WriteLine("Import failed:");
            Console.WriteLine($"  File: {file.FullName}");
            Console.WriteLine($"  Error: {exception.GetType().Name}: {TruncateMessage(exception.Message, 240)}");
            Console.WriteLine();
            Console.WriteLine("  [S] Skip — leave source in place");
            Console.WriteLine("  [E] Error workflow — copy to MediaLibrary/SokkaCorp/Errors/");
            Console.WriteLine("  [A] Abort — stop this run");
            Console.Write("Choice [S/e/a]: ");

            FileErrorDisposition choice = ReadDispositionChoice(Console.ReadLine());
            Console.Write("Apply this choice to all remaining errors this run? [y/N]: ");
            if (IsYes(Console.ReadLine()))
            {
                this.applyToAll = choice;
            }

            return choice;
        }

        private static FileErrorDisposition ReadDispositionChoice(string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return FileErrorDisposition.Skip;
            }

            switch (line.Trim()[0])
            {
                case 'e':
                case 'E':
                    return FileErrorDisposition.ErrorWorkflow;
                case 'a':
                case 'A':
                    return FileErrorDisposition.Abort;
                default:
                    return FileErrorDisposition.Skip;
            }
        }

        private static bool IsYes(string? line)
        {
            return !string.IsNullOrWhiteSpace(line)
                && (line.Trim()[0] == 'y' || line.Trim()[0] == 'Y');
        }

        private static string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message) || message.Length <= maxLength)
            {
                return message;
            }

            return message.Substring(0, maxLength) + "...";
        }
    }
}
