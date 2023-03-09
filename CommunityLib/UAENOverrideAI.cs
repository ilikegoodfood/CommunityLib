using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using static Assets.Code.Unit;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        public Dictionary<Type, bool> enableOverrideAI;

        public double deepOneCultChance = 0.5;

        public double ghastMoveChance = 0.5;

        public Dictionary<Type, Dictionary<Type, List<ChallengeTags>>> UAENChallenges;

        private Map map;

        private Cache cache;

        public UAENOverrideAI(Cache cache, Map map)
        {
            this.cache = cache;
            this.map = map;

            enableOverrideAI = new Dictionary<Type, bool> {
                {typeof(UAEN_DeepOne), true},
                {typeof(UAEN_Ghast), true},
                {typeof(UAEN_OrcUpstart), true},
                {typeof(UAEN_Vampire), true}
            };

            UAENChallenges = new Dictionary<Type, Dictionary<Type, List<ChallengeTags>>> {
                {typeof(UAEN_DeepOne), new Dictionary<Type , List < ChallengeTags >>()},
                {typeof(UAEN_Ghast), new Dictionary<Type , List < ChallengeTags >>()},
                {typeof(UAEN_OrcUpstart), new Dictionary < Type , List < ChallengeTags > >()},
                {typeof(UAEN_Vampire), new Dictionary < Type , List < ChallengeTags > >()}
            };
        }

        public bool addUAENChallengeType(Type t, Type c, List<ChallengeTags> tags = null)
        {
            if (!UAENChallenges.ContainsKey(t) || UAENChallenges[t].ContainsKey(c))
            {
                return false;
            }



            return true;
        }

        public bool removeUAENChallengeType(Type t, Type c)
        {
            bool result = false;

            if (UAENChallenges.ContainsKey(t))
            {
                List<ChallengeTags> tagsToRemove = new List<ChallengeTags>();
                foreach (KeyValuePair<ChallengeTags, List<Type>> pair in UAENChallenges[t])
                {
                    if (pair.Value == null || pair.Value.Count == 0)
                    {
                        tagsToRemove.Add(pair.Key);
                        continue;
                    }

                    bool retValue = pair.Value.Remove(c);

                    if (pair.Value.Count == 0)
                    {
                        tagsToRemove.Add(pair.Key);
                    }

                    if (retValue)
                    {
                        result = retValue;
                    }
                }

                if (tagsToRemove.Count > 0)
                {
                    foreach (ChallengeTags tag in tagsToRemove)
                    {
                        UAENChallenges[t].Remove(tag);
                    }
                }
            }

            return result;
        }

        public bool removeUAENChallengeType(Type t, ChallengeTags tag, Type c)
        {
            bool result = false;

            if (UAENChallenges.ContainsKey(t) && UAENChallenges[t].ContainsKey(tag))
            {
                if (UAENChallenges[t][tag] == null || UAENChallenges[t][tag].Count == 0)
                {
                    UAENChallenges[t].Remove(tag);
                    return false;
                }

                result = UAENChallenges[t][tag].Remove(c);

                if (UAENChallenges[t][tag].Count == 0)
                {
                    UAENChallenges[t].Remove(tag);
                }
            }

            return result;
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
                    Pr_DeepOneCult deepOneCult = deepOne.location.properties.OfType<Pr_DeepOneCult>().FirstOrDefault();

                    if (deepOneCult == null)
                    {
                        deepOne.task = new Task_PerformChallenge(deepOne.rt_reproduce);
                        return;
                    }
                }
            }

            List<Type> customChallengeTypes = new List<Type>();

            foreach (KeyValuePair<ChallengeTags, List<Type>> pair in UAENChallenges[typeof(UAEN_DeepOne)])
            {
                customChallengeTypes.AddRange(pair.Value);
            }

            List<Location> tendCultTargets = new List<Location>();
            List<Location> jobTargets = new List<Location>();
            List<SettlementHuman> newCultTargets = new List<SettlementHuman>();
            foreach (Location coastalLocation in cache.coastalLocations)
            {
                if (deepOne.menace >= (double)map.param.ua_armyBlockMenace)
                {
                    UM_HumanArmy armyHuman = coastalLocation.units.OfType<UM_HumanArmy>().FirstOrDefault();

                    if (armyHuman != null)
                    {
                        continue;
                    }
                }

                Pr_DeepOneCult deepOneCult = coastalLocation.properties.OfType<Pr_DeepOneCult>().FirstOrDefault();

                if (deepOneCult != null && ((deepOneCult.menace > 25.0 && deepOneCult.fake.claimedBy == null) || (deepOneCult.profile > 25.0 && deepOneCult.conceal.claimedBy == null)))
                {
                    tendCultTargets.Add(coastalLocation);
                }

                foreach (Challenge challenge in coastalLocation.GetChallenges())
                {
                    if (customChallengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(deepOne) && challenge.claimedBy == null)
                    {
                        jobTargets.Add(coastalLocation);
                        break;
                    }
                }

                if (deepOneCult == null)
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
                    Pr_DeepOneCult pr_DeepOneCult = target.properties.OfType<Pr_DeepOneCult>().FirstOrDefault();
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

            if (jobTargets.Count > 0 && (newCultTargets.Count == 0 || Eleven.random.NextDouble() > deepOneCultChance))
            {
                Location target = jobTargets[Eleven.random.Next(jobTargets.Count)];
                Location[] pathTo = map.getPathTo(deepOne.location, target, deepOne, safeMove: true);
                if (pathTo != null)
                {
                    List<Challenge> validChallenges = new List<Challenge>();
                    foreach (Challenge challenge in target.GetChallenges())
                    {
                        if (customChallengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(deepOne) && challenge.claimedBy == null)
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
            List<Settlement> nearbySettlements = new List<Settlement>();
            if (settlementHuman != null)
            {
                Pr_Ward ward = settlementHuman.location.properties.OfType<Pr_Ward>().FirstOrDefault();
                Society society = settlementHuman.location.soc as Society;

                List<Type> challengeTypes = new List<Type>();
                List<Challenge> validChallenges = new List<Challenge>();

                if (settlementHuman.shadow < 1.0 && (society == null || !society.isAlliance || map.opt_allianceState != 1) && (ward == null || ward.charge < 0.66))
                {
                    Pr_Opha_Faith ophaFaith = settlementHuman.location.properties.OfType<Pr_Opha_Faith>().FirstOrDefault();

                    challengeTypes.Add(typeof(Rt_GhastEnshadow));

                    if (UAENChallenges[typeof(UAEN_Ghast)].ContainsKey(ChallengeTags.Enshadows))
                    {
                        challengeTypes.AddRange(UAENChallenges[typeof(UAEN_Ghast)][ChallengeTags.Enshadows]);
                    }

                    if (ophaFaith == null)
                    {
                        foreach (Challenge challenge in settlementHuman.location.GetChallenges())
                        {
                            if (challengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(ghast) && challenge.claimedBy == null)
                            {
                                validChallenges.Add(challenge);
                            }
                        }

                        if (validChallenges.Count > 0)
                        {
                            Challenge challenge = validChallenges[Eleven.random.Next(validChallenges.Count)];
                            challenge.claimedBy = ghast;
                            ghast.task = new Task_PerformChallenge(challenge);
                            return;
                        }
                    }
                }

                if (UAENChallenges[typeof(UAEN_Ghast)].ContainsKey(ChallengeTags.PushesShadow) && UAENChallenges[typeof(UAEN_Ghast)][ChallengeTags.PushesShadow]?.Count > 0)
                {
                    challengeTypes.Clear();
                    challengeTypes.AddRange(UAENChallenges[typeof(UAEN_Ghast)][ChallengeTags.PushesShadow]);

                    double enshadowmentDif = 0.0;
                    if (ghast.location.settlement != null)
                    {
                        if (cache.settlementsByStepsExclusiveFromLocation.ContainsKey(ghast.location) && cache.settlementsByStepsExclusiveFromLocation[ghast.location] != null && cache.settlementsByStepsExclusiveFromLocation[ghast.location][1] != null)
                        {
                            nearbySettlements = cache.settlementsByStepsExclusiveFromLocation[ghast.location][1];
                        }

                        if (nearbySettlements != null && nearbySettlements.Count > 0)
                        {
                            foreach (Settlement nearbySettlement in nearbySettlements)
                            {
                                ward = null;
                                Pr_Opha_Faith ophaFaith = null;
                                foreach (Property property in settlementHuman.location.properties)
                                {
                                    if (ward == null)
                                    {
                                        ward = property as Pr_Ward;
                                    }

                                    if (ophaFaith == null)
                                    {
                                        ophaFaith = property as Pr_Opha_Faith;
                                    }

                                    if (ward != null && ophaFaith != null)
                                    {
                                        break;
                                    }
                                }

                                Society society = nearbySettlement.location.soc as Society;
                                if (nearbySettlement is SettlementHuman && (society == null || !society.isAlliance || map.opt_allianceState != 1) && (ward == null || ward.charge < 0.66) && ophaFaith == null)
                                {
                                    enshadowmentDif += Math.Max(0.0, ghast.location.settlement.shadow - nearbySettlement.shadow);
                                }
                            }
                        }

                        if (enshadowmentDif > 0.05)
                        {
                            foreach (Challenge challenge in settlementHuman.location.GetChallenges())
                            {
                                if (challengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(ghast) && challenge.claimedBy == null)
                                {
                                    validChallenges.Add(challenge);
                                }
                            }

                            if (validChallenges.Count > 0)
                            {
                                Challenge challenge = validChallenges[Eleven.random.Next(validChallenges.Count)];
                                challenge.claimedBy = ghast;
                                ghast.task = new Task_PerformChallenge(challenge);
                                return;
                            }
                        }
                    }
                }

                challengeTypes.Clear();
                foreach (KeyValuePair<ChallengeTags, List<Type>> pair in UAENChallenges[typeof(UAEN_Ghast)])
                {
                    if (pair.Key != ChallengeTags.Enshadows && pair.Key != ChallengeTags.PushesShadow)
                    {
                        challengeTypes.AddRange(pair.Value);
                    }
                }

                validChallenges.Clear();
                foreach (Challenge challenge in settlementHuman.location.GetChallenges())
                {
                    if (challengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(ghast) && challenge.claimedBy == null)
                    {
                        validChallenges.Add(challenge);
                    }
                }

                if (validChallenges.Count > 0 && Eleven.random.NextDouble() > ghastMoveChance)
                {
                    Challenge challenge = validChallenges[Eleven.random.Next(validChallenges.Count)];
                    challenge.claimedBy = ghast;
                    ghast.task = new Task_PerformChallenge(challenge);
                    return;
                }
            }

            List<SettlementHuman> targetHumanSettlements = new List<SettlementHuman>();
            nearbySettlements.Clear();
            for (int i = 1; i < 125; i++)
            {
                if (cache.settlementsByStepsExclusiveFromLocation.ContainsKey(ghast.location) && cache.settlementsByStepsExclusiveFromLocation[ghast.location] != null && cache.settlementsByStepsExclusiveFromLocation[ghast.location][i] != null && cache.settlementsByStepsExclusiveFromLocation[ghast.location][i].Count > 0)
                {
                    nearbySettlements = cache.settlementsByStepsExclusiveFromLocation[ghast.location][i];
                }

                foreach (Settlement nearbySettlement in nearbySettlements)
                {
                    SettlementHuman nearbyHumanSettlement = nearbySettlement as SettlementHuman;

                    if (settlementHuman == null)
                    {
                        continue;
                    }

                    Pr_Ward ward = null;
                    Pr_Opha_Faith ophaFaith = null;
                    foreach (Property property in settlementHuman.location.properties)
                    {
                        if (ward == null)
                        {
                            ward = property as Pr_Ward;
                        }
                            
                        if (ophaFaith == null)
                        {
                            ophaFaith = property as Pr_Opha_Faith;
                        }

                        if (ward != null && ophaFaith != null)
                        {
                            break;
                        }
                    }

                    if (ophaFaith == null && settlementHuman.shadow < 1.0 && (ward == null || ward.charge < 0.66))
                    {
                        targetHumanSettlements.Add(settlementHuman);
                    }
                }

                if (targetHumanSettlements.Count > 0)
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
            List<Type> challengeTypes = new List<Type>() { typeof(Ch_OrcRaiding), typeof(Ch_Rest_InOrcCamp) };
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
                challengeTypes.Add(typeof(Ch_RecruitMinion));
            }

            foreach(KeyValuePair<ChallengeTags, List<Type>> pair in UAENChallenges[typeof(UAEN_OrcUpstart)])
            {
                challengeTypes.AddRange(pair.Value);
            }
            //Console.WriteLine("CommunityLib: Orc Upstart has " + validChallengeTypes.Count.ToString() + " valid challenge types.");

            Set_OrcCamp orcCamp = orcCamps[Eleven.random.Next(orcCamps.Count)];
            foreach (Challenge challenge in orcCamp.location.GetChallenges())
            {
                //Console.WriteLine("CommunityLib: Checking if challenge of Type " + challenge.GetType().Name + " is of valid type.");
                if (challengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(upstart) && challenge.getUtility(upstart, null) > 0.0)
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
            T_TheHunger call = vampire.person.traits.OfType<T_TheHunger>().FirstOrDefault();
            Rt_Feed feed = vampire.person.traits.OfType<Rt_Feed>().FirstOrDefault();

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

                        if (settlements == null || settlements.Count == 0)
                        {
                            continue;
                        }

                        for (int j = 0; j < settlements.Count; j++)
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

            bool fullMinions = true;
            Minion[] minions = vampire.minions;
            if (vampire.getCurrentlyUsedCommand() < vampire.getStatCommandLimit())
            {
                foreach (Minion minion in minions)
                {
                    if (minion == null)
                    {
                        fullMinions = false;
                        break;
                    }
                }
            }

            double enshadowmentDif = 0.0;
            if (vampire.location.settlement != null)
            {
                List<Settlement> nearbySettlements = null;

                if (cache.settlementsByStepsExclusiveFromLocation.ContainsKey(vampire.location) && cache.settlementsByStepsExclusiveFromLocation[vampire.location] != null && cache.settlementsByStepsExclusiveFromLocation[vampire.location][1] != null)
                {
                    nearbySettlements = cache.settlementsByStepsExclusiveFromLocation[vampire.location][1];
                }

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
            List<Challenge> challenges = new List<Challenge>();

            List<Type> challengeTypes = new List<Type>() { typeof(Ch_Enshadow), typeof(Ch_Desecrate) };
            List<Type> challengeTypes_Death = new List<Type>();
            List<Type> challengeTypes_PushShadow = new List<Type>();
            List<Type> challengeTypes_Enshadow = new List<Type>();
            List<Type> challengeTypes_RecruitMinion = new List<Type>();

            foreach (KeyValuePair<ChallengeTags, List<Type>> pair in UAENChallenges[typeof(UAEN_Vampire)])
            {
                switch (pair.Key)
                {
                    case ChallengeTags.NeedsDeath:
                        challengeTypes_Death = pair.Value;
                        break;
                    case ChallengeTags.PushesShadow:
                        challengeTypes_PushShadow = pair.Value;
                        break;
                    case ChallengeTags.Enshadows:
                        challengeTypes_Enshadow = pair.Value;
                        break;
                    case ChallengeTags.RecruitsMinion:
                        challengeTypes_RecruitMinion = pair.Value;
                        break;
                    default:
                        challengeTypes.AddRange(pair.Value);
                        break;
                }
            }

            //Console.WriteLine("CommunityLib: Vampire has " + (validChallengeTypes.Count + validChallengeTypes_Death.Count).ToString() + " valid challenge types.");
            foreach (Challenge validChallenge in vampire.getAllValidChallenges())
            {
                Pr_Death death = validChallenge.location.properties.OfType<Pr_Death>().FirstOrDefault();
                double deathCharge = death?.charge ?? 0.0;

                if (fullMinions && validChallenge is Mg_DeathsShadow)
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    // Random number between 1.0 and 3.0, weighted by bell curve towards 2.0.
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    cost /= Math.Min(5.0, deathCharge / 5.0);

                    if (challenges.Count == 0 || cost == value)
                    {
                        value = cost;
                        challenges.Add(validChallenge);
                    }
                    else if (cost < value)
                    {
                        value = cost;
                        challenges.Clear();
                        challenges.Add(validChallenge);
                    }
                }

                double wellOfShadowsCharge = validChallenge.location.properties.OfType<Pr_WellOfShadows>().FirstOrDefault()?.charge ?? 0.0;

                if (validChallenge is Ch_WellOfShadows && wellOfShadowsCharge < 75.0 && enshadowmentDif > 0.05)
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    if (challenges.Count == 0 || cost == value)
                    {
                        value = cost;
                        challenges.Add(validChallenge);
                    }
                    else if (cost < value)
                    {
                        value = cost;
                        challenges.Clear();
                        challenges.Add(validChallenge);
                    }
                }

                Pr_Ward ward = null;
                Pr_Opha_Faith ophaFaith = null;

                if (challengeTypes_Enshadow.Contains(validChallenge.GetType()))
                {
                    Settlement set = validChallenge.location.settlement;
                    SettlementHuman settlementHuman = null;

                    if (set != null)
                    {
                        settlementHuman = set as SettlementHuman;
                    }

                    if (settlementHuman != null)
                    {
                        foreach (Property property in settlementHuman.location.properties)
                        {
                            if (ward == null)
                            {
                                ward = property as Pr_Ward;
                            }

                            if (ophaFaith == null)
                            {
                                ophaFaith = property as Pr_Opha_Faith;
                            }

                            if (ward != null && ophaFaith != null)
                            {
                                break;
                            }
                        }
                    }

                    if (set == null)
                    {

                    }
                    else if (settlementHuman == null)
                    {

                    }
                    else if (ophaFaith == null && (ward == null || ward.charge < 0.66))
                    {

                    }
                }

                if (challengeTypes.Contains(validChallenge.GetType()))
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    if (challenges.Count == 0 || cost == value)
                    {
                        value = cost;
                        challenges.Add(validChallenge);
                    }
                    else if (cost < value)
                    {
                        value = cost;
                        challenges.Clear();
                        challenges.Add(validChallenge);
                    }
                }

                if (challengeTypes_Death.Contains(validChallenge.GetType()))
                {
                    double cost = (double)map.getStepDist(validChallenge.location, vampire.location) + 2.5;
                    // Random number between 1.0 and 3.0, weighted by bell curve towards 2.0.
                    cost *= 1.0 + (Eleven.random.NextDouble() + Eleven.random.NextDouble());
                    cost /= Math.Min(5.0, deathCharge / 5.0);
                    if (challenges.Count == 0 || cost == value)
                    {
                        value = cost;
                        challenges.Add(validChallenge);
                    }
                    else if (cost < value)
                    {
                        value = cost;
                        challenges.Clear();
                        challenges.Add(validChallenge);
                    }
                }
            }

            if (challenges.Count > 0)
            {
                //Console.WriteLine("CommunityLib: Randomly selected " + challenge.getName() + " challenge.");
                vampire.task = new Task_GoToPerformChallenge(challenges[Eleven.random.Next(challenges.Count)]);
            }
            else
            {


                for (int i = 0; i < 125; i++)
                {

                }

                map.overmind.autoAI.aiShadow.turnTick(vampire);
            }
        }
    }
}
