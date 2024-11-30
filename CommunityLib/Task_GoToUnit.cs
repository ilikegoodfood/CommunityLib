using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Task_GoToUnit : Task_Follow
    {
        public Task_GoToUnit(Unit self, Unit c) : base(self, c)
        {

        }

        public override string getShort()
        {
            return $"Travelling to {target.getName()}";
        }

        public override string getLong()
        {
            return $"This unit is travelling to {target.getName()}";
        }

        public override Location getLocation()
        {
            return target.location;
        }

        public override void turnTick(Unit unit)
        {
            if (target == null || target.isDead)
            {
                unit.task = null;
                return;
            }

            if (unit.location == target.location)
            {
                unit.task = null;
                unit.turnTickAI();
                return;
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                bool moved = unit.map.moveTowards(unit, target.location);
                if (!moved)
                {
                    World.log("Move unsuccessful. Cancelling go to challenge");
                    unit.task = null;
                    return;
                }

                if (unit.location == target.location)
                {
                    unit.task = null;
                    unit.turnTickAI();
                    return;
                }
            }
        }
    }
}
