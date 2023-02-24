using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Code;

namespace CommunityLib
{
    public class ModCore : Assets.Code.Modding.ModKernel
    {
        private Cache cache;

        private Filters filters;

        private List<Hooks> registeredHooks = new List<Hooks>();

        private UAENOverrideAI overrideAI;

        private DataTests dataTests;

        private bool runGeneralDataTests = false;

        private bool runSpecificDataTests = false;

        private bool patched = false;

        public override void onModsInitiallyLoaded()
        {
            if (!patched)
            {
                patched = true;
                HarmonyPatches.PatchingInit(this);
            }
        }

        public override void afterMapGenBeforeHistorical(Map map)
        {
            //Initialize subclasses.
            if (cache == null)
            {
                cache = new Cache();
            }
            else
            {
                cache.ClearCache();
            }

            if (filters == null)
            {
                filters = new Filters(cache, map);
            }

            if (overrideAI == null)
            {
                overrideAI = new UAENOverrideAI(cache, map);
            }

            if (runGeneralDataTests || runSpecificDataTests)
            {
                dataTests = new DataTests(cache, filters, map);
            }

            filters.afterMapGenBeforeHistorical(map);
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            filters.afterMapGenAfterHistorical(map);
        }

        public override void afterLoading(Map map)
        {
            //Initialize subclasses.
            if (cache == null)
            {
                cache = new Cache();
            }
            else
            {
                cache.ClearCache();
            }

            if (filters == null)
            {
                filters = new Filters(cache, map);
            }

            if (overrideAI == null)
            {
                overrideAI = new UAENOverrideAI(cache, map);
            }

            if (runGeneralDataTests || runSpecificDataTests)
            {
                dataTests = new DataTests(cache, filters, map);
            }

            filters.afterMapGenBeforeHistorical(map);
        }

        public override void onTurnStart(Map map)
        {
            filters.onTurnStart(map);

            if (runGeneralDataTests || runSpecificDataTests)
            {
                dataTests.RunDataTests(runGeneralDataTests, runSpecificDataTests);
            }
        }

        public override void onTurnEnd(Map map)
        {
            UpdateCommandableUnitVisibility();
        }

        public override void onAgentAIDecision(UA uA)
        {
            //Console.WriteLine("CommunityLib: Running onAgentAIDecision");
            switch (uA)
            {
                case UAEN_DeepOne deepOne:
                    if (overrideAI.overrideAI_DeepOne && overrideAI.deepOneCultChance < 1.0 && overrideAI.customChallenges_DeepOne.Count > 0)
                    {
                        overrideAI.OverrideAI_DeepOne(deepOne);
                    }
                    break;
                case UAEN_Ghast ghast:
                    if (overrideAI.overrideAI_Ghast && overrideAI.ghastMoveChance < 1.0 && overrideAI.customChallenges_Ghast.Count > 0)
                    {
                        overrideAI.OverrideAI_Ghast(ghast);
                    }
                    break;
                case UAEN_OrcUpstart upstart:
                    if (overrideAI.overrideAI_OrcUpstart && overrideAI.customChallenges_OrcUpstart.Count > 0)
                    {
                        overrideAI.OverrideAI_OrcUpstart(upstart);
                    }
                    break;
                case UAEN_Vampire vampire:
                    if (overrideAI.overrideAI_Vampire && (overrideAI.customChallenges_Vampire.Count > 0 || overrideAI.customChallenges_Vampire_Death.Count > 0))
                    {
                        overrideAI.OverrideAI_Vampire(vampire);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Forces an update of the internal location distance and steps maps, based off te game's data. This should only be called if and when a location or connection on the map is changed.
        /// </summary>
        public void updateLocationDistances()
        {
            filters.UpdateLocationDistances();
        }

        /// <summary>
        /// Forces an update of the visibility of commandable units. This only updates the units that can see the commandable units, not the units that the commandable units can see.
        /// This should only be called if excecuting an effect on units that can see the commandable units during the player turn, such as a taunt challenge or power.
        /// </summary>
        public void UpdateCommandableUnitVisibility()
        {
            filters.UpdateCommandableUnitVisibility();
        }

        /// <summary>
        /// Returns the instance of the Community Library Cache class.
        /// </summary>
        /// <returns></returns>
        public Cache GetCache()
        {
            return cache;
        }

        /// <summary>
        /// <para></para>Returns the instance of the UAENOverrideAI class.
        /// <para>A reference to this class is required to add a challenge to a UAENOverrideAI's challenge list, to read the challenge list, or to disable one or more of the Community Library's UAENOverrideAIs.</para>
        /// </summary>
        /// <returns></returns>
        public UAENOverrideAI GetUAENOverrideAI()
        {
            return overrideAI;
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
