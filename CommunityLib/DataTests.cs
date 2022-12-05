using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CommunityLib
{
    internal class DataTests
    {
        private Cache cache;

        private Filters filters;

        private Map map;

        internal DataTests(Cache cache, Filters filters, Map map)
        {
            this.cache = cache;
            this.filters = filters;
            this.map = map;
        }

        public void RunDataTests(bool testsGeneral, bool testsSpecific)
        {
            if (testsSpecific)
            {
                testSpecific();
            }
            if (testsGeneral)
            {
                dataTestBatchA();
                dataTestBatchB();
            }
        }

        private void testSpecific()
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

        private void dataTestBatchA()
        {
            Console.WriteLine("CommunityLib Caher Test: Starting test routine");
            string messageString = "This is a batch of tests for the cacher. It retrieved the following items at random:";

            Unit randCommandableUnit = null;
            Unit randUnit = null;

            IList value;
            if (cache.socialGroupsByType.TryGetValue(typeof(SocialGroup), out value) && value != null && value.Count > 0)
            {
                SocialGroup randSG;
                for (int i = 0; i < 3; i++)
                {
                    List<SocialGroup> socialGroups = value as List<SocialGroup>;
                    randSG = socialGroups[Eleven.random.Next(value.Count)];
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
                List<SG_Orc> orcSocieties = value as List<SG_Orc>;
                SG_Orc randOrcSG = orcSocieties[Eleven.random.Next(value.Count)];
                List<Set_OrcCamp> specialisedCamps = filters.getSpecialisedOrcCampsBySocialGroup(randOrcSG);
                messageString += " Random Orc Society is " + randOrcSG.getName() + " of type " + randOrcSG.GetType().Name + ".";
                Console.WriteLine("CommunityLib Cacher Test: Random Orc Society is " + randOrcSG.getName() + " of type " + randOrcSG.GetType().Name + ".");

                if (specialisedCamps != null && specialisedCamps.Count > 0)
                {
                    messageString += " " + randOrcSG.getName() + " has " + specialisedCamps.Count + " specialised orc camps.";
                    if (specialisedCamps.Count == 1)
                    {
                        messageString = messageString.Substring(0, messageString.Length - 2);
                        messageString += ".";
                    }
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Orc Social Groups in Cache");
            }

            if (cache.socialGroupsByTypeExclusive.TryGetValue(typeof(HolyOrder), out value) && value != null && value.Count > 0)
            {
                List<HolyOrder> humanHolyOrders = value as List<HolyOrder>;
                HolyOrder randHumanHolyOrder = humanHolyOrders[Eleven.random.Next(value.Count)];
                if (randHumanHolyOrder.seat != null && randHumanHolyOrder.seat.settlement != null)
                {
                    messageString += " Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in " + randHumanHolyOrder.seat.settlement.getName() + ".";
                    Console.WriteLine("CommunityLib Cacher Test: Random Human Holy Order is " + randHumanHolyOrder.getName() + " with its capitol in Location " + randHumanHolyOrder.seat.settlement.getName() + ".");
                }
                else
                {
                    messageString += " Random Human Holy Order is " + randHumanHolyOrder.getName() + ". It's capitol has been destroyed.";
                    Console.WriteLine("CommunityLib Cacher Test: Random Human Holy Order is " + randHumanHolyOrder.getName() + ". It's capitol has been destroyed.");
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Human Holy Orders in Cache");
            }

            if (cache.commandableUnitsByType.TryGetValue(typeof(Unit), out value) && value != null && value.Count > 0)
            {
                List<Unit> commandableAgents = value as List<Unit>;
                randCommandableUnit = commandableAgents[Eleven.random.Next(value.Count)];
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
                List<Unit> units = value as List<Unit>;
                randUnit = units[Eleven.random.Next(value.Count)];
                while (randUnit == randCommandableUnit)
                {
                    randUnit = units[Eleven.random.Next(value.Count)];
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

        private void dataTestBatchB()
        {
            Console.WriteLine("CommunityLib Caher Test: Starting test routine");
            string messageString = "This is a batch of tests for the cacher. It retrieved the following items at random:";

            Pr_GeomanticLocus randGL = null;
            List<Settlement> nearbySettlements = null;

            if (cache.coastalSettlements != null && cache.coastalSettlements.Count > 0)
            {
                Settlement randCS;
                for (int i = 0; i < 3; i++)
                {
                    randCS = cache.coastalSettlements[Eleven.random.Next(cache.coastalSettlements.Count)];
                    messageString += " Random Coastal Settlement " + (i + 1).ToString() + " is " + randCS.getName() + " of Type " + randCS.GetType().Name + ",";
                    string consoleMessage = "CommunityLib Cacher Test: Random Coastal Settlement " + (i + 1).ToString() + " is " + randCS.getName() + " of Type " + randCS.GetType().Name + ",";
                    if (randCS.location.soc != null)
                    {
                        messageString += " which belongs to " + randCS.location.soc.getName() + " of Type " + randCS.location.soc.GetType().Name + ".";
                        consoleMessage += " which belongs to " + randCS.location.soc.getName() + " of Type " + randCS.location.soc.GetType().Name + ".";
                    }
                    else
                    {
                        messageString += " which does not belong to any Social Group.";
                        consoleMessage += " which does not belong to any Social Group.";
                    }
                    Console.WriteLine(consoleMessage);
                }
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: ERROR : No Coastal Settlements in Cache");
            }

            IList value;
            if (cache.propertiesByTypeExclusive.TryGetValue(typeof(Pr_GeomanticLocus), out value) && value != null && value.Count > 0)
            {
                randGL = value[Eleven.random.Next(value.Count)] as Pr_GeomanticLocus;
                SocialGroup sG = randGL.location.soc;
                messageString += " Random Geomantic Locus has a charge of " + randGL.charge.ToString() + " and is located in " + randGL.location.getName() + ",";
                string consoleOutput = "CommunityLib Cacher Test: Random Geomantic Locus has a charge of " + randGL.charge.ToString() + " and is located in " + randGL.location.getName() + ",";
                if (sG != null)
                {
                    messageString += " which belongs to the " + sG.getName() + " of Type " + sG.GetType().Name + ".";
                    consoleOutput += " which belongs to the " + sG.getName() + " of Type " + sG.GetType().Name + ".";
                }
                else
                {
                    messageString += " which does not belojng to any Social Group.";
                    consoleOutput += " which does not belojng to any Social Group.";
                }
                Console.WriteLine(consoleOutput);
            }

            List<Settlement>[] array;
            int steps = 0;
            if (cache.settlementsByStepsExclusiveFromLocation.TryGetValue(randGL.location, out array) && array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] != null && array[i].Count > 0)
                    {
                        steps = i;
                        break;
                    }
                }
                
                nearbySettlements = filters.getSettlementsWithinSteps(randGL.location, 3);
                if (steps == 0)
                {
                    messageString += " It is located within the Settlement " + randGL.location.settlement.getName() + ", and has " + nearbySettlements.Count + " settlements within 3 steps of it.";
                    Console.WriteLine("CommunityLib Cacher Test: It is location within a settlement, and has " + nearbySettlements.Count + "settlements within 3 steps of it.");
                }
                else
                {
                    messageString += " It is not located in a settlement. The nearest settlemnt is " + steps + " steps away. It has " + nearbySettlements.Count + " settlements within " + Math.Max(3, steps + 1) + " steps of it.";
                    Console.WriteLine("CommunityLib Cacher Test: It is not located in a settlement. The nearest settlemnt is " + steps + " steps away. It has " + nearbySettlements.Count + "settlements within " + Math.Max(3, steps + 1) + " steps of it.");
                }
            }

            if (randGL != null)
            {
                map.addUnifiedMessage(randGL.location, nearbySettlements[Eleven.random.Next(nearbySettlements.Count)].location, "Cache Testing", messageString, "CommunityLib Test");
                Console.WriteLine("CommunityLib Cacher Test: Test routine completed successfully");
            }
            else
            {
                Console.WriteLine("CommunityLib Cacher Test: Test routine failed. randUnit or randCommandableAgent was null.");
            }
        }
    }
}
