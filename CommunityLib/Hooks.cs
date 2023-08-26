using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityLib
{
    public abstract class Hooks
    {
        public Map map;

        public struct TaskData
        {
            public Challenge challenge;
            public Action<World, UM, UIE_Challenge> onClick;
            public string title;
            public Sprite icon;
            public int profileGain;
            public int menaceGain;
            public Color backColor;
            public bool enabled;
            public int special;
            public UA targetUA;
            public UM targetUM;
            public Location targetLocation;
        }

        public struct TaskData_Popout
        {
            public string title;
            public Sprite icon;
            public Sprite iconBackground;
            public string description;
            public string restrictions;
            public int profileGain;
            public int menaceGain;
            public Color backColor;
            public int complexity;
            public int progressPerTurn;
            public List<ReasonMsg> progressReasonMsgs;
        }

        public Hooks(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// This hook fires when the relgion UI is openned. It recieves the holy order that the ui has opened to. This hook does not fire when a player switches which religion they are viewing.
        /// </summary>
        /// <param name="order"></param>
        public virtual void onPlayerOpensReligionUI(HolyOrder order)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a patch is requested between two locations. It recieves the location the path is from (locA), the location the path is aiming to reach (locB), the unit that is seeking the path (u), which is null if not applicable, and whether to consider safeMove (safeMove). <br></br>
        /// If this hook returns any Location[] other than null, the rest of the pathFinding process will not happen. Instead, the function will return the array returned by this hook.
        /// <para>No instances of this hook will run after one which has not returned null.</para>
        /// </summary>
        /// <param name="locA"></param>
        /// <param name="locB"></param>
        /// <param name="u"></param>
        /// <param name="safeMove"></param>
        /// <returns></returns>
        public virtual Location[] interceptGetPathTo_Location(Location locA, Location locB, Unit u, bool safeMove)
        {
            return null;
        }

        /// <summary>
        /// This hook fires when a patch is requested between a location and a social group. It recieves the location the path is from (loc), the social group the path is trying to reach (sg), the unit that is seeking the path (u), which is null if not applicable, and whether to consider safeMove (safeMove). <br></br>
        /// If this hook returns any Location[] other than null, the rest of the pathFinding process will not happen. Instead, the function will return the array returned by this hook.
        /// <para>No instances of this hook will run after one which has not returned null.</para>
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="sg"></param>
        /// <param name="u"></param>
        /// <param name="safeMove"></param>
        /// <returns></returns>
        public virtual Location[] interceptGetPathTo_SocialGroup(Location loc, SocialGroup sg, Unit u, bool safeMove)
        {
            return null;
        }

        /// <summary>
        /// This hook fires when the Community Library's pathfiding algorithm is called. It recieves the location the path is from (locA), the location the path is aiming to reach (locB), the unit that is seeking the path (u), which is null if not applicable, and the list of pathfinding delegates that have alsready been assigned to the path (pathfindingDelegates), including the unit's movement type and safemove requirements. <br></br>
        /// In order to modify how the path is calculated, add one or more new pathfinding delegates to the pathfindingDelegates variable.
        /// </summary>
        /// <param name="locA"></param>
        /// <param name="locB"></param>
        /// <param name="u"></param>
        /// <param name="pathfindingDelegates"></param>
        public virtual void onPopulatingPathfindingDelegates_Location(Location locA, Location locB, Unit u, List<Func<Location[], Location, Unit, bool>> pathfindingDelegates)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's pathfiding algorithm is called. It recieves the location the path is from (loc), the social group the path is trying to reach (sg), the unit that is seeking the path (u), which is null if not applicable, and the list of pathfinding delegates that have alsready been assigned to the path (pathfindingDelegates), including the unit's movement type and safemove requirements. <br></br>
        /// In order to modify how the path is calculated, add one or more new pathfinding delegates to the pathfindingDelegates variable.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="sg"></param>
        /// <param name="u"></param>
        /// <param name="pathfindingDelegates"></param>
        public virtual void onPopulatingPathfindingDelegates_SocialGroup(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, bool>> pathfindingDelegates)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a unit is taking a step to a new location. All base-game movement processing have already been completed. It receives the unit moving (u), the unit's current location (locA), and the unit's new location (locB).
        /// <para>The unit's `int movesTaken` value will not yet have been incremented, as that is done by the function that called for the movement to take place, not by the movement function itself. To refund a move taken, simply decrement the unit's movesTaken value in this hook, even if that results in a temporariliy negative value.</para>
        /// </summary>
        /// <param name="u"></param>
        /// <param name="locA"></param>
        /// <param name="locB"></param>
        public virtual void onMoveTaken(Unit u, Location locA, Location locB)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an agent's AI checks the distance to a challenge for the purposes of the distance divisor component of its utility calculation for that challenge. It receives the agent (ua), the challenge (challenge), and the calculated distance (stepDistance). It returns the distance to the challenge as the number of turns required to reach it (typically equal to the step distance).
        /// <para>If another mod has already returned a modified value, the distance as time value (stepDistance) that you recieve will already include that alteration.</para>
        /// <para>This hook does not fire if the agent is already at the challenge.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="c"></param>
        /// <param name="stepDistance"></param>
        /// <returns></returns>
        public virtual int unitAgentAI_getChallengeUtility_getDistanceForDivisor(UA ua, Challenge c, int stepDistance)
        {
            return stepDistance;
        }

        /// <summary>
        /// This hook fires when a unit is instructed to die. It recieves the Unit (u), a string representation of the cause (v), and the person, if applicable, that casued their death (killer). <br></br>
        /// If this hook returns true, the rest of the death proccess will not happen. If you wish to keep the unit alive and prevent this check being performed multiple times per turn, make sure that their health is greater than 0, or their cause of death has been removed. The process which initially instructed the unit to die will still continue, so if you wish to keep the unit alive, make to sure to account for, and act in response to, the method of its death.
        /// <para>All instances of this hook will run whenever a unit is instructed to die, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="kiler"></param>
        /// <returns></returns>
        public virtual bool interceptUnitDeath(Unit u, string v, Person kiler = null)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when a unit is instructed to die, but after the interceptUnitDeath hook. It recieves the Unit (u), a string representation of the cause (v), and the person, if applicable, that casued their death (killer).
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="killer"></param>
        public virtual void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            return;
        }

        /// <summary>
        /// This hook fires at the beginning of a cycle of an army battle. It recieves the battle (battle) that is about to undergo its cycle and returns a boolean. <br></br>
        /// <para>If this hook returns true, the rest of the battle cycle proccess will not happen. This will not prevent processing in the AI of any units that are in the battle, or the player from ordering a controllable unit out of the battle. If you wish to do so, you will need to account for and act in response to those interactions.</para>
        /// <para>All instances of this hook will run whenever an army battle begins its cycle, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="battle"></param>
        /// <returns></returns>
        public virtual bool interceptArmyBattleCycle(BattleArmy battle)
        {
            return false;
        }

        /// <summary>
        /// This hook fires at the beginning of a cycle of an army battle, but after the interceptArmyBattleCycle hook. It recieves the battle (battle) that is about to undergo its cycle.
        /// </summary>
        /// <param name="battle"></param>
        public virtual void onArmyBattleCycle_StartOfProcess(BattleArmy battle)
        {
            return;
        }

        /// <summary>
        /// This hook fires at the end of a cycle of an ongoing army battle, not if the battle was won. It recieves the battle (battle) that has just undergone its cycle.
        /// </summary>
        /// <param name="battle"></param>
        public virtual void onArmyBattleCycle_EndOfProcess(BattleArmy battle)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an army battle cycle computes which side has a command advantage. It recieves the army battle (battle), and the calculated advantage (advantage), pre-constraint. It returns the calculated advantage as a double, which is then constrained to a value between 2.0 and -2.0, inclusively.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="advantage"></param>
        /// <returns></returns>
        public virtual double onArmyBattleCycle_ComputeAdvantage(BattleArmy battle, double advantage)
        {
            return advantage;
        }

        /// <summary>
        /// This hook fires when an army battle cycle calculates the damage that a unit will do to its target on the opposed side of the battle. It recieves the army battle (battle), the damage that will be dealt (dmg), the unit dealing the damage (unit), and the unit that will recieve the damage (target). It returns the damage that will be dealt.
        /// </summary>
        /// <param name="batle"></param>
        /// <param name="dmg"></param>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual int onArmyBattleCycle_DamageCalculated(BattleArmy batle, int dmg, UM unit, UM target)
        {
            return dmg;
        }

        /// <summary>
        /// This hook fires when an army battle has allocated damage to the military units fighting on a side, but before the damage is applied. It recieves the army battle (battle), the list of units (units) that are about to recieve damage, and an integer array of the total damage that the units are about to recieve (dmgs). These values are matched by index.<br></br>
        /// The messages generated in the combat log, and the messages that are sent to the player, have already been genereated at this point. Only use this hook if you must change the total damage value, rather than the individual values, and make sure to add a new message to the battle log that explains the discrepancy.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="units"></param>
        /// <param name="dmgs"></param>
        public virtual void onArmyBattleCycle_AllocateDamage(BattleArmy battle, List<UM> units, int[] dmgs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an army battle has been won, by one side having zero remaining units. It recieves the battle (battle) that has just been won, a list of surviving military units on the winning side (victorUnits), a list of surviving agents commanding the winning side (victorComs), a list of military units of the defeated side that were alive and in battle at the start of the battle cycle (defeatedUnits), and a list of the agents commanding the defeated side that were alive and in battle at the start of the battle cycle (defeatedComs).<br></br>
        /// If a battle is won as a consequence of units being killed, or retreating, between cycles, the defeatedUnits and defeatedComs lists will be empty. <br></br>
        /// If neither side has any surviving units, the lists of surviving victorious units and agents (victorUnits and victorComs) will all be empty, and the list of defeated units and agents (defeatedUnits and defeatedComs) will contain all units and agents that were alive and in battle at the start of the battle cycle.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="victorUnits"></param>
        /// <param name="victorComs"></param>
        /// <param name="defeatedUnits"></param>
        /// <param name="defeatedComs"></param>
        public virtual void onArmyBattleVictory(BattleArmy battle, List<UM> victorUnits, List<UA> victorComs, List<UM> defeatedUnits, List<UA> defeatedComs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a battle is ended as a consequence of the last military unit on one of the sides moving out of the battle, after the unit is removed from the battle's data. It recieves the battle (battle) that has just been terminated, a list of remaining military units (victorUnits), a list of remaining agents commanding the remaining side (victorComs), and the military unit who fled the battle (u). <br></br>
        /// If neither side has any surviving units, an error has occured, and the lists of surviving victorious units and agents (victorUnits and victorComs) will both be empty.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="victorUnits"></param>
        /// <param name="victorComs"></param>
        /// <param name="u"></param>
        public virtual void onArmyBattleTerminated(BattleArmy battle, List<UM> victorUnits, List<UA> victorComs, UM u)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a unit moves out of an army battle, before they are removed from the battle's data. It recieves the battle (battle), and the unit that fled (u).
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="u"></param>
        public virtual void onArmyBattleRetreatOrFlee(BattleArmy battle, Unit u)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a minion in a minion battle is about to make an attack. It receives the Minion that is about to attack (attacker), the enemy Agent (other), the Battle Popup Window (battle), which is null if history is generating or popups are otherwise disabled, the agent battle (battle), the damage that it si about to deal (dmg), and the row that it is attick down (row). It returns the damage that the minion will do.<br></br>
        /// The Row directly corrisponds to the index of the minions on both agents. If "other.minions[row]" is equal to null, then the damage will be dealt to the enemy agent.
        /// <para>If another mod has already modified the damage value using this hook, that change will be in the damage value that you receive.</para>
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="other"></param>
        /// <param name="popup"></param>
        /// <param name="battle"></param>
        /// <param name="dmg"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual int onMinionAttackAboutToBePerformed(Minion attacker, UA other, PopupBattleAgent popup, BattleAgents battle, int dmg, int row)
        {
            //Console.WriteLine("CommunityLib: Minion attack about to be performed");
            return dmg;
        }

        /// <summary>
        /// This hook fires when an agent or one of their minions is about to take attack damage in an agent battle. It receives the popup battle window (battle), which is null if the history is generating or popups are otherwise disabled, the agent battle (battle), the agent whom the attack is directed towards (defender), the minion that is about to recieve the damge in the agent's stead (minion) if any, the damage they are about to receive (dmg), and the row that the attack is made down (row). It returns the damage that the victim will receive.<br></br>
        /// The Row directly corrisponds to the index of the minions on both agents. If minion is not null, it is stored in "defender.minion[row]".
        /// <para>If another mod has already modified the damage value using this hook, that change will be in the damage value that you receive.</para>
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="battle"></param>
        /// <param name="defender"></param>
        /// <param name="minion"></param>
        /// <param name="dmg"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual int onAgentBattle_ReceiveDamage(PopupBattleAgent popup, BattleAgents battle, UA defender, Minion minion, int dmg, int row)
        {
            //Console.WriteLine("CommunityLib: Agent battle damage About to be received.");
            return dmg;
        }

        /// <summary>
        /// This hook fires when a military unit is about to recieve damage in an army battle. It recieves the army battle (battle), the military unit (u), and the total damage it is about to recieve (dmg). It returns the damage the unit is about to recieve as an int. <br></br>
        /// The damage that the unit is about to recieve is the total of all damage sources being applied to that unit in that battle cycle. This hook does not fire for each individual damage source. If you wish to modify the damage sources individually, use the 'onArmyBattleCycle_DamageCalculated' hook.
        /// <para>The messages generated in the combat log, and the messages that are sent to the player, have already been genereated at this point. Only use this hook if you must change the total damage value, rather than the individual values, and make sure to add a new message to the battle log that explains the discrepancy.</para>
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="u"></param>
        /// <param name="dmg"></param>
        /// <returns></returns>
        public virtual int onUnitReceivesArmyBattleDamage(BattleArmy battle, UM u, int dmg)
        {
            return dmg;
        }

        /// <summary>
        /// This hook fires when a religion is viewed in the religion screen. It recieves the holy order (order), and the list of reason messages (msgs) that is used to compile the output. <br></br>
        /// The list of reason message can contain both 'ReasonMsg' and 'CommunityLib.ReasonMsgMax'. If you want to display a singular value, as with 'Income', use a 'reasonMsg'. If you wish to dipslay a value out of a maximum value, as with 'Gold for Acolytes', use a 'CommunityLib.ReasonMsgMax'. <br></br>
        /// The message is displayed in the left hand panel of the budget screen, and the value and/or max value is displayed in the value column, in the same line as the message.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msgs"></param>
        public virtual void onPopupHolyOrder_DisplayBudget(HolyOrder order, List<ReasonMsg> msgs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a relgiion is viewed in the religion screen. It recieves the holy order (order), and the list of reason messages (msgs) that is used to compile the stats displayed in the top left (acolytes, worshippers, etc.).
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msgs"></param>
        public virtual void onPopupHolyOrder_DisplayStats(HolyOrder order, List<ReasonMsg> msgs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a religion is viewed in the religion screen. It recieves the holy order (order), the string (s) that will be dsiplayed, and the amount of influence that was gained by the player that turn (infGain). It returns a string, which is, after all instances of this hook have been run, then diplayed in the HolyOrder screen.
        /// <para>If any other mods have already mdofiied the string prior to yours, those changes will already be present in the argument s.</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="s"></param>
        /// <param name="infGain"></param>
        /// <returns></returns>
        public virtual string onPopupHolyOrder_DisplayInfluenceElder(HolyOrder order, string s, int infGain)
        {
            return s;
        }

        /// <summary>
        /// This hook fires when a religion is viewed in the religion screen. It recieves the holy order (order), the string (s) that will be displayed, and the amount of influence that was gained by the humans that turn (infGain). It returns a string, which is, after all instances of this hook have been run, then diplayed in the HolyOrder screen.
        /// <para>If any other mods have already mdofiied the string prior to yours, those changes will already be present in the argument s.</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="s"></param>
        /// <param name="infGain"></param>
        /// <returns></returns>
        public virtual string onPopupHolyOrder_DisplayInfluenceHuman(HolyOrder order, string s, int infGain)
        {
            return s;
        }


        /// <summary>
        /// This hook fires when a religion is viewed in the religion screen. It recieves the holy order (order), the string (s) that will be displayed in the large central text box, and the index of the page that is being viewed (pageIndex). It returns the string that will be displayed in the large central text box for that page.
        /// <para>If any other mods have already mdofiied the string prior to yours, those changes will already be present in the argument s.</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="s"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public virtual string onPopupHolyOrder_DisplayPageText(HolyOrder order, string s, int pageIndex)
        {
            return s;
        }

        /// <summary>
        /// This hook fires just after a player influences a HolyTenet from the Holy Order UI. It recieves the holy order (order), and the holy tenet (tenet).
        /// </summary>
        /// <param name="order"></param>
        /// <param name="tenet"></param>
        public virtual void onPlayerInfluenceTenet(HolyOrder order, HolyTenet tenet)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a settlement is destroyed. It recieves the settlement (set), a string representation of the cause (v), and the object, if applicable, that casued its destruction (killer). <br></br>
        /// If this hook returns true, the rest of the ruin proccess will not happen. If you wish to keep the settlement intact and prevent this check being performed multiple times per turn, make sure that their cause of destruction has been removed. The process which initially instructed the settlement to fall into ruin will still continue, so if you wish to keep the settlement intact, make to sure to account for, and act in response to, the method of its destruction.
        /// <para>All instances of this hook will run whenever a settlemnt is instructed to fall into ruin, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="set"></param>
        /// <param name="v"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        public virtual bool interceptSettlementFallIntoRuin(Settlement set, string v, object killer = null)
        {
            return false;
        }

        /// <summary>
        /// This hook fires each turn while a military unit is razing a settlement, after validating that the task is still valid. It recieves the military unit that is performing the task (um).
        /// </summary>
        /// <param name="um"></param>
        public virtual void onRazeLocation_StartOfProcess(UM um)
        {
            
        }

        /// <summary>
        /// This hook fires each turn while a military unit is razing a settlement, after the task's turnTick has occured. It will not fire if the task becomes invalid. It recieves the military unit that is performing the task (um).
        /// <para>If the settlement being razed was destroyed this turn, the task of the military unit (um) will equal null.</para>
        /// </summary>
        /// <param name="um"></param>
        public virtual void onRazeLocation_EndOfProcess(UM um)
        {

        }

        /// <summary>
        /// This hook fires when a settlement is destroyed, but after the interceptSettlementFallIntoRuin hook. It recieves the settlement (set), a string representation of the cause (v), and the object, if applicable, that casued its destruction (killer).
        /// </summary>
        /// <param name="set"></param>
        /// <param name="v"></param>
        /// <param name="killer"></param>
        public virtual void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a settlement is destroyed, after all other processing has concluded. It recieves the settlement (set), a string representation of the cause (v), and the object, if applicable, that casued its destruction (killer).
        /// </summary>
        /// <param name="set"></param>
        /// <param name="v"></param>
        /// <param name="killer"></param>
        public virtual void onSettlementFallIntoRuin_EndOfProcess(Settlement set, string v, object killer = null)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI beings processing an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), a list of AgentAI.ChallengeData structs (validChallengeData), a list of AgentAI.TaskData structs (validTaskData), and a list of Units (visibleUnits).<br></br>
        /// If this hook returns true, the rest of the AI process will not happen.
        /// <para>All instances of this hook will run whenever an AgentAI runs, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="validTaskData"></param>
        /// <param name="visibleUnits"></param>
        /// <returns></returns>
        public virtual bool interceptAgentAI(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has started processing an agent, immediately after the 'interceptAgentAI' hook. It recieves the agent (ua), the AIData for its Agent AI (aiData), a list of AgentAI.ChallengeData structs (validChallengeData), a list of AgentAI.TaskData structs (validTaskData), and a list of Units (visibleUnits).
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="validTaskData"></param>
        /// <param name="visibleUnits"></param>
        public virtual void onAgentAI_StartOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has finished processing an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), a list of AgentAI.ChallengeData structs (validChallengeData), a list of AgentAI.TaskData structs (validTaskData), and a list of Units (visibleUnits).
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="validTaskData"></param>
        /// <param name="visibleUnits"></param>
        public virtual void onAgentAI_EndOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI beings processing processing the utility of a challenge for an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), the AgentAI.ChallengeData (challengeData), the utility (utility), and a list of ReasonMsgs (reasonMsgs).<br></br>
        /// If this hook returns true, the rest of the getChallengeUtility process will not happen.
        /// <para>All instances of this hook will run whenever an AgentAI runs, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="challengeData"></param>
        /// <param name="utility"></param>
        /// <param name="reasonMsgs"></param>
        /// <returns></returns>
        public virtual bool interceptAgentAI_GetChallengeUtility(UA ua, AgentAI.AIData aiData, AgentAI.ChallengeData challengeData, ref double utility, List<ReasonMsg> reasonMsgs)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has finished processing the utility of a challenge for an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), the AgentAI.ChallengeData (challengeData), the utility (utility), and a list of ReasonMsgs (reasonMsgs), which may be null.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="challengeData"></param>
        /// <param name="utility"></param>
        /// <param name="reasonMsgs"></param>
        /// <returns></returns>
        public virtual double onAgentAI_GetChallengeUtility(UA ua, AgentAI.AIData aiData, AgentAI.ChallengeData challengeData, double utility, List<ReasonMsg> reasonMsgs)
        {
            return utility;
        }

        /// <summary>
        /// This hook fires when the Community Library's AGent AI checks the distance to a challenge for the purposes of the distance divisor component of it's utility calculation for that challenge. It recieves the agent (ua), the AIData for its Agent AI (aiData), the AgentAI.ChallengeData (challengeData), and the calculated distance (stepDistance). It returns the distance to the challenge as the number of turns required to reach it (typically equal to the step distance).
        /// <para>If another mod has already returned a modified value, the distance as time value (stepDistance) that you recieve will already include that alteration.</para>
        /// <para>This hook does not fire if the agent is already at the challenge.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="challengeData"></param>
        /// <param name="c"></param>
        /// <param name="stepDistance"></param>
        /// <returns></returns>
        public virtual int onAgentAI_GetChallengeUtility_GetDistanceForDivisor(UA ua, AgentAI.AIData aiData, AgentAI.ChallengeData challengeData, int stepDistance)
        {
            return stepDistance;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI beings processing processing the utility of a task for an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), the AgentAI.TaskData (taskData), the utility (utility), and a list of ReasonMsgs (reasonMsgs).<br></br>
        /// If this hook returns true, the rest of the getTaskUtility process will not happen.
        /// <para>All instances of this hook will run whenever an AgentAI runs, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="taskData"></param>
        /// <param name="utility"></param>
        /// <param name="reasonMsgs"></param>
        /// <returns></returns>
        public virtual bool interceptAgentAI_GetTaskUtility(UA ua, AgentAI.AIData aiData, AgentAI.TaskData taskData, ref double utility, List<ReasonMsg> reasonMsgs)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has finished processing processing the utility of a task for an agent. It recieves the agent (ua), the AIData for its Agent AI (aiData), the AgentAI.TaskData (taskData), the utility (utility), and a list of ReasonMsgs (reasonMsgs).
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="aiData"></param>
        /// <param name="taskData"></param>
        /// <param name="utility"></param>
        /// <param name="reasonMsgs"></param>
        /// <returns></returns>
        public virtual double onAgentAI_GetTaskUtility(UA ua, AgentAI.AIData aiData, AgentAI.TaskData taskData, double utility, List<ReasonMsg> reasonMsgs)
        {
            return utility;
        }

        /// <summary>
        /// This hook fires when an agent gets the valid list of traits that they can choose from when levelling up, but before they have chosen one. It receives the agent (ua), the list of traits (availableTraits) and whether it is getting the starting traits for specific agent types (isStartingTraits)
        /// <para>The function that this hook fires after caches if the agent ua has any possible traits left to get, in the 'ua.person.cachedOutOfTraits' boolean. If adding new traits to the list, make sure to set 'cachedOutOfTraits' to false if your trait is still available. If removing traits, make sure to set 'cachedOutOfTraits' to true if trait other than the one you are removing are not available.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="availableTraits"></param>
        /// <param name="startingTraits"></param>
        public virtual void onAgentLevelup_GetTraits(UA ua, List<Trait> availableTraits, bool startingTraits)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an agent tries to get visible units to test for unit interactions. It recieves the agent (ua), and the list of units that it considers visible (visibleUnits). <br></br>
        /// If this hook returns true, the base game's getVisibleUnits function will not run.
        /// <para>All instances of this hook will run whenever an AgentAI runs, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <returns></returns>
        public virtual bool interceptGetVisibleUnits(UA ua, List<Unit> visibleUnits)
        {
            return false;
        }

        /// <summary>
        /// This hook fires immediately after an agent has gathered visible units to test for unit interactions. It recieves the agent (ua), and the list of units that it considers visible (visibleUnits). It returns the list of visible units.
        /// </summary>
        /// <param name="visibleUnits"></param>
        /// <param name="ua"></param>
        /// <returns></returns>
        public virtual List<Unit> getVisibleUnits_EndOfProcess(UA ua, List<Unit> visibleUnits)
        {
            return visibleUnits;
        }

        /// <summary>
        /// This hook fires when a non-commandable Agent is considering whether to swap an item in their inventory with a new item of higher level. It recieves the person (person) who is about to make the swap, the item (item) that they are about to get rid of, the item (newItem) that they are about to gain, and a boolean that determines if they must take the new item (obligateHold). <br></br>
        /// If this hook returns true, the person will not swap the item for the new item, and will instead test against the following item slots.
        /// <para>All instances of this hook will run whenever a non-commandable agent, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="person"></param>
        /// <param name="item"></param>
        /// <param name="newItem"></param>
        /// <param name="obligateHold"></param>
        /// <returns></returns>
        public virtual bool interceptReplaceItem(Person person, Item item, Item newItem, bool obligateHold)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the UIScroll_Unit interface (the panel on the right that shows challenges and tasks available to the selected unit) attempts to populate tasks for a commandable military unit. It returns a list of TaskData structs, which are then used to create action buttons for the unit.
        /// </summary>
        /// <param name="um"></param>
        /// <returns></returns>
        public virtual List<TaskData> onUIScroll_Unit_populateUM(UM um)
        {
            return null;
        }

        /// <summary>
        /// This hook fires when you hover your mouse over an action button in the UIScroll_Unit interface (the panel on the right that shows challenges and tasks available to the selected unit), excluding buttons for challenges, for a commandable military unit. It receives the military unit (um), a partial reconstruction of the TaskData object that could have been used to create it, not the original object. It does not include the onClick delegate, profile gain, or menace gain. <br></br>
        /// If the partially reconstructed TaskData matches a task that your mod adds, populate the popoutData (TaskData_Popout) and return true. Otherwise, return false to allow another mod or the base game to handle the challenge popout screen for that action button.
        /// <br></br>The title, icon and icon background are pre-set in popoutData when it is passed into the hook, and the progressReasonMsgs are set to an empty list of ReasonMsg.
        /// <para>No hook after the first hook to return true will be called.</para>
        /// </summary>
        /// <param name="um"></param>
        /// <param name="taskData"></param>
        /// <param name="popoutData"></param>
        /// <returns></returns>
        public virtual bool interceptChallengePopout(UM um, TaskData taskData, ref TaskData_Popout popoutData)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when an action taking monster finishes populating it's actions. It recieves the action taking monster (monster) and a list of monster actions (actions).
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="actions"></param>
        public virtual void populatingMonsterActions(SG_ActionTakingMonster monster, List<MonsterAction> actions)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an action taking monster gets the utility score for a monster action. It recieves the action taking monster (monster), the monster action (action), the utility score (utility), and a list of reason messages (reasonMsgs). It returns the utility score.
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="utility"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual double onActionTakingMonster_getUtility(SG_ActionTakingMonster monster, MonsterAction action, double utility, List<ReasonMsg> reasonMsgs)
        {
            return utility;
        }

        /// <summary>
        /// This hook fires when an event checks if a location is the location of the Elder Tomb. It only fires if the location's settlement is not an instance of `Set_TombOfGods`, or a subclass thereof. It recieves the Location beibng checked (location) and returns whether the location should be considered to be the elder tomb. <br></br>
        /// This hook is not called to determine whether a graphical hex should display a property overlay for an atypical Elder Tomb. If you wish your atypical Elder Tomb to not display a property overlay, of if you wish it to display a speciifc property overlay, use the base game's `onGraphicalHexUpdated` hook instead.
        /// <para>No hook after the first hook to return true will be called.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool onEvent_IsLocationElderTomb(Location location)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the Recruit Agent menu is being populated, once for each agent on the map that is not already commandable. It recieves the agent being considered (ua), and the current determination on if it should be recruitable, including the base game's output and all previous instances of this hook (result), and returns if the should be reqruitable.
        /// <para>If you do not wish to change the value for a given agent, simply return result.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <returns></returns>
        public virtual bool onAgentIsRecruitable(UA ua, bool result)
        {
            return result;
        }
    }
}
