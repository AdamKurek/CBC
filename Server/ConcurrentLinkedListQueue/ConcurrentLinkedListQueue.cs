using CBC.Shared;
using System.Collections;

namespace ConcurrentLinkedListQueue
{
    public class ConcurrentLinkedListQueueUserPreferences:IEnumerable<QQNode<UserPreferences>>
    {
        public ConcurrentLinkedListQueueUserPreferences() {
            values = new();
        }
        
        public void Enqueue(UserPreferences val)
        {
            enqu:
                try
                { 
                    values.AddHead(val);
                }catch (NullReferenceException ex)
                {
                    goto enqu;
                }
        }

        public QQList<UserPreferences> values;//todo make it internal after changing settings of unit tests
        public bool GetFirstUserWithCondition(Func<UserPreferences, bool> condition, ref string ConnId)
        {
            try
            {
                QQNode<UserPreferences> node = values.Tail;
                while (node.Next is not null)
                {
                    var nxt = node.Next;
                    try{
                        if (condition(nxt.value))
                        {
                            try{
                                if (Monitor.TryEnter(nxt))
                                {
                                    if (nxt.IsConnected)
                                    {
                                        ConnId = nxt.value.ConnectionId;
                                        values.removeNextNode(node);
                                        Monitor.Exit(nxt);
                                        return true;
                                    }
                                }
                            }  catch (Exception e)
                            {
                                Console.WriteLine("Bug: " + e);
                            }
                            continue;
                        }
                    }
                    catch (NullReferenceException e)
                    {
                        if(node is null)
                        {
                            break;
                        }
                    }
                    node = nxt; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Rare bug: " + e);
            }
            return false;
        }

        public IEnumerator<QQNode<UserPreferences>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }
}