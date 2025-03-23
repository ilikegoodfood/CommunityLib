using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityLib
{
    public abstract class Hooks
    {
        public Map map;

        public class TaskUIData
        {
            public Challenge challenge;
            public Action<World, UM, UIE_Challenge> onClick;
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
            public bool enabled;
            public int special;
            public UA targetUA;
            public UM targetUM;
            public Location targetLocation;
        }

        public Hooks(Map map)
        {
            this.map = map;
        }

        public virtual List<WonderData> onMapGen_PlaceWonders()
        {
            return null;
        }

        public virtual void onMapGen_PlaceWonders(Type t, out bool failedToPlaceWonder)
        {
            failedToPlaceWonder = false;
            return;
        }

        /// <summary>
        /// This hook fires just after a graphical unit has been updated. It receives the graphical unit (graphicalUnit).
        /// </summary>
        /// <param name="graphicalUnit"></param>
        public virtual void onGraphicalUnitUpdated(GraphicalUnit graphicalUnit)
        {

        }

        /// <summary>
        /// This hook fires just after a graphical link has been updated. It receives the graphical link (graphicalLink).
        /// </summary>
        /// <param name="graphicalLink"></param>
        public virtual void onGraphicalLinkUpdated(GraphicalLink graphicalLink)
        {

        }

        /// <summary>
        /// This hook fires when the Community Library's pathfiding algorithm is called. It recieves the location the path is from (locA), the location the path is aiming to reach (locB), the unit that is seeking the path (u), which is null if not applicable, and the list of pathfinding delegates that have alsready been assigned to the path (pathfindingDelegates), including the unit's movement type and safemove requirements. <br></br>
        /// In order to modify how the path is calculated, add one or more new pathfinding delegates to the pathfindingDelegates variable.
        /// </summary>
        /// <param name="locA"></param>
        /// <param name="locB"></param>
        /// <param name="u"></param>
        /// <param name="pathfindingDelegates"></param>
        public virtual void onPopulatingPathfindingDelegates(Location loc, Unit u, List<int> expectedMapLayers, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the pathfinding system has failed to find a valid path to the destination in the first pass. By default, this is via only the layers of the path's origin and destination. It recieves the location the path is from (locA), the location the path is aiming to reach (locB), the unit that is seeking the path (u), which is null if not applicable, and the list of pathfinding delegates that have alsready been assigned to the path (pathfindingDelegates), including the unit's movement type and safemove requirements. It returns if the unit should be allowed to path via other layers to try and reach the destination, as a bool. <br></br>
        /// In order to modify how the path is calculated, remove one or more pathfinding delegates from the pathfindingDelegates variable. <br></br>
        /// All instances of this hook are called, even after one returns true, so that all required delegates get removed.
        /// </summary>
        /// <param name="locA"></param>
        /// <param name="locB"></param>
        /// <param name="u"></param>
        /// <param name="pathfindingDelegates"></param>
        /// <returns></returns>
        public virtual bool onPathfinding_AllowSecondPass(Location locA, Unit u, List<int> expectedMapLayers, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the game gathers all locations that should be connected to a tarde route. It recieves the map (map), and the list of locations that should be connected to a trade route (endpoints). <br></br>
        /// To force a location that is not the capitol city of a society to be connected to the network of trade routes, add it to the list of endpoints.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="endpoints"></param>
        public virtual void onGetTradeRouteEndpoints(Map map, List<Location> endpoints)
        {
            return;
        }

        /// <summary>
        /// This hook fires after the trade network has been rebuilt
        /// </summary>
        /// <param name="map"></param>
        /// <param name="tradeManager"></param>
        public virtual void onBuildTradeNetwork_EndOfProcess(Map map, ManagerTrade tradeManager, List<Location> endpoints)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the Community Library's pathfinding algorithm is called to generate a trade route from a location. It recieves the location the tarde route is from (start), the map layers that the destination should be on (endPointMapLayers), which is a size-zero array if not applicable, the list of pathfinding delegates that have already been assigned to the path (pathfndingDelegates), and the list of validity desligates that have already been assigned to determine if the destination is valid (destinationValidityDelegates). <br></br>
        /// In order to modify how the path is calculated, add one or more new pathfinding delegates to the pathfindingDelegates variable. In order to control the end point that is considered valid to form a trade route to, add one or more destination validity delegates to the destinationValidityDelegates variable.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="endPointMapLayer"></param>
        /// <param name="pathfindingDelegates"></param>
        /// <param name="destinationValidityDelegates"></param>
        public virtual void onPopulatingTradeRoutePathfindingDelegates(Location start, List<int> expectedMapLayer, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {

        }

        /// <summary>
        /// This hook fires when the pathfidning system has failed to find a valid path to the destination in the first pass. By default, this is via only the layers of the path's origin, and the end point map layers. It recieves the location the trade route is from (start), the acceptable map layers for the end point to be on (endPointMapLayers), the list of pathfinding delegates that have already been assigned to the path (pathfndingDelegates), and the list of validity desligates that have already been assigned to determine if the destination is valid (destinationValidityDelegates). <br></br>
        /// In order to modify how the path is calculated, remove one or more pathfinding delegates from the pathfindingDelegates variable. In order to modify what destinations are considered valid, remove one or more delegates from the destination validity delegates.<br></br>
        /// All instances of this hook are called, even after one returns true, so that all required delegates get removed.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="u"></param>
        /// <param name="pathfindingDelegates"></param>
        /// <param name="destinationValidityDelegates"></param>
        /// <returns></returns>
        public virtual bool onPathfindingTadeRoute_AllowSecondPass(Location start, List<int> expectedMapLayer, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates)
        {
            return false;
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
        /// This hook fires when several key functions in the base game tries to get the shortest distance to a location. These functions are `UA.distanceDivisor`, `Task_AttackArmy`, `Task_AttackUnit`, `Task_AttackUnitWithEscort`, `Task_Bodyguard`, `Task_DisruptUA`, and `CommunityLibrary.AgentAI.getDistanceDivisor`.<br></br>
        /// It receives the unit (u), the target location (target), the path to the target location (pathTo), and the time it will take the agent to traverse it's path to the destination (travelTime). The travelTime is equal to the length of the pathTo, unles the PathTo is null, in which case it will be equal to `Map.getStepDistance(u.location, loc) / u.getMaxSteps()`, rounded up. It returns the calculated travelTime.
        /// <para>If a mod that is loaded before yours has already altered the travel time, the travelTime passed into this hook will already include that change.</para>
        /// </summary>
        /// <param name="u"></param>
        /// <param name="target"></param>
        /// <param name="travelTime"></param>
        /// <returns></returns>
        public virtual int onUnitAI_GetsDistanceToLocation(Unit u, Location target, Location[] pathTo, int travelTime)
        {
            return travelTime;
        }

        /// <summary>
        /// This hook fires when a mod needs to check if a unit is subsumed (marked as dead but is actually alive as part of another unit). It recieves the original unit (uOriginal), and the current unit that is assigned to the person of the original unit (uSubsuming). It returns if the original unit is subsumed by the subsuming unit as a bool. <br></br>
        /// No instance of this hook is called after the first to retun true.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="subsumingUnit"></param>
        /// <returns></returns>
        public virtual bool isUnitSubsumed(Unit uOriginal, Unit uSubsuming)
        {
            return false;
        }

        /// <summary>
        /// This hook fires when the number of recruited agents is recalculated. It recieves the list of controllable units (playerControlledUnits), and the current count of recruited agents in that list (recruitmentCapUsed). It returns the modfied number of recruited agents. <br></br>
        /// If a mod that is loaded before yours has already altered the current count of recruited agents, the recruitmentCapUsed passed into this hook will already include that change.
        /// </summary>
        /// <param name="playerControlledUnits"></param>
        /// <param name="recruitmentCapUsed"></param>
        /// <returns></returns>
        public virtual int onCalculateAgentsUsed(List<Unit> playerControlledUnits, int recruitmentCapUsed)
        {
            return recruitmentCapUsed;
        }

        /// <summary>
        /// This hook fires when the game requests the player's agent limits. It recieves the current god (god) and the array agent limits, indexed by the number of seals broken, (agentCapBySeal). A mod may modifify the values in the array.
        /// </summary>
        /// <param name="agentCapBySeal"></param>
        public virtual void onGetAgentCaps(God god, int[] agentCapBySeal)
        {
            return;
        }

        /// <summary>
        /// This hook fires when the game requests the maximum amount of power that the player may have. It recieves the current god (god) and the maximim power value (maxPower), and returns the new maximum power value. <br></br>
        /// If a mod that is loaded before yours has already altered the maximim power value, the maxPower passed into this hook will already include that change.
        /// </summary>
        /// <param name="maxPower"></param>
        /// <returns></returns>
        public virtual int onGetMaxPower(God god, int maxPower)
        {
            return maxPower;
        }

        /// <summary>
        /// This hook fires when a unit is instructed to die. It recieves the Unit (u), a string representation of the cause (v), and the person, if applicable, that casued their death (killer). <br></br>
        /// If this hook returns true, the rest of the death proccess will not happen. If you wish to keep the unit alive and prevent this check being performed multiple times per turn, make sure that their health is greater than 0, or their cause of death has been removed. The process which initially instructed the unit to die will still continue, so if you wish to keep the unit alive, make to sure to account for, and act in response to, the method of its death.
        /// <para>Instances of this hook will be called up to the first to return true, aborting the death process.</para>
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
        /// This hook fires when an agent battle is about to start. It recieves the attacking agent (att), and the defending agent (def). It returns a custom battle, if the battle should be of a custom type, or null. <br></br>
        /// No instance of this hook fires after the first that returns a BattleAgents other than null.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="att"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public virtual BattleAgents onAgentBattleStarts(UA att, UA def)
        {
            return null;
        }

        /// <summary>
        /// This hook fires when an agent battle completes setup. It recieves thte battle that has just completed setup (battle).
        /// </summary>
        /// <param name="battle"></param>
        public virtual void onAgentBattle_Setup(BattleAgents battle)
        {
            return;
        }

        /// <summary>
        /// This hook fires when an agent battle has been replaced by a custom agent battle and is about to resolve automatically (without player involvement). It recieves the battle that is processing (battle). It returns is the normal battle logic should be skipped, as a bool. <br></br>
        /// If the normal battle process is skipped, this hook can be used to run an entirely diferent battle logic system. <br></br>
        /// No instance of this hook fires after the first to return true.
        /// </summary>
        /// <param name="battle"></param>
        /// <returns></returns>
        public virtual bool interceptAgentBattleAutomatic(BattleAgents battle)
        {
            return false;
        }

        /// <summary>
        /// This hook fires during the step processing of an agent battle (`BattleAgents`). It recieves the Popup battle agent (popupBattle), which is the UI panel for the battle, and the batle being processed (battle). It returns if the normal battle process should be skipped, as a bool. <br></br>
        /// If the normal battle process is skipped, this hook can be used to run an entirely diferent battle logic system. <br></br>
        /// No instance of this hook fires after the first to return true.
        /// </summary>
        /// <param name="battle"></param>
        /// <returns></returns>
        public virtual bool interceptAgentBattleStep(PopupBattleAgent popupBattle, BattleAgents battle, out bool battleOver)
        {
            battleOver = false;
            return false;
        }

        /// <summary>
        /// This hook fires once for each dead minion or empty minion slot of each agent in the battle that has an escort. It recieves the agent who is attempting to reinforce (ua), and the military escort (escort). It returns the Minion that will fill the agent's minion slot. <br></br>
        /// No instance of this hook will fire after the first to return a result that is not null.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="escort"></param>
        /// <returns></returns>
        public virtual Minion onAgentBattle_ReinforceFromEscort(UA ua, UM escort)
        {
            return null;
        }

        /// <summary>
        /// This hook fires after a PopupBattleAgent, the UI panel for a BattleAgents, populates itself with data from the battle it is duisplaying. This occurs both when the battle is intially setup, and during each battle step, for displayed battles.
        /// </summary>
        /// <param name="popupBattle"></param>
        public virtual void onPopupBattleAgent_Populate(PopupBattleAgent popupBattle, BattleAgents battle)
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
            return dmg;
        }

        /// <summary>
        /// This hook fires when the influence gain of a holy order is calculated, or when a mod calls the `checkIsProphetPlayerAligned` helper function in the Community Library's Mod Kernel. It recieves the order for which influence is being calculated (order), and the unit that is their prophet (prophet). It returns whether the prophet should be considered player controlled for the purposes of influence gain. <br></br>
        /// No hook is called after the first isnatnce to return true.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="prophet"></param>
        /// <returns></returns>
        public virtual bool onCheckIsProphetPlayerAligned(HolyOrder order, UA prophet)
        {
            return false;
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
        /// This hook fires when a settlement without a default relgion source is selected. It recieves the location of the settlement (loc). It returns the holy order that the view holy order button should be linked to.<br></br>
        /// No hook after the first that does not return null, will be called.
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public virtual HolyOrder onLocationViewFaithButton_GetHolyOrder(Location loc)
        {
            return null;
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
        /// This hook fires when a settlement has computed its shadow gain. It recives the settlement (set), the list of reasonMsgs (msgs), and the current total shadow gain (shadoiwGain). It returns the new total shadow gain.<br></br>
        /// If a mod earlier in the load order modifies a settlement's shadow gain, the msgs and shadowGain parameters will already include the modifications.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="msgs"></param>
        /// <param name="shadowGain"></param>
        /// <returns></returns>
        public virtual double onSettlementComputesShadowGain(Settlement set, List<ReasonMsg> msgs, double shadowGain)
        {
            return shadowGain;
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
        public virtual List<TaskUIData> onUIScroll_Unit_populateUM(UM um)
        {
            return null;
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
        /// This hook fires after an action taking monster social group starts a new monster action. It recieves the action taking monster social group (monster).
        /// </summary>
        /// <param name="actionTakingMonster"></param>
        public virtual void onActionTakingMonsterAIDecision(SG_ActionTakingMonster monster)
        {
            return;
        }

        /// <summary>
        /// This hook fires after a society social group with a sovereign starts a new national action. It recieves the society social group (society) and the sovereign (sovereign).
        /// </summary>
        /// <param name="society"></param>
        /// <param name="sovereign"></param>
        public virtual void onSovereignAIDecision(Society society, Person sovereign)
        {
            return;
        }

        /// <summary>
        /// This hook fires after the Challenge.getProgressPerTurn function is called. It recieves the Challenge (challenge) for which it is being called, the agent that is or will be performing the challenge (ua), the list of reason messages that are displayed to the player (reasons), and the progress that will be made (progress). It returns the progress that will be mae after modification.
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="ua"></param>
        /// <param name="reasons"></param>
        /// <param name="change"></param>
        /// <returns></returns>
        public virtual double onGetChallengeProgressPerTurn(Challenge challenge, UA ua, List<ReasonMsg> reasons, double progress)
        {
            return progress;
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

        /// <summary>
        /// This hook fires when the Broken Maker's powers "P_Eternity_CreateAgent" and "P_Eternity_CreateAgentReusable" are used to create an agent. It recieves the curse that is being processed (curse), the person of the agent (person), their location (location), and the text of the message that will be displayed to the player (text). It returns the text that will be displayed to the player.<br></br>
        /// If the agent waa created using the "P_Eternity_CreateAgentReusable" power, the power has already been removed from the house. If created with the "P_Eternity_CreateAgent" power, the curse count has already been halved in strength.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="location"></param>
        public virtual string onBrokenMakerPowerCreatesAgent_ProcessCurse(Curse curse, Person person, Location location, string text)
        {
            return text;
        }

        /// <summary>
        /// This hook fires when the revivePerson function is attempting to determine what agent should be produced for a person who does not have an Agent (UA) assigned to them. It recieves the Person (person) that is being revived, and the Location (location) at which they should be revived. It should return the intended Agent for that person, or null. <br></br>
        /// Hooks after the first to return something other than null do not fire. If all mods return null, a new agent will be selected based on hard-coded defaults.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public virtual UA onRevivePerson_CreateAgent(Person person, Location location)
        {
            return null;
        }

        /// <summary>
        /// This hook fires after the revivePerson function has revived a person. It recieves the Person that is being revived (person), and their current Location (location).
        /// <para>It is intended to provide dependent mods the opportunity to reset any values that the base function does not know exists.</para>
        /// </summary>
        /// <param name="person"></param>
        /// <param name="location"></param>
        public virtual void onRevivePerson_EndOfProcess(Person person, Location location)
        {
            return;
        }
    }
}
