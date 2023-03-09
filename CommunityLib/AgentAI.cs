﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Code;

namespace CommunityLib
{
    public class AgentAI
    {
        ModCore mod;

        Map map;

        private Dictionary<Type, List<AIChallenge>> ai;

        public AgentAI(ModCore core, Map map)
        {
            mod = core;
            this.map = map;

            ai = new Dictionary<Type, List<AIChallenge>>();
        }

        /// <summary>
        /// Registers an agent type t to be controlled by the AgentAI and enables it. Returns false if the agent type t is not a subtype of UA, or it has already been registered and populated with challenges.
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
                }
                if (ai[t].Count == 0)
                {
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
        public bool AddChallngesToAgentType(Type t, List<AIChallenge> newAIChallenges)
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
        public bool AddChallngesToAgentType(Type t, AIChallenge[] newAIChallenges)
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

        public void onTurnTickAI(UA ua, List<AIChallenge> aiChallenges, bool respectChallengeVisibility = false, bool respectUnitVisibility = false, bool respectDanger = true, bool valueTimeCost = false)
        {
            if (ua == null)
            {
                return;
            }

            if (aiChallenges == null || aiChallenges.Count == 0)
            {
                return;
            }

            // Seperates aiChallnges between challenges and rituals.
            Dictionary<Type, AIChallenge> aiChallengesFiltered = new Dictionary<Type, AIChallenge>();
            Dictionary<Type, AIChallenge> aiRituals = new Dictionary<Type, AIChallenge>();
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

            if (ua.isCommandable() && map.automatic)
            {
                map.overmind.autoAI.UAAI(ua);
            }
            else
            {
                getAllValidChallengesAndRituals(ua, aiChallengesFiltered, aiRituals, out List<Challenge> challenges, out Dictionary<Location, List<Challenge>> ritualData);

                double utility = 0.0;
                double utility2;
                Challenge targetChallenge = null;
                Unit targetUnit = null;
                Unit targetGuard = null;
                UA targetDisrupt = null;
                Location targetLocation = null;
                AIChallenge aiChallenge;

                foreach (Challenge challenge in challenges)
                {
                    aiChallenge = aiChallengesFiltered[challenge.GetType()];
                    if (respectChallengeVisibility)
                    {
                        if (!aiChallenge.checkChallengeVisibility(challenge, ua, challenge.location))
                        {
                            continue;
                        }
                    }

                    utility2 = getChallengeUtility(challenge, aiChallenge, ua, challenge.location, respectDanger, valueTimeCost);

                    if (utility2 > utility)
                    {
                        utility = utility2;
                        targetChallenge = challenge;
                        targetLocation = challenge.location;
                    }
                }

                foreach (KeyValuePair<Location, List<Challenge>> pair in ritualData)
                {
                    foreach (Challenge ritual in pair.Value)
                    {
                        aiChallenge = aiRituals[ritual.GetType()];
                        if (respectChallengeVisibility)
                        {
                            if (!aiChallenge.checkChallengeVisibility(ritual, ua, pair.Key))
                            {
                                continue;
                            }
                        }

                        utility2 = getChallengeUtility(ritual, aiChallenge, ua, pair.Key, respectDanger, valueTimeCost);

                        if (utility2 > utility)
                        {
                            utility = utility2;
                            targetChallenge = ritual;
                            targetLocation = pair.Key;
                        }
                    }
                }

                List<Unit> units = null;

                if (respectUnitVisibility)
                {
                    if (mod.GetCache().visibleUnitsByUnit.ContainsKey(ua))
                    {
                        units = mod.GetCache().visibleUnitsByUnit[ua] as List<Unit>;
                    }
                }
                else
                {
                    if (mod.GetCache().unitsByType.ContainsKey(typeof(Unit)))
                    {
                        units = mod.GetCache().unitsByType[typeof(Unit)] as List<Unit>;
                    }
                }
                
                if (units?.Count > 0)
                {
                    foreach (Unit unit in units)
                    {
                        UA agent = unit as UA;
                        if (agent != null)
                        {
                            if (!(agent.task is Task_InHiding))
                            {
                                utility2 = ua.getAttackUtility(agent, null, true);

                                if (targetUnit == null || utility2 > utility)
                                {
                                    utility = utility2;
                                    targetChallenge = null;
                                    targetUnit = agent;
                                    targetGuard = null;
                                    targetDisrupt = null;
                                    targetLocation = null;
                                }
                            }

                            if (ua != map.awarenessManager.getChosenOne())
                            {
                                if (ua.society?.getRel(unit.society).state != DipRel.dipState.war)
                                {
                                    utility2 = ua.getBodyguardUtility(unit, null);

                                    if (targetUnit == null || utility2 > utility)
                                    {
                                        utility = utility2;
                                        targetChallenge = null;
                                        targetUnit = null;
                                        targetGuard = agent;
                                        targetDisrupt = null;
                                        targetLocation = null;
                                    }
                                }
                            }
                            Task_PerformChallenge task = unit.task as Task_PerformChallenge;
                            if (!(task?.challenge.isChannelled() ?? true))
                            {
                                utility2 = ua.getDisruptUtility(unit, null);

                                if (targetUnit == null || utility2 > utility)
                                {
                                    utility = utility2;
                                    targetChallenge = null;
                                    targetUnit = null;
                                    targetGuard = null;
                                    targetDisrupt = agent;
                                    targetLocation = null;
                                }
                            }
                        }
                    }
                }

                if (targetUnit != null)
                {
                    UA agent = targetUnit as UA;
                    if (agent != null && (ua.person.isWatched() || ua.isCommandable()))
                    {
                        map.addUnifiedMessage(ua, targetUnit, ua.person.getName() + " attacking", ua.getName() + " is moving to attack " + targetUnit.getName() + ". They will seek them out for a number of turns before losing the trail.", UnifiedMessage.messageType.HERO_ATTACKING);
                    }
                    Task_AttackUnit task = new Task_AttackUnit(ua, targetUnit);
                    ua.getAttackUtility(targetUnit, task.reasonsMessages, true);
                    ua.task = task;
                }
                else if (targetDisrupt != null)
                {
                    if (ua.person.isWatched())
                    {
                        map.addUnifiedMessage(ua, targetDisrupt, ua.person.getName() + " moving to disrupt", ua.getName() + " is moving to disrupt " + targetDisrupt.getName() + ", which will slow their challenge progress", UnifiedMessage.messageType.MOVING_TO_DISRUPT);
                    }
                    Task_DisruptUA task = new Task_DisruptUA(ua, targetDisrupt);
                    ua.getDisruptUtility(targetDisrupt, task.reasons);
                    ua.task = task;
                }
                else if (targetGuard != null)
                {
                    if (ua.person.isWatched())
                    {
                        map.addUnifiedMessage(ua, targetGuard, ua.person.getName() + " moving to guard", ua.getName() + " is moving to protect " + targetGuard.getName() + " from any attackers", UnifiedMessage.messageType.MOVING_TO_GUARD);
                    }
                    Task_Bodyguard task = new Task_Bodyguard(ua, targetGuard);
                    ua.task = task;
                }
                else if (targetChallenge != null && targetLocation != null)
                {
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
            }
            if (ua.task == null && ua.location.index != ua.homeLocation)
            {
                ua.task = new Task_GoToLocation(map.locations[ua.homeLocation]);
            }
        }

        public void getAllValidChallengesAndRituals(UA ua, Dictionary<Type, AIChallenge> aiChallenges, Dictionary<Type, AIChallenge> aiRituals, out List<Challenge> challenges, out Dictionary<Location, List<Challenge>> ritualData)
        {
            challenges = new List<Challenge>();
            List<Challenge> rituals = new List<Challenge>();
            ritualData = new Dictionary<Location, List<Challenge>>();

            // Gets all rituals attached to the UA, directly and indirectly.
            if (aiRituals.Count > 0)
            {
                List<Challenge> uaRituals = new List<Challenge>();

                uaRituals.AddRange(ua.rituals);
                foreach (Item item in ua.person.items)
                {
                    uaRituals.AddRange(item.challenges);
                }

                foreach (Challenge challenge in uaRituals)
                {
                    if (aiRituals.ContainsKey(challenge.GetType()))
                    {
                        rituals.Add(challenge);
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
                        if (!(challenge.isGoodTernary() == -1 && ua is UAG && !ua.corrupted))
                        {
                            if (!(challenge.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                            {
                                if (!challenge.allowMultipleUsers() && challenge.claimedBy == null)
                                {
                                    if (challenge.valid() && challenge.validFor(ua))
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
                    if (!(ritual.isGoodTernary() == -1 && ua is UAG && !ua.corrupted))
                    {
                        if (!(ritual.isGoodTernary() == 1 && (ua is UAE || ua.corrupted)))
                        {
                            AIChallenge aiChallenge = aiChallenges[ritual.GetType()];
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

        public double getChallengeUtility(Challenge challenge, AIChallenge aiChallenge, UA ua, Location location, bool respectDanger, bool valueTimeCost, List<ReasonMsg> reasonMsgs = null)
        {
            double utility = 0.0;

            utility += ua.person.getTagUtility(challenge.getPositiveTags(), challenge.getNegativeTags(), reasonMsgs);

            if (respectDanger)
            {
                double danger = getDangerUtility(challenge, ua);
                utility += danger;
            }

            utility += aiChallenge.checkChallengeUtility(challenge, ua, reasonMsgs, location);

            if (valueTimeCost)
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
