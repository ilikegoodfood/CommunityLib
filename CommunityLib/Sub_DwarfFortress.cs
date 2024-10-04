using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Sub_DwarfFortress : Sub_Fort
    {
        public Sub_DwarfFortress(SettlementHuman set)
            : base(set)
        {

        }

        public override bool definesSprite()
        {
            return false;
        }
    }
}
