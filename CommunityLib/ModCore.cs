using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;

namespace CommunityLib
{
    public class ModCore : ModKernel
    {
        public static ModCore core;

        public ModData data;

        public static double versionID;

        public Dictionary<UA, Dictionary<object, Dictionary<string, double>>> randStore;

        private List<Hooks> registeredHooks = new List<Hooks>();

        private Dictionary<Type, List<Type>> settlementTypesForOrcExpansion = new Dictionary<Type,  List<Type>>();

        private AgentAI agentAI;

        private Hooks hooks;

        public Pathfinding pathfinding;

        private UAENOverrideAI overrideAI;

        private bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit();
            }
        }

        public override void beforeMapGen(Map map)
        {
            // Set local variables;
            core.randStore = new Dictionary<UA, Dictionary<object, Dictionary<string, double>>>();

            //Initialize subclasses.
            data = new ModData(map);
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit();

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

            if (core.data == null)
            {
                core.data = new ModData(map);
            }
            core.data.onLoad(map);
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit();

            // Set local variables
            if (core.randStore == null)
            {
                core.randStore = new Dictionary<UA, Dictionary<object, Dictionary<string, double>>>();
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
                    default:
                        break;
                }
            }
        }

        public void orcExpansionDefaults()
        {
            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
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
        }

        public override void onTurnEnd(Map map)
        {
            cleanRandStore();
        }

        public override void onCheatEntered(string command)
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
                    default:
                        break;
                }
            }
        }

        public void cheat_InfluenceHolyOrder(int value, bool isElder = false)
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
        /// <param name="object"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public double tryGetRand(UA ua, object @object, string key, double newValue)
        {
            if (ua == null || key == null)
            {
                return -1.0;
            }

            if (!core.randStore.ContainsKey(ua))
            {
                core.randStore.Add(ua, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!core.randStore[ua].ContainsKey(@object))
            {
                core.randStore[ua].Add(@object, new Dictionary<string, double>());
            }
            if (!core.randStore[ua][@object].ContainsKey(key))
            {
                core.randStore[ua][@object].Add(key, newValue);
            }

            return core.randStore[ua][@object][key];
        }

        /// <summary>
        /// Safely sets the value to randStore.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setRand(UA ua, object obj, string key, double value)
        {
            if (!core.randStore.ContainsKey(ua))
            {
                core.randStore.Add(ua, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!core.randStore[ua].ContainsKey(obj))
            {
                core.randStore[ua].Add(obj, new Dictionary<string, double>());
            }
            if (!core.randStore[ua][obj].ContainsKey(key))
            {
                core.randStore[ua][obj].Add(key, value);
            }
            else
            {
                core.randStore[ua][obj][key] = value;
            }
        }

        internal void cleanRandStore()
        {
            List<UA> deadAgents = new List<UA>();
            foreach (UA ua in core.randStore.Keys)
            {
                if (ua.isDead || ua.homeLocation == -1)
                {
                    deadAgents.Add(ua);
                }
            }

            foreach (UA ua in deadAgents)
            {
                core.randStore.Remove(ua);
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
                bool retValue = hook.onIsElderTomb(location);
                if (retValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
