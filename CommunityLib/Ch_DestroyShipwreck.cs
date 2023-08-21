using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Ch_DestroyShipwreck : Challenge
    {
        public Sub_Shipwreck wreck;

        public Ch_DestroyShipwreck( Sub_Shipwreck wreck)
            : base (wreck.settlement.location)
        {
            this.wreck = wreck;
        }

        public override string getName()
        {
            return "Destroy Shipwreck";
        }

        public override string getDesc()
        {
            return "Destroy the " + wreck.getName() + ".";
        }

        public override string getCastFlavour()
        {
            return "Even half drowned in the sea, fire is a ship's worst enemy. Once set ablaze, their tarred wooden hulls, canvas sails and thick rope rigging is consumed greedily by the flames.";
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("CLib.Icon_BurningShip.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override double getComplexity()
        {
            return Math.Ceiling(map.param.ch_exploreRuinsTime / 2.0);
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 1.0;
            msgs?.Add(new ReasonMsg("Base", result));

            if (unit.isCommandable() && unit.location.getShadow() > unit.map.param.ch_exploreruins_parameterValue1)
            {
                result++;
                msgs?.Add(new ReasonMsg("Agent in Shadow", 1.0));
            }

            return result;
        }

        public override void complete(UA u)
        {
            wreck.removeWreck("Wreck destroyed by " + u.getName(), u);
        }

        public override void complete(UM u)
        {
            wreck.removeWreck("Wreck destroyed by " + u.getName(), u);
        }

        public override int[] buildNegativeTags()
        {
            return new int[]
            {
                Tags.GOLD
            };
        }
    }
}
