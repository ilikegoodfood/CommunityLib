using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Task_GoToWilderness : Assets.Code.Task
    {
        public bool goToLand;

        public bool safeMove;

        public Location target = null;

        public Task_GoToWilderness(Location targetLocation, bool safeMove = false)
        {
            goToLand = false;
            this.safeMove = safeMove;
        }

        public Task_GoToWilderness(bool goToLandOnly = false, bool safeMove = false)
        {
            goToLand = goToLandOnly;
            this.safeMove = safeMove;
        }

        public override string getShort()
        {
            return "Travelling to Wilderness";
        }

        public override string getLong()
        {
            return "This unit is travelling to the wilderness" + (target != null ? " (" + target.getName() + ")" : "");
        }

        public override void turnTick(Unit unit)
        {
            if (unit.location.soc == null)
            {
                unit.task = null;
                return;
            }

            if (target == null || target.soc != null)
            {
                target = null;
                int steps = -1;
                List<Location> targetLocations = new List<Location>();
                foreach (Location loc in unit.map.locations)
                {
                    if (loc.soc == null && (!goToLand || !loc.isOcean))
                    {
                        int stepDistance = unit.map.getStepDist(unit.location, loc);
                        if (loc != unit.location)
                        {
                            Location[] pathTo = unit.map.getPathTo(unit.location, loc, unit);
                            if (pathTo == null || pathTo.Length < 2)
                            {
                                continue;
                            }
                            stepDistance = pathTo.Length;
                        }

                        if (steps == -1 || stepDistance <= steps)
                        {
                            if (stepDistance < steps)
                            {
                                targetLocations.Clear();
                            }

                            targetLocations.Add(loc);
                            steps = stepDistance;
                        }
                    }
                }


                if (targetLocations.Count == 1)
                {
                    target = targetLocations[0];
                }
                else if (targetLocations.Count > 1)
                {
                    target = targetLocations[(targetLocations.Count)];
                }
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo;
                if (target == null)
                {
                    pathTo = unit.map.getPathTo(unit.location, (SocialGroup)null, unit, safeMove);
                }
                else
                {
                    pathTo = unit.map.getPathTo(unit.location, target, unit, safeMove);
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    unit.task = null;
                    break;
                }

                unit.location.map.adjacentMoveTo(unit, pathTo[1]);
                unit.movesTaken++;
                if (unit.location == target)
                {
                    unit.task = null;
                    break;
                }
            }
        }

        public override bool isBusy()
        {
            return false;
        }
    }
}