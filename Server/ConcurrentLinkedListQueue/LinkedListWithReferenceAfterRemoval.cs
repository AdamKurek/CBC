using System.Collections;

namespace ConcurrentLinkedListQueue
{
    public class QQNode<T>
    {
        public T value { get; set; }
        public QQNode<T>? Next { get; set; }
        //public bool IsConnected { get; set; } = true;
        public QQNode(T v, QQList<T> list)
        {
            value = v;
        }
        public QQNode()
        {
        }
    }
    public class QQList<T> : IEnumerable<QQNode<T>>
    {

        public QQNode<T> Tail = new();
        public QQNode<T> Head;
        private readonly object o = new();
        public void AddHead(T value)
        {
            lock (o)
            {
                Head.Next = new QQNode<T>(value, this);
                Head = Head.Next;
                return;
            }
        }

        public QQNode<T> removeNextNode(QQNode<T> prev)
        {
            var ret = prev.Next;
            prev.Next = prev.Next!.Next;
            //ret!.IsConnected = false;
            lock (o)
            {
                if (prev.Next == null)
                {
                    Head = prev;
                }
            }
            return ret!;
        }

        public QQList()
        {
            Head = Tail;
        }

        public IEnumerator<QQNode<T>> GetEnumerator()
        {
            QQNode<T> current = Tail;

            while (current?.Next != null)
            {
                current = current.Next;
                yield return current;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


