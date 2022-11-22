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
        public Cache cache;
        private Filters filters;
        private UAENOverrideAI overrideAI;

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

            filters.afterMapGenBeforeHistorical(map);
        }

        public override void afterMapGenAfterHistorical(Map map)
        {
            filters.afterMapGenAfterHistorical(map);
        }

        public override void onTurnStart(Map map)
        {
            filters.onTurnStart(map);

            // testRoutine(map);
            // testSpecific(map);
        }

        public override void onTurnEnd(Map map)
        {
            filters.onTurnEnd(map);
        }

        public override void onAgentAIDecision(UA uA)
        {
            //Console.WriteLine("CommunityLib: Running onAgentAIDecision");
            switch (uA)
            {
                case UAEN_OrcUpstart _:
                    if (overrideAI.overrideAI_OrcUpstart && overrideAI.customChallenges_OrcUpstart.Count > 0)
                    {
                        overrideAI.OverrideAI_OrcUpstart(uA);
                    }
                    break;
                default:
                    break;
            }
        }

        private void testSpecific(Map map)
        {
            if (cache.settlementsByType.ContainsKey(typeof(SettlementHuman)))
            {
                List<SettlementHuman> humanSettlements = cache.settlementsByType[typeof(SettlementHuman)] as List<SettlementHuman>;
                SettlementHuman humanSettlement = null;
                if (humanSettlements != null && humanSettlements.Count > 0)
                {
                    humanSettlement = humanSettlements[Eleven.random.Next(humanSettlements.Count)];
                }
                if (humanSettlement != null)
                {
                    map.addUnifiedMessage(humanSettlement.location, humanSettlement.location, "Cache Testing", "Item Found: " + humanSettlement.getName(), "CommunityLib Test");
                }
            }
        }

        private void testRoutine(Map map)
        {
            Console.WriteLine("CommunityLib Caher Test: Starting test routine");
            string messageString = "This is a batch of tests for the cacher. It retrieved the following items at random:";

            Unit randCommandableUnit = null;
            Unit randUnit = null;

            int rand;
            IList value;
            if (cache.socialGroupsByType.TryGetValue(typeof(SocialGroup), out value) && value != null && value.Count > 0)
            {
                SocialGroup randSG;
                for (int i = 0; i < 3; i++)
                {
                    rand = Eleven.random.Next(value.Count);
                    List<SocialGroup> socialGroups = value as List<SocialGroup>;
                    randSG = socialGroups[rand];
                    messageString += " Random Social Group " + (i + 1).ToString() + " is " + randSG.getName() + " of type " + randSG.GetType().Name + ".";
                    Console.WriteLine("CommunityLib Cacher Test: Random Social Group " + (i + 1).ToString() + " is " + randSG.getName() + " of type " + randSG.GetType().Name + ".");
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Social Groups in Cache");
            }

            if (cache.socialGroupsByTypeExclusive.TryGetValue(typeof(SG_Orc), out value) && value != null && value.Count > 0)
            {
                rand = Eleven.random.Next(value.Count);
                List<SG_Orc> orcSocieties = value as List<SG_Orc>;
                SG_Orc randOrcSG = orcSocieties[rand];
                messageString += " Random Orc Society is " + randOrcSG.getName() + " of type " + randOrcSG.GetType().Name + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random Orc Society is " + randOrcSG.getName() + " of type " + randOrcSG.GetType().Name + ".");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Orc Social Groups in Cache");
            }

            if (cache.socialGroupsByTypeExclusive.TryGetValue(typeof(HolyOrder), out value) && value != null && value.Count > 0)
            {
                rand = Eleven.random.Next(value.Count);
                List<HolyOrder> humanHolyOrders = value as List<HolyOrder>;
                HolyOrder randHumanHolyOrder = humanHolyOrders[rand];
                messageString += " Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in " + randHumanHolyOrder.seat.settlement.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in Location " + randHumanHolyOrder.seat.settlement.getName() + ".");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Human Holy Orders in Cache");
            }

            if (cache.commandableUnitsByType.TryGetValue(typeof(Unit), out value) && value != null && value.Count > 0)
            {
                rand = Eleven.random.Next(value.Count);
                List<Unit> commandableAgents = value as List<Unit>;
                randCommandableUnit = commandableAgents[rand];
                messageString += " Random commandable Unit is " + randCommandableUnit.getName() + " of Type " + randCommandableUnit.GetType().Name + " at Location " + randCommandableUnit.location.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random commandable Unit is " + randCommandableUnit.getName() + " of Type " + randCommandableUnit.GetType().Name + " at Location " + randCommandableUnit.location.getName() + ".");
                string visMessageString = " " + randCommandableUnit.getName();
                if (cache.unitVisibleToUnits.ContainsKey(randCommandableUnit) && cache.unitVisibleToUnits[randCommandableUnit] != null && cache.unitVisibleToUnits[randCommandableUnit].Count > 0)
                {
                    visMessageString += " is visible to: ";
                    foreach (Unit u in cache.unitVisibleToUnits[randCommandableUnit])
                    {
                        visMessageString += u.getName() + ", ";
                    }
                    visMessageString = visMessageString.Substring(0, visMessageString.Length - 2);
                    visMessageString += ".";
                }
                else
                {
                    visMessageString += " is not visible to anyone.";
                }

                if (cache.visibleUnitsByUnit.ContainsKey(randCommandableUnit) && cache.visibleUnitsByUnit[randCommandableUnit] != null && cache.visibleUnitsByUnit[randCommandableUnit].Count > 0)
                {
                    visMessageString += " It can can see: ";
                    foreach (Unit u in cache.visibleUnitsByUnit[randCommandableUnit])
                    {
                        visMessageString += u.getName() + ", ";
                    }
                    visMessageString = visMessageString.Substring(0, visMessageString.Length - 2);
                    visMessageString += ".";
                }
                else
                {
                    visMessageString += " It cannot see anyone.";
                }

                messageString += visMessageString;
                Console.WriteLine("CommunityLib Cacher Test:" + visMessageString);
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Commandable Units in Cache");
            }

            if (cache.unitsByType.TryGetValue(typeof(Unit), out value) && value != null && value.Count > 0)
            {
                rand = Eleven.random.Next(value.Count);
                List<Unit> units = value as List<Unit>;
                randUnit = units[rand];
                while (randUnit == randCommandableUnit)
                {
                    rand = Eleven.random.Next(value.Count);
                    randUnit = units[rand];
                }

                messageString += " Random unit is " + randUnit.getName() + " of type " + randUnit.GetType().Name + ", belonging to " + randUnit.society.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random unit is " + randUnit.getName() + " of type " + randUnit.GetType().Name + ", belonging to " + randUnit.society.getName() + ".");

                if (randCommandableUnit != null)
                {
                    double distance = cache.distanceByLocationsFromLocation[randCommandableUnit.location][randUnit.location];
                    Console.WriteLine("Got Distance");
                    int steps = map.stepDistMap[randCommandableUnit.location.index][randUnit.location.index];
                    Console.WriteLine("Got Steps");


                    messageString += " " + randCommandableUnit.getName() + " and " + randUnit.getName() + " are " + steps.ToString() + " steps appart, a distance of " + distance.ToString() + ".";
                    Console.WriteLine("CommunityLib Cacher Test: " + randCommandableUnit.getName() + " and " + randUnit.getName() + " are " + steps.ToString() + " steps appart, a distance of " + distance.ToString() + ".");
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Units in Cache");
            }

            if (randUnit != null && randCommandableUnit != null)
            {
                map.addUnifiedMessage(randUnit.location, randUnit, "Cache Testing", messageString, "CommunityLib Test");
                Console.WriteLine("CommunityLib Cacher Test: Test routine completed successfully");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: Test routine failed. randUnit or randCommandableAgent was null.");
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
