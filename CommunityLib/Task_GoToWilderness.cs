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

        public Task_GoToWilderness(bool goToLand = false, bool safeMove = false)
        {
            this.goToLand = goToLand;
            this.safeMove = safeMove;
        }

        public override string getShort()
        {
            return "Travelling to Wilderness";
        }

        public override string getLong()
        {
            return getShort();
        }

        public override void turnTick(Unit unit)
        {
            if (unit.location.soc == null)
            {
                unit.task = null;
                return;
            }

            Location targetLocation = null;
            if (goToLand)
            {
                int steps = -1;
                List<Location> targetLocations = new List<Location>();
                foreach (Location loc in unit.map.locations)
                {
                    if (loc.soc == null && !loc.isOcean)
                    {
                        int dist = unit.map.getStepDist(unit.location, loc);
                        if (steps == -1 || dist <= steps)
                        {
                            if (dist < steps)
                            {
                                targetLocations.Clear();
                            }

                            targetLocations.Add(loc);
                            steps = dist;
                        }
                    }
                }

                if (targetLocations.Count == 1)
                {
                    targetLocation = targetLocations[0];
                }
                else if (targetLocations.Count > 1)
                {
                    targetLocation = targetLocations[
                        (targetLocations.Count)];
                }
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo;
                if (targetLocation == null)
                {
                    pathTo = unit.map.getPathTo(unit.location, (SocialGroup)null, unit, safeMove);
                }
                else
                {
                    pathTo = unit.map.getPathTo(unit.location, targetLocation, unit, safeMove);
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    unit.task = null;
                    break;
                }

                unit.location.map.adjacentMoveTo(unit, pathTo[1]);
                unit.movesTaken++;
                if (unit.location.soc == null)
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
