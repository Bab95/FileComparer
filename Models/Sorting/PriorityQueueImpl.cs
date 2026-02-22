namespace FileComparer.Models.Sorting
{
    /// <summary>
    /// Represents a generic priority queue that stores elements and allows retrieval of the element with the highest
    /// priority according to a specified comparer.
    /// </summary>
    /// <remarks>This implementation maintains elements in a min-heap structure, where the element with the
    /// lowest value (as determined by the comparer) is considered the highest priority and is dequeued first. The queue
    /// uses the default comparer for the type unless a custom comparer is provided. The class is not thread-safe;
    /// concurrent access should be synchronized externally if used in multithreaded scenarios.</remarks>
    /// <typeparam name="T">The type of elements stored in the priority queue.</typeparam>
    public class PriorityQueueImpl<T>
    {
        /// <summary>
        /// Represents the internal collection of elements stored in the heap.
        /// </summary>
        private List<T> heap = new List<T>();

        /// <summary>
        /// Represents the comparer used to determine the ordering of elements of type T.
        /// </summary>
        /// <remarks>The comparer defines how elements are compared for sorting or ordering operations. If
        /// not specified, the default comparer for type T may be used.</remarks>
        private IComparer<T> comparer;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count { get { return heap.Count; } }

        /// <summary>
        /// Initializes a new instance of the PriorityQueueImpl class using the default comparer for the element type.
        /// </summary>
        /// <remarks>Use this constructor when you want the queue to order elements according to their
        /// natural ordering as defined by the default comparer for type T. If a custom ordering is required, use the
        /// constructor that accepts an IComparer<T>.</remarks>
        public PriorityQueueImpl() : this(Comparer<T>.Default) { }

        /// <summary>
        /// Initializes a new instance of the PriorityQueueImpl class using the specified comparer to determine the
        /// order of elements.
        /// </summary>
        /// <remarks>If the comparer is null, an ArgumentNullException is thrown. The provided comparer
        /// determines how elements are prioritized within the queue, affecting dequeue and enqueue
        /// operations.</remarks>
        /// <param name="comparer">The comparer used to define the priority ordering of elements in the queue. Cannot be null.</param>
        public PriorityQueueImpl(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Adds the specified item to the priority queue, maintaining the queue's ordering according to the comparer.
        /// </summary>
        /// <param name="item">The item to add to the priority queue. The item's priority is determined by the comparer associated with the
        /// queue.</param>
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

        /// <summary>
        /// Removes and returns the element with the highest priority from the queue.
        /// </summary>
        /// <remarks>This method modifies the queue by removing its highest-priority element. Subsequent
        /// calls will return the next element in priority order. The operation is efficient and maintains the priority
        /// ordering of the remaining elements.</remarks>
        /// <returns>The element of type T that was removed from the queue. The returned element is the one with the highest
        /// priority according to the queue's comparer.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
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

        /// <summary>
        /// Determines whether the heap contains any elements.
        /// </summary>
        /// <returns>true if the heap is empty; otherwise, false.</returns>
        public bool IsEmpty()
        {
            return heap.Count == 0;
        }

        /// <summary>
        /// Determines if the heap is empty.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T Peek()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Queue is empty");

            return heap[0];
        }

        /// <summary>
        /// Exchanges the elements at the specified indices within the heap.
        /// </summary>
        /// <remarks>Both indices must refer to valid positions in the heap. If either index is out of
        /// range, an exception may be thrown.</remarks>
        /// <param name="i">The index of the first element to swap. Must be a valid index within the heap.</param>
        /// <param name="j">The index of the second element to swap. Must be a valid index within the heap.</param>
        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}
