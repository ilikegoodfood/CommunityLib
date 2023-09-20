using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Rt_DeepOnes_TravelBeneath : Ritual
    {
        public Rt_DeepOnes_TravelBeneath(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Travel Beneath";
        }

        public override string getDesc()
        {
            return "Relocate to a random ocean location.";
        }

        public override string getCastFlavour()
        {
            return "Travel through the deepest depths, by paths unknown to those who live on land and ship, emerging in some distant waters.";
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.LORE;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            int result = unit.getStatLore();

            if (result < 1)
            {
                msgs?.Add(new ReasonMsg("BAse", 1));
                result = 1;
            }
            else
            {
                msgs?.Add(new ReasonMsg("Stat: Lore", result));
            }

            return result;
        }

        public override double getComplexity()
        {
            return 10.0;
        }

        public override Sprite getSprite()
        {
            return this.map.world.iconStore.deepOnes;
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
            return true;
        }

        public override int getCompletionMenace()
        {
            return 0;
        }

        public override int getSimplificationLevel()
        {
            return 0;
        }

        public override void complete(UA u)
        {
            List<Location> locations = u.map.locations.FindAll(l => l.isOcean && l != u.location).ToList();

            if (locations.Count == 0)
            {
                u.disband(u.map, "Disappeared beneath the waves");
                World.log("Trapped Deep One Disbanded - No other ocean locations on map");
            }
            else
            {
                int index = Eleven.random.Next(locations.Count);
                Location loc = locations[index];

                u.location.units.Remove(u);
                u.location = loc;
                loc.units.Add(u);
            }
        }
    }
}