using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using System.Linq;
using static SortedDictionaryProvider;
using FullSerializer;

namespace CommunityLib
{
    internal class HooksInternal : Hooks
    {
        internal HooksInternal(Map map)
            : base(map)
        {
            
        }

        public override void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            if (ModCore.opt_forceShipwrecks || ModCore.opt_spawnShipwrecks)
            {
                if (u.location.isOcean)
                {
                    //Console.WriteLine("Orcs_Plus: Unit died in ocean");
                    int wreckRoll = Eleven.random.Next(10);

                    if (wreckRoll == 0)
                    {
                        ModCore.Get().spawnShipwreck(u.location);
                    }
                }
            }
        }

        public override void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
        {
            if (ModCore.opt_forceShipwrecks || ModCore.opt_spawnShipwrecks)
            {
                if (set is SettlementHuman settlementHuman)
                {
                    if (set.subs.Any(sub => sub is Sub_Docks))
                    {
                        ModCore.Get().spawnShipwreck(set.location);
                    }
                }
                else if (set is Set_OrcCamp camp && camp.specialism == 5)
                {
                    ModCore.Get().spawnShipwreck(set.location);
                }
            }
        }

        public override HolyOrder onLocationViewFaithButton_GetHolyOrder(Location loc)
        {
            if (ModCore.opt_ophanimFaithTomb)
            {
                if (loc.map.overmind.god is God_Ophanim opha && opha.faith != null)
                {
                    if (loc.settlement is Set_TombOfGods)
                    {
                        return opha.faith;
                    }
                }
            }

            return null;
        }

        public override void onGraphicalLinkUpdated(GraphicalLink graphicalLink)
        {

            if (ModCore.opt_enhancedTradeRouteLinks)
            {
                TradeRoute route = map.tradeManager.routes.FirstOrDefault(tr => tr.path.Contains(graphicalLink.link.a) && tr.path.Contains(graphicalLink.link.b));
                if (route != null)
                {
                    float width = 0.04f;
                    float alpha = 1f;

                    if (graphicalLink.link.disabled)
                    {
                        alpha = 0.2f;
                    }
                    else if (route.raidingCooldown > 0)
                    {
                        alpha *= 0.8f;
                    }

                    if (graphicalLink.link.map.masker.mask > MapMaskManager.maskType.NONE)
                    {
                        if (graphicalLink.link.map.masker.mask == MapMaskManager.maskType.TRADE_ROUTE)
                        {
                            width = 0.03f;
                        }
                        else
                        {
                            alpha *= 0.15f;
                        }
                    }

                    if (graphicalLink.link.map.world.ui.uiScrollables.scrollable_threats.targetRoute != null && graphicalLink.link.map.world.ui.uiScrollables.scrollable_threats.targetRoute != route)
                    {
                        width = 0.04f;
                        alpha *= 0.15f;
                    }

                    graphicalLink.lineRenderer.startColor = new Color(graphicalLink.lineRenderer.startColor.r, graphicalLink.lineRenderer.startColor.g, graphicalLink.lineRenderer.startColor.b, alpha);
                    graphicalLink.lineRenderer.endColor = new Color(graphicalLink.lineRenderer.endColor.r, graphicalLink.lineRenderer.endColor.g, graphicalLink.lineRenderer.endColor.b, alpha);
                    graphicalLink.lineRenderer.startWidth = width;
                    graphicalLink.lineRenderer.endWidth = width;
                }
            }
        }

        public override bool interceptGetVisibleUnits(UA ua, List<Unit> visibleUnits)
        {
            switch (ua)
            {
                case UAEN_CaveSpider _:
                    visibleUnits = new List<Unit>();
                    visibleUnits.AddRange(ua.location.units);
                    return true;
                case UAEN_DeepOne _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_Ghast _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_OrcUpstart _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_Vampire _:
                    visibleUnits = new List<Unit>();
                    return true;
                default:
                    break;
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
                {
                    if (ua.GetType() == droneType)
                    {
                        visibleUnits = new List<Unit>();
                        return true;
                    }
                }

                if (intDataCord.typeDict.TryGetValue("Haematophage", out Type haematophageType) && haematophageType != null)
                {
                    if (ua.GetType() == haematophageType)
                    {
                        visibleUnits = new List<Unit>();

                        foreach (Unit unit in map.units)
                        {
                            if (map.getStepDist(ua.location, unit.location) < 4)
                            {
                                visibleUnits.Add(unit);
                            }
                        }

                        return true;
                    }
                }

            }

            return false;
        }

        public override Location[] interceptGetPathTo_Location(Location locA, Location locB, Unit u, bool safeMove)
        {
            if (ModCore.opt_forceCommunityLibraryPathfinding)
            {
                return ModCore.Get().pathfinding.getPathTo(locA, locB, u, safeMove);
            }

            return null;
        }

        public override Location[] interceptGetPathTo_SocialGroup(Location loc, SocialGroup sg, Unit u, bool safeMove)
        {
            if (ModCore.opt_forceCommunityLibraryPathfinding)
            {
                return ModCore.Get().pathfinding.getPathTo(loc, sg, u, safeMove);
            }

            return null;
        }

        public override bool interceptAgentAI(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> challengeData, List<AgentAI.TaskData> taskData, List<Unit> visibleUnits)
        {
            switch (ua)
            {
                case UAEN_OrcUpstart upstart:
                    return interceptOrcUpstart(upstart);
                default:
                    break;
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
                {
                    if (ua.GetType() == droneType)
                    {
                        if (intDataCord.typeDict.TryGetValue("God", out Type godType) && godType != null && (ua.map.overmind.god.GetType() == godType || ua.map.overmind.god.GetType().IsSubclassOf(godType)))
                        {
                            return interceptCordycepsDrone(ua, intDataCord);
                        }
                        else
                        {
                            ua.die(ua.map, "Died in Wilderness", null);
                            return true;
                        }
                    }
                }
                
            }

            return false;
        }
        private bool interceptOrcUpstart(UAEN_OrcUpstart upstart)
        {
            if (upstart.society.checkIsGone() || upstart.society.lastTurnLocs.Count == 0)
            {
                upstart.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }

        private bool interceptCordycepsDrone(UA ua, ModIntegrationData intData)
        {
            if (intData.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null && intData.fieldInfoDict.TryGetValue("Drone.prey", out FieldInfo FI_Prey) && FI_Prey != null && intData.methodInfoDict.TryGetValue("God.eat", out MethodInfo MI_GodEat) && MI_GodEat != null)
            {
                if (intData.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null && intData.typeDict.TryGetValue("LarvalMass", out Type larvalType) && larvalType != null)
                {
                    if (ua.location.settlement != null && (ua.location.settlement.GetType() == hiveType || ua.location.settlement.GetType().IsSubclassOf(hiveType)))
                    {
                        Property larvalMass = ua.location.properties.FirstOrDefault(pr => pr.GetType() == larvalType);
                        if (larvalMass != null)
                        {
                            larvalMass.charge += (int)FI_Prey.GetValue(ua);
                            MI_GodEat.Invoke(ua.map.overmind.god, new object[] { (int)FI_Prey.GetValue(ua) });
                            FI_Prey.SetValue(ua, 0);
                        }
                    }
                }
            }

            return false;
        }

        public override void onAgentAI_EndOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            if (ua is UAEN_DeepOne)
            {
                if ((ua.task == null || (ua.task is Task_GoToLocation tLocation && tLocation.target.index == ua.homeLocation)) && validChallengeData.FindAll(cd => !(cd.challenge is Rt_DeepOnes_TravelBeneath)).Count == 0 && validTaskData.Count == 0)
                {
                    Rt_DeepOnes_TravelBeneath travel = (Rt_DeepOnes_TravelBeneath)ua.rituals.FirstOrDefault(rt => rt is Rt_DeepOnes_TravelBeneath);
                    if (travel != null)
                    {
                        ua.task = new Task_PerformChallenge(travel);
                    }
                }
            }
            else if (ua is UAEN_CaveSpider)
            {
                if (ua.task is Task_GoToLocation)
                {
                    ua.task.turnTick(ua);
                }
            }
        }

        public override int onUnitAI_GetsDistanceToLocation(Unit u, Location target, Location[] pathTo, int travelTime)
        {
            //Console.WriteLine("CommunityLib: Internal Hook GetDistance for " + u.getName() + " of type " + u.GetType().Name);
            if (u.person != null)
            {
                //Console.WriteLine("CommunityLib: " + u.getName() + " has person");
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC != null && intDataCCC.typeDict.TryGetValue("HeroicBoots", out Type heroicBootsType) && heroicBootsType != null)
                {
                    if (u.person.items.Any(i => i != null && i.GetType() == heroicBootsType))
                    {
                        travelTime = (int)Math.Ceiling(travelTime / (u.getMaxMoves() + 2.0));
                    }
                }
            }

            return travelTime;
        }
    }
}
