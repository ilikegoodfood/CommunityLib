using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static HarmonyLib.Code;

namespace CommunityLib
{
    public class Sub_Shipwreck : Subsettlement
    {
        public Location location;

        public double allure = 0.0;

        public double integrity;

        public double integrityLossPlunder = 15.0;

        public bool reinforced = false;

        public List<Challenge> challenges;

        public Sub_Shipwreck(Settlement set, Location loc)
            : base(set)
        {
            integrity = 40.0 + Eleven.random.Next(11) + Eleven.random.Next(11);
            location = loc;

            challenges = new List<Challenge>
            {
                new Ch_PlunderShipwreck(this),
                new Ch_RecoverShipwreck(this),
                new Ch_DestroyShipwreck(this),
                new Ch_LayLowWilderness(settlement.location)
            };

            if (loc.eventState.environment.ContainsKey("FOG_WRECK_ID"))
            {
                loc.eventState.environment["FOG_WRECK_ID"] = 0.ToString();
            }
        }

        public override string getName()
        {
            return "Wreck of " + settlement.location.shortName;
        }

        public override List<Challenge> getChallenges()
        {
            return challenges;
        }

        public override bool definesName()
        {
            if (settlement.subs.Any(sub => sub is Sub_AncientRuins))
            {
                return false;
            }

            return true;
        }

        public override bool definesSprite()
        {
            if (settlement.subs.Any(sub => sub is Sub_AncientRuins))
            {
                return false;
            }

            return true;
        }

        public override Sprite getLocationSprite(World world)
        {
            return EventManager.getImg("CLib.loc_Shipwreck.png");
        }

        public override string generateParentName()
        {
            return "Wreck of " + settlement.location.shortName;
        }

        public override string getIconText()
        {
            string result = "Integrity: " + Math.Round(integrity, 1) + "%";

            if (isReinforced())
            {
                result += " (reinforced)";
            }

            return result;
        }

        public override string getHoverOverText()
        {
            return "The wreck of a fleet of ships, damaged by storm and charred by fire. They continually shift and break up under the relentless motion of the waves. With funds and labour, some may be recoverable, or they could be ransacked for whatever riches lie forgotten within.";
        }

        public override Sprite getIcon()
        {
            return EventManager.getImg("CLib.Shipwreck.png");
        }

        public override bool canBeInfiltrated()
        {
            return false;
        }

        public override bool survivesRuin()
        {
            return true;
        }

        public override void turnTick()
        {
            if (!isReinforced())
            {
                integrity--;
            }

            if (integrity <= 0.0)
            {
                removeWreck();
            }
        }

        public bool isReinforced()
        {
            return reinforced;
        }

        public void removeWreck(string v = "Shipwreck Degraded to Nothing", object killer = null)
        {
            if (location.settlement != null)
            {
                location.settlement.subs.Remove(this);

                if (location.settlement is Set_Shipwreck)
                {
                    location.settlement.fallIntoRuin(v, killer);
                    location.settlement = null;
                }
            }
        }
    }
}
