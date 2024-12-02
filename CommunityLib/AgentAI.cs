using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Code;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;

namespace CommunityLib
{
    public class AgentAI
    {
        public static DebugProperties debug;

        public static DebugProperties debugInternal;

        private bool aiRunning = false;

        private bool aiCheckingUtility = false;

        private Dictionary<Type, AIData> ai;

        private bool isDuringIntercept;

        public bool IsDuringIntercept => isDuringIntercept;

        public Map map;

        public struct DebugProperties
        {
            public bool debug;

            public bool outputProfile_AllChallenges;
            public bool outputProfile_VisibleChallenges;
            public bool outputProfile_VisibleAgents;

            public bool outputVisibility_AllChallenges;
            public bool outputVisibility_VisibleChallenges;

            public bool outputValidity_AllChallenges;
            public bool outputValidity_ValidChallenges;

            public bool outputUtility_ValidChallenges;

            public bool outputUtility_VisibleAgentsAttack;
            public bool outputUtility_VisibleAgentsBodyguard;
            public bool outputUtility_VisibleAgentsDisrupt;

            public bool outputValidity_AllTasks;
            public bool outputValidity_ValidTasks;
            public bool outputUtility_ValidTasks;

            public bool outputUtility_ChosenAction;

            public void setOff()
            {
                debug = false;
                outputProfile_AllChallenges = false;
                outputProfile_VisibleChallenges = false;
                outputProfile_VisibleAgents = false;
                outputVisibility_AllChallenges = false;
                outputVisibility_VisibleChallenges = false;
                outputValidity_AllChallenges = false;
                outputValidity_ValidChallenges = false;
                outputUtility_ValidChallenges = false;
                outputUtility_VisibleAgentsAttack = false;
                outputUtility_VisibleAgentsBodyguard = false;
                outputUtility_VisibleAgentsDisrupt = false;
                outputValidity_AllTasks = false;
                outputValidity_ValidTasks = false;
                outputUtility_ValidTasks = false;
                outputUtility_ChosenAction = false;
            }

            public void setOn()
            {
                debug = true;
                outputProfile_AllChallenges = true;
                outputProfile_VisibleChallenges = true;
                outputProfile_VisibleAgents = true;
                outputVisibility_AllChallenges = true;
                outputVisibility_VisibleChallenges = true;
                outputValidity_AllChallenges = true;
                outputValidity_ValidChallenges = true;
                outputUtility_ValidChallenges = true;
                outputUtility_VisibleAgentsAttack = true;
                outputUtility_VisibleAgentsBodyguard = true;
                outputUtility_VisibleAgentsDisrupt = true;
                outputValidity_AllTasks = true;
                outputValidity_ValidTasks = true;
                outputUtility_ValidTasks = true;
                outputUtility_ChosenAction = true;
            }

            public DebugProperties(bool isOn)
            {
                if (isOn)
                {
                    debug = true;
                    outputProfile_AllChallenges = true;
                    outputProfile_VisibleChallenges = true;
                    outputProfile_VisibleAgents = true;
                    outputVisibility_AllChallenges = true;
                    outputVisibility_VisibleChallenges = true;
                    outputValidity_AllChallenges = true;
                    outputValidity_ValidChallenges = true;
                    outputUtility_ValidChallenges = true;
                    outputUtility_VisibleAgentsAttack = true;
                    outputUtility_VisibleAgentsBodyguard = true;
                    outputUtility_VisibleAgentsDisrupt = true;
                    outputValidity_AllTasks = true;
                    outputValidity_ValidTasks = true;
                    outputUtility_ValidTasks = true;
                    outputUtility_ChosenAction = true;
                }
                else
                {
                    debug = false;
                    outputProfile_AllChallenges = false;
                    outputProfile_VisibleChallenges = false;
                    outputProfile_VisibleAgents = false;
                    outputVisibility_AllChallenges = false;
                    outputVisibility_VisibleChallenges = false;
                    outputValidity_AllChallenges = false;
                    outputValidity_ValidChallenges = false;
                    outputUtility_ValidChallenges = false;
                    outputUtility_VisibleAgentsAttack = false;
                    outputUtility_VisibleAgentsBodyguard = false;
                    outputUtility_VisibleAgentsDisrupt = false;
                    outputValidity_AllTasks = true;
                    outputValidity_ValidTasks = true;
                    outputUtility_ValidTasks = true;
                    outputUtility_ChosenAction = false;
                }
            }
        }

        public class ControlParameters
        {
            public bool considerAllChallenges;
            public bool considerAllRituals;
            public bool forceSafeMove;

            public bool respectChallengeVisibility;
            public bool respectDanger;
            public bool respectArmyIntercept;
            public bool respectChallengeAlignment;
            public bool respectTags;
            public bool respectTenets;
            public bool respectTraits;
            public bool valueTimeCost;

            public bool includeDangerousFoe;
            public bool includeNotHolyTask;

            public bool hideThoughts;
            public DebugProperties debugProperties;

            public ControlParameters(bool isDark)
            {
                if (isDark)
                {
                    {
                        considerAllChallenges = false;
                        considerAllRituals = true;
                        forceSafeMove = false;

                        respectChallengeVisibility = false;
                        respectDanger = true;
                        respectArmyIntercept = true;
                        respectChallengeAlignment = false;
                        respectTags = true;
                        respectTenets = true;
                        respectTraits = true;
                        valueTimeCost = false;

                        includeDangerousFoe = true;
                        includeNotHolyTask = false;

                        hideThoughts = false;
                        debugProperties = new DebugProperties(false);
                    }
                }
                else
                {
                    {
                        considerAllChallenges = true;
                        considerAllRituals = true;
                        forceSafeMove = false;

                        respectChallengeVisibility = true;
                        respectDanger = true;
                        respectArmyIntercept = true;
                        respectChallengeAlignment = true;
                        respectTags = true;
                        respectTenets = true;
                        respectTraits = true;
                        valueTimeCost = true;

                        includeDangerousFoe = true;
                        includeNotHolyTask = false;

                        hideThoughts = false;
                        debugProperties = new DebugProperties(false);
                    }
                }
            }
        }

        public class AIData
        {
            public Type agentType;
            public List<AIChallenge> aiChallenges;
            public List<Func<ChallengeData, UA, double, double>> aiChallenges_UniversalDelegates_Profile = new List<Func<ChallengeData, UA, double, double>>();
            public List<Func<ChallengeData, bool>> aiChallenges_UniversalDelegates_Valid = new List<Func<ChallengeData, bool>>();
            public List<Func<ChallengeData, UA, bool>> aiChallenges_UniversalDelegates_ValidFor = new List<Func<ChallengeData, UA, bool>>();
            public List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>> aiChallenges_UniversalDelegates_Utility = new List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>>();
            public List<AITask> aiTasks;
            public ControlParameters controlParameters;

            public AIData(Type agentType, ControlParameters controlParameters)
            {
                this.agentType = agentType;
                aiChallenges = new List<AIChallenge>();
                aiChallenges_UniversalDelegates_Profile = new List<Func<ChallengeData, UA, double, double>>();
                aiChallenges_UniversalDelegates_Valid = new List<Func<ChallengeData, bool>>();
                aiChallenges_UniversalDelegates_ValidFor = new List<Func<ChallengeData, UA, bool>>();
                aiChallenges_UniversalDelegates_Utility = new List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>>();
                aiTasks = new List<AITask>();
                this.controlParameters = controlParameters;
            }

            public AIData(Type agentType, List<AIChallenge> aIChallenges, List<AITask> aiTasks, ControlParameters controlParameters)
            {
                this.agentType = agentType;
                aiChallenges = aIChallenges;
                aiChallenges_UniversalDelegates_Profile = new List<Func<ChallengeData, UA, double, double>>();
                aiChallenges_UniversalDelegates_Valid = new List<Func<ChallengeData, bool>>();
                aiChallenges_UniversalDelegates_ValidFor = new List<Func<ChallengeData, UA, bool>>();
                aiChallenges_UniversalDelegates_Utility = new List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>>();
                this.aiTasks = aiTasks;
                this.controlParameters = controlParameters;
            }
        }

        public class ChallengeData
        {
            public AIChallenge aiChallenge;
            public Challenge challenge;
            public Location location;

            public List<Func<ChallengeData, UA, double, double>> universalDelegates_Profile = new List<Func<ChallengeData, UA, double, double>>();
            public List<Func<ChallengeData, bool>> universalDelegates_Valid = new List<Func<ChallengeData, bool>>();
            public List<Func<ChallengeData, UA, bool>> universalDelegates_ValidFor = new List<Func<ChallengeData, UA, bool>>();
            public List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>> universalDelegates_Utility = new List<Func<ChallengeData, UA, double, List<ReasonMsg>, double>>();

            public ChallengeData()
            {

            }

            public ChallengeData(ChallengeData challengeData)
            {
                aiChallenge = challengeData.aiChallenge;
                challenge = challengeData.challenge;
                location = challengeData.location;

                universalDelegates_Profile = challengeData.universalDelegates_Profile;
                universalDelegates_Valid = challengeData.universalDelegates_Valid;
                universalDelegates_ValidFor = challengeData.universalDelegates_ValidFor;
                universalDelegates_Utility = challengeData.universalDelegates_Utility;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ChallengeData);
            }

            public bool Equals(ChallengeData other)
            {
                if (other == null)
                    return false;

                // Check equality for simple types and lists separately
                return EqualityComparer<AIChallenge>.Default.Equals(aiChallenge, other.aiChallenge) &&
                       EqualityComparer<Challenge>.Default.Equals(challenge, other.challenge) &&
                       EqualityComparer<Location>.Default.Equals(location, other.location) &&
                       ListsEqual(universalDelegates_Profile, other.universalDelegates_Profile) &&
                       ListsEqual(universalDelegates_Valid, other.universalDelegates_Valid) &&
                       ListsEqual(universalDelegates_ValidFor, other.universalDelegates_ValidFor) &&
                       ListsEqual(universalDelegates_Utility, other.universalDelegates_Utility);
            }

            private bool ListsEqual<T>(List<T> list1, List<T> list2)
            {
                if (list1 == null && list2 == null)
                {
                    return true;
                }

                if (list1 == null || list2 == null || list1.Count != list2.Count)
                    {
                    return false;
                }

                for (int i = 0; i < list1.Count; i++)
                {
                    if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                        return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = -1692118631;
                hashCode = hashCode * -1521134295 + EqualityComparer<AIChallenge>.Default.GetHashCode(aiChallenge);
                hashCode = hashCode * -1521134295 + EqualityComparer<Challenge>.Default.GetHashCode(challenge);
                hashCode = hashCode * -1521134295 + EqualityComparer<Location>.Default.GetHashCode(location);
                hashCode = hashCode * -1521134295 + GetListHashCode(universalDelegates_Profile);
                hashCode = hashCode * -1521134295 + GetListHashCode(universalDelegates_Valid);
                hashCode = hashCode * -1521134295 + GetListHashCode(universalDelegates_ValidFor);
                hashCode = hashCode * -1521134295 + GetListHashCode(universalDelegates_Utility);
                return hashCode;
            }

            private int GetListHashCode<T>(List<T> list)
            {
                if (list == null)
                {
                    return 0;
                }

                int hash = 17;
                foreach (var item in list)
                {
                    hash = hash * 31 + (item != null ? item.GetHashCode() : 0);
                }

                return hash;
            }
        }

        public class TaskData
        {
            public AITask aiTask;
            public AITask.TargetCategory targetCategory;
            public Location targetLocation;
            public SocialGroup targetSocialGroup;
            public Unit targetUnit;

            public TaskData()
            {

            }

            public TaskData(TaskData taskData)
            {
                aiTask = taskData.aiTask;
                targetCategory = taskData.targetCategory;
                targetLocation = taskData.targetLocation;
                targetSocialGroup = taskData.targetSocialGroup;
                targetUnit = taskData.targetUnit;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as TaskData);
            }

            public bool Equals(TaskData other)
            {
                if (other == null)
                {
                    return false;
                }

                return EqualityComparer<AITask>.Default.Equals(aiTask, other.aiTask) &&
                       EqualityComparer<AITask.TargetCategory>.Default.Equals(targetCategory, other.targetCategory) &&
                       EqualityComparer<Location>.Default.Equals(targetLocation, other.targetLocation) &&
                       EqualityComparer<SocialGroup>.Default.Equals(targetSocialGroup, other.targetSocialGroup) &&
                       EqualityComparer<Unit>.Default.Equals(targetUnit, other.targetUnit);
            }

            public override int GetHashCode()
            {
                int hashCode = 1002019024;
                hashCode = hashCode * -1521134295 + EqualityComparer<AITask>.Default.GetHashCode(aiTask);
                hashCode = hashCode * -1521134295 + targetCategory.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Location>.Default.GetHashCode(targetLocation);
                hashCode = hashCode * -1521134295 + EqualityComparer<SocialGroup>.Default.GetHashCode(targetSocialGroup);
                hashCode = hashCode * -1521134295 + EqualityComparer<Unit>.Default.GetHashCode(targetUnit);
                return hashCode;
            }
        }

        public AgentAI(Map map)
        {
            debug = new DebugProperties(false);
            this.map = map;

            ai = new Dictionary<Type, AIData>();
        }

        /// <summary>
        /// Registers the agent type (tAgent) to be controlled by the AgentAI, along with the AIData (aiData) for that agent type. Returns false if the agent type (tAgent) is not a subtype of UA, or it has already been registered.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// /// <param name="aiData"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type tAgent, AIData aiData)
        {
            if (!tAgent.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (!ai.ContainsKey(tAgent))
            {
                ai.Add(tAgent, aiData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers the agent type (tAgent) to be controlled by the AgentAI, along with the control parameters (control) for that agent type. Returns false if the agent type (tAgent) is not a subtype of UA, or it has already been registered.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// /// <param name="control"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type tAgent, ControlParameters control)
        {
            if (!tAgent.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (!ai.ContainsKey(tAgent))
            {
                ai.Add(tAgent, new AIData(tAgent, control));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers the agent type (tAgent) to be controlled by the AgentAI, along with the control parameters (control) for that agent type, and assigns the provided ai challenges (aiChallenges) and ai tasks (aiTasks) to them. Returns false if the agent type (tAgent) is not a subtype of UA, or it has already been registered AND it failed to register all aiChallenges and aiTasks.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenges"></param>
        /// <param name="aiTasks"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type tAgent, List<AIChallenge> aiChallenges, List<AITask> aiTasks, ControlParameters control)
        {
            if (!tAgent.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (aiChallenges == null)
            {
                aiChallenges = new List<AIChallenge>();
            }
            if (aiTasks == null)
            {
                aiTasks = new List<AITask>();
            }

            if (ai.ContainsKey(tAgent))
            {
                bool challenges = AddChallengesToAgentType(tAgent, aiChallenges);
                bool tasks = AddTasksToAgentType(tAgent, aiTasks);

                if (challenges || tasks)
                {
                    return true;
                }
            }
            if (!ai.ContainsKey(tAgent))
            {
                ai.Add(tAgent, new AIData(tAgent, aiChallenges, aiTasks, control));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers the agent type (tAgent) to be controlled by the AgentAI, along with the control parameters (control) for that agent type, and assigns the provided ai challenges (aiChallenges) and ai tasks (aiTasks) to them. Returns false if the agent type (tAgent) is not a subtype of UA, or it has already been registered AND it failed to register all aiChallenges and aiTasks.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenges"></param>
        /// <param name="aiTasks"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type tAgent, AIChallenge[] aiChallenges, AITask[] aiTasks, ControlParameters control)
        {
            if (!tAgent.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (aiChallenges == null)
            {
                aiChallenges = new AIChallenge[0];
            }
            if (aiTasks == null)
            {
                aiTasks = new AITask[0];
            }

            if (ai.ContainsKey(tAgent))
            {
                bool challenges = AddChallengesToAgentType(tAgent, aiChallenges);
                bool tasks = AddTasksToAgentType(tAgent, aiTasks);

                if (challenges || tasks)
                {
                    return true;
                }
            }
            if (!ai.ContainsKey(tAgent))
            {
                ai.Add(tAgent, new AIData(tAgent, aiChallenges.ToList(), aiTasks.ToList(), control));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the agent type (tAgent) has been registered. If it has, it returns true.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <returns></returns>
        public bool ContainsAgentType(Type tAgent)
        {
            if (ai.ContainsKey(tAgent))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the agent type (tAgent) has been registered. If it has, it returns ture and outputs the AI data for that agent type (tAgent). If it has not, it returns false and the output AI data is null.
        /// <para>This is the primary means of altering AIChallnges that were assigned to the agent type's AI by other mods, including those that mimic the base game that were added by the Community Library itself.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiData"></param>
        /// <returns></returns>
        public bool TryGetAgentType(Type tAgent, out AIData aiData)
        {
            if (ai.TryGetValue(tAgent, out AIData data))
            {
                aiData = data;
                return true;
            }

            aiData = null;
            return false;
        }

        /// <summary>
        /// IMPORTANT! Use TryGetAgentType and modify the AIChallenges registered to it instead of using this function. Removing an agent type added by another mod is irreperable, and will prevent their agent from operating.
        /// <para>If the agent type (tAgent) has been registered and it can be removed, it returns true and outputs the AI data for that agent type. If it has not been registered, it returns false and the AI data is null</para>
        /// <para>If you Deregister an Agent that was registered by another mod, the AI for that agent type will not operate. Only do this if you are impementing an alternative AI for it, and cannot achieve the desired result by modifying the AiChallnges.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiData"></param>
        /// <returns></returns>
        public bool DeregisterAgentType(Type tAgent, out AIData aiData)
        {
            if (ai.TryGetValue(tAgent, out AIData data))
            {
                aiData = data;
                return ai.Remove(tAgent);
            }

            aiData = null;
            return false;
        }

        // CHALLENGE SECTION //

        /// <summary>
        /// Assigns an AIChallenge (aiChallenge) to the agent AI for the agent type (tAgent). If the agent type has not been registered, or an AIChallenge has already been assigned for that challenge type, it returns false.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool AddChallengeToAgentType(Type tAgent, AIChallenge aiChallenge)
        {
            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                if (!aiData.aiChallenges.Any(aiC => aiC.challengeType == aiChallenge.challengeType))
                {
                    aiData.aiChallenges.Add(aiChallenge);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assigns a list of AIChallenges to the agent AI for the agent type (tAgent). If the agent type has not been registered, or an AIChallenge has already been assigned for ALL challenge types in the list, it returns false.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenges"></param>
        /// <returns></returns>
        public bool AddChallengesToAgentType(Type tAgent, List<AIChallenge> aiChallenges)
        {
            bool result = false;

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                foreach (AIChallenge aiChallenge in aiChallenges)
                {
                    if (!aiData.aiChallenges.Any(aiC => aiC.challengeType == aiChallenge.challengeType))
                    {
                        aiData.aiChallenges.Add(aiChallenge);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns an array of AIChallenges to the agent AI for agent type (tAgent). If the agent type has not been registered, or an AIChallenge has already been assigned for ALL challenge types in the array, it returns false.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenges"></param>
        /// <returns></returns>
        public bool AddChallengesToAgentType(Type tAgent, AIChallenge[] aiChallenges)
        {
            bool result = false;

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                foreach (AIChallenge aiChallenge in aiChallenges)
                {
                    if (!aiData.aiChallenges.Any(aiC => aiC.challengeType == aiChallenge.challengeType))
                    {
                        aiData.aiChallenges.Add(aiChallenge);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the agent type (tAgent) has been registered and, if it has, returns the instance of AIChallenge where 'AIChallenge.challengeType' is challenge type (tChallenge). If the agent type has not been registered, or no AIChallenge has been asigned to it with that challengeType, it returns null.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="tChallenge"></param>
        /// <returns></returns>
        public AIChallenge GetAIChallengeFromAgentType(Type tAgent, Type tChallenge)
        {
            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                return aiData.aiChallenges.FirstOrDefault(aiC => aiC.challengeType == tChallenge);
            }

            return null;
        }

        /// <summary>
        /// IMPORTANT! Add the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags instead of using this function. Removing an AIChallenge added by another mod is irreperable, and will prevent their agent from performing the challnge indefinately.
        /// <para>Checks if the agent type (tAgent) has an AIChallenge assigned to it, and removes it if it has. It returns true if it has removed the AIChallenge.</para>
        /// <para>If you remove an AIChallenge that was registered by another mod, you may not be able to re-add it, if needed at a later time. Adding the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags will prevent the agent AI from performing the challenge in a non-destructive way.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool RemoveChallengeFromAgentType(Type tAgent, AIChallenge aiChallenge)
        {
            if (aiChallenge == null)
            {
                return false;
            }

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                return aiData.aiChallenges.Remove(aiChallenge);
            }

            return false;
        }

        /// <summary>
        /// IMPORTANT! Add the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags instead of using this function. Removing an AIChallenge added by another mod is irreperable, and will prevent their agent from performing the challnge indefinately.
        /// <para>Checks if the agent type (tAgent) has an AIChallenge assigned to it where 'AIChallenge.challengeType' is challenge type (tChallenge), and removes it if it has. It returns true if it has removed the AIChallenge.</para>
        /// <para>If you remove an AIChallenge that was registered by another mod, you may not be bale to re-add it, if needed at a later time. Adding the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags will prevent the agent AI from performing the challenge in a non-destructive way.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool RemoveChallengeFromAgentType(Type tAgent, Type tChallenge)
        {
            if (tChallenge == null || !tChallenge.IsSubclassOf(typeof(Challenge)))
            {
                return false;
            }

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                AIChallenge aiChallenge = aiData.aiChallenges.FirstOrDefault(aiC => aiC.challengeType == tChallenge);
                if (aiChallenge != null)
                {
                    return aiData.aiChallenges.Remove(aiChallenge);
                }
            }

            return false;
        }

        // TASK SECTION //

        /// <summary>
        /// Assigns an AITask to the agent AI for agent type (tAgent). If the agent type has not been registered, or an AITask has already been assigned for that task type, it returns false.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiTask"></param>
        /// <returns></returns>
        public bool AddTaskToAgentType(Type tAgent, AITask aiTask)
        {
            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                if (!aiData.aiTasks.Any(aiT => aiT.taskType == aiTask.taskType && aiT.title == aiTask.title))
                {
                    aiData.aiTasks.Add(aiTask);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assigns a list of AITasks (aiTasks) to the agent AI for agent type (tAgent). If the agent type has not been registered, or an AITask has already been assigned for task types in the list, it returns false.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiTasks"></param>
        /// <returns></returns>
        public bool AddTasksToAgentType(Type t, List<AITask> aiTasks)
        {
            bool result = false;

            if (TryGetAgentType(t, out AIData aiData) && aiData != null)
            {
                foreach (AITask aiTask in aiTasks)
                {
                    if (!aiData.aiTasks.Any(aiT => aiT.taskType == aiTask.taskType && aiT.title == aiTask.title))
                    {
                        aiData.aiTasks.Add(aiTask);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns an array of AITasks (aiTasks) to the agent AI for agent type (tAgent). If the agent type has not been registered, or an AITask has already been assigned for ALL challenge types in the array, it returns false;
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiTasks"></param>
        /// <returns></returns>
        public bool AddTasksToAgentType(Type tAgent, AITask[] aiTasks)
        {
            bool result = false;

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                foreach (AITask aiTask in aiTasks)
                {
                    if (!aiData.aiTasks.Contains(aiTask))
                    {
                        aiData.aiTasks.Add(aiTask);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if agent type (tAgent) has been registered and, if it has, returns the instance of AITask where 'AITask.taskType' is task type (tTask). If the agent type has not been registered or no AITask has been assigned to it with taskType tTask, it returns null.
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="tTask"></param>
        /// <returns></returns>
        public AITask GetAITaskFromAgentType(Type tAgent, Type tTask, string title)
        {
            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                return aiData.aiTasks.FirstOrDefault(aiT => aiT.taskType == tTask && aiT.title == title);
            }

            return null;
        }

        /// <summary>
        /// IMPORTANT! Add the 'TaskTags.Forbidden' tag to the AITask's tags instead of using this function. Removing an AITAsk added by another mod is irreperable, and will prevent their agent from performing the task indefinately.
        /// <para>Checks if the agent type (tAgent) has an AITask assigned to it, and removes it if it has. It returns true if it has removed the AITask.</para>
        /// <para>If you remove an AITask that was registered by another mod, you may not be bale to re-add it if needed at a later time. Adding the 'TaskTags.Forbidden' to the AITask's tags will prevent the agent AI from performing the task in a non-destructive way.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="aiTask"></param>
        /// <returns></returns>
        public bool RemoveTaskFromAgentType(Type tAgent, AITask aiTask)
        {
            if (aiTask == null)
            {
                return false;
            }

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                return aiData.aiTasks.Remove(aiTask);
            }

            return false;
        }

        /// <summary>
        /// IMPORTANT! Add the 'TaskTags.Forbidden' tag to the AITask's tags instead of using this function. Removing an AITask added by another mod is irreperable, and will prevent their agent from performing the task indefinately.
        /// <para>Checks if the agent type (tAgent) has an AITask assigned to it where 'AITask.taskType' is task type (tTask), and removes it if it has. It returns true if it has removed the AITask.</para>
        /// <para>If you remove an AITask that was registered by another mod, you may not be bale to re-add it if needed at a later time. Adding the 'TaskTags.Forbidden' to the AITask's tags will prevent the agent AI from performing the task in a non-destructive way.</para>
        /// </summary>
        /// <param name="tAgent"></param>
        /// <param name="tTask"></param>
        /// <returns></returns>
        public bool RemoveTaskFromAgentType(Type tAgent, Type tTask, string title)
        {
            if (tAgent == null || tTask == null || !tTask.IsSubclassOf(typeof(Assets.Code.Task)))
            {
                return false;
            }

            if (TryGetAgentType(tAgent, out AIData aiData) && aiData != null)
            {
                AITask aiTask = aiData.aiTasks.FirstOrDefault(aiT => aiT.taskType == tTask && aiT.title == title);
                if (aiTask != null)
                {
                    return aiData.aiTasks.Remove(aiTask);
                }
            }

            return false;
        }

        public static DebugProperties setupDebugInternal(DebugProperties debug)
        {
            DebugProperties result = new DebugProperties(false);

            if (AgentAI.debug.debug || debug.debug)
            {
                result.debug = true;

                if (AgentAI.debug.outputProfile_AllChallenges || debug.outputProfile_AllChallenges)
                {
                    result.outputProfile_AllChallenges = true;
                }
                if (AgentAI.debug.outputProfile_VisibleChallenges || debug.outputProfile_VisibleChallenges)
                {
                    result.outputProfile_VisibleChallenges = true;
                }
                if (AgentAI.debug.outputProfile_VisibleAgents || debug.outputProfile_VisibleAgents)
                {
                    result.outputProfile_VisibleAgents = true;
                }
                if (AgentAI.debug.outputVisibility_AllChallenges || debug.outputVisibility_AllChallenges)
                {
                    result.outputVisibility_AllChallenges = true;
                }
                if (AgentAI.debug.outputVisibility_VisibleChallenges || debug.outputVisibility_VisibleChallenges)
                {
                    result.outputVisibility_VisibleChallenges = true;
                }
                if (AgentAI.debug.outputValidity_AllChallenges || debug.outputValidity_AllChallenges)
                {
                    result.outputValidity_AllChallenges = true;
                }
                if (AgentAI.debug.outputValidity_ValidChallenges || debug.outputValidity_ValidChallenges)
                {
                    result.outputValidity_ValidChallenges = true;
                }
                if (AgentAI.debug.outputUtility_ValidChallenges || debug.outputUtility_ValidChallenges)
                {
                    result.outputUtility_ValidChallenges = true;
                }
                if (AgentAI.debug.outputUtility_VisibleAgentsAttack || debug.outputUtility_VisibleAgentsAttack)
                {
                    result.outputUtility_VisibleAgentsAttack = true;
                }
                if (AgentAI.debug.outputUtility_VisibleAgentsBodyguard || debug.outputUtility_VisibleAgentsBodyguard)
                {
                    result.outputUtility_VisibleAgentsBodyguard = true;
                }
                if (AgentAI.debug.outputUtility_VisibleAgentsDisrupt || debug.outputUtility_VisibleAgentsDisrupt)
                {
                    result.outputUtility_VisibleAgentsDisrupt = true;
                }
                if (AgentAI.debug.outputValidity_AllTasks || debug.outputValidity_AllTasks)
                {
                    result.outputValidity_AllTasks = true;
                }
                if (AgentAI.debug.outputValidity_ValidTasks || debug.outputValidity_ValidTasks)
                {
                    result.outputValidity_ValidTasks = true;
                }
                if (AgentAI.debug.outputUtility_ValidTasks || debug.outputUtility_ValidTasks)
                {
                    result.outputUtility_ValidTasks = true;
                }
                if (AgentAI.debug.outputUtility_ChosenAction || debug.outputUtility_ChosenAction)
                {
                    result.outputUtility_ChosenAction = true;
                }
            }

            return result;
        }

        public bool isAIRunning()
        {
            return aiRunning;
        }

        public bool isAICheckingUtility()
        {
            return aiCheckingUtility;
        }

        public void turnTickAI(UA ua)
        {
            if (ua == null)
            {
                return;
            }

            if (!TryGetAgentType(ua.GetType(), out AIData aiData) || aiData == null)
            {
                return;
            }

            debugInternal = setupDebugInternal(aiData.controlParameters.debugProperties);
            if (debugInternal.debug)
            {
                Console.WriteLine("CommunityLib: Running Agent AI for " + ua.getName());
            }
            isDuringIntercept = false;
            List<ChallengeData> validChallengeData = getAllValidChallengesAndRituals(ua);
            List<Unit> visibleUnits;
            MethodInfo MI_getVisibleUnits = AccessTools.DeclaredMethod(ua.GetType(), "getVisibleUnits", new Type[0]);
            if (MI_getVisibleUnits != null)
            {
                visibleUnits = (List<Unit>)MI_getVisibleUnits.Invoke(ua, new object[0]);
            }
            else
            {
                visibleUnits = ua.getVisibleUnits();
            }
            List<TaskData> validTasks = getAllValidTasks(ua);

            bool result = false;
            isDuringIntercept = true;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptAgentAI(ua, aiData, validChallengeData, validTasks, visibleUnits);
                if (retValue)
                {
                    result = true;
                }
            }

            isDuringIntercept = false;
            if (result)
            {
                if (debugInternal.debug)
                {
                    Console.WriteLine("CommunityLib: Agent AI for " + ua.getName() + " intercepted.");
                }
                return;
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onAgentAI_StartOfProcess(ua, aiData, validChallengeData, validTasks, visibleUnits);
            }

            aiRunning = true;

            double utility = 0.01;
            double utility2;
            List<ChallengeData> targetChallenges = new List<ChallengeData>();
            List<Unit> targetUnits = new List<Unit>();
            List<Unit> targetGuards = new List<Unit>();
            List<UA> targetDisrupts = new List<UA>();
            List<TaskData> targetTasks = new List<TaskData>();

            foreach (ChallengeData cData in validChallengeData)
            {
                List<ReasonMsg> reasonMsgs = null;
                if (debugInternal.debug && debugInternal.outputValidity_ValidChallenges)
                {
                    reasonMsgs = new List<ReasonMsg>();
                }

                utility2 = getChallengeUtility(cData, ua, aiData, aiData.controlParameters, reasonMsgs);

                if (debugInternal.debug && debugInternal.outputValidity_ValidChallenges && reasonMsgs != null)
                {
                    Console.WriteLine("CommunityLib: Utility for " + cData.challenge.getName() + " at " + cData.challenge.location.getName() + " (" + (cData.challenge.location.soc?.getName() ?? "Wilderness") + ")");
                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                    {
                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                    }
                    Console.WriteLine("CommunityLib: Total: " + utility2);
                }

                if (utility2 >= utility)
                {
                    if (utility2 > utility)
                    {
                        targetChallenges.Clear();
                    }
                    targetChallenges.Add(cData);
                    utility = utility2;
                }
                else if (utility2 == utility)
                {
                    targetChallenges.Add(cData);
                }
            }

            if (visibleUnits != null)
            {
                foreach (Unit unit in visibleUnits)
                {
                    UA agent = unit as UA;
                    if (agent != null && !agent.isDead)
                    {
                        if (debugInternal.debug && (debugInternal.outputProfile_VisibleAgents || debugInternal.outputUtility_VisibleAgentsAttack || debugInternal.outputUtility_VisibleAgentsBodyguard || debugInternal.outputUtility_VisibleAgentsDisrupt))
                        {
                            Console.WriteLine("CommunityLib: Unit " + unit.getName() + " (" + (unit.society?.getName() ?? "No Society") + ") at " + unit.location.getName() + " (" + (unit.location.soc?.getName() ?? "No Society") + ")");
                        }

                        if (debugInternal.debug && debugInternal.outputProfile_VisibleAgents)
                        {
                            Console.WriteLine("CommunityLib: Profile: " + agent.profile);
                        }

                        List<ReasonMsg> reasonMsgs = null;
                        if (!(agent.task is Task_InHiding))
                        {
                            if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsAttack)
                            {
                                reasonMsgs = new List<ReasonMsg>();
                            }

                            utility2 = ua.getAttackUtility(agent, reasonMsgs, aiData.controlParameters.includeDangerousFoe);

                            if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsAttack && reasonMsgs != null)
                            {
                                Console.WriteLine("CommunityLib: Attack Utility");
                                foreach (ReasonMsg reasonMsg in reasonMsgs)
                                {
                                    Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                }
                                Console.WriteLine("CommunityLib: Total: " + utility2);
                            }

                            if (utility2 >= utility)
                            {
                                targetChallenges.Clear();
                                if (utility2 > utility)
                                {
                                    targetUnits.Clear();
                                }
                                targetUnits.Add(unit);
                                utility = utility2;
                            }
                        }

                        reasonMsgs = null;
                        if (ua != map.awarenessManager.getChosenOne())
                        {
                            if (ua.society?.getRel(unit.society).state != DipRel.dipState.war)
                            {
                                if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsBodyguard)
                                {
                                    reasonMsgs = new List<ReasonMsg>();
                                }

                                utility2 = ua.getBodyguardUtility(unit, reasonMsgs);

                                if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsBodyguard && reasonMsgs != null)
                                {
                                    Console.WriteLine("CommunityLib: Bodyguard Utility");
                                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                                    {
                                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                    }
                                    Console.WriteLine("CommunityLib: Total: " + utility2);
                                }

                                if (utility2 >= utility)
                                {

                                    targetChallenges.Clear();
                                    targetUnits.Clear();

                                    if (utility2 > utility)
                                    {
                                        targetGuards.Clear();
                                    }
                                    targetGuards.Add(unit);
                                    utility = utility2;
                                }
                            }
                        }

                        reasonMsgs = null;
                        Task_PerformChallenge task = unit.task as Task_PerformChallenge;
                        if (!(task?.challenge.isChannelled() ?? true))
                        {
                            if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsDisrupt)
                            {
                                reasonMsgs = new List<ReasonMsg>();
                            }

                            utility2 = ua.getDisruptUtility(unit, null);

                            if (debugInternal.debug && debugInternal.outputUtility_VisibleAgentsDisrupt && reasonMsgs != null)
                            {
                                Console.WriteLine("CommunityLib: Disrupt Utility");
                                foreach (ReasonMsg reasonMsg in reasonMsgs)
                                {
                                    Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                }
                                Console.WriteLine("CommunityLib: Total: " + utility2);
                            }

                            if (utility2 >= utility)
                            {
                                targetChallenges.Clear();
                                targetUnits.Clear();
                                targetGuards.Clear();

                                if (utility2 > utility)
                                {
                                    targetDisrupts.Clear();
                                }

                                targetDisrupts.Add(agent);
                                utility = utility2;
                            }
                        }
                    }
                }
            }

            foreach (TaskData taskData in validTasks)
            {
                List<ReasonMsg> reasonMsgs = null;
                if (debugInternal.debug && debugInternal.outputUtility_ValidTasks)
                {
                    reasonMsgs = new List<ReasonMsg>();
                }

                utility2 = checkTaskUtility(taskData, ua, aiData, aiData.controlParameters, reasonMsgs);

                if (debugInternal.debug && debugInternal.outputUtility_ValidTasks && reasonMsgs != null)
                {
                    Console.WriteLine("CommunityLib: Task Utility");
                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                    {
                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                    }
                    Console.WriteLine("CommunityLib: Total: " + utility2);
                }

                if (utility2 >= utility)
                {
                    targetChallenges.Clear();
                    targetUnits.Clear();
                    targetGuards.Clear();
                    targetDisrupts.Clear();

                    if (utility2 > utility)
                    {
                        targetTasks.Clear();
                    }

                    targetTasks.Add(taskData);
                    utility = utility2;
                }
            }

            if (targetTasks.Count > 0)
            {
                TaskData targetTask = targetTasks[0];
                if (targetTasks.Count > 0)
                {
                    targetTask = targetTasks[Eleven.random.Next(targetTasks.Count)];
                }
                if (debugInternal.debug && debugInternal.outputUtility_ChosenAction)
                {
                    List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                    Console.WriteLine("CommunityLib: " + ua.getName() + " is going to perform task of type " + targetTask.aiTask.taskType);
                    targetTask.aiTask.checkTaskUtility(targetTask, ua, aiData.controlParameters, reasonMsgs);
                    if (reasonMsgs != null)
                    {
                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                        {
                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                        }
                        Console.WriteLine("CommunityLib: Utility: " + utility);
                    }
                }
                ua.task = targetTask.aiTask.instantiateTask(ua, targetTask.targetCategory, targetTask);
            }
            else if (targetUnits.Count > 0)
            {
                Unit targetUnit = targetUnits[0];
                if (targetUnits.Count > 1)
                {
                    targetUnit = targetUnits[Eleven.random.Next(targetUnits.Count)];
                }

                if (debugInternal.debug && debugInternal.outputUtility_ChosenAction)
                {
                    List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                    Console.WriteLine("CommunityLib: " + ua.getName() + " is moving to attack " + targetUnit.getName() + " (" + (targetUnit.society?.getName() ?? "No Soceity") + ") at " + targetUnit.location.getName() + " (" + (targetUnit.location.soc?.getName() ?? "No Soceity") + ")");
                    ua.getAttackUtility(targetUnit, reasonMsgs, true);
                    if (reasonMsgs != null)
                    {
                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                        {
                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                        }
                        Console.WriteLine("CommunityLib: Utility: " + utility);
                    }
                }

                UA agent = targetUnit as UA;
                if (agent != null && (ua.person.isWatched() || ua.isCommandable()))
                {
                    map.addUnifiedMessage(ua, targetUnit, ua.person.getName() + " attacking", ua.getName() + " is moving to attack " + targetUnit.getName() + ". They will seek them out for a number of turns before losing the trail.", UnifiedMessage.messageType.HERO_ATTACKING);
                }
                Task_AttackUnit task = new Task_AttackUnit(ua, targetUnit);
                ua.getAttackUtility(targetUnit, task.reasonsMessages, true);
                ua.task = task;
            }
            else if (targetDisrupts.Count > 0)
            {
                UA targetDisrupt = targetDisrupts[0];
                if (targetDisrupts.Count > 1)
                {
                    targetDisrupt = targetDisrupts[Eleven.random.Next(targetDisrupts.Count)];
                }

                if (debugInternal.debug && debugInternal.outputUtility_ChosenAction)
                {
                    List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                    Console.WriteLine("CommunityLib: " + ua.getName() + " is moving to disrupt " + targetDisrupt.getName() + " (" + (targetDisrupt.society?.getName() ?? "No Soceity") + ") at " + targetDisrupt.location.getName() + " (" + (targetDisrupt.location.soc?.getName() ?? "No Soceity") + ")");
                    ua.getDisruptUtility(targetDisrupt, reasonMsgs);
                    if (reasonMsgs != null)
                    {
                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                        {
                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                        }
                        Console.WriteLine("CommunityLib: Utility: " + utility);
                    }
                }

                if (ua.person.isWatched())
                {
                    map.addUnifiedMessage(ua, targetDisrupt, ua.person.getName() + " moving to disrupt", ua.getName() + " is moving to disrupt " + targetDisrupt.getName() + ", which will slow their challenge progress", UnifiedMessage.messageType.MOVING_TO_DISRUPT);
                }
                Task_DisruptUA task = new Task_DisruptUA(ua, targetDisrupt);
                ua.getDisruptUtility(targetDisrupt, task.reasons);
                ua.task = task;
            }
            else if (targetGuards.Count > 0)
            {
                Unit targetGuard = targetGuards[0];
                if (targetUnits.Count > 1)
                {
                    targetGuard = targetGuards[Eleven.random.Next(targetGuards.Count)];
                }

                if (debugInternal.debug && debugInternal.outputUtility_ChosenAction)
                {
                    List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                    Console.WriteLine("CommunityLib: " + ua.getName() + " is moving to guard " + targetGuard.getName() + " (" + (targetGuard.society?.getName() ?? "No Soceity") + ") at " + targetGuard.location.getName() + " (" + (targetGuard.location.soc?.getName() ?? "No Soceity") + ")");
                    ua.getDisruptUtility(targetGuard, reasonMsgs);
                    if (reasonMsgs != null)
                    {
                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                        {
                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                        }
                        Console.WriteLine("CommunityLib: Utility: " + utility);
                    }
                }

                if (ua.person.isWatched())
                {
                    map.addUnifiedMessage(ua, targetGuard, ua.person.getName() + " moving to guard", ua.getName() + " is moving to protect " + targetGuard.getName() + " from any attackers", UnifiedMessage.messageType.MOVING_TO_GUARD);
                }
                Task_Bodyguard task = new Task_Bodyguard(ua, targetGuard);
                ua.task = task;
            }
            else if (targetChallenges.Count > 0)
            {
                ChallengeData targetChallenge = targetChallenges[0];
                if (targetChallenges.Count > 1)
                { 
                    targetChallenge = targetChallenges[Eleven.random.Next(targetChallenges.Count)];
                }

                if (debugInternal.debug && debugInternal.outputUtility_ChosenAction)
                {
                    Console.WriteLine("CommunityLib: " + ua.getName() + " is going to perform challenge " + targetChallenge.challenge.getName() + " at " + targetChallenge.location.getName() + " (" + (targetChallenge.location.soc?.getName() ?? "No Society") + ")");

                    List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                    getChallengeUtility(targetChallenge, ua,aiData, aiData.controlParameters, reasonMsgs);

                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                    {
                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                    }
                    Console.WriteLine("CommunityLib: Total: " + utility);
                }

                if (ua.person.isWatched() || targetChallenge.location.isWatched)
                {
                    map.addUnifiedMessage(ua, null, "Beginning Quest", ua.getName() + " is beginning quest " + targetChallenge.challenge.getName() + " at location " + targetChallenge.location.getName(true), UnifiedMessage.messageType.BEGINNING_QUEST);
                }

                bool safeMove = false;
                if (aiData.controlParameters.forceSafeMove)
                {
                    safeMove = true;
                }
                else if (targetChallenge.aiChallenge != null)
                {
                    safeMove = targetChallenge.aiChallenge.safeMove;
                }

                if (ua.location == targetChallenge.location)
                {
                    ua.task = new Task_PerformChallenge(targetChallenge.challenge);
                    targetChallenge.challenge.claimedBy = ua;
                }
                else
                {
                    ua.task = new Task_GoToPerformChallengeAtLocation(targetChallenge.challenge, targetChallenge.location, safeMove);
                }
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onAgentAI_EndOfProcess(ua, aiData, validChallengeData, validTasks, visibleUnits);
            }

            if (ua.task == null && ua.location.index != ua.homeLocation)
            {
                ua.task = new Task_GoToLocation(map.locations[ua.homeLocation]);
            }
            else if (ua.task == null && ua.location.index == ua.homeLocation)
            {
                ua.task = new Task_GoToLocation(map.locations[ua.homeLocation].getNeighbours()[Eleven.random.Next(map.locations[ua.homeLocation].getNeighbours().Count)]);
            }

            aiRunning = false;
        }

        public List<ChallengeData> getAllValidChallengesAndRituals(UA ua)
        {
            List<ChallengeData> result = new List<ChallengeData>();
            List<ChallengeData> ritualData = new List<ChallengeData>();

            if (!TryGetAgentType(ua.GetType(), out AIData aiData) || aiData == null)
            {
                Console.WriteLine("CommunityLib: ERROR: Failed to Get aiData for " + ua.getName() + " of type " + ua.GetType());
                return null;
            }
            debugInternal = setupDebugInternal(aiData.controlParameters.debugProperties);

            // Sort all aiChallenges into type-keyed dictionaries for faster searching.
            Dictionary<Type, AIChallenge>  aiChallenges = new Dictionary<Type, AIChallenge>();
            Dictionary<Type, AIChallenge>  aiRituals = new Dictionary<Type, AIChallenge>();

            if (aiData.aiChallenges != null)
            {
                foreach (AIChallenge aiChallenge in aiData.aiChallenges)
                {
                    if (aiChallenge.isRitual)
                    {
                        aiRituals.Add(aiChallenge.challengeType, aiChallenge);
                    }
                    else
                    {
                        aiChallenges.Add(aiChallenge.challengeType, aiChallenge);
                    }
                }
            }

            // Get instances of Ritual challenges and store in ChallengeData objects.
            if (aiData.controlParameters.considerAllRituals || aiRituals.Count > 0)
            {
                //Console.WriteLine("CommunityLib: Has " + aiRituals.Count + " rituals");
                List<Challenge> uaRituals = new List<Challenge>();
                uaRituals.AddRange(ua.rituals);
                foreach (Item item in ua.person.items)
                {
                    if (item != null)
                    {
                        uaRituals.AddRange(item.getRituals(ua));
                    }
                }
                //Console.WriteLine("CommunityLib: Found " + uaRituals.Count + " rituals");

                foreach (Challenge ritual in uaRituals)
                {
                    if (aiRituals.TryGetValue(ritual.GetType(), out AIChallenge aiChallenge))
                    {
                        //Console.WriteLine("CommunityLib: Found ritual " + ritual.getName());
                        ChallengeData d = new ChallengeData
                        {
                            aiChallenge = aiChallenge,
                            challenge = ritual,
                            universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                            universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                            universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                            universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                        };

                        if (aiChallenge.tags.Contains(AIChallenge.ChallengeTags.RequireLocal))
                        {
                            d.location = ua.location;

                            if (getChallengeIsValid(ua, d, aiData.controlParameters))
                            {
                                result.Add(d);
                            }
                        }
                        else
                        {
                            ritualData.Add(d);
                        }
                    }
                    else if (aiData.controlParameters.considerAllRituals)
                    {
                        ChallengeData d = new ChallengeData
                        {
                            aiChallenge = null,
                            challenge = ritual,
                            location = ua.location,
                            universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                            universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                            universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                            universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                        };

                        if (getChallengeIsValid(ua, d, aiData.controlParameters))
                        {
                            result.Add(d);
                        }
                    }
                }
            }

            if (aiData.controlParameters.considerAllChallenges || aiChallenges.Count > 0 || aiRituals.Count > 0)
            {
                foreach (Location location in ua.map.locations)
                {
                    List<Challenge> challenges = location.GetChallenges();

                    foreach (Challenge challenge in challenges)
                    {
                        if (challenge is Ritual)
                        {
                            continue;
                        }

                        if (aiChallenges.TryGetValue(challenge.GetType(), out AIChallenge aiChallenge))
                        {
                            if (aiChallenge.tags.Contains(AIChallenge.ChallengeTags.RequireLocal) && location != ua.location)
                            {
                                continue;
                            }

                            ChallengeData d = new ChallengeData
                            {
                                aiChallenge = aiChallenge,
                                challenge = challenge,
                                location = location,
                                universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                                universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                                universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                                universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                            };

                            if (getChallengeIsValid(ua, d, aiData.controlParameters))
                            {
                                result.Add(d);
                            }
                        }
                        else if (aiData.controlParameters.considerAllChallenges)
                        {
                            ChallengeData d = new ChallengeData
                            {
                                aiChallenge = null,
                                challenge = challenge,
                                location = location,
                                universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                                universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                                universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                                universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                            };
                            if (getChallengeIsValid(ua, d, aiData.controlParameters))
                            {
                                result.Add(d);
                            }
                        }
                    }

                    foreach (ChallengeData rData in ritualData)
                    {
                        ChallengeData d = new ChallengeData(rData) { location = location };

                        if (getChallengeIsValid(ua, d, aiData.controlParameters))
                        {
                            result.Add(d);
                        }
                    }
                }
            }

            return result;
        }

        public bool getChallengeIsValid(UA ua, Challenge challenge, Location location = null)
        {
            if (ua == null || challenge == null)
            {
                return false;
            }

            if (ModCore.Get().GetAgentAI().TryGetAgentType(ua.GetType(), out AIData aiData) && aiData != null)
            {
                AIChallenge aiChallenge = ModCore.Get().GetAgentAI().GetAIChallengeFromAgentType(ua.GetType(), challenge.GetType());
                ChallengeData challengeData = new ChallengeData
                {
                    aiChallenge = aiChallenge,
                    challenge = challenge,
                    location = challenge.location,
                    universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                    universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                    universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                    universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                };

                if (challenge is Ritual)
                {
                    if (location == null)
                    {
                        challengeData.location = ua.location;
                    }
                    else
                    {
                        challengeData.location = location;
                    }
                }

                return getChallengeIsValid(ua, challengeData, aiData.controlParameters);
            }
            else
            {
                return challenge.valid() && challenge.validFor(ua);
            }
        }

        public bool getChallengeIsValid(UA ua, ChallengeData challengeData, ControlParameters controlParams)
        {
            if (challengeData.challenge.claimedBy != null && challengeData.challenge.claimedBy.isDead)
            {
                challengeData.challenge.claimedBy = null;
            }

            if (debugInternal.debug && (debugInternal.outputProfile_AllChallenges || debugInternal.outputVisibility_AllChallenges || debugInternal.outputValidity_AllChallenges || debugInternal.outputValidity_ValidChallenges))
            {
                Console.WriteLine("CommunityLib: Visibility and Validity for " + challengeData.challenge.getName() + " at " + challengeData.location.getName() + " (" + (challengeData.location.soc?.getName() ?? "Wilderness") + ") by " + ua.getName() + " (" + (ua.society?.getName() ?? "No Society") + ")");
            }

            if (!controlParams.respectChallengeVisibility
                || (challengeData.aiChallenge != null && challengeData.aiChallenge.checkChallengeVisibility(challengeData, ua, controlParams))
                || (challengeData.aiChallenge == null && controlParams.considerAllChallenges && ua.map.getStepDist(ua.location, challengeData.location) <= (challengeData.challenge.getProfile() / 10)))
            {
                if (!controlParams.respectChallengeAlignment || !(challengeData.challenge.isGoodTernary() == -1 && (ua is UAG || ua is UAA) && !ua.corrupted))
                {
                    if (!controlParams.respectChallengeAlignment || !(challengeData.challenge.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                    {
                        if (challengeData.challenge.allowMultipleUsers() || challengeData.challenge.claimedBy == null || challengeData.challenge.claimedBy == ua)
                        {
                            if ((challengeData.aiChallenge != null && challengeData.aiChallenge.checkChallengeIsValid(challengeData, ua, controlParams))
                                || (challengeData.aiChallenge == null && challengeData.challenge is Ritual && controlParams.considerAllRituals && (challengeData.universalDelegates_Valid?.All(d => d(challengeData) == true) ?? true) && (challengeData.universalDelegates_ValidFor?.All(d => d(challengeData, ua) == true) ?? true) && challengeData.challenge.valid() && challengeData.challenge.validFor(ua))
                                || (challengeData.aiChallenge == null && !(challengeData.challenge is Ritual) && controlParams.considerAllChallenges && (challengeData.universalDelegates_Valid?.All(d => d(challengeData) == true) ?? true) && (challengeData.universalDelegates_ValidFor?.All(d => d(challengeData, ua) == true) ?? true) && challengeData.challenge.valid() && challengeData.challenge.validFor(ua)))
                            {
                                return true;
                            }
                            else if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                            {
                                Console.WriteLine("CommunityLib: Invalid: Challenge Not Valid, or Not Valid For " + ua.getName());
                            }
                        }
                        else if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                        {
                            Console.WriteLine("CommunityLib: Invalid: Challenge Claimed by Other Agent");
                        }
                    }
                    else if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                    {
                        Console.WriteLine("CommunityLib: Invalid: Evil Agent Vs Good Challenge");
                    }
                }
                else if (debugInternal.debug && debugInternal.outputValidity_AllChallenges)
                {
                    Console.WriteLine("CommunityLib: Invalid: Good Agent Vs Evil Challenge");
                }
            }

            return false;
        }

        public double getChallengeUtility(UA ua, Challenge challenge, List<ReasonMsg> reasonMsgs, Location location = null)
        {
            double utility = 0.0;

            if (ua == null || challenge == null)
            {
                return -1.0;
            }

            if (ModCore.Get().GetAgentAI().TryGetAgentType(ua.GetType(), out AgentAI.AIData aiData) && aiData != null)
            {
                AIChallenge aiChallenge = ModCore.Get().GetAgentAI().GetAIChallengeFromAgentType(ua.GetType(), challenge.GetType());

                if (aiChallenge != null)
                {
                    AgentAI.ChallengeData cData = new AgentAI.ChallengeData
                    {
                        aiChallenge = aiChallenge,
                        challenge = challenge,
                        location = challenge.location
                    };

                    if (challenge is Ritual)
                    {
                        if (location == null)
                        {
                            cData.location = ua.location;
                        }
                        else
                        {
                            cData.location = location;
                        }
                    }

                    utility = ModCore.Get().GetAgentAI().getChallengeUtility(cData, ua, aiData, aiData.controlParameters, reasonMsgs);
                }
                else if (challenge is Ritual)
                {
                    if (aiData.controlParameters.considerAllRituals)
                    {
                        aiCheckingUtility = true;
                        utility = ua.getChallengeUtility(challenge, reasonMsgs);
                        aiCheckingUtility = false;

                        if (aiData.aiChallenges_UniversalDelegates_Utility.Count > 0)
                        {
                            ChallengeData d = new ChallengeData
                            {
                                aiChallenge = null,
                                challenge = challenge,
                                location = ua.location,
                                universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                                universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                                universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                                universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                            };

                            foreach (Func<ChallengeData, UA, double, List<ReasonMsg>, double> @delegate in aiData.aiChallenges_UniversalDelegates_Utility)
                            {
                                utility = @delegate(d, ua, utility, reasonMsgs);
                            }
                        }
                    }
                    else
                    {
                        utility = -1.0;
                    }
                }
                else if (aiData.controlParameters.considerAllChallenges)
                {
                    aiCheckingUtility = true;
                    utility = ua.getChallengeUtility(challenge, reasonMsgs);
                    aiCheckingUtility = false;

                    if (aiData.aiChallenges_UniversalDelegates_Utility.Count > 0)
                    {
                        ChallengeData d = new ChallengeData
                        {
                            aiChallenge = null,
                            challenge = challenge,
                            location = challenge.location,
                            universalDelegates_Profile = aiData.aiChallenges_UniversalDelegates_Profile,
                            universalDelegates_Valid = aiData.aiChallenges_UniversalDelegates_Valid,
                            universalDelegates_ValidFor = aiData.aiChallenges_UniversalDelegates_ValidFor,
                            universalDelegates_Utility = aiData.aiChallenges_UniversalDelegates_Utility
                        };

                        foreach (Func<ChallengeData, UA, double, List<ReasonMsg>, double> @delegate in aiData.aiChallenges_UniversalDelegates_Utility)
                        {
                            utility = @delegate(d, ua, utility, reasonMsgs);
                        }
                    }
                }
                else
                {
                    utility = -1.0;
                }
            }
            else
            {
                aiCheckingUtility = true;
                utility = ua.getChallengeUtility(challenge, reasonMsgs);
                aiCheckingUtility = false;
            }

            return utility;
        }

        public double getChallengeUtility(ChallengeData challengeData, UA ua, AIData aiData, ControlParameters controlParams, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            bool intercept = false;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptAgentAI_GetChallengeUtility(ua, aiData, challengeData, ref utility, reasonMsgs);

                if (retValue)
                {
                    intercept = true;
                }
            }

            if (intercept)
            {
                return utility;
            }

            Challenge challenge = challengeData.challenge;
            if (challengeData.aiChallenge == null)
            {
                aiCheckingUtility = true;
                utility = ua.getChallengeUtility(challengeData.challenge, reasonMsgs);
                aiCheckingUtility = false;
                return utility;
            }

            utility += challengeData.aiChallenge.checkChallengeUtility(challengeData, ua, controlParams, reasonMsgs);

            if (controlParams.respectTags)
            {
                utility += ua.person.getTagUtility(challenge.getPositiveTags(), challenge.getNegativeTags(), reasonMsgs);
            }

            if (controlParams.respectDanger)
            {
                double danger = getDangerUtility(challengeData, ua);
                reasonMsgs?.Add(new ReasonMsg("Danger (vs my HP)", danger));
                utility += danger;
            }

            if (controlParams.respectArmyIntercept)
            {
                foreach (Unit unit in challengeData.location.units)
                {
                    if (unit is UM um && um.homeLocation != -1 && um.map.locations[um.homeLocation] == challengeData.location && um.hostileTo(ua, false))
                    {
                        double armyIntercept = -125.0;
                        reasonMsgs?.Add(new ReasonMsg("Army Blocking me", armyIntercept));
                        utility += armyIntercept;
                    }
                }
            }

            if (controlParams.includeNotHolyTask)
            {
                if (!(challenge is ChallengeHoly) && !(challenge is Ch_RecruitMinion) && !(challenge is Ch_RecruitOgre) && !(challenge is Ch_LevelUp) && !(challenge is Ch_Rest) && !(challenge is Ch_Rest_InOrcCamp) && !(challenge is Ch_LayLow) && !(challenge is Ch_LayLowWilderness) && !(challenge is Ritual))
                {
                    utility += ua.map.param.holy_nonHolyTaskAversion;
                    reasonMsgs?.Add(new ReasonMsg("Not Holy Task", ua.map.param.holy_nonHolyTaskAversion));
                }
            }

            if (controlParams.respectTenets && ua.society is HolyOrder order)
            {
                foreach (HolyTenet tenet in order.tenets)
                {
                    utility += tenet.addUtility(challengeData.challenge, ua, reasonMsgs);
                }
            }

            if (controlParams.respectTraits && ua.person != null)
            {
                foreach (Trait t in ua.person.traits)
                {
                    utility += t.getUtilityChanges(challengeData.challenge, ua, reasonMsgs);
                }
            }

            if (controlParams.valueTimeCost)
            {
                //Console.WriteLine("CommunityLib: distance Divisor is " + getDistanceDivisor(challengeData, ua).ToString());
                utility /= getDistanceDivisor(challengeData, aiData, ua);
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                utility = hook.onAgentAI_GetChallengeUtility(ua, aiData, challengeData, utility, reasonMsgs);
            }

            return utility;
        }

        private double getDangerUtility(ChallengeData challengeData, UA ua, List<ReasonMsg> reasonMsgs = null)
        {
            double result = 0.0;

            if (challengeData.challenge.getDanger() <= 0)
            {
                return result;
            }

            result = challengeData.challenge.getDanger();
            result /= Math.Max(ua.hp, 1);

            foreach (Minion minion in ua.minions)
            {
                if (minion != null)
                {
                    result *= 0.8;
                }
            }

            result *= map.param.utility_ua_challengeDangerAversion;
            result *= -1;
            reasonMsgs?.Add(new ReasonMsg("Danger (vs my HP)", result));

            bool dangerTag = false;
            foreach (int tag in challengeData.challenge.getPositiveTags())
            {
                if (tag == Tags.DANGER)
                {
                    dangerTag = true;
                    break;
                }
            }

            if (!dangerTag)
            {
                result += ua.person.getTagUtility(new int[] { Tags.DANGER }, new int[0], reasonMsgs);
            }

            return result;
        }

        private double getDistanceDivisor(ChallengeData challengeData, AIData aiData, UA ua)
        {
            int distance = 0;

            Location[] pathTo = ua.map.getPathTo(ua.location, challengeData.location);

            if (pathTo == null || pathTo.Length < 2)
            {
                return distance;
            }

            distance = (int)Math.Ceiling((double)(pathTo.Length - 1) / (double)ua.getMaxMoves());

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                distance = hook.onUnitAI_GetsDistanceToLocation(ua, challengeData.location, pathTo, (int)distance);
            }

            int duration = (int)Math.Max(1.0, Math.Ceiling(challengeData.challenge.getCompletionMenaceAfterDifficulty() / challengeData.challenge.getProgressPerTurn(ua, null)));
            return (map.param.ua_flatTimeCostUtility + distance + duration) / 10.0;
        }

        public List<TaskData> getAllValidTasks(UA ua)
        {
            List<TaskData> results = new List<TaskData>();

            if (!TryGetAgentType(ua.GetType(), out AIData aiData) || aiData == null)
            {
                Console.WriteLine("CommunityLib: ERROR: Failed to Get aiData for " + ua.getName() + " of type " + ua.GetType());
                return null;
            }

            if (aiData.aiTasks == null)
            {
                return results;
            }

            setupDebugInternal(aiData.controlParameters.debugProperties);
            if (debugInternal.debug)
            {
                Console.WriteLine("CommunityLib: Agent AI for type " + ua.GetType() + " has " + aiData.aiTasks.Count + " assigned tasks.");
            }

            foreach (AITask aiTask in aiData.aiTasks)
            {
                TaskData taskData = new TaskData()
                {
                    aiTask = aiTask,
                    targetCategory = AITask.TargetCategory.None
                };

                if (debugInternal.debug && (debugInternal.outputValidity_AllTasks || debugInternal.outputValidity_ValidTasks))
                {
                    Console.WriteLine("CommunityLib: Validity for " + aiTask.taskType + " by " + ua.getName() + " (" + (ua.society?.getName() ?? "No Society)") + " at " + ua.location.getName() + " (" + (ua.location.soc?.getName() ?? "Wilderness") + ")");
                }

                bool valid = aiTask.checkTaskIsValid(taskData, ua, aiData.controlParameters);
                debugTaskValidity(valid, ua, aiTask, aiData);

                if (valid)
                {
                    switch (aiTask.targetCategory)
                    {
                        case AITask.TargetCategory.Location:
                            taskData.targetCategory = AITask.TargetCategory.Location;
                            foreach (Location location in ua.map.locations)
                            {
                                TaskData taskDataLoc = new TaskData(taskData);
                                taskDataLoc.targetLocation = location;
                                valid = aiTask.checkTaskIsValid(taskDataLoc, ua, aiData.controlParameters);

                                debugTaskValidity(valid, ua, aiTask, aiData);
                                if (valid)
                                {
                                    results.Add(taskDataLoc);
                                }
                            }
                            break;
                        case AITask.TargetCategory.SocialGroup:
                            taskData.targetCategory = AITask.TargetCategory.SocialGroup;
                            foreach (SocialGroup sg in map.socialGroups)
                            {
                                TaskData taskDataSg = new TaskData(taskData);
                                taskDataSg.targetSocialGroup = sg;
                                valid = aiTask.checkTaskIsValid(taskDataSg, ua, aiData.controlParameters);

                                debugTaskValidity(valid, ua, aiTask, aiData);
                                if (valid)
                                {
                                    results.Add(taskDataSg);
                                }
                            }
                            break;
                        case AITask.TargetCategory.Unit:
                            taskData.targetCategory = AITask.TargetCategory.Unit;
                            foreach (Unit unit in ua.map.units)
                            {
                                TaskData taskDataUnit = new TaskData(taskData);
                                taskDataUnit.targetUnit = unit;
                                valid = aiTask.checkTaskIsValid(taskDataUnit, ua, aiData.controlParameters);

                                debugTaskValidity(valid, ua, aiTask, aiData);
                                if (valid)
                                {
                                    results.Add(taskDataUnit);
                                }
                            }
                            break;
                        default:
                            results.Add(taskData);
                            break;
                    }
                }
            }

            return results;
        }

        public void debugTaskValidity(bool valid, UA ua, AITask aiTask, AIData data)
        {
            if (debugInternal.debug)
            {
                if (valid && (debugInternal.outputValidity_AllTasks || debugInternal.outputValidity_ValidTasks))
                {
                    Console.WriteLine("CommunityLib: Valid");
                }
                else if (!valid && debugInternal.outputValidity_AllTasks)
                {
                    Console.WriteLine("CommunityLib: Invalid");
                }
            }
        }

        public double checkTaskUtility(TaskData taskData, UA ua, AIData aiData, ControlParameters controlParams, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            bool intercept = false;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptAgentAI_GetTaskUtility(ua, aiData, taskData, ref utility, reasonMsgs);
                if (retValue)
                {
                    intercept = true;
                }
            }
            if (intercept)
            {
                return utility;
            }

            utility = taskData.aiTask.checkTaskUtility(taskData, ua, controlParams, reasonMsgs);

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                utility = hook.onAgentAI_GetTaskUtility(ua, aiData, taskData, utility, reasonMsgs);
            }

            return utility;
        }
    }
}
