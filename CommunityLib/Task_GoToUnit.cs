using Assets.Code;

namespace CommunityLib
{
    public class Task_GoToUnit : Task_Follow
    {
        public int maxTravelTime;

        public int arrivalCancellationDelay;

        public bool runAIImmediatelyOnArrival;

        public bool hasArrived;

        public Location targetLocation = null;

        public Task_GoToUnit(Unit self, Unit c, int maxTravelTime = -1, int arrivalCancellationDelay = 0, bool runAIImmediatelyOnArrival = false)
            : base(self, c)
        {
            this.maxTravelTime = maxTravelTime;
            this.arrivalCancellationDelay = arrivalCancellationDelay;
            this.runAIImmediatelyOnArrival = runAIImmediatelyOnArrival;
            hasArrived = false;
            targetLocation = target.location;
            if (ModCore.Get().checkIsUnitSubsumed(target) && target.person != null)
            {
                targetLocation = target.person.unit.location;
            }
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
                return $"Travelling to no-one.";
            }

            if (hasArrived && arrivalCancellationDelay > 0)
            {
                return $"Following {target.getName()} who is currently at {target.location.getName()} ({arrivalCancellationDelay} turns remaining).";
            }

            if (maxTravelTime > -1)
            {
                return $"Travelling to {target.getName()} at {target.location.getName()} ({maxTravelTime} turns remaining).";
            }

            return $"This unit is travelling to {target.getName()} at {target.location.getName()}.";
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

            if (target == null || (!ModCore.Get().checkIsUnitSubsumed(target) && target.isDead) || targetLocation == null)
            {
                unit.task = null;
                return;
            }

            if (maxTravelTime > -1)
            {
                maxTravelTime--;
                if (maxTravelTime == 0)
                {
                    unit.task = null;
                }
            }

            targetLocation = target.location;
            if (ModCore.Get().checkIsUnitSubsumed(target) && target.person != null)
            {
                targetLocation = target.person.unit.location;
            }

            if (unit.location == targetLocation)
            {
                hasArrived = true;
                if (runAIImmediatelyOnArrival)
                {
                    unit.turnTickAI();
                    return;
                }
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                targetLocation = target.location;
                if (ModCore.Get().checkIsUnitSubsumed(target) && target.person != null)
                {
                    targetLocation = target.person.unit.location;
                }

                Location[] path = Pathfinding.getPathTo(unit.location, targetLocation, unit, false);
                if (path == null || path.Length < 2)
                {
                    unit.task = null;
                    return;
                }

                unit.map.adjacentMoveTo(unit, path[1]);
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

                if (unit.location == targetLocation)
                {
                    if (!hasArrived && unit.isCommandable())
                    {
                        unit.map.addUnifiedMessage(unit, target, "Unit Arrives", $"{unit.getName()} has reached {target.getName()} at {targetLocation.getName()}{(arrivalCancellationDelay > 0 ? $", and will continue following them for {arrivalCancellationDelay} {(arrivalCancellationDelay == 1 ? "turn" : "turns")}." : ".")}", UnifiedMessage.messageType.UNIT_ARRIVES, false);
                    }
                    hasArrived = true;
                    
                    if (arrivalCancellationDelay <= 0)
                    {
                        unit.task = null;
                    }
                    if (runAIImmediatelyOnArrival)
                    {
                        unit.turnTickAI();
                        return;
                    }
                    break;
                }
            }
        }
    }
}
