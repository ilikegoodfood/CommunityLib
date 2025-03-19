using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    public class HooksDelegateRegistry
    {
        //===========================================================================
        // Custom delegate type definitions (for hooks with out or ref parameters)
        //===========================================================================

        public delegate void OnMapGen_PlaceWonders_2Delegate(Type t, out bool failedToPlaceWonder);

        public delegate bool InterceptAgentBattleStepDelegate(PopupBattleAgent popupBattle, BattleAgents battle, out bool battleOver);

        public delegate bool InterceptAgentAI_GetChallengeUtilityDelegate(
            UA ua,
            AgentAI.AIData aiData,
            AgentAI.ChallengeData challengeData,
            ref double utility,
            List<ReasonMsg> reasonMsgs);

        public delegate bool InterceptAgentAI_GetTaskUtilityDelegate(
            UA ua,
            AgentAI.AIData aiData,
            AgentAI.TaskData taskData,
            ref double utility,
            List<ReasonMsg> reasonMsgs);

        //===========================================================================
        // Delegate collections and registration methods
        //===========================================================================

        // onMapGen_PlaceWonders (overload 1)
        
        private List<Func<List<WonderData>>> _delegate_onMapGen_PlaceWonders_1 = new List<Func<List<WonderData>>>();
        public List<Func<List<WonderData>>> Delegate_onMapGen_PlaceWonders_1 { get { return _delegate_onMapGen_PlaceWonders_1; } }
        public bool RegisterHook_onMapGen_PlaceWonders_1(Func<List<WonderData>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onMapGen_PlaceWonders_1.Contains(hook))
                return false;
            _delegate_onMapGen_PlaceWonders_1.Add(hook);
            return true;
        }

        // onMapGen_PlaceWonders (overload 2)
        
        private List<OnMapGen_PlaceWonders_2Delegate> _delegate_onMapGen_PlaceWonders_2 = new List<OnMapGen_PlaceWonders_2Delegate>();
        public List<OnMapGen_PlaceWonders_2Delegate> Delegate_onMapGen_PlaceWonders_2 { get { return _delegate_onMapGen_PlaceWonders_2; } }
        public bool RegisterHook_onMapGen_PlaceWonders_2(OnMapGen_PlaceWonders_2Delegate hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onMapGen_PlaceWonders_2.Contains(hook))
                return false;
            _delegate_onMapGen_PlaceWonders_2.Add(hook);
            return true;
        }

        // onGraphicalUnitUpdated
        
        private List<Action<GraphicalUnit>> _delegate_onGraphicalUnitUpdated = new List<Action<GraphicalUnit>>();
        public List<Action<GraphicalUnit>> Delegate_onGraphicalUnitUpdated { get { return _delegate_onGraphicalUnitUpdated; } }
        public bool RegisterHook_onGraphicalUnitUpdated(Action<GraphicalUnit> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGraphicalUnitUpdated.Contains(hook))
                return false;
            _delegate_onGraphicalUnitUpdated.Add(hook);
            return true;
        }

        // onGraphicalLinkUpdated
        
        private List<Action<GraphicalLink>> _delegate_onGraphicalLinkUpdated = new List<Action<GraphicalLink>>();
        public List<Action<GraphicalLink>> Delegate_onGraphicalLinkUpdated { get { return _delegate_onGraphicalLinkUpdated; } }
        public bool RegisterHook_onGraphicalLinkUpdated(Action<GraphicalLink> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGraphicalLinkUpdated.Contains(hook))
                return false;
            _delegate_onGraphicalLinkUpdated.Add(hook);
            return true;
        }

        // onPopulatingPathfindingDelegates
        private List<Action<Location, Unit, List<Func<Location[], Location, Unit, double>>>> _delegate_onPopulatingPathfindingDelegates = new List<Action<Location, Unit, List<Func<Location[], Location, Unit, double>>>>();
        public List<Action<Location, Unit, List<Func<Location[], Location, Unit, double>>>> Delegate_onPopulatingPathfindingDelegates
        {
            get { return _delegate_onPopulatingPathfindingDelegates; }
        }
        public bool RegisterHook_onPopulatingPathfindingDelegates(Action<Location, Unit, List<Func<Location[], Location, Unit, double>>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopulatingPathfindingDelegates.Contains(hook))
                return false;
            _delegate_onPopulatingPathfindingDelegates.Add(hook);
            return true;
        }

        // onPathfinding_AllowSecondPass
        private List<Func<Location, Unit, List<Func<Location[], Location, Unit, double>>, bool>> _delegate_onPathfinding_AllowSecondPass = new List<Func<Location, Unit, List<Func<Location[], Location, Unit, double>>, bool>>();
        public List<Func<Location, Unit, List<Func<Location[], Location, Unit, double>>, bool>> Delegate_onPathfinding_AllowSecondPass
        {
            get { return _delegate_onPathfinding_AllowSecondPass; }
        }
        public bool RegisterHook_onPathfinding_AllowSecondPass(Func<Location, Unit, List<Func<Location[], Location, Unit, double>>, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPathfinding_AllowSecondPass.Contains(hook))
                return false;
            _delegate_onPathfinding_AllowSecondPass.Add(hook);
            return true;
        }

        // onGetTradeRouteEndpoints
        
        private List<Action<Map, List<Location>>> _delegate_onGetTradeRouteEndpoints = new List<Action<Map, List<Location>>>();
        public List<Action<Map, List<Location>>> Delegate_onGetTradeRouteEndpoints { get { return _delegate_onGetTradeRouteEndpoints; } }
        public bool RegisterHook_onGetTradeRouteEndpoints(Action<Map, List<Location>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGetTradeRouteEndpoints.Contains(hook))
                return false;
            _delegate_onGetTradeRouteEndpoints.Add(hook);
            return true;
        }

        // onBuildTradeNetwork_EndOfProcess
        
        private List<Action<Map, ManagerTrade, List<Location>>> _delegate_onBuildTradeNetwork_EndOfProcess = new List<Action<Map, ManagerTrade, List<Location>>>();
        public List<Action<Map, ManagerTrade, List<Location>>> Delegate_onBuildTradeNetwork_EndOfProcess { get { return _delegate_onBuildTradeNetwork_EndOfProcess; } }
        public bool RegisterHook_onBuildTradeNetwork_EndOfProcess(Action<Map, ManagerTrade, List<Location>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onBuildTradeNetwork_EndOfProcess.Contains(hook))
                return false;
            _delegate_onBuildTradeNetwork_EndOfProcess.Add(hook);
            return true;
        }

        // onPopulatingTradeRoutePathfindingDelegates
        private List<Action<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>>> _delegate_onPopulatingTradeRoutePathfindingDelegates = new List<Action<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>>>();
        public List<Action<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>>> Delegate_onPopulatingTradeRoutePathfindingDelegates
        {
            get { return _delegate_onPopulatingTradeRoutePathfindingDelegates; }
        }
        public bool RegisterHook_onPopulatingTradeRoutePathfindingDelegates(
            Action<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopulatingTradeRoutePathfindingDelegates.Contains(hook))
                return false;
            _delegate_onPopulatingTradeRoutePathfindingDelegates.Add(hook);
            return true;
        }

        // onPathfindingTadeRoute_AllowSecondPass
        private List<Func<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>, bool>> _delegate_onPathfindingTadeRoute_AllowSecondPass = new List<Func<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>, bool>>();
        public List<Func<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>, bool>> Delegate_onPathfindingTadeRoute_AllowSecondPass
        {
            get { return _delegate_onPathfindingTadeRoute_AllowSecondPass; }
        }
        public bool RegisterHook_onPathfindingTadeRoute_AllowSecondPass(
            Func<Location, List<Func<Location[], Location, double>>, List<Func<Location[], Location, bool>>, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPathfindingTadeRoute_AllowSecondPass.Contains(hook))
                return false;
            _delegate_onPathfindingTadeRoute_AllowSecondPass.Add(hook);
            return true;
        }

        // onMoveTaken
        
        private List<Action<Unit, Location, Location>> _delegate_onMoveTaken = new List<Action<Unit, Location, Location>>();
        public List<Action<Unit, Location, Location>> Delegate_onMoveTaken { get { return _delegate_onMoveTaken; } }
        public bool RegisterHook_onMoveTaken(Action<Unit, Location, Location> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onMoveTaken.Contains(hook))
                return false;
            _delegate_onMoveTaken.Add(hook);
            return true;
        }

        // onUnitAI_GetsDistanceToLocation
        
        private List<Func<Unit, Location, Location[], int, int>> _delegate_onUnitAI_GetsDistanceToLocation = new List<Func<Unit, Location, Location[], int, int>>();
        public List<Func<Unit, Location, Location[], int, int>> Delegate_onUnitAI_GetsDistanceToLocation { get { return _delegate_onUnitAI_GetsDistanceToLocation; } }
        public bool RegisterHook_onUnitAI_GetsDistanceToLocation(Func<Unit, Location, Location[], int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onUnitAI_GetsDistanceToLocation.Contains(hook))
                return false;
            _delegate_onUnitAI_GetsDistanceToLocation.Add(hook);
            return true;
        }

        // isUnitSubsumed
        
        private List<Func<Unit, Unit, bool>> _delegate_isUnitSubsumed = new List<Func<Unit, Unit, bool>>();
        public List<Func<Unit, Unit, bool>> Delegate_isUnitSubsumed { get { return _delegate_isUnitSubsumed; } }
        public bool RegisterHook_isUnitSubsumed(Func<Unit, Unit, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_isUnitSubsumed.Contains(hook))
                return false;
            _delegate_isUnitSubsumed.Add(hook);
            return true;
        }

        // onCalculateAgentsUsed
        
        private List<Func<List<Unit>, int, int>> _delegate_onCalculateAgentsUsed = new List<Func<List<Unit>, int, int>>();
        public List<Func<List<Unit>, int, int>> Delegate_onCalculateAgentsUsed { get { return _delegate_onCalculateAgentsUsed; } }
        public bool RegisterHook_onCalculateAgentsUsed(Func<List<Unit>, int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onCalculateAgentsUsed.Contains(hook))
                return false;
            _delegate_onCalculateAgentsUsed.Add(hook);
            return true;
        }

        // onGetAgentCaps
        
        private List<Action<God, int[]>> _delegate_onGetAgentCaps = new List<Action<God, int[]>>();
        public List<Action<God, int[]>> Delegate_onGetAgentCaps { get { return _delegate_onGetAgentCaps; } }
        public bool RegisterHook_onGetAgentCaps(Action<God, int[]> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGetAgentCaps.Contains(hook))
                return false;
            _delegate_onGetAgentCaps.Add(hook);
            return true;
        }

        // onGetMaxPower
        
        private List<Func<God, int, int>> _delegate_onGetMaxPower = new List<Func<God, int, int>>();
        public List<Func<God, int, int>> Delegate_onGetMaxPower { get { return _delegate_onGetMaxPower; } }
        public bool RegisterHook_onGetMaxPower(Func<God, int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGetMaxPower.Contains(hook))
                return false;
            _delegate_onGetMaxPower.Add(hook);
            return true;
        }

        // interceptUnitDeath
        
        private List<Func<Unit, string, Person, bool>> _delegate_interceptUnitDeath = new List<Func<Unit, string, Person, bool>>();
        public List<Func<Unit, string, Person, bool>> Delegate_interceptUnitDeath { get { return _delegate_interceptUnitDeath; } }
        public bool RegisterHook_interceptUnitDeath(Func<Unit, string, Person, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptUnitDeath.Contains(hook))
                return false;
            _delegate_interceptUnitDeath.Add(hook);
            return true;
        }

        // onUnitDeath_StartOfProcess
        
        private List<Action<Unit, string, Person>> _delegate_onUnitDeath_StartOfProcess = new List<Action<Unit, string, Person>>();
        public List<Action<Unit, string, Person>> Delegate_onUnitDeath_StartOfProcess { get { return _delegate_onUnitDeath_StartOfProcess; } }
        public bool RegisterHook_onUnitDeath_StartOfProcess(Action<Unit, string, Person> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onUnitDeath_StartOfProcess.Contains(hook))
                return false;
            _delegate_onUnitDeath_StartOfProcess.Add(hook);
            return true;
        }

        // interceptArmyBattleCycle
        
        private List<Func<BattleArmy, bool>> _delegate_interceptArmyBattleCycle = new List<Func<BattleArmy, bool>>();
        public List<Func<BattleArmy, bool>> Delegate_interceptArmyBattleCycle { get { return _delegate_interceptArmyBattleCycle; } }
        public bool RegisterHook_interceptArmyBattleCycle(Func<BattleArmy, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptArmyBattleCycle.Contains(hook))
                return false;
            _delegate_interceptArmyBattleCycle.Add(hook);
            return true;
        }

        // onArmyBattleCycle_StartOfProcess
        
        private List<Action<BattleArmy>> _delegate_onArmyBattleCycle_StartOfProcess = new List<Action<BattleArmy>>();
        public List<Action<BattleArmy>> Delegate_onArmyBattleCycle_StartOfProcess { get { return _delegate_onArmyBattleCycle_StartOfProcess; } }
        public bool RegisterHook_onArmyBattleCycle_StartOfProcess(Action<BattleArmy> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleCycle_StartOfProcess.Contains(hook))
                return false;
            _delegate_onArmyBattleCycle_StartOfProcess.Add(hook);
            return true;
        }

        // onArmyBattleCycle_EndOfProcess
        
        private List<Action<BattleArmy>> _delegate_onArmyBattleCycle_EndOfProcess = new List<Action<BattleArmy>>();
        public List<Action<BattleArmy>> Delegate_onArmyBattleCycle_EndOfProcess { get { return _delegate_onArmyBattleCycle_EndOfProcess; } }
        public bool RegisterHook_onArmyBattleCycle_EndOfProcess(Action<BattleArmy> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleCycle_EndOfProcess.Contains(hook))
                return false;
            _delegate_onArmyBattleCycle_EndOfProcess.Add(hook);
            return true;
        }

        // onArmyBattleCycle_ComputeAdvantage
        
        private List<Func<BattleArmy, double, double>> _delegate_onArmyBattleCycle_ComputeAdvantage = new List<Func<BattleArmy, double, double>>();
        public List<Func<BattleArmy, double, double>> Delegate_onArmyBattleCycle_ComputeAdvantage { get { return _delegate_onArmyBattleCycle_ComputeAdvantage; } }
        public bool RegisterHook_onArmyBattleCycle_ComputeAdvantage(Func<BattleArmy, double, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleCycle_ComputeAdvantage.Contains(hook))
                return false;
            _delegate_onArmyBattleCycle_ComputeAdvantage.Add(hook);
            return true;
        }

        // onArmyBattleCycle_DamageCalculated
        
        private List<Func<BattleArmy, int, UM, UM, int>> _delegate_onArmyBattleCycle_DamageCalculated = new List<Func<BattleArmy, int, UM, UM, int>>();
        public List<Func<BattleArmy, int, UM, UM, int>> Delegate_onArmyBattleCycle_DamageCalculated { get { return _delegate_onArmyBattleCycle_DamageCalculated; } }
        public bool RegisterHook_onArmyBattleCycle_DamageCalculated(Func<BattleArmy, int, UM, UM, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleCycle_DamageCalculated.Contains(hook))
                return false;
            _delegate_onArmyBattleCycle_DamageCalculated.Add(hook);
            return true;
        }

        // onArmyBattleCycle_AllocateDamage
        
        private List<Action<BattleArmy, List<UM>, int[]>> _delegate_onArmyBattleCycle_AllocateDamage = new List<Action<BattleArmy, List<UM>, int[]>>();
        public List<Action<BattleArmy, List<UM>, int[]>> Delegate_onArmyBattleCycle_AllocateDamage { get { return _delegate_onArmyBattleCycle_AllocateDamage; } }
        public bool RegisterHook_onArmyBattleCycle_AllocateDamage(Action<BattleArmy, List<UM>, int[]> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleCycle_AllocateDamage.Contains(hook))
                return false;
            _delegate_onArmyBattleCycle_AllocateDamage.Add(hook);
            return true;
        }

        // onArmyBattleVictory
        
        private List<Action<BattleArmy, List<UM>, List<UA>, List<UM>, List<UA>>> _delegate_onArmyBattleVictory = new List<Action<BattleArmy, List<UM>, List<UA>, List<UM>, List<UA>>>();
        public List<Action<BattleArmy, List<UM>, List<UA>, List<UM>, List<UA>>> Delegate_onArmyBattleVictory { get { return _delegate_onArmyBattleVictory; } }
        public bool RegisterHook_onArmyBattleVictory(Action<BattleArmy, List<UM>, List<UA>, List<UM>, List<UA>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleVictory.Contains(hook))
                return false;
            _delegate_onArmyBattleVictory.Add(hook);
            return true;
        }

        // onArmyBattleTerminated
        
        private List<Action<BattleArmy, List<UM>, List<UA>, UM>> _delegate_onArmyBattleTerminated = new List<Action<BattleArmy, List<UM>, List<UA>, UM>>();
        public List<Action<BattleArmy, List<UM>, List<UA>, UM>> Delegate_onArmyBattleTerminated { get { return _delegate_onArmyBattleTerminated; } }
        public bool RegisterHook_onArmyBattleTerminated(Action<BattleArmy, List<UM>, List<UA>, UM> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleTerminated.Contains(hook))
                return false;
            _delegate_onArmyBattleTerminated.Add(hook);
            return true;
        }

        // onArmyBattleRetreatOrFlee
        
        private List<Action<BattleArmy, Unit>> _delegate_onArmyBattleRetreatOrFlee = new List<Action<BattleArmy, Unit>>();
        public List<Action<BattleArmy, Unit>> Delegate_onArmyBattleRetreatOrFlee { get { return _delegate_onArmyBattleRetreatOrFlee; } }
        public bool RegisterHook_onArmyBattleRetreatOrFlee(Action<BattleArmy, Unit> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onArmyBattleRetreatOrFlee.Contains(hook))
                return false;
            _delegate_onArmyBattleRetreatOrFlee.Add(hook);
            return true;
        }

        // onUnitReceivesArmyBattleDamage
        
        private List<Func<BattleArmy, UM, int, int>> _delegate_onUnitReceivesArmyBattleDamage = new List<Func<BattleArmy, UM, int, int>>();
        public List<Func<BattleArmy, UM, int, int>> Delegate_onUnitReceivesArmyBattleDamage { get { return _delegate_onUnitReceivesArmyBattleDamage; } }
        public bool RegisterHook_onUnitReceivesArmyBattleDamage(Func<BattleArmy, UM, int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onUnitReceivesArmyBattleDamage.Contains(hook))
                return false;
            _delegate_onUnitReceivesArmyBattleDamage.Add(hook);
            return true;
        }

        // onAgentBattleStarts
        
        private List<Func<UA, UA, BattleAgents>> _delegate_onAgentBattleStarts = new List<Func<UA, UA, BattleAgents>>();
        public List<Func<UA, UA, BattleAgents>> Delegate_onAgentBattleStarts { get { return _delegate_onAgentBattleStarts; } }
        public bool RegisterHook_onAgentBattleStarts(Func<UA, UA, BattleAgents> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentBattleStarts.Contains(hook))
                return false;
            _delegate_onAgentBattleStarts.Add(hook);
            return true;
        }

        // onAgentBattle_Setup
        
        private List<Action<BattleAgents>> _delegate_onAgentBattle_Setup = new List<Action<BattleAgents>>();
        public List<Action<BattleAgents>> Delegate_onAgentBattle_Setup { get { return _delegate_onAgentBattle_Setup; } }
        public bool RegisterHook_onAgentBattle_Setup(Action<BattleAgents> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentBattle_Setup.Contains(hook))
                return false;
            _delegate_onAgentBattle_Setup.Add(hook);
            return true;
        }

        // interceptAgentBattleAutomatic
        
        private List<Func<BattleAgents, bool>> _delegate_interceptAgentBattleAutomatic = new List<Func<BattleAgents, bool>>();
        public List<Func<BattleAgents, bool>> Delegate_interceptAgentBattleAutomatic { get { return _delegate_interceptAgentBattleAutomatic; } }
        public bool RegisterHook_interceptAgentBattleAutomatic(Func<BattleAgents, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptAgentBattleAutomatic.Contains(hook))
                return false;
            _delegate_interceptAgentBattleAutomatic.Add(hook);
            return true;
        }

        // interceptAgentBattleStep
        
        private List<InterceptAgentBattleStepDelegate> _delegate_interceptAgentBattleStep = new List<InterceptAgentBattleStepDelegate>();
        public List<InterceptAgentBattleStepDelegate> Delegate_interceptAgentBattleStep { get { return _delegate_interceptAgentBattleStep; } }
        public bool RegisterHook_interceptAgentBattleStep(InterceptAgentBattleStepDelegate hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptAgentBattleStep.Contains(hook))
                return false;
            _delegate_interceptAgentBattleStep.Add(hook);
            return true;
        }

        // onAgentBattle_ReinforceFromEscort
        
        private List<Func<UA, UM, Minion>> _delegate_onAgentBattle_ReinforceFromEscort = new List<Func<UA, UM, Minion>>();
        public List<Func<UA, UM, Minion>> Delegate_onAgentBattle_ReinforceFromEscort { get { return _delegate_onAgentBattle_ReinforceFromEscort; } }
        public bool RegisterHook_onAgentBattle_ReinforceFromEscort(Func<UA, UM, Minion> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentBattle_ReinforceFromEscort.Contains(hook))
                return false;
            _delegate_onAgentBattle_ReinforceFromEscort.Add(hook);
            return true;
        }

        // onPopupBattleAgent_Populate
        
        private List<Action<PopupBattleAgent, BattleAgents>> _delegate_onPopupBattleAgent_Populate = new List<Action<PopupBattleAgent, BattleAgents>>();
        public List<Action<PopupBattleAgent, BattleAgents>> Delegate_onPopupBattleAgent_Populate { get { return _delegate_onPopupBattleAgent_Populate; } }
        public bool RegisterHook_onPopupBattleAgent_Populate(Action<PopupBattleAgent, BattleAgents> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupBattleAgent_Populate.Contains(hook))
                return false;
            _delegate_onPopupBattleAgent_Populate.Add(hook);
            return true;
        }

        // onMinionAttackAboutToBePerformed
        
        private List<Func<Minion, UA, PopupBattleAgent, BattleAgents, int, int, int>> _delegate_onMinionAttackAboutToBePerformed = new List<Func<Minion, UA, PopupBattleAgent, BattleAgents, int, int, int>>();
        public List<Func<Minion, UA, PopupBattleAgent, BattleAgents, int, int, int>> Delegate_onMinionAttackAboutToBePerformed { get { return _delegate_onMinionAttackAboutToBePerformed; } }
        public bool RegisterHook_onMinionAttackAboutToBePerformed(Func<Minion, UA, PopupBattleAgent, BattleAgents, int, int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onMinionAttackAboutToBePerformed.Contains(hook))
                return false;
            _delegate_onMinionAttackAboutToBePerformed.Add(hook);
            return true;
        }

        // onAgentBattle_ReceiveDamage
        
        private List<Func<PopupBattleAgent, BattleAgents, UA, Minion, int, int, int>> _delegate_onAgentBattle_ReceiveDamage = new List<Func<PopupBattleAgent, BattleAgents, UA, Minion, int, int, int>>();
        public List<Func<PopupBattleAgent, BattleAgents, UA, Minion, int, int, int>> Delegate_onAgentBattle_ReceiveDamage { get { return _delegate_onAgentBattle_ReceiveDamage; } }
        public bool RegisterHook_onAgentBattle_ReceiveDamage(Func<PopupBattleAgent, BattleAgents, UA, Minion, int, int, int> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentBattle_ReceiveDamage.Contains(hook))
                return false;
            _delegate_onAgentBattle_ReceiveDamage.Add(hook);
            return true;
        }

        // onCheckIsProphetPlayerAligned
        
        private List<Func<HolyOrder, UA, bool>> _delegate_onCheckIsProphetPlayerAligned = new List<Func<HolyOrder, UA, bool>>();
        public List<Func<HolyOrder, UA, bool>> Delegate_onCheckIsProphetPlayerAligned { get { return _delegate_onCheckIsProphetPlayerAligned; } }
        public bool RegisterHook_onCheckIsProphetPlayerAligned(Func<HolyOrder, UA, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onCheckIsProphetPlayerAligned.Contains(hook))
                return false;
            _delegate_onCheckIsProphetPlayerAligned.Add(hook);
            return true;
        }

        // onPlayerOpensReligionUI
        
        private List<Action<HolyOrder>> _delegate_onPlayerOpensReligionUI = new List<Action<HolyOrder>>();
        public List<Action<HolyOrder>> Delegate_onPlayerOpensReligionUI { get { return _delegate_onPlayerOpensReligionUI; } }
        public bool RegisterHook_onPlayerOpensReligionUI(Action<HolyOrder> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPlayerOpensReligionUI.Contains(hook))
                return false;
            _delegate_onPlayerOpensReligionUI.Add(hook);
            return true;
        }

        // onPopupHolyOrder_DisplayBudget
        
        private List<Action<HolyOrder, List<ReasonMsg>>> _delegate_onPopupHolyOrder_DisplayBudget = new List<Action<HolyOrder, List<ReasonMsg>>>();
        public List<Action<HolyOrder, List<ReasonMsg>>> Delegate_onPopupHolyOrder_DisplayBudget { get { return _delegate_onPopupHolyOrder_DisplayBudget; } }
        public bool RegisterHook_onPopupHolyOrder_DisplayBudget(Action<HolyOrder, List<ReasonMsg>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupHolyOrder_DisplayBudget.Contains(hook))
                return false;
            _delegate_onPopupHolyOrder_DisplayBudget.Add(hook);
            return true;
        }

        // onPopupHolyOrder_DisplayStats
        
        private List<Action<HolyOrder, List<ReasonMsg>>> _delegate_onPopupHolyOrder_DisplayStats = new List<Action<HolyOrder, List<ReasonMsg>>>();
        public List<Action<HolyOrder, List<ReasonMsg>>> Delegate_onPopupHolyOrder_DisplayStats { get { return _delegate_onPopupHolyOrder_DisplayStats; } }
        public bool RegisterHook_onPopupHolyOrder_DisplayStats(Action<HolyOrder, List<ReasonMsg>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupHolyOrder_DisplayStats.Contains(hook))
                return false;
            _delegate_onPopupHolyOrder_DisplayStats.Add(hook);
            return true;
        }

        // onPopupHolyOrder_DisplayInfluenceElder
        
        private List<Func<HolyOrder, string, int, string>> _delegate_onPopupHolyOrder_DisplayInfluenceElder = new List<Func<HolyOrder, string, int, string>>();
        public List<Func<HolyOrder, string, int, string>> Delegate_onPopupHolyOrder_DisplayInfluenceElder { get { return _delegate_onPopupHolyOrder_DisplayInfluenceElder; } }
        public bool RegisterHook_onPopupHolyOrder_DisplayInfluenceElder(Func<HolyOrder, string, int, string> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupHolyOrder_DisplayInfluenceElder.Contains(hook))
                return false;
            _delegate_onPopupHolyOrder_DisplayInfluenceElder.Add(hook);
            return true;
        }

        // onPopupHolyOrder_DisplayInfluenceHuman
        
        private List<Func<HolyOrder, string, int, string>> _delegate_onPopupHolyOrder_DisplayInfluenceHuman = new List<Func<HolyOrder, string, int, string>>();
        public List<Func<HolyOrder, string, int, string>> Delegate_onPopupHolyOrder_DisplayInfluenceHuman { get { return _delegate_onPopupHolyOrder_DisplayInfluenceHuman; } }
        public bool RegisterHook_onPopupHolyOrder_DisplayInfluenceHuman(Func<HolyOrder, string, int, string> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupHolyOrder_DisplayInfluenceHuman.Contains(hook))
                return false;
            _delegate_onPopupHolyOrder_DisplayInfluenceHuman.Add(hook);
            return true;
        }

        // onPopupHolyOrder_DisplayPageText
        
        private List<Func<HolyOrder, string, int, string>> _delegate_onPopupHolyOrder_DisplayPageText = new List<Func<HolyOrder, string, int, string>>();
        public List<Func<HolyOrder, string, int, string>> Delegate_onPopupHolyOrder_DisplayPageText { get { return _delegate_onPopupHolyOrder_DisplayPageText; } }
        public bool RegisterHook_onPopupHolyOrder_DisplayPageText(Func<HolyOrder, string, int, string> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPopupHolyOrder_DisplayPageText.Contains(hook))
                return false;
            _delegate_onPopupHolyOrder_DisplayPageText.Add(hook);
            return true;
        }

        // onPlayerInfluenceTenet
        
        private List<Action<HolyOrder, HolyTenet>> _delegate_onPlayerInfluenceTenet = new List<Action<HolyOrder, HolyTenet>>();
        public List<Action<HolyOrder, HolyTenet>> Delegate_onPlayerInfluenceTenet { get { return _delegate_onPlayerInfluenceTenet; } }
        public bool RegisterHook_onPlayerInfluenceTenet(Action<HolyOrder, HolyTenet> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onPlayerInfluenceTenet.Contains(hook))
                return false;
            _delegate_onPlayerInfluenceTenet.Add(hook);
            return true;
        }

        // onLocationViewFaithButton_GetHolyOrder
        private List<Func<Location, HolyOrder>> _delegate_onLocationViewFaithButton_GetHolyOrder = new List<Func<Location, HolyOrder>>();
        public List<Func<Location, HolyOrder>> Delegate_onLocationViewFaithButton_GetHolyOrder { get { return _delegate_onLocationViewFaithButton_GetHolyOrder; } }
        public bool RegisterHook_onLocationViewFaithButton_GetHolyOrder(Func<Location, HolyOrder> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onLocationViewFaithButton_GetHolyOrder.Contains(hook))
                return false;
            _delegate_onLocationViewFaithButton_GetHolyOrder.Add(hook);
            return true;
        }

        // onRazeLocation_StartOfProcess
        private List<Action<UM>> _delegate_onRazeLocation_StartOfProcess = new List<Action<UM>>();
        public List<Action<UM>> Delegate_onRazeLocation_StartOfProcess { get { return _delegate_onRazeLocation_StartOfProcess; } }
        public bool RegisterHook_onRazeLocation_StartOfProcess(Action<UM> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onRazeLocation_StartOfProcess.Contains(hook))
                return false;
            _delegate_onRazeLocation_StartOfProcess.Add(hook);
            return true;
        }

        // onRazeLocation_EndOfProcess
        private List<Action<UM>> _delegate_onRazeLocation_EndOfProcess = new List<Action<UM>>();
        public List<Action<UM>> Delegate_onRazeLocation_EndOfProcess { get { return _delegate_onRazeLocation_EndOfProcess; } }
        public bool RegisterHook_onRazeLocation_EndOfProcess(Action<UM> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onRazeLocation_EndOfProcess.Contains(hook))
                return false;
            _delegate_onRazeLocation_EndOfProcess.Add(hook);
            return true;
        }

        // interceptSettlementFallIntoRuin
        private List<Func<Settlement, string, object, bool>> _delegate_interceptSettlementFallIntoRuin = new List<Func<Settlement, string, object, bool>>();
        public List<Func<Settlement, string, object, bool>> Delegate_interceptSettlementFallIntoRuin { get { return _delegate_interceptSettlementFallIntoRuin; } }
        public bool RegisterHook_interceptSettlementFallIntoRuin(Func<Settlement, string, object, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptSettlementFallIntoRuin.Contains(hook))
                return false;
            _delegate_interceptSettlementFallIntoRuin.Add(hook);
            return true;
        }

        // onSettlementFallIntoRuin_StartOfProcess
        private List<Action<Settlement, string, object>> _delegate_onSettlementFallIntoRuin_StartOfProcess = new List<Action<Settlement, string, object>>();
        public List<Action<Settlement, string, object>> Delegate_onSettlementFallIntoRuin_StartOfProcess { get { return _delegate_onSettlementFallIntoRuin_StartOfProcess; } }
        public bool RegisterHook_onSettlementFallIntoRuin_StartOfProcess(Action<Settlement, string, object> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onSettlementFallIntoRuin_StartOfProcess.Contains(hook))
                return false;
            _delegate_onSettlementFallIntoRuin_StartOfProcess.Add(hook);
            return true;
        }

        // onSettlementFallIntoRuin_EndOfProcess
        private List<Action<Settlement, string, object>> _delegate_onSettlementFallIntoRuin_EndOfProcess = new List<Action<Settlement, string, object>>();
        public List<Action<Settlement, string, object>> Delegate_onSettlementFallIntoRuin_EndOfProcess { get { return _delegate_onSettlementFallIntoRuin_EndOfProcess; } }
        public bool RegisterHook_onSettlementFallIntoRuin_EndOfProcess(Action<Settlement, string, object> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onSettlementFallIntoRuin_EndOfProcess.Contains(hook))
                return false;
            _delegate_onSettlementFallIntoRuin_EndOfProcess.Add(hook);
            return true;
        }

        // onSettlementCalculatesShadowGain
        private List<Func<Settlement, List<ReasonMsg>, double, double>> _delegate_onSettlementCalculatesShadowGain = new List<Func<Settlement, List<ReasonMsg>, double, double>>();
        public List<Func<Settlement, List<ReasonMsg>, double, double>> Delegate_onSettlementCalculatesShadowGain { get { return _delegate_onSettlementCalculatesShadowGain; } }
        public bool RegisterHook_onSettlementCalculatesShadowGain(Func<Settlement, List<ReasonMsg>, double, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onSettlementCalculatesShadowGain.Contains(hook))
                return false;
            _delegate_onSettlementCalculatesShadowGain.Add(hook);
            return true;
        }

        // onAgentAI

        private List<Func<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>, bool>> _delegate_interceptAgentAI = new List<Func<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>, bool>>();
        public List<Func<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>, bool>> Delegate_interceptAgentAI { get { return _delegate_interceptAgentAI; } }
        public bool RegisterHook_interceptAgentAI(Func<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptAgentAI.Contains(hook))
                return false;
            _delegate_interceptAgentAI.Add(hook);
            return true;
        }

        // onAgentAI_StartOfProcess
        
        private List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>> _delegate_onAgentAI_StartOfProcess = new List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>>();
        public List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>> Delegate_onAgentAI_StartOfProcess { get { return _delegate_onAgentAI_StartOfProcess; } }
        public bool RegisterHook_onAgentAI_StartOfProcess(Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentAI_StartOfProcess.Contains(hook))
                return false;
            _delegate_onAgentAI_StartOfProcess.Add(hook);
            return true;
        }

        // onAgentAI_EndOfProcess
        
        private List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>> _delegate_onAgentAI_EndOfProcess = new List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>>();
        public List<Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>>> Delegate_onAgentAI_EndOfProcess { get { return _delegate_onAgentAI_EndOfProcess; } }
        public bool RegisterHook_onAgentAI_EndOfProcess(Action<UA, AgentAI.AIData, List<AgentAI.ChallengeData>, List<AgentAI.TaskData>, List<Unit>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentAI_EndOfProcess.Contains(hook))
                return false;
            _delegate_onAgentAI_EndOfProcess.Add(hook);
            return true;
        }

        // interceptAgentAI_GetChallengeUtility
        
        private List<InterceptAgentAI_GetChallengeUtilityDelegate> _delegate_interceptAgentAI_GetChallengeUtility = new List<InterceptAgentAI_GetChallengeUtilityDelegate>();
        public List<InterceptAgentAI_GetChallengeUtilityDelegate> Delegate_interceptAgentAI_GetChallengeUtility { get { return _delegate_interceptAgentAI_GetChallengeUtility; } }
        public bool RegisterHook_interceptAgentAI_GetChallengeUtility(InterceptAgentAI_GetChallengeUtilityDelegate hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptAgentAI_GetChallengeUtility.Contains(hook))
                return false;
            _delegate_interceptAgentAI_GetChallengeUtility.Add(hook);
            return true;
        }

        // onAgentAI_GetChallengeUtility
        
        private List<Func<UA, AgentAI.AIData, AgentAI.ChallengeData, double, List<ReasonMsg>, double>> _delegate_onAgentAI_GetChallengeUtility = new List<Func<UA, AgentAI.AIData, AgentAI.ChallengeData, double, List<ReasonMsg>, double>>();
        public List<Func<UA, AgentAI.AIData, AgentAI.ChallengeData, double, List<ReasonMsg>, double>> Delegate_onAgentAI_GetChallengeUtility { get { return _delegate_onAgentAI_GetChallengeUtility; } }
        public bool RegisterHook_onAgentAI_GetChallengeUtility(Func<UA, AgentAI.AIData, AgentAI.ChallengeData, double, List<ReasonMsg>, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentAI_GetChallengeUtility.Contains(hook))
                return false;
            _delegate_onAgentAI_GetChallengeUtility.Add(hook);
            return true;
        }

        // interceptAgentAI_GetTaskUtility
        
        private List<InterceptAgentAI_GetTaskUtilityDelegate> _delegate_interceptAgentAI_GetTaskUtility = new List<InterceptAgentAI_GetTaskUtilityDelegate>();
        public List<InterceptAgentAI_GetTaskUtilityDelegate> Delegate_interceptAgentAI_GetTaskUtility { get { return _delegate_interceptAgentAI_GetTaskUtility; } }
        public bool RegisterHook_interceptAgentAI_GetTaskUtility(InterceptAgentAI_GetTaskUtilityDelegate hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptAgentAI_GetTaskUtility.Contains(hook))
                return false;
            _delegate_interceptAgentAI_GetTaskUtility.Add(hook);
            return true;
        }

        // onAgentAI_GetTaskUtility
        
        private List<Func<UA, AgentAI.AIData, AgentAI.TaskData, double, List<ReasonMsg>, double>> _delegate_onAgentAI_GetTaskUtility = new List<Func<UA, AgentAI.AIData, AgentAI.TaskData, double, List<ReasonMsg>, double>>();
        public List<Func<UA, AgentAI.AIData, AgentAI.TaskData, double, List<ReasonMsg>, double>> Delegate_onAgentAI_GetTaskUtility { get { return _delegate_onAgentAI_GetTaskUtility; } }
        public bool RegisterHook_onAgentAI_GetTaskUtility(Func<UA, AgentAI.AIData, AgentAI.TaskData, double, List<ReasonMsg>, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentAI_GetTaskUtility.Contains(hook))
                return false;
            _delegate_onAgentAI_GetTaskUtility.Add(hook);
            return true;
        }

        // onAgentLevelup_GetTraits
        
        private List<Action<UA, List<Trait>, bool>> _delegate_onAgentLevelup_GetTraits = new List<Action<UA, List<Trait>, bool>>();
        public List<Action<UA, List<Trait>, bool>> Delegate_onAgentLevelup_GetTraits { get { return _delegate_onAgentLevelup_GetTraits; } }
        public bool RegisterHook_onAgentLevelup_GetTraits(Action<UA, List<Trait>, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentLevelup_GetTraits.Contains(hook))
                return false;
            _delegate_onAgentLevelup_GetTraits.Add(hook);
            return true;
        }

        // interceptGetVisibleUnits
        
        private List<Func<UA, List<Unit>, bool>> _delegate_interceptGetVisibleUnits = new List<Func<UA, List<Unit>, bool>>();
        public List<Func<UA, List<Unit>, bool>> Delegate_interceptGetVisibleUnits { get { return _delegate_interceptGetVisibleUnits; } }
        public bool RegisterHook_interceptGetVisibleUnits(Func<UA, List<Unit>, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptGetVisibleUnits.Contains(hook))
                return false;
            _delegate_interceptGetVisibleUnits.Add(hook);
            return true;
        }

        // getVisibleUnits_EndOfProcess
        
        private List<Func<UA, List<Unit>, List<Unit>>> _delegate_getVisibleUnits_EndOfProcess = new List<Func<UA, List<Unit>, List<Unit>>>();
        public List<Func<UA, List<Unit>, List<Unit>>> Delegate_getVisibleUnits_EndOfProcess { get { return _delegate_getVisibleUnits_EndOfProcess; } }
        public bool RegisterHook_getVisibleUnits_EndOfProcess(Func<UA, List<Unit>, List<Unit>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_getVisibleUnits_EndOfProcess.Contains(hook))
                return false;
            _delegate_getVisibleUnits_EndOfProcess.Add(hook);
            return true;
        }

        // interceptReplaceItem
        
        private List<Func<Person, Item, Item, bool, bool>> _delegate_interceptReplaceItem = new List<Func<Person, Item, Item, bool, bool>>();
        public List<Func<Person, Item, Item, bool, bool>> Delegate_interceptReplaceItem { get { return _delegate_interceptReplaceItem; } }
        public bool RegisterHook_interceptReplaceItem(Func<Person, Item, Item, bool, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_interceptReplaceItem.Contains(hook))
                return false;
            _delegate_interceptReplaceItem.Add(hook);
            return true;
        }

        // onUIScroll_Unit_populateUM
        
        private List<Func<UM, List<Hooks.TaskUIData>>> _delegate_onUIScroll_Unit_populateUM = new List<Func<UM, List<Hooks.TaskUIData>>>();
        public List<Func<UM, List<Hooks.TaskUIData>>> Delegate_onUIScroll_Unit_populateUM { get { return _delegate_onUIScroll_Unit_populateUM; } }
        public bool RegisterHook_onUIScroll_Unit_populateUM(Func<UM, List<Hooks.TaskUIData>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onUIScroll_Unit_populateUM.Contains(hook))
                return false;
            _delegate_onUIScroll_Unit_populateUM.Add(hook);
            return true;
        }

        // populatingMonsterActions
        
        private List<Action<SG_ActionTakingMonster, List<MonsterAction>>> _delegate_populatingMonsterActions = new List<Action<SG_ActionTakingMonster, List<MonsterAction>>>();
        public List<Action<SG_ActionTakingMonster, List<MonsterAction>>> Delegate_populatingMonsterActions { get { return _delegate_populatingMonsterActions; } }
        public bool RegisterHook_populatingMonsterActions(Action<SG_ActionTakingMonster, List<MonsterAction>> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_populatingMonsterActions.Contains(hook))
                return false;
            _delegate_populatingMonsterActions.Add(hook);
            return true;
        }

        // onActionTakingMonster_getUtility
        
        private List<Func<SG_ActionTakingMonster, MonsterAction, double, List<ReasonMsg>, double>> _delegate_onActionTakingMonster_getUtility = new List<Func<SG_ActionTakingMonster, MonsterAction, double, List<ReasonMsg>, double>>();
        public List<Func<SG_ActionTakingMonster, MonsterAction, double, List<ReasonMsg>, double>> Delegate_onActionTakingMonster_getUtility { get { return _delegate_onActionTakingMonster_getUtility; } }
        public bool RegisterHook_onActionTakingMonster_getUtility(Func<SG_ActionTakingMonster, MonsterAction, double, List<ReasonMsg>, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onActionTakingMonster_getUtility.Contains(hook))
                return false;
            _delegate_onActionTakingMonster_getUtility.Add(hook);
            return true;
        }

        // onActionTakingMonsterAIDecision
        
        private List<Action<SG_ActionTakingMonster>> _delegate_onActionTakingMonsterAIDecision = new List<Action<SG_ActionTakingMonster>>();
        public List<Action<SG_ActionTakingMonster>> Delegate_onActionTakingMonsterAIDecision { get { return _delegate_onActionTakingMonsterAIDecision; } }
        public bool RegisterHook_onActionTakingMonsterAIDecision(Action<SG_ActionTakingMonster> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onActionTakingMonsterAIDecision.Contains(hook))
                return false;
            _delegate_onActionTakingMonsterAIDecision.Add(hook);
            return true;
        }

        // onSovereignAIDecision
        
        private List<Action<Society, Person>> _delegate_onSovereignAIDecision = new List<Action<Society, Person>>();
        public List<Action<Society, Person>> Delegate_onSovereignAIDecision { get { return _delegate_onSovereignAIDecision; } }
        public bool RegisterHook_onSovereignAIDecision(Action<Society, Person> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onSovereignAIDecision.Contains(hook))
                return false;
            _delegate_onSovereignAIDecision.Add(hook);
            return true;
        }

        // onGetChallengeProgressPerTurn
        
        private List<Func<Challenge, UA, List<ReasonMsg>, double, double>> _delegate_onGetChallengeProgressPerTurn = new List<Func<Challenge, UA, List<ReasonMsg>, double, double>>();
        public List<Func<Challenge, UA, List<ReasonMsg>, double, double>> Delegate_onGetChallengeProgressPerTurn { get { return _delegate_onGetChallengeProgressPerTurn; } }
        public bool RegisterHook_onGetChallengeProgressPerTurn(Func<Challenge, UA, List<ReasonMsg>, double, double> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onGetChallengeProgressPerTurn.Contains(hook))
                return false;
            _delegate_onGetChallengeProgressPerTurn.Add(hook);
            return true;
        }

        // onEvent_IsLocationElderTomb
        
        private List<Func<Location, bool>> _delegate_onEvent_IsLocationElderTomb = new List<Func<Location, bool>>();
        public List<Func<Location, bool>> Delegate_onEvent_IsLocationElderTomb { get { return _delegate_onEvent_IsLocationElderTomb; } }
        public bool RegisterHook_onEvent_IsLocationElderTomb(Func<Location, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onEvent_IsLocationElderTomb.Contains(hook))
                return false;
            _delegate_onEvent_IsLocationElderTomb.Add(hook);
            return true;
        }

        // onAgentIsRecruitable
        
        private List<Func<UA, bool, bool>> _delegate_onAgentIsRecruitable = new List<Func<UA, bool, bool>>();
        public List<Func<UA, bool, bool>> Delegate_onAgentIsRecruitable { get { return _delegate_onAgentIsRecruitable; } }
        public bool RegisterHook_onAgentIsRecruitable(Func<UA, bool, bool> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onAgentIsRecruitable.Contains(hook))
                return false;
            _delegate_onAgentIsRecruitable.Add(hook);
            return true;
        }

        // onBrokenMakerPowerCreatesAgent_ProcessCurse
        
        private List<Func<Curse, Person, Location, string, string>> _delegate_onBrokenMakerPowerCreatesAgent_ProcessCurse = new List<Func<Curse, Person, Location, string, string>>();
        public List<Func<Curse, Person, Location, string, string>> Delegate_onBrokenMakerPowerCreatesAgent_ProcessCurse { get { return _delegate_onBrokenMakerPowerCreatesAgent_ProcessCurse; } }
        public bool RegisterHook_onBrokenMakerPowerCreatesAgent_ProcessCurse(Func<Curse, Person, Location, string, string> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onBrokenMakerPowerCreatesAgent_ProcessCurse.Contains(hook))
                return false;
            _delegate_onBrokenMakerPowerCreatesAgent_ProcessCurse.Add(hook);
            return true;
        }

        // onRevivePerson_CreateAgent
        
        private List<Func<Person, Location, UA>> _delegate_onRevivePerson_CreateAgent = new List<Func<Person, Location, UA>>();
        public List<Func<Person, Location, UA>> Delegate_onRevivePerson_CreateAgent { get { return _delegate_onRevivePerson_CreateAgent; } }
        public bool RegisterHook_onRevivePerson_CreateAgent(Func<Person, Location, UA> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onRevivePerson_CreateAgent.Contains(hook))
                return false;
            _delegate_onRevivePerson_CreateAgent.Add(hook);
            return true;
        }

        // onRevivePerson_EndOfProcess
        
        private List<Action<Person, Location>> _delegate_onRevivePerson_EndOfProcess = new List<Action<Person, Location>>();
        public List<Action<Person, Location>> Delegate_onRevivePerson_EndOfProcess { get { return _delegate_onRevivePerson_EndOfProcess; } }
        public bool RegisterHook_onRevivePerson_EndOfProcess(Action<Person, Location> hook)
        {
            if (hook == null)
                return false;
            if (_delegate_onRevivePerson_EndOfProcess.Contains(hook))
                return false;
            _delegate_onRevivePerson_EndOfProcess.Add(hook);
            return true;
        }
    }
}
