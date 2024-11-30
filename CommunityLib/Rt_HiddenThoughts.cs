using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public  class Rt_HiddenThoughts : Ritual
    {
        public Rt_HiddenThoughts(Location location)
            : base(location)
        {

        }

        public override string getName()
        {
            return "Veiled Thoughts";
        }

        public override string getDesc()
        {
            return "The thoughts of this agent are hidden from your sight";
        }

        public override Sprite getSprite()
        {
            return location.map.world.iconStore.arcaneFortress;
        }

        public override challengeStat getChallengeType()
        {
            return Challenge.challengeStat.OTHER;
        }

        public override bool valid()
        {
            return false;
        }
    }
}
