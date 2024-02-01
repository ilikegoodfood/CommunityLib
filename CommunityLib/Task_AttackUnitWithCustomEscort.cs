using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Task_AttackUnitWithCustomEscort : Task_AttackUnitWithEscort
    {
        public UM customEscort;

        public Task_AttackUnitWithCustomEscort(Unit self, Unit c, UM escort)
            : base(self, c, escort as UM_CavalryEscort)
        {
            customEscort = escort;
        }

        public override string getLong()
        {
            return "This agent is off to attack [" + target.getName() + "] at " + target.location.getName() + ", they have " + turnsRemaining + " turns left until they lose the trail. They are escorted by a" + customEscort.getName() + ", making them near unbeatable in combat";
        }

        public override void turnTick(Unit unit)
        {
            if (target.isDead || target.location == null || !target.location.units.Contains(target) || target == unit)
            {
                unit.task = null;
                return;
            }

            turnsRemaining--;
            if (turnsRemaining <= 0)
            {
                unit.task = null;
                return;
            }

            if (unit.location == target.location)
            {
                if (target.engagedBy == null && target.turnLastEngaged != unit.map.turn)
                {
                    UA ua = unit as UA;
                    UA targetUA = target as UA;

                    if (ua != null && targetUA != null)
                    {
                        if (target.isCommandable())
                        {
                            if (unit.map.automatic)
                            {
                                BattleAgents battleAgents = new BattleAgents(ua, targetUA);
                                battleAgents.automatic();
                            }
                            else
                            {
                                unit.engaging = target;
                                target.engagedBy = unit;
                                unit.turnLastEngaged = unit.map.turn;
                                target.turnLastEngaged = unit.map.turn;
                            }
                        }
                        else
                        {
                            BattleAgents battleAgents = new BattleAgents(ua, targetUA);
                            battleAgents.automatic();
                        }
                    }

                    unit.task = null;
                }
            }
            else
            {
                int maxSteps = Math.Min(unit.getMaxMoves(), customEscort.getMaxMoves());
                int stepsTaken = 0;

                while (stepsTaken < maxSteps)
                {
                    stepsTaken++;

                    if (unit.map.moveTowards(unit, target.location))
                    {
                        escort.map.adjacentMoveTo(escort, unit.location);
                    }
                    else
                    {
                        World.log("Move unsuccessful. Cancelling go to challenge");
                        unit.task = null;
                        return;
                    }
                }
            }
        }
    }
}
