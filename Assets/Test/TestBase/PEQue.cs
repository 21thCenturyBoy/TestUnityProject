using System;
using System.Collections.Generic;

namespace PEUtils {
    public class PEQue<T> where T : IComparable<T> {
        public List<T> lst = null;
        public int Count { get => lst.Count; }
        public PEQue(int capacity = 4) {
            lst = new List<T>(capacity);
        }

        /// <summary>
        /// 出队列
        /// </summary>
        public void Enqueue(T item) {
            lst.Add(item);

            HeapfiyUp(lst.Count - 1);
        }
        /// <summary>
        /// 出队列
        /// </summary>
        public T Dequeue() {
            if(lst.Count == 0) {
                return default;
            }
            T item = lst[0];
            int endIndex = lst.Count - 1;
            lst[0] = lst[endIndex];
            lst.RemoveAt(endIndex);
            --endIndex;
            HeapfiyDown(0, endIndex);

            return item;
        }

        public T Peek() {
            return lst.Count > 0 ? lst[0] : default;
        }
        public int IndexOf(T t) {
            return lst.IndexOf(t);
        }
        public T RemoveAt(int rmvIndex) {
            if(lst.Count <= rmvIndex) {
                return default;
            }
            T item = lst[rmvIndex];
            int endIndex = lst.Count - 1;
            lst[rmvIndex] = lst[endIndex];
            lst.RemoveAt(endIndex);
            --endIndex;

            if(rmvIndex < endIndex) {
                int parentIndex = (rmvIndex - 1) / 2;
                if(parentIndex > 0 && lst[rmvIndex].CompareTo(lst[parentIndex]) < 0) {
                    HeapfiyUp(rmvIndex);
                }
                else {
                    HeapfiyDown(rmvIndex, endIndex);
                }
            }

            return item;
        }
        public T RemoveItem(T t) {
            int index = IndexOf(t);
            return index != -1 ? RemoveAt(index) : default;
        }

        public void Clear() {
            lst.Clear();
        }
        public bool Contains(T t) {
            return lst.Contains(t);
        }
        public bool IsEmpty() {
            return lst.Count == 0;
        }
        public List<T> ToList() {
            return lst;
        }
        public T[] ToArray() {
            return lst.ToArray();
        }

        void HeapfiyUp(int childIndex) {
            int parentIndex = (childIndex - 1) / 2;
            while(childIndex > 0 && lst[childIndex].CompareTo(lst[parentIndex]) < 0) {
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
                parentIndex = (childIndex - 1) / 2;
            }
        }
        void HeapfiyDown(int topIndex, int endIndex) {
            while(true) {
                int minIndex = topIndex;
                int childIndex = topIndex * 2 + 1;
                if(childIndex <= endIndex && lst[childIndex].CompareTo(lst[topIndex]) < 0)
                    minIndex = childIndex;
                childIndex = topIndex * 2 + 2;
                if(childIndex <= endIndex && lst[childIndex].CompareTo(lst[minIndex]) < 0)
                    minIndex = childIndex;
                if(topIndex == minIndex) break;
                Swap(topIndex, minIndex);
                topIndex = minIndex;
            }
        }

        void Swap(int a, int b) {
            T temp = lst[a];
            lst[a] = lst[b];
            lst[b] = temp;
        }
    }
}
