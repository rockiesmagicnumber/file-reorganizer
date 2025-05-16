// <copyright file="FileDictionary.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    public class FileDictionary : IDictionary<string, List<FileInfo>>
    {
        private readonly Dictionary<string, List<FileInfo>> dict = new Dictionary<string, List<FileInfo>>();

        /// <inheritdoc/>
        public List<FileInfo> this[string key]
        {
            get
            {
                if (!this.dict.TryGetValue(key, out List<FileInfo>? files))
                {
                    files = new List<FileInfo>();
                }

                return files;
            }

            set
            {
                if (this.dict.TryGetValue(key, out List<FileInfo>? files))
                {
                    this.dict[key] = files.Union(value).ToList();
                }
                else
                {
                    this.dict[key] = value;
                }
            }
        }

        /// <inheritdoc/>
        public ICollection<string> Keys => this.dict.Keys;

        /// <inheritdoc/>
        public ICollection<List<FileInfo>> Values => this.dict.Values;

        /// <inheritdoc/>
        public int Count => this.dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(string key, List<FileInfo> value)
        {
            this.dict.Add(key, value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, List<FileInfo>> item)
        {
            this.dict.Add(item.Key, item.Value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.dict.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, List<FileInfo>> item)
        {
            return this.dict.Contains(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return this.dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, List<FileInfo>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, List<FileInfo>>> GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return this.dict.Remove(key);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, List<FileInfo>> item)
        {
            return this.dict.Remove(item.Key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out List<FileInfo> value)
        {
            if (this.dict.TryGetValue(key, out List<FileInfo>? t))
            {
                value = t;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }
    }
}