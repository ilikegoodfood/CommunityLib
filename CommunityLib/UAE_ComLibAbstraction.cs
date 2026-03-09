using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class UAE_ComLibAbstraction : UAE_Abstraction
    {
        public UAE_ComLibAbstraction(Map map, UA ua)
            : base(map, ua)
        {

        }

        public virtual int getRecruitementCost()
        {
            return 1;
        }
    }
}
