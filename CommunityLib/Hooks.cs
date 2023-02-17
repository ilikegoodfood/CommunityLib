using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public abstract class Hooks
    {
        public Map map;

        public Hooks(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// This hook fires when a unit is instructed to die. It recieves the Unit (u), a string representation of the cause (v), and a the person, if applicable, that casued their death (killer).
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
        /// This hook fires when a unit is instructed to die, but after the interceptUnitDeath hook. It recieves the Unit (u), a string representation of the cause (v), and a the person, if applicable, that casued their death (killer).
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
    }
}
