#region License
// Copyright 2009-2015 Buu Nguyen
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at https://github.com/buunguyen/combres
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Combres
{
    [Serializable]
    internal sealed class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> source;

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionaryToWrap)
        {
            if (dictionaryToWrap == null)
                throw new ArgumentNullException("dictionaryToWrap");
            source = dictionaryToWrap;
        }

        public int Count
        {
            get { return source.Count; }
        }

        public void Add(TKey key, TValue value)
        {
            ThrowNotSupportedException();
        }

        public bool ContainsKey(TKey key)
        {
            return source.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return source.Keys; }
        }

        public bool Remove(TKey key)
        {
            ThrowNotSupportedException();
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return source.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return source.Values; }
        }

        public TValue this[TKey key]
        {
            get { return source[key]; }
            set { ThrowNotSupportedException(); }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
        }

        public void Clear()
        {
            ThrowNotSupportedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return source.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            source.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)source).GetEnumerator();
        }

        private static void ThrowNotSupportedException()
        {
            throw new NotSupportedException("This dictionary is read-only");
        }
    }
}
