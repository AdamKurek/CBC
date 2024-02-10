using System.Collections;

namespace ConcurrentLinkedListQueue
{
    public class ConcurrentLinkedListQueue<T> : IEnumerable<QQNode<T>> where T : class

        //todo2026 make it concurrentcircular buffer and return last element if can't find the one you looking for 
    {
        public ConcurrentLinkedListQueue()
        {
            values = new();
        }

        public void Enqueue(T val)
        {
            //enqu:
            try
            {
                values.AddHead(val);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //goto enqu;
            }
        }

        public QQList<T> values;//todo make it internal after changing settings of unit tests

        public QQNode<T> Tail => values.Tail;
        public QQNode<T> Head => values.Head;


        public T GetFirstUserWithCondition(Func<T, bool> condition)
        {
            QQNode<T> node = values.Tail;
            try
            {
                while (node.Next is not null)
                {
                    QQNode<T> nxt = node.Next;
                    if (nxt == null)
                    {
                        continue;
                    }
                    if (condition(nxt.value))
                    {
                        if (Monitor.TryEnter(node))
                        {
                            try
                            {
                                if (Monitor.TryEnter(nxt))
                                {
                                    try
                                    {
                                        if (nxt == node.Next)
                                        {
                                            //Console.WriteLine((nxt.value as UserPreferences).ConnectionId + " locked");
                                            //if(nxt.IsConnected == true) { 
                                            Console.WriteLine((nxt.value as VideoChatHub.InQueueStatus).preferences.ConnectionId + 3);

                                            return values.removeNextNode(node).value;
                                            //}
                                        }

                                    }
                                    finally
                                    {
                                        Monitor.Exit(nxt);
                                        //Console.WriteLine((nxt.value as UserPreferences).ConnectionId + " Unlocked");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Bug2: " + e);
                            }
                            finally
                            {
                                Monitor.Exit(node);
                            }
                        }
                    }
                    node = nxt;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"bug: {node?.value.ToString()}");
            }
            return null;
        }

        public void RemoveAllOccurences(Func<T, bool> condition)
        {
            int failCount = 0;
        restart:
            try
            {
                QQNode<T> node = values.Tail;
                while (node.Next is not null)
                {
                    var nxt = node.Next;
                    if (nxt == null)
                    {
                        continue;
                    }
                    if (condition(nxt.value))
                    {
                        if (Monitor.TryEnter(node))
                        {
                            try
                            {
                                if (Monitor.TryEnter(nxt))
                                {
                                    try
                                    {
                                        if (nxt == node.Next)
                                        {
                                            //Console.WriteLine((nxt.value as UserPreferences).ConnectionId + " locked");
                                            //if (nxt.IsConnected == true) { 
                                            _ = values.removeNextNode(node);
                                            continue;
                                            //}
                                        }
                                    }
                                    finally
                                    {
                                        Monitor.Exit(nxt);
                                        //Console.WriteLine((nxt.value as UserPreferences).ConnectionId + " Unlocked");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Bug when removing all: " + e);
                            }
                            finally { Monitor.Exit(node); }
                        }
                    }
                    node = nxt;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Rare bug when removing all: " + e);
                if (failCount++ < 3)
                {
                    goto restart;
                }
            }
            return;
        }

        public bool EnsureExistance(Func<T, bool> condition)
        {
            try
            {
                QQNode<T> node = values.Tail;
                while (node.Next is not null)
                {
                    var nxt = node.Next;
                    if (condition(nxt.value))
                    {
                        return true;
                    }
                    node = nxt;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        public IEnumerator<QQNode<T>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}