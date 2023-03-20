﻿using Assets.Code;

namespace CommunityLib
{
    public class Task_GoToPerformChallengeAtLocation : Task_GoToPerformChallenge
    {
        public Location target;
        public bool safeMove;
        public int steps = 0;

        public Task_GoToPerformChallengeAtLocation(Challenge c, Location loc, bool safeMove = false)
            : base (c)
        {
            target = loc;
            this.safeMove = safeMove;
            
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

            if (!challenge.allowMultipleUsers() && !(challenge.claimedBy == null || challenge.claimedBy == unit))
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
                if (unit.isCommandable() || target.isWatched)
                {
                    unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " and begun challenge " + challenge.getName(), UnifiedMessage.messageType.UNIT_ARRIVES, false);
                }
                
                if (challenge.allowMultipleUsers() || challenge.claimedBy == null || challenge.claimedBy == unit)
                {
                    unit.task = new Task_PerformChallenge(challenge);
                    if (!challenge.allowMultipleUsers())
                    {
                        challenge.claimedBy = unit;
                    }
                }
                else
                {
                    unit.task = null;
                }
            }
            else
            {
                while (unit.movesTaken < unit.getMaxMoves())
                {
                    Location[] pathTo = unit.location.map.getPathTo(unit.location, target, unit, safeMove);

                    if (pathTo == null || pathTo.Length < 2)
                    {
                        unit.task = null;
                        return;
                    }

                    unit.location.map.adjacentMoveTo(unit, pathTo[1]);
                    unit.movesTaken++;
                    steps++;

                    if (!unit.map.moveTowards(unit, target))
                    {
                        World.log("Move unsuccessful. Cancelling");
                        unit.task = null;
                        break;
                    }

                    if (unit.location == target)
                    {
                        if (unit.isCommandable() || target.isWatched)
                        {
                            unit.map.addUnifiedMessage(unit, null, "Unit Arrives", unit.getName() + " has reached " + unit.location.getName(true) + " and begun challenge " + challenge.getName(), UnifiedMessage.messageType.UNIT_ARRIVES, false);
                        }
                        if (challenge.allowMultipleUsers() || challenge.claimedBy == null || challenge.claimedBy == unit)
                        {
                            unit.task = new Task_PerformChallenge(challenge);
                            if (!challenge.allowMultipleUsers())
                            {
                                challenge.claimedBy = unit;
                            }
                        }
                        else
                        {
                            unit.task = null;
                        }
                        break;
                    }
                }
            }
        }

        public override Location getLocation()
        {
            return target;
        }
    }
}
