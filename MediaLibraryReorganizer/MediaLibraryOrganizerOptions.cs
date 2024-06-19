// <copyright file="Options.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    public record MediaLibraryOrganizerOptions(DirectoryInfo rootDirectoryInfo, bool isReadOnly = false, bool deleteDuplicates = false)
    {
        public DirectoryInfo RootDirectoryInfo { get; private set; } = rootDirectoryInfo;
        public bool IsReadOnly { get; private set; } = isReadOnly;
        public bool DeleteDuplicates { get; private set; } = deleteDuplicates;
    }
}
