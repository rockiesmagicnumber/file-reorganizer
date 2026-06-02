// <copyright file="FileErrorDisposition.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    /// <summary>
    /// User-selected action when a file fails to import (interactive mode).
    /// </summary>
    public enum FileErrorDisposition
    {
        /// <summary>
        /// Leave the source file in place and continue.
        /// </summary>
        Skip,

        /// <summary>
        /// Copy the source into the SokkaCorp/Errors/ tree (existing workflow).
        /// </summary>
        ErrorWorkflow,

        /// <summary>
        /// Stop the run immediately.
        /// </summary>
        Abort,
    }
}
