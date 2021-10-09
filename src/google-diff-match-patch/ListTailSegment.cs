using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DiffMatchPatch
{
    internal static class ListTailSegment
    {
        public static ListTailSegment<T> ToTailSegment<T>(this List<T> list, int startAt) => new ListTailSegment<T>(list, startAt);
    }

    internal class ListTailSegment<T> : IList<T>
    {
        readonly List<T> _list;
        readonly int _startAt;

        public ListTailSegment(List<T> list, int startAt)
        {
            _list = list;
            _startAt = startAt;
        }

        public IEnumerator<T> GetEnumerator() => _list.Skip(_startAt).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.RemoveRange(_startAt, Count);
        }

        public bool Contains(T item) => _list.Skip(_startAt).Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(_startAt, array, arrayIndex, Count);
        }

        public bool Remove(T item)
        {
            for (var i = _startAt; i < _list.Count; i++)
            {
                var itemToTest = _list[i];
                if (Equals(item, itemToTest))
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public int Count => _list.Count - _startAt;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            for (var i = _startAt; i < _list.Count; i++)
            {
                var itemToTest = _list[i];
                if (Equals(item, itemToTest))
                {
                    _list.RemoveAt(i);
                    return i - _startAt;
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index + _startAt, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index + _startAt);
        }

        public void RemoveRange(int start, int count)
        {
            start = _startAt + start;
            _list.RemoveRange(start, count);
        }

        public void InsertRange(int start, IEnumerable<T> collection)
        {
            start = _startAt + start;
            _list.InsertRange(start, collection);
        }

        public T this[int index]
        {
            get => _list[index + _startAt];
            set => _list[index + _startAt] = value;
        }
    }
}