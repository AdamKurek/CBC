using System.Collections;

namespace CBC.Shared
{
    public class SearchableQueue<T> : IEnumerable<T> where T : class
    {
        private T[] buffer;
        private int capacity;
        private int head;
        private int tail;
        private int count;

        public SearchableQueue(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            this.capacity = capacity;
            this.buffer = new T[capacity];
            this.head = 0;
            this.tail = 0;
            this.count = 0;
        }

        public int Count
        {
            get { return count; }
        }

        public void Enqueue(T item)
        {
            if (count == capacity)
            {
                head = (head + 1) % capacity;
                count--;
            }

            buffer[tail] = item;
            tail = (tail + 1) % capacity;
            count++;
        }

        public T Dequeue()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("Buffer is empty.");
            }

            T item = buffer[head];
            head = (head + 1) % capacity;
            count--;
            return item;
        }

        public T SearchFromMostRecent(Func<T, bool> predicate)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                int index = (head + i) % capacity;
                if (predicate(buffer[index]))
                {
                    return buffer[index];
                }
            }

            throw new InvalidOperationException("No matching item found.");
        }

        public T Peek()
        {
            return buffer[head];
        }

        public void Clear()
        {
            head = 0;
            tail = 0;
            count = 0;
        }
        public IEnumerator<T> GetEnumerator()
        {
            int current = head;
            for (int i = 0; i < count; i++)
            {
                yield return buffer[current];
                current = (current + 1) % capacity;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
