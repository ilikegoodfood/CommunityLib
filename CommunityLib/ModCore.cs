﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;
using UnityEngine;

namespace CommunityLib
{
    public class ModCore : ModKernel
    {
        private static ModCore core;

        public ModData data;

        public static double versionID;

        public Dictionary<Unit, Dictionary<object, Dictionary<string, double>>> randStore;

        private Map map;

        private List<Hooks> registeredHooks = new List<Hooks>();

        private Dictionary<Type, List<Type>> settlementTypesForOrcExpansion = new Dictionary<Type,  List<Type>>();

        private AgentAI agentAI;

        private Hooks hooks;

        public Pathfinding pathfinding;

        private UAENOverrideAI overrideAI;

        private bool patched = false;

        public static bool opt_SpawnShipwrecks = false;

        public static bool opt_forceShipwrecks = false;

        public static bool opt_panToHolyOrderScreen = true;

        public static bool opt_ophanimFaithTomb = true;

        public static ModCore Get() => core;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit();
            }
        }

        public override void receiveModConfigOpts_bool(string optName, bool value)
        {
            switch(optName)
            {
                case "Spawn Shipwrecks":
                    opt_SpawnShipwrecks = value;
                    break;
                case "Pan To Holy Order Screen":
                    opt_panToHolyOrderScreen = value;
                    break;
                case "Show Ophanim's Faith at Elder Tomb":
                    opt_ophanimFaithTomb = value;
                    break;
                default:
                    break;
            }
        }

        public override void beforeMapGen(Map map)
        {
            core = this;
            opt_forceShipwrecks = false;
            this.map = map;

            // Set local variables;
            core.randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();

            //Initialize subclasses.
            core.data = new ModData(map);
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit(map);

            core.pathfinding = new Pathfinding();

            core.agentAI = new AgentAI(map);

            core.overrideAI = new UAENOverrideAI(map);

            core.hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
            eventModifications();
        }

        public override void afterLoading(Map map)
        {
            core = this;
            this.map = map;

            if (core.data == null)
            {
                core.data = new ModData(map);
            }
            core.data.onLoad(map);
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit(map);

            // Set local variables
            if (core.randStore == null)
            {
                core.randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();
            }

            //Initialize subclasses.
            if (core.pathfinding == null)
            {
                pathfinding = new Pathfinding();
            }

            core.agentAI = new AgentAI(map);

            core.overrideAI = new UAENOverrideAI(map);

            core.hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
            eventModifications();
        }

        private void getModKernels (Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                switch (kernel.GetType().Namespace)
                {
                    case "ShadowsInsectGod.Code":
                        core.data.addModAssembly("Cordyceps", new ModData.ModIntegrationData(kernel.GetType().Assembly));

                        if (core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
                        {
                            Type godType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.God_Insect", false);
                            if (godType != null)
                            {
                                intDataCord.typeDict.Add("God", godType);
                                intDataCord.methodInfoDict.Add("God.eat", AccessTools.Method(godType, "eat", new Type[] { typeof(int) }));
                                intDataCord.fieldInfoDict.Add("God.phHome", AccessTools.Field(godType, "phHome"));
                            }

                            Type droneType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UAEN_Drone", false);
                            if (droneType != null)
                            {
                                intDataCord.typeDict.Add("Drone", droneType);
                                intDataCord.methodInfoDict.Add("Drone.turnTickAI", AccessTools.Method(droneType, "turnTickAI", new Type[0]));
                                intDataCord.fieldInfoDict.Add("Drone.prey", AccessTools.Field(droneType, "prey"));
                            }

                            Type hiveType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Set_Hive", false);
                            if (hiveType != null)
                            {
                                intDataCord.typeDict.Add("Hive", hiveType);
                            }

                            Type larvalType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Pr_LarvalMass", false);
                            if (larvalType != null)
                            {
                                intDataCord.typeDict.Add("LarvalMass", larvalType);
                            }

                            Type phFeedType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Pr_Pheromone_Feeding", false);
                            if (phFeedType != null)
                            {
                                intDataCord.typeDict.Add("phFeed", phFeedType);
                            }

                            Type seekType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_SeekPrey", false);
                            if (seekType != null)
                            {
                                intDataCord.typeDict.Add("SeekTask", seekType);
                            }

                            Type exploreType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_Explore", false);
                            if (exploreType != null)
                            {
                                intDataCord.typeDict.Add("ExploreTask", exploreType);
                            }

                            Type homeType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_GoHome", false);
                            if (homeType != null)
                            {
                                intDataCord.typeDict.Add("GoHomeTask", homeType);
                            }
                        }
                        break;
                    case "Wonderblunder_DeepOnes":
                        ModData.ModIntegrationData intDataDOPlus = new ModData.ModIntegrationData(kernel.GetType().Assembly);
                        core.data.addModAssembly("DeepOnesPlus", intDataDOPlus);

                        if (core.data.tryGetModAssembly("DeepOnesPlus", out intDataDOPlus))
                        {
                            Type kernelType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.Modcore", false);
                            if (kernelType != null)
                            {
                                intDataDOPlus.typeDict.Add("Kernel", kernelType);
                            }

                            intDataDOPlus.methodInfoDict.Add("getAbyssalItem", AccessTools.Method(kernelType, "getItemFromAbyssalPool", new Type[] { typeof(Map), typeof(UA) }));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void orcExpansionDefaults()
        {
            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
            registerSettlementTypeForOrcExpansion(typeof(Set_Shipwreck));
            registerSettlementTypeForOrcExpansion(typeof(Set_MinorOther), new Type[] { typeof(Sub_WitchCoven), typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont), typeof(Sub_Temple) });
        }

        public void eventModifications()
        {
            Dictionary<string, EventRuntime.Field> fields = EventRuntime.fields;
            Dictionary<string, EventRuntime.Property> properties = EventRuntime.properties;

            if (fields.ContainsKey("is_elder_tomb"))
            {
                fields["is_elder_tomb"] = new EventRuntime.TypedField<bool>((EventContext c) => checkIsElderTomb(c.location));
            }

            if (fields.ContainsKey("is_city_ruins"))
            {
                fields["is_city_ruins"] = new EventRuntime.TypedField<bool>((EventContext c) => c.location != null && c.location.settlement != null && c.location.settlement is Set_CityRuins && !(c.location.settlement is Set_Shipwreck));
            }

            if (properties.ContainsKey("TELEPORT_TO_ELDER_TOMB"))
            {
                properties["TELEPORT_TO_ELDER_TOMB"] = new EventRuntime.TypedProperty<string>(delegate (EventContext c, string v)
                {
                    Location location = null;

                    foreach (Location loc in c.map.locations)
                    {
                        if (checkIsElderTomb(loc))
                        {
                            location = loc;
                            break;
                        }
                    }

                    if (location == null)
                    {
                        List<Location> locations = new List<Location>();
                        foreach (Location loc in c.map.locations)
                        {
                            if (!loc.isOcean)
                            {
                                locations.Add(loc);
                            }
                        }

                        if (locations.Count == 1)
                        {
                            location = locations[0];
                        }
                        if (locations.Count > 1)
                        {
                            location = locations[Eleven.random.Next(locations.Count)];
                        }
                    }

                    if (location != null)
                    {
                        c.unit.location.units.Remove(c.unit);
                        c.unit.location = location;
                        location.units.Add(c.unit);
                    }
                });
            }

            if (!properties.ContainsKey("CREATE_SHIPWRECK"))
            {
                properties.Add(
                        "CREATE_SHIPWRECK",
                        new EventRuntime.TypedProperty<string>(delegate (EventContext c, string _)
                        {
                            spawnShipwreck(c.location);
                        })
                    );
            }

            if (!properties.ContainsKey("PLUNDER_SHIPWRECK"))
            {
                properties.Add(
                        "PLUNDER_SHIPWRECK",
                        new EventRuntime.TypedProperty<int>(delegate (EventContext c, int v)
                        {
                            if (c.location.settlement != null)
                            {
                                Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                                if (wreck != null)
                                {
                                    wreck.integrity -= v * wreck.integrityLossPlunder;
                                    if (wreck.integrity <= 0.0)
                                    {
                                        wreck.removeWreck();
                                    }
                                }
                            }
                        })
                    );
            }

            if (!properties.ContainsKey("INCREASE_SHIPWRECK_ALLURE"))
            {
                properties.Add(
                    "INCREASE_SHIPWRECK_ALLURE",
                    new EventRuntime.TypedProperty<int>(delegate (EventContext c, int v)
                    {
                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.allure += v;
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("REINFORCE_SHIPRECK"))
            {
                properties.Add(
                    "REINFORCE_SHIPRECK",
                    new EventRuntime.TypedProperty<bool>(delegate (EventContext c, bool v)
                    {
                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.reinforced = v;
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("DESTROY_SHIPWRECK"))
            {
                properties.Add(
                    "DESTROY_SHIPWRECK",
                    new EventRuntime.TypedProperty<string>(delegate (EventContext c, string v)
                    {
                        if (v == "")
                        {
                            v = "Shipwreck Destroyed by Event Outcome";
                        }

                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.removeWreck(v);
                            }
                        }
                    })
                );
            }
        }

        public override void onTurnEnd(Map map)
        {
            cleanRandStore(map);
        }

        public override void onChallengeComplete(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            OnChallengeComplete.processChallenge(challenge, ua, task_PerformChallenge);
        }

        public override void onCheatEntered(string command)
        {
            Cheats.parseCheat(command, map);
        }

        public override void onGraphicalHexUpdated(GraphicalHex graphicalHex)
        {
            if (opt_ophanimFaithTomb)
            {
                if (graphicalHex.map.masker.mask == MapMaskManager.maskType.RELIGION)
                {
                    if (map.overmind.god is God_Ophanim opha && opha.faith != null)
                    {
                        Location location = graphicalHex.hex.location;
                        if (location != null && location.settlement is Set_TombOfGods)
                        {
                            HolyOrder targetOrder = graphicalHex.map.world.ui.uiScrollables.scrollable_threats.targetOrder;
                            if (targetOrder == null || targetOrder == opha.faith)
                            {
                                Color colour = opha.faith.color;
                                if (colour.a > 0f && colour.r > 0f && colour.g > 0f && colour.b > 0f)
                                {
                                    graphicalHex.terrainLayer.color = colour;
                                    graphicalHex.locLayer.color = colour;
                                    graphicalHex.mask.enabled = false;
                                }
                            }
                            else
                            {
                                graphicalHex.mask.color = new Color(0f, 0f, 0f, 0.75f);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the instance of the AgentAI.
        /// </summary>
        /// <returns></returns>
        public AgentAI GetAgentAI()
        {
            return core.agentAI;
        }

        /// <summary>
        /// Registers an instance of the Hooks class to the Community Library. Only registered instances will be called by the hooks included in the Comunity Library.
        /// </summary>
        /// <param name="hook"></param>
        public void RegisterHooks(Hooks hook)
        {
            if (hook != null && !core.registeredHooks.Contains(hook))
            {
                core.registeredHooks.Add(hook);
            }
        }

        public List<Hooks> GetRegisteredHooks()
        {
            return core.registeredHooks;
        }

        /// <summary>
        /// Safely checks for a value in randStore. If none exists, it sets the value to the new value.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="innerKey"></param>
        /// <param name="stringKey"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public double tryGetRand(Unit outerKey, object innerKey, string stringKey, double newValue)
        {
            if (outerKey == null || innerKey == null || stringKey == null || stringKey == "")
            {
                return -1.0;
            }

            if (!core.randStore.ContainsKey(outerKey))
            {
                core.randStore.Add(outerKey, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!core.randStore[outerKey].ContainsKey(innerKey))
            {
                core.randStore[outerKey].Add(innerKey, new Dictionary<string, double>());
            }
            if (!core.randStore[outerKey][innerKey].ContainsKey(stringKey))
            {
                core.randStore[outerKey][innerKey].Add(stringKey, newValue);
            }

            return core.randStore[outerKey][innerKey][stringKey];
        }

        /// <summary>
        /// Safely sets the value to randStore.
        /// </summary>
        /// <param name="outerKey"></param>
        /// <param name="innerKey"></param>
        /// <param name="stringKey"></param>
        /// <param name="value"></param>
        public void setRand(Unit outerKey, object innerKey, string stringKey, double value)
        {
            if (outerKey == null || innerKey == null || stringKey == null || stringKey == "")
            {
                return;
            }

            if (!core.randStore.ContainsKey(outerKey))
            {
                core.randStore.Add(outerKey, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!core.randStore[outerKey].ContainsKey(innerKey))
            {
                core.randStore[outerKey].Add(innerKey, new Dictionary<string, double>());
            }
            if (!core.randStore[outerKey][innerKey].ContainsKey(stringKey))
            {
                core.randStore[outerKey][innerKey].Add(stringKey, value);
            }
            else
            {
                core.randStore[outerKey][innerKey][stringKey] = value;
            }
        }

        internal void cleanRandStore(Map map)
        {
            List<Unit> outerKeys = new List<Unit>();
            List<Tuple<Unit, Unit>> innerKeys = new List<Tuple<Unit, Unit>>();
            foreach (Unit outerKey in core.randStore.Keys)
            {
                if (outerKey.isDead || !map.units.Contains(outerKey))
                {
                    outerKeys.Add(outerKey);
                }
                else
                {
                    foreach (object innerKey in core.randStore[outerKey])
                    {
                        if (innerKey is Unit unit && (unit.isDead || !map.units.Contains(outerKey)))
                        {
                            innerKeys.Add(new Tuple<Unit, Unit>(outerKey, unit));
                        }
                    }
                }
            }

            foreach (Unit unit in outerKeys)
            {
                core.randStore.Remove(unit);
            }

            foreach (Tuple<Unit, Unit> tuple in innerKeys)
            {
                core.randStore[tuple.Item1].Remove(tuple.Item2);
            }
        }

        public bool registerSettlementTypeForOrcExpansion(Type t)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || core.settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            core.settlementTypesForOrcExpansion.Add(t, null);
            return true;
        }

        public bool registerSettlementTypeForOrcExpansion(Type t, List<Type> subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || core.settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            core.settlementTypesForOrcExpansion.Add(t, subsettlementBlacklist);
            return true;
        }

        public bool registerSettlementTypeForOrcExpansion(Type t, Type[] subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || core.settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            core.settlementTypesForOrcExpansion.Add(t, subsettlementBlacklist?.ToList() ?? null);
            return true;
        }

        public bool removeSettlementTypeForOrcExpansion(Type t, out List<Type> subsettlementBlacklist)
        {
            if (t.IsSubclassOf(typeof(Settlement)) && core.settlementTypesForOrcExpansion.TryGetValue(t, out subsettlementBlacklist))
            {
                return core.settlementTypesForOrcExpansion.Remove(t);
            }

            subsettlementBlacklist = null;
            return false;
        }

        internal Dictionary<Type, List<Type>> getSettlementTypesForOrcExpanion()
        {
            return core.settlementTypesForOrcExpansion;
        }

        public bool tryGetSettlementTypeForOrcExpansion(Type t, out List<Type> subsettlementBlacklist)
        {
            if (t.IsSubclassOf(typeof(Settlement)) && core.settlementTypesForOrcExpansion.TryGetValue(t, out subsettlementBlacklist))
            {
                return true;
            }

            subsettlementBlacklist = null;
            return false;
        }

        public bool checkIsElderTomb(Location location)
        {
            if (location.settlement is Set_TombOfGods)
            {
                return true;
            }

            foreach (Hooks hook in core.GetRegisteredHooks())
            {
                bool retValue = hook.onEvent_IsLocationElderTomb(location);
                if (retValue)
                {
                    return true;
                }
            }

            return false;
        }

        public int getTravelTimeTo(Unit u, Location location)
        {
            int travelTime = 0;

            Location[] path = core.pathfinding.getPathTo(u.location, location, u);
            if (path != null)
            {
                travelTime = (int)Math.Ceiling((double)path.Count() / (double)u.getMaxMoves());
            }

            if (travelTime > 0)
            {
                foreach (Hooks hook in core.GetRegisteredHooks())
                {
                    travelTime = hook.onUnitAI_GetsDistanceToLocation(u, location, travelTime);
                }
            }

            return Math.Max(0, travelTime);
        }

        public void forceShipwrecks()
        {
            opt_forceShipwrecks = true;
        }

        public void spawnShipwreck(Location location)
        {
            if (location.settlement == null)
            {
                location.settlement = new Set_Shipwreck(location);
            }
            else
            {
                Sub_Shipwreck wreck = location.settlement.subs.OfType<Sub_Shipwreck>().FirstOrDefault();

                if (wreck == null)
                {
                    wreck = new Sub_Shipwreck(location.settlement, location);
                    location.settlement.subs.Add(wreck);
                }
                else
                {
                    wreck.integrity += Eleven.random.Next(6) + Eleven.random.Next(6);
                }
            }
        }
    }
}
