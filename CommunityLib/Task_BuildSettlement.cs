using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class Task_BuildSettlement : Assets.Code.Task
    {
        public Location target;

        public int delay;

        public int timeRemaining;

        public bool isFortress;

        public bool arrived;

        public Task_BuildSettlement(Location targetLoc, int delay = 0, int constructionTime = 10)
        {
            target = targetLoc;
            this.delay = delay;
            timeRemaining = constructionTime;
            isFortress = false;
            arrived = false;
        }

        public Task_BuildSettlement(Location targetLoc, bool isFortress, int delay = 0, int constructionTime = 10)
        {
            target = targetLoc;
            this.delay = delay;
            timeRemaining = constructionTime;
            this.isFortress = isFortress;
            arrived  = false;
        }

        public override string getShort()
        {
            if (delay > 0)
            {
                return $"Building {(isFortress ? "fortress" : "settlement")} at {target?.getName() ?? "ERROR: Undefined Target Location"} (gathering construction materials)";
            }
            
            if (!arrived)
            {
                return $"Building {(isFortress ? "fortress" : "settlement")} at {target?.getName() ?? "ERROR: Undefined Target Location"} (travelling to construction site)";
            }

            return $"Building {(isFortress? "fortress" : "settlement")} at {target?.getName() ?? "ERROR: Undefined Target Location"} ({timeRemaining} turns remaining)";
        }

        public override string getLong()
        {
            return getShort();
        }

        public override void turnTick(Unit unit)
        {
            if (delay > 0)
            {
                delay--;
                return;
            }

            if (!(unit is UM um))
            {
                unit.task = null;
                return;
            }

            if (target == null || (target.settlement != null && !(target.settlement is Set_CityRuins)))
            {
                unit.task = null;
                return;
            }

            if (um.location != target)
            {
                Location[] pathTo = Pathfinding.getPathTo(um.location, target, unit, true);
                if (pathTo == null || pathTo.Length < 2)
                {
                    um.task = null;
                    return;
                }

                int index = 1;
                while (um.movesTaken < um.getMaxMoves())
                {
                    um.location.map.adjacentMoveTo(um, pathTo[index]);
                    um.movesTaken++;
                    index++;

                    if (um.location == target)
                    {
                        arrived = true;
                        return;
                    }
                }
            }

            arrived = true;
            timeRemaining--;
            if (timeRemaining <= 0)
            {
                um.task = null;
                if (um.society is Society soc)
                {

                    SettlementHuman setHuman = null;
                    if (soc is Soc_Dwarves)
                    {
                        if (isFortress)
                        {
                            setHuman = new Set_DwarvenCity(target);
                            setHuman.subs.Add(new Sub_DwarfFortress(setHuman));
                        }
                        else if (target.isMajor)
                        {
                            setHuman = new Set_DwarvenCity(target);
                        }
                        else
                        {
                            setHuman = new Set_DwarvenOutpost(target);
                        }

                        target.properties.Add(new Pr_GrowingEconomy(target, 0.5, 0.25));
                    }
                    else if (soc is Soc_Elven)
                    {
                        setHuman = new Set_ElvenCity(target);
                    }
                    else
                    {
                        if (isFortress)
                        {
                            setHuman = new Set_MinorHuman(target);
                        }
                        else if (target.isMajor)
                        {
                            setHuman = new Set_City(target);
                        }
                        else
                        {
                            setHuman = new Set_MinorHuman(target);
                        }
                    }
                    
                    if (setHuman == null)
                    {
                        return;
                    }

                    target.settlement = setHuman;
                    setHuman.population = um.hp;

                    if (ModCore.Get().data.tryGetModIntegrationData("LivingSocieties", out ModIntegrationData intDataLS) && intDataLS.assembly != null)
                    {
                        if (intDataLS.methodInfoDict.TryGetValue("UpdateSettlement", out MethodInfo MI_UpdateSettlement))
                        {
                            object[] parameters = new object[] { soc, setHuman };
                            MI_UpdateSettlement.Invoke(null, parameters);
                        }
                    }

                    if (target.settlement is SettlementHuman setHuman2)
                    {
                        if (isFortress)
                        {
                            if (setHuman2 is Set_DwarvenCity)
                            {
                                setHuman2.subs.Add(new Sub_DwarfFortress(setHuman2));
                            }
                            else if (setHuman2 is Set_MinorHuman)
                            {
                                List<Subsettlement> subsToRemove = new List<Subsettlement>();
                                foreach (Subsettlement sub in setHuman2.subs)
                                {
                                    if (sub is Sub_Farms || sub is Sub_Cathedral || sub is Sub_Catacombs)
                                    {
                                        subsToRemove.Add(sub);
                                    }
                                }

                                if (subsToRemove.Count > 0)
                                {
                                    foreach (Subsettlement sub in subsToRemove)
                                    {
                                        setHuman2.subs.Remove(sub);
                                    }

                                    setHuman2.subs.Add(new Sub_Fort(setHuman2));
                                }

                            }
                        }
                    }
                }
                else if (um.society is SG_Orc)
                {
                    Set_OrcCamp camp = new Set_OrcCamp(target);
                    target.settlement = camp;

                    if (isFortress)
                    {
                        camp.specialism = 1;
                    }

                    Set_OrcCamp homeCamp = um.map.locations[um.homeLocation].settlement as Set_OrcCamp;
                    if (homeCamp != null && homeCamp.infiltration >= 1.0)
                    {
                        camp.isInfiltrated = true;
                    }
                }
                else if (um.society is SG_DeepOnes)
                {
                    if (target.isOcean)
                    {
                        target.settlement = new Set_DeepOneAbyssalCity(target);
                    }
                    else
                    {
                        target.settlement = new Set_DeepOneSanctum(target);
                    }
                }

                if (um is UM_Refugees refugees && target.settlement != null)
                {
                    target.settlement.shadow = refugees.shadow;
                }

                target.soc = um.society;
                um.hp = 0;
                um.map.units.Remove(um);
                um.location.units.Remove(um);
                um.disband(um.map, "Settlement Built");
            }
        }

        public override Location getLocation()
        {
            return target;
        }
    }
}
