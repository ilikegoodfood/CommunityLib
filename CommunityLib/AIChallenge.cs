using Assets.Code;
using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    public class AIChallenge : IEquatable<AIChallenge>
    {
        public enum ChallengeTags
        {
            None,
            BaseValid,
            BaseValidFor,
            BaseUtility,
            Forbidden,
            RequiresDeath,
            RequiresShadow,
            RequiresInfiltrated,
            Enshadows,
            PushesShadow,
            RemovesShadow,
            PreventsReceiveingShadow,
            RecruitsMinion,
            ManageMenace,
            ManageProfile,
            ManageMenaceProfile,
            ManageSocietyMenace,
            HealAll,
            HealGood,
            HealOrc,
            HealUndead,
            Rest,
            StayInShadow,
            RequiresSociety,
            RequiresNoSociety,
            RequiresOwnSociety,
            ForbidOwnSociety,
            PreferOwnSociety,
            AvoidOwnSociety,
            PreferPositiveRelations,
            AvoidPositiveRelations,
            PreferNegativeRelations,
            AvoidNegativeRelations,
            ForbidWar,
            ForbidPeace,
            PreferLocal,
            PreferLocalRandomized,
            Aquaphibious
        }

        public Type challengeType;

        public List<ChallengeTags> tags;

        public bool safeMove;

        public bool isRitual;

        public double profile;

        public List<Func<Challenge, UA, Location, double>> delegates_Profile;

        public List<Func<Challenge, Location, bool>> delegates_Valid;

        public List<Func<Challenge, UA, Location, bool>> delegates_ValidFor;

        public List<Func<Challenge, UA, Location, List<ReasonMsg>, double>> delegates_Utility;

        /// <summary>
        /// The constructor for AIChallnges. Delegates must be assigned after initialization.
        /// </summary>
        /// <param name="challengeType"></param>
        /// <param name="tags"></param>
        /// <param name="profile"></param>
        /// <param name="safeMove"></param>
        /// <param name="ignoreDistance"></param>
        /// <exception cref="ArgumentException"></exception>
        public AIChallenge(Type challengeType, double profile, List<ChallengeTags> tags = null, bool safeMove = false)
        {
            if (!challengeType.IsSubclassOf(typeof(Challenge)))
            {
                throw new ArgumentException("challengeType is not subclass of Challenge", "challengeType");
            }

            this.challengeType = challengeType;
            this.profile = profile;
            this.safeMove = safeMove;

            if (tags == null)
            {
                tags = new List<ChallengeTags>();
            }
            this.tags = tags;

            delegates_Profile = new List<Func<Challenge, UA, Location, double>>();
            delegates_Valid = new List<Func<Challenge, Location, bool>>();
            delegates_ValidFor = new List<Func<Challenge, UA, Location, bool>>();
            delegates_Utility = new List<Func<Challenge, UA, Location, List<ReasonMsg>, double>>();

            isRitual = challengeType.IsSubclassOf(typeof(Ritual));
        }

        public double checkChallengeProfile(Challenge challenge, UA ua, Location location = null)
        {
            if (challenge.GetType() != challengeType)
            {
                return -1;
            }

            if (isRitual)
            {
                if (location == null)
                {
                    return -1;
                }
            }
            else
            {
                location = challenge.location;
            }

            if (delegates_Profile != null)
            {
                foreach (Func<Challenge, UA, Location, double> delegate_Profile in delegates_Profile)
                {
                    profile += delegate_Profile(challenge, ua, location);
                }
            }

            return profile;
        }

        public bool checkChallengeVisibility(Challenge challenge, UA ua, Location location = null)
        {
            if (challenge.GetType() != challengeType)
            {
                return false;
            }

            if (isRitual)
            {
                if (location == null)
                {
                    return false;
                }
            }
            else
            {
                location = challenge.location;
            }

            double profile = checkChallengeProfile(challenge, ua, location);
            if (profile / 10 >= ua.map.getStepDist(ua.location, location))
            {
                return true;
            }

            return false;
        }

        public bool checkChallengeIsValid(Challenge challenge, UA ua, Location location = null)
        {
            // Test Message
            Console.WriteLine("CommunityLib: Checking validity of " + challenge.getName() + " at " + location.getName() + " for " + ua.getName() + " of social group " + ua.society.getName());

            if (challenge.GetType() != challengeType)
            {
                Console.WriteLine("ERROR:: Challenge is not of Type " + challengeType);
                return false;
            }

            if (isRitual)
            {
                if (location == null)
                {
                    Console.WriteLine("ERROR:: Challenge is ritual and location is null");
                    return false;
                }
            }
            else
            {
                location = challenge.location;
            }

            if (location != ua.location)
            {
                Location[] pathTo = ua.location.map.getPathTo(ua.location, location, ua, safeMove);
                if (pathTo == null || pathTo.Length < 2)
                {
                    return false;
                }
            }
            else if (safeMove && (location.soc?.hostileTo(ua) ?? false))
            {
                return false;
            }

            if (!validTags(challenge, ua, location))
            {
                return false;
            }

            if (delegates_Valid != null)
            {
                foreach (Func<Challenge, Location, bool> delegate_Valid in delegates_Valid)
                {
                    if (!delegate_Valid(challenge, location))
                    {
                        return false;
                    }
                }
            }

            if (delegates_ValidFor != null)
            {
                foreach (Func<Challenge, UA, Location, bool> delegate_ValidFor in delegates_ValidFor)
                {
                    if (!delegate_ValidFor(challenge, ua, location))
                    {
                        return false;
                    }
                }
            }

            // Test Message
            Console.WriteLine("CommunityLib: Valid");
            //Console.WriteLine("CommunityLib: " + challenge.getName() + " at " + location.getName() + " is valid for " + ua.getName() + " of social group " + ua.society.getName());
            return true;
        }

        private bool validTags(Challenge challenge, UA ua, Location location)
        {
            foreach (ChallengeTags tag in tags)
            {
                switch (tag)
                {
                    case ChallengeTags.BaseValid:
                        if (!challenge.valid())
                        {
                            // Console.WriteLine("Not Base Valid");
                            return false;
                        }
                        break;
                    case ChallengeTags.BaseValidFor:
                        if (!challenge.validFor(ua))
                        {
                            // Console.WriteLine("Not Base ValidFor " + ua.getName());
                            return false;
                        }
                        break;
                    case ChallengeTags.Forbidden:
                        return false;
                    case ChallengeTags.RequiresDeath:
                        Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
                        if (death == null || death.charge <= 0.0)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresShadow:
                        if (location.getShadow() < 0.05)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresInfiltrated:
                        if (location.settlement?.infiltration < 1.0)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.Enshadows:
                        if (location.getShadow() >= 1.0)
                        {
                            return false;
                        }
                        Settlement settlement = location.settlement;
                        if (settlement != null)
                        {
                            if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                            {
                                return false;
                            }

                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                return false;
                            }
                        }
                        Society society = location.soc as Society;
                        if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.PushesShadow:
                        if (location.getShadow() < 0.05)
                        {
                            return false;
                        }
                        double deltaShadow = 0.0;
                        List<Location> neighbouringLocations = location.getNeighbours();
                        Pr_Ward ward;
                        foreach (Location loc in neighbouringLocations)
                        {
                            settlement = loc.settlement;
                            if (settlement == null && challenge is Ch_WellOfShadows)
                            {
                                continue;
                            }
                            else
                            {
                                if(settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                                {
                                    continue;
                                }
                                SettlementHuman settlementHuman = settlement as SettlementHuman;
                                if (settlementHuman == null)
                                {
                                    if (challenge is Ch_WellOfShadows)
                                    {
                                        continue;
                                    }
                                }
                                else if (settlementHuman.ophanimTakeOver)
                                {
                                    continue;
                                }
                            }
                            society = loc.soc as Society;
                            if (society != null && society.isAlliance && challenge.map.opt_allianceState == 1)
                            {
                                continue;
                            }
                            ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                            double charge = (ward?.charge ?? 0.0) / 100;
                            deltaShadow += Math.Max((location.getShadow() - loc.getShadow()) * (1 - charge), 0.0);
                        }
                        if (deltaShadow < 0.05)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RemovesShadow:
                        settlement = location.settlement;
                        if (settlement != null)
                        {
                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                return false;
                            }
                        }
                        society = location.soc as Society;
                        if (society != null && society.isAlliance && challenge.map.opt_allianceState == 1)
                        {
                            return false;
                        }
                        if (location.getShadow() < 0.05)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.PreventsReceiveingShadow:
                        settlement = location.settlement;
                        if (settlement != null)
                        {
                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                return false;
                            }
                        }
                        society = location.soc as Society;
                        if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                        {
                            return false;
                        }
                        if (location.getShadow() >= 1.0)
                        {
                            return false;
                        }
                        ward = location.properties.OfType<Pr_Ward>().FirstOrDefault();
                        if (ward?.charge >= 99)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RecruitsMinion:
                        Ch_RecruitMinion recruitMinion = challenge as Ch_RecruitMinion;
                        Ch_RecruitOgre recruitOgre = challenge as Ch_RecruitOgre;
                        if (recruitMinion != null)
                        {
                            if (ua.getStatCommandLimit() < recruitMinion.exemplar.getCommandCost())
                            {
                                // Console.WriteLine("CommunityLib: Insufficient command to recruit minion");
                                return false;
                            }
                            if (ua.person?.gold < recruitMinion.exemplar.getGoldCost())
                            {
                                // Console.WriteLine("CommunityLib: Insufficient gold to recruit minion");
                                return false;
                            }
                        }
                        else if (recruitOgre != null)
                        {
                            if (ua.getStatCommandLimit() < recruitOgre.exemplar.getCommandCost())
                            {
                                return false;
                            }
                            if (ua.person?.gold < recruitOgre.exemplar.getGoldCost())
                            {
                                return false;
                            }
                        }
                        else if (ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageMenace:
                        if (ua.menace < ua.inner_menaceMin)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageProfile:
                        if (ua.profile < ua.inner_profileMin)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageMenaceProfile:
                        if (ua.menace < ua.inner_menaceMin + ua.inner_profileMin)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageSocietyMenace:
                        if (ua.society?.menace <= 0.05)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.HealGood:
                        foreach (int tagIndex in ua.getPositiveTags())
                        {
                            if (tagIndex == Tags.UNDEAD || tagIndex == Tags.ORC)
                            {
                                return false;
                            }
                        }
                        break;
                    case ChallengeTags.RequiresSociety:
                        if (ua.location.soc == null)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresNoSociety:
                        if (ua.location.soc != null)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresOwnSociety:
                        if (ua.society == null ||  ua.society != location.soc)
                        {
                            // Console.WriteLine("CommunityLib: Society " + location.soc?.getName() ?? "NULL" + " is not own society.");
                            return false;
                        }
                        break;
                    case ChallengeTags.ForbidOwnSociety:
                        if (ua.society != null && ua.society == location.soc)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.ForbidWar:
                        if ((ua.society == null || ua.society != location.soc))
                        {
                            if (location.soc != null && ua.society.relations.ContainsKey(location.soc) && ua.society.relations[location.soc].state == DipRel.dipState.war)
                            {
                                return false;
                            }
                        }
                        break;
                    case ChallengeTags.ForbidPeace:
                        if (ua.society != null && ua.society == location.soc)
                        {
                            break;
                        }
                        else if (location.soc != null && ua.society.relations.ContainsKey(location.soc) && ua.society.relations[location.soc].state != DipRel.dipState.war)
                        {
                            return false;
                        }
                        break;
                    case ChallengeTags.Aquaphibious:
                        if (!location.isOcean && !location.isCoastal)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        public double checkChallengeUtility(Challenge challenge, UA ua, List<ReasonMsg> reasonMsgs = null, Location location = null)
        {
            // Reome after testing.
            if (reasonMsgs == null)
            {
                reasonMsgs = new List<ReasonMsg>();
            }
            //

            double result = 0.0;

            if (challenge.GetType() != challengeType)
            {
                reasonMsgs?.Add(new ReasonMsg("ERROR: Challenge " + challenge.getName() + " is not of type " + challengeType, -10000.0));
                return -10000.0;
            }

            if (isRitual)
            {
                if (location == null)
                {
                    reasonMsgs?.Add(new ReasonMsg("ERROR: Ritual " + challenge.getName() + " requires a location to be performed at. Provided location was null", -10000.0));
                    return -10000.0;
                }
            }
            else
            {
                location = challenge.location;
            }

            if (location != ua.location)
            {
                Location[] pathTo = ua.location.map.getPathTo(ua.location, location, ua, safeMove);
                if (pathTo == null || pathTo.Length < 2)
                {
                    if (safeMove)
                    {
                        reasonMsgs?.Add(new ReasonMsg("Army Blocking Me", -125.0));
                        result -= 125;
                    }
                    else
                    {
                        reasonMsgs?.Add(new ReasonMsg("ERROR: Cannot find path to challenge", -10000.0));
                        result -= 10000.0;
                    }
                }
            }
            else if (safeMove && location.soc != null && location.soc.hostileTo(ua))
            {
                reasonMsgs?.Add(new ReasonMsg("Army Blocking Me", -125.0));
                result -= 125;
            }

            result += utilityTags(challenge, ua, location, reasonMsgs);

            if (delegates_Utility != null)
            {
                foreach (Func<Challenge, UA, Location, List<ReasonMsg>, double> delegate_Utility in delegates_Utility)
                {
                    result += delegate_Utility(challenge, ua, location, reasonMsgs);
                }
            }

            // Test Message
            Console.WriteLine("CommunityLib: AgentAI getting Utility for challenge " + challenge.getName() + " at " + location.getName() + " on behalf of " + ua.getName() + " of social group " + ua.society.getName());
            if (reasonMsgs != null)
            {
                foreach (ReasonMsg reasonMsg in reasonMsgs)
                {
                    Console.WriteLine(reasonMsg.msg + ": " + reasonMsg.value);
                }
                Console.WriteLine("Utility: " + result);
            }
            
            return result;
        }

        private double utilityTags(Challenge challenge, UA ua, Location location, List<ReasonMsg> reasonMsgs)
        {
            double result = 0.0;
            foreach (ChallengeTags tag in tags)
            {
                switch (tag)
                {
                    case ChallengeTags.BaseUtility:
                        result += challenge.getUtility(ua, reasonMsgs);
                        break;
                    case ChallengeTags.Forbidden:
                        double val = -10000.0;
                        reasonMsgs?.Add(new ReasonMsg("Forbidden", val));
                        result += val;
                        break;
                    case ChallengeTags.RequiresDeath:
                        Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
                        val = -1000;
                        if (death == null || death.charge <= 0.0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Requires Death", val));
                            result += val;
                        }
                        else
                        {
                            val = death.charge;
                            reasonMsgs?.Add(new ReasonMsg("Death", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.RequiresShadow:
                        if (location.getShadow() < 0.05)
                        {
                            val = -1000;
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.Enshadows:
                        val = -1000;
                        if (location.getShadow() == 1.0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Location Already Ensahdowed", val));
                            result += val;
                        }
                        Settlement settlement = location.settlement;
                        if (settlement != null)
                        {
                            if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Settlement cannot be enshadowed", val));
                                result += val;
                            }

                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Perfected settlements cannot be enshadowed", val));
                                result += val;
                            }
                        }
                        Society society = location.soc as Society;
                        if (society != null && society.isAlliance)
                        {
                            if (challenge.map.opt_allianceState == 1)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Alliance cannot be enshadowed", val));
                                result += val;
                            }
                            else if (challenge.map.opt_allianceState == 2)
                            {
                                val = -20;
                                reasonMsgs?.Add(new ReasonMsg("Alliance can purge shadow", val));
                                result += val;
                            }
                        }
                        val = (1 - location.getShadow()) * 100;
                        reasonMsgs?.Add(new ReasonMsg("Purity", val));
                        result += val;
                        break;
                    case ChallengeTags.PushesShadow:
                        val = -1000;
                        if (location.getShadow() < 0.05)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        else
                        {
                            double deltaShadow = 0.0;
                            foreach (Location loc in location.getNeighbours())
                            {
                                settlement = loc.settlement;
                                if (settlement == null && challenge is Ch_WellOfShadows)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                                    {
                                        continue;
                                    }
                                    SettlementHuman settlementHuman = settlement as SettlementHuman;
                                    if (settlementHuman == null)
                                    {
                                        if (challenge is Ch_WellOfShadows)
                                        {
                                            continue;
                                        }
                                    }
                                    else if (settlementHuman.ophanimTakeOver)
                                    {
                                        continue;
                                    }
                                }
                                society = loc.soc as Society;
                                if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                Pr_Ward ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                                double charge = (ward?.charge ?? 0.0) / 100;
                                double diff = location.getShadow() - loc.getShadow();
                                deltaShadow += Math.Max(diff * (1 - charge), 0.0);
                            }
                            if (deltaShadow < 0.05)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Potential Shadow Spread", val));
                                result += val;
                            }
                            else
                            {
                                val = deltaShadow * 100;
                                reasonMsgs?.Add(new ReasonMsg("Potential Shadow Spread", val));
                                result += val;
                            }
                        }
                        break;
                    case ChallengeTags.RemovesShadow:
                        val = -1000;
                        if (location.getShadow() < 0.05)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        else
                        {
                            settlement = location.settlement;
                            Pr_WellOfShadows well = location.properties.OfType<Pr_WellOfShadows>().FirstOrDefault();
                            if (settlement != null)
                            {
                                if (settlement.shadowPolicy == Settlement.shadowResponse.DENY && well == null)
                                {
                                    reasonMsgs?.Add(new ReasonMsg("Settlement cannot be enshadowed", val));
                                    result += val;
                                    break;
                                }
                                else if (settlement.shadowPolicy == Settlement.shadowResponse.RECEIVE_ONLY && well == null)
                                {
                                    val = -20;
                                    reasonMsgs?.Add(new ReasonMsg("Settlement cannot spread shadow", val));
                                    result += val;
                                    break;
                                }
                            }

                            double deltaShadow = 0.0;
                            foreach (Location loc in location.getNeighbours())
                            {
                                settlement = loc.settlement;
                                if (settlement != null)
                                {
                                    if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                                    {
                                        continue;
                                    }
                                    SettlementHuman settlementHuman = settlement as SettlementHuman;
                                    if (settlementHuman != null)
                                    {
                                        if (settlementHuman.ophanimTakeOver)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                society = loc.soc as Society;
                                if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                Pr_Ward ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                                double charge = ward?.charge ?? 0.0;
                                double diff = location.getShadow() - loc.getShadow();
                                deltaShadow += Math.Max(diff * (1 - charge), 0.0);
                            }

                            val = deltaShadow * 100;
                            reasonMsgs?.Add(new ReasonMsg("Threat of Shadow Spread", val));
                            result += val;

                            if ( well?.charge >= 0.05)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Well of Shadows", well.charge));
                                result += well.charge;
                            }
                        }
                        break;
                    case ChallengeTags.PreventsReceiveingShadow:
                        val = -1000;
                        if (location.getShadow() >= 1.0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Location Already Enshadowed", val));
                            result += val;
                        }
                        else
                        {
                            Pr_Ward ward = location.properties.OfType<Pr_Ward>().FirstOrDefault();
                            val = (ward?.charge ?? -1.0) * -1;
                            if (val < 0.0)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Ward Strength", val));
                                result += val;
                            }

                            settlement = location.settlement;
                            Pr_WellOfShadows well = location.properties.OfType<Pr_WellOfShadows>().FirstOrDefault();
                            if (settlement != null)
                            {
                                if (settlement.shadowPolicy == Settlement.shadowResponse.DENY && well == null)
                                {
                                    val = -1000;
                                    reasonMsgs?.Add(new ReasonMsg("Settlement cannot be enshadowed", val));
                                    result += val;
                                    break;
                                }
                                else if (settlement.shadowPolicy == Settlement.shadowResponse.RECEIVE_ONLY && well == null)
                                {
                                    val = -20;
                                    reasonMsgs?.Add(new ReasonMsg("Settlement cannot spread shadow", val));
                                    result += val;
                                    break;
                                }
                            }

                            double deltaShadow = 0.0;
                            double deltaShadow2 = 0.0;
                            foreach (Location loc in location.getNeighbours())
                            {
                                settlement = location.settlement;
                                if (settlement != null)
                                {
                                    if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                                    {
                                        continue;
                                    }
                                    SettlementHuman settlementHuman = settlement as SettlementHuman;
                                    if (settlementHuman?.ophanimTakeOver ?? false)
                                    {
                                        continue;
                                    }
                                }
                                society = loc.soc as Society;
                                if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                double diff = loc.getShadow() - location.getShadow();
                                if (diff > 0.0)
                                {
                                    Pr_WellOfShadows well2 = loc.properties.OfType<Pr_WellOfShadows>().FirstOrDefault();
                                    if (well2 != null)
                                    {
                                        reasonMsgs?.Add(new ReasonMsg("Neighbouring Well of Shadows", diff));
                                        diff *= 2;
                                    }
                                }
                                deltaShadow += Math.Max(diff, 0.0);
                                deltaShadow2 += Math.Max(diff * -1, 0.0);
                            }

                            val = deltaShadow * 100;
                            reasonMsgs?.Add(new ReasonMsg("Potential Shadow Spread", val));
                            result += val;

                            if (deltaShadow2 >= 0.05 && well?.charge >= 0.05)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Well of Shadows", well.charge));
                                result += well.charge;
                            }
                        }
                        break;
                    case ChallengeTags.RecruitsMinion:
                        Ch_RecruitMinion recruitMinion = challenge as Ch_RecruitMinion;
                        Ch_RecruitOgre recruitOgre = challenge as Ch_RecruitOgre;
                        if (recruitMinion != null)
                        {
                            if (recruitMinion.exemplar.getCommandCost() > ua.getStatCommandLimit())
                            {
                                reasonMsgs?.Add(new ReasonMsg("Exceeds Command Limit", ua.map.param.ch_recruitminion_parameterValue1));
                                result += ua.map.param.ch_recruitminion_parameterValue1;
                                continue;
                            }
                            bool[] dismissalPlan = recruitMinion.dismissalPlan(ua, recruitMinion.exemplar.getCommandCost());
                            for (int i = 0; i < dismissalPlan.Length; i++)
                            {
                                if (dismissalPlan[i])
                                {
                                    if (ua.minions[i] != null)
                                    {
                                        double dismissalCost = -recruitMinion.getMinionUtility(ua, ua.minions[i]);
                                        reasonMsgs?.Add(new ReasonMsg("Would have to dismiss " + ua.minions[i].getName(), dismissalCost));
                                        result += dismissalCost;
                                    }
                                }
                            }
                            val = recruitMinion.getMinionUtility(ua, recruitMinion.exemplar);
                            reasonMsgs?.Add(new ReasonMsg("Would gain " + recruitMinion.exemplar.getName(), val));
                            result += val;
                        }
                        else if (recruitOgre != null)
                        {
                            if (recruitOgre.exemplar.getCommandCost() > ua.getStatCommandLimit())
                            {
                                reasonMsgs?.Add(new ReasonMsg("Exceeds Command Limit", ua.map.param.ch_recruitogre_parameterValue2));
                                result += ua.map.param.ch_recruitogre_parameterValue2;
                                continue;
                            }
                            bool[] dismissalPlan = recruitOgre.dismissalPlan(ua, recruitOgre.exemplar.getCommandCost());
                            for (int i = 0; i < dismissalPlan.Length; i++)
                            {
                                if (dismissalPlan[i])
                                {
                                    if (ua.minions[i] != null)
                                    {
                                        double dismissalCost = -(ua.minions[i].getCommandCost() * ua.map.param.utility_UA_recruitPerPoint + recruitOgre.getPersonaUtilityTowardsMinion(ua, ua.minions[i]));
                                        reasonMsgs?.Add(new ReasonMsg("Would have to dismiss " + ua.minions[i].getName(), dismissalCost));
                                        result += dismissalCost;
                                    }
                                }
                            }
                            val = (recruitOgre.exemplar.getCommandCost() * ua.map.param.utility_UA_recruitPerPoint) + recruitOgre.getPersonaUtilityTowardsMinion(ua, recruitOgre.exemplar);
                            reasonMsgs?.Add(new ReasonMsg("Would gain " + recruitMinion.exemplar.getName(), val));
                            result += val;
                        }
                        else if (ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
                        {
                            if (ua.getStatCommandLimit() <= ua.getCurrentlyUsedCommand())
                            {
                                reasonMsgs?.Add(new ReasonMsg("Maximum Command Limit", ua.map.param.ch_recruitminion_parameterValue1));
                                result += ua.map.param.ch_recruitminion_parameterValue1;
                            }
                            else
                            {
                                int commandSlots = 0;
                                foreach (Minion minion in ua.minions)
                                {
                                    if (minion == null)
                                    {
                                        commandSlots++;
                                    }
                                }
                                val = commandSlots * 10;
                                reasonMsgs?.Add(new ReasonMsg("Available Command Slots", val));
                                result += val;
                                val = ua.map.param.utility_UA_recruitPerPoint * (ua.getStatCommandLimit() - ua.getCurrentlyUsedCommand());
                                reasonMsgs?.Add(new ReasonMsg("Available Command Limit", val));
                                result += val;
                            }
                        }
                        break;
                    case ChallengeTags.ManageMenace:
                        if (ua.menace > ua.inner_menaceMin)
                        {
                            val = ua.menace - ua.inner_menaceMin;
                            reasonMsgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.ManageProfile:
                        if (ua.profile > ua.inner_profileMin)
                        {
                            val = ua.profile - ua.inner_profileMin;
                            reasonMsgs?.Add(new ReasonMsg("Potential Profile Reduction", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.ManageMenaceProfile:
                        if (ua.menace > ua.inner_menaceMin || ua.profile > ua.inner_profileMin)
                        {
                            val = ua.menace - ua.inner_menaceMin;
                            reasonMsgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                            result+= val;
                            val = ua.profile - ua.inner_profileMin;
                            reasonMsgs?.Add(new ReasonMsg("Potential Profile Reduction", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.ManageSocietyMenace:
                        val = (ua.society?.menace ?? 0);
                        reasonMsgs?.Add(new ReasonMsg("Potential Menace Reduction", val * 5));
                        result += val;
                        break;
                    case ChallengeTags.HealAll:
                        val = ua.hp;
                        if (ua.maxHp > 0)
                        {
                            val /= ua.maxHp;
                        }
                        val = (1 - val) * ua.map.param.utility_UA_heal;
                        reasonMsgs?.Add(new ReasonMsg("HP Losses", val));
                        result += val;
                        double minionMaxHP = 0.0;
                        double minionHP = 0.0;
                        foreach (Minion minion in ua.minions)
                        {
                            if (minion != null)
                            {
                                minionMaxHP += minion.getMaxHP();
                                minionHP += minion.hp;
                            }
                        }
                        if (minionMaxHP > 0.0)
                        {
                            val = minionHP / minionMaxHP;
                            val = (1 - val) * ua.map.param.utility_UA_heal;
                            reasonMsgs?.Add(new ReasonMsg("Minion HP Loses", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.HealGood:
                        val = ua.hp;
                        if (ua.maxHp > 0)
                        {
                            val /= ua.maxHp;
                        }
                        val = (1 - val) * ua.map.param.utility_UA_heal;
                        reasonMsgs?.Add(new ReasonMsg("HP Losses", val));
                        result += val;
                        minionMaxHP = 0.0;
                        minionHP = 0.0;
                        foreach (Minion minion in ua.minions)
                        {
                            if (minion != null)
                            {
                                bool good = true;
                                foreach (int tag3 in minion.getTags())
                                {
                                    if (tag3 == Tags.UNDEAD || tag3 == Tags.ORC)
                                    {
                                        good = false;
                                        break;
                                    }
                                }
                                if (!good)
                                {
                                    continue;
                                }

                                minionMaxHP += minion.getMaxHP();
                                minionHP += minion.hp;
                            }
                        }
                        if (minionMaxHP > 0.0)
                        {
                            val = minionHP / minionMaxHP;
                            val = (1 - val) * ua.map.param.utility_UA_heal;
                            reasonMsgs?.Add(new ReasonMsg("Minion HP Loses", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.HealOrc:
                        val = ua.hp;
                        if (ua.maxHp > 0)
                        {
                            val /= ua.maxHp;
                        }
                        val = (1 - val) * ua.map.param.utility_UA_heal;
                        reasonMsgs?.Add(new ReasonMsg("HP Losses", val));
                        result += val;
                        minionMaxHP = 0.0;
                        minionHP = 0.0;
                        foreach (Minion minion in ua.minions)
                        {
                            if (minion != null)
                            {
                                bool isUndead = false;
                                foreach (int tag3 in minion.getTags())
                                {
                                    if (tag3 == Tags.UNDEAD)
                                    {
                                        isUndead = true;
                                        break;
                                    }
                                }
                                if (isUndead)
                                {
                                    continue;
                                }

                                minionMaxHP += minion.getMaxHP();
                                minionHP += minion.hp;
                            }
                        }
                        if (minionMaxHP > 0.0)
                        {
                            val = minionHP / minionMaxHP;
                            val = (1 - val) * ua.map.param.utility_UA_heal;
                            reasonMsgs?.Add(new ReasonMsg("Minion HP Loses", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.HealUndead:
                        val = ua.hp;
                        if (ua.maxHp > 0)
                        {
                            val /= ua.maxHp;
                        }
                        val = (1 - val) * ua.map.param.utility_UA_heal;
                        reasonMsgs?.Add(new ReasonMsg("HP Losses", val));
                        result += val;
                        minionMaxHP = 0.0;
                        minionHP = 0.0;
                        foreach (Minion minion in ua.minions)
                        {
                            if (minion != null)
                            {
                                bool isUndead = false;
                                foreach (int tag3 in minion.getTags())
                                {
                                    if (tag3 == Tags.UNDEAD)
                                    {
                                        isUndead = true;
                                        break;
                                    }
                                }
                                if (!isUndead)
                                {
                                    continue;
                                }

                                minionMaxHP += minion.getMaxHP();
                                minionHP += minion.hp;
                            }
                        }
                        if (minionMaxHP > 0.0)
                        {
                            val = minionHP / minionMaxHP;
                            val = (1 - val) * ua.map.param.utility_UA_heal;
                            reasonMsgs?.Add(new ReasonMsg("Minion HP Loses", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.Rest:
                        val = ua.map.param.ch_rest_parameterValue1;
                        reasonMsgs?.Add(new ReasonMsg("Base", val));
                        result += val;
                        val = ua.map.param.utility_ua_restDesire * ua.challengesSinceRest;
                        if (ua.location.index == ua.homeLocation)
                        {
                            val *= 2;
                        }
                        reasonMsgs?.Add(new ReasonMsg("Challenges Since Resting", val));
                        result += val;
                        break;
                    case ChallengeTags.StayInShadow:
                        if (location.hex.purity > 0.25)
                        {
                            val = -75.0;
                            reasonMsgs?.Add(new ReasonMsg("Shouldn't Leave Shadow", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.RequiresSociety:
                        if (ua.location.soc == null)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires Society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.RequiresNoSociety:
                        if (ua.location.soc != null)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires Wilderness", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.RequiresOwnSociety:
                        if (ua.society != null && ua.society != location.soc)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires Own Society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.ForbidOwnSociety:
                        if (ua.society != null && ua.society == location.soc)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires society that is not own society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.PreferOwnSociety:
                        if (ua.society != null && ua.society == location.soc)
                        {
                            val = 10;
                            reasonMsgs?.Add(new ReasonMsg("Own Society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.AvoidOwnSociety:
                            if (ua.society != null && ua.society == location.soc)
                            {
                                val = -10;
                                reasonMsgs?.Add(new ReasonMsg("Own Society", val));
                                result += val;
                            }
                        break;
                    case ChallengeTags.PreferPositiveRelations:
                        if (ua.society != null && location.soc != null)
                        {
                            if (ua.society != null && ua.society != location.soc)
                            {
                                if (location.soc != null && ua.society.relations.ContainsKey(location.soc))
                                {
                                    DipRel rel = ua.society.relations[location.soc];
                                    if (rel != null && rel.state != DipRel.dipState.war && rel.state != DipRel.dipState.hostile)
                                    {
                                        val = rel.status * 10;
                                        reasonMsgs?.Add(new ReasonMsg("International Relations", val));
                                        result += val;
                                    }
                                }
                            }
                        }
                        break;
                    case ChallengeTags.AvoidPositiveRelations:
                        if (ua.society != null && location.soc != null)
                        {
                            if (ua.society != null && ua.society != location.soc)
                            {
                                if (location.soc != null && ua.society.relations.ContainsKey(location.soc))
                                {
                                    DipRel rel = ua.society.relations[location.soc];
                                    if (rel != null && rel.state != DipRel.dipState.war && rel.state != DipRel.dipState.hostile)
                                    {
                                        val = rel.status * -10;
                                        reasonMsgs?.Add(new ReasonMsg("International Relations", val));
                                        result += val;
                                    }
                                }
                            }
                        }
                        break;
                    case ChallengeTags.PreferNegativeRelations:
                        if (ua.society != null && location.soc != null)
                        {
                            if (ua.society != null && ua.society != location.soc)
                            {
                                if (location.soc != null && ua.society.relations.ContainsKey(location.soc))
                                {
                                    DipRel rel = ua.society.relations[location.soc];
                                    if (rel != null && rel.state != DipRel.dipState.alliance)
                                    {
                                        if (rel.status <= 0.0)
                                        {
                                            val = rel.status * -10;
                                            reasonMsgs?.Add(new ReasonMsg("International Relations", val));
                                            result += val;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ChallengeTags.AvoidNegativeRelations:
                        if (ua.society != null && location.soc != null)
                        {
                            if (ua.society != null && ua.society != location.soc)
                            {
                                if (location.soc != null && ua.society.relations.ContainsKey(location.soc))
                                {
                                    DipRel rel = ua.society.relations[location.soc];
                                    if (rel != null && rel.state != DipRel.dipState.alliance)
                                    {
                                        if (rel.status <= 0.0)
                                        {
                                            val = rel.status * 10;
                                            reasonMsgs?.Add(new ReasonMsg("International Relations", val));
                                            result += val;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ChallengeTags.ForbidWar:
                        if (ua.society != null && ua.location.soc != null && ua.society != location.soc)
                        {
                            if (ua.society.relations.ContainsKey(location.soc) && ua.society.relations[location.soc].state == DipRel.dipState.war)
                            {
                                val = -1000.0;
                                reasonMsgs?.Add(new ReasonMsg("Is At War", val));
                                result += val;
                            }
                        }
                        break;
                    case ChallengeTags.ForbidPeace:
                        if (ua.society != null && location.soc != null && ua.society != location.soc)
                        {
                            if (ua.society.relations.ContainsKey(location.soc) && ua.society.relations[location.soc].state != DipRel.dipState.war)
                            {
                                val = -1000.0;
                                reasonMsgs?.Add(new ReasonMsg("Is Not At War", val));
                                result += val;
                            }
                        }
                        break;
                    case ChallengeTags.PreferLocal:
                        double dist = ua.map.getStepDist(ua.location, location);
                        if (dist == 0)
                        {
                            val = 20;
                        }
                        else
                        {
                            val = dist * -10;
                        }
                        reasonMsgs?.Add(new ReasonMsg("Distance", val));
                        result += val;
                        break;
                    case ChallengeTags.PreferLocalRandomized:
                        dist = ua.map.getStepDist(ua.location, location);
                        dist -= 1 + Eleven.random.Next(2) + Eleven.random.Next(2);
                        val = dist * -10;
                        reasonMsgs?.Add(new ReasonMsg("Distance", val));
                        result += val;
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            Type type = obj as Type;
            if (type != null && type == challengeType)
            {
                return true;
            }

            AIChallenge aiChallenge = obj as AIChallenge;
            if (aiChallenge?.challengeType == challengeType)
            {
                return true;
            }

            return false;
        }

        public bool Equals(AIChallenge other)
        {
            if (other?.challengeType == challengeType)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return challengeType.GetHashCode();
        }
    }
}
