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
        public int maxTravelTime;

        public int arrivalCancellationDelay;

        public bool runAIImmediatelyOnArrival;

        public bool hasArrived;

        public Task_GoToUnit(Unit self, Unit c, int maxTravelTime = -1, int arrivalCancellationDelay = 0, bool runAIImmediatelyOnArrival = false) : base(self, c)
        {
            this.maxTravelTime = maxTravelTime;
            this.arrivalCancellationDelay = arrivalCancellationDelay;
            this.runAIImmediatelyOnArrival = runAIImmediatelyOnArrival;
            hasArrived = false;
        }

        public override string getShort()
        {
            if (target == null)
            {
                return $"Travelling to no-one";
            }

            if (hasArrived && arrivalCancellationDelay > 0)
            {
                return $"Following {target.getName()} ({arrivalCancellationDelay} turns remaining)";
            }

            if (maxTravelTime > -1)
            {
                return $"Travelling to {target.getName()} ({maxTravelTime} turns remaining)";
            }

            return $"Travelling to {target.getName()}";
        }

        public override string getLong()
        {
            if (target == null)
            {
                return $"Travelling to no-one at nowhere";
            }

            if (hasArrived && arrivalCancellationDelay > 0)
            {
                return $"Following {target.getName()} who is currently at {target.location.getName()} ({arrivalCancellationDelay} turns remaining)";
            }

            if (maxTravelTime > -1)
            {
                return $"Travelling to {target.getName()} at {target.location.getName()} ({maxTravelTime} turns remaining)";
            }

            return $"This unit is travelling to {target.getName()} at {target.location.getName()}";
        }

        public override Location getLocation()
        {
            return target.location;
        }

        public override void turnTick(Unit unit)
        {
            if (hasArrived)
            {
                if (arrivalCancellationDelay > 0)
                {
                    arrivalCancellationDelay--;
                }
                else
                {
                    unit.task = null;
                }
            }

            if (target == null || (!ModCore.Get().checkIsUnitSubsumed(target) && target.isDead))
            {
                unit.task = null;
                return;
            }

            if (unit.location == target.location)
            {
                hasArrived = true;
                if (runAIImmediatelyOnArrival)
                {
                    unit.turnTickAI();
                }
            }

            if (maxTravelTime > -1)
            {
                maxTravelTime--;
                if (maxTravelTime == 0)
                {
                    unit.task = null;
                }
            }

            Location[] path = Pathfinding.getPathTo(unit.location, target.location, unit, false);
            while (unit.movesTaken < unit.getMaxMoves())
            {
                bool moved = unit.map.moveTowards(unit, target.location);
                if (!moved)
                {
                    World.log("Move unsuccessful. Cancelling go to challenge");
                    unit.task = null;
                    return;
                }
                unit.movesTaken++;

                if (unit.location == target.location)
                {
                    hasArrived = true;
                    if (runAIImmediatelyOnArrival)
                    {
                        unit.turnTickAI();
                    }
                    break;
                }
            }
        }
    }
}
