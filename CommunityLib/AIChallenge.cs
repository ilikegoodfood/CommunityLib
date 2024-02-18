using Assets.Code;
using System;
using System.Collections.Generic;
using System.IO;
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
            Aquaphibious,
            RequireLocal
        }

        public static Tuple<Unit, Location, int, Location[]> lastPath;

        public Type challengeType;

        public List<ChallengeTags> tags;

        public bool safeMove;

        public bool isRitual;

        public double profile;

        public List<Func<AgentAI.ChallengeData, UA, double, double>> delegates_Profile;

        public List<Func<AgentAI.ChallengeData, bool>> delegates_Valid;

        public List<Func<AgentAI.ChallengeData, UA, bool>> delegates_ValidFor;

        public List<Func<AgentAI.ChallengeData, UA, double, List<ReasonMsg>, double>> delegates_Utility;

        private AgentAI.DebugProperties debugInternal = new AgentAI.DebugProperties();

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
                throw new ArgumentException("CommunityLib: challengeType " + challengeType + " is not subclass of Challenge");
            }

            this.challengeType = challengeType;
            this.profile = profile;
            this.safeMove = safeMove;

            if (tags == null)
            {
                tags = new List<ChallengeTags>();
            }
            this.tags = tags;

            delegates_Profile = new List<Func<AgentAI.ChallengeData, UA,  double, double>>();
            delegates_Valid = new List<Func<AgentAI.ChallengeData,  bool>>();
            delegates_ValidFor = new List<Func<AgentAI.ChallengeData, UA, bool>>();
            delegates_Utility = new List<Func<AgentAI.ChallengeData, UA, double, List<ReasonMsg>, double>>();

            isRitual = challengeType.IsSubclassOf(typeof(Ritual));
        }

        public double checkChallengeProfile(AgentAI.ChallengeData challengeData, UA ua, AgentAI.ControlParameters controlParams)
        {
            if (challengeData.challenge.GetType() != challengeType)
            {
                Console.WriteLine("CommunityLib: ERROR: Challenge is not of Type " + challengeType);
                return -1;
            }

            double result = profile;
            if (delegates_Profile != null)
            {
                foreach (Func<AgentAI.ChallengeData, UA, double, double> delegate_Profile in delegates_Profile)
                {
                    result = delegate_Profile(challengeData, ua, profile);
                }
            }

            debugInternal = AgentAI.debugInternal;
            if (debugInternal.debug && !ModCore.Get().GetAgentAI().isAIRunning() && (debugInternal.outputProfile_AllChallenges || debugInternal.outputProfile_VisibleChallenges))
            {
                Console.WriteLine("CommunityLib: Checking profile of" + challengeData.challenge.getName());
            }

            if (debugInternal.debug && debugInternal.outputProfile_AllChallenges)
            {
                Console.WriteLine("CommunityLib: Profile: " + result);
            }

            return result;
        }

        public bool checkChallengeVisibility(AgentAI.ChallengeData challengeData, UA ua, AgentAI.ControlParameters controlParams)
        {
            if (challengeData.challenge.GetType() != challengeType)
            {
                Console.WriteLine("CommunityLib: ERROR: Challenge is not of Type " + challengeType);
                return false;
            }

            debugInternal = AgentAI.debugInternal;
            if (debugInternal.debug && !ModCore.Get().GetAgentAI().isAIRunning() && (debugInternal.outputProfile_AllChallenges || debugInternal.outputProfile_VisibleChallenges))
            {
                Console.WriteLine("CommunityLib: Checking " + challengeData.challenge.getName() + " is visible to " + ua.getName());
            }

            double profile = checkChallengeProfile(challengeData, ua, controlParams);
            int dist = ua.map.getStepDist(ua.location, challengeData.location);
            if (profile / 10 >= dist)
            {
                if (debugInternal.debug && !debugInternal.outputProfile_AllChallenges && debugInternal.outputProfile_VisibleChallenges)
                {
                    Console.WriteLine("CommunityLib: Profile: " + profile);
                }
                if (debugInternal.debug && (debugInternal.outputVisibility_AllChallenges || debugInternal.outputVisibility_VisibleChallenges))
                {
                    Console.WriteLine("CommunityLib: Visible");
                }
                return true;
            }

            if (debugInternal.outputVisibility_AllChallenges)
            {
                Console.WriteLine("CommunityLib: NOT Visible");
            }
            return false;
        }

        public bool checkChallengeIsValid(AgentAI.ChallengeData challengeData, UA ua, AgentAI.ControlParameters controlParams)
        {
            if (challengeData.challenge.GetType() != challengeType)
            {
                Console.WriteLine("CommunityLib: ERROR: Challenge is not of Type " + challengeType);
                return false;
            }

            debugInternal = AgentAI.debugInternal;
            if (debugInternal.debug && !ModCore.Get().GetAgentAI().isAIRunning() && (debugInternal.outputValidity_AllChallenges || debugInternal.outputValidity_ValidChallenges))
            {
                Console.WriteLine("CommunityLib: Checking " + challengeData.challenge.getName() + " is valid for " + ua.getName());
            }

            if (!validTags(challengeData, ua, controlParams))
            {
                return false;
            }

            if (delegates_Valid != null)
            {
                foreach (Func<AgentAI.ChallengeData, bool> delegate_Valid in delegates_Valid)
                {
                    if (!delegate_Valid(challengeData))
                    {
                        if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                        {
                            Console.WriteLine("CommunityLib: Invalid: Failed Valid delegates");
                        }
                        return false;
                    }
                }
            }

            if (delegates_ValidFor != null)
            {
                foreach (Func<AgentAI.ChallengeData, UA, bool> delegate_ValidFor in delegates_ValidFor)
                {
                    if (!delegate_ValidFor(challengeData, ua))
                    {
                        if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                        {
                            Console.WriteLine("CommunityLib: Invalid: Failed ValidFor delegates");
                        }
                        return false;
                    }
                }
            }

            if (challengeData.location != ua.location)
            {
                Location[] pathTo;

                bool newPath = false;
                if (lastPath.Item1 == ua && lastPath.Item2 == challengeData.location && lastPath.Item3 == ua.map.turn)
                {
                    pathTo = lastPath.Item4;
                }
                else
                {
                    pathTo = ua.location.map.getPathTo(ua.location, challengeData.location, ua, safeMove);
                    newPath = true;
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    if (debugInternal.outputValidity_AllChallenges)
                    {
                        Console.WriteLine("CommunityLib: Invalid: Failed to find Path");
                    }

                    if (newPath)
                    {
                        lastPath = new Tuple<Unit, Location, int, Location[]>(ua, challengeData.location, ua.map.turn, pathTo);
                    }
                    return false;
                }
            }
            else if (safeMove && (challengeData.location.soc?.hostileTo(ua) ?? false))
            {
                if (debugInternal.outputValidity_AllChallenges)
                {
                    Console.WriteLine("CommunityLib: Invalid: Failed safeMove");
                }
                return false;
            }

            if (debugInternal.debug && (debugInternal.outputValidity_AllChallenges || debugInternal.outputValidity_ValidChallenges))
            {
                Console.WriteLine("CommunityLib: Valid");
            }
            return true;
        }

        private bool validTags(AgentAI.ChallengeData challengeData, UA ua, AgentAI.ControlParameters controlParams)
        {
            foreach (ChallengeTags tag in tags)
            {
                switch (tag)
                {
                    case ChallengeTags.BaseValid:
                        if (!challengeData.challenge.valid())
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: challenge.valid returned false");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.BaseValidFor:
                        if (!challengeData.challenge.validFor(ua))
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: challenge.validFor returned false");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.Forbidden:
                        if (debugInternal.outputValidity_AllChallenges)
                        {
                            Console.WriteLine("CommunityLib: Invalid: Challenge Forbidden");
                        }
                        return false;
                    case ChallengeTags.RequiresDeath:
                        Pr_Death death = challengeData.location.properties.OfType<Pr_Death>().FirstOrDefault();
                        if (death == null || death.charge <= 0.0)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No Death at location");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresShadow:
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No shadow at location");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresInfiltrated:
                        if (challengeData.location.settlement?.infiltration < 1.0)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Settlement not infiltrated");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.Enshadows:
                        if (challengeData.location.getShadow() >= 1.0)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No shadow at location");
                            }
                            return false;
                        }
                        Settlement settlement = challengeData.location.settlement;
                        if (settlement != null)
                        {
                            if (settlement.shadowPolicy == Settlement.shadowResponse.DENY)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Settlement cannot be ensahdowed");
                                }
                                return false;
                            }

                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Perfected settlement cannot be enshadowed");
                                }
                                return false;
                            }
                        }
                        Society society = challengeData.location.soc as Society;
                        if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Alliance cannot be enshadowed");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.PushesShadow:
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No shadow at location");
                            }
                            return false;
                        }
                        double deltaShadow = 0.0;
                        List<Location> neighbouringLocations = challengeData.location.getNeighbours();
                        Pr_Ward ward;
                        foreach (Location loc in neighbouringLocations)
                        {
                            settlement = loc.settlement;
                            if (settlement == null && challengeData.challenge is Ch_WellOfShadows)
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
                                    if (challengeData.challenge is Ch_WellOfShadows)
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
                            if (society != null && society.isAlliance && challengeData.challenge.map.opt_allianceState == 1)
                            {
                                continue;
                            }
                            ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                            double charge = (ward?.charge ?? 0.0) / 100;
                            deltaShadow += Math.Max((challengeData.location.getShadow() - loc.getShadow()) * (1 - charge), 0.0);
                        }
                        if (deltaShadow < 0.05)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No potential to push shadow to neighbouring locations");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RemovesShadow:
                        settlement = challengeData.location.settlement;
                        if (settlement != null)
                        {
                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Perfected settlements cannot be enshadowed");
                                }
                                return false;
                            }
                        }
                        society = challengeData.location.soc as Society;
                        if (society != null && society.isAlliance && challengeData.challenge.map.opt_allianceState == 1)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Alliance cannot be enshadowed");
                            }
                            return false;
                        }
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: No shadow at location");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.PreventsReceiveingShadow:
                        settlement = challengeData.location.settlement;
                        if (settlement != null)
                        {
                            SettlementHuman settlementHuman = settlement as SettlementHuman;
                            if (settlementHuman?.ophanimTakeOver ?? false)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Perfected settlements cannot be enshadowed");
                                }
                                return false;
                            }
                        }
                        society = challengeData.location.soc as Society;
                        if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Alliance cannot be enshadowed");
                            }
                            return false;
                        }
                        if (challengeData.location.getShadow() >= 1.0)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location alreadt enshadowed");
                            }
                            return false;
                        }
                        ward = challengeData.location.properties.OfType<Pr_Ward>().FirstOrDefault();
                        if (ward?.charge >= 99)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location protected by ward");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RecruitsMinion:
                        Ch_RecruitMinion recruitMinion = challengeData.challenge as Ch_RecruitMinion;
                        Ch_RecruitOgre recruitOgre = challengeData.challenge as Ch_RecruitOgre;
                        if (recruitMinion != null)
                        {
                            if (ua.person?.gold < recruitMinion.exemplar.getGoldCost())
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Insufficient Gold");
                                }
                                return false;
                            }
                        }
                        else if (recruitOgre != null)
                        {
                            if (ua.person?.gold < recruitOgre.exemplar.getGoldCost())
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Insufficient Gold");
                                }
                                return false;
                            }
                        }
                        break;
                    case ChallengeTags.ManageMenace:
                        if (ua.menace <= ua.inner_menaceMin)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Menace at minimum");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageProfile:
                        if (ua.profile <= ua.inner_profileMin)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Profile at minimum");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageMenaceProfile:
                        if (ua.menace + ua.profile <= ua.inner_menaceMin + ua.inner_profileMin)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Menace and Profile are at minimum");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.ManageSocietyMenace:
                        if (ua.society?.menace <= 0.05)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: No society menace");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.HealGood:
                        foreach (int tagIndex in ua.getPositiveTags())
                        {
                            if (tagIndex == Tags.UNDEAD || tagIndex == Tags.ORC)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: UA is Orc or Undead");
                                }
                                return false;
                            }
                        }
                        break;
                    case ChallengeTags.RequiresSociety:
                        if (ua.location.soc == null)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location is wilderness");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresNoSociety:
                        if (ua.location.soc != null)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location is not wilderness");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RequiresOwnSociety:
                        if (ua.society == null ||  ua.society != challengeData.location.soc)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location does not belong to own soceity");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.ForbidOwnSociety:
                        if (ua.society != null && ua.society == challengeData.location.soc)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location belongs to own soceity");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.ForbidWar:
                        if ((ua.society == null || ua.society != challengeData.location.soc))
                        {
                            if (challengeData.location.soc != null && ua.society.relations.ContainsKey(challengeData.location.soc) && ua.society.relations[challengeData.location.soc].state == DipRel.dipState.war)
                            {
                                if (debugInternal.outputValidity_AllChallenges)
                                {
                                    Console.WriteLine("CommunityLib: Invalid: Is at war with society that controls location");
                                }
                                return false;
                            }
                        }
                        break;
                    case ChallengeTags.ForbidPeace:
                        if (ua.society != null && ua.society == challengeData.location.soc)
                        {
                            break;
                        }
                        else if (challengeData.location.soc != null && ua.society.relations.ContainsKey(challengeData.location.soc) && ua.society.relations[challengeData.location.soc].state != DipRel.dipState.war)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Is not at war with soceity that controls location");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.Aquaphibious:
                        if (!challengeData.location.isOcean && !challengeData.location.isCoastal)
                        {
                            if (debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Location is not coastal or ocean");
                            }
                            return false;
                        }
                        break;
                    case ChallengeTags.RequireLocal:
                        if (challengeData.location == ua.location)
                        {
                            return true;
                        }
                        return false;
                    default:
                        break;
                }
            }

            return true;
        }

        public double checkChallengeUtility(AgentAI.ChallengeData challengeData, UA ua, AgentAI.ControlParameters controlParams, List<ReasonMsg> reasonMsgs = null)
        {
            double result = 0.0;

            if (challengeData.challenge.GetType() != challengeType)
            {
                reasonMsgs?.Add(new ReasonMsg("ERROR: Challenge " + challengeData.challenge.getName() + " is not subtype of " + challengeType, -10000.0));
                return -10000.0;
            }

            if (challengeData.location != ua.location)
            {
                Location[] pathTo;

                bool newPath = false;
                if (lastPath.Item1 == ua && lastPath.Item2 == challengeData.location && lastPath.Item3 == ua.map.turn)
                {
                    pathTo = lastPath.Item4;
                }
                else
                {
                    pathTo = ua.location.map.getPathTo(ua.location, challengeData.location, ua, safeMove);
                    newPath = true;
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    reasonMsgs?.Add(new ReasonMsg("Cannot find path to challenge", -10000.0));
                    result -= 10000.0;
                }

                if (newPath)
                {
                    lastPath = new Tuple<Unit, Location, int, Location[]>(ua, challengeData.location, ua.map.turn, pathTo);
                }
            }

            result += utilityTags(challengeData, ua, reasonMsgs);

            if (delegates_Utility != null)
            {
                foreach (Func<AgentAI.ChallengeData, UA, double, List<ReasonMsg>, double> delegate_Utility in delegates_Utility)
                {
                    result = delegate_Utility(challengeData, ua, result, reasonMsgs);
                }
            }
            
            return result;
        }

        private double utilityTags(AgentAI.ChallengeData challengeData, UA ua, List<ReasonMsg> reasonMsgs)
        {
            double result = 0.0;
            foreach (ChallengeTags tag in tags)
            {
                switch (tag)
                {
                    case ChallengeTags.BaseUtility:
                        double val = challengeData.challenge.getMenace();
                        if (val > 0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Menace", val));
                            result += val;
                        }
                        result += challengeData.challenge.getUtility(ua, reasonMsgs);
                        break;
                    case ChallengeTags.Forbidden:
                        val = -10000.0;
                        reasonMsgs?.Add(new ReasonMsg("Forbidden", val));
                        result += val;
                        break;
                    case ChallengeTags.RequiresDeath:
                        Pr_Death death = challengeData.location.properties.OfType<Pr_Death>().FirstOrDefault();
                        val = -1000;
                        if (death == null || death.charge <= 0.05)
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
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            val = -1000;
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.Enshadows:
                        val = -1000;
                        if (challengeData.location.getShadow() == 1.0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Location Already Ensahdowed", val));
                            result += val;
                        }
                        Settlement settlement = challengeData.location.settlement;
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
                        Society society = challengeData.location.soc as Society;
                        if (society != null && society.isAlliance)
                        {
                            if (challengeData.challenge.map.opt_allianceState == 1)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Alliance cannot be enshadowed", val));
                                result += val;
                            }
                            else if (challengeData.challenge.map.opt_allianceState == 2)
                            {
                                val = -20;
                                reasonMsgs?.Add(new ReasonMsg("Alliance can purge shadow", val));
                                result += val;
                            }
                        }
                        val = (1 - challengeData.location.getShadow()) * 100;
                        reasonMsgs?.Add(new ReasonMsg("Purity", val));
                        result += val;
                        break;
                    case ChallengeTags.PushesShadow:
                        val = -1000;
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        else
                        {
                            double deltaShadow = 0.0;
                            foreach (Location loc in challengeData.location.getNeighbours())
                            {
                                settlement = loc.settlement;
                                if (settlement == null && challengeData.challenge is Ch_WellOfShadows)
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
                                        if (challengeData.challenge is Ch_WellOfShadows)
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
                                if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                Pr_Ward ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                                double charge = 0.0;

                                if (ward != null)
                                {
                                    charge = ward.charge / 100;
                                }

                                double diff = challengeData.location.getShadow() - loc.getShadow();
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
                        if (challengeData.location.getShadow() < 0.05)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Requires Shadow", val));
                            result += val;
                        }
                        else
                        {
                            settlement = challengeData.location.settlement;
                            Pr_WellOfShadows well = challengeData.location.properties.OfType<Pr_WellOfShadows>().FirstOrDefault();
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
                            foreach (Location loc in challengeData.location.getNeighbours())
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
                                if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                Pr_Ward ward = loc.properties.OfType<Pr_Ward>().FirstOrDefault();
                                double charge = ward?.charge ?? 0.0;
                                double diff = challengeData.location.getShadow() - loc.getShadow();
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
                        if (challengeData.location.getShadow() >= 1.0)
                        {
                            reasonMsgs?.Add(new ReasonMsg("Location Already Enshadowed", val));
                            result += val;
                        }
                        else
                        {
                            Pr_Ward ward = challengeData.location.properties.OfType<Pr_Ward>().FirstOrDefault();
                            val = (ward?.charge ?? -1.0) * -1;
                            if (val < 0.0)
                            {
                                reasonMsgs?.Add(new ReasonMsg("Ward Strength", val));
                                result += val;
                            }

                            settlement = challengeData.location.settlement;
                            Pr_WellOfShadows well = challengeData.location.properties.OfType<Pr_WellOfShadows>().FirstOrDefault();
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
                            foreach (Location loc in challengeData.location.getNeighbours())
                            {
                                settlement = challengeData.location.settlement;
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
                                if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                                {
                                    continue;
                                }
                                double diff = loc.getShadow() - challengeData.location.getShadow();
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
                        Ch_RecruitMinion recruitMinion = challengeData.challenge as Ch_RecruitMinion;
                        Ch_RecruitOgre recruitOgre = challengeData.challenge as Ch_RecruitOgre;
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
                            if (dismissalPlan.Length > 0)
                            {
                                for (int i = 0; i < dismissalPlan.Length; i++)
                                {
                                    if (dismissalPlan[i] && ua.minions[i] != null)
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
                        if (challengeData.location.hex.purity > 0.25)
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
                        if (ua.society != null && ua.society != challengeData.location.soc)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires Own Society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.ForbidOwnSociety:
                        if (ua.society != null && ua.society == challengeData.location.soc)
                        {
                            val = -1000.0;
                            reasonMsgs?.Add(new ReasonMsg("Requires society that is not own society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.PreferOwnSociety:
                        if (ua.society != null && ua.society == challengeData.location.soc)
                        {
                            val = 10;
                            reasonMsgs?.Add(new ReasonMsg("Own Society", val));
                            result += val;
                        }
                        break;
                    case ChallengeTags.AvoidOwnSociety:
                            if (ua.society != null && ua.society == challengeData.location.soc)
                            {
                                val = -10;
                                reasonMsgs?.Add(new ReasonMsg("Own Society", val));
                                result += val;
                            }
                        break;
                    case ChallengeTags.PreferPositiveRelations:
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null)
                                {
                                    if (rel.state != DipRel.dipState.war && rel.state != DipRel.dipState.hostile)
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
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null)
                                {
                                    if (rel.state != DipRel.dipState.war && rel.state != DipRel.dipState.hostile)
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
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null)
                                {
                                    if (rel.state != DipRel.dipState.alliance)
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
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null)
                                {
                                    if (rel.state != DipRel.dipState.alliance)
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
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null && rel.state == DipRel.dipState.war)
                                {
                                    val = -1000.0;
                                    reasonMsgs?.Add(new ReasonMsg("At War", val));
                                    result += val;
                                }
                            }
                        }
                        break;
                    case ChallengeTags.ForbidPeace:
                        if (ua.society != null && challengeData.location.soc != null)
                        {
                            if (ua.society != null && ua.society != challengeData.location.soc)
                            {
                                if (challengeData.location.soc != null && ua.society.relations.TryGetValue(challengeData.location.soc, out DipRel rel) && rel != null && rel.state != DipRel.dipState.war)
                                {
                                    val = -1000.0;
                                    reasonMsgs?.Add(new ReasonMsg("Not At War", val));
                                    result += val;
                                }
                            }
                        }
                        break;
                    case ChallengeTags.PreferLocal:
                        double dist = ua.map.getStepDist(ua.location, challengeData.location);
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
                        dist = ua.map.getStepDist(ua.location, challengeData.location);
                        dist -= ModCore.Get().tryGetRand(ua, challengeData, "localRand", 1 + Eleven.random.Next(2) + Eleven.random.Next(2));
                        if (ModCore.Get().GetAgentAI().isAIRunning())
                        {
                            ModCore.Get().setRand(ua, challengeData, "localRand", 1 + Eleven.random.Next(2) + Eleven.random.Next(2));
                        }
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
