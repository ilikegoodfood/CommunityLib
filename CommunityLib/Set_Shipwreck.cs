using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Set_Shipwreck : Set_CityRuins
    {
        public Set_Shipwreck(Location location)
            : base(location)
        {
            name = "Wreck of " + location.shortName;

            subs.Add(new Sub_Shipwreck(this, location));
        }

        public override Sprite getSprite()
        {
            return EventManager.getImg("CLib.loc_Shipwreck.png");
        }
    }
}
