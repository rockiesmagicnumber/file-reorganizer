// <copyright file="FileDictionary.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace SokkaCorp.MediaLibraryOrganizer.Lib
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    public class FileDictionary<T, TK> : IDictionary<string, List<string>>
    where T : struct
    where TK : List<string>
    {
        private readonly Dictionary<string, List<string>> dict = [];

        /// <inheritdoc/>
        public List<string> this[string key]
        {
            get
            {
                if (!this.dict.TryGetValue(key, out List<string>? files))
                {
                    files = [];
                }

                return files;
            }

            set
            {
                if (this.dict.TryGetValue(key, out List<string>? files))
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
        public ICollection<List<string>> Values => this.dict.Values;

        /// <inheritdoc/>
        public int Count => this.dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(string key, List<string> value)
        {
            this.dict.Add(key, value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, List<string>> item)
        {
            this.dict.Add(item.Key, item.Value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.dict.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, List<string>> item)
        {
            return this.dict.Contains(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return this.dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, List<string>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return this.dict.Remove(key);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, List<string>> item)
        {
            return this.dict.Remove(item.Key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out List<string> value)
        {
            if (this.dict.TryGetValue(key, out List<string>? t))
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