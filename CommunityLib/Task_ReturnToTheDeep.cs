using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Task_ReturnToTheDeep : Assets.Code.Task
    {
        public Location target = null;

        public Task_ReturnToTheDeep(Location location)
        {
            target = location;
        }

        public Task_ReturnToTheDeep()
        {
            target = null;
        }


        public static bool delegate_VALID_OCEAN(Location[] currentPath, Location location, Unit u)
        {
            return location.isOcean;
        }

        public override string getShort()
        {
            return "Return to the Deep";
        }

        // Token: 0x060001BB RID: 443 RVA: 0x00008E1C File Offset: 0x0000701C
        public override string getLong()
        {
            if (target != null)
            {
                return "This deep one is returning to the Sea at " + target.getName();
            }

            return "This deep one is returning to the Sea";
        }

        public override void turnTick(Unit unit)
        {
            if (!(unit is UAEN_DeepOne deepOne) || deepOne.moveType == Unit.MoveType.AQUAPHIBIOUS)
            {
                unit.task = null;
                return;
            }

            if (deepOne.location.isOcean)
            {
                //Console.WriteLine("CommunityLib: DeepOne is at ocean location");
                deepOne.moveType = Unit.MoveType.AQUAPHIBIOUS;
                deepOne.task = null;
                return;
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo;
                if (target != null && target.isOcean)
                {
                    pathTo = Pathfinding.getPathTo(unit.location, target, unit, false);

                    Location[] otherPath = Pathfinding.getPathTo(unit.location, delegate_VALID_OCEAN, deepOne, false);
                    if (pathTo == null || pathTo.Length < 2)
                    {
                        unit.task = null;
                        return;
                    }
                    if (otherPath.Length < pathTo.Length)
                    {
                        pathTo = otherPath;
                        target = pathTo[pathTo.Length - 1];
                    }
                }
                else
                {
                    pathTo = Pathfinding.getPathTo(unit.location, delegate_VALID_OCEAN, deepOne, false);
                    if (pathTo == null || pathTo.Length < 2)
                    {
                        unit.task = null;
                        return;
                    }
                    target = pathTo[pathTo.Length - 1];
                }

                deepOne.map.adjacentMoveTo(unit, pathTo[1]);
                deepOne.movesTaken++;

                if (unit.isCommandable())
                {
                    EventManager.onEnthralledUnitMove(unit.location.map, unit);
                    foreach (Property property in unit.location.properties)
                    {
                        if (property is Pr_DeepOneCult)
                        {
                            unit.map.hintSystem.popHint(HintSystem.hintType.DEEP_ONES);
                        }
                        else if (property is Pr_ArcaneSecret)
                        {
                            unit.map.hintSystem.popHint(HintSystem.hintType.MAGIC);
                        }
                    }
                }

                if (deepOne.location.isOcean)
                {
                    deepOne.moveType = Unit.MoveType.AQUAPHIBIOUS;

                    if (unit.isCommandable())
                    {
                        unit.map.addUnifiedMessage(unit, target, "Returned To The Deep", $"{unit.getName()} has plunged beneath the waves at {unit.location.getName()}. Now that their gills are full, and their skin wet, they will never again traverse the dryness of land.", "RETURNED TO THE DEEP", false);
                    }

                    deepOne.task = null;
                    return;
                }
            }
        }

        public override Location getLocation()
        {
            return target;
        }
    }
}
