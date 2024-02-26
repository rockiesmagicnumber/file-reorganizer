using System.IO.Compression;

namespace PhotoLibraryCleaner.Lib;

public class PhotoReorganizer
{
    private FileDictionary<NString, List<string>> ProcessedFiles;
    private Options Options { get; set; }

    public PhotoReorganizer(Options options)
    {
        this.Options = options;
        this.ProcessedFiles = [];
    }

    public JobReturn OrganizePhotos()
    {
        JobReturn jobReturn = new();
        try
        {
            this.ProcessDirectory(Options.RootDirectoryInfo);
            jobReturn.Success = true;
        }
        catch (Exception ex)
        {
            jobReturn.Error = ex;
            jobReturn.Success = false;
        }
        return jobReturn;
    }

    private void ProcessDirectory(DirectoryInfo directory)
    {
        var childDirectories = directory.EnumerateDirectories();
        var childFiles = directory.EnumerateFiles().Select(x => x.FullName);

        // Extract all Zip files first so we can process their child directories
        foreach (var cf in childFiles.Where(c => c.IsZip()))
        {
            this.ProcessZip(cf);
        }

        // process all child directories
        for (int i = 0; i < childDirectories.Count(); i++)
        {
            var child = childDirectories.ElementAt(i);
            this.ProcessDirectory(child);
        }

        // process all NOT-zip files
        for (int i = 0; i > childFiles.Where(c => !c.IsZip()).Count(); i++)
        {
            var file = childFiles.ElementAt(i);
            this.ProcessFile(file);
        }
    }

    private void ProcessFile(string filePath)
    {
        string fileSHAchksum = string.Empty;

        // get file checksum
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                var hash = sha.ComputeHash(stream);
                fileSHAchksum = BitConverter.ToString(hash).Replace("-", "");
            }
        }

        // check if our dictionary already has that checksum
        if (ProcessedFiles.ContainsKey(fileSHAchksum))
        {
            if (this.Options.DeleteDuplicates)
            {
                File.Delete(filePath);
                return;
            }
        }
        else
        {
            ProcessedFiles[fileSHAchksum] = new List<string>();
        }

        if (filePath.IsPhoto())
        {
            ProcessPhoto(filePath);
        }
        else if (filePath.IsZip())
        {
            ProcessZip(filePath);
        }
        else
        {
            ProcessMiscFile(filePath);
        }

        this.ProcessedFiles[fileSHAchksum].Add(filePath);
    }

    private string ProcessPhoto(string filePath)
    {
        var file = TagLib.File.Create(filePath);
        var image = file as TagLib.Image.File;
        string destinationDirectory = string.Empty;
        if (image?.ImageTag?.DateTime.HasValue ?? false)
        {
            // attempt to 
            DateTime photoDt = image.ImageTag.DateTime.Value;
            destinationDirectory = this.GetDirectoryFromDateTime(photoDt);
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(filePath, Path.Combine(destinationDirectory, Path.GetFileName(filePath)));
            return destinationDirectory;
        }
        return ProcessMiscFile(filePath);
    }

    private void ProcessZip(string filePath)
    {
        string dname = Path.GetDirectoryName(filePath) ?? filePath.Replace(Path.GetFileNameWithoutExtension(filePath), string.Empty);
        string newDname = Path.Combine(dname, Path.GetFileNameWithoutExtension(filePath));
        System.IO.Directory.CreateDirectory(newDname);
        ZipFile.ExtractToDirectory(filePath, dname);
    }

    private string ProcessMiscFile(string filePath)
    {
        FileInfo file1 = new FileInfo(filePath);
        string destinationDirectory = this.GetDirectoryFromDateTime(file1.CreationTime);
        Directory.CreateDirectory(destinationDirectory);
        File.Copy(filePath, Path.Combine(destinationDirectory, Path.GetFileName(filePath)));
        return destinationDirectory;
    }

    private string GetDirectoryFromDateTime(DateTime? dt)
    {
        if (!dt.HasValue)
        {
            return string.Empty;
        }
        else
        {
            DateTime fileDt = dt.Value;
            return Path.Combine(this.Options.RootDirectoryInfo.FullName, fileDt.Year.ToString("0000"), fileDt.Month.ToString("00"), fileDt.Day.ToString("00"));
        }
    }
}
