using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Power_Temporary : Power
    {
        public Power_Temporary (Map map)
        : base (map)
        {

        }

        public virtual void updtateState()
        {
            return;
        }

        public virtual void addPower(int powerLevelRequirement = 0)
        {
            if (!map.overmind.god.powers.Contains(this))
            {
                map.overmind.god.powers.Add(this);
                map.overmind.god.powerLevelReqs.Add(powerLevelRequirement);
            }
        }

        public virtual void removePower()
        {
            int index = map.overmind.god.powers.FindIndex(p => p == this);
            if (index != -1)
            {
                map.overmind.god.powers.RemoveAt(index);
                map.overmind.god.powerLevelReqs.RemoveAt(index);
            }
        }
    }
}
