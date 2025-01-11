using Assets.Code;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace CommunityLib
{
    internal static class ConsoleCommandOverrides
    {
        public static bool overrideCommand(Map map, string command)
        {
            string caselessCommand = command.ToLower();

            switch (caselessCommand)
            {
                case "ghast":
                    Ghast(map);
                    return false;
                case "ravenous":
                    Ravenous(map);
                    return false;
                case "shadow":
                    Shadow(map, 100);
                    return false;
                case "halfshadow":
                    Shadow(map, 50);
                    return false;
                case "99shadow":
                    Shadow(map, 99);
                    return false;
                case "forceshadow":
                    Shadow(map, 100, true);
                    return false;
                case "forcehalfshadow":
                    Shadow(map, 50, true);
                    return false;
                case "force99shadow":
                    Shadow(map, 99, true);
                    return false;
                case "nationshadow":
                    NationShadow(map, 100);
                    return false;
                case "forcenationshadow":
                    NationShadow(map, 100, true);
                    return false;
                default:
                    break;
            }

            string[] commandChunks = caselessCommand.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commandChunks.Length == 1)
            {
                return true;
            }

            if (commandChunks.Length > 2)
            {
                return true;
            }

            switch(commandChunks[0])
            {
                case "shadow":
                    if (int.TryParse(commandChunks[1], out int value))
                    {
                        Shadow(map, value);
                    }
                    else
                    {
                        Shadow(map, 100);
                    }
                    return false;
                case "forceshadow":
                    if (int.TryParse(commandChunks[1], out int value2))
                    {
                        Shadow(map, value2, true);
                    }
                    else
                    {
                        Shadow(map, 100, true);
                    }
                    return false;
                case "nationshadow":
                    if (int.TryParse(commandChunks[1], out int value3))
                    {
                        NationShadow(map, value3);
                    }
                    else
                    {
                        Shadow(map, 100);
                    }
                    return false;
                case "forcenationshadow":
                    if (int.TryParse(commandChunks[1], out int value4))
                    {
                        Shadow(map, value4, true);
                    }
                    else
                    {
                        Shadow(map, 100, true);
                    }
                    return false;
                default:
                    break;
            }

            return true;
        }

        public static void Ghast(Map map)
        {
            Location location = null;
            if (GraphicalMap.selectedHex != null)
            {
                location = GraphicalMap.selectedHex.location;
            }

            if (location == null && GraphicalMap.selectedUnit != null)
            {
                location = GraphicalMap.selectedUnit.location;
            }

            if (location == null)
            {
                return;
            }

            Person p = new Person(map.soc_dark, null);
            UAEN_Ghast ghast = new UAEN_Ghast(location, map.soc_dark, p);
            location.units.Add(ghast);
            map.units.Add(ghast);
        }

        public static void Ravenous(Map map)
        {
            Location location = null;
            if (GraphicalMap.selectedHex != null)
            {
                location = GraphicalMap.selectedHex.location;
            }

            if (location == null && GraphicalMap.selectedUnit != null)
            {
                location = GraphicalMap.selectedUnit.location;
            }

            if (location == null)
            {
                return;
            }

            UM_RavenousDead dead = new UM_RavenousDead(location, map.soc_dark, 100);
            location.units.Add(dead);
            map.units.Add(dead);
        }

        public static void Shadow(Map map, int value, bool force = false)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 100)
            {
                value = 100;
            }

            if (GraphicalMap.selectedUnit != null)
            {
                if (GraphicalMap.selectedUnit.person != null)
                {
                    if (GraphicalMap.selectedUnit.person.rulerOf != -1)
                    {
                        Location ruledLoc = map.locations[GraphicalMap.selectedUnit.person.rulerOf];
                        if (!force && ruledLoc.settlement != null && ruledLoc.settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                        {
                            return;
                        }
                    }

                    GraphicalMap.selectedUnit.person.shadow = value / 100.0;
                    GraphicalMap.checkLoaded();
                    map.world.ui.checkData();
                }

                return;
            }

            if (GraphicalMap.selectedHex != null)
            {
                double shadow = value / 100.0;
                float purity = (float)(1.0 - shadow);

                Location location = GraphicalMap.selectedHex.location;
                if (location == null && GraphicalMap.selectedHex.territoryOf != -1)
                {
                    location = map.locations[GraphicalMap.selectedHex.territoryOf];
                }

                if (location != null)
                {
                    if (location.settlement == null)
                    {
                        location.hex.purity = purity;
                    }
                    else
                    {
                        if (!force && location.settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                        {
                            return;
                        }

                        location.settlement.shadow = shadow;

                        if (location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null)
                        {
                            humanSettlement.ruler.shadow = shadow;
                        }
                    }

                    foreach (Hex hex in location.territory)
                    {
                        if (hex == null)
                        {
                            continue;
                        }

                        hex.purity = purity;
                    }

                    GraphicalMap.checkLoaded();
                    map.world.ui.checkData();
                    return;
                }
                
                if (force)
                {
                    GraphicalMap.selectedHex.purity = purity;
                    GraphicalMap.checkLoaded();
                    map.world.ui.checkData();
                    return;
                }
            }
        }

        public static void NationShadow(Map map, int value, bool force = false)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 100)
            {
                value = 100;
            }

            SocialGroup sg = null;

            if (GraphicalMap.selectedHex != null)
            {
                if (GraphicalMap.selectedHex.location != null)
                {
                    sg = GraphicalMap.selectedHex.location.soc;
                }
                else if (GraphicalMap.selectedHex.territoryOf != -1)
                {
                    sg = map.locations[GraphicalMap.selectedHex.territoryOf].soc;
                }
            }
            
            if (sg == null && GraphicalMap.selectedUnit != null)
            {
                sg = GraphicalMap.selectedUnit.society;
            }

            if (sg == null)
            {
                return;
            }

            if (!force && sg is Society soc && soc.isAlliance && map.opt_allianceState == 1)
            {
                return;
            }

            double shadow = value / 100.0;
            float purity = (float)(1.0 - shadow);
            foreach (Location location in map.locations)
            {
                if (location.soc != sg)
                {
                    continue;
                }

                if (location.settlement == null)
                {
                    location.hex.purity = purity;
                }
                else
                {
                    if (!force && location.settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                    {
                        continue;
                    }

                    location.settlement.shadow = shadow;

                    if (location.settlement is SettlementHuman humanSettlement && humanSettlement.ruler != null)
                    {
                        humanSettlement.ruler.shadow = shadow;
                    }
                }

                foreach (Hex hex in location.territory)
                {
                    if (hex == null)
                    {
                        continue;
                    }

                    hex.purity = purity;
                }
            }

            foreach (Unit unit in map.units)
            {
                if (unit.society != sg)
                {
                    continue;
                }

                if (unit.person != null)
                {
                    if (unit.person.rulerOf != -1)
                    {
                        Location ruledLoc = map.locations[unit.person.rulerOf];
                        if (!force && ruledLoc.settlement != null && ruledLoc.settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                        {
                            continue;
                        }
                    }

                    unit.person.shadow = value / 100.0;
                }
            }

            GraphicalMap.checkLoaded();
            map.world.ui.checkData();
        }
    }
}
