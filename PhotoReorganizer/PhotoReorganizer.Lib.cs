// <copyright file="PhotoReorganizer.Lib.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    using System.Text.Json;
    using Serilog;

    public class PhotoReorganizer
    {
        private FileDictionary<NString, List<string>> processedFiles;

        private PhotoReorganizerOptions Options { get; set; }

        private List<Exception> Errors { get; set; }

        public PhotoReorganizer(PhotoReorganizerOptions options)
        {
            this.Options = options;
            this.processedFiles = [];
            this.Errors = [];

            // if (Directory.Exists(Path.Combine(this.Options.RootDirectoryInfo.FullName, Constants.RuntimeDirectories.ProcessedDirectoryName))
            //     && File.Exists(Path.Combine(this.Options.RootDirectoryInfo.FullName, "jsonBackup.json")))
            // {
            //     string jsonString = File.ReadAllText(Path.Combine(this.Options.RootDirectoryInfo.FullName, "jsonBackup.json"));
            //     this.ProcessedFiles = JsonSerializer.Deserialize<FileDictionary<NString, List<string>>>(jsonString);
            // }
        }

        public JobReturn OrganizePhotos()
        {
            StaticLog.Enter(nameof(this.OrganizePhotos));
            try
            {
                var jobReturn = new JobReturn();
                var everything = Directory.GetFiles(this.Options.RootDirectoryInfo.FullName, "*.*", SearchOption.AllDirectories).ToList();
                foreach (var currentPath in everything)
                {
                    string destPath = currentPath.Replace(this.Options.RootDirectoryInfo.FullName, Statics.GetOriginalDirectory().FullName);
                    try
                    {
                        _ = Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(currentPath, destPath);
                    }
                    catch (Exception ex)
                    {
                        this.Errors.Add(ex);
                        this.ProcessErrorFile(currentPath);
                    }
                }

                this.ProcessDirectory(Statics.GetOriginalDirectory());
                jobReturn.Success = true;
                jobReturn.HandledError = new AggregateException(this.Errors);
                string jsArchive = JsonSerializer.Serialize(this.processedFiles);
                // File.WriteAllText(Path.Combine(this.Options.RootDirectoryInfo.FullName, "jsonBackup.json"), jsArchive);
                return jobReturn;
            }
            catch (Exception ex)
            {
                Log.Error("Error", ex);
                this.Errors.Add(ex);
                AggregateException exx = new AggregateException(this.Errors);
                return new JobReturn()
                {
                    HandledError = exx,
                    Success = false,
                };
            }
            finally
            {
                StaticLog.Exit(nameof(this.OrganizePhotos));
            }
        }

        private void ProcessDirectory(DirectoryInfo directory)
        {
            StaticLog.Enter(nameof(this.ProcessDirectory));
            try
            {
                // TODO WHAT IF THERE IS A DIRECTORY CALLED PROCESSED ALREADY
                var childDirectories = directory.EnumerateDirectories().ToList();
                var childFiles = directory.EnumerateFiles().Select(x => x.FullName).ToList();

                // Extract all Zip files first so we can process their child directories
                foreach (var cf in childFiles.Where(c => c.EndsWith(Constants.FileExtensions.Zip)))
                {
                    Statics.UnZip(cf);
                }

                // process all child directories
                foreach (var child in childDirectories)
                {
                    this.ProcessDirectory(child);
                }

                // process all NOT-zip files
                foreach (var cf in childFiles.Where(c => !c.EndsWith(Constants.FileExtensions.Zip)))
                {
                    try
                    {
                        this.ProcessFile(cf);
                    }
                    catch (Exception ex)
                    {
                        this.Errors.Add(ex);
                        this.ProcessErrorFile(cf);
                    }
                }
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessDirectory));
            }
        }

        private void ProcessFile(string filePath)
        {
            StaticLog.Enter(nameof(this.ProcessFile));
            try
            {
                string fileSHAchksum = string.Empty;

                // get file checksum
                // using (var sha = System.Security.Cryptography.SHA256.Create())
                // {
                //     using (var stream = File.OpenRead(filePath))
                //     {
                //         var hash = sha.ComputeHash(stream);
                //         fileSHAchksum = BitConverter.ToString(hash).Replace("-", string.Empty);
                //     }
                // }

                // check if our dictionary already has that checksum
                // if (this.processedFiles.TryGetValue(fileSHAchksum, out _))
                // {
                //     if (this.Options.DeleteDuplicates)
                //     {
                //         File.Delete(filePath);
                //         return;
                //     }
                // }
                // else
                // {
                //     this.processedFiles[fileSHAchksum] = new List<string>();
                // }

                if (filePath.IsZip())
                {
                    Statics.UnZip(filePath);
                }
                else if (filePath.IsPhoto())
                {
                    this.ProcessPhoto(filePath);
                }
                else if (filePath.IsVideo())
                {
                    this.ProcessVideo(filePath);
                }
                else if (filePath.IsMusic())
                {
                    //     this.ProcessMusic(filePath);
                }
                else
                {
                    this.ProcessMiscFile(filePath);
                }

                this.processedFiles[fileSHAchksum].Add(filePath);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        public string ProcessErrorFile(string filePath)
        {
            StaticLog.Enter(nameof(this.ProcessFile));
            try
            {
                DirectoryInfo errorDir = Statics.GetErrorMiscDirectory();
                if (filePath.IsPhoto())
                {
                    errorDir = Statics.GetErrorPhotoDirectory();
                }
                else if (filePath.IsVideo())
                {
                    errorDir = Statics.GetErrorVideoDirectory();
                }

                string destFileName = Path.Combine(errorDir.FullName, Path.GetFileName(filePath));
                if (File.Exists(destFileName))
                {
                    int existing = 0;
                    do
                    {
                        existing++;
                        destFileName = destFileName.Replace(Path.GetFileNameWithoutExtension(destFileName), Path.GetFileNameWithoutExtension(destFileName) + existing.ToString());
                    }
                    while (File.Exists(destFileName));
                }

                File.Copy(filePath, destFileName);
                return destFileName;
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        public FileInfo ProcessPhoto(string filePath)
        {
            StaticLog.Enter(nameof(this.ProcessFile));
            try
            {
                // capture our filepath as a TagLib Image
                TagLib.File file = TagLib.File.Create(filePath);
                DateTime photoDt;
                // attempt to get photo datetime from metadata
                if (file is TagLib.Image.File image && (image?.ImageTag?.DateTime ?? default) != default)
                {
                    photoDt = image.ImageTag.DateTime.Value;
                }
                else if ((file.Tag.DateTagged ?? default) != default)
                {
                    photoDt = file.Tag.DateTagged.Value;
                }
                else
                {
                    FileInfo fi = new FileInfo(filePath);
                    photoDt = fi.CreationTime;
                }

                // get datetime directory
                var destinationDirectory = Statics.GetPhotoDirectoryFromDateTime(photoDt);
                string destinationFilePath = Path.Combine(destinationDirectory.FullName, Path.GetFileName(filePath));

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
                return new FileInfo(destinationFilePath);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        public FileInfo ProcessVideo(string filePath)
        {
            StaticLog.Enter(nameof(this.ProcessFile));
            try
            {
                // capture our filepath as a TagLib Image
                TagLib.File file = TagLib.File.Create(filePath);

                // attempt to get photo datetime from metadata
                DateTime videoDt;
                if (file is not null && (file.Tag?.DateTagged ?? default) != default)
                {
                    videoDt = file.Tag.DateTagged.Value;
                }
                else
                {
                    FileInfo fi = new FileInfo(filePath);
                    videoDt = fi.CreationTime;
                }

                // get datetime directory
                var destinationDirectory = Statics.GetVideoDirectoryFromDateTime(videoDt);
                string destinationFilePath = Path.Combine(destinationDirectory.FullName, Path.GetFileName(filePath));

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
                return new FileInfo(destinationFilePath);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        public FileInfo ProcessMiscFile(string filePath)
        {
            StaticLog.Enter(nameof(this.ProcessFile));
            try
            {
                FileInfo file1 = new FileInfo(filePath);
                var destinationDirectory = Statics.GetMiscDirectoryFromDateTime(file1.CreationTime);
                string destFileName = Path.Combine(destinationDirectory.FullName, Path.GetFileName(filePath));
                if (File.Exists(destFileName))
                {
                    destFileName = Path.Combine(destinationDirectory.FullName, Path.GetFileNameWithoutExtension(filePath) + Guid.NewGuid().ToString() + Path.GetExtension(filePath));
                }

                File.Copy(filePath, destFileName);
                return new FileInfo(destFileName);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        // public FileInfo ProcessMusicFile(string filePath)
        // {

        // }
    }
}