using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Code.Challenge;
using UnityEngine;

namespace CommunityLib
{
    public class Ch_RecoverShipwreck : Challenge
    {
        public Sub_Shipwreck wreck;

        public double minCharge = 25.0;

        public int cost = 50;

        public Ch_RecoverShipwreck(Sub_Shipwreck wreck)
            : base(wreck.settlement.location)
        {
            this.wreck = wreck;
        }

        public override string getName()
        {
            return "Recover Shipwreck";
        }

        public override string getDesc()
        {
            return "Recover ships from the wreck, repair them, and transport them to an empty orc shipyard.";
        }

        public override string getRestriction()
        {
            return "Costs " + cost + " gold, requires the shipwreck to have a minimum charge of " + minCharge + " and to not have been reinforced, and an orc horde to have an empty shipyard.";
        }

        public override string getCastFlavour()
        {
            return "Teams of labourers scurry over the wrecked hulls, cranes strain in the wind, vast piles of lumber and waterproofing tar rest on the beaches nearby. Through dangerous physical labour, and skilled craftsmanship, these once great vessels will be hauled from the depths, and returned to their rightful place atop the waves.";
        }

        public override double getProfile()
        {
            return 15.0;
        }

        public override double getMenace()
        {
            return 10.0;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("CLib.Shipwreck.png");
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override bool valid()
        {
            if (!wreck.isReinforced() && wreck.integrity >= minCharge)
            {
                foreach (Location loc in location.map.locations)
                {
                    if (loc.settlement is Set_OrcCamp camp && camp.specialism == 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool validFor(UA ua)
        {
            return ua.person.getGold() >= cost;
        }

        public override double getComplexity()
        {
            return 10;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            double result = 1.0;
            msgs?.Add(new ReasonMsg("Base", result));

            return result;
        }

        public override int getDanger()
        {
            return 4;
        }

        public override void complete(UA u)
        {
            List<Set_OrcCamp> emptyShipyards = new List<Set_OrcCamp>();

            foreach (Location loc in u.map.locations)
            {
                if (loc.settlement is Set_OrcCamp camp && camp.specialism == 4)
                {
                    emptyShipyards.Add(camp);
                }
            }

            if (emptyShipyards.Count > 0)
            {
                Set_OrcCamp emptyShipyard = emptyShipyards[0];
                if (emptyShipyards.Count > 1)
                {
                    emptyShipyard = emptyShipyards[Eleven.random.Next(emptyShipyards.Count)];
                }

                emptyShipyard.specialism = 5;
                msgString = "Ships from the " + wreck.getName() + " have been restored and sailed to the orc shipyard at " + emptyShipyard.location.shortName + ".";
                wreck.integrity = 0.0;
                wreck.removeWreck();
            }
        }

        public override int getCompletionMenace()
        {
            return 5;
        }

        public override int getCompletionProfile()
        {
            return 10;
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.ORC
            };
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
