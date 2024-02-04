using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class I_Test_DeathSave : Item
    {
        public I_Test_DeathSave(Map map)
            :base(map)
        {

        }

        public override string getName()
        {
            return "Potion of Perfect Healing";
        }

        public override string getShortDesc()
        {
            return "A powerful potion that can be drunk to save oneself from any form of death.\n\nThis item is intended as a test article. You should not be seeing it in a normal game.";
        }

        public override Sprite getIconFore()
        {
            return map.world.iconStore.i_potionOfHealing;
        }

        public override int getLevel()
        {
            return Item.LEVEL_NODROP;
        }
    }
}
