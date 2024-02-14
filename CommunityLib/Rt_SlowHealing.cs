using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Rt_SlowHealing : Ritual
    {
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
            int result = 1;
            msgs?.Add(new ReasonMsg("Base", 1));

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
            if (ua.hp < ua.maxHp)
            {
                utility = 100.0;
                msgs?.Add(new ReasonMsg("HP Losses", utility));
            }

            return utility;
        }

        public override int getSimplificationLevel()
        {
            return 0;
        }

        public override void turnTick(UA ua)
        {
            counter++;
            if (counter >= 3)
            {
                counter = 0;

                ua.hp++;
                if (ua.hp >= ua.maxHp)
                {
                    ua.hp = ua.maxHp;
                    ua.task = null;

                    if (ua.isCommandable())
                    {
                        map.addMessage(ua.getName() + " completes: " + getName(), map.param.ch_laylow_parameterValue5, true, ua.location.hex);
                        popCompletionMessage(ua);
                    }
                }
            }
        }

        public override void complete(UA u)
        {
            base.complete(u);
        }
    }
}
