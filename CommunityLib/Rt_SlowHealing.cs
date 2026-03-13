using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityLib
{
    public class Rt_SlowHealing : Ritual
    {
        public double PreciseCount = 0.0;

        public int counter = 0;

        public Rt_SlowHealing(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Slow Healing";
        }

        public override string getDesc()
        {
            return "Heal 1 hp every 3 turns.";
        }

        public override string getCastFlavour()
        {
            return "Allow the slow passage of time to heal your wounds.";
        }

        public override string getRestriction()
        {
            return "Unit's hp must be below it's maximum hp.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 1.0;
            msgs?.Add(new ReasonMsg("Base", 1.0));

            return result;
        }

        public override double getComplexity()
        {
            return map.param.ch_laylow_complexity;
        }

        public override bool isIndefinite()
        {
            return true;
        }

        public override Sprite getSprite()
        {
            return this.map.world.iconStore.restAndRearm;
        }

        public override int isGoodTernary()
        {
            return 0;
        }

        public override bool valid()
        {
            return true;
        }

        public override bool validFor(UA ua)
        {
            return ua.location.index == ua.homeLocation && ua.hp < ua.maxHp;
        }

        public override double getUtility(UA ua, List<ReasonMsg> msgs)
        {
            double utility = 0.0;
            double val = 0.0;
            if (ua.hp < ua.maxHp)
            {
                val = 1.0 - ((double)ua.hp / (double)ua.maxHp);
                val *= map.param.utility_UA_heal * 2;
                msgs?.Add(new ReasonMsg("HP Losses", val));
                utility += val;
            }

            int minionHp = 0;
            int minionMaxHp = 0;
            for (int i = 0; i < ua.minions.Length; i++)
            {
                if (ua.minions[i] != null)
                {
                    minionHp += ua.minions[i].hp;
                    minionMaxHp += ua.minions[i].getMaxHP();
                }
            }

            if (minionHp < minionMaxHp)
            {
                val = 1.0 - ((double)minionHp / (double)minionMaxHp);
                val *= map.param.utility_UA_heal * 2;
                msgs?.Add(new ReasonMsg("Minion HP Losses", val));
            }

            return utility;
        }

        public override int getSimplificationLevel()
        {
            return 0;
        }

        public double progressTracker = 0.0;

        public override void onBegin(Unit unit)
        {
            progressTracker = 0.0;
        }

        public override void turnTick(UA ua)
        {
            double oldProgress = progressTracker; // Get oldProgress from tracker
            double newProgress = (ua.task as Task_PerformChallenge)?.progress ?? 0.0; // Get new progress from agent's task, including a null check.
            double progressMade = newProgress - oldProgress; // Indefinite challenges don't need progress scaling.

            if (!isIndefinite()) // If it's not indeifnite, we need progress scaling.
            {
                double complexityAfterDifficulty = Math.Ceiling(getComplexityAfterDifficulty()); // Get the complexity after difficulty.

                double expectedProgress = getProgressPerTurn(ua, null);
                if (newProgress < oldProgress + expectedProgress) // Something has reduced the progress between the last turn and this turn.
                {
                    progressTracker = newProgress;
                    progressMade = expectedProgress; // Use an estimate.
                }
                else
                {
                    if (newProgress >= complexityAfterDifficulty) // Check if this completes the challenge
                    {

                        newProgress = complexityAfterDifficulty; // clamp the new progress.
                        progressTracker = 0.0;
                    }

                    progressMade = newProgress - oldProgress; // recalculate the progress made.
                }
                
                progressMade /= getComplexity() / complexityAfterDifficulty; // Scale the progress made to turn it back into base complexity progress
            }

            PreciseCount += progressMade; // Use that instead of getProgressPerTurnInner to determine the strength of the effect.

            int singleProgresses = (int)PreciseCount;
            PreciseCount -= singleProgresses;
            counter += singleProgresses;
            int tripleProgresses = counter / 3;
            counter -= tripleProgresses * 3;

            int minionHp = 0;
            int minionMaxHp = 0;
            for (int i = 0; i < ua.minions.Length; i++)
            {
                if (ua.minions[i] != null)
                {
                    Minion minion = ua.minions[i];
                    int maxHp = minion.getMaxHP();
                    if (minion.hp < maxHp)
                    {
                        minion.hp += singleProgresses;

                        if (minion.hp > maxHp)
                        {
                            minion.hp = maxHp;
                        }
                    }

                    minionHp += minion.hp;
                    minionMaxHp = maxHp;
                }
            }

            if (tripleProgresses > 0)
            {
                if (ua.hp < ua.maxHp)
                {
                    ua.hp += tripleProgresses;

                    if (ua.hp > ua.maxHp)
                    {
                        ua.hp = ua.maxHp;
                    }
                }

                if (ua.hp >= ua.maxHp && minionHp >= minionMaxHp)
                {
                    ua.task = null;

                    if (ua.isCommandable())
                    {
                        map.addMessage(ua.getName() + " completes: " + getName(), map.param.ch_laylow_parameterValue5, true, ua.location.hex);
                        popCompletionMessage(ua);
                    }
                    return;
                }
            }

            if (ua.hp >= ua.maxHp && minionHp >= minionMaxHp)
            {
                ua.hp = ua.maxHp;
                ua.task = null;

                if (ua.isCommandable())
                {
                    map.addMessage(ua.getName() + " completes: " + getName(), map.param.ch_laylow_parameterValue5, true, ua.location.hex);
                    popCompletionMessage(ua);
                    return;
                }
            }
        }

        public override void complete(UA u)
        {
            base.complete(u);
        }
    }
}
