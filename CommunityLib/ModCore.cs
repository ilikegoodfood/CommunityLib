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
            //Initialize subclasses.
            randStore = new Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>>();

            agentAI = new AgentAI(map);

            hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            overrideAI = new UAENOverrideAI(map);

            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
            registerSettlementTypeForOrcExpansion(typeof(Set_MinorOther), new Type[] { typeof(Sub_WitchCoven), typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont) });
        }

        public override void afterLoading(Map map)
        {
            core = this;

            if (randStore == null)
            {
                randStore = new Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>>();
            }

            //Initialize subclasses.
            agentAI = new AgentAI(map);

            hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            overrideAI = new UAENOverrideAI(map);

            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
            registerSettlementTypeForOrcExpansion(typeof(Set_MinorOther), new Type[] { typeof(Sub_WitchCoven) });
        }

        public override void onTurnEnd(Map map)
        {
            cleanRandStore();
        }

        /// <summary>
        /// Returns the instance of the AgentAI.
        /// </summary>
        /// <returns></returns>
        public AgentAI GetAgentAI()
        {
            return agentAI;
        }

        /// <summary>
        /// Registers an instance of the Hooks class to the Community Library. Only registered instances will be called by the hooks included in the Comunity Library.
        /// </summary>
        /// <param name="hook"></param>
        public void RegisterHooks(Hooks hook)
        {
            if (hook != null && !registeredHooks.Contains(hook))
            {
                registeredHooks.Add(hook);
            }
        }

        internal List<Hooks> GetRegisteredHooks()
        {
            return registeredHooks;
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

            if (!randStore.ContainsKey(ua))
            {
                randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!randStore[ua].ContainsKey(challengeData))
            {
                randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!randStore[ua][challengeData].ContainsKey(key))
            {
                randStore[ua][challengeData].Add(key, newValue);
            }

            return randStore[ua][challengeData][key];
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
            if (!randStore.ContainsKey(ua))
            {
                randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!randStore[ua].ContainsKey(challengeData))
            {
                randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!randStore[ua][challengeData].ContainsKey(key))
            {
                randStore[ua][challengeData].Add(key, value);
            }
            else
            {
                randStore[ua][challengeData][key] = value;
            }
        }

        internal void cleanRandStore()
        {
            List<UA> deadAgents = new List<UA>();
            foreach (UA ua in randStore.Keys)
            {
                if (ua.isDead)
                {
                    deadAgents.Add(ua);
                }
            }

            foreach (UA ua in deadAgents)
            {
                randStore.Remove(ua);
            }
        }

        public bool registerSettlementTypeForOrcExpansion(Type t)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            settlementTypesForOrcExpansion.Add(t, null);
            return true;
        }

        public bool registerSettlementTypeForOrcExpansion(Type t, List<Type> subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            settlementTypesForOrcExpansion.Add(t, subsettlementBlacklist);
            return true;
        }

        public bool registerSettlementTypeForOrcExpansion(Type t, Type[] subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) || settlementTypesForOrcExpansion.ContainsKey(t))
            {
                return false;
            }

            settlementTypesForOrcExpansion.Add(t, subsettlementBlacklist?.ToList() ?? null);
            return true;
        }

        public bool removeSettlementTypeForOrcExpansion(Type t, out List<Type> subsettlementBlacklist)
        {
            if (t.IsSubclassOf(typeof(Settlement)) && settlementTypesForOrcExpansion.TryGetValue(t, out subsettlementBlacklist))
            {
                return settlementTypesForOrcExpansion.Remove(t);
            }

            subsettlementBlacklist = null;
            return false;
        }

        internal Dictionary<Type, List<Type>> getSettlementTypesForOrcExpanion()
        {
            return settlementTypesForOrcExpansion;
        }

        public bool tryGetSettlementTypeForOrcExpansion(Type t, out List<Type> subsettlementBlacklist)
        {
            if (t.IsSubclassOf(typeof(Settlement)) && settlementTypesForOrcExpansion.TryGetValue(t, out subsettlementBlacklist))
            {
                return true;
            }

            subsettlementBlacklist = null;
            return false;
        }
    }
}
