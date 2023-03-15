using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Code;

namespace CommunityLib
{
    public class ModCore : Assets.Code.Modding.ModKernel
    {
        public static ModCore modCore;

        private List<Hooks> registeredHooks = new List<Hooks>();

        private AgentAI agentAI;

        private Hooks hooks;

        private UAENOverrideAI overrideAI;

        private bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit(this);
            }

            modCore = this;
        }

        public override void beforeMapGen(Map map)
        {
            //Initialize subclasses.
            if (agentAI == null)
            {
                agentAI = new AgentAI(this, map);
            }

            if (hooks == null)
            {
                hooks = new HooksInternal(map);
                RegisterHooks(hooks);
            }

            if (overrideAI == null)
            {
                overrideAI = new UAENOverrideAI(this, map);
            }
        }

        public override void afterLoading(Map map)
        {
            //Initialize subclasses.
            if (agentAI == null)
            {
                agentAI = new AgentAI(this, map);
            }

            if (hooks == null)
            {
                hooks = new HooksInternal(map);
                RegisterHooks(hooks);
            }

            if (overrideAI == null)
            {
                overrideAI = new UAENOverrideAI(this, map);
            }
        }

        public override void onAgentAIDecision(UA uA)
        {
            //Console.WriteLine("CommunityLib: Running onAgentAIDecision");
            switch (uA)
            {
                case UAEN_DeepOne deepOne:
                    agentAI.onTurnTickAI(deepOne, AgentAI.InputParams.newDefault());
                    break;
                case UAEN_Ghast ghast:
                    agentAI.onTurnTickAI(ghast, AgentAI.InputParams.newDefault());
                    break;
                case UAEN_OrcUpstart upstart:
                    agentAI.onTurnTickAI(upstart, AgentAI.InputParams.newDefault());
                    break;
                case UAEN_Vampire vampire:
                    agentAI.onTurnTickAI(vampire, AgentAI.InputParams.newDefault());
                    break;
                default:
                    break;
            }
        }

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
    }
}
