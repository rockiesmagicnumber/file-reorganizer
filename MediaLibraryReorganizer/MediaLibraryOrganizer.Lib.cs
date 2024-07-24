// <copyright file="MediaLibraryOrganizer.Lib.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System.Text.Json;
    using MDE = MetadataExtractor;
    using Serilog;

    public class MediaLibraryOrganizer
    {
        private FileDictionary<NString, List<FileInfo>> processedFiles;

        private MediaLibraryOrganizerOptions Options { get; set; }

        private List<Exception> Errors { get; set; }

        public MediaLibraryOrganizer(MediaLibraryOrganizerOptions options)
        {
            this.Options = options;
            this.processedFiles = [];
            this.Errors = [];

            if (File.Exists(Statics.GetJsonBackup().FullName))
            {
                string jsonString = File.ReadAllText(Statics.GetJsonBackup().FullName);
                if (!string.IsNullOrEmpty(jsonString))
                {
                    this.processedFiles = JsonSerializer.Deserialize<FileDictionary<NString, List<FileInfo>>>(jsonString);
                }
            }
        }

        public JobReturn PruneJsonBackup()
        {
            StaticLog.Enter(nameof(this.PruneJsonBackup));
            var jr = new JobReturn() { Success = true };
            try
            {
                for (int i = 0; i < this.processedFiles.Count; i++)
                {
                    var file = this.processedFiles.ElementAt(i);
                    if (!file.Value.FirstOrDefault()?.Exists ?? true)
                    {
                        this.processedFiles.Remove(file.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ah jeez");
                jr.HandledError = new AggregateException("error processing dictionary", ex);
                jr.Success = false;
            }
            finally
            {
                this.WriteJsonBackup();
                StaticLog.Exit(nameof(this.PruneJsonBackup));
            }

            return jr;
        }

        public JobReturn RepopulateJsonBackup()
        {
            StaticLog.Enter(nameof(this.RepopulateJsonBackup));
            var jobReturn = new JobReturn() { Success = true };
            try
            {
                this.processedFiles = new FileDictionary<NString, List<FileInfo>>();
                var allThemFiles = Statics.GetProcessedDirectory().EnumerateFiles("*.*", SearchOption.AllDirectories);
                foreach (var thatFile in allThemFiles)
                {
                    try
                    {
                        this.ProcessFile(thatFile);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Ah jeez");
                        this.Errors.Add(ex);
                    }
                }

                jobReturn.Success = true;
                jobReturn.HandledError = new AggregateException(this.Errors);
                this.Cleanup();
                return jobReturn;
            }
            finally
            {
                this.WriteJsonBackup();
                StaticLog.Exit(nameof(this.RepopulateJsonBackup));
            }
        }

        public JobReturn OrganizeFiles()
        {
            StaticLog.Enter(nameof(this.OrganizeFiles));
            try
            {
                var jobReturn = new JobReturn();

                this.UnzipAllRecursively(this.Options.SourceDirectoryInfo);
                this.ProcessDirectory(this.Options.SourceDirectoryInfo);
                jobReturn.Success = true;
                jobReturn.HandledError = new AggregateException(this.Errors);
                this.Cleanup();
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
                this.WriteJsonBackup();
                StaticLog.Exit(nameof(this.OrganizeFiles));
            }
        }

        private void WriteJsonBackup()
        {
            string jsArchive = JsonSerializer.Serialize(this.processedFiles);
            File.WriteAllText(Statics.GetJsonBackup().FullName, jsArchive);
        }

        private void Cleanup()
        {
            System.IO.Directory.Delete(Statics.GetUnzippedDirectory(this.Options).FullName, true);
        }

        private void ProcessDirectory(DirectoryInfo directory)
        {
            StaticLog.Enter($"{nameof(this.ProcessDirectory)} - {directory.FullName}");
            try
            {
                var childDirectories = directory.EnumerateDirectories();
                var childFiles = directory.EnumerateFiles();

                // process all child directories
                foreach (var child in childDirectories)
                {
                    this.ProcessDirectory(child);
                }

                // process all files
                int cnt = 0;
                foreach (var cf in childFiles)
                {
                    cnt++;
                    try
                    {
                        Log.Information($"Processing {cnt}/{childFiles.Count()}");
                        this.ProcessFile(cf);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error processing file {cf}", ex);
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

        private void ProcessFile(FileInfo fileInfo)
        {
            StaticLog.Enter($"{nameof(this.ProcessFile)} - {fileInfo.FullName}");
            try
            {
                string fileSHAchksum = Statics.GetChecksum(fileInfo);

                // check if our dictionary already has that checksum
                if (this.processedFiles.TryGetValue(fileSHAchksum, out var files) && files is not null)
                {
                    if (files.Contains(fileInfo))
                    {
                        Log.Information($"File already processed, skipping | {fileInfo}");
                        return;
                    }

                    if (this.Options.ExcludeDuplicates)
                    {
                        Log.Information($"Hash already processed, skipping | {fileInfo} | {fileSHAchksum}");
                        return;
                    }
                }
                else
                {
                    this.processedFiles[fileSHAchksum] = new List<FileInfo>();
                }

                if (fileInfo.IsZip())
                {
                    return;
                }

                FileInfo destinationPath = this.GetDestinationFile(fileInfo);
                destinationPath = this.CreateDestinationFile(fileInfo, destinationPath);

                this.processedFiles[fileSHAchksum].Add(destinationPath);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessFile));
            }
        }

        private FileInfo CreateDestinationFile(FileInfo fileInfo, FileInfo destinationPath)
        {
            if (!this.Options.ExcludeDuplicates)
            {
                if (destinationPath.Exists)
                {
                    do
                    {
                        string newFilename = Path.Combine(
                            destinationPath.DirectoryName,
                            Path.GetFileNameWithoutExtension(destinationPath.FullName),
                            Guid.NewGuid().ToString(),
                            destinationPath.Extension);
                        destinationPath = new FileInfo(newFilename);
                    }
                    while (destinationPath.Exists);
                }
            }

            Log.Information($"Copying {fileInfo.FullName} => {destinationPath.FullName}");

            if (!destinationPath.Exists)
            {
                destinationPath = fileInfo.CopyTo(destinationPath.FullName);
            }

            return destinationPath;
        }

        private FileInfo GetDestinationFile(FileInfo fileInfo)
        {
            FileInfo destinationPath;
            if (fileInfo.IsPhoto())
            {
                destinationPath = this.ProcessPhoto(fileInfo);
            }
            else if (fileInfo.IsVideo())
            {
                destinationPath = this.ProcessVideo(fileInfo);
            }
            // else if (file.IsMusic())
            // {
            //     //     this.ProcessMusic(file);
            // }
            else
            {
                destinationPath = this.ProcessMiscFile(fileInfo);
            }

            return destinationPath;
        }

        private FileInfo ProcessErrorFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessErrorFile));
            try
            {
                DirectoryInfo errorDir = Statics.GetErrorMiscDirectory();
                if (file.IsPhoto())
                {
                    errorDir = Statics.GetErrorPhotoDirectory();
                }
                else if (file.IsVideo())
                {
                    errorDir = Statics.GetErrorVideoDirectory();
                }

                return new FileInfo(Path.Combine(errorDir.FullName, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessErrorFile));
            }
        }

        private FileInfo ProcessPhoto(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessPhoto));
            try
            {
                // capture our file as a TagLib Image
                TagLib.File tagfile = TagLib.File.Create(file.FullName);
                DateTime photoDt;

                // attempt to get photo datetime from metadata
                if (tagfile is TagLib.Image.File image && (image?.ImageTag?.DateTime ?? default) != default)
                {
                    photoDt = image.ImageTag.DateTime.Value;
                }
                else if ((tagfile.Tag.DateTagged ?? default) != default)
                {
                    photoDt = tagfile.Tag.DateTagged.Value;
                }
                else
                {
                    photoDt = file.CreationTime;
                }

                // get datetime directory
                return new FileInfo(Path.Combine(Statics.GetPhotoDirectoryFromDateTime(photoDt).FullName, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessPhoto));
            }
        }

        private FileInfo ProcessVideo(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessVideo));
            try
            {
                TagLib.File tagfile = null;

                // capture our file as a TagLib Image
                try
                {
                    tagfile = TagLib.File.Create(file.FullName);
                }

                // we're not actually gonna handle the error, TagLib is just gonna fail on .MOV files.
                catch
                {
                    var dir = MDE.ImageMetadataReader.ReadMetadata(file.FullName);
                }

                // attempt to get photo datetime from metadata
                DateTime videoDt;
                if (file is not null && (tagfile.Tag?.DateTagged ?? default) != default)
                {
                    videoDt = tagfile.Tag.DateTagged.Value;
                }
                else
                {
                    videoDt = file.CreationTime;
                }

                // get datetime directory
                return new FileInfo(Path.Combine(Statics.GetVideoDirectoryFromDateTime(videoDt).FullName, file.Name));
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessVideo));
            }
        }

        private FileInfo ProcessMiscFile(FileInfo file)
        {
            StaticLog.Enter(nameof(this.ProcessMiscFile));
            try
            {
                var destinationDirectory = Statics.GetMiscDirectoryFromDateTime(file.CreationTime);
                string destFileName = Path.Combine(destinationDirectory.FullName, file.Name);
                return new FileInfo(destFileName);
            }
            finally
            {
                StaticLog.Exit(nameof(this.ProcessMiscFile));
            }
        }

        private void UnzipAllRecursively(DirectoryInfo directory)
        {
            StaticLog.Enter(nameof(this.UnzipAllRecursively));
            try
            {
                var allZips = directory.EnumerateFiles("*.zip", new EnumerationOptions() { RecurseSubdirectories = true });
                foreach (var zip in allZips)
                {
                    try
                    {
                        Log.Information($"Unzipping {zip.FullName}");
                        Statics.UnZip(zip, this.Options);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error unzipping " + zip.FullName, ex);
                        this.ProcessErrorFile(zip);
                    }
                }

                var newZips = Statics.GetUnzippedDirectory(this.Options).EnumerateFiles("*.zip", new EnumerationOptions { RecurseSubdirectories = true });
                while (newZips.Any())
                {
                    var newZip = newZips.First();
                    Statics.UnZip(newZip, this.Options);
                    newZip.Delete();
                }
            }
            finally
            {
                StaticLog.Exit(nameof(this.UnzipAllRecursively));
            }
        }

        // public FileInfo ProcessMusicFile(string file)
        // {

        // }
    }
}