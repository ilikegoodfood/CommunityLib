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

        public int[] mapLayers;

        public Task_GoToWilderness(Location targetLocation, bool safeMove = false)
        {
            goToLand = false;
            this.safeMove = safeMove;
            mapLayers = new int[0];
        }

        public Task_GoToWilderness(bool goToLandOnly = false, int mapLayer = -1, bool safeMove = false)
        {
            goToLand = goToLandOnly;
            this.safeMove = safeMove;

            if (mapLayer < 0)
            {
                mapLayers = new int[0];
            }
            else
            {
                mapLayers = new int[1] { mapLayer };
            }
        }

        public Task_GoToWilderness(bool goToLandOnly = false, int[] mapLayers = null, bool safeMove = false)
        {
            this.goToLand = goToLandOnly;
            this.safeMove = safeMove;

            if (mapLayers == null)
            {
                mapLayers = new int[0];
            }

            this.mapLayers = mapLayers;
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

            Location[] pathTo = Pathfinding.getPathTo(unit.location, (SocialGroup)null, unit, mapLayers.ToList(), safeMove);
            if (pathTo == null || pathTo.Length < 2)
            {
                unit.task = null;
                return;
            }
            target = pathTo[pathTo.Length - 1];

            int index = 1;
            while (unit.movesTaken < unit.getMaxMoves())
            {
                unit.location.map.adjacentMoveTo(unit, pathTo[index]);
                unit.movesTaken++;
                index++;

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

        public override Location getLocation()
        {
            return target;
        }
    }
}
