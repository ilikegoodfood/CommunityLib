using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        private Map map;

        public UAENOverrideAI(Map map)
        {
            this.map = map;

            populateCaveSpider();
            populateDeepOne();
            populateGhast();
            populateOrcUpstart();
            populateVampire();
        }

        private void populateCaveSpider()
        {
            List<AITask> aiTasks_CaveSpider = new List<AITask>
            {
                new AITask(taskType: typeof(Task_GoToLocation), title: "Stalk the Caverns", map: map, delegate_Instantiate: delegate_Instantiate_GoToLocation, targetCategory: AITask.TargetCategory.Location, foregroundSprite: map.world.textureStore.hex_terrain_underground[0]),
                new AITask(taskType: typeof(Task_GoToWilderness), title: "Retreat to the Wilds", map: map, delegate_Instantiate: delegate_Instantiate_GoToWilderness, foregroundSprite: map.world.textureStore.hex_terrain_underground[0])
            };

            aiTasks_CaveSpider[0].delegates_Valid.Add(delegate_Validity_Wander);
            aiTasks_CaveSpider[0].delegates_Utility.Add(delegate_Utility_Wander);

            aiTasks_CaveSpider[1].delegates_Valid.Add(delegate_Validity_GoToWilderness);
            aiTasks_CaveSpider[1].delegates_Utility.Add(delegate_Utility_GoToWilderness);

            AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true);
            controlParams.respectDanger = false;
            controlParams.respectArmyIntercept = false;
            controlParams.includeDangerousFoe = false;
            controlParams.canAttack = true;

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_CaveSpider), controlParams);
            ModCore.Get().GetAgentAI().AddTasksToAgentType(typeof(UAEN_CaveSpider), aiTasks_CaveSpider);
        }

        private Task delegate_Instantiate_GoToLocation(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return new Task_GoToLocation(taskData.targetLocation);
        }

        private bool delegate_Validity_Wander(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (taskData.targetCategory == AITask.TargetCategory.None)
            {
                return ua.location.getNeighbours().Any(n => n.hex.z == 1 && (n.units.Any(u => u is UA && !(u is UAEN_CaveSpider)) || !n.isOcean && (n.settlement == null || n.settlement is Set_CityRuins || (n.settlement is Set_MinorOther && (n.settlement.subs.Any(sub => sub is Sub_AncientRuins) || n.settlement.subs.Any(sub => sub is Sub_Temple temple && temple.order is HolyOrder_Witches))) || ModCore.Get().checkIsNaturalWonder(n))));
            }

            if (taskData.targetLocation.hex.z == 1 && taskData.targetLocation.getNeighbours().Contains(ua.location) && (taskData.targetLocation.units.Any(u => u is UA && !(u is UAEN_CaveSpider)) || (!taskData.targetLocation.isOcean && (taskData.targetLocation.settlement == null || taskData.targetLocation.settlement is Set_CityRuins || (taskData.targetLocation.settlement is Set_MinorOther && (taskData.targetLocation.settlement.subs.Any(sub => sub is Sub_AncientRuins) || taskData.targetLocation.settlement.subs.Any(sub => sub is Sub_Temple temple && temple.order is HolyOrder_Witches))) || ModCore.Get().checkIsNaturalWonder(taskData.targetLocation)))))
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

        private Task delegate_Instantiate_GoToWilderness(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            return new Task_GoToWilderness(true, 1);
        }

        private bool delegate_Validity_GoToWilderness(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (targetCategory == AITask.TargetCategory.None)
            {
                return (ua.location.isOcean || ua.location.settlement != null) && !ua.location.getNeighbours().Any(n => n.hex.z == 1 && (n.units.Any(u => u is UA && !(u is UAEN_CaveSpider)) || !n.isOcean && n.settlement == null)); ;
            }

            return false;
        }

        private double delegate_Utility_GoToWilderness(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        private void populateDeepOne()
        {
            //Console.WriteLine("CommunityLibrary: Populating DeepOne Override AI");
            List<AIChallenge> aiChallenges_DeepOne = new List<AIChallenge>
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

            AITask task = new AITask(taskType: typeof(Task_ReturnToTheDeep), title: "Return to the Deep", map: map, delegate_Instantiate: delegate_Instantiate_ReturnDeep, foregroundSprite: map.world.iconStore.hideInAbyss, colour: new Color(0.2f, 0.2f, 0.7f));
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

                if (ModCore.Get().data.tryGetModIntegrationData("AberrantMetal", out ModIntegrationData intDataMetal))
                {
                    if (intDataMetal.typeDict.TryGetValue("Factory", out Type factoryType))
                    {
                        if (settlementHuman.GetType() == factoryType || settlementHuman.GetType().IsSubclassOf(factoryType))
                        {
                            return false;
                        }
                    }
                }

                if (!challengeData.location.properties.Any(pr => pr is Pr_DeepOneCult))
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
            return new Task_ReturnToTheDeep(-1);
        }

        private bool delegate_Validity_ReturnDeep(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (targetCategory == AITask.TargetCategory.None && ua.moveType == Unit.MoveType.NORMAL)
            {
                return true;
            }

            return false;
        }

        private double delegate_Utility_ReturnDeep(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 10000;
            reasonMsgs?.Add(new ReasonMsg("Must Return to the Deep", utility));

            return utility;
        }

        private void populateGhast()
        {
            List<AIChallenge> aiChallenges_Ghast = new List<AIChallenge>
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
                if (ward != null && ward.charge > 0.9)
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
            List<AIChallenge> aiChallenges_OrcUpstart = new List<AIChallenge>
            {
                new AIChallenge(typeof(Ch_OrcRaiding), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequiresOwnSociety }),
                new AIChallenge(typeof(Ch_RecruitMinion), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.RecruitsMinion }),
                new AIChallenge(typeof(Ch_Rest_InOrcCamp), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.RequiresOwnSociety, AIChallenge.ChallengeTags.HealOrc, AIChallenge.ChallengeTags.Rest }),
                new AIChallenge(typeof(Rti_Orc_CeaseWar), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.PreferLocal } )
            };

            aiChallenges_OrcUpstart[0].delegates_Utility.Add(delegate_Utility_Ch_OrcRaiding);

            aiChallenges_OrcUpstart[1].delegates_Valid.Add(delegate_Valid_Ch_RecruitMinion);

            aiChallenges_OrcUpstart[2].delegates_Utility.Add(delegate_Utility_Ch_Rest_InOrcCamp);

            aiChallenges_OrcUpstart[3].delegates_ValidFor.Add(delegate_ValidFor_Rti_Orcs_CeaseWar);
            aiChallenges_OrcUpstart[3].delegates_Utility.Add(delegate_Utility_Rti_Orc_CeaseWar);

            ModCore.Get().GetAgentAI().RegisterAgentType(typeof(UAEN_OrcUpstart), new AgentAI.ControlParameters(true));
            ModCore.Get().GetAgentAI().AddChallengesToAgentType(typeof(UAEN_OrcUpstart), aiChallenges_OrcUpstart);

            if (ModCore.Get().GetAgentAI().TryGetAgentType(typeof(UAEN_OrcUpstart), out AgentAI.AIData aiData) && aiData != null)
            {
                aiData.aiChallenges_UniversalDelegates_ValidFor.Add(universalDelegate_ValidFor_Underground);
            }
        }

        private bool universalDelegate_ValidFor_Underground(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (challengeData.challenge.canBeSeenAcrossZLevels())
            {
                return true;
            }

            if (challengeData.location.hex.z == 1 && ua.society is SG_Orc orcs && !orcs.canGoUnderground())
            {
                return false;
            }

            return true;
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

        private bool delegate_ValidFor_Rti_Orcs_CeaseWar(AgentAI.ChallengeData challengeData, UA ua)
        {
            if (challengeData.location.soc == null || challengeData.location.soc == ua.society)
            {
                return true;
            }
            return false;
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
                else if (orcMight - 10 >= enemyMight)
                {
                    val = enemyMight - orcMight;
                    reasonMsgs?.Add(new ReasonMsg("Superior military", val));
                    utility += val;
                }
                else if (orcMight - 10 < enemyMight)
                {
                    if (offensiveWarCount >= warCount || offensiveEnemyMight >= enemyMight)
                    {
                        val = 2 * (enemyMight - orcMight);
                        reasonMsgs?.Add(new ReasonMsg("Inferior military", val));
                        utility += val;
                    }
                    else
                    {
                        val = enemyMight - orcMight;
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
            List<AIChallenge> aiChallenges_Vampire = new List<AIChallenge>
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
    }
}
