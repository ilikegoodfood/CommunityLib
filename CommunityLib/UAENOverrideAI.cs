using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        private List<AITask> aiTasks_CaveSpider;

        private List<AIChallenge> aiChallenges_DeepOne;

        private List<AIChallenge> aiChallenges_Ghast;

        private List<AIChallenge> aiChallenges_OrcUpstart;

        private List<AIChallenge> aiChallenges_Vampire;

        private Map map;

        public UAENOverrideAI(Map map)
        {
            this.map = map;

            populateCaveSpider();
            populateDeepOne();
            populateGhast();
            populateOrcUpstart();
            populateVampire();

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                populateCordycepsDrone(intDataCord);
                populateCoryceptsHaematophage(intDataCord);
            }

            // Test Articles
        }

        private void populateCaveSpider()
        {
            aiTasks_CaveSpider = new List<AITask>
            {
                new AITask(taskType: typeof(Task_GoToLocation), title: "Wander", map: map, delegate_Instantiate: delegate_Instantiate_GoToLocation, targetCategory: AITask.TargetCategory.Location, foregroundSprite: map.world.textureStore.evil_caveSpider)
            };

            aiTasks_CaveSpider[0].delegates_Valid.Add(delegate_Validity_Wander);
            aiTasks_CaveSpider[0].delegates_Utility.Add(delegate_Utility_Wander);

            AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true);
            controlParams.respectDanger = false;
            controlParams.respectArmyIntercept = false;
            controlParams.includeDangerousFoe = false;
            

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_CaveSpider), controlParams);
            ModCore.Get().GetAgentAI().AddTasksToAgentType(typeof(UAEN_CaveSpider), aiTasks_CaveSpider);
        }

        private Task delegate_Instantiate_GoToLocation(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return new Task_GoToLocation(taskData.targetLocation);
        }

        private bool delegate_Validity_Wander(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (taskData.targetLocation.hex.z == 1 && taskData.targetLocation.getNeighbours().Contains(ua.location))
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_Wander(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 0.0;

            double val = 25.0;
            if (taskData.targetLocation.units.Any(u => u is UA && !(u is UAEN_CaveSpider)))
            {
                reasonMsgs?.Add(new ReasonMsg("Potential prey", val));
                utility += val;
                return utility;
            }

            if (!taskData.targetLocation.isOcean && taskData.targetLocation.settlement == null)
            {
                val = 20.0;
                reasonMsgs?.Add(new ReasonMsg("Base", val));
                utility += val;
                return utility;
            }

            return utility;
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
            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_DeepOne), new AgentAI.ControlParameters(true));
            //Console.WriteLine("CommunityLibrary: Adding challenges to agent");
            ModCore.Get().GetAgentAI().AddChallengesToAgentType(typeof(UAEN_DeepOne), aiChallenges_DeepOne);

            AITask task = new AITask(taskType: typeof(Task_ReturnToTheDeep), title: "Return to the Deep", map: map, delegate_Instantiate: delegate_Instantiate_ReturnDeep, targetCategory: AITask.TargetCategory.Location, foregroundSprite: map.world.iconStore.hideInAbyss, colour: new Color(0.2f, 0.2f, 0.7f));
            task.delegates_Valid.Add(delegate_Validity_ReturnDeep);
            task.delegates_Utility.Add(delegate_Utility_ReturnDeep);

            ModCore.Get().GetAgentAI().AddTaskToAgentType(typeof(UAEN_DeepOne), task);
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

        private Task delegate_Instantiate_ReturnDeep(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return new Task_ReturnToTheDeep(taskData.targetLocation);
        }

        private bool delegate_Validity_ReturnDeep(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (targetCategory == AITask.TargetCategory.None)
            {
                if (ua.moveType == Unit.MoveType.NORMAL)
                {
                    return true;
                }
            }
            else if (targetCategory == AITask.TargetCategory.Location && taskData.targetLocation != null)
            {
                if (taskData.targetLocation.isOcean)
                {
                    return true;
                }
            }

            return false;
        }

        private double delegate_Utility_ReturnDeep(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 10000;
            reasonMsgs?.Add(new ReasonMsg("Must Return to the Deep", utility));

            double val = -10 * ua.map.getStepDist(ua.location, taskData.targetLocation);
            reasonMsgs?.Add(new ReasonMsg("Distance", val));
            utility += val;

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

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_Ghast), new AgentAI.ControlParameters(true));
            ModCore.Get().GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Ghast), aiChallenges_Ghast);
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

                if (challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Opha_Faith) != null)
                {
                    return false;
                }

                Society society = challengeData.location.soc as Society;
                if (society != null && (society.isAlliance && challengeData.challenge.map.opt_allianceState == 1))
                {
                    return false;
                }

                Pr_Ward ward = (Pr_Ward)challengeData.location.properties.FirstOrDefault(pr => pr is Pr_Ward);
                if (ward != null && ward.charge >= 66)
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
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest }),
                new AIChallenge(typeof(Rti_Orc_CeaseWar), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferLocal } )
            };

            aiChallenges_OrcUpstart[0].delegates_Utility.Add(delegate_Utility_Ch_OrcRaiding);

            aiChallenges_OrcUpstart[1].delegates_Valid.Add(delegate_Valid_Ch_RecruitMinion);

            aiChallenges_OrcUpstart[2].delegates_Utility.Add(delegate_Utility_Ch_Rest_InOrcCamp);

            aiChallenges_OrcUpstart[3].delegates_Utility.Add(delegate_Utility_Rti_Orc_CeaseWar);

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_OrcUpstart), new AgentAI.ControlParameters(true));
            ModCore.Get().GetAgentAI().AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_OrcUpstart);
        }

        private double delegate_Utility_Ch_OrcRaiding(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            int gold = 0;
            foreach (Location loc in challengeData.location.getNeighbours())
            {
                if (loc.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
                {
                    if (settlementHuman.ruler.gold > gold)
                    {
                        gold = settlementHuman.ruler.gold;
                    }
                }
            }

            if (gold > 0)
            {
                double val = gold * map.param.ch_orcRaidingGoldGain;
                reasonMsgs?.Add(new ReasonMsg("Potential Gold Gain", val));
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

        private double delegate_Utility_Ch_Rest_InOrcCamp(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            utility -= map.param.ch_rest_parameterValue1;
            utility += 1.0;

            if (reasonMsgs != null)
            {
                ReasonMsg msg = reasonMsgs.FirstOrDefault(m => m.msg == "Base");

                if (msg != null)
                {
                    msg.value = 1.0;
                }
            }

            return utility;
        }

        private double delegate_Utility_Rti_Orc_CeaseWar(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            if (challengeData.challenge is Rti_Orc_CeaseWar ceaseWar)
            {
                SG_Orc orcs = ceaseWar.caster.orcs;

                int warCount = 0;
                int offensiveWarCount = 0;

                double enemyMight = 0.0;
                double offensiveEnemyMight = 0.0;
                double orcMight = orcs.currentMilitary;

                foreach (DipRel rel in orcs.getAllRelations())
                {
                    SocialGroup other = rel.other(orcs);
                    if (other.isGone())
                    {
                        continue;
                    }

                    if (rel.state == DipRel.dipState.war)
                    {
                        enemyMight += other.currentMilitary;
                        warCount++;

                        if (rel.war.att == orcs)
                        {
                            offensiveEnemyMight += other.currentMilitary;
                            offensiveWarCount++;
                        }
                    }
                }

                double val = 0.0;
                if (offensiveWarCount == 0 || offensiveEnemyMight == 0.0)
                {
                    val = -100.0;
                    reasonMsgs?.Add(new ReasonMsg("Would not effect outcome", val));
                    utility += val;
                    return utility;
                }

                if (orcMight - 10 >= enemyMight)
                {
                    val = enemyMight - orcMight;
                    reasonMsgs?.Add(new ReasonMsg("Superior military", val));
                    utility += val;
                }
                else if (orcMight - 10 < enemyMight)
                {
                    if (offensiveWarCount >= warCount || offensiveEnemyMight >= enemyMight)
                    {
                        val = 2 * (enemyMight - (orcMight - 10));
                        reasonMsgs?.Add(new ReasonMsg("Inferior military", val));
                        utility += val;
                    }
                    else
                    {
                        val = enemyMight - (orcMight - 10);
                        reasonMsgs?.Add(new ReasonMsg("Inferior military", val));
                        utility += val;

                        if (offensiveEnemyMight > 0.0)
                        {
                            val = offensiveEnemyMight;
                            reasonMsgs?.Add(new ReasonMsg("Removes " + offensiveEnemyMight + " combatants from war", val));
                        }
                    }
                }
            }

            return utility;
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
                new AIChallenge(typeof(Ch_Rest), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized }),
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.BaseUtility, AIChallenge.ChallengeTags.PreferLocalRandomized })
            };

            aiChallenges_Vampire[0].delegates_ValidFor.Add(delegate_ValidFor_Rt_Feed);
            aiChallenges_Vampire[0].delegates_Utility.Add(delegate_Utility_Rt_Feed);

            aiChallenges_Vampire[1].delegates_ValidFor.Add(delegate_ValidFor_Mg_DeathsShadow);
            aiChallenges_Vampire[1].delegates_Utility.Add(delegate_Utility_Mg_DeathsShadow);

            aiChallenges_Vampire[2].delegates_Utility.Add(delegate_Utility_Ch_Desecrate);

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_Vampire), new AgentAI.ControlParameters(true));
            ModCore.Get().GetAgentAI().AddChallengesToAgentType(typeof(UAEN_Vampire), aiChallenges_Vampire);
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

        private void populateCordycepsDrone(ModIntegrationData intData)
        {
            //Console.WriteLine("CommunityLib: populating Drone AI");
            if (intData.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
            {
                List<AITask> aiTasks_Cordyceps_Drone = new List<AITask>();

                if (intData.typeDict.TryGetValue("SeekTask", out Type seekType) && seekType != null)
                {
                    //Console.WriteLine("CommunityLib: adding Task_SeekPrey");
                    AITask aiTask = new AITask(taskType: seekType, title: "Seek Prey", map: map, delegate_Instantiate: AITask.delegate_Instantiate_NOARGS, foregroundSprite: map.world.textureStore.agent_insect_lateStage, colour: new Color(1f, 0f, 0f));
                    aiTask.delegates_Valid.Add(delegate_Validity_SeekPrey);
                    aiTask.delegates_Utility.Add(delegate_Utility_SeekPrey);

                    aiTasks_Cordyceps_Drone.Add(aiTask);
                }

                if (intData.typeDict.TryGetValue("ExploreTask", out Type exploreType) && exploreType != null)
                {
                    //Console.WriteLine("CommunityLib: Adding Task_Explore");
                    AITask aiTask = new AITask(taskType: exploreType, title: "Explore", map: map, delegate_Instantiate: AITask.delegate_Instantiate_NOARGS, foregroundSprite: map.world.iconStore.ophanimSwiftOfFoot);
                    aiTask.delegates_Valid.Add(delegate_Validity_ExploreDrone);
                    aiTask.delegates_Utility.Add(delegate_Utility_ExploreDrone);

                    aiTasks_Cordyceps_Drone.Add(aiTask);
                }

                if (intData.typeDict.TryGetValue("GoHomeTask", out Type homeType) && homeType != null)
                {
                    //Console.WriteLine("CommunityLib: Adding Task_GoHome");
                    AITask aiTask = new AITask(taskType: homeType, title: "Return to Hive", map: map, delegate_Instantiate: AITask.delegate_Instantiate_NOARGS, foregroundSprite: map.world.iconStore.ophanimSwiftOfFoot);
                    aiTask.delegates_Valid.Add(delegate_Validity_GoHome);
                    aiTask.delegates_Utility.Add(delegate_Utility_GoHome);

                    aiTasks_Cordyceps_Drone.Add(aiTask);
                }

                ModCore.Get().GetAgentAI().RegisterAgentType(droneType, new AgentAI.ControlParameters(true));
                ModCore.Get().GetAgentAI().AddTasksToAgentType(droneType, aiTasks_Cordyceps_Drone);
            }
        }

        private bool delegate_Validity_SeekPrey(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null)
                {
                    if (ua.location.settlement != null && (ua.location.settlement.GetType() == hiveType || ua.location.settlement.GetType().IsSubclassOf(hiveType)))
                    {
                        if (intDataCord.typeDict.TryGetValue("God", out Type godType) && godType != null && (ua.map.overmind.god.GetType() == godType || ua.map.overmind.god.GetType().IsSubclassOf(godType)))
                        {
                            if (intDataCord.fieldInfoDict.TryGetValue("God.phHome", out FieldInfo phHomeInfo) && phHomeInfo != null)
                            {
                                Property[] phHome = (Property[])phHomeInfo.GetValue(ua.map.overmind.god);
                                if (phHome[ua.locIndex].charge > 1.0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_SeekPrey(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = ModCore.Get().tryGetRand(ua, taskData, "baseRand", Eleven.random.Next(100));
            reasonMsgs?.Add(new ReasonMsg("Base (Randomized)", utility));

            if (ModCore.Get().GetAgentAI().isAIRunning())
            {
                ModCore.Get().setRand(ua, taskData, "baseRand", Eleven.random.Next(100));
            }

            return utility;
        }

        private bool delegate_Validity_ExploreDrone(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null && intDataCord.typeDict.TryGetValue("God", out Type godType) && godType != null)
                {
                    if (ua.location.settlement != null && (ua.location.settlement.GetType() == hiveType || ua.location.settlement.GetType().IsSubclassOf(hiveType)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_ExploreDrone(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 20;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        private bool delegate_Validity_GoHome(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null)
                {
                    if (ua.location.settlement == null || (ua.location.settlement.GetType() != hiveType && !ua.location.settlement.GetType().IsSubclassOf(hiveType)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_GoHome(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 100;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        private void populateCoryceptsHaematophage(ModIntegrationData intData)
        {
            if (intData.typeDict.TryGetValue("Haematophage", out Type haematophageType) && haematophageType != null)
            {
                List<AIChallenge> aiChallenges_Cordyceps_Haematophage = new List<AIChallenge>();

                if (intData.typeDict.TryGetValue("SlowHealTask", out Type slowHealType) && slowHealType != null)
                {
                    AIChallenge aiChallenge = new AIChallenge(slowHealType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseUtility });
                    aiChallenge.delegates_ValidFor.Add(delegate_ValidFor_SlowHealing);
                    aiChallenges_Cordyceps_Haematophage.Add(aiChallenge);
                }

                AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true);
                controlParams.respectDanger = false;
                controlParams.respectArmyIntercept = false;
                controlParams.includeDangerousFoe = false;

                ModCore.Get().GetAgentAI().RegisterAgentType(haematophageType, controlParams);
                ModCore.Get().GetAgentAI().AddChallengesToAgentType(haematophageType, aiChallenges_Cordyceps_Haematophage);
            }
        }

        private bool delegate_ValidFor_SlowHealing(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (challengeData.location.index == ua.homeLocation && ua.hp < ua.maxHp)
            {
                return true;
            }

            return false;
        }
    }
}
