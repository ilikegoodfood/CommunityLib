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

        public override string getShort()
        {
            return "Return to the Deep";
        }

        // Token: 0x060001BB RID: 443 RVA: 0x00008E1C File Offset: 0x0000701C
        public override string getLong()
        {
            return "This deep one is returning to the Sea at " + target.getName();
        }

        public override void turnTick(Unit unit)
        {
            if (!(unit is UAEN_DeepOne deepOne) || deepOne.moveType == Unit.MoveType.AQUAPHIBIOUS)
            {
                unit.task = null;
                return;
            }
            else
            {
                if (deepOne.location.isOcean)
                {
                    //Console.WriteLine("CommunityLib: DeepOne is at ocean location");
                    deepOne.moveType = Unit.MoveType.AQUAPHIBIOUS;
                    deepOne.task = null;
                    return;
                }

                if (target == null || !target.isOcean)
                {
                    target = null;
                    List<Location> nearbyOceanLocations = new List<Location>();
                    int distance = -1;
                    foreach (Location loc in deepOne.map.locations)
                    {
                        if (loc.isOcean)
                        {
                            int stepDistance = deepOne.map.getStepDist(deepOne.location, loc);
                            if (loc != deepOne.location)
                            {
                                Location[] pathTo = deepOne.map.getPathTo(deepOne.location, loc, deepOne);
                                if (pathTo == null || pathTo.Length < 2)
                                {
                                    continue;
                                }
                                stepDistance = pathTo.Length;
                            }

                            if (distance == -1 || stepDistance <= distance)
                            {
                                if (stepDistance < distance)
                                {
                                    nearbyOceanLocations.Clear();
                                }
                                nearbyOceanLocations.Add(loc);
                                distance = stepDistance;
                            }
                        }
                    }

                    if (nearbyOceanLocations.Count == 1)
                    {
                        target = nearbyOceanLocations[0];
                    }
                    else if (nearbyOceanLocations.Count > 1)
                    {
                        target = nearbyOceanLocations[Eleven.random.Next(nearbyOceanLocations.Count)];
                    }

                    if (target == null)
                    {
                        deepOne.die(deepOne.map, "Unable to reach the ocean");
                        return;
                    }
                }

                if (target != null)
                {
                    while (unit.movesTaken < unit.getMaxMoves())
                    {
                        Location[] pathTo = deepOne.map.getPathTo(deepOne.location, target, deepOne);
                        if (pathTo == null || pathTo.Length < 2)
                        {
                            deepOne.die(deepOne.map, "Unable to reach the ocean");
                            return;
                        }

                        deepOne.map.adjacentMoveTo(unit, pathTo[1]);
                        deepOne.movesTaken++;

                        if (deepOne.location == target)
                        {
                            deepOne.moveType = Unit.MoveType.AQUAPHIBIOUS;
                            deepOne.task = null;
                            return;
                        }
                    }
                }
            }
        }
    }
}