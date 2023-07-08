using Assets.Code;
using System;

namespace CommunityLib
{
    public class Task_GoToPerformChallengeAtLocation : Task_GoToPerformChallenge
    {
        public Location target;
        public bool safeMove;
        public Func<Location[], Location, Unit, bool> pathfindingDelegate;

        public Task_GoToPerformChallengeAtLocation(Challenge c, Location loc, bool safeMove = false)
            : base (c)
        {
            target = loc;
            this.safeMove = safeMove;
            
        }

        public Task_GoToPerformChallengeAtLocation(Challenge c, Location loc, Func<Location[], Location, Unit, bool> pathfindingDelegate)
            : base (c)
        {
            target = loc;
            safeMove = false;
            this.pathfindingDelegate = pathfindingDelegate;
        }

        public override string getLong()
        {
            return "This agent is off to perform challenge [" + challenge.getName() + "] at " + target.getName();
        }

        public override void turnTick(Unit unit)
        {
            if (!(challenge is Ritual))
            {
                if (challenge.location != target)
                {
                    World.log("Location of challenge (" + challenge.getName() + ") does not match target location (" + target.getName() + "). Cancelling");
                    unit.task = null;
                    return;
                }

                if (!challenge.location.GetChallenges().Contains(challenge))
                {
                    World.log("Challenge (" + challenge.getName() + ") is gone. Cancelling");
                    unit.task = null;
                    return;
                }
            }

            if (!challenge.allowMultipleUsers() && challenge.claimedBy != null && challenge.claimedBy != unit)
            {
                unit.task = null;
            }

            if (unit.map.automatic && (challenge is Ch_FulfillTheProphecy || challenge is Ch_ReforgeTheSeals))
            {
                foreach (Unit unit2 in unit.map.overmind.agents)
                {
                    UA ua = unit2 as UA;
                    if (ua != null)
                    {
                        if (!(ua.task is Task_AttackUnit))
                        {
                            ua.task = new Task_AttackUnit(ua, unit);
                        }
                    }
                }
            }

            if (unit.location == target)
            {
                startChallenge(unit, challenge);
                return;
            }

            while (unit.movesTaken < unit.getMaxMoves())
            {
                Location[] pathTo;
                if (pathfindingDelegate == null)
                {
                    pathTo = unit.location.map.getPathTo(unit.location, target, unit, safeMove);
                }
                else
                {
                    pathTo = ModCore.core.pathfinding.getPathTo(unit.location, target, pathfindingDelegate, unit);
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    World.log("Path unavailable. Cancelling");
                    unit.task = null;
                    return;
                }

                unit.map.adjacentMoveTo(unit, pathTo[1]);
                if (unit.location != pathTo[1])
                {
                    World.log("Move unsuccessful. Cancelling");
                    unit.task = null;
                    break;
                }
                unit.movesTaken++;

                if (unit.location == target)
                {
                    startChallenge(unit, challenge);
                    return;
                }
            }
        }

        public void startChallenge(Unit unit, Challenge challenge)
        {
            if (challenge.allowMultipleUsers() || challenge.claimedBy == null || challenge.claimedBy == unit || challenge.claimedBy.location != target)
            {
                if (challenge.valid())
                {
                    if (unit is UM um && challenge.validFor(um))
                    {
                        if (unit.isCommandable() || target.isWatched)
                        {
                            unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " and begun challenge " + challenge.getName(), UnifiedMessage.messageType.UNIT_ARRIVES, false);
                        }

                        unit.task = new Task_PerformChallenge(challenge);
                        challenge.claimedBy = unit;
                    }
                    else if (unit is UA ua && challenge.validFor(ua))
                    {
                        if (unit.isCommandable() || target.isWatched)
                        {
                            unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " and begun challenge " + challenge.getName(), UnifiedMessage.messageType.UNIT_ARRIVES, false);
                        }

                        unit.task = new Task_PerformChallenge(challenge);
                        challenge.claimedBy = unit;
                    }
                    else
                    {
                        if (unit.isCommandable() || target.isWatched)
                        {
                            unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " but could not perform challenge " + challenge.getName() + " because " + unit.getName() + " does not meat the rquirements to start the challenge.", UnifiedMessage.messageType.UNIT_ARRIVES, false);
                        }

                        unit.task = null;
                    }
                }
                else
                {
                    if (unit.isCommandable() || target.isWatched)
                    {
                        unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " but could not perform challenge " + challenge.getName() + " because the requirements to start this challenge have not yet been met.", UnifiedMessage.messageType.UNIT_ARRIVES, false);
                        unit.task = null;
                    }
                }

            }
            else
            {
                if (unit.isCommandable() || target.isWatched)
                {
                    unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " but could not perform challenge " + challenge.getName() + " because it is already being performed by " + challenge.claimedBy.getName() + ".", UnifiedMessage.messageType.UNIT_ARRIVES, false);
                    unit.task = null;
                }

                unit.task = null;
            }
        }

        public override Location getLocation()
        {
            return target;
        }
    }
}
