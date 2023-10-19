using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparer.Models
{
    public class PriorityQueueImpl<T>
    {
        private List<T> heap = new List<T>();
        private IComparer<T> comparer;

        public int Count { get { return heap.Count; } }

        public PriorityQueueImpl() : this(Comparer<T>.Default) { }

        public PriorityQueueImpl(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public void Enqueue(T item)
        {
            heap.Add(item);
            int i = heap.Count - 1;

            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (comparer.Compare(heap[i], heap[parent]) >= 0)
                    break;

                Swap(i, parent);
                i = parent;
            }
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            T min = heap[0];
            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);

            int i = 0;
            while (true)
            {
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left < heap.Count && comparer.Compare(heap[left], heap[smallest]) < 0)
                    smallest = left;

                if (right < heap.Count && comparer.Compare(heap[right], heap[smallest]) < 0)
                    smallest = right;

                if (smallest == i)
                    break;

                Swap(i, smallest);
                i = smallest;
            }

            return min;
        }

        public bool IsEmpty()
        {
            return heap.Count == 0;
        }
        public T Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty");

            return heap[0];
        }

        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}
