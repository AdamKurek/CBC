using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBC.Shared
{
    public class takolejka<T> :IEnumerable<T> where T : class //todo use circular buffer
    {
        int maxFields;
        public takolejka(int maxFields)
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
