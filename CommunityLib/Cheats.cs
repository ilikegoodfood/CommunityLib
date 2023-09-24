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
        public static void parseCheat(string command, Map map)
        {
            string[] commandComps = command.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (commandComps.Length > 0)
            {
                if (commandComps[0] == "link_locations")
                {
                    if (commandComps.Length == 3 && int.TryParse(commandComps[1], out int val1) && int.TryParse(commandComps[2], out int val2))
                    {
                        cheat_LinkLocations(map, val1, val2);
                    }
                }
                else if (commandComps[0] == "influenceElder")
                {
                    if (commandComps.Length == 1)
                    {
                        cheat_InfluenceHolyOrder(0, true);
                    }
                    else if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val))
                    {
                        cheat_InfluenceHolyOrder(val, true);
                    }
                }
                else if (commandComps[0] == "influenceHuman")
                {
                    if (commandComps.Length == 1)
                    {
                        cheat_InfluenceHolyOrder(0);
                    }
                    else if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val2))
                    {
                        cheat_InfluenceHolyOrder(val2);
                    }
                }
                else if (commandComps[0] == "shipwreck")
                {
                    cheat_Shipwreck();
                }
                else if (commandComps[0] == "gold")
                {
                    if (commandComps.Length == 2 && int.TryParse(commandComps[1], out int val3))
                    {
                        cheat_Gold(val3);
                    }
                }
            }
        }

        public static void cheat_LinkLocations(Map map, int indexA, int indexB)
        {
            if (indexA < 0)
            {
                indexA = 0;
            }
            else if (indexA >= map.locations.Count)
            {
                indexA -= map.locations.Count - 1;
            }

            if (indexB < 0)
            {
                indexB = 0;
            }
            else if (indexB >= map.locations.Count)
            {
                indexB -= map.locations.Count - 1;
            }

            Location locA = map.locations[indexA];
            Location locB = map.locations[indexB];
            Link linkA = locA.links.FirstOrDefault(l => l.other(locA) == locB);
            Link linkB = locB.links.FirstOrDefault(l => l.other(locB) == locA);
            bool addLink = true;
            bool removeLink = locA.links.Count > 1 && locB.links.Count > 1;

            if (linkA != null)
            {
                addLink = false;

                if (removeLink)
                {
                    locA.links.Remove(linkA);
                }
            }

            if (linkB != null)
            {
                addLink = false;

                if (removeLink)
                {
                    locB.links.Remove(linkB);
                }
            }

            if (addLink)
            {
                linkA = new Link(map, locA, locB);
                locA.links.Add(linkA);
                linkB = new Link(map, locB, locA);
                locB.links.Add(linkB);
            }

            GraphicalMap.purge();
            GraphicalMap.checkLoaded();
            map.recomputeStepDistMap();
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
