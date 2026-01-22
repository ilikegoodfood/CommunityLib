using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Assets.Code.Modding;

namespace CommunityLib
{
    internal class HooksInternal
    {
        private Map _map;

        internal bool DensifyingTradeRoutes = false;

        internal HooksInternal(Map map)
        {
            _map = map;

            HooksDelegateRegistry registry = ModCore.Get().HookRegistry;
            registry.RegisterHook_onMapGen_PlaceWonders_1(onMapGen_PlaceWonders);
            registry.RegisterHook_onMapGen_PlaceWonders_2(onMapGen_PlaceWonders);
            registry.RegisterHook_mapMask_PopulatingThreats(mapMask_PopulatingThreats); // TEST ITEM
            registry.RegisterHook_mapMask_onThreatHovorOver(mapMask_onThreatHovorOver); // TEST ITEM
            registry.RegisterHook_isUnitSubsumed(isUnitSubsumed);
            registry.RegisterHook_onUnitDeath_StartOfProcess(onUnitDeath_StartOfProcess);
            registry.RegisterHook_onSettlementFallIntoRuin_StartOfProcess(onSettlementFallIntoRuin_StartOfProcess);
            registry.RegisterHook_onSettlementFallIntoRuin_EndOfProcess(onSettlementFallIntoRuin_EndOfProcess);
            registry.RegisterHook_onSettlementCalculatesShadowGain(onSettlementComputesShadowGain);
            registry.RegisterHook_onBrokenMakerSleeps_TurnTick(onBrokenMakerSleeps_TurnTick);
            registry.RegisterHook_onCheckIsProphetPlayerAligned(onCheckIsProphetPlayerAligned);
            registry.RegisterHook_onLocationViewFaithButton_GetHolyOrder(onLocationViewFaithButton_GetHolyOrder);
            //registry.RegisterHook_onGraphicalLinkUpdated(onGraphicalLinkUpdated);
            registry.RegisterHook_interceptGetVisibleUnits(interceptGetVisibleUnits);
            registry.RegisterHook_onPathfinding_AllowSecondPass(onPathfinding_AllowSecondPass);
            registry.RegisterHook_onPathfindingTadeRoute_AllowSecondPass(onPathfindingTadeRoute_AllowSecondPass);
            registry.RegisterHook_onPopulatingTradeRoutePathfindingDelegates(onPopulatingTradeRoutePathfindingDelegates);
            registry.RegisterHook_onGetTradeRouteEndpoints(onGetTradeRouteEndpoints);
            registry.RegisterHook_onBuildTradeNetwork_EndOfProcess(onBuildTradeNetwork_EndOfProcess);
            registry.RegisterHook_interceptAgentAI(interceptAgentAI);
            registry.RegisterHook_onAgentAI_EndOfProcess(onAgentAI_EndOfProcess);
        }

        public List<WonderData> onMapGen_PlaceWonders()
        {
            return new List<WonderData> {
                new WonderData(typeof(Sub_Wonder_Doorway), ModCore.opt_wonderPriority_entrance),
                new WonderData(typeof(Sub_Wonder_DeathIsland), ModCore.opt_wonderPriority_brother),
                new WonderData(typeof(Sub_Wonder_PrimalFont), ModCore.opt_wonderPriority_font)
            };
        }

        public void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;

            if (t == typeof(Sub_Wonder_DeathIsland))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in _map.locations)
                {
                    if (location.hex.z == 0 && location.isOcean && !location.getNeighbours().Any(n => !n.isOcean) && location.settlement == null)
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    if (locations.Count > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                    else
                    {
                        target = locations[0];
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
                return;
            }
            
            if (t == typeof(Sub_Wonder_Doorway))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in _map.locations)
                {
                    if (location.hex.z == 0 && location.settlement == null && !location.isOcean && (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY))
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    if (locations.Count > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                    else
                    {
                        target = locations[0];
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
                return;
            }
            
            if (t == typeof(Sub_Wonder_PrimalFont))
            {
                List<Location> locations = new List<Location>();
                Location target = null;

                foreach (Location location in _map.locations)
                {
                    if (location.hex.z == 0 && location.settlement == null && !location.isOcean)
                    {
                        locations.Add(location);
                    }
                }

                if (locations.Count > 0)
                {
                    if (locations.Count > 1)
                    {
                        target = locations[Eleven.random.Next(locations.Count)];
                    }
                    else
                    {
                        target = locations[0];
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
                return;
            }
        }

        public void mapMask_PopulatingThreats(UIScrollThreats threats, ModKernel maskingMod, int maskID, string title, string buttonLabel, string description)
        {
            if (maskID == -1 || maskID != ModCore.Get().bachelorsMaskID)
            {
                return;
            }

            HashSet<Person> visited = new HashSet<Person>();
            foreach(Location location in threats.world.map.locations)
            {
                if (!(location.settlement is SettlementHuman settlementHuman) || settlementHuman.ruler == null || settlementHuman.ruler.getSpouse() != null || settlementHuman.ruler.traits.Any(t => t is T_Mourning mourn && mourn.turnsLeft > 0) || visited.Contains(settlementHuman.ruler))
                {
                    continue;
                }
                visited.Add(settlementHuman.ruler);
                
                string filterText = threats.filterField.text.ToLower();
                if (filterText == "" || settlementHuman.ruler.getName().ToLower().Contains(filterText))
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(threats.world.prefabStore.uieHeroViewer, threats.subsetArea);
                    UIE_HeroViewer viewer = gameObject.GetComponent<UIE_HeroViewer>();
                    viewer.setToPerson(threats.world, settlementHuman.ruler);
                }
            }
        }

        public virtual void mapMask_onThreatHovorOver(UIScrollThreats threats, MonoBehaviour hoveredItem, ModKernel maskingMod, int maskID, string title, string buttonLabel, string description)
        {
            //Console.WriteLine($"CommunityLib: MaskId: {maskID}.");
            if (maskID == -1 || maskingMod != ModCore.Get() || maskID != ModCore.Get().bachelorsMaskID)
            {
                //Console.WriteLine($"CommunityLib: Invalid Mask ID.");
                return;
            }

            //Console.WriteLine($"CommunityLib: Hover over threat instance for bachelor's modded map mask.");
            if (hoveredItem == null)
            {
                //Console.WriteLine($"CommunityLib: Hovered MonoBehaviour is null.");
                return;
            }

            if (!(hoveredItem is UIE_HeroViewer viewer))
            {
                //Console.WriteLine($"CommunityLib: Hovered over threat viewer instance that was not a hero viewer.");
                return;
            }

            if (viewer.personExamplar == null)
            {
                //Console.WriteLine($"CommunityLib: Target Hero Viewer has no person exemplar.");
                return;
            }

            //Console.WriteLine($"CommunityLib: Hovered over Hero Viewer threat panel isntance for {viewer.personExamplar.getName()}");
            threats.targetSettlement = threats.world.map.locations[viewer.personExamplar.rulerOf].settlement;
        }

        public bool isUnitSubsumed(Unit uOriginal, Unit uSubsuming)
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

        public void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
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

            if (ModCore.opt_chosenOneDeathMessage && _map.awarenessManager.chosenOne == u || (u.person != null && u.person.traits.Any(t => t is T_ChosenOne)))
            {
                _map.addUnifiedMessage(u, u.location, "Chosen One Dies", $"{u.getName()}, the Chosen One, has died. Another may step up to take their place. \n\n Cause of Death: {v}", "Chosen One Dies", true);
            }
        }

        public void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            if (set is Set_OrcCamp && set.location.settlement is Set_CityRuins ruins && ruins.subs.Count == 0)
            {
                set.location.settlement = null;
            }
        }

        public double onSettlementComputesShadowGain(Settlement set, List<ReasonMsg> msgs, double shadowGain)
        {
            if (msgs == null)
            {
                return shadowGain;
            }

            SettlementHuman humanSettlement = set as SettlementHuman;

            Pr_Ward ward = null;
            Pr_DeepOneCult deepOneCult = null;
            Pr_Opha_Faith ophanimsFaith = null;
            Pr_MalignCatch malignCatch = null;
            foreach (Property property in set.location.properties)
            {
                if (ward == null && property is Pr_Ward shadowWard)
                {
                    ward = shadowWard;
                }

                if (deepOneCult == null && property is Pr_DeepOneCult deepOnes)
                {
                    if (property.charge > 100.0)
                    {
                        deepOneCult = deepOnes;
                        continue;
                    }
                }

                if (humanSettlement == null)
                {
                    continue;
                }

                if (ophanimsFaith == null && property is Pr_Opha_Faith ophaFaith)
                {
                    ophanimsFaith = ophaFaith;
                    continue;
                }
                if (malignCatch == null && property is Pr_MalignCatch malign)
                {
                    malignCatch = malign;
                    continue;
                }
            }

            if (deepOneCult != null)
            {
                double delta = _map.param.prop_deepOneShadow * (deepOneCult.charge / 300.0);
                msgs.Add(new ReasonMsg("Deep One Cult", delta));
                shadowGain += delta;
            }

            if (humanSettlement != null)
            {
                Society society = set.location.soc as Society;

                if (ophanimsFaith != null)
                {
                    double delta = -ophanimsFaith.charge / 500.0;
                    msgs.Add(new ReasonMsg("Ophanim's Faith", delta));
                    shadowGain += delta;
                }

                if (malignCatch != null)
                {
                    double delta = _map.param.ch_malignCatchShadow;
                    msgs.Add(new ReasonMsg("Malign Catch", delta));
                    shadowGain += delta;
                }

                int T_DyingLight_Count = 0;
                int T_SettingSun_Count = 0;
                int T_TheyWillObey_Count = 0;
                int I_DarkStone_Count = 0;
                List<UAEN_Ghast> enshadowingGhasts = new List<UAEN_Ghast>();
                foreach (Unit unit in set.location.units)
                {
                    if (unit.person != null)
                    {
                        foreach (Item item in unit.person.items)
                        {
                            if (item is I_DarkStone)
                            {
                                I_DarkStone_Count++;
                            }
                        }

                        foreach (Trait trait in unit.person.traits)
                        {
                            if (trait is T_Snake_Enshadower)
                            {
                                T_DyingLight_Count++;
                                continue;
                            }
                            if (trait is T_TheSettingSun)
                            {
                                T_SettingSun_Count++;
                                continue;
                            }
                            if (trait is T_TheyWillObey)
                            {
                                T_TheyWillObey_Count++;
                            }
                        }

                        if (unit is UAEN_Ghast ghast)
                        {
                            if (ghast.task is Task_PerformChallenge challengeTask && challengeTask.challenge is Rt_GhastEnshadow && (ward == null || ward.charge <= 0.9))
                            {
                                enshadowingGhasts.Add(ghast);
                            }
                            continue;
                        }
                    }
                }

                if (I_DarkStone_Count > 0)
                {
                    msgs.Add(new ReasonMsg($"{(I_DarkStone_Count == 1 ? "Dark Stone" : "Dark Stones")}", 0.01 * I_DarkStone_Count));
                    shadowGain += 0.01;
                }
                if (T_DyingLight_Count > 0)
                {
                    msgs.Add(new ReasonMsg("The Dying Light", 0.01 * T_DyingLight_Count));
                    shadowGain += 0.01;
                }
                if (T_SettingSun_Count > 0)
                {
                    msgs.Add(new ReasonMsg("The Setting Sun", _map.param.trait_settingSunShadowPerTurn * T_SettingSun_Count));
                    shadowGain += 0.01;
                }
                if (T_TheyWillObey_Count > 0 && society != null && society.isDarkEmpire)
                {
                    msgs.Add(new ReasonMsg("They Will Obey", _map.param.trait_theyWillObeyShadowPerTurn * T_SettingSun_Count));
                    shadowGain += 0.01;
                }

                foreach (UAEN_Ghast ghast in enshadowingGhasts)
                {
                    double delta = _map.param.ch_ghastShadowPerTurnPerLore * ghast.getStatLore() * Math.Max(0.0, 1.0 - (ward?.charge ?? 0.0));
                    msgs.Add(new ReasonMsg($"Being Enshadowed by {ghast.getName()}", delta));
                    shadowGain += delta;
                }

                int desecrateCathedralCount = humanSettlement.subs.FindAll(sub => sub is Sub_Cathedral cathedral && cathedral.desecrated).Count;
                if (desecrateCathedralCount > 0)
                {
                    msgs.Add(new ReasonMsg("Desecrated Cathedral", 0.01 * desecrateCathedralCount));
                    shadowGain += 0.01;
                }

                if (_map.tradeManager.tradeDensity[set.location.index] != null)
                {
                    int snakeTradeRouteCount = 0;
                    foreach (TradeRoute route in _map.tradeManager.tradeDensity[set.location.index])
                    {
                        if (route.snake > 0)
                        {
                            snakeTradeRouteCount++;
                        }
                    }

                    if (snakeTradeRouteCount > 0)
                    {
                        msgs.Add(new ReasonMsg("Serpent's Coils", Math.Max(0.0, 100 - (ward?.charge ?? 0.0)) * 0.01 * _map.param.power_serpentsCoilsShadowGain * snakeTradeRouteCount));
                        shadowGain += 0.01;
                    }
                }
            }

            return shadowGain;
        }

        public void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
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

        public bool onCheckIsProphetPlayerAligned(HolyOrder order, UA prophet)
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

        public void onBrokenMakerSleeps_TurnTick(Map map)
        {
            List<Unit> unitsToKill = new List<Unit>();
            foreach (Unit unit in _map.units)
            {
                if (unit is UM_RavenousDead || unit is UM_UntamedDead || unit is UM_Cthonians)
                {
                    unitsToKill.Add(unit);
                }
            }

            foreach (Unit unit in unitsToKill)
            {
                unit.die(map, "Gone");
            }
        }

        public HolyOrder onLocationViewFaithButton_GetHolyOrder(Location loc)
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

        public void onGraphicalLinkUpdated(GraphicalLink graphicalLink)
        {
            /*if (ModCore.opt_enhancedTradeRouteLinks)
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

                    float alphaA = alpha;
                    float alphaB = alpha;

                    if (graphicalLink.link.a.hex.z != graphicalLink.link.b.hex.z)
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
            }*/
        }

        public bool interceptGetVisibleUnits(UA ua, List<Unit> visibleUnits)
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

                        foreach (Unit unit in _map.units)
                        {
                            if (!unit.isDead && _map.getStepDist(ua.location, unit.location) < 4)
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

        public bool onPathfinding_AllowSecondPass(Location locA, Unit u, List<int> mapLayers, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates)
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

                if (u.task != null)
                {
                    if (u.task is Task_AttackArmy t_AttackArmy)
                    {
                        if (t_AttackArmy.other != null && t_AttackArmy.other.location.hex.z != u.location.hex.z)
                        {
                            return true;
                        }
                    }
                    else if (u.task is Task_AttackUnit t_AttackUnit)
                    {
                        if (t_AttackUnit.target != null && t_AttackUnit.target.location.hex.z != u.location.hex.z)
                        {
                            return true;
                        }
                    }
                    else if (u.task is Task_Bodyguard t_Guard)
                    {
                        if (t_Guard.target != null && t_Guard.target.location.hex.z != u.location.hex.z)
                        {
                            return true;
                        }
                    }
                    else if (u.task is Task_DisruptUA t_Disrupt)
                    {
                        if (t_Disrupt.other != null && t_Disrupt.other.location.hex.z != u.location.hex.z)
                        {
                            return true;
                        }
                    }
                    else if (u.task is Task_Follow t_Follow)
                    {
                        if (t_Follow.target != null && t_Follow.target.location.hex.z != u.location.hex.z)
                        {
                            return true;
                        }
                    }
                    
                }
            }

            if (result)
            {
                pathfindingDelegates.Remove(Pathfinding.delegate_LAYERBOUND);
            }

            return result;
        }

        public bool onPathfindingTadeRoute_AllowSecondPass(Location start, List<int> endPointMapLayers, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {
            return pathfindingDelegates.Remove(Pathfinding.delegate_TRADE_LAYERBOUND);
        }

        public void onPopulatingTradeRoutePathfindingDelegates(Location start, List<int> endPointMapLayers, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {
            if (start.settlement is Set_TombOfGods && start.map.overmind.god is God_Mammon)
            {
                pathfindingDelegates.Remove(Pathfinding.delegate_TRADE_UNDERGROUNDAWARENESS);
            }

            if (ModCore.opt_denseTradeRoutes && DensifyingTradeRoutes)
            {
                destinationValidityDelegates.Remove(Pathfinding.delegate_TRADEVALID_NODUPLICATES);
            }
        }

        public void onGetTradeRouteEndpoints(Map map, List<Location> endpoints)
        {
            if (ModCore.opt_dwarven_fortresses)
            {
                foreach (Location location in _map.locations)
                {
                    if (location.hex.z == 0 && location.settlement is Set_DwarvenCity dwarvenCity && dwarvenCity.subs.Any(sub => sub is Sub_DwarfFortress) && location.getNeighbours().Any(n => n.hex.z == 1))
                    {
                        if (!endpoints.Contains(location))
                        {
                            endpoints.Add(location);
                        }
                    }
                }
            }
        }

        public void onBuildTradeNetwork_EndOfProcess(Map map, ManagerTrade tradeManager, List<Location> endpoints)
        {
            if (_map.overmind.god is God_Mammon)
            {
                Location tomb = endpoints.FirstOrDefault(l => l.settlement is Set_TombOfGods);
                if (tomb != null)
                {
                    HashSet<int> linkedLayerIDs = new HashSet<int>();
                    foreach (TradeRoute route in tradeManager.routes)
                    {
                        if (route.start() == tomb)
                        {
                            linkedLayerIDs.Add(route.end().hex.z);
                        }
                        else if (route.end() == tomb)
                        {
                            linkedLayerIDs.Add(route.start().hex.z);
                        }
                    }

                    if (!linkedLayerIDs.Contains(0))
                    {
                        World.log($"CommunityLib: Finding Trade Route connecting Mammon's Tomb to the surface (map layer 0).");
                        Location[] routePath = Pathfinding.getTradeRouteFrom(tomb, 0);
                        if (routePath == null || routePath.Length < 2)
                        {
                            World.log($"CommunityLib: Failed to find Trade Route connecting Mammon's Tomb to the surface (map layer 0).");
                        }
                        else
                        {
                            World.log($"CommunityLib: Trade Route made connecting Mammon's Tomb to {routePath[routePath.Length - 1].getName()} on the surface (map layer 0).");
                            tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                        }
                    }

                    if (!linkedLayerIDs.Contains(1))
                    {
                        World.log($"CommunityLib: Finding Trade Route connecting Mammon's Tomb to the uderground (map layer 1).");
                        Location[] routePath = Pathfinding.getTradeRouteFrom(tomb, 1);
                        if (routePath == null || routePath.Length < 2)
                        {
                            World.log($"CommunityLib: Failed to find Trade Route connecting Mammon's Tomb to the underground (map layer 1).");
                        }
                        else
                        {
                            World.log($"CommunityLib: Trade Route made connecting Mammon's Tomb to {routePath[routePath.Length - 1].getName()} in the underground (map layer 1).");
                            tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                        }
                    }
                }
            }
        }

        public bool interceptAgentAI(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> challengeData, List<AgentAI.TaskData> taskData, List<Unit> visibleUnits)
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
                upstart.die(_map, "Died in the wilderness", null);
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

        public void onAgentAI_EndOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
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
    }
}
