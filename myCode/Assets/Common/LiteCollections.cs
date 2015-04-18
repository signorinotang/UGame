/*
** This source file is the implementation of LiteCollections
**
** For the latest info, see https://github.com/paladin-t/LiteCollections
**
** Copyright (c) 2015 Wang Renxin
**
** Permission is hereby granted, free of charge, to any person obtaining a copy of
** this software and associated documentation files (the "Software"), to deal in
** the Software without restriction, including without limitation the rights to
** use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
** the Software, and to permit persons to whom the Software is furnished to do so,
** subject to the following conditions:
**
** The above copyright notice and this permission notice shall be included in all
** copies or substantial portions of the Software.
**
** THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
** IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
** FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
** COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
** IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
** CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Collections.LiteCollections
{
	public class LiteList<T>
	{
		public ArrayList _raw = null;

		public int Count { get { return _raw.Count; } }

		public T this[int index]
		{
			get { return (T)_raw[index]; }
			set { _raw[index] = value; }
		}

		public LiteList()
		{
			_raw = new ArrayList();
		}

		public LiteList(int capicity)
		{
			_raw = new ArrayList(capicity);
		}
	}
	
	public class LiteMap<TKey, TValue>
	{
		public Hashtable _raw = null;
		
		public int Count { get { return _raw.Count; } }
		
		public ICollection Keys { get { return _raw.Keys; } }
		
		public ICollection Values { get { return _raw.Values; } }
		
		public TValue this[TKey key]
		{
			get { return (TValue)_raw[key]; }
			set { _raw[key] = value; }
		}
		
		public LiteMap()
		{
			_raw = new Hashtable();
		}
		
		public LiteMap(int capicity)
		{
			_raw = new Hashtable(capicity);
		}
	}

	public static class Extentions
	{
		public static void Add<T>(this LiteList<T> c, T item)
		{
			c._raw.Add(item);
		}

		public static void AddRange<T>(this LiteList<T> c, ICollection collection)
		{
			c._raw.AddRange(collection);
		}

		public static ReadOnlyCollection<T> AsReadOnly<T>(this LiteList<T> c)
		{
			return new ReadOnlyCollection<T>(c.ToArray());
		}

		public static int BinarySearch<T>(this LiteList<T> c, int index, int count, T item, IComparer comparer)
		{
			return c._raw.BinarySearch(index, count, item, comparer);
		}

		public static int BinarySearch<T>(this LiteList<T> c, T item)
		{
			return c._raw.BinarySearch(item);
		}

		public static int BinarySearch<T>(this LiteList<T> c, T item, IComparer comparer)
		{
			return c._raw.BinarySearch(item, comparer);
		}

		public static void Clear<T>(this LiteList<T> c)
		{
			c._raw.Clear();
		}

		public static bool Contains<T>(this LiteList<T> c, T item)
		{
			return c._raw.Contains(item);
		}

		public static void CopyTo<T>(this LiteList<T> c, T[] array, int arrayIndex)
		{
			c._raw.CopyTo(array, arrayIndex);
		}

		public static void CopyTo<T>(this LiteList<T> c, int index, T[] array, int arrayIndex, int count)
		{
			c._raw.CopyTo(index, array, arrayIndex, count);
		}

		public static void CopyTo<T>(this LiteList<T> c, T[] array)
		{
			c._raw.CopyTo(array);
		}

		public static IEnumerator GetEnumerator<T>(this LiteList<T> c)
		{
			return c._raw.GetEnumerator();
		}

		public static int IndexOf<T>(this LiteList<T> c, T item, int index)
		{
			return c._raw.IndexOf(item, index);
		}
		
		public static int IndexOf<T>(this LiteList<T> c, T item, int index, int count)
		{
			return c._raw.IndexOf(item, index, count);
		}
		
		public static int IndexOf<T>(this LiteList<T> c, T item)
		{
			return c._raw.IndexOf(item);
		}
		
		public static void Insert<T>(this LiteList<T> c, int index, T item)
		{
			c._raw.Insert(index, item);
		}
		
		public static void InsertRange<T>(this LiteList<T> c, int index, ICollection collection)
		{
			c._raw.InsertRange(index, collection);
		}
		
		public static int LastIndexOf<T>(this LiteList<T> c, T item, int index)
		{
			return c._raw.LastIndexOf(item, index);
		}
		
		public static int LastIndexOf<T>(this LiteList<T> c, T item, int index, int count)
		{
			return c._raw.LastIndexOf(item, index, count);
		}
		
		public static int LastIndexOf<T>(this LiteList<T> c, T item)
		{
			return c._raw.LastIndexOf(item);
		}

		public static bool Remove<T>(this LiteList<T> c, T item)
		{
			if (!c.Contains(item))
				return false;

			c._raw.Remove(item);

			return true;
		}

		public static void RemoveAt<T>(this LiteList<T> c, int index)
		{
			c._raw.RemoveAt(index);
		}

		public static void RemoveRange<T>(this LiteList<T> c, int index, int count)
		{
			c._raw.RemoveRange(index, count);
		}

        public static void Shuffle<T>(this LiteList<T> c)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            for (int k = 0; k < c.Count; k++)
            {
                int i = rnd.Next(0, c.Count - 1);
                int j = rnd.Next(0, c.Count - 1);
                T t = c[i];
                c[i] = c[j];
                c[j] = t;
            }
        }

		public static void Sort<T>(this LiteList<T> c, IComparer comparer)
		{
			c._raw.Sort(comparer);
		}
		
		public static void Sort<T>(this LiteList<T> c, int index, int count, IComparer comparer)
		{
			c._raw.Sort(index, count, comparer);
		}
		
		public static void Sort<T>(this LiteList<T> c)
		{
			c._raw.Sort();
		}

		public static T[] ToArray<T>(this LiteList<T> c)
		{
			T[] result = new T[c.Count];
			for (int i = 0; i < c.Count; i++)
				result[i] = c[i];

			return result;
		}

		public static void Add<TKey, TValue>(this LiteMap<TKey, TValue> c, TKey key, TValue value)
		{
			c._raw.Add(key, value);
		}

		public static void Clear<TKey, TValue>(this LiteMap<TKey, TValue> c)
		{
			c._raw.Clear();
		}
		
		public static bool ContainsKey<TKey, TValue>(this LiteMap<TKey, TValue> c, TKey key)
		{
			return c._raw.ContainsKey(key);
		}
		
		public static bool ContainsValue<TKey, TValue>(this LiteMap<TKey, TValue> c, TValue value)
		{
			return c._raw.ContainsValue(value);
		}
		
		public static IDictionaryEnumerator GetEnumerator<TKey, TValue>(this LiteMap<TKey, TValue> c)
		{
			return c._raw.GetEnumerator();
		}
		
		public static bool Remove<TKey, TValue>(this LiteMap<TKey, TValue> c, TKey key)
		{
			if (!c._raw.ContainsKey(key))
				return false;
			
			c._raw.Remove(key);
			
			return true;
		}
		
		public static bool TryGetValue<TKey, TValue>(this LiteMap<TKey, TValue> c, TKey key, out TValue value)
		{
			value = default(TValue);
			
			if (!c.ContainsKey(key))
				return false;
			
			value = (TValue)c._raw[key];
			
			return true;
		}
	}
}
