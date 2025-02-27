using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class TradeRouteData
    {
        public readonly List<List<int>> indexGroups = new List<List<int>>();

        public void Clear()
        {
            indexGroups.Clear();
        }
    }
}
