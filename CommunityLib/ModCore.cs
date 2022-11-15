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

        public override void afterMapGenBeforeHistorical(Map map)
        {
            base.afterMapGenBeforeHistorical(map);

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

            filters.UpdateLocationDistances();
        }

        public override void onTurnStart(Map map)
        {
            base.onTurnStart(map);

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

            // Begin Filtering Process.
            filters.FilterSocialGroups();
            filters.FilterUnits();
            filters.FilterLocations();
            //testRoutine(map);
        }

        private void testRoutine(Map map)
        {
            Console.WriteLine("Starting test routine");
            string messageString = "This is a batch of tests for the cacher. It retrieved the following items at random:";

            UA randCommandableAgent = null;
            Unit randUnit = null;

            int rand;
            IList value;
            if (cache.socialGroupsByType.TryGetValue(typeof(SocialGroup), out value))
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

            if (cache.socialGroupsByTypeExclusive.TryGetValue(typeof(SG_Orc), out value))
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

            if (cache.socialGroupsByTypeExclusive.TryGetValue(typeof(HolyOrder), out value))
            {
                rand = Eleven.random.Next(value.Count);
                List<HolyOrder> humanHolyOrders = value as List<HolyOrder>;
                HolyOrder randHumanHolyOrder = humanHolyOrders[rand];
                messageString += " Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in " + randHumanHolyOrder.seat.settlement.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in " + randHumanHolyOrder.seat.settlement.getName() + ".");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Human Holy Orders in Cache");
            }

            if (cache.commandableUnitsByType.TryGetValue(typeof(UA), out value))
            {
                rand = Eleven.random.Next(value.Count);
                List<UA> commandableAgents = value as List<UA>;
                randCommandableAgent = commandableAgents[rand];
                messageString += " Random commandable is " + randCommandableAgent.getName() + " of type " + randCommandableAgent.GetType().Name + " at location " + randCommandableAgent.location.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random commandable unit is " + randCommandableAgent.getName() + " of type " + randCommandableAgent.GetType().Name + " at location " + randCommandableAgent.location.getName() + ".");
                string visMessageString = " " + randCommandableAgent.getName();
                if (cache.unitVisibleToUnits[randCommandableAgent].Count > 0)
                {
                    visMessageString += " is visible to: ";
                    foreach (Unit u in cache.unitVisibleToUnits[randCommandableAgent])
                    {
                        visMessageString += u.getName() + ", ";
                    }
                    visMessageString = visMessageString.Substring(visMessageString.Length - 2);
                    visMessageString += ".";
                }
                else
                {
                    visMessageString += " is not visible to anyone.";
                }

                if (cache.visibleUnitsByUnit[randCommandableAgent].Count > 0)
                {
                    visMessageString += " and can see: ";
                    foreach (Unit u in cache.visibleUnitsByUnit[randCommandableAgent])
                    {
                        visMessageString += u.getName() + ", ";
                    }
                    visMessageString = visMessageString.Substring(visMessageString.Length - 2);
                    visMessageString += ".";
                }
                else
                {
                    visMessageString += " and cannot see anyone.";
                }

                messageString += visMessageString;
                Console.WriteLine("CommunityLib Cacher Test:" + visMessageString);
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Commandable Units in Cache");
            }

            if (cache.unitsByType.TryGetValue(typeof(Unit), out value))
            {
                rand = Eleven.random.Next(value.Count);
                List<Unit> units = value as List<Unit>;
                randUnit = units[rand];
                while (randUnit == randCommandableAgent)
                {
                    rand = Eleven.random.Next(value.Count);
                    randUnit = units[rand];
                }

                messageString += " Random unit is " + randUnit.getName() + " of type " + randUnit.GetType().Name + ", belonging to " + randUnit.society.getName() + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random unit is " + randUnit.getName() + " of type " + randUnit.GetType().Name + ", belonging to " + randUnit.society.getName() + ".");

                if (randCommandableAgent != null)
                {
                    double distance = cache.distanceByLocationsFromLocation[randCommandableAgent.location][randUnit.location];
                    Console.WriteLine("Got Distance");
                    int steps = cache.stepsByLocationsFromLocation[randCommandableAgent.location][randUnit.location];
                    Console.WriteLine("Got Steps");


                    messageString += " " + randCommandableAgent.getName() + " and " + randUnit.getName() + " are " + steps.ToString() + " steps appart, a distance of " + distance.ToString() + ".";
                    Console.WriteLine("CommunityLib Cacher Test: " + randCommandableAgent.getName() + " and " + randUnit.getName() + " are " + steps.ToString() + " steps appart, a distance of " + distance.ToString() + ".");
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Units in Cache");
            }

            if (randUnit != null && randCommandableAgent != null)
            {
                map.addUnifiedMessage(randUnit.location, randUnit, "Cache Testing", messageString, "CommunityLib Test");
                Console.WriteLine("CommunityLib Cacher Test: Test routine completed successfully");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: Test routine failed. randUnit or randCommandableAgent was null.");
            }
        }

        public List<Location> getLocationsWithinDistance(Location source, double distance)
        {
            List<Location> result = new List<Location>();
            Dictionary<Location, double> dict;
            if (cache.distanceByLocationsFromLocation.TryGetValue(source, out dict))
            {
                foreach (KeyValuePair<Location, double> pair in dict)
                {
                    if (pair.Value <= distance)
                    {
                        result.Add(pair.Key);
                    }
                }
            }

            return result;
        }

        public List<Settlement> getSettlmentsWithinDistance(Location source, double distance)
        {
            List<Settlement> result = new List<Settlement>();
            Dictionary<Location, double> dict;

            double dist;

            if (cache.distanceByLocationsFromLocation.TryGetValue(source, out dict))
            {
                foreach (Settlement set in cache.settlementsByType[typeof(Settlement)])
                {
                    dist = 0;
                    if (dict.TryGetValue(set.location, out dist))
                    {
                        if (dist == 0)
                        {
                            continue;
                        }

                        if (dist <= distance)
                        {
                            result.Add(set);
                        }
                    }
                }
            }

            return result;
        }

        public void updateLocationDistances()
        {
            filters.UpdateLocationDistances();
        }

        public Cache GetCache()
        {
            return cache;
        }
    }
}
