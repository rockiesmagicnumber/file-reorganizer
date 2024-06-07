// <copyright file="FileDictionary.cs" company="SokkaCorp">
// Copyright (c) SokkaCorp. All rights reserved.
// </copyright>

namespace PhotoLibraryCleaner.Lib
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    public class FileDictionary<T, TK> : IDictionary<string, List<string>>
    where T : struct
    where TK : List<string>
    {
        private readonly Dictionary<string, List<string>> dict = [];

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

        public ICollection<string> Keys => this.dict.Keys;

        public ICollection<List<string>> Values => this.dict.Values;

        public int Count => this.dict.Count;

        public bool IsReadOnly => false;

        public void Add(string key, List<string> value)
        {
            this.dict.Add(key, value);
        }

        public void Add(KeyValuePair<string, List<string>> item)
        {
            this.dict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.dict.Clear();
        }

        public bool Contains(KeyValuePair<string, List<string>> item)
        {
            return this.dict.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return this.dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, List<string>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this.dict.Remove(key);
        }

        public bool Remove(KeyValuePair<string, List<string>> item)
        {
            return this.dict.Remove(item.Key);
        }

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }
    }
}