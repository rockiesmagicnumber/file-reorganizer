// <copyright file="Options.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public class MediaLibraryOrganizerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLibraryOrganizerOptions"/> class.
        /// </summary>
        /// <param name="originDirectory"></param>
        /// <param name="isReadOnly"></param>
        /// <param name="excludeDuplicates"></param>
        /// <param name="processedDirectory"></param>
        public MediaLibraryOrganizerOptions(
            DirectoryInfo originDirectory,
            bool isReadOnly,
            bool excludeDuplicates,
            DirectoryInfo processedDirectory)
        {
            this.SourceDirectoryInfo = originDirectory;
            Statics.SourceDirectory ??= this.SourceDirectoryInfo;

            this.IsReadOnly = isReadOnly;
            this.ExcludeDuplicates = excludeDuplicates;
            if (processedDirectory != null)
            {
                Statics.OutputDirectory ??= processedDirectory;
            }
        }

        public DirectoryInfo SourceDirectoryInfo { get; set; }

        public bool IsReadOnly { get; set; } = false;

        public bool ExcludeDuplicates { get; set; } = false;
    }
}
