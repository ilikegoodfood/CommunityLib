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

        // Implicit conversion FROM Tuple TO ValuePriorityPair
        public static implicit operator ValuePriorityPair<TValue, TPriority>((TValue, TPriority) tuple)
        {
            return new ValuePriorityPair<TValue, TPriority>(tuple.Item1, tuple.Item2);
        }

        // Implicit conversion FROM ValuePriorityPair TO Tuple
        public static implicit operator (TValue, TPriority)(ValuePriorityPair<TValue, TPriority> pair)
        {
            return (pair.Value, pair.Priority);
        }

        public override string ToString()
        {
            return $"({Value}, {Priority})";
        }
    }
}
