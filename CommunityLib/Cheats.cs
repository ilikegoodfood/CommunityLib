using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Cheats
    {
        public static void parseCheat(string command)
        {
            string[] commandComps = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (commandComps.Length > 0)
            {
                switch (commandComps[0])
                {
                    case "influenceElder":
                        if (commandComps.Length == 1)
                        {
                            cheat_InfluenceHolyOrder(0, true);
                        }
                        else if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val))
                        {
                            cheat_InfluenceHolyOrder(val, true);
                        }
                        break;
                    case "influenceHuman":
                        if (commandComps.Length == 1)
                        {
                            cheat_InfluenceHolyOrder(0);
                        }
                        else if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val2))
                        {
                            cheat_InfluenceHolyOrder(val2);
                        }
                        break;
                    case "shipwreck":
                        cheat_Shipwreck();
                        break;
                    case "gold":
                        if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val3))
                        {
                            cheat_Gold(val3);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public static void cheat_InfluenceHolyOrder(int value, bool isElder = false)
        {
            HolyOrder order = null;

            if (GraphicalMap.selectedUnit != null)
            {
                Unit unit = GraphicalMap.selectedUnit;
                order = unit.society as HolyOrder;
            }
            else if (GraphicalMap.selectedHex != null && GraphicalMap.selectedHex.location != null)
            {
                Location loc = GraphicalMap.selectedHex.location;
                order = loc.soc as HolyOrder;

                if (order == null && loc.settlement != null)
                {
                    if (loc.settlement is SettlementHuman settlementHuman)
                    {
                        order = settlementHuman.order;
                    }

                    if (order == null)
                    {
                        foreach (Subsettlement sub in loc.settlement.subs)
                        {
                            if (sub is Sub_Temple temple)
                            {
                                order = temple.order;
                            }
                        }
                    }
                }
            }

            if (order != null)
            {
                if (isElder)
                {
                    if (value == 0)
                    {
                        order.influenceElder = order.influenceElderReq;
                    }
                    else
                    {
                        order.influenceElder += value;

                        if (order.influenceElder < 0)
                        {
                            order.influenceElder = 0;
                        }
                        else if (order.influenceElder > order.influenceElderReq)
                        {
                            order.influenceElder = order.influenceElderReq;
                        }
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        order.influenceHuman = order.influenceHumanReq;
                    }
                    else
                    {
                        order.influenceHuman += value;

                        if (order.influenceHuman < 0)
                        {
                            order.influenceHuman = 0;
                        }
                        else if (order.influenceHuman > order.influenceHumanReq)
                        {
                            order.influenceHuman = order.influenceHumanReq;
                        }
                    }
                }
            }
        }

        public static void cheat_Shipwreck()
        {
            Hex hex = GraphicalMap.selectedHex;

            if (hex != null && hex.location != null && (hex.location.isOcean || hex.location.isCoastal))
            {
                ModCore.core.spawnShipwreck(hex.location);
            }
        }

        public static void cheat_Gold(int gold)
        {
            Hex hex = GraphicalMap.selectedHex;

            if (hex != null && hex.location != null)
            {
                if (hex.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                {
                    settlementHuman.ruler.gold += gold;
                    if (settlementHuman.ruler.gold < 0)
                    {
                        settlementHuman.ruler.gold = 0;
                    }
                    return;
                }
            }

            Unit u = GraphicalMap.selectedUnit;
            if (u != null && u is UA ua && ua.person != null)
            {
                ua.person.gold += gold;
                if (ua.person.gold < 0)
                {
                    ua.person.gold = 0;
                }
                return;
            }
        }
    }
}
