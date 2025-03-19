using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public static class ConsoleCommands
    {
        public static void parseCommand(string command, Map map)
        {
            string lowerCaseCommnand = command.ToLower();
            string[] commandComps = lowerCaseCommnand.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (commandComps.Length > 0)
            {
                if (commandComps[0] == "link_locations")
                {
                    if (commandComps.Length == 3 && int.TryParse(commandComps[1], out int val1) && int.TryParse(commandComps[2], out int val2))
                    {
                        cheat_LinkLocations(map, val1, val2);
                    }
                }
                else if (commandComps[0] == "influenceelder")
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
                else if (commandComps[0] == "influencehuman")
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
                else if (commandComps[0] == "blazingsun")
                {
                    foreach(Hex[] array in map.grid)
                    {
                        foreach(Hex hex in array)
                        {
                            if (hex.territoryOf != -1 && hex.territoryOf < map.locations.Count)
                            {
                                Location owner = map.locations[hex.territoryOf];

                                if (owner == null || owner.settlement == null)
                                {
                                    if (hex.location == null || hex.location.settlement == null)
                                    {
                                        hex.purity = 1.0f;
                                    }
                                    else
                                    {
                                        hex.location.settlement.shadow = 0.0f;
                                    }
                                }
                                else
                                {
                                    if (owner.settlement is Set_DeepOneAbyssalCity || owner.settlement is Set_DeepOneSanctum || owner.settlement is Set_TombOfGods || (owner.soc != null && owner.soc.isDark() || (owner.soc is Society society && (society.isDarkEmpire || society.isOphanimControlled))))
                                    {
                                        continue;
                                    }

                                    if (hex.location == null || hex.location.settlement == null)
                                    {
                                        hex.purity = 1.0f;
                                    }
                                    else
                                    {
                                        hex.location.settlement.shadow = 0.0f;
                                    }
                                }
                            }
                        }
                    }

                    foreach (Person person in map.persons)
                    {
                        if (!person.isDead && person.society is Society society && !society.isDark() && !society.isDarkEmpire && !society.isOphanimControlled)
                        {
                            person.shadow = 0.0f;
                        }
                    }
                }
                else if (commandComps[0] == "testitem")
                {
                    Item item = new I_TestItem_Blank(map);
                    if (commandComps.Length > 1)
                    {
                        if (commandComps[1] == "blank")
                        {
                            item = new I_TestItem_Blank(map);
                        }
                        else if (commandComps[1] == "nodeath")
                        {
                            item = new I_Test_DeathSave(map);
                        }
                    }

                    Unit selectedUnit = GraphicalMap.selectedUnit;
                    if (selectedUnit != null)
                    {
                        if (selectedUnit.person != null)
                        {
                            int index = -1;
                            for (int i = 0; i < selectedUnit.person.items.Length; i++)
                            {
                                if (selectedUnit.person.items[i] == null)
                                {
                                    index = i;
                                    break;
                                }
                            }

                            if (index != -1)
                            {
                                selectedUnit.person.items[index] = item;
                            }
                            else if (selectedUnit.isCommandable())
                            {
                                selectedUnit.person.gainItem(item);
                            }
                            else
                            {
                                selectedUnit.person.items[0] = item;
                            }
                        }
                    }

                    Hex selectedHex = GraphicalMap.selectedHex;
                    if (selectedHex != null)
                    {
                        Location selectedLocation = selectedHex.location;
                        if (selectedLocation != null && selectedLocation.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                        {
                            int index = -1;
                            for (int i = 0; i < settlementHuman.ruler.items.Length; i++)
                            {
                                if (settlementHuman.ruler.items[i] == null)
                                {
                                    index = i;
                                    break;
                                }
                            }

                            if (index != -1)
                            {
                                settlementHuman.ruler.items[index] = item;
                            }
                            else
                            {
                                settlementHuman.ruler.items[0] = item;
                            }
                        }
                    }
                }
                else if (commandComps[0] == "debug")
                {
                    if (commandComps.Length > 0)
                    {
                        if (commandComps[1] == "multidimensionalarray")
                        {
                            DebugHelper helper = new DebugHelper();
                            helper.saveRoot = map;
                            helper.DebugScan();
                        }
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

            string linkMsg = "";
            if (!addLink)
            {
                if (removeLink)
                {
                    linkMsg = "The link connecting " + locA.getName() + " and " + locB.getName() + " have been severed";
                }
                else
                {
                    linkMsg = "The link connecting " + locA.getName() + " and " + locB.getName() + " could not be severed, as it was the last link connecting to one of them";
                }
            }
            else
            {
                linkMsg = locA.getName() + " and " + locB.getName() + " were linked together";
            }

            map.addUnifiedMessage(locA, locB, "Locations Linked", linkMsg, "Locations Liked");
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
                    bool playerCanInfluenceFlag = order.influenceElder >= order.influenceElderReq;

                    if (value == 0)
                    {
                        order.influenceElder = order.influenceElderReq;

                        ModCore.Get().AddInfluenceGainElder(order, new ReasonMsg("Console Command", order.influenceElderReq - order.influenceElder));
                    }
                    else
                    {
                        order.influenceElder += value;

                        ModCore.Get().AddInfluenceGainElder(order, new ReasonMsg("Console Command", value));

                        if (order.influenceElder < 0)
                        {
                            order.influenceElder = 0;
                        }
                        else if (order.influenceElder >= order.influenceElderReq)
                        {
                            order.influenceElder = order.influenceElderReq;
                        }
                    }

                    if (!playerCanInfluenceFlag)
                    {
                        order.map.addUnifiedMessage(order, null, "Can Influence Holy Order", "You have enough influence to change the tenets of " + order.getName() + ", via the holy order screen", UnifiedMessage.messageType.CAN_INFLUENCE_ORDER);
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        order.influenceHuman = order.influenceHumanReq;

                        ModCore.Get().AddInfluenceGainHuman(order, new ReasonMsg("Console Command", order.influenceHumanReq - order.influenceHuman));
                    }
                    else
                    {
                        order.influenceHuman += value;

                        ModCore.Get().AddInfluenceGainHuman(order, new ReasonMsg("Console Command", value));

                        if (order.influenceHuman < 0)
                        {
                            order.influenceHuman = 0;
                        }
                        else if (order.influenceHuman >= order.influenceHumanReq)
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
                ModCore.Get().spawnShipwreck(hex.location);
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
