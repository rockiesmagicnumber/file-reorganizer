// <copyright file="OrganizerOptions.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    /// <summary>
    /// What to do with a source file whose MD5 already exists in the manifest (requires <see cref="OrganizerOptions.TrackManifest"/>).
    /// </summary>
    public enum DuplicateDisposition
    {
        /// <summary>
        /// Leave the duplicate source file in place; do not import again.
        /// </summary>
        Skip,

        /// <summary>
        /// Delete the duplicate source file after detection (fails on read-only media).
        /// </summary>
        Delete,

        /// <summary>
        /// Move the duplicate into <c>{MediaLibrary}/SokkaCorp/Duplicates/</c> with the MD5 prefix on the filename.
        /// </summary>
        Quarantine,
    }

    /// <summary>
    /// Runtime options for organize / dedupe behavior.
    /// </summary>
    public sealed class OrganizerOptions
    {
        /// <summary>
        /// Gets a value indicating whether MD5 manifest, JSON persistence, and duplicate detection are enabled.
        /// </summary>
        public bool TrackManifest { get; set; }

        /// <summary>
        /// Gets or sets the action when a duplicate MD5 is seen (only when <see cref="TrackManifest"/> is true).
        /// </summary>
        public DuplicateDisposition OnDuplicate { get; set; } = DuplicateDisposition.Skip;

        /// <summary>
        /// When true, ZIP identity for skip-already-extracted uses MD5; when false, uses path/size/mtime (no per-zip hash).
        /// </summary>
        public bool FingerprintZipContents => this.TrackManifest;

        /// <summary>
        /// When true, always copy from source instead of moving (non-destructive on the source tree).
        /// </summary>
        public bool CopyOnly { get; set; }

        /// <summary>
        /// First zip nesting index at which expansion stops (outermost archives use index 0). Use 0 for no limit.
        /// </summary>
        public int MaxZipNestingDepth { get; set; } = 5;

        /// <summary>
        /// Comparison threshold for the current nesting depth: extraction is skipped when depth is greater than or equal to this value. <see cref="int.MaxValue"/> when <see cref="MaxZipNestingDepth"/> is 0 (unlimited).
        /// </summary>
        public int ZipNestingDepthCutoff =>
            this.MaxZipNestingDepth == 0 ? int.MaxValue : this.MaxZipNestingDepth;

        /// <summary>
        /// When true, file errors prompt the user (skip, error workflow, abort) instead of auto-quarantining.
        /// </summary>
        public bool Interactive { get; set; }

        /// <summary>
        /// Host-provided handler for interactive file errors. Required when <see cref="Interactive"/> is true.
        /// </summary>
        public IFileErrorHandler? ErrorHandler { get; set; }
    }
}
