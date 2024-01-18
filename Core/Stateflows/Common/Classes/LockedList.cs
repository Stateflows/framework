using System.Collections;
using System.Collections.Generic;

namespace Stateflows.Common.Classes
{
    internal class LockedList<T> : ICollection<T>
    {
        private readonly List<T> List = new List<T>();

        public int Count
        {
            get
            {
                lock (List)
                {
                    return List.Count;
                }
            }
        }

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (List)
            {
                List.Add(item);
            }
        }

        public void Clear()
        {
            lock (List)
            {
                List.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (List)
            {
                return List.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (List)
            {
                List.CopyTo(array, arrayIndex);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (List)
            {
                return List.GetEnumerator();
            }
        }

        public bool Remove(T item)
        {
            lock (List)
            {
                return List.Remove(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (List)
            {
                return List.GetEnumerator();
            }
        }
    }
}
