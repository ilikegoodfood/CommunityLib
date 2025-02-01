using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class T_MagicMastery : Trait
    {
        public override int getMaxLevel()
        {
            return 3;
        }

        public override void turnTick(Person p)
        {
            base.turnTick(p);

            if (p.map.opt_allowMagicalArmsRace && p.unit != null && p.unit.isCommandable())
            {
                p.map.overmind.magicalArmsRace = Math.Max(level, p.map.overmind.magicalArmsRace);
            }
        }
    }
}
