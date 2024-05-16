using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class ItemPriorityPair<T, U> where U : IComparable<U>
    {
        public T Item = default(T);

        public U Priority = default(U);

        public ItemPriorityPair(T item, U priority)
        {
            this.Item = item;
            this.Priority = priority;
        }
    }
}
