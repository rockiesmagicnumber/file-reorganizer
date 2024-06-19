// <copyright file="Options.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

using Serilog;

namespace PhotoLibraryCleaner.Lib
{
    public record PhotoReorganizerOptions(DirectoryInfo rootDirectoryInfo, bool isReadOnly = false, bool deleteDuplicates = false)
    {
        public DirectoryInfo RootDirectoryInfo { get; private set; } = rootDirectoryInfo;
        public bool IsReadOnly { get; private set; } = isReadOnly;
        public bool DeleteDuplicates { get; private set; } = deleteDuplicates;
    }
}
