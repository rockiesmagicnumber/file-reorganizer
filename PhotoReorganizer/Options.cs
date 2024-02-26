namespace PhotoLibraryCleaner.Lib;

public class Options
{
    public DirectoryInfo RootDirectoryInfo { get; private set; }

    public bool IsReadOnly { get; private set; }

    public bool DeleteDuplicates { get; private set; }

    public Options(DirectoryInfo directoryInfo, bool isReadOnly = false, bool deleteDuplicates = false)
    {
        this.RootDirectoryInfo = directoryInfo;
        this.IsReadOnly = isReadOnly;
        this.DeleteDuplicates = deleteDuplicates;
    }
}
