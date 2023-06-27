using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;
using static Assets.Code.RelObj;
using static CommunityLib.AgentAI;

namespace CommunityLib
{
    public class ModCore : Assets.Code.Modding.ModKernel
    {
        public static ModCore core;

        public static double versionID;

        public Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>> randStore;

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
            core.randStore = new Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>>();

            //Initialize subclasses.
            core.pathfinding = new Pathfinding();

            core.agentAI = new AgentAI(map);

            core.overrideAI = new UAENOverrideAI(map);

            core.hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
        }

        public override void afterLoading(Map map)
        {
            core = this;

            // Set local variables
            if (core.randStore == null)
            {
                core.randStore = new Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>>();
            }

            if (core.pathfinding == null)
            {
                pathfinding = new Pathfinding();
            }

            //Initialize subclasses.
            core.agentAI = new AgentAI(map);

            core.overrideAI = new UAENOverrideAI(map);

            core.hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
        }

        public void orcExpansionDefaults()
        {
            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
            registerSettlementTypeForOrcExpansion(typeof(Set_MinorOther), new Type[] { typeof(Sub_WitchCoven), typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont), typeof(Sub_Temple) });
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
        /// <param name="challengeData"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public double tryGetRand(UA ua, ChallengeData challengeData, string key, double newValue)
        {
            if (ua == null || key == null)
            {
                return -1.0;
            }

            if (!core.randStore.ContainsKey(ua))
            {
                core.randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!core.randStore[ua].ContainsKey(challengeData))
            {
                core.randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!core.randStore[ua][challengeData].ContainsKey(key))
            {
                core.randStore[ua][challengeData].Add(key, newValue);
            }

            return core.randStore[ua][challengeData][key];
        }

        /// <summary>
        /// Safely sets the value to randStore.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="challengeData"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setRand(UA ua, ChallengeData challengeData, string key, double value)
        {
            if (!core.randStore.ContainsKey(ua))
            {
                core.randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!core.randStore[ua].ContainsKey(challengeData))
            {
                core.randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!core.randStore[ua][challengeData].ContainsKey(key))
            {
                core.randStore[ua][challengeData].Add(key, value);
            }
            else
            {
                core.randStore[ua][challengeData][key] = value;
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
    }
}
