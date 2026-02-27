using Assets.Code;
using System.Linq;

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

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo;
                if (target != null && target.soc == null)
                {
                    pathTo = Pathfinding.getPathTo(unit.location, target, unit, safeMove);

                    Location[] otherPath = Pathfinding.getPathTo(unit.location, (SocialGroup)null, unit, safeMove);
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
                    pathTo = Pathfinding.getPathTo(unit.location, (SocialGroup)null, unit, safeMove);
                    if (pathTo == null || pathTo.Length < 2)
                    {
                        unit.task = null;
                        return;
                    }
                    target = pathTo[pathTo.Length - 1];
                }

                unit.location.map.adjacentMoveTo(unit, pathTo[1]);
                unit.movesTaken++;

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

                if (unit.location.soc == null)
                {
                    if (unit.isCommandable())
                    {
                        unit.map.addUnifiedMessage(unit, target, "Unit Arrives", $"{unit.getName()} has reached the wilderness of {unit.location.getName(true)}.", UnifiedMessage.messageType.UNIT_ARRIVES, false);
                    }

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