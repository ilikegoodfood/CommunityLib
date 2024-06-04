using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using static SortedDictionaryProvider;

namespace CommunityLib
{
    internal class HooksInternal : Hooks
    {
        internal HooksInternal(Map map)
            : base(map)
        {
            
        }

        public override bool isUnitSubsumed(Unit uOriginal, Unit uSubsuming)
        {
            return uSubsuming is UM_OrcRaiders raiders && raiders.subsumedUnit == uOriginal;
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
                List<TradeRoute> routes = new List<TradeRoute>();
                foreach (TradeRoute route in graphicalLink.link.a.map.tradeManager.routes)
                {
                    int indexA = route.path.IndexOf(graphicalLink.link.a);
                    int indexB = route.path.IndexOf(graphicalLink.link.b);

                    if (indexA == -1 || indexB == -1)
                    {
                        continue;
                    }

                    if (indexB == indexA - 1 || indexB == indexA + 1)
                    {
                        routes.Add(route);
                    }
                }


                bool linkCrossesLayer = graphicalLink.link.a.hex.z != graphicalLink.link.b.hex.z;
                if (routes.Count > 0)
                {
                    float width = 0.04f;
                    float alpha = 1f;

                    if (graphicalLink.link.disabled)
                    {
                        alpha = 0.2f;
                    }
                    else
                    {
                        if (graphicalLink.link.map.masker.mask > 0)
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

                        TradeRoute selectedRoute = graphicalLink.link.map.world.ui.uiScrollables.scrollable_threats.targetRoute;
                        if (selectedRoute != null)
                        {
                            if (!routes.Contains(selectedRoute))
                            {
                                width = 0.04f;
                                alpha *= 0.15f;
                            }
                            else if (selectedRoute.raidingCooldown > 0)
                            {
                                alpha *= 0.8f;

                                if (graphicalLink.link.map.masker.mask == MapMaskManager.maskType.TRADE_ROUTE)
                                {
                                    if (linkCrossesLayer)
                                    {
                                        if (graphicalLink.link.a.hex.z != GraphicalMap.z)
                                        {
                                            graphicalLink.lineRenderer.startColor = new Color(1f, 0.1f, 0.1f, alpha);
                                        }
                                        else if (graphicalLink.link.b.hex.z != GraphicalMap.z)
                                        {
                                            graphicalLink.lineRenderer.endColor = new Color(1f, 0.1f, 0.1f, alpha);
                                        }
                                    }
                                    else
                                    {
                                        graphicalLink.lineRenderer.startColor = new Color(1f, 0.1f, 0.1f, alpha);
                                        graphicalLink.lineRenderer.endColor = new Color(1f, 0.1f, 0.1f, alpha);
                                    }
                                }
                            }
                        }
                        else if (routes.All(tr => tr.raidingCooldown > 0))
                        {
                            alpha *= 0.8f;

                            if (graphicalLink.link.map.masker.mask == MapMaskManager.maskType.TRADE_ROUTE)
                            {
                                graphicalLink.lineRenderer.startColor = new Color(1f, 0.1f, 0.1f, alpha);
                                graphicalLink.lineRenderer.endColor = new Color(1f, 0.1f, 0.1f, alpha);
                            }
                            else if (!linkCrossesLayer)
                            {
                                graphicalLink.lineRenderer.startColor = Color.grey;
                                graphicalLink.lineRenderer.endColor = Color.grey;
                            }
                            else
                            {
                                if (graphicalLink.link.a.hex.z != GraphicalMap.z)
                                {
                                    graphicalLink.lineRenderer.endColor = new Color(0.5f, 0.5f, 1f, alpha);
                                }
                                else if (graphicalLink.link.b.hex.z != GraphicalMap.z)
                                {
                                    graphicalLink.lineRenderer.startColor = new Color(0.5f, 0.5f, 1f, alpha);
                                }
                            }
                        }
                    }

                    float alphaA = alpha;
                    float alphaB = alpha;

                    if (linkCrossesLayer)
                    {
                        if (graphicalLink.link.a.hex.z != GraphicalMap.z)
                        {
                            alphaA = 0f;
                        }
                        else if (graphicalLink.link.b.hex.z != GraphicalMap.z)
                        {
                            alphaB = 0f;
                        }
                    }

                    graphicalLink.lineRenderer.startColor = new Color(graphicalLink.lineRenderer.startColor.r, graphicalLink.lineRenderer.startColor.g, graphicalLink.lineRenderer.startColor.b, alphaA);
                    graphicalLink.lineRenderer.endColor = new Color(graphicalLink.lineRenderer.endColor.r, graphicalLink.lineRenderer.endColor.g, graphicalLink.lineRenderer.endColor.b, alphaB);
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
                    visibleUnits.Clear();
                    visibleUnits.AddRange(ua.location.units);
                    return true;
                case UAEN_DeepOne _:
                    visibleUnits.Clear();
                    return true;
                case UAEN_Ghast _:
                    visibleUnits.Clear();
                    return true;
                case UAEN_OrcUpstart _:
                    visibleUnits.Clear();
                    return true;
                case UAEN_Vampire _:
                    visibleUnits.Clear();
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
                        visibleUnits.Clear();
                        return true;
                    }
                }

                if (intDataCord.typeDict.TryGetValue("Haematophage", out Type haematophageType) && haematophageType != null)
                {
                    if (ua.GetType() == haematophageType)
                    {
                        visibleUnits.Clear();

                        foreach (Unit unit in map.units)
                        {
                            if (!unit.isDead && map.getStepDist(ua.location, unit.location) < 4)
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

        public override bool onPathfinding_AllowSecondPass(Location locA, Unit u, List<int> mapLayers, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates)
        {
            bool result = false;

            if (locA.map.awarenessOfUnderground >= 1.0)
            {
                result = true;
            }
            else if (u != null)
            {
                if (u.society is Society society && (society.isOphanimControlled || society.isDarkEmpire))
                {
                    result = true;
                }
                else if (u.society == u.map.soc_dark || u.society == u.map.soc_neutral)
                {
                    result = true;
                }
                else if (u.society is SG_Orc orcSociety && orcSociety.canGoUnderground())
                {
                    result = true;
                }
            }

            if (result)
            {
                pathfindingDelegates.Remove(Pathfinding.delegate_LAYERBOUND);
            }

            return result;
        }

        public override bool onPathfindingTadeRoute_AllowSecondPass(Location start, List<int> endPointMapLayers, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {
            pathfindingDelegates.Remove(Pathfinding.delegate_TRADE_LAYERBOUND);

            return true;
        }

        public override void onPopulatingTradeRoutePathfindingDelegates(Location start, List<int> endPointMapLayers, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {
            if (start.settlement is Set_TombOfGods && start.map.overmind.god is God_Mammon)
            {
                pathfindingDelegates.Remove(Pathfinding.delegate_TRADE_UNDERGROUNDAWARENESS);
            }
        }

        public override void onBuildTradeNetwork_EndOfProcess(Map map, ManagerTrade tradeManager, List<Location> endpoints)
        {
            if (map.overmind.god is God_Mammon)
            {
                Location tomb = endpoints.FirstOrDefault(l => l.settlement is Set_TombOfGods);
                if (tomb != null)
                {
                    List<int> linkedLayerIDs = new List<int>();
                    foreach (TradeRoute route in tradeManager.routes)
                    {
                        if (route.start() == tomb)
                        {
                            if (!linkedLayerIDs.Contains(route.end().hex.z))
                            {
                                linkedLayerIDs.Add(route.end().hex.z);
                            }
                        }
                        else if (route.end() == tomb)
                        {
                            if (!linkedLayerIDs.Contains(route.start().hex.z))
                            {
                                linkedLayerIDs.Add(route.start().hex.z);
                            }
                        }
                    }

                    if (!linkedLayerIDs.Contains(0))
                    {
                        Location[] routePath = Pathfinding.getTradeRouteFrom(tomb, 0);
                        if (routePath != null && routePath.Length >= 2)
                        {
                            tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                        }
                    }

                    if (!linkedLayerIDs.Contains(1))
                    {
                        Location[] routePath = Pathfinding.getTradeRouteFrom(tomb, 1);
                        if (routePath != null && routePath.Length >= 2)
                        {
                            tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                        }
                    }
                }
            }
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
                if (ua.task is Task_GoToLocation || ua.task is Task_AttackUnit)
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
