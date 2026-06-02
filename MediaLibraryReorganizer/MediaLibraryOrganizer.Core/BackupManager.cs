// <copyright file="BackupManager.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using Serilog;

    /// <summary>
    /// Manages the JSON manifest for processed media files (optional; see <see cref="BackupManager.TrackingEnabled"/>).
    /// </summary>
    public class BackupManager
    {
        private readonly FileDictionary processedFiles;
        private readonly List<Exception> errors;
        private readonly DirectoryManager directoryManager;
        private readonly bool trackingEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupManager"/> class.
        /// </summary>
        /// <param name="errors">The list to collect errors.</param>
        /// <param name="directoryManager">Output layout (manifest and tool artifacts under SokkaCorp).</param>
        /// <param name="trackingEnabled">When false, load/save and mutations are no-ops.</param>
        public BackupManager(List<Exception> errors, DirectoryManager directoryManager, bool trackingEnabled)
        {
            this.processedFiles = new FileDictionary();
            this.errors = errors;
            this.directoryManager = directoryManager;
            this.trackingEnabled = trackingEnabled;
            if (this.trackingEnabled)
            {
                this.LoadJsonBackup();
            }
        }

        /// <summary>
        /// Gets a value indicating whether manifest I/O is active.
        /// </summary>
        public bool TrackingEnabled => this.trackingEnabled;

        /// <summary>
        /// Gets the manifest file path (<c>{output}/SokkaCorp/jsonBackup.json</c>).
        /// </summary>
        /// <returns>Manifest file.</returns>
        public FileInfo GetManifestFile()
        {
            return new FileInfo(Path.Combine(this.directoryManager.GetSokkaCorpDirectory().FullName, "jsonBackup.json"));
        }

        /// <summary>
        /// Adds a processed file to the backup data.
        /// </summary>
        /// <param name="checksum">The checksum of the file.</param>
        /// <param name="fileInfo">The FileInfo of the processed file.</param>
        public void AddProcessedFile(string checksum, FileInfo fileInfo)
        {
            if (!this.trackingEnabled)
            {
                return;
            }

            if (!this.processedFiles.TryGetValue(checksum, out List<FileInfo>? files) || files == null)
            {
                files = new List<FileInfo>();
                this.processedFiles[checksum] = files;
            }

            files.Add(fileInfo);
        }

        /// <summary>
        /// Checks if a file with the given checksum is already in the backup.
        /// </summary>
        /// <param name="checksum">The checksum of the file.</param>
        /// <returns>True if the checksum exists in the backup; otherwise, false.</returns>
        public bool IsProcessed(string checksum)
        {
            if (!this.trackingEnabled)
            {
                return false;
            }

            return this.processedFiles.ContainsKey(checksum);
        }

        /// <summary>
        /// Removes entries from the JSON backup that no longer exist in the file system.
        /// </summary>
        public void PruneJsonBackup()
        {
            if (!this.trackingEnabled)
            {
                return;
            }

            StaticLog.Enter(nameof(this.PruneJsonBackup));
            try
            {
                List<string> keysToRemove = new List<string>();
                foreach (KeyValuePair<string, List<FileInfo>> file in this.processedFiles)
                {
                    if (!(file.Value?.FirstOrDefault()?.Exists ?? false))
                    {
                        keysToRemove.Add(file.Key);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    this.processedFiles.Remove(key);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error pruning backup");
                this.errors.Add(ex);
            }
            finally
            {
                this.WriteJsonBackup();
                StaticLog.Exit(nameof(this.PruneJsonBackup));
            }
        }

        /// <summary>
        /// Repopulates the JSON backup by scanning all files in the processed directory.
        /// </summary>
        public void RepopulateJsonBackup()
        {
            if (!this.trackingEnabled)
            {
                return;
            }

            StaticLog.Enter(nameof(this.RepopulateJsonBackup));
            try
            {
                this.processedFiles.Clear();

                DirectoryInfo processedDirectory = this.directoryManager.GetProcessedRootDirectory();
                if (processedDirectory.Exists)
                {
                    Log.Information("Repopulating backup from processed directory: {Path}", processedDirectory.FullName);
                    IEnumerable<FileInfo> allThemFiles = processedDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo thatFile in allThemFiles)
                    {
                        try
                        {
                            string checksum = Statics.GetChecksum(thatFile);
                            this.AddProcessedFile(checksum, thatFile);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error repopulating backup with file: {Path}", thatFile.FullName);
                            this.errors.Add(ex);
                        }
                    }
                }
                else
                {
                    Log.Warning("Processed directory does not exist: {Path}. Cannot repopulate backup.", processedDirectory.FullName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error repopulating backup");
                this.errors.Add(ex);
            }
            finally
            {
                this.WriteJsonBackup();
                StaticLog.Exit(nameof(this.RepopulateJsonBackup));
            }
        }

        /// <summary>
        /// Writes the processed files backup to JSON.
        /// </summary>
        public void WriteJsonBackup()
        {
            if (!this.trackingEnabled)
            {
                return;
            }

            try
            {
                string jsArchive = JsonSerializer.Serialize(this.processedFiles.ToDictionary(x => x.Key, x => x.Value.Select(y => y.FullName)));
                File.WriteAllText(this.GetManifestFile().FullName, jsArchive);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error writing JSON backup");
                this.errors.Add(ex);
            }
        }

        /// <summary>
        /// Loads the processed files backup from JSON.
        /// </summary>
        private void LoadJsonBackup()
        {
            try
            {
                FileInfo manifest = this.GetManifestFile();
                if (File.Exists(manifest.FullName))
                {
                    string jsonString = File.ReadAllText(manifest.FullName);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        Dictionary<string, List<string>>? files = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
                        if (files != null)
                        {
                            foreach (string k in files.Keys)
                            {
                                if (files[k] != null)
                                {
                                    this.processedFiles[k] = files[k].Select(x => new FileInfo(x)).ToList();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading JSON backup");
                this.errors.Add(ex);
            }
        }
    }
}
