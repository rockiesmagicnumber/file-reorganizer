// <copyright file="IFileErrorHandler.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.IO;

    /// <summary>
    /// Resolves how to handle a per-file processing failure (host implements console prompts).
    /// </summary>
    public interface IFileErrorHandler
    {
        /// <summary>
        /// Prompts or applies a stored default for a failed file import.
        /// </summary>
        /// <param name="file">The source file that failed.</param>
        /// <param name="exception">The exception raised during processing.</param>
        /// <returns>Skip, error workflow, or abort the run.</returns>
        FileErrorDisposition Resolve(FileInfo file, Exception exception);
    }
}
