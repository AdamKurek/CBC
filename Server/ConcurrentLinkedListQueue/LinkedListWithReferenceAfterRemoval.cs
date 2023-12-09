using CBC.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentLinkedListQueue
{
    public class QQNode<T>
    {
        public T value { get; set; }
        public T NextValue { get { return Next!.value; } set => Next!.value = value; }
        public QQNode<T>? Next { get; set; }
        public bool IsConnected { get; set; } = true; 
        public QQNode(T v, QQList<T> list)
        {
            value = v;
        }
        public QQNode()
        {
        }
    }
    public class QQList<T>:IEnumerable<QQNode<T>>
    {

        public QQNode<T> Tail = new();
        private QQNode<T> Head;
        private object o = new();
        public void AddHead(T value)
        {
            lock(o)
            {
                {
                    Head.Next = new QQNode<T>(value, this);
                    Head = Head.Next;
                    return;
                }
            }
        }

        public void removeNextNode(QQNode<T> rmn)
        {
            rmn.Next!.IsConnected = false;
            lock (o)
            {
                if (Head == rmn.Next)
                {
                    Head = rmn;
                    rmn.Next = rmn.Next.Next;
                    return;
                }
            }
            rmn.Next = rmn.Next.Next;
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
                yield return current;
                current = current.Next;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


