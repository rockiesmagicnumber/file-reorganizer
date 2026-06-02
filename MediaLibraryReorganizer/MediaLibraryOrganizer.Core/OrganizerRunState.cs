// <copyright file="OrganizerRunState.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    /// <summary>
    /// Mutable run outcome shared across organizer components.
    /// </summary>
    public sealed class OrganizerRunState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user aborted the run.
        /// </summary>
        public bool Aborted { get; set; }
    }
}
