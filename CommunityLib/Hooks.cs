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
        /// This hook fires when a unit is instructed to die. It recieves the Unit (u), a string representation of the cause (v), and the person, if applicable, that casued their death (killer).
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
        /// This hook fires at the beginning of a cycle of an army battle. It recieves the battle (battle) that is about to undergo its cycle and returns a boolean.
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
        /// This hook fires at the end of a cycle of an army battle, but not if the battle was won. It recieves the battle (battle) that has just undergone its cycle.
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
        /// This hook fires when an army battle has allocated damage to the military units fighting on a side, but before the damage is applied. It recieves the army battle (battle), the list of units (units) that are about to recieve damage, and an integer array of the damage that the units are about to recieve (dmgs). These values are matched by index.
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="units"></param>
        /// <param name="dmgs"></param>
        public virtual void onArmyBattleCycle_AllocateDamage(BattleArmy battle, List<UM> units, int[] dmgs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an army battle has been won, by one side having zero remaining units. It recieves the battle (battle) that has just been won, a list of surviving military units on the winning side (victorUnits), a list of surviving agents commanding the winning side (victorComs), a list of military units of the defeated side that were alive and in battle at the start of the battle cycle (defeatedUnits), and a list of the agents commanding the defeated side that were alive and in battle at the start of the battle cycle (defeatedComs).
        /// <para>If a battle is won as a consequence of units being killed, or retreating, between cycles, the defeatedUnits and defeatedComs lists will be empty.</para>
        /// <para>If neither side has any surviving units, an error has occured, and the lists of units and agents will all be empty.</para>
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
        /// This hook fires when a battle is ended as a consequence of the last military unit on one of the sides moving out of the battle, after the unit is removed from the battle's data. It recieves the battle (battle) that has just been terminated, a list of remaining military units (victorUnits), a list of remaining agents commanding the remaining side (victorComs), and the military unit who fled the battle (u).
        /// <para>If neither side has any surviving units, an error has occured, and the lists of units and agents will all be empty.</para>
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
        /// This hook fires when a unit moves out of an army battle, befoire they are removed from the battle's data. It recieves the battle (battle), and the unit that fled (u).
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="u"></param>
        public virtual void onArmyBattleRetreatOrFlee(BattleArmy battle, Unit u)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a military unit is about to recieve damage in an army battle. It recieves the army battle (battle), the military unit (u), and the damage it is about to recieve (dmg). It returns a new integer damage value.
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
        /// This hook fires when a religion is viewed in the religion screen. It recieves the holy order (order), and the list of reason messages (msgs) that is used to compile the output.
        /// <para>The list of reason message can contain both 'ReasonMsg' and 'CommunityLib.ReasonMsgMax'. If you want to display a singular value, as with 'Income', use a 'reasonMsg'. If you wish to dipslay a value out of a maximum value, as with 'Gold for Acolytes', use a 'CommunityLib.ReasonMsgMax'.</para>
        /// <para>The message is displayed in the left hand panel of the budget screen, and the value and/or max value is displayed in the value column, in the same line as the message.</para>
        /// </summary>
        /// <param name="order"></param>
        /// <param name="msgs"></param>
        public virtual void onPopupHolyOrder_DisplayBudget(HolyOrder order, List<ReasonMsg> msgs)
        {
            return;
        }

        /// <summary>
        /// This hook fires when a relgiion is viewed in the religion screen. It recieves the holy order (order), and the list of reason messages (msgs) that is used to compile the output.
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
        /// This hook fires when a settlement is destroyed. It recieves the settlement (set), a string representation of the cause (v), and the object, if applicable, that casued its destruction (killer).
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
        /// This hook fires when the Community Library's Agent AI beings processing an agent. It recieves the agent (ua), a list of AgentAI.ChallengeData structs (validChallengeData), and the boolean control values within an 'AgentAI.ControlParameters' struct (controlParams).
        /// If this hook returns true, the rest of the AI process will not happen.
        /// <para>All instances of this hook will run whenever an AgentAI runs, even those after one which has returned true.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="inputParams"</param>
        /// <returns></returns>
        public virtual bool interceptAgentAI(UA ua, List<AgentAI.ChallengeData> validChallengeData, AgentAI.ControlParameters controlParams)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has started processing an agent, immediately after the 'interceptAgentAI' hook. It recieves the agent (ua), a list of AgentAI.ChallengeData structs (validChallengeData), and the boolean control values within an 'AgentAI.ControlParameters' struct (controlParams)
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="controlParams"></param>
        public virtual void onAgentAI_StartOfProcess(UA ua, List<AgentAI.ChallengeData> validChallengeData, AgentAI.ControlParameters controlParams)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's Agent AI has finished processing an agent. It recieves the agent (ua), a list of AgentAI.ChallengeData structs (validChallengeData), and the boolean control values within an 'AgentAI.ControlParameters' struct (controlParams)
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="validChallengeData"></param>
        /// <param name="inputParams"></param>
        public virtual void onAgentAI_EndOfProcess(UA ua, List<AgentAI.ChallengeData> validChallengeData, AgentAI.ControlParameters controlParams)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an agent gets the valid list of traits that they can choose from when levelling up, but before they have chosen one. It receives the agent (ua), the list of traits (availableTraits) and whether it is getting the starting traits for specific agent types (isStartingTraits)
        /// <para>The function that this hooks fires after caches if the agent ua has any possible traits left to get, in the 'ua.person.cachedOutOfTraits' boolean. If adding new traits to the list, make sure to set 'cachedOutOfTraits' to false if your trait is still available. If removing traits, make sure to set 'cachedOutOfTraits' to true if trait other than the one you are removing is available.</para>
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="availableTraits"></param>
        /// <param name="startingTraits"></param>
        public virtual void onAgentLevelup_GetTraits(UA ua, List<Trait> availableTraits, bool startingTraits)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an agent tries to get visible units to test for unit interactions. It recieves the agent (ua), and the list of units that it considers visible (visibleUnits).
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
        /// This hook fires when the UIScroll_Unit interface (the panel on the right that shows challenges and tasks available to the selected unit) attempts to populate tasks for a commandable military unit. It returns a liss of TaskData structs, which are then used to create an action button for the unit.
        /// </summary>
        /// <param name="um"></param>
        /// <returns></returns>
        public virtual List<TaskData> onUIScroll_Unit_populateUM(UM um)
        {
            return null;
        }


        /// <summary>
        /// This hook fires when you hover your mouse over an action button in the UIScroll_Unit interface (the panel on the right that shows challenges and tasks available to the selected unit), excluding buttons for challenges, for a commandable military unit. It receives the military unit (um), a partial reconstruction of the TaskData object that could have been used to create it, not the original object. It does not include the onClick delegate, profile gain, or menace gain
        /// <para>If the partially reconstructed TaskData matches a task that your mod adds, populate the popoutData (TaskData_Popout) and return true. Otherwise, return false to allow another mod or the base game to handle the challenge popout screen for that action button.</para>
        /// <para>The title, icon and icon background are pre-set in popoutData when it is passed into the hook, and the progressReasonMsgs are set to an empty list of ReasonMsg</para>
        /// <para>No hook after the first hook to return true will be called.</para>
        /// </summary>
        /// </summary>
        /// <param name="um"></param>
        /// <param name="taskData"></param>
        /// <param name="popoutData"></param>
        /// <returns></returns>
        public virtual bool interceptChallengePopout(UM um, TaskData taskData, ref TaskData_Popout popoutData)
        {
            return false;
        }
    }
}
