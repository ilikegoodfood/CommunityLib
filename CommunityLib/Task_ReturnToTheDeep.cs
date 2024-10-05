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

        public int[] mapLayers;

        public Task_ReturnToTheDeep(Location location)
        {
            target = location;
            mapLayers = new int[0];
        }

        public Task_ReturnToTheDeep(int mapLayer = -1)
        {
            target = null;

            if (mapLayer == -1)
            {
                mapLayers = new int[0];
            }
            else
            {
                mapLayers = new int[1] { mapLayer };
            }
        }

        public Task_ReturnToTheDeep(int[] mapLayers = null)
        {
            target = null;

            if (mapLayers == null)
            {
                mapLayers = new int[0];
            }

            this.mapLayers = mapLayers;
        }


        public static bool delegate_VALID_OCEAN(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
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

            Location[] pathTo = Pathfinding.getPathTo(unit.location, delegate_VALID_OCEAN, deepOne, null, false);
            if (pathTo == null || pathTo.Length < 2)
            {
                deepOne.die(deepOne.map, "Unable to reach the ocean");
                return;
            }
            target = pathTo[pathTo.Length - 1];

            int index = 1;
            while (unit.movesTaken < unit.getMaxMoves())
            {
                deepOne.map.adjacentMoveTo(unit, pathTo[index]);
                deepOne.movesTaken++;
                index++;

                if (deepOne.location == target)
                {
                    deepOne.moveType = Unit.MoveType.AQUAPHIBIOUS;
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
