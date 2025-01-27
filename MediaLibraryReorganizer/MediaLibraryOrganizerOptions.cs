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
        /// <param name="sourceDirectory">From whence the files come.</param>
        /// <param name="processedDirectory">Whereto the files shall go.</param>
        public MediaLibraryOrganizerOptions(
            DirectoryInfo sourceDirectory,
            DirectoryInfo processedDirectory)
        {
            this.SourceDirectoryInfo = sourceDirectory;
            Statics.SourceDirectory ??= this.SourceDirectoryInfo;

            if (processedDirectory != null)
            {
                Statics.OutputDirectory ??= processedDirectory;
            }
        }

        public DirectoryInfo SourceDirectoryInfo { get; set; }
    }
}
