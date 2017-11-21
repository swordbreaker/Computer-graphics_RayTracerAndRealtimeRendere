using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CornellBox.Models
{
    public class PriorityQueue<T> : ICollection, IEnumerable<T>
    {
        private readonly List<T> _values;
        private readonly IComparer<T> _comp;
        public int Count => _values.Count;
        public bool IsReadOnly { get; }
        public object SyncRoot { get; } = false;
        public bool IsSynchronized { get; } = true;
        public bool IsEmpty => _values.Count == 0;

        public PriorityQueue(IComparer<T> comparer)
        {
            _comp = comparer;
            _values = new List<T>();
        }

        public PriorityQueue(IEnumerable<T> values, IComparer<T> comparer)
        {
            _values = new List<T>(values);
            _comp = comparer;

            int mid = _values.Count() / 2 - 1;
            for (int i = mid; i >= 0; i--)
            {
                SiftDown(i);
            }
        }

        /// <summary>
        /// Adds
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(T data)
        {
            _values.Add(data);
            SiftUp(_values.Count - 1);
        }

        /// <summary>
        /// Removes and returns the minimum
        /// </summary>
        /// <returns></returns>
        public T Deque()
        {
            if (Count == 0) throw new Exception("Queue is empty");
            if (Count == 1)
            {
                var v = _values[0];
                _values.RemoveAt(0);
                return v;
            }

            int last = Count - 1;
            // swap a[first] with a[last]
            var tmp = _values[0];
            _values[0] = _values[last];
            _values[last] = tmp;

            var data = _values[last];
            _values.RemoveAt(last);

            SiftDown(0);
            return data;
        }


        /// <summary>
        /// Returns
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (Count == 0) throw new Exception("Queue is empty");
            return _values[0];
        }

        public bool Contains(T value)
        {
            return _values.Contains(value);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public void Clear()
        {
            _values.Clear();
        }

        private void SiftDown(int index)
        {
            var end = _values.Count - 1;
            int son = index * 2 + 1;

            while (son <= end)
            {
                //son is left
                if (son < end)
                {

                    if (_comp.Compare(_values[son], _values[son + 1]) > 0) son++;
                }
                //son is the biggest son
                if (_comp.Compare(_values[index], _values[son]) > 0)
                {
                    //swap
                    var tmp = _values[index];
                    _values[index] = _values[son];
                    _values[son] = tmp;

                    index = son;
                    son = 2 * index + 1;
                }
                else
                {
                    return;
                }
            }
        }

        private void SiftUp(int index)
        {
            int parent = (index - 1) / 2; // parent

            while (parent >= 0)
            {
                if (_comp.Compare(_values[parent], _values[index]) > 0)
                {
                    // swap
                    var tmp = _values[index];
                    _values[index] = _values[parent];
                    _values[parent] = tmp;

                    index = parent;
                    parent = (index - 1) / 2;
                }
                else
                {
                    return;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public T this[int index] => _values[index];

        public void Remove(T value)
        {
            if (!Contains(value)) throw new ArgumentOutOfRangeException(nameof(value), "not found in the Queue");
            var itemId = _values.FindIndex(v => v.Equals(value));
            RemoveAt(itemId);
            //_values[itemId] = _values[Count - 1];
            //_values[Count - 1] = value;
            //SiftUp(itemId);
        }

        public void RemoveAt(int id)
        {
            var value = _values[id];
            _values[id] = _values[Count - 1];
            _values[Count - 1] = value;
            SiftUp(id);
            _values.RemoveAt(Count - 1);
        }

        public void Remove(Predicate<T> predicate)
        {
            var id = _values.FindIndex(predicate);
            RemoveAt(id);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            _values.CopyTo((T[])array, index);
        }
    }
}
