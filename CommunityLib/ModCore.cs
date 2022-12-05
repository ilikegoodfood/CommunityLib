using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Assets.Code;
using UnityEngine.Diagnostics;

namespace CommunityLib
{
    public class ModCore : Assets.Code.Modding.ModKernel
    {
        private Cache cache;

        private Filters filters;

        private UAENOverrideAI overrideAI;

        private DataTests dataTests;

        private bool runGeneralDataTests = false;

        private bool runSpecificDataTests = false;

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

        public void updateLocationDistances()
        {
            filters.UpdateLocationDistances();
        }

        public void UpdateCommandableUnitVisibility()
        {
            filters.UpdateCommandableUnitVisibility();
        }

        public Cache GetCache()
        {
            return cache;
        }

        public UAENOverrideAI GetUAENOverrideAI()
        {
            return overrideAI;
        }
    }
}
