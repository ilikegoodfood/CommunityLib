using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        public List<AIChallenge> aiChallenges_DeepOne;

        public List<AIChallenge> aiChallenges_Ghast;

        public List<AIChallenge> aiChallenges_OrcUpstart;

        public List<AIChallenge> aiChallenges_Vampire;

        private Map map;

        public UAENOverrideAI(Map map)
        {
            this.map = map;

            aiChallenges_DeepOne = new List<AIChallenge>();
            aiChallenges_Ghast = new List<AIChallenge>();
            aiChallenges_OrcUpstart = new List<AIChallenge>();
            aiChallenges_Vampire = new List<AIChallenge>();

            populateDeepOne();
            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_DeepOne));
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_DeepOne), aiChallenges_DeepOne);

            populateGhast();
            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_Ghast));
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Ghast), aiChallenges_Ghast);

            populateOrcUpstart();
            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_OrcUpstart));
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_OrcUpstart);

            populateVampire();
            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_Vampire));
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Vampire), aiChallenges_Vampire);
        }

        private void populateDeepOne()
        {
            AIChallenge challenge = new AIChallenge(typeof(Rt_DeepOneReproduce), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true);
            challenge.delegates_Valid.Add(delegate_Valid_Rt_DeepOneReproduce);
            challenge.delegates_Utility.Add(delegate_Utility_Rt_DeepOneReproduce);
            aiChallenges_DeepOne.Add(challenge);

            AIChallenge challenge1 = new AIChallenge(typeof(Ch_DeepOnesHumanAppearance), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true);
            challenge1.delegates_Valid.Add(delegate_Valid_Ch_DeepOnesHumanAppearance);
            challenge1.delegates_Utility.Add(delegate_Utility_Ch_DeepOnesHumanAppearance);
            aiChallenges_DeepOne.Add(challenge1);

            AIChallenge challenge2 = new AIChallenge(typeof(Ch_ConcealDeepOnes), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true);
            challenge2.delegates_Valid.Add(delegate_Valid_Ch_ConcealDeepOnes);
            challenge2.delegates_Utility.Add(delegate_Utility_Ch_ConcealDeepOnes);
            aiChallenges_DeepOne.Add(challenge2);
        }

        private bool delegate_Valid_Rt_DeepOneReproduce(Challenge challenge, Location location)
        {
            SettlementHuman settlementHuman = location.settlement as SettlementHuman;

            if ((location.soc as Society)?.isOphanimControlled ?? false)
            {
                return false;
            }

            if (settlementHuman != null)
            {
                if (settlementHuman.ophanimTakeOver == true)
                {
                    return false;
                }

                Pr_DeepOneCult cult = location.properties.OfType<Pr_DeepOneCult>().FirstOrDefault();
                if (cult == null)
                {
                    return true;
                }
            }

            return false;
        }

        private double delegate_Utility_Rt_DeepOneReproduce(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }

        private bool delegate_Valid_Ch_DeepOnesHumanAppearance(Challenge challenge, Location location)
        {
            Pr_DeepOneCult cult = (challenge as Ch_DeepOnesHumanAppearance)?.deepOnes;
            if (cult?.menace > 25.0)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_Ch_DeepOnesHumanAppearance(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_DeepOneCult cult = (challenge as Ch_DeepOnesHumanAppearance)?.deepOnes;
            if (cult?.menace > 25.0)
            {
                double val = (cult.menace) * 5;
                reasonMsgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_Valid_Ch_ConcealDeepOnes(Challenge challenge, Location location)
        {
            Pr_DeepOneCult cult = (challenge as Ch_ConcealDeepOnes)?.deepOnes;
            if (cult?.profile > 25.0)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_Ch_ConcealDeepOnes(Challenge challenge, UA ua, Location location,  double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_DeepOneCult cult = (challenge as Ch_ConcealDeepOnes)?.deepOnes;
            if (cult?.profile > 25.0)
            {
                double val = (cult.profile) * 5;
                reasonMsgs?.Add(new ReasonMsg("Potential Profile Reduction", val));
                utility += val;
            }

            return utility;
        }

        private void populateGhast()
        {
            AIChallenge challenge = new AIChallenge(typeof(Rt_GhastEnshadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocal });
            challenge.delegates_Valid.Add(delegate_Valid_Rt_GhastEnshadow);
            challenge.delegates_Utility.Add(delegate_Utility_Rt_GhastEnshadow);
            aiChallenges_Ghast.Add(challenge);
        }

        private bool delegate_Valid_Rt_GhastEnshadow(Challenge challenge, Location location)
        {
            SettlementHuman settlementHuman = location.settlement as SettlementHuman;
            if (settlementHuman != null)
            {
                if (location.getShadow() >= 1.0)
                {
                    return false;
                }

                if (settlementHuman.shadowPolicy == Settlement.shadowResponse.DENY)
                {
                    return false;
                }

                if (settlementHuman.ophanimTakeOver)
                {
                    return false;
                }

                Pr_Opha_Faith faith = location.properties.OfType<Pr_Opha_Faith>().FirstOrDefault();
                if (faith != null)
                {
                    return false;
                }

                Society society = location.soc as Society;
                if (society != null && (society.isAlliance && challenge.map.opt_allianceState == 1))
                {
                    return false;
                }

                Pr_Ward ward = location.properties.OfType<Pr_Ward>().FirstOrDefault();
                if (ward?.charge >= 66)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private double delegate_Utility_Rt_GhastEnshadow(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }

        private void populateOrcUpstart()
        {
            AIChallenge challenge = new AIChallenge(typeof(Ch_OrcRaiding), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety });
            challenge.delegates_Utility.Add(delegate_Utility_Ch_OrcRaiding);
            aiChallenges_OrcUpstart.Add(challenge);

            AIChallenge challenge1 = new AIChallenge(typeof(Ch_RecruitMinion), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.RecruitsMinion });
            challenge1.delegates_Valid.Add(delegate_Valid_Ch_RecruitMinion);
            aiChallenges_OrcUpstart.Add(challenge1);

            aiChallenges_OrcUpstart.Add(new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest }));
        }

        private double delegate_Utility_Ch_OrcRaiding(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            double potentialDevastation = 0.0;
            int neighbourCount = 0;
            foreach (Location loc in location.getNeighbours())
            {
                if (loc.settlement is SettlementHuman && (ua.society == null || ua.society != loc.soc))
                {
                    Pr_Devastation devastation = loc.properties.OfType<Pr_Devastation>().FirstOrDefault();
                    double charge = devastation?.charge ?? 0.0;
                    potentialDevastation += Math.Max(250 - charge, 0.0) / 5;
                    if (potentialDevastation > 0)
                    {
                        neighbourCount++;
                    }
                }
            }

            if (neighbourCount > 0)
            {
                double val = potentialDevastation / neighbourCount;
                reasonMsgs?.Add(new ReasonMsg("Potential Devastation", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_Valid_Ch_RecruitMinion(Challenge challenge, Location location)
        {
            if (map.worldPanic < map.param.panic_forFundHeroes)
            {
                return false;
            }

            return true;
        }

        private void populateVampire()
        {
            AIChallenge challenge = new AIChallenge(typeof(Rt_Feed), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocalRandomized });
            challenge.delegates_ValidFor.Add(delegate_ValidFor_Rt_Feed);
            challenge.delegates_Utility.Add(delegate_Utility_Rt_Feed);
            aiChallenges_Vampire.Add(challenge);

            AIChallenge challenge1 = new AIChallenge(typeof(Mg_DeathsShadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocalRandomized });
            challenge1.delegates_ValidFor.Add(delegate_ValidFor_Mg_DeathsShadow);
            challenge1.delegates_Utility.Add(delegate_Utility_Mg_DeathsShadow);
            aiChallenges_Vampire.Add(challenge1);

            AIChallenge Challenge2 = new AIChallenge(typeof(Ch_Desecrate), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresInfiltrated, AIChallenge.ChallengeTags.PreferLocalRandomized });
            Challenge2.delegates_Utility.Add(delegate_Utility_Ch_Desecrate);
            aiChallenges_Vampire.Add(Challenge2);

            aiChallenges_Vampire.Add(new AIChallenge(typeof(Ch_Enshadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Enshadows, AIChallenge.ChallengeTags.RequiresInfiltrated, AIChallenge.ChallengeTags.PreferLocalRandomized }));
            aiChallenges_Vampire.Add(new AIChallenge(typeof(Ch_WellOfShadows), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.PushesShadow, AIChallenge.ChallengeTags.RequiresSociety, AIChallenge.ChallengeTags.PreferLocalRandomized }));
            aiChallenges_Vampire.Add(new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.PreferLocalRandomized }));
        }

        private bool delegate_ValidFor_Rt_Feed(Challenge challenge, UA ua, Location location)
        {
            UAEN_Vampire vampire = ua as UAEN_Vampire;
            
            if (vampire == null || vampire.call.strength <= 50.0)
            {
                return false;
            }

            SettlementHuman settlementHuman = location.settlement as SettlementHuman;
            if (settlementHuman != null)
            {
                if (location == vampire.location)
                {
                    if (settlementHuman.shadow < 0.9 || vampire.call.strength >= 150.0)
                    {
                        return true;
                    }
                }
                else if (settlementHuman.shadow < 0.7 && vampire.call.strength > 100.0)
                {
                    return true;
                }
            }

            return false;
        }

        private double delegate_Utility_Rt_Feed(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            UAEN_Vampire vampire = ua as UAEN_Vampire;

            if (vampire != null)
            {
                double val = vampire.call.strength;
                reasonMsgs?.Add(new ReasonMsg("Strength of the Hunger", val));
                utility += val;

                val = -((1.0 - vampire.person.shadow) * vampire.map.param.mg_theHungerNonShadowResistance);
                reasonMsgs?.Add(new ReasonMsg("Light in Soul", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_ValidFor_Mg_DeathsShadow(Challenge challenge, UA ua, Location location)
        {
            foreach (Minion minion in ua.minions)
            {
                if (minion == null)
                {
                    return false;
                }
            }

            if (ua.getDeathMastery() < map.param.mg_deathsShadowDeadMasteryReq)
            {
                return false;
            }

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death == null || death.charge < map.param.mg_deathsShadowDeathModifierReq)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_Mg_DeathsShadow(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val;

            Pr_Death death = location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death != null && death.charge >= map.param.mg_deathsShadowDeathModifierReq)
            {
                val = death.charge;
                val *= Math.Min(5.0, Math.Max(0.5, 5 / (2 * map.getStepDist(ua.location, location) + 1)));

                reasonMsgs?.Add(new ReasonMsg("Nearby Death", val));
                utility += val;
            }
            else
            { 
                reasonMsgs?.Add(new ReasonMsg("Requires " + map.param.mg_deathsShadowDeathModifierReq + " Death", -1000.0));
                return -1000;
            }

            return utility;
        }

        private double delegate_Utility_Ch_Desecrate(Challenge challenge, UA ua, Location location, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 150.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }
    }
}
