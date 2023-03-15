using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Code;
using System.Linq;

namespace CommunityLib
{
    public class AgentAI
    {
        public static DebugProperties debug;

        ModCore mod;

        Map map;

        private Dictionary<Type, List<AIChallenge>> ai;

        public struct DebugProperties
        {
            public bool outputProfile_AllChallenges;
            public bool outputProfile_VisibleChallenges;
            public bool outputProfile_AllAgents;
            public bool outputProfile_VisibleAgents;

            public bool outputVisibility_AllChallenges;
            public bool outputVisibility_VisibleChallenges;
            public bool outputVisibility_AllAgents;
            public bool outputVisibility_VisibleAgents;

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
                outputProfile_AllAgents = false;
                outputVisibility_VisibleAgents = false;
                outputVisibility_AllChallenges = false;
                outputVisibility_VisibleChallenges = false;
                outputValidity_AllChallenges = false;
                outputVisibility_AllAgents = false;
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
                outputProfile_AllAgents = true;
                outputVisibility_VisibleAgents = true;
                outputVisibility_AllChallenges = true;
                outputVisibility_VisibleChallenges = true;
                outputVisibility_AllAgents = true;
                outputVisibility_VisibleAgents = true;
                outputValidity_AllChallenges = true;
                outputValidity_ValidChallenges = true;
                outputUtility_ValidChallenges = true;
                outputUtility_VisibleAgentsAttack = true;
                outputUtility_VisibleAgentsBodyguard = true;
                outputUtility_VisibleAgentsDisrupt = true;
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

        public struct InputParams
        {
            public bool respectChallengeVisibility;
            public bool respectUnitVisibility;
            public bool respectDanger;
            public bool respectChallengeAlignment;
            public bool valueTimeCost;

            public void setDefaults()
            {
                respectChallengeVisibility = false;
                respectUnitVisibility = false;
                respectDanger = true;
                respectChallengeAlignment = false;
                valueTimeCost = false;
            }

            /// <summary>
            /// Returns a new instance of InputParams with the following default values: respectChallengeAlignment = false; respectUnitVisibility = false; respectDanger = true; respectChallengeAlignment = false; valueTimeCost = false;
            /// </summary>
            /// <returns></returns>
            public static InputParams newDefault()
            {
                InputParams result = new InputParams();
                result.setDefaults();
                return result;
            }
        }

        public AgentAI(ModCore core, Map map)
        {
            debug = DebugProperties.newOff();
            mod = core;
            this.map = map;

            ai = new Dictionary<Type, List<AIChallenge>>();
        }

        /// <summary>
        /// Registers an agent type t to be controlled by the AgentAI. Returns false if the agent type t is not a subtype of UA, or it has already been registered.
        /// <para>NOTE: Registering the AI does not run it. You still need to call 'AgentAI.turnTickAI' from the agent type's 'onTurnTickAI', or in the 'onAgentAIDecision' hook in your ModKernel if overriding an existing built-in Agent AI. This is neccesary both to ensure that the AI is called appropriately, and so that you can control the paramters for the 'AgentAI.turnTickAI' function.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool RegisterAgentType(Type t)
        {
            if (!t.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (ai.ContainsKey(t))
            {
                if (ai[t] == null)
                {
                    ai[t] = new List<AIChallenge>();
                    return true;
                }
            }
            else
            {
                ai.Add(t, new List<AIChallenge>());
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if an agent type t has been registered. If it has, it provides the list of AIChallenges that have been assigned to it. If it has not, it returns false and the returned list is null.
        /// <para>This is the primary means of altering AIChallnges that were assigned to the agent type's AI by other mods, including those that mimic the base game that were added by the Community Library itself.</para>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="aiChallenges"></param>
        /// <returns></returns>
        public bool TryGetAgentType(Type t, out List<AIChallenge> aiChallenges)
        {
            aiChallenges = null;
            if (!t.IsSubclassOf(typeof(UA)))
            {
                return false;
            }

            if (ai.ContainsKey(t))
            {
                aiChallenges = ai[t];
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
        public bool DeregisterAgentType(Type t, out List<AIChallenge> aiChallenges)
        {
            bool result = TryGetAgentType(t, out aiChallenges);

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
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

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
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

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
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

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
            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

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

            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

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

            bool result = TryGetAgentType(t, out List<AIChallenge> aiChallenges);

            if (result)
            {
                AIChallenge challenge = GetAIChallengeFromAgentType(t, c);
                if (challenge != null)
                {
                    return aiChallenges.Remove(challenge);
                }
            }

            return false;
        }

        public void onTurnTickAI(UA ua, InputParams inputParams)
        {
            if (ua == null)
            {
                return;
            }

            List<AIChallenge> aiChallenges;
            bool gotAgentType = TryGetAgentType(ua.GetType(), out aiChallenges);

            if (!gotAgentType || aiChallenges == null)
            {
                return;
            }

            // Seperates aiChallenges between challenges and rituals.
            Dictionary<Type, AIChallenge> aiChallengesFiltered;
            Dictionary<Type, AIChallenge> aiRituals;
            filterAIChallengeAndRituals(aiChallenges, out aiChallengesFiltered, out aiRituals);

            bool result = false;
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                bool retValue = hook.interceptAgentAI(ua, aiChallengesFiltered.Values.ToList(), aiRituals.Values.ToList(), inputParams);
                if (retValue)
                {
                    result = true;
                }
            }
            if (result)
            {
                return;
            }

            if (ua.isCommandable() && map.automatic)
            {
                map.overmind.autoAI.UAAI(ua);
            }
            else
            {
                List<Challenge> challenges = null;
                Dictionary<Location, List<Challenge>> ritualData = null;

                double utility = -double.MaxValue;
                double utility2;
                List<Challenge> targetChallenges = new List<Challenge>();
                List<Unit> targetUnits = new List<Unit>();
                List<Unit> targetGuards = new List<Unit>();
                List<UA> targetDisrupts = new List<UA>();
                List<Location> targetLocations = new List<Location>();
                AIChallenge aiChallenge;

                if (aiChallenges.Count > 0)
                {
                    // Process aiChallenges
                    getAllValidChallengesAndRituals(ua, aiChallengesFiltered, aiRituals, out challenges, out ritualData, inputParams);

                    foreach (Challenge challenge in challenges)
                    {
                        aiChallenge = aiChallengesFiltered[challenge.GetType()];

                        List<ReasonMsg> reasonMsgs = null;
                        if (debug.outputUtility_ValidChallenges)
                        {
                            reasonMsgs = new List<ReasonMsg>();
                        }

                        utility2 = getChallengeUtility(challenge, aiChallenge, ua, challenge.location, inputParams);

                        if (reasonMsgs != null)
                        {
                            Console.WriteLine("CommunityLib: Utility for " + challenge.getName() + " at " + challenge.location.getName() + " (" + (challenge.location.soc?.getName() ?? "Wilderness") + ")");
                            foreach (ReasonMsg reasonMsg in reasonMsgs)
                            {
                                Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                            }
                            Console.WriteLine("CommunityLib: Total: " + utility2);
                        }

                        if (targetChallenges.Count == 0 || utility2 > utility)
                        {
                            utility = utility2;
                            targetChallenges.Clear();
                            targetChallenges.Add(challenge);
                            targetLocations.Clear();
                            targetLocations.Add(challenge.location);
                        }
                        else if (utility2 == utility)
                        {
                            targetChallenges.Add(challenge);
                            targetLocations.Add(challenge.location);
                        }
                    }

                    foreach (KeyValuePair<Location, List<Challenge>> pair in ritualData)
                    {
                        foreach (Challenge ritual in pair.Value)
                        {
                            aiChallenge = aiRituals[ritual.GetType()];

                            List<ReasonMsg> reasonMsgs = null;
                            if (debug.outputUtility_ValidChallenges)
                            {
                                reasonMsgs = new List<ReasonMsg>();
                            }

                            utility2 = getChallengeUtility(ritual, aiChallenge, ua, pair.Key, inputParams);

                            if (reasonMsgs != null)
                            {
                                Console.WriteLine("CommunityLib: Utility for " + ritual.getName() + " at " + pair.Key.getName() + " (" + (pair.Key.soc?.getName() ?? "Wilderness") + ")");
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
                                targetChallenges.Add(ritual);
                                targetLocations.Clear();
                                targetLocations.Add(pair.Key);
                            }
                            else if (utility2 == utility)
                            {
                                targetChallenges.Add(ritual);
                                targetLocations.Add(pair.Key);
                            }
                        }
                    }
                }

                foreach (Unit unit in map.units)
                {
                    UA agent = unit as UA;
                    if (agent != null && !agent.isDead)
                    {
                        if (debug.outputProfile_AllAgents || debug.outputVisibility_AllAgents || debug.outputUtility_VisibleAgentsAttack || debug.outputUtility_VisibleAgentsBodyguard || debug.outputUtility_VisibleAgentsDisrupt)
                        {
                            Console.WriteLine("CommunityLib: Unit " + unit.getName() + " (" + (unit.society?.getName() ?? "No Society") + ") at " + unit.location.getName() + " (" + (unit.location.soc?.getName() ?? "No Society") + ")");
                        }

                        bool visible = (agent.profile / 10) > map.getStepDist(ua.location, agent.location);
                        if (!inputParams.respectUnitVisibility || visible)
                        {
                            if (debug.outputProfile_AllAgents)
                            {
                                Console.WriteLine("CommunityLib: Profile: " + agent.profile);
                            }
                            else if (debug.outputProfile_VisibleAgents && visible)
                            {
                                Console.WriteLine("CommunityLib: Profile: " + agent.profile);
                            }
                            if (debug.outputVisibility_AllAgents)
                            {
                                if (visible)
                                {
                                    Console.WriteLine("CommunityLib: Visible");
                                }
                                else
                                {
                                    Console.WriteLine("CommunityLib: NOT Visible");
                                }
                            }
                            else if (debug.outputVisibility_VisibleAgents && visible)
                            {
                                Console.WriteLine("CommunityLib: Visible");
                            }

                            List<ReasonMsg> reasonMsgs = null;
                            if (!(agent.task is Task_InHiding))
                            {
                                if (debug.outputUtility_VisibleAgentsAttack)
                                {
                                    reasonMsgs = new List<ReasonMsg>();
                                }

                                utility2 = ua.getAttackUtility(agent, reasonMsgs, true);

                                if (reasonMsgs != null)
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
                                    targetLocations.Clear();
                                }
                                else if (utility2 == utility)
                                {
                                    targetChallenges.Clear();
                                    targetUnits.Add(unit);
                                    targetLocations.Clear();
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

                                    if (reasonMsgs != null)
                                    {
                                        Console.WriteLine("CommunityLib: Bodyguard Utility");
                                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                                        {
                                            Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                        }
                                        Console.WriteLine("CommunityLib: Total: " + utility2);
                                    }

                                    if (reasonMsgs != null)
                                    {
                                        foreach (ReasonMsg reasonMsg in reasonMsgs)
                                        {
                                            Console.WriteLine(reasonMsg.msg + ": " + reasonMsg.value);
                                        }
                                    }

                                    if (utility2 > utility)
                                    {
                                        utility = utility2;
                                        targetChallenges.Clear();
                                        targetUnits.Clear();
                                        targetGuards.Clear();
                                        targetGuards.Add(unit);
                                        targetLocations.Clear();
                                    }
                                    else if (utility2 == utility)
                                    {
                                        targetChallenges.Clear();
                                        targetUnits.Clear();
                                        targetGuards.Add(unit);
                                        targetLocations.Clear();

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

                                if (reasonMsgs != null)
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
                                    targetLocations.Clear();
                                    targetLocations.Clear();
                                    targetLocations.Clear();
                                    targetDisrupts.Clear();
                                    targetDisrupts.Add(agent);
                                    targetLocations.Clear();
                                }
                                else if (utility2 == utility)
                                {
                                    targetLocations.Clear();
                                    targetLocations.Clear();
                                    targetLocations.Clear();
                                    targetDisrupts.Add(agent);
                                    targetLocations.Clear();
                                }
                            }
                        }
                        else
                        {

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
                else if (targetChallenges.Count > 0 && targetChallenges.Count == targetLocations.Count)
                {
                    Challenge targetChallenge = targetChallenges[0];
                    Location targetLocation = targetLocations[0];

                    if (targetChallenges.Count > 1)
                    {
                        int index = Eleven.random.Next(targetChallenges.Count);
                        targetChallenge = targetChallenges[index];
                        targetLocation = targetLocations[index];
                    }

                    if (debug.outputUtility_ChosenAction)
                    {
                        Console.WriteLine("CommunityLib: " + ua.getName() + " is going to perform challenge " + targetChallenge.getName() + " at " + targetLocation.getName() + " (" + (targetLocation.soc?.getName() ?? "No Society") + ")");

                        List<ReasonMsg> reasonMsgs = new List<ReasonMsg>();
                        AIChallenge targetAIChalenge = aiChallenges.FirstOrDefault(aiC => aiC.challengeType == targetChallenge.GetType());
                        getChallengeUtility(targetChallenge, targetAIChalenge, ua, targetLocation, inputParams, reasonMsgs);

                        if (!debug.outputUtility_ValidChallenges)
                        {
                            {
                                foreach (ReasonMsg reasonMsg in reasonMsgs)
                                {
                                    Console.WriteLine("CommunityLib: " + reasonMsg.msg + ": " + reasonMsg.value);
                                }
                                Console.WriteLine("CommunityLib: Total: " + utility);
                            }
                        }
                    }

                    if (ua.person.isWatched() || targetLocation.isWatched)
                    {
                        map.addUnifiedMessage(ua, null, "Beginning Quest", ua.getName() + " is beginning quest " + targetChallenge.getName() + " at location " + targetLocation.getName(true), UnifiedMessage.messageType.BEGINNING_QUEST);
                    }
                    targetChallenge.claimedBy = ua;

                    bool safeMove;
                    if (targetChallenge is Ritual)
                    {
                        safeMove = aiRituals[targetChallenge.GetType()].safeMove;
                    }
                    else
                    {
                        safeMove = aiChallengesFiltered[targetChallenge.GetType()].safeMove;
                    }

                    ua.task = new Task_GoToPerformChallengeAtLocation(targetChallenge, targetLocation, safeMove);
                }

                foreach (Hooks hook in mod.GetRegisteredHooks())
                {
                    hook.onAgentAI_EndOfProcess(ua, challenges, ritualData, inputParams);
                }
            }

            if (ua.task == null && ua.location.index != ua.homeLocation)
            {
                ua.task = new Task_GoToLocation(map.locations[ua.homeLocation]);
            }
        }

        public void filterAIChallengeAndRituals(List<AIChallenge> aiChallenges, out Dictionary<Type, AIChallenge> aiChallengesFiltered, out Dictionary<Type, AIChallenge> aiRituals)
        {
            aiChallengesFiltered = new Dictionary<Type, AIChallenge>();
            aiRituals = new Dictionary<Type, AIChallenge>();
            if (aiChallenges.Count > 0)
            {
                foreach (AIChallenge aiChallenge in aiChallenges)
                {
                    if (aiChallenge.isRitual)
                    {
                        aiRituals.Add(aiChallenge.challengeType, aiChallenge);
                    }
                    else
                    {
                        aiChallengesFiltered.Add(aiChallenge.challengeType, aiChallenge);
                    }
                }
            }
        }

        public void getAllValidChallengesAndRituals(UA ua, Dictionary<Type, AIChallenge> aiChallenges, Dictionary<Type, AIChallenge> aiRituals, out List<Challenge> challenges, out Dictionary<Location, List<Challenge>> ritualData, InputParams inputParams)
        {
            challenges = new List<Challenge>();
            List<Challenge> rituals = new List<Challenge>();
            ritualData = new Dictionary<Location, List<Challenge>>();

            // Gets all rituals attached to the UA, directly and indirectly.
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
                        rituals.Add(ritual);
                    }
                }
            }

            // Iterate over all map locations, gather valid challenges, and test rituals.
            foreach (Location location in map.locations)
            {
                foreach (Challenge challenge in location.GetChallenges())
                {
                    if (challenge.claimedBy?.isDead ?? false)
                    {
                        challenge.claimedBy = null;
                    }

                    if (aiChallenges.ContainsKey(challenge.GetType()))
                    {
                        if (debug.outputProfile_AllChallenges || debug.outputVisibility_AllChallenges || debug.outputValidity_AllChallenges || debug.outputValidity_ValidChallenges)
                        {
                            Console.WriteLine("CommunityLib: Visibility and Validity for " + challenge.getName() + " at " + location.getName() + " (" + (location.soc?.getName() ?? "Wilderness") + ") by " + ua.getName() + " (" + (ua.society?.getName() ?? "No Society") + ")");
                        }

                        if (!inputParams.respectChallengeVisibility || aiChallenges[challenge.GetType()].checkChallengeVisibility(challenge, ua, location))
                        {
                            if (!inputParams.respectChallengeAlignment || !(challenge.isGoodTernary() == -1 && ua is UAG && !ua.corrupted))
                            {
                                if (!inputParams.respectChallengeAlignment || !(challenge.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                                {
                                    if (challenge.allowMultipleUsers() || challenge.claimedBy == null || challenge.claimedBy == ua)
                                    {
                                        if (aiChallenges[challenge.GetType()].checkChallengeIsValid(challenge, ua, location))
                                        {
                                            challenges.Add(challenge);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach(Challenge ritual in rituals)
                {
                    if (debug.outputProfile_AllChallenges || debug.outputVisibility_AllChallenges || debug.outputValidity_AllChallenges || debug.outputValidity_ValidChallenges)
                    {
                        Console.WriteLine("CommunityLib: Visibility and Validity for " + ritual.getName() + " at " + location.getName() + " (" + (location.soc?.getName() ?? "Wilderness") + ")");
                    }

                    if (!inputParams.respectChallengeVisibility || aiChallenges[ritual.GetType()].checkChallengeVisibility(ritual, ua, location))
                    {
                        if (!inputParams.respectChallengeAlignment || !(ritual.isGoodTernary() == -1 && ua is UAG && !ua.corrupted))
                        {
                            if (!inputParams.respectChallengeAlignment || !(ritual.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                            {
                                AIChallenge aiChallenge = aiRituals[ritual.GetType()];
                                if (aiChallenge.checkChallengeIsValid(ritual, ua, location))
                                {
                                    if (!ritualData.ContainsKey(location))
                                    {
                                        ritualData.Add(location, new List<Challenge>());
                                    }
                                    else if (ritualData[location] == null)
                                    {
                                        ritualData[location] = new List<Challenge>();
                                    }

                                    if (!ritualData[location].Contains(ritual))
                                    {
                                        ritualData[location].Add(ritual);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public double getChallengeUtility(Challenge challenge, AIChallenge aiChallenge, UA ua, Location location, InputParams inputParams, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            utility += ua.person.getTagUtility(challenge.getPositiveTags(), challenge.getNegativeTags(), reasonMsgs);

            if (inputParams.respectDanger)
            {
                double danger = getDangerUtility(challenge, ua);
                utility += danger;
            }

            utility += aiChallenge.checkChallengeUtility(challenge, ua, reasonMsgs, location);

            if (inputParams.valueTimeCost)
            {
                double distanceDivisor = getDistanceDivisor(challenge, ua, location);
                reasonMsgs?.Add(new ReasonMsg("Distance Divisor", distanceDivisor));
                utility /= distanceDivisor;
            }

            return utility;
        }

        private double getDangerUtility(Challenge challenge, UA ua, List<ReasonMsg> reasonMsgs = null)
        {
            double result = 0.0;

            if (challenge.getDanger() <= 0)
            {
                return result;
            }

            result = challenge.getDanger();
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
            foreach (int tag in challenge.getPositiveTags())
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

        private double getDistanceDivisor(Challenge challenge, UA ua, Location location)
        {
            int distance = map.getStepDist(ua.location, location) / ua.getMaxMoves();
            int duration = (int)Math.Max(1.0, Math.Ceiling(challenge.getCompletionMenaceAfterDifficulty() / challenge.getProgressPerTurn(ua, null)));
            return (map.param.ua_flatTimeCostUtility + distance + duration) / 10;
        }
    }
}
