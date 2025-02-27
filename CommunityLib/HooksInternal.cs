﻿using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace CommunityLib
{
    internal class HooksInternal : Hooks
    {
        public bool densifyingTradeRoutes = false;

        internal HooksInternal(Map map)
            : base(map)
        {
            
        }

        public override List<WonderData> onMapGen_PlaceWonders()
        {
            return new List<WonderData> {
                new WonderData(typeof(Sub_Wonder_Doorway), ModCore.opt_wonderPriority_entrance),
                new WonderData(typeof(Sub_Wonder_DeathIsland), ModCore.opt_wonderPriority_brother),
                new WonderData(typeof(Sub_Wonder_PrimalFont), ModCore.opt_wonderPriority_font)
            };
        }

        public override void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;

            if (t == typeof(Sub_Wonder_DeathIsland))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in map.locations)
                {
                    if (location.isOcean && !location.getNeighbours().Any(n => !n.isOcean) && location.settlement == null)
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    target = locations[0];
                    if (Location.indexCounter > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                }

                if (target != null)
                {
                    target.settlement = new Set_MinorOther(target);
                    target.settlement.subs.Clear();
                    target.settlement.subs.Add(new Sub_Wonder_DeathIsland(target.settlement));
                    return;
                }

                failedToPlaceWonder = true;

            }

            if (t == typeof(Sub_Wonder_Doorway))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in map.locations)
                {
                    if (location.settlement == null && !location.isOcean && (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY))
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    target = locations[0];
                    if (Location.indexCounter > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                }

                if (target != null)
                {
                    target.settlement = new Set_MinorOther(target);
                    target.settlement.subs.Clear();
                    target.settlement.subs.Add(new Sub_Wonder_Doorway(target.settlement));
                    return;
                }

                failedToPlaceWonder = true;
            }
            
            if (t == typeof(Sub_Wonder_PrimalFont))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in map.locations)
                {
                    if (location.settlement == null && !location.isOcean)
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    target = locations[0];
                    if (Location.indexCounter > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                }

                if (target != null)
                {
                    target.settlement = new Set_MinorOther(target);
                    target.settlement.subs.Clear();
                    target.settlement.subs.Add(new Sub_Wonder_PrimalFont(target.settlement));
                    return;
                }

                failedToPlaceWonder = true;
            }
        }

        public override bool isUnitSubsumed(Unit uOriginal, Unit uSubsuming)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("LivingWilds", out ModIntegrationData intDataLW))
            {
                if (intDataLW.typeDict.TryGetValue("InfectedWerewolf", out Type infectedWolfType) && infectedWolfType != null)
                {
                    if (uSubsuming.GetType() == infectedWolfType || uSubsuming.GetType().IsSubclassOf(infectedWolfType))
                    {
                        if (intDataLW.fieldInfoDict.TryGetValue("InfectedWerewolfSubsumedUnit", out FieldInfo FI_SubsumedUnit) && FI_SubsumedUnit != null)
                        {
                            if (uOriginal == FI_SubsumedUnit.GetValue(uSubsuming))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }

                if (intDataLW.typeDict.TryGetValue("InfectedWerewolf", out Type infectedWolfHeroType) && infectedWolfHeroType != null)
                {
                    if (uSubsuming.GetType() == infectedWolfHeroType || uSubsuming.GetType().IsSubclassOf(infectedWolfHeroType))
                    {
                        if (intDataLW.fieldInfoDict.TryGetValue("InfectedWerewolfHeroSubsumedUnit", out FieldInfo FI_SubsumedUnit) && FI_SubsumedUnit != null)
                        {
                            if (uOriginal == FI_SubsumedUnit.GetValue(uSubsuming))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }
            }

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

            if (ModCore.opt_chosenOneDeathMessage && map.awarenessManager.chosenOne == u || (u.person != null && u.person.traits.Any(t => t is T_ChosenOne)))
            {
                map.addUnifiedMessage(u, u.location, "Chosen One Dies", $"{u.getName()}, the Chosen One, has died. Another may step up to take their place. \n\n Cause of Death: {v}", "Chosen One Dies", true);
            }
        }

        public override void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            if (set is Set_OrcCamp && set.location.settlement is Set_CityRuins ruins && ruins.subs.Count == 0)
            {
                set.location.settlement = null;
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

        public override bool onCheckIsProphetPlayerAligned(HolyOrder order, UA prophet)
        {
            if (prophet.isCommandable())
            {
                return true;
            }

            if (ModCore.Get().data.tryGetModIntegrationData("DeepOnesPlus", out ModIntegrationData intDataDOP) && intDataDOP.typeDict.TryGetValue("DrownedProphet", out Type drownedProphetType) && drownedProphetType != null)
            {
                if (prophet.GetType() == drownedProphetType)
                {
                    return true;
                }
            }

            if (ModCore.opt_darkProphets)
            {
                if (prophet.society is Society society)
                {
                    if (society.isDarkEmpire)
                    {
                        if (prophet.person.shadow > 50.0)
                        {
                            return true;
                        }
                    }
                    else if (society.isOphanimControlled)
                    {
                        return true;
                    }
                }
            }

            return false;
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
                                alpha *= 0.6f;
                            }
                        }
                        else if (routes.All(tr => tr.raidingCooldown > 0))
                        {
                            alpha *= 0.6f;
                        }
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

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
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

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
                {
                    if (ua.GetType() == pigeonType)
                    {
                        visibleUnits.Clear();
                        return true;
                    }
                }

                if (intDataCCC.typeDict.TryGetValue("UAEN_Toad", out Type toadType) && toadType != null)
                {
                    if (ua.GetType() == toadType)
                    {
                        visibleUnits.Clear();
                        return true;
                    }
                }
            }

            return false;
        }


        public override void onPopulatingTradeRoutePathfindingDelegates(Location start, List<Func<Location[], Location, double>> pathfindingDelegates, List<Func<Location[], Location, bool>> destinationValidityDelegates)
        {
            if (ModCore.opt_denseTradeRoutes && densifyingTradeRoutes)
            {
                destinationValidityDelegates.Remove(Pathfinding.delegate_TRADEVALID_NODUPLICATES);
            }
        }

        public override void onAgentLevelup_GetTraits(UA ua, List<Trait> availableTraits, bool startingTraits)
        {
            if (startingTraits && ua is UAE_Warlock)
            {
                if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
                {
                    if (intDataCCC.typeDict.TryGetValue("Curseweaving", out Type curseweavingType))
                    {
                        Trait curseweaving = (Trait)Activator.CreateInstance(curseweavingType);
                        availableTraits.Add(curseweaving);
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

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
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

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
                {
                    if (ua.GetType() == pigeonType)
                    {
                        if (ua.task == null)
                        {
                            if (ua.person.gold > 0 || ua.person.items.Any(i => i != null))
                            {
                                Pr_ItemCache pr_ItemCache = new Pr_ItemCache(ua.location);
                                foreach (Item item in ua.person.items)
                                {
                                    if (item == null)
                                    {
                                        continue;
                                    }

                                    pr_ItemCache.addItemToSet(item);
                                }
                                pr_ItemCache.gold = ua.person.gold;
                            }

                            ua.map.addUnifiedMessage(ua, ua.location, "Pigeon flew away", $"After loosing its owner the pigeon has flown away, leaving any gold or items it was carrying behind.", "PigeonFlewAway");
                            if (GraphicalMap.selectedUnit == ua)
                            {
                                GraphicalMap.selectedUnit = null;
                            }
                            ua.disband(ua.map, "Ownerless pigeon dissapeared into the wilds");
                            return;
                        }
                    }
                }
            }

            return;
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
