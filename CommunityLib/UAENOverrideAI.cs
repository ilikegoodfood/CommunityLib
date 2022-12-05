using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Code.Unit;
using static SortedDictionaryProvider;
using static UnityEngine.GraphicsBuffer;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        public bool overrideAI_DeepOne = true;

        public bool overrideAI_Ghast = true;

        public bool overrideAI_OrcUpstart = true;

        public bool overrideAI_Vampire = true;

        public double deepOneCultChance = 0.5;

        public double ghastMoveChance = 0.5;

        public List<Type> customChallenges_DeepOne = new List<Type>();

        public List<Type> customChallenges_Ghast = new List<Type>();

        public List<Type> customChallenges_OrcUpstart = new List<Type>();

        public List<Type> customChallenges_Vampire = new List<Type>();

        public List<Type> customChallenges_Vampire_Death = new List<Type>();

        private Map map;

        private Cache cache;

        public UAENOverrideAI(Cache cache, Map map)
        {
            this.cache = cache;
            this.map = map;
        }

        public void OverrideAI_DeepOne(UAEN_DeepOne deepOne)
        {
            if (deepOne.moveType == MoveType.NORMAL)
            {
                if (deepOne.location.isOcean)
                {
                    deepOne.moveType = MoveType.AQUAPHIBIOUS;
                    return;
                }

                Location nearestOceanLocation = null;
                List<Location>[] array;
                if (cache.oceanLocationsByStepsExclusiveFromLocation.TryGetValue(deepOne.location, out array) && array != null)
                {
                    List<Location> nearbyOceanLocations;
                    for (int i = 1; i < array.Length; i++)
                    {
                        nearbyOceanLocations = array[i];
                        if (nearbyOceanLocations != null && nearbyOceanLocations.Count > 0)
                        {
                            nearestOceanLocation = nearbyOceanLocations[Eleven.random.Next(nearbyOceanLocations.Count)];
                            break;
                        }
                    }
                }

                if (nearestOceanLocation != null)
                {
                    deepOne.task = new Task_GoToLocation(nearestOceanLocation);
                }
                else
                {
                    deepOne.die(map, "Unable to reach the ocean");
                }

                return;
            }

            if (deepOne.location.settlement is SettlementHuman)
            {
                if ((deepOne.location.soc as Society)?.isOphanimControlled ?? false)
                {
                    deepOne.task = null;
                }
                else
                {
                    bool deepOneCultPresent = false;
                    foreach (Property property in deepOne.location.properties)
                    {
                        Pr_DeepOneCult pr_DeepOneCult = property as Pr_DeepOneCult;
                        if (pr_DeepOneCult != null)
                        {
                            deepOneCultPresent = true;
                            break;
                        }
                    }

                    if (!deepOneCultPresent)
                    {
                        deepOne.task = new Task_PerformChallenge(deepOne.rt_reproduce);
                        return;
                    }
                }
            }

            List<Location> tendCultTargets = new List<Location>();
            List<Location> jobTargets = new List<Location>();
            List<SettlementHuman> newCultTargets = new List<SettlementHuman>();
            foreach (Location coastalLocation in cache.coastalLocations)
            {
                if (deepOne.menace >= (double)map.param.ua_armyBlockMenace)
                {
                    bool blockedByArmy = false;
                    foreach (Unit unit in coastalLocation.units)
                    {
                        if (unit is UM_HumanArmy)
                        {
                            blockedByArmy = true;
                            break;
                        }
                    }

                    if (blockedByArmy)
                    {
                        continue;
                    }
                }

                bool locationHasCult = false;
                bool cultNeedsTending = false;
                foreach (Property property in coastalLocation.properties)
                {
                    Pr_DeepOneCult pr_DeepOneCult = property as Pr_DeepOneCult;
                    if (pr_DeepOneCult != null)
                    {
                        if (pr_DeepOneCult.menace > 25.0 && pr_DeepOneCult.fake.claimedBy == null)
                        {
                            cultNeedsTending = true;
                            break;
                        }
                        if (pr_DeepOneCult.profile > 25.0 && pr_DeepOneCult.conceal.claimedBy == null)
                        {
                            cultNeedsTending = true;
                            break;
                        }

                        locationHasCult = true;
                    }
                }

                if (cultNeedsTending)
                {
                    tendCultTargets.Add(coastalLocation);
                }

                foreach (Challenge challenge in coastalLocation.GetChallenges())
                {
                    if (customChallenges_DeepOne.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(deepOne) && challenge.claimedBy == null)
                    {
                        jobTargets.Add(coastalLocation);
                        break;
                    }
                }

                if (!locationHasCult)
                {
                    SettlementHuman settlementHuman = coastalLocation.settlement as SettlementHuman;
                    if (settlementHuman != null && !((coastalLocation.soc as Society)?.isOphanimControlled ?? false))
                    {
                        newCultTargets.Add(settlementHuman);
                    }
                }
            }

            if (tendCultTargets.Count > 0)
            {
                Location target = tendCultTargets[Eleven.random.Next(tendCultTargets.Count)];
                Location[] pathTo = map.getPathTo(deepOne.location, target, deepOne, safeMove: true);
                if (pathTo != null)
                {
                    foreach (Property property in target.properties)
                    {
                        Pr_DeepOneCult pr_DeepOneCult = property as Pr_DeepOneCult;
                        if (pr_DeepOneCult != null)
                        {
                            if (pr_DeepOneCult.menace > 25.0 && pr_DeepOneCult.fake.claimedBy == null)
                            {
                                pr_DeepOneCult.fake.claimedBy = deepOne;
                                deepOne.task = new Task_GoToPerformChallenge(pr_DeepOneCult.fake);
                                return;
                            }
                            if (pr_DeepOneCult.profile > 25.0 && pr_DeepOneCult.conceal.claimedBy == null)
                            {
                                pr_DeepOneCult.conceal.claimedBy = deepOne;
                                deepOne.task = new Task_GoToPerformChallenge(pr_DeepOneCult.conceal);
                                return;
                            }
                        }
                    }
                }
            }

            if (jobTargets.Count > 0 && Eleven.random.NextDouble() > deepOneCultChance)
            {
                Location target = jobTargets[Eleven.random.Next(jobTargets.Count)];
                Location[] pathTo = map.getPathTo(deepOne.location, target, deepOne, safeMove: true);
                if (pathTo != null)
                {
                    List<Challenge> validChallenges = new List<Challenge>();
                    foreach (Challenge challenge in target.GetChallenges())
                    {
                        if (customChallenges_DeepOne.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(deepOne) && challenge.claimedBy == null)
                        {
                            validChallenges.Add(challenge);
                        }
                    }

                    if (validChallenges.Count > 0)
                    {
                        Challenge challenge = validChallenges[Eleven.random.Next(validChallenges.Count)];
                        challenge.claimedBy = deepOne;
                        deepOne.task = new Task_GoToPerformChallenge(challenge);
                        return;
                    }
                }
            }

            if (newCultTargets.Count > 0)
            {
                SettlementHuman settlement = newCultTargets[Eleven.random.Next(newCultTargets.Count)];
                Location[] pathTo = map.getPathTo(deepOne.location, settlement.location, deepOne, safeMove: true);
                if (pathTo != null)
                {
                    deepOne.task = new Task_GoToLocation(settlement.location);
                    return;
                }
            }
        }

        public void OverrideAI_Ghast(UAEN_Ghast ghast)
        {
            SettlementHuman settlementHuman = ghast.location.settlement as SettlementHuman;
            if (settlementHuman != null)
            {
                Pr_Ward ward = null;
                foreach (Property property in settlementHuman.location.properties)
                {
                    if (property is Pr_Ward)
                    {
                        ward = property as Pr_Ward;
                        break;
                    }
                }

                if (settlementHuman.shadow < 1.0 && (ward == null || ward.charge < 0.66))
                {
                    bool ophanimFaithPresent = false;
                    foreach (Property property in ghast.location.properties)
                    {
                        if (property is Pr_Opha_Faith)
                        {
                            ophanimFaithPresent = true;
                            break;
                        }
                    }

                    if (!ophanimFaithPresent)
                    {
                        ghast.task = new Task_PerformChallenge(ghast.rt_enshadow);
                        return;
                    }
                }

                List<Challenge> validChallenges = new List<Challenge>();
                foreach (Challenge challenge in settlementHuman.location.GetChallenges())
                {
                    if (customChallenges_Ghast.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(ghast))
                    {
                        validChallenges.Add(challenge);
                    }
                }

                if (validChallenges.Count > 0 && Eleven.random.NextDouble() > ghastMoveChance)
                {
                    ghast.task = new Task_PerformChallenge(validChallenges[Eleven.random.Next(validChallenges.Count)]);
                    return;
                }
            }

            List<SettlementHuman> targetHumanSettlements = new List<SettlementHuman>();
            List<Settlement> nearbySettlements = new List<Settlement>();
            for (int i = 1; i < 125; i++)
            {
                if (cache.settlementsByStepsExclusiveFromLocation.ContainsKey(ghast.location) && cache.settlementsByStepsExclusiveFromLocation[ghast.location] != null && cache.settlementsByStepsExclusiveFromLocation[ghast.location][i] != null && cache.settlementsByStepsExclusiveFromLocation[ghast.location][i].Count > 0)
                {
                    nearbySettlements = cache.settlementsByStepsExclusiveFromLocation[ghast.location][i];
                }

                foreach (Settlement nearbySettlement in nearbySettlements)
                {
                    SettlementHuman nearbyHumanSettlement = nearbySettlement as SettlementHuman;
                    if (settlementHuman != null)
                    {
                        Pr_Ward ward = null;
                        foreach (Property property in settlementHuman.location.properties)
                        {
                            if (property is Pr_Ward)
                            {
                                ward = property as Pr_Ward;
                                break;
                            }
                        }
                        bool ophanimFaithPresent = false;
                        foreach (Property property in ghast.location.properties)
                        {
                            if (property is Pr_Opha_Faith)
                            {
                                ophanimFaithPresent = true;
                                break;
                            }
                        }

                        if (!ophanimFaithPresent && settlementHuman.shadow < 1.0 && (ward == null || ward.charge < 0.66))
                        {
                            targetHumanSettlements.Add(settlementHuman);
                        }
                    }
                }

                if (targetHumanSettlements != null && targetHumanSettlements.Count > 0)
                {
                    break;
                }
            }

            if (targetHumanSettlements.Count > 0)
            {
                ghast.task = new Task_GoToLocation(targetHumanSettlements[Eleven.random.Next(targetHumanSettlements.Count)].location);
            }
        }

        public void OverrideAI_OrcUpstart(UAEN_OrcUpstart upstart)
        {
            //Console.WriteLine("CommunityLib: Running OverrideAI_OrcUpstart.");
            List<Set_OrcCamp> orcCamps = null;
            List<Type> validChallengeTypes = new List<Type>() { typeof(Ch_OrcRaiding), typeof(Ch_Rest_InOrcCamp) };
            List<Challenge> validChallenges = new List<Challenge>();

            if (cache.settlementsBySocialGroupByType.ContainsKey(upstart.society) && cache.settlementsBySocialGroupByType[upstart.society] != null && cache.settlementsBySocialGroupByType[upstart.society].ContainsKey(typeof(Set_OrcCamp)) && cache.settlementsBySocialGroupByType[upstart.society][typeof(Set_OrcCamp)] != null)
            {
                //Console.WriteLine("CommunityLib: Getting orc camps belonging to orc upstart's social group.");
                orcCamps = cache.settlementsBySocialGroupByType[upstart.society][typeof(Set_OrcCamp)] as List<Set_OrcCamp>;
            }

            if (upstart.society.isGone() || orcCamps == null || orcCamps.Count == 0)
            {
                upstart.die(map, "Died in the wilderness");
                return;
            }

            //Console.WriteLine("CommunityLib: Orc camps found.");
            if (map.worldPanic > map.param.panic_forFundHeroes)
            {
                //Console.WriteLine("CommunityLib: World panic high enough to recuit minions.");
                validChallengeTypes.Add(typeof(Ch_RecruitMinion));
            }
            validChallengeTypes.AddRange(customChallenges_OrcUpstart);
            //Console.WriteLine("CommunityLib: Orc Upstart has " + validChallengeTypes.Count.ToString() + " valid challenge types.");

            Set_OrcCamp orcCamp = orcCamps[Eleven.random.Next(orcCamps.Count)];
            foreach (Challenge challenge in orcCamp.location.GetChallenges())
            {
                //Console.WriteLine("CommunityLib: Checking if challenge of Type " + challenge.GetType().Name + " is of valid type.");
                if (validChallengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(upstart) && challenge.getUtility(upstart, null) > 0.0)
                {
                    //Console.WriteLine("CommunityLib: Valid challenge found: " + challenge.getName() + ".");
                    validChallenges.Add(challenge);
                }
            }

            if (validChallenges.Count > 0)
            {
                upstart.task = new Task_GoToPerformChallenge(validChallenges[Eleven.random.Next(validChallenges.Count)]);
                //Console.WriteLine("CommunityLib: Randomly selected " + (upstart.task as Task_GoToPerformChallenge).challenge.getName() + " challenge from " + validChallenges.Count.ToString() + " valid challenges.");
            }
        }

        public void OverrideAI_Vampire(UAEN_Vampire vampire)
        {
            //Console.WriteLine("CommunityLib: Running OverrideAI_Vampire.");
            T_TheHunger call = null;
            Rt_Feed feed = null;

            //Console.WriteLine("CommunityLib: Getting required traits, rituals, and other variables.");
            foreach (Trait trait in vampire.person.traits)
            {
                if (trait is T_TheHunger)
                {
                    //Console.WriteLine("CommunityLib: Got The Hunger");
                    call = trait as T_TheHunger;
                    break;
                }
            }

            foreach (Ritual ritual in vampire.rituals)
            {
                if (ritual is Rt_Feed)
                {
                    //Console.WriteLine("CommunityLib: Got Feed");
                    feed = ritual as Rt_Feed;
                    break;
                }
            }

            if (call == null || feed == null)
            {
                //Console.WriteLine("CommunityLib: Failed to collect required data.");
                return;
            }

            if (call.strength >= 50.0)
            {
                SettlementHuman humanSettlement = vampire.location.settlement as SettlementHuman;
                if (humanSettlement != null && humanSettlement.shadow < 0.9 && feed.valid() && feed.validFor(vampire))
                {
                    vampire.task = new Task_PerformChallenge(feed);
                    return;
                }

                if (call.strength > 100.0)
                {
                    List<Settlement> targetSettlements = new List<Settlement>();
                    List<Settlement> settlements = null;
                    for (int i = 0; i < 125; i++)
                    {

                        if (cache.settlementsByStepsExclusiveFromLocation.ContainsKey(vampire.location) && cache.settlementsByStepsExclusiveFromLocation[vampire.location] != null && cache.settlementsByStepsExclusiveFromLocation[vampire.location][i] != null)
                        {
                            settlements = cache.settlementsByStepsExclusiveFromLocation[vampire.location][i];
                        }

                        if (settlements != null && settlements.Count == 0)
                        {
                            continue;
                        }

                        for (int j = 0; j < settlements.Count; j++)
                        {
                            if (settlements != null && settlements.Count > 0)
                            {
                                foreach (Settlement settlement in settlements)
                                {
                                    if (settlement is SettlementHuman && settlement.shadow < 0.7)
                                    {
                                        targetSettlements.Add(settlement);
                                    }
                                }

                                if (targetSettlements.Count > 0)
                                {
                                    Location location = targetSettlements[Eleven.random.Next(targetSettlements.Count)].location;
                                    vampire.task = new Task_GoToLocation(location);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            bool fullMinions = true;
            Minion[] minions = vampire.minions;
            foreach (Minion minion in minions)
            {
                if (minion == null)
                {
                    fullMinions = false;
                    break;
                }
            }

            double enshadowmentDif = 0.0;
            if (vampire.location.settlement != null)
            {
                List<Settlement> nearbySettlements = cache.settlementsByStepsExclusiveFromLocation[vampire.location][1];
                if (nearbySettlements != null && nearbySettlements.Count > 0)
                {
                    foreach (Settlement nearbySettlement in nearbySettlements)
                    {
                        Society society = nearbySettlement.location.soc as Society;
                        if (nearbySettlement is SettlementHuman && (society == null || !society.isAlliance || map.opt_allianceState != 1))
                        {
                            enshadowmentDif += Math.Max(0.0, vampire.location.settlement.shadow - nearbySettlement.shadow);
                        }
                    }
                }
            }

            double value = 0.0;
            Challenge challenge = null;
            List<Type> validChallengeTypes = new List<Type>() { typeof(Ch_Enshadow), typeof(Ch_Desecrate) };
            validChallengeTypes.AddRange(customChallenges_Vampire);
            List<Type> validChallengeTypes_Death = new List<Type>();
            validChallengeTypes_Death.AddRange(customChallenges_Vampire_Death);
            //Console.WriteLine("CommunityLib: Vampire has " + (validChallengeTypes.Count + validChallengeTypes_Death.Count).ToString() + " valid challenge types.");
            foreach (Challenge validChallenge in vampire.getAllValidChallenges())
            {
                double deathCharge = 0.0;
                foreach (Property property in validChallenge.location.properties)
                {
                    if (property is Pr_Death)
                    {
                        deathCharge = property.charge;
                        break;
                    }
                }

                if (fullMinions && validChallenge is Mg_DeathsShadow)
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    // Random number between 1.0 and 3.0, weighted by bell curve towards 2.0.
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    cost /= Math.Min(5.0, deathCharge / 5.0);

                    if (challenge == null || cost < value)
                    {
                        value = cost;
                        challenge = validChallenge;
                    }
                }

                double wellOfShadowsCharge = 0.0;
                foreach (Property property in validChallenge.location.properties)
                {
                    if (property is Pr_WellOfShadows)
                    {
                        wellOfShadowsCharge = property.charge;
                        break;
                    }
                }

                if (validChallenge is Ch_WellOfShadows && wellOfShadowsCharge < 75.0 && enshadowmentDif > 0.05)
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    if (challenge == null || cost < value)
                    {
                        value = cost;
                        challenge = validChallenge;
                    }
                }

                if (validChallengeTypes.Contains(validChallenge.GetType()))
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    if (challenge == null || cost < value)
                    {
                        value = cost;
                        challenge = validChallenge;
                    }
                }

                if (validChallengeTypes_Death.Contains(validChallenge.GetType()))
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    // Random number between 1.0 and 3.0, weighted by bell curve towards 2.0.
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    cost /= Math.Min(5.0, deathCharge / 5.0);

                    if (challenge == null || cost < value)
                    {
                        value = cost;
                        challenge = validChallenge;
                    }
                }
            }

            if (challenge != null)
            {
                //Console.WriteLine("CommunityLib: Randomly selected " + challenge.getName() + " challenge.");
                vampire.task = new Task_GoToPerformChallenge(challenge);
            }
            else
            {
                map.overmind.autoAI.aiShadow.turnTick(vampire);
            }
        }
    }
}
