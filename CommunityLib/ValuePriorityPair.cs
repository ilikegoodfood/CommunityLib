using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class ValuePriorityPair<TValue, TPriority> where TPriority : IComparable<TPriority>
    {
        public TValue Value = default(TValue);

        public TPriority Priority = default(TPriority);

        public ValuePriorityPair(TValue item, TPriority priority)
        {
            this.Value = item;
            this.Priority = priority;
        }
    }
}
