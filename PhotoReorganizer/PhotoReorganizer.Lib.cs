// <copyright file="PhotoReorganizer.Lib.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    using System.IO.Compression;

    public class PhotoReorganizer
    {
        private FileDictionary<NString, List<string>> ProcessedFiles;

        private PhotoReorganizerOptions Options { get; set; }

        public PhotoReorganizer(PhotoReorganizerOptions options)
        {
            this.Options = options;
            this.ProcessedFiles = [];
        }

        public JobReturn OrganizePhotos()
        {
            JobReturn jobReturn = new ();
            try
            {
                this.ProcessDirectory(this.Options.RootDirectoryInfo);
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
                    fileSHAchksum = BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }

            // check if our dictionary already has that checksum
            if (this.ProcessedFiles.ContainsKey(fileSHAchksum))
            {
                if (this.Options.DeleteDuplicates)
                {
                    File.Delete(filePath);
                    return;
                }
            }
            else
            {
                this.ProcessedFiles[fileSHAchksum] = new List<string>();
            }

            if (filePath.IsPhoto())
            {
                this.ProcessPhoto(filePath);
            }
            else if (filePath.IsZip())
            {
                this.ProcessZip(filePath);
            }
            else
            {
                this.ProcessMiscFile(filePath);
            }

            this.ProcessedFiles[fileSHAchksum].Add(filePath);
        }

        private string ProcessPhoto(string filePath)
        {
            var file = TagLib.File.Create(filePath);
            var image = file as TagLib.Image.File;

            // attempt to get photo datetime from metadata
            if ((image?.ImageTag?.DateTime.HasValue ?? false) && image.ImageTag.DateTime.Value is DateTime dtt && dtt != default)
            {
                string destinationDirectory = string.Empty;

                // attempt to pull photo metadata - the date the photo was taken
                DateTime photoDt = image.ImageTag.DateTime.Value;

                // get datetime directory
                destinationDirectory = this.GetDirectoryFromDateTime(photoDt);
                Directory.CreateDirectory(destinationDirectory);
                string destinationFilePath = Path.Combine(destinationDirectory, Path.GetFileName(filePath));

                // check for duplicates, rename file if they exist
                if (File.Exists(destinationFilePath))
                {
                    int existing = 0;
                    do
                    {
                        existing++;
                        destinationFilePath = destinationFilePath.Replace(Path.GetFileNameWithoutExtension(destinationFilePath), Path.GetFileNameWithoutExtension(destinationFilePath) + existing.ToString());
                    }
                    while (File.Exists(destinationFilePath));
                }

                // copy file to new destination
                File.Copy(filePath, destinationFilePath);
                return destinationDirectory;
            }

            // resort to the actual file creation date
            return this.ProcessMiscFile(filePath);
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
}