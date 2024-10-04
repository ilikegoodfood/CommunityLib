using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class UM_DwarvenSettlers : UM_Refugees
    {
        public SettlementHuman homeSettlement;

        public UM_DwarvenSettlers(Location loc, SocialGroup sg, int maxHP, SettlementHuman setHuman)
            : base (loc, sg, maxHP, setHuman)
        {
            homeSettlement = setHuman;
        }

        public override string getName()
        {
            return "Dwarven Settlers";
        }

        public override Sprite getPortraitForeground()
        {
            return EventManager.getImg("CLib.Foreground_DwarvenSettlers.jpg");
        }

        public override void turnTickInner(Map map)
        {
            if (hp < maxHp)
            {
                maxHp = hp;
            }
        }

        public override void turnTickAI()
        {
            UM_Refugees refugees = new UM_Refugees(location, society, maxHp, homeSettlement);
            refugees.shadow = shadow;

            map.units.Add(refugees);
            location.units.Add(refugees);

            map.units.Remove(this);
            location.units.Remove(this);
            disband(map, "Failed Settlers became Refugees");
        }
    }
}
