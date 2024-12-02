using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public  class ConditionalUAENOverrideAI
    {
        private Map map;

        public ConditionalUAENOverrideAI(Map map)
        {
            this.map = map;

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
            {
                populateCordycepsDrone(intDataCord);
                populateCoryceptsHaematophage(intDataCord);
            }

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                populateCCCToad(intDataCCC);
                populateCCCPigion(intDataCCC);
            }
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

                AIChallenge aiChallenge = new AIChallenge(typeof(Rt_SlowHealing), 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseUtility });
                aiChallenge.delegates_ValidFor.Add(delegate_ValidFor_SlowHealing);
                aiChallenges_Cordyceps_Haematophage.Add(aiChallenge);

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

        private void populateCCCToad(ModIntegrationData intData)
        {
            if (intData.typeDict.TryGetValue("UAEN_Toad", out Type toadType) && toadType != null && intData.typeDict.TryGetValue("Rt_Ribbit", out Type ribbitType) && ribbitType != null)
            {
                AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true);
                controlParams.respectDanger = false;
                controlParams.respectArmyIntercept = false;
                controlParams.includeDangerousFoe = false;

                AIChallenge challenge = new AIChallenge(ribbitType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.BaseValid, AIChallenge.ChallengeTags.BaseValidFor, AIChallenge.ChallengeTags.RequireLocal });
                challenge.delegates_Utility.Add(delegate_Utility_Ribbit);

                ModCore.Get().GetAgentAI().RegisterAgentType(toadType, controlParams);
                ModCore.Get().GetAgentAI().AddChallengeToAgentType(toadType, challenge);
            }
        }

        private double delegate_Utility_Ribbit(AgentAI.ChallengeData challengeData, UA ua, double utility, List<ReasonMsg> reasonMsgs)
        {
            utility += 100.0;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));
            return utility;
        }

        private void populateCCCPigion(ModIntegrationData intData)
        {
            if (intData.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
            {
                AgentAI.ControlParameters controlParams = new AgentAI.ControlParameters(true);
                controlParams.respectDanger = false;
                controlParams.respectArmyIntercept = false;
                controlParams.includeDangerousFoe = false;

                List<AITask> tasks = new List<AITask> {
                    new AITask(typeof(Task_GoToUnit), "Deliver Item", map, delegate_Instantiate_DeliverItems, AITask.TargetCategory.Unit, null, new Color(0.5f, 0.5f, 0.5f, 1.0f)),
                    new AITask(typeof(Task_GoToUnit), "Fly Home", map, delegate_Instantiate_PigeonFlyHome, AITask.TargetCategory.Unit, null, new Color(0.5f, 0.5f, 0.5f, 1.0f))
                };

                tasks[0].delegates_Valid.Add(delegate_Validity_DeliverItems);
                tasks[0].delegates_Utility.Add(delegate_Utility_DeliverItems);
                tasks[1].delegates_Valid.Add(delegate_Validity_PigeonFlyHome);
                tasks[1].delegates_Utility.Add(delegate_Utility_PigeonFlyHome);

                ModCore.Get().GetAgentAI().RegisterAgentType(pigeonType, controlParams);
                ModCore.Get().GetAgentAI().AddTasksToAgentType(pigeonType, tasks);

                if (intData.typeDict.TryGetValue("Rt_flyingPigeon", out Type debugType) && debugType != null)
                {
                    ModCore.Get().GetAgentAI().AddChallengeToAgentType(pigeonType, new AIChallenge(debugType, 0.0, new List<AIChallenge.ChallengeTags> { AIChallenge.ChallengeTags.Forbidden }));
                }
            }
        }

        private Assets.Code.Task delegate_Instantiate_DeliverItems(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null && ua.GetType() == pigeonType)
                {
                    if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.returning", out FieldInfo FI_Returning) && FI_Returning != null && !(bool)FI_Returning.GetValue(ua))
                    {
                        UA target = taskData.targetUnit as UA;
                        if (target != null && ((ModCore.Get().checkIsUnitSubsumed(target) && !target.person.unit.isDead) || !target.isDead))
                        {
                            Task_GoToUnit follow = new Task_GoToUnit(ua, target.person.unit, -1, 1);
                            follow.reasonsMessages.Add(new ReasonMsg("Delivering Items", 100.0));
                            return follow;
                        }
                    }
                }
            }

            return null;
        }

        private bool delegate_Validity_DeliverItems(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
                {
                    if (ua.GetType() == pigeonType)
                    {
                        if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.returning", out FieldInfo FI_Returning) && FI_Returning != null)
                        {
                            bool returning = (bool)FI_Returning.GetValue(ua);
                            if (returning)
                            {
                                return false;
                            }

                            if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.target", out FieldInfo FI_Target) && FI_Target != null)
                            {
                                UA target = (UA)FI_Target.GetValue(ua);

                                if (target == null || target.isDead)
                                {
                                    FI_Returning.SetValue(ua, true);
                                    return false;
                                }

                                if (targetCategory == AITask.TargetCategory.Unit && taskData.targetUnit != target)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_DeliverItems(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 100;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }

        private Assets.Code.Task delegate_Instantiate_PigeonFlyHome(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null && ua.GetType() == pigeonType)
                {
                    if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.returning", out FieldInfo FI_Returning) && FI_Returning != null && (bool)FI_Returning.GetValue(ua))
                    {
                        UA owner = taskData.targetUnit as UA;
                        if (owner != null && ((ModCore.Get().checkIsUnitSubsumed(owner) && !owner.person.unit.isDead) || !owner.isDead))
                        {
                            Task_GoToUnit follow = new Task_GoToUnit(ua, owner.person.unit, -1, 1);
                            follow.reasonsMessages.Add(new ReasonMsg("Delivering Items", 100.0));
                            return follow;
                        }
                    }
                }
            }

            return null;
        }

        private bool delegate_Validity_PigeonFlyHome(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
                {
                    if (ua.GetType() == pigeonType)
                    {
                        if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.returning", out FieldInfo FI_Returning) && FI_Returning != null)
                        {
                            bool returning = (bool)FI_Returning.GetValue(ua);
                            if (!returning)
                            {
                                return false;
                            }

                            if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.owner", out FieldInfo FI_Owner) && FI_Owner != null)
                            {
                                UA owner = (UA)FI_Owner.GetValue(ua);

                                if (owner == null || owner.isDead)
                                {
                                    return false;
                                }

                                if (targetCategory == AITask.TargetCategory.Unit && taskData.targetUnit != owner)
                                {
                                    return false;
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private double delegate_Utility_PigeonFlyHome(UA ua, AITask.TargetCategory targetCategory, AgentAI.TaskData taskData, List<ReasonMsg> reasonMsgs)
        {
            double utility = 100;
            reasonMsgs?.Add(new ReasonMsg("Base", utility));

            return utility;
        }
    }
}
