using System.Collections;

namespace CBC.Shared
{
    public class SearchableQueue<T> :IEnumerable<T> where T : class //todo use circular buffer
    {
        int maxFields;
        public SearchableQueue(int maxFields)
        {
            this.maxFields = maxFields;
        }
        
        public void Push(T val)
        {
            queue.AddLast(val);
            if(queue.Count > maxFields)
            {
                queue.RemoveFirst();
            }
        }


        LinkedList<T> queue = new();
       
        public bool checkForExistance(Func<T,bool> condition)
        {
            foreach(T t in queue)
            {
                if (condition(t))
                {
                    return true;
                }
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)queue).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)queue).GetEnumerator();
        }
    }   
}
