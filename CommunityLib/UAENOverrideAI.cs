using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        private List<AIChallenge> aiChallenges_DeepOne;

        private List<AIChallenge> aiChallenges_Ghast;

        private List<AIChallenge> aiChallenges_OrcUpstart;

        private List<AIChallenge> aiChallenges_Vampire;

        private Map map;

        public UAENOverrideAI(Map map)
        {
            this.map = map;

            populateDeepOne();
            populateGhast();
            populateOrcUpstart();
            populateVampire();

            // Test Articles
            //populateUAA();
            
        }

        private void populateDeepOne()
        {
            //Console.WriteLine("CommunityLibrary: Populating DeepOne Override AI");
            aiChallenges_DeepOne = new List<AIChallenge>
            {
                new AIChallenge(typeof(Rt_DeepOneReproduce), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true),
                new AIChallenge(typeof(Ch_DeepOnesHumanAppearance), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true),
                new AIChallenge(typeof(Ch_ConcealDeepOnes), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Aquaphibious }, true)
            };

            //Console.WriteLine("CommunityLibrary: Adding delegates to 0...");
            aiChallenges_DeepOne[0].delegates_Valid.Add(delegate_Valid_Rt_DeepOneReproduce);
            aiChallenges_DeepOne[0].delegates_Utility.Add(delegate_Utility_Rt_DeepOneReproduce);

            //Console.WriteLine("CommunityLibrary: Adding delegates to 1...");
            aiChallenges_DeepOne[1].delegates_Valid.Add(delegate_Valid_Ch_DeepOnesHumanAppearance);
            aiChallenges_DeepOne[1].delegates_Utility.Add(delegate_Utility_Ch_DeepOnesHumanAppearance);

            //Console.WriteLine("CommunityLibrary: Adding delegates to 2...");
            aiChallenges_DeepOne[2].delegates_Valid.Add(delegate_Valid_Ch_ConcealDeepOnes);
            aiChallenges_DeepOne[2].delegates_Utility.Add(delegate_Utility_Ch_ConcealDeepOnes);

            //Console.WriteLine("CommunityLibrary: Registering agent");
            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_DeepOne), AgentAI.ControlParameters.newDefault());
            //Console.WriteLine("CommunityLibrary: Adding challenges to agent");
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_DeepOne), aiChallenges_DeepOne);
        }

        private bool delegate_Valid_Rt_DeepOneReproduce(AgentAI.ChallengeData challengeData)
        {
            SettlementHuman settlementHuman = challengeData.location.settlement as SettlementHuman;

            if ((challengeData.location.soc as Society)?.isOphanimControlled ?? false)
            {
                return false;
            }

            if (settlementHuman != null)
            {
                if (settlementHuman.ophanimTakeOver == true)
                {
                    return false;
                }

                Pr_DeepOneCult cult = challengeData.location.properties.OfType<Pr_DeepOneCult>().FirstOrDefault();
                if (cult == null)
                {
                    return true;
                }
            }

            return false;
        }

        private double delegate_Utility_Rt_DeepOneReproduce(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }

        private bool delegate_Valid_Ch_DeepOnesHumanAppearance(AgentAI.ChallengeData challengeData)
        {
            Pr_DeepOneCult cult = (challengeData.challenge as Ch_DeepOnesHumanAppearance)?.deepOnes;
            if (cult?.menace > 25.0)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_Ch_DeepOnesHumanAppearance(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_DeepOneCult cult = (challengeData.challenge as Ch_DeepOnesHumanAppearance)?.deepOnes;
            if (cult?.menace > 25.0)
            {
                double val = (cult.menace) * 5;
                reasonMsgs?.Add(new ReasonMsg("Potential Menace Reduction", val));
                utility += val;
            }

            return utility;
        }

        private bool delegate_Valid_Ch_ConcealDeepOnes(AgentAI.ChallengeData challengeData)
        {
            Pr_DeepOneCult cult = (challengeData.challenge as Ch_ConcealDeepOnes)?.deepOnes;
            if (cult?.profile > 25.0)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_Ch_ConcealDeepOnes(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            Pr_DeepOneCult cult = (challengeData.challenge as Ch_ConcealDeepOnes)?.deepOnes;
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
            aiChallenges_Ghast = new List<AIChallenge>
            {
                new AIChallenge(typeof(Rt_GhastEnshadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocal })
            };

            aiChallenges_Ghast[0].delegates_Valid.Add(delegate_Valid_Rt_GhastEnshadow);
            aiChallenges_Ghast[0].delegates_Utility.Add(delegate_Utility_Rt_GhastEnshadow);

            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_Ghast), AgentAI.ControlParameters.newDefault());
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Ghast), aiChallenges_Ghast);
        }

        private bool delegate_Valid_Rt_GhastEnshadow(AgentAI.ChallengeData challengeData)
        {
            SettlementHuman settlementHuman = challengeData.location.settlement as SettlementHuman;
            if (settlementHuman != null)
            {
                if (challengeData.location.getShadow() >= 1.0)
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

                Pr_Opha_Faith faith = challengeData.location.properties.OfType<Pr_Opha_Faith>().FirstOrDefault();
                if (faith != null)
                {
                    return false;
                }

                Society society = challengeData.location.soc as Society;
                if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                {
                    return false;
                }

                Pr_Ward ward = challengeData.location.properties.OfType<Pr_Ward>().FirstOrDefault();
                if (ward?.charge >= 66)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private double delegate_Utility_Rt_GhastEnshadow(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }

        private void populateOrcUpstart()
        {
            aiChallenges_OrcUpstart = new List<AIChallenge>
            {
                new AIChallenge(typeof(Ch_OrcRaiding), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                new AIChallenge(typeof(Ch_RecruitMinion), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.RecruitsMinion }),
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest })
            };

            aiChallenges_OrcUpstart[0].delegates_Utility.Add(delegate_Utility_Ch_OrcRaiding);

            aiChallenges_OrcUpstart[1].delegates_Valid.Add(delegate_Valid_Ch_RecruitMinion);

            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_OrcUpstart), AgentAI.ControlParameters.newDefault());
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_OrcUpstart);
        }

        private double delegate_Utility_Ch_OrcRaiding(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double potentialDevastation = 0.0;
            int neighbourCount = 0;
            foreach (Location loc in challengeData.location.getNeighbours())
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

        private bool delegate_Valid_Ch_RecruitMinion(AgentAI.ChallengeData challengeData)
        {
            if (map.worldPanic < map.param.panic_forFundHeroes)
            {
                return false;
            }

            return true;
        }

        private void populateVampire()
        {
            aiChallenges_Vampire = new List<AIChallenge>
            {
                new AIChallenge(typeof(Rt_Feed), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Mg_DeathsShadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Ch_Desecrate), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresInfiltrated, AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Ch_Enshadow), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Enshadows, AIChallenge.ChallengeTags.RequiresInfiltrated, AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Ch_WellOfShadows), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.PushesShadow, AIChallenge.ChallengeTags.RequiresSociety, AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.PreferLocalRandomized })
            };

            aiChallenges_Vampire[0].delegates_ValidFor.Add(delegate_ValidFor_Rt_Feed);
            aiChallenges_Vampire[0].delegates_Utility.Add(delegate_Utility_Rt_Feed);

            aiChallenges_Vampire[1].delegates_ValidFor.Add(delegate_ValidFor_Mg_DeathsShadow);
            aiChallenges_Vampire[1].delegates_Utility.Add(delegate_Utility_Mg_DeathsShadow);

            aiChallenges_Vampire[2].delegates_Utility.Add(delegate_Utility_Ch_Desecrate);

            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAEN_Vampire), AgentAI.ControlParameters.newDefault());
            ModCore.core.GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Vampire), aiChallenges_Vampire);
        }

        private bool delegate_ValidFor_Rt_Feed(AgentAI.ChallengeData challengeData, UA ua)
        {
            UAEN_Vampire vampire = ua as UAEN_Vampire;
            
            if (vampire == null || vampire.call.strength <= 50.0)
            {
                return false;
            }

            SettlementHuman settlementHuman = challengeData.location.settlement as SettlementHuman;
            if (settlementHuman != null)
            {
                if (challengeData.location == vampire.location)
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

        private double delegate_Utility_Rt_Feed(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
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

        private bool delegate_ValidFor_Mg_DeathsShadow(AgentAI.ChallengeData challengeData, UA ua)
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

            Pr_Death death = challengeData.location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death == null || death.charge < map.param.mg_deathsShadowDeathModifierReq)
            {
                return false;
            }

            return true;
        }

        private double delegate_Utility_Mg_DeathsShadow(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val;

            Pr_Death death = challengeData.location.properties.OfType<Pr_Death>().FirstOrDefault();
            if (death != null && death.charge >= map.param.mg_deathsShadowDeathModifierReq)
            {
                val = death.charge;
                val *= Math.Min(5.0, Math.Max(0.5, 5 / (2 * map.getStepDist(ua.location, challengeData.location) + 1)));

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

        private double delegate_Utility_Ch_Desecrate(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            double val = 150.0;
            reasonMsgs?.Add(new ReasonMsg("Base", val));
            utility += val;

            return utility;
        }

        private void populateUAA()
        {
            AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters();
            //Console.WriteLine("CommunityLib: Instantiated Control Params");
            controlParams.considerAllChallenges = true;
            //Console.WriteLine("CommunityLib: Set considerAllChallenges to true");
            controlParams.forceSafeMove = false;
            //Console.WriteLine("CommunityLib: Set forceSafeMove to false");
            controlParams.respectChallengeVisibility = true;
            //Console.WriteLine("CommunityLib: Set respectChallengeVisibility to true");
            controlParams.respectDanger = true;
            //Console.WriteLine("CommunityLib: Set respectDanger to true");
            controlParams.respectChallengeAlignment = true;
            //Console.WriteLine("CommunityLib: Set respectChallengeAlignment to true");
            controlParams.valueTimeCost = true;
            //Console.WriteLine("CommunityLib: Set valueTimeCost to true");
            controlParams.includeDangerousFoe = true;
            //Console.WriteLine("CommunityLib: Set includeDangerousFoe to true");
            controlParams.includeNotHolyTask = true;
            //Console.WriteLine("CommunityLib: Set includeNotHolyTask to true");
            controlParams.pathfindingDeligate = ModCore.core.pathfinding.delegate_SAFE_MOVE;
            //Console.WriteLine("CommunityLib: Set pathfindingDeligate to delegate_SAFE_MOVE");

            ModCore.core.GetAgentAI().RegisterAgentType(typeof(UAA), controlParams);
            //Console.WriteLine("CommunityLib: Registered Agent Type UAA");
        }
    }
}
