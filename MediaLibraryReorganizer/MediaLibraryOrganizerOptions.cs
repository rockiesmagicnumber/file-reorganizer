// <copyright file="Options.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public class MediaLibraryOrganizerOptions
    {
        public MediaLibraryOrganizerOptions(DirectoryInfo originDirectory, bool isReadOnly, bool deleteDuplicates, DirectoryInfo processedDirectory = null)
        {
            this.RootDirectoryInfo = originDirectory;
            this.IsReadOnly = isReadOnly;
            this.DeleteDuplicates = deleteDuplicates;
            if (processedDirectory != null)
            {
                this.ProcessedDirectoryInfo = processedDirectory;
            }
        }

        public DirectoryInfo RootDirectoryInfo { get; set; }

        public bool IsReadOnly { get; set; } = false;

        public bool DeleteDuplicates { get; set; } = false;

        public DirectoryInfo ProcessedDirectoryInfo { get; set; } = Statics.GetProcessedDirectory();
    }
}
