using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Code;
using System.Linq;
using System.Diagnostics;

namespace CommunityLib
{
    public class AgentAI
    {
        public static DebugProperties debug;

        Map map;

        private Dictionary<Type, Tuple<List<AIChallenge>, ControlParameters>> ai;

        public Dictionary <UA, Dictionary<ChallengeData, Dictionary<string, double>>> randStore;

        public bool aiRunning = false;

        public struct DebugProperties
        {
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
            public bool outputUtility_ChosenAction;

            public void setOff()
            {
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
                outputUtility_ChosenAction = false;
            }

            public void setOn()
            {
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
                outputUtility_ChosenAction = true;
            }

            public static DebugProperties newOff()
            {
                DebugProperties result = new DebugProperties();
                result.setOff();
                return result;
            }

            public static DebugProperties newOn()
            {
                DebugProperties result = new DebugProperties();
                result.setOn();
                return result;
            }
        }

        public struct ControlParameters
        {
            public bool respectChallengeVisibility;
            public bool respectDanger;
            public bool respectChallengeAlignment;
            public bool valueTimeCost;

            public bool includeDangerousFoe;

            public void setDefaults()
            {
                respectChallengeVisibility = false;
                respectDanger = true;
                respectChallengeAlignment = false;
                valueTimeCost = false;

                includeDangerousFoe = true;
            }

            /// <summary>
            /// Returns a new instance of InputParams with the following default values: respectChallengeAlignment = false; respectUnitVisibility = false; respectDanger = true; respectChallengeAlignment = false; valueTimeCost = false;
            /// </summary>
            /// <returns></returns>
            public static ControlParameters newDefault()
            {
                ControlParameters result = new ControlParameters();
                result.setDefaults();
                return result;
            }
        }

        public struct ChallengeData
        {
            public AIChallenge aiChallenge;
            public Challenge challenge;
            public Location location;
        }

        public AgentAI(Map map)
        {
            debug = DebugProperties.newOff();
            this.map = map;

            ai = new Dictionary<Type, Tuple<List<AIChallenge>, ControlParameters>>();

            randStore = new Dictionary<UA, Dictionary<ChallengeData, Dictionary<string, double>>>();
        }

        /// <summary>
        /// Registers an agent type t to be controlled by the AgentAI. Returns false if the agent type t is not a subtype of UA, or it has already been registered.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type t, ControlParameters control)
        {
            if (!t.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (ai.ContainsKey(t))
            {
                if (ai[t] == null)
                {
                    ai[t] = new Tuple<List<AIChallenge>, ControlParameters>(new List<AIChallenge>(), control);
                    return true;
                }
            }
            else
            {
                ai.Add(t, new Tuple<List<AIChallenge>, ControlParameters>(new List<AIChallenge>(), control));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an agent type t has been registered. If it has, it returns true.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool TryGetAgentType(Type t)
        {
            if (!t.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (ai.ContainsKey(t))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an agent type t has been registered. If it has, it provides the list of AIChallenges that have been assigned to it and the ControlParameters. If it has not, it returns false and both the returned list and ControlParameters are null.
        /// <para>This is the primary means of altering AIChallnges that were assigned to the agent type's AI by other mods, including those that mimic the base game that were added by the Community Library itself.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenges"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool TryGetAgentType(Type t, out List<AIChallenge> aiChallenges, out ControlParameters? control)
        {
            aiChallenges = null;
            control = null;
            if (!t.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (ai.ContainsKey(t))
            {
                aiChallenges = ai[t].Item1;
                control = ai[t].Item2;
                return true;
            }

            return false;
        }

        /// <summary>
        /// IMPORTANT! Use TryGetAgentType and modify the AIChallenges registered to it instead of using this function. Removing an agent type added by another mod is irreperable, and will prevent their agent from operating.
        /// <para>Checks if an agent type t has been registers, and removes it if it has. Ouputs the list of AIChallnges that were assigned to it.</para>para>
        /// <para>If you Deregister an Agent that was registered by another mod, the AI for that agent type will not operate. Only do this if you are impementing an alternative AI for it, and cannot achieve the desired result by modifying the AiChallnges.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenges"></param>
        /// <returns></returns>
        public bool DeregisterAgentType(Type t, out List<AIChallenge> aiChallenges, out ControlParameters? control)
        {
            bool result = TryGetAgentType(t, out aiChallenges, out control);

            if (result)
            {
                result = ai.Remove(t);
            }

            return result;
        }

        /// <summary>
        /// Assigns an AIChallenge to the agent AI for agent type t. If the agent type has not been registered, or an AIChallenge has already been assigned for that challenge type, it returns false;
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool AddChallengeToAgentType(Type t, AIChallenge aiChallenge)
        {
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges, out _);

            if (result && !aiChallenges.Contains(aiChallenge))
            {
                aiChallenges.Add(aiChallenge);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Assigns a list of AIChallenges to the agent AI for agent type t. If the agent type has not been registered, or an AIChallenge has already been assigned for that challenge type for ALL AIChallenges in the list, it returns false;
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newAIChallenges"></param>
        /// <returns></returns>
        public bool AddChallengesToAgentType(Type t, List<AIChallenge> newAIChallenges)
        {
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges, out _);

            if (result)
            {
                foreach (AIChallenge aiChallenge in newAIChallenges)
                {
                    if (!aiChallenges.Contains(aiChallenge))
                    {
                        aiChallenges.Add(aiChallenge);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Assigns an array of AIChallenges to the agent AI for agent type t. If the agent type has not been registered, or an AIChallenge has already been assigned for that challenge type for ALL AIChallenges in the array, it returns false;
        /// </summary>
        /// <param name="t"></param>
        /// <param name="newAIChallenges"></param>
        /// <returns></returns>
        public bool AddChallengesToAgentType(Type t, AIChallenge[] newAIChallenges)
        {
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges, out _);

            if (result)
            {
                foreach (AIChallenge aiChallenge in newAIChallenges)
                {
                    if (!aiChallenges.Contains(aiChallenge))
                    {
                        aiChallenges.Add(aiChallenge);
                        result = true;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if agent type t has been registered and, if it has, returns the instance of AIChallenge where 'AIChallenge.challengeType' is challenge type c. If the agent type t has not been registered or no AIChallenge has been asigned to it with challengeType c, it returns null.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public AIChallenge GetAIChallengeFromAgentType(Type t, Type c)
        {
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges, out _);

            if (result)
            {
                foreach (AIChallenge aiChallenge in aiChallenges)
                {
                    if (aiChallenge.challengeType == c)
                    {
                        return aiChallenge;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// IMPORTANT! Add the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags instead of using this function. Removing an AIChallenge added by another mod is irreperable, and will prevent their agent from performing the challnge indefinately.
        /// <para>Checks if an agent type t has an AIChallnge assigned to it, and removes it if it has. It returns true if it has removed the AIChallenge</para>
        /// <para>If you remove an AIChallenge that was registered by another mod, you may not be bale to re-add it if needed at a later time. Adding the 'ChallengeTags.Forbidden' to the AIChallenge's tags will prevent the agent AI from performing the challenge.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool RemoveChallengeFromAgentType(Type t, AIChallenge aiChallenge)
        {
            if (aiChallenge == null)
            {
                return false;
            }

            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges, out _);

            if (result)
            {
                return aiChallenges.Remove(aiChallenge);
            }

            return false;
        }

        /// <summary>
        /// IMPORTANT! Add the 'ChallengeTags.Forbidden' tag to the AIChallenge's tags instead of using this function. Removing an AIChallenge added by another mod is irreperable, and will prevent their agent from performing the challnge indefinately.
        /// <para>Checks if an agent type t has an AIChallenge assigned to it where 'AIChallenge.challengeType' is challenge type c, and removes it if it has. It returns true if it has removed the AIChallenge.</para>
        /// <para>If you remove an AIChallenge that was registered by another mod, you may not be bale to re-add it if needed at a later time. Adding the 'ChallengeTags.Forbidden' to the AIChallenge's tags will prevent the agent AI from performing the challenge.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenge"></param>
        /// <returns></returns>
        public bool RemoveChallengeFromAgentType(Type t, Type c)
        {
            if (c == null || !c.IsSubclassOf(typeof(Challenge)))
            {
                return false;
            }

            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallengesAll, out _);

            if (result)
            {
                AIChallenge challenge = GetAIChallengeFromAgentType(t, c);
                if (challenge != null)
                {
                    return aiChallengesAll.Remove(challenge);
                }
            }

            return false;
        }

        /// <summary>
        /// Safely checks for a value in randStore. If none exists, it sets the value to the new value.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="challengeData"></param>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public double tryGetRand(UA ua, ChallengeData challengeData, string key, double newValue)
        {
            if (ua == null || key == null)
            {
                return -1.0;
            }

            if (!randStore.ContainsKey(ua))
            {
                randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!randStore[ua].ContainsKey(challengeData))
            {
                randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!randStore[ua][challengeData].ContainsKey(key))
            {
                randStore[ua][challengeData].Add(key, newValue);
            }

            return randStore[ua][challengeData][key];
        }

        /// <summary>
        /// Safely sets the value to randStore.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="challengeData"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void setRand(UA ua, ChallengeData challengeData, string key, double value)
        {
            if (!randStore.ContainsKey(ua))
            {
                randStore.Add(ua, new Dictionary<AgentAI.ChallengeData, Dictionary<string, double>>());
            }
            if (!randStore[ua].ContainsKey(challengeData))
            {
                randStore[ua].Add(challengeData, new Dictionary<string, double>());
            }
            if (!randStore[ua][challengeData].ContainsKey(key))
            {
                randStore[ua][challengeData].Add(key, value);
            }
            else
            {
                randStore[ua][challengeData][key] = value;
            }
        }

        public void onTurnTickAI(UA ua)
        {
            if (ua == null)
            {
                return;
            }

            bool gotAgentType = TryGetAgentType(ua.GetType(), out List<AIChallenge> aiChallenges, out ControlParameters? control);

            if (!gotAgentType || aiChallenges == null || control == null)
            {
                return;
            }
            ControlParameters controlParams = (ControlParameters)control;
            
            List<ChallengeData> validChallengeData = getAllValidChallengesAndRituals(ua);

            bool result = false;
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook.interceptAgentAI(ua, validChallengeData, controlParams);
                if (retValue)
                {
                    result = true;
                }
            }
            if (result)
            {
                return;
            }

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook.onAgentAI_StartOfProcess(ua, validChallengeData, controlParams);
            }

            if (ua.isCommandable() && map.automatic)
            {
                map.overmind.autoAI.UAAI(ua);
            }
            else
            {
                aiRunning = true;
                if (!randStore.ContainsKey(ua))
                {
                    randStore.Add(ua, new Dictionary<ChallengeData, Dictionary<string, double>>());
                }

                double utility = 0.01;
                double utility2;
                List<ChallengeData> targetChallenges = new List<ChallengeData>();
                List<Unit> targetUnits = new List<Unit>();
                List<Unit> targetGuards = new List<Unit>();
                List<UA> targetDisrupts = new List<UA>();

                if (aiChallenges.Count > 0)
                {
                    foreach (ChallengeData cData in validChallengeData)
                    {
                        if (!randStore[ua].ContainsKey(cData))
                        {
                            randStore[ua].Add(cData, new Dictionary<string, double>());
                        }

                        List<ReasonMsg> reasonMsgs = null;
                        if (debug.outputUtility_ValidChallenges)
                        {
                            reasonMsgs = new List<ReasonMsg>();
                        }

                        utility2 = getChallengeUtility(cData, ua, controlParams);

                        if (debug.outputUtility_ValidChallenges && reasonMsgs != null)
                        {
                            Console.WriteLine("CommunityLib: Utility for " + cData.challenge.getName() + " at " + cData.challenge.location.getName() + " (" + (cData.challenge.location.soc?.getName() ?? "Wilderness") + ")");
                            foreach (ReasonMsg reasonMsg in reasonMsgs)
                            {
                                Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                            }
                            Console.WriteLine("CommunityLib: Total: " + utility2);
                        }

                        if (utility2 > utility)
                        {
                            utility = utility2;
                            targetChallenges.Clear();
                            targetChallenges.Add(cData);
                        }
                        else if (utility2 == utility)
                        {
                            targetChallenges.Add(cData);
                        }
                    }
                }

                List<Unit> visibleUnits = ua.getVisibleUnits();
                if (visibleUnits?.Count > 0)
                {
                    foreach (Unit unit in visibleUnits)
                    {
                        UA agent = unit as UA;
                        if (agent != null && !agent.isDead)
                        {
                            if (debug.outputUtility_VisibleAgentsAttack || debug.outputUtility_VisibleAgentsBodyguard || debug.outputUtility_VisibleAgentsDisrupt)
                            {
                                Console.WriteLine("CommunityLib: Unit " + unit.getName() + " (" + (unit.society?.getName() ?? "No Society") + ") at " + unit.location.getName() + " (" + (unit.location.soc?.getName() ?? "No Society") + ")");
                            }

                            if (debug.outputProfile_VisibleAgents)
                            {
                                Console.WriteLine("CommunityLib: Profile: " + agent.profile);
                            }

                            List<ReasonMsg> reasonMsgs = null;
                            if (!(agent.task is Task_InHiding))
                            {
                                if (debug.outputUtility_VisibleAgentsAttack)
                                {
                                    reasonMsgs = new List<ReasonMsg>();
                                }

                                utility2 = ua.getAttackUtility(agent, reasonMsgs, controlParams.includeDangerousFoe);

                                if (debug.outputUtility_VisibleAgentsAttack && reasonMsgs != null)
                                {
                                    Console.WriteLine("CommunityLib: Attack Utility");
                                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                                    {
                                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                    }
                                    Console.WriteLine("CommunityLib: Total: " + utility2);
                                }

                                if (utility2 > utility)
                                {
                                    utility = utility2;
                                    targetChallenges.Clear();
                                    targetUnits.Clear();
                                    targetUnits.Add(unit);
                                }
                                else if (utility2 == utility)
                                {
                                    targetChallenges.Clear();
                                    targetUnits.Add(unit);
                                }
                            }

                            reasonMsgs = null;
                            if (ua != map.awarenessManager.getChosenOne())
                            {
                                if (ua.society?.getRel(unit.society).state != DipRel.dipState.war)
                                {
                                    if (debug.outputUtility_VisibleAgentsBodyguard)
                                    {
                                        reasonMsgs = new List<ReasonMsg>();
                                    }

                                    utility2 = ua.getBodyguardUtility(unit, null);

                                    if (debug.outputUtility_VisibleAgentsBodyguard && reasonMsgs != null)
                                    {
                                        Console.WriteLine("CommunityLib: Bodyguard Utility");
                                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                                        {
                                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                        }
                                        Console.WriteLine("CommunityLib: Total: " + utility2);
                                    }

                                    if (utility2 > utility)
                                    {
                                        utility = utility2;
                                        targetChallenges.Clear();
                                        targetUnits.Clear();
                                        targetGuards.Clear();
                                        targetGuards.Add(unit);
                                    }
                                    else if (utility2 == utility)
                                    {
                                        targetChallenges.Clear();
                                        targetUnits.Clear();
                                        targetGuards.Add(unit);

                                    }
                                }
                            }

                            reasonMsgs = null;
                            Task_PerformChallenge task = unit.task as Task_PerformChallenge;
                            if (!(task?.challenge.isChannelled() ?? true))
                            {
                                if (debug.outputUtility_VisibleAgentsDisrupt)
                                {
                                    reasonMsgs = new List<ReasonMsg>();
                                }

                                utility2 = ua.getDisruptUtility(unit, null);

                                if (debug.outputUtility_VisibleAgentsDisrupt && reasonMsgs != null)
                                {
                                    Console.WriteLine("CommunityLib: Disrupt Utility");
                                    foreach (ReasonMsg reasonMsg in reasonMsgs)
                                    {
                                        Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                    }
                                    Console.WriteLine("CommunityLib: Total: " + utility2);
                                }

                                if (utility2 > utility)
                                {
                                    utility = utility2;
                                    targetChallenges.Clear();
                                    targetUnits.Clear();
                                    targetGuards.Clear();
                                    targetDisrupts.Clear();
                                    targetDisrupts.Add(agent);
                                }
                                else if (utility2 == utility)
                                {
                                    targetChallenges.Clear();
                                    targetUnits.Clear();
                                    targetGuards.Clear();
                                    targetDisrupts.Add(agent);
                                }
                            }
                        }
                    }
                }

                if (targetUnits.Count > 0)
                {
                    Unit targetUnit = targetUnits[0];
                    if (targetUnits.Count > 1)
                    {
                        targetUnit = targetUnits[Eleven.random.Next(targetUnits.Count)];
                    }

                    if (debug.outputUtility_ChosenAction)
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

                    if (debug.outputUtility_ChosenAction)
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

                    if (debug.outputUtility_ChosenAction)
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

                    if (debug.outputUtility_ChosenAction)
                    {
                        Console.WriteLine("CommunityLib: " + ua.getName() + " is going to perform challenge " + targetChallenge.challenge.getName() + " at " + targetChallenge.location.getName() + " (" + (targetChallenge.location.soc?.getName() ?? "No Society") + ")");

                        List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                        getChallengeUtility(targetChallenge, ua, controlParams, reasonMsgs);

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
                    if (!targetChallenge.challenge.allowMultipleUsers())
                    {
                        targetChallenge.challenge.claimedBy = ua;
                    }

                    bool safeMove = targetChallenge.aiChallenge.safeMove;

                    ua.task = new Task_GoToPerformChallengeAtLocation(targetChallenge.challenge, targetChallenge.location, safeMove);
                }

                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    hook.onAgentAI_EndOfProcess(ua, validChallengeData, controlParams);
                }
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

            if (!TryGetAgentType(ua.GetType(), out List<AIChallenge> aiChallengesAll, out ControlParameters? control) || aiChallengesAll == null || control == null)
            {
                return null;
            }
            ControlParameters controlParams = (ControlParameters)control;

            // Sort all aiChallenges into type-keyed dictionaries for faster searching.
            Dictionary<Type, AIChallenge>  aiChallenges = new Dictionary<Type, AIChallenge>();
            Dictionary<Type, AIChallenge>  aiRituals = new Dictionary<Type, AIChallenge>();
            if (aiChallengesAll.Count > 0)
            {
                foreach (AIChallenge aiChallenge in aiChallengesAll)
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
            if (aiRituals.Count > 0)
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
                    if (aiRituals.ContainsKey(ritual.GetType()))
                    {
                        //Console.WriteLine("CommunityLib: Found ritual " + ritual.getName());
                        ChallengeData d = new ChallengeData();
                        d.aiChallenge = aiRituals[ritual.GetType()];
                        d.challenge = ritual;
                        ritualData.Add(d);
                    }
                }
            }

            if (aiChallenges.Count > 0 || aiRituals.Count > 0)
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

                        if (aiChallenges.ContainsKey(challenge.GetType()))
                        {
                            ChallengeData d = new ChallengeData();
                            d.aiChallenge = aiChallenges[challenge.GetType()];
                            d.challenge = challenge;
                            d.location = location;
                            if (getChallengeIsValid(ua, d, controlParams))
                            {
                                result.Add(d);
                            }
                        }
                    }

                    foreach (ChallengeData rData in ritualData)
                    {
                        ChallengeData d = new ChallengeData();
                        d.aiChallenge = rData.aiChallenge;
                        d.challenge = rData.challenge;
                        d.location = location;
                        if (getChallengeIsValid(ua, d, controlParams))
                        {
                            result.Add(d);
                        }
                    }
                }
            }

            return result;
        }

        public bool getChallengeIsValid(UA ua, ChallengeData challengeData, ControlParameters controlParams)
        {
            if (challengeData.challenge.claimedBy?.isDead ?? false)
            {
                challengeData.challenge.claimedBy = null;
            }

            if (debug.outputProfile_AllChallenges || debug.outputVisibility_AllChallenges || debug.outputValidity_AllChallenges || debug.outputValidity_ValidChallenges)
            {
                Console.WriteLine("CommunityLib: Visibility and Validity for " + challengeData.challenge.getName() + " at " + challengeData.location.getName() + " (" + (challengeData.location.soc?.getName() ?? "Wilderness") + ") by " + ua.getName() + " (" + (ua.society?.getName() ?? "No Society") + ")");
            }

            if (!controlParams.respectChallengeVisibility || challengeData.aiChallenge.checkChallengeVisibility(challengeData, ua))
            {
                if (!controlParams.respectChallengeAlignment || !(challengeData.challenge.isGoodTernary() == -1 && ua is UAG && !ua.corrupted))
                {
                    if (!controlParams.respectChallengeAlignment || !(challengeData.challenge.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                    {
                        if (challengeData.challenge.allowMultipleUsers() || challengeData.challenge.claimedBy == null || challengeData.challenge.claimedBy == ua)
                        {
                            if (challengeData.aiChallenge.checkChallengeIsValid(challengeData, ua))
                            {
                                return true;
                            }
                        }
                        else if (debug.outputValidity_AllChallenges)
                        {
                            Console.WriteLine("CommunityLib: Invalid: Challenge Claimed by Other Agent");
                        }
                    }
                    else if (debug.outputValidity_AllChallenges)
                    {
                        Console.WriteLine("CommunityLib: Invalid: Evil Agent Vs Good Challenge");
                    }
                }
                else if (debug.outputValidity_AllChallenges)
                {
                    Console.WriteLine("CommunityLib: Invalid: Good Agent Vs Evil Challenge");
                }
            }

            return false;
        }

        public double getChallengeUtility(ChallengeData challengeData, UA ua, ControlParameters controlParams, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            utility += ua.person.getTagUtility(challengeData.challenge.getPositiveTags(), challengeData.challenge.getNegativeTags(), reasonMsgs);

            if (controlParams.respectDanger)
            {
                double danger = getDangerUtility(challengeData, ua);
                utility += danger;
            }

            utility += challengeData.aiChallenge.checkChallengeUtility(challengeData, ua, reasonMsgs);

            if (controlParams.valueTimeCost)
            {
                //Console.WriteLine("CommunityLib: distance Divisor is " + getDistanceDivisor(challengeData, ua).ToString());
                utility /= getDistanceDivisor(challengeData, ua);
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

        private double getDistanceDivisor(ChallengeData challengeData, UA ua)
        {
            double distance = map.getStepDist(ua.location, challengeData.location) / ua.getMaxMoves();
            distance = Math.Ceiling(distance);
            int duration = (int)Math.Max(1.0, Math.Ceiling(challengeData.challenge.getCompletionMenaceAfterDifficulty() / challengeData.challenge.getProgressPerTurn(ua, null)));
            return (map.param.ua_flatTimeCostUtility + distance + duration) / 10.0;
        }
    }
}
