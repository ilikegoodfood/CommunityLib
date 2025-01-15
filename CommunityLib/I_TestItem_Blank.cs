using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class I_TestItem_Blank : Item
    {
        public I_TestItem_Blank(Map map)
            : base(map)
        {

        }

        public override string getName()
        {
            return "Blank Test Item";
        }

        public override string getShortDesc()
        {
            return "A test item used for devlopement. It  has not in-game uses.";
        }

        public override Sprite getIconFore()
        {
            return map.world.iconStore.standardBack;
        }
    }
}
