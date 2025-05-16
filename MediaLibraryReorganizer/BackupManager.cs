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
    using static Statics; // Needed for GetJsonBackup and GetProcessedDirectory, and GetChecksum

    /// <summary>
    /// Manages the JSON backup file for processed media files.
    /// </summary>
    public class BackupManager
    {
        private readonly FileDictionary processedFiles;
        private readonly List<Exception> errors; // Added error list dependency

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupManager"/> class.
        /// </summary>
        /// <param name="errors">The list to collect errors.</param>
        public BackupManager(List<Exception> errors)
        {
            this.processedFiles = new FileDictionary();
            this.errors = errors; // Assign the error list
            this.LoadJsonBackup(); // Load backup on initialization
        }

        /// <summary>
        /// Adds a processed file to the backup data.
        /// </summary>
        /// <param name="checksum">The checksum of the file.</param>
        /// <param name="fileInfo">The FileInfo of the processed file.</param>
        public void AddProcessedFile(string checksum, FileInfo fileInfo)
        {
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
            return this.processedFiles.ContainsKey(checksum);
        }

        /// <summary>
        /// Removes entries from the JSON backup that no longer exist in the file system.
        /// </summary>
        public void PruneJsonBackup()
        {
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
                this.errors.Add(ex); // Report error
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
            StaticLog.Enter(nameof(this.RepopulateJsonBackup));
            try
            {
                this.processedFiles.Clear(); // Clear existing data before repopulating

                DirectoryInfo processedDirectory = GetProcessedDirectory(); // Uses Statics.OutputDirectory
                if (processedDirectory.Exists)
                {
                    Log.Information($"Repopulating backup from processed directory: {processedDirectory.FullName}");
                    IEnumerable<FileInfo> allThemFiles = processedDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo? thatFile in allThemFiles)
                    {
                        try
                        {
                            // Calculate checksum and add to processed files
                            string checksum = GetChecksum(thatFile); // Uses Statics
                            this.AddProcessedFile(checksum, thatFile);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error repopulating backup with file: {thatFile.FullName}");
                            this.errors.Add(ex); // Report error
                        }
                    }
                }
                else
                {
                    Log.Warning($"Processed directory does not exist: {processedDirectory.FullName}. Cannot repopulate backup.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error repopulating backup");
                this.errors.Add(ex); // Report error
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
            try
            {
                string jsArchive = JsonSerializer.Serialize(this.processedFiles.ToDictionary(x => x.Key, x => x.Value.Select(y => y.FullName)));
                File.WriteAllText(GetJsonBackup().FullName, jsArchive);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error writing JSON backup");
                this.errors.Add(ex); // Report error
            }
        }

        /// <summary>
        /// Loads the processed files backup from JSON.
        /// </summary>
        private void LoadJsonBackup()
        {
            try
            {
                if (File.Exists(GetJsonBackup().FullName))
                {
                    string jsonString = File.ReadAllText(GetJsonBackup().FullName);
                    if (!string.IsNullOrEmpty(jsonString))
                    {
                        Dictionary<string, List<string>> files = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);
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
                this.errors.Add(ex); // Report error
            }
        }
    }
}