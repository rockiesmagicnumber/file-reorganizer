// <copyright file="OrganizerAbortedException.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;

    /// <summary>
    /// Raised when the user chooses to abort an organize run after a file error.
    /// </summary>
    public sealed class OrganizerAbortedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizerAbortedException"/> class.
        /// </summary>
        /// <param name="message">Reason shown in logs.</param>
        public OrganizerAbortedException(string message)
            : base(message)
        {
        }
    }
}
