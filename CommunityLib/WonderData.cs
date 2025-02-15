using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class WonderData : IEquatable<WonderData>
    {
        public readonly Type WonderType;

        private int _priority = 1;

        public int Priority
        {
            get
            {
                return _priority;
            }
            private set
            {
                if (value < 1)
                {
                    _priority = 0;
                }
                else if (Priority > 3)
                {
                    _priority = 3;
                }
                else
                {
                    _priority = value;
                }
            }
        }

        public readonly bool Unique = false;

        public WonderData(Type wonderType, int priority = 1, bool unique = false)
        {
            WonderType = wonderType;
            Priority = priority;
            Unique = unique;
        }

        public bool Equals(WonderData other)
        {
            return WonderType == other.WonderType;
        }
    }
}
