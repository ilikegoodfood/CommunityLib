using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class HolyTenet_PowerSource : HolyTenet
    {
        public static List<Power_Temporary> powers = new List<Power_Temporary>();

        public HolyTenet_PowerSource(HolyOrder order)
            : base(order)
        {

        }

        public virtual List<Power_Temporary> getPowers()
        {
            return powers;
        }
    }
}
