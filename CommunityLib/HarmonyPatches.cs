using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityLib
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        public static ArmyBattleData armyBattleData_StartOfCycle;

        public static string[] popupHolyOrder_DefaultPageText = new string[6];

        public static Text popupHolyOrder_PageText;

        private static bool populatedUM;

        private static bool razeIsValid;

        private static Tuple<Unit, Location, int, Location[]> lastPath;

        /// <summary>
        /// Initialises variables in this class that are required to perform patches, then executes harmony patches.
        /// </summary>
        /// <param name="core"></param>
        public static void PatchingInit()
        {
            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = false;
            string harmonyID = "ILikeGoodFood.SOFG.CommunityLib";
            Harmony harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                return;
            }

            // --- HOOKS --- //
            // Graphical unit updated hook
            harmony.Patch(original: AccessTools.Method(typeof(GraphicalUnit), nameof(GraphicalUnit.checkData), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(GraphicalUnit_checkData_Postfix)));

            // Graphical link updated hook
            harmony.Patch(original: AccessTools.Method(typeof(GraphicalLink), nameof(GraphicalLink.Update), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(GraphicalLink_Update_Postfix)), transpiler: new HarmonyMethod(patchType, nameof(GraphicalLink_Update_Transpiler)));

            // Unit death hooks
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.die), new Type[] { typeof(Map), typeof(string), typeof(Person) }), prefix: new HarmonyMethod(patchType, nameof(Unit_die_Prefix)), transpiler: new HarmonyMethod(patchType, nameof(Unit_die_Transpiler)));

            // Army Battle hooks
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.cycle), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_cycle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.unitMovesFromLocation), new Type[] { typeof(Unit), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_unitMovesFromLocation_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.computeAdvantage), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_computeAdvantage_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), "allocateDamage", new Type[] { typeof(List<UM>), typeof(int[]) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_allocateDamage_Transpiler)));

            // Agent Battle hooks
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), nameof(BattleAgents.setupBattle), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(BattleAgents_setupBattle_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), nameof(BattleAgents.step), new Type[] { typeof(PopupBattleAgent) }), transpiler: new HarmonyMethod(patchType, nameof(BattleAgents_step_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), nameof(BattleAgents.attackDownRow), new Type[] { typeof(int), typeof(UA), typeof(UA), typeof(PopupBattleAgent) }), transpiler: new HarmonyMethod(patchType, nameof(BattleAgents_AttackDownRow_Minion_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), nameof(BattleAgents.attackDownRow), new Type[] { typeof(int), typeof(int), typeof(AgentCombatInterface), typeof(UA), typeof(UA), typeof(PopupBattleAgent) }), transpiler: new HarmonyMethod(patchType, nameof(BattleAgents_AttackDownRow_Agent_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), nameof(BattleAgents.automatic), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(BattleAgents_automatic_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(World), nameof(World.bEndTurn), new Type[] { typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(World_bEndTurn_Transpiler)));

            // Agent Barttle Popup Hooks
            harmony.Patch(original: AccessTools.Method(typeof(PopupBattleAgent), nameof(PopupBattleAgent.populate), new Type[] { typeof(BattleAgents) }), postfix: new HarmonyMethod(patchType, nameof(PopupBattleAgent_populate_Postfix)));

            // Raze Location hooks
            harmony.Patch(original: AccessTools.Method(typeof(Task_RazeLocation), nameof(Task_RazeLocation.turnTick), new Type[] { typeof(Unit) }), prefix: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Postfix)), transpiler: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Transpiler)));

            // Settlement destruction hooks
            harmony.Patch(original: AccessTools.Method(typeof(Settlement), nameof(Settlement.fallIntoRuin), new Type[] { typeof(string), typeof(object) }), prefix: new HarmonyMethod(patchType, nameof(Settlement_FallIntoRuin_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Settlement_FallIntoRuin_Postfix)));

            // Religion UI Screen hooks
            harmony.Patch(original: AccessTools.Method(typeof(PrefabStore), nameof(PrefabStore.popHolyOrder), new Type[] { typeof(HolyOrder) }), prefix: new HarmonyMethod(patchType, nameof(PrefabStore_popHolyOrder_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.setTo), new Type[] { typeof(HolyOrder), typeof(int) }), prefix: new HarmonyMethod(patchType, nameof(PopupHolyOrder_setTo_Prefix)), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_setTo_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_HolyTenet), nameof(UIE_HolyTenet.bInfluenceNegatively), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UIE_HolyTenet_bInfluence_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_HolyTenet), nameof(UIE_HolyTenet.bInfluencePositively), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UIE_HolyTenet_bInfluence_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UILeftLocation), nameof(UILeftLocation.setTo), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(UILeftLocation_setTo_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UILeftLocation), nameof(UILeftLocation.bViewFaith), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UILeftLocation_bViewFaith_Transpiler)));

            // LevelUp Traits Hook
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getStartingTraits), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UA_getStartingTraits_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Trait), nameof(Trait.getAvailableTraits), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Trait_getAvailableTraits_Postfix)));

            // Gain Item Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Person), nameof(Person.gainItem), new Type[] { typeof(Item), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Person_gainItem_Transpiler)));

            // Action Taking Monster Hooks
            harmony.Patch(original: AccessTools.Method(typeof(SG_ActionTakingMonster), nameof(SG_ActionTakingMonster.turnTick), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(SG_ActionTakingMonster_turnTick_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Locs), nameof(UIScroll_Locs.checkData), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Locs_checkData_Transpiler)));

            // Sovereign Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Society), nameof(Society.processActions), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Society_processActions_Transpiler)));

            // onIsElderTomb Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.ai_testDark), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_ai_testDark_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.ai_testMagic), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_ai_testMagic_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.checkSpawnAgent), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_checkSpawnAgent_Transpiler)));

            // OnAgentIsRecruitable
            harmony.Patch(original: AccessTools.Method(typeof(PopupAgentCreation), nameof(PopupAgentCreation.populate), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupAgentCreation_populate_Transpiler)));

            // Broken Maker creates agents using powers
            harmony.Patch(original: AccessTools.Method(typeof(P_Eternity_CreateAgent), nameof(P_Eternity_CreateAgent.createAgent), new Type[] { typeof(Person), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(P_Eternity_CreateAgent_createAgent_transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Eternity_CreateAgentReusable), nameof(P_Eternity_CreateAgentReusable.createAgent), new Type[] { typeof(Person), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(P_Eternity_CreateAgentReusable_createAgent_transpiler)));

            // Get Distance To hooks
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.distanceDivisor), new Type[] { typeof(Challenge) }), transpiler: new HarmonyMethod(patchType, nameof(UA_distanceDivisor_Transpiler)));
            harmony.Patch(original: AccessTools.Constructor(typeof(Task_AttackArmy), new Type[] { typeof(UM), typeof(UM) }), postfix: new HarmonyMethod(patchType, nameof(Task_AttackArmy_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Constructor(typeof(Task_AttackUnit), new Type[] { typeof(Unit), typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(Task_AttackUnit_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Constructor(typeof(Task_AttackUnitWithEscort), new Type[] { typeof(Unit), typeof(Unit), typeof(UM_CavalryEscort) }), postfix: new HarmonyMethod(patchType, nameof(Task_AttackUnitWithEscort_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Constructor(typeof(Task_Bodyguard), new Type[] { typeof(Unit), typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(Task_Bodyguard_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Constructor(typeof(Task_DisruptUA), new Type[] { typeof(UA), typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Task_DisruptUA_ctor_Postfix)));

            // Prefab Store hooks
            harmony.Patch(original: AccessTools.Method(typeof(PrefabStore), nameof(PrefabStore.popHolyOrder), new Type[] { typeof(HolyOrder) }), prefix: new HarmonyMethod(patchType, nameof(Prefab_popHolyOrder_Prefix)));

            // --- SYSTEM MODIFICATIONS --- //
            // Mod Config Button Update
            harmony.Patch(original: AccessTools.Method(typeof(PopupModConfig), nameof(PopupModConfig.Update), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(PopupModConfig_update_postfix)));

            // Auto Relaunch
            harmony.Patch(original: AccessTools.Method(typeof(PopupModConfig), nameof(PopupModConfig.dismiss), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupModConfig_dismiss_transpiler)));

            // Agent Fixes
            harmony.Patch(original: AccessTools.Method(typeof(UAE_Abstraction), nameof(UAE_Abstraction.validTarget), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(UAE_Abstraction_validTarget_transpiler)));

            // AgentBattle Fixes
            harmony.Patch(original: AccessTools.Method(typeof(BattleAgents), "retreatOrFlee", new Type[] { typeof(UA), typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(BattleAgents_retreatOrFlee_Transpiler)));

            // Assign Killer to Miscellaneous causes of death
            harmony.Patch(original: AccessTools.Method(typeof(UM_HumanArmy), nameof(UM_HumanArmy.turnTickInner)), transpiler: new HarmonyMethod(patchType, nameof(UM_HumanArmy_turnTickInner_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.skirmishDanger), new Type[] { typeof(UA), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishAttacking_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.skirmishDanger), new Type[] { typeof(UA), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishDefending_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Mg_Volcano), nameof(Mg_Volcano.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Mg_Volcano_Complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(God_Snake), nameof(God_Snake.awaken), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(God_Snake_Awaken_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Person), nameof(Person.die), new Type[] { typeof(string), typeof(bool), typeof(object), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Person_die_Transpiler)));

            // Challenge fixes //
            // Infiltrate
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Infiltrate), nameof (Ch_Infiltrate.getComplexity), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Ch_Infiltrate_getComplexity_Transpiler)));
            // Buy Item
            harmony.Patch(original: AccessTools.Method(typeof(Ch_BuyItem), nameof(Ch_BuyItem.complete), new Type[] { typeof(UA) }), prefix: new HarmonyMethod(patchType, nameof(Ch_BuyItem_complete_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Ch_BuyItem_complete_Postfix)));
            // Death of The Dun
            harmony.Patch(original: AccessTools.Method(typeof(Mg_DeathOfTheSun), nameof(Mg_DeathOfTheSun.turnTick), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Mg_DeathOfTheSun_turnTick_Transpiler)));
            // Opportunistict Encroachment
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.valid), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_valid_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_OpportunisticEncroachment), nameof(Ch_Orcs_OpportunisticEncroachment.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_complete_Transpiler)));
            // Orcs Attack Here
            harmony.Patch(original: AccessTools.Method(typeof(Rti_Orc_AttackHere), nameof(Rti_Orc_AttackHere.valid), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Rti_Orc_AttackHere_Postfix)));
            // Orcs Build Menagerie
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.getRestriction), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_getRestriction_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMenagerie), nameof(Ch_Orcs_BuildMenagerie.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMenagerie_validFor_Postfix)));
            // Orcs Build Shipyard
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildShipyard), nameof(Ch_Orcs_BuildShipyard.valid), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildShipyard_valid_Transpiler)));
            // Orcs Build Mine
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMines), nameof(Ch_Orcs_BuildMines.getRestriction), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMines_getRestriction_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Orcs_BuildMines), nameof(Ch_Orcs_BuildMines.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Orcs_BuildMines_validFor_Postfix)));
            // Orcs Raiding Party
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_RaidingParty), nameof(Rt_Orcs_RaidingParty.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_RaidingParty_complete_Transpiler)));

            // Item Fixes //
            // I_DarkStone
            harmony.Patch(original: AccessTools.Method(typeof(I_DarkStone), nameof(I_DarkStone.getShortDesc), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(I_DarkStone_getShortDesc_Postfix)));

            // Local Action Fixes
            harmony.Patch(original: AccessTools.Method(typeof(Act_FundOutpost), nameof(Act_FundOutpost.valid), new Type[] { typeof(Person), typeof(SettlementHuman) }), postfix: new HarmonyMethod(patchType, nameof(Act_FundOutpost_valid_Postfix)));

            // Realtionship Interaction Fixes
            harmony.Patch(original: AccessTools.Method(typeof(Society), nameof(Society.populateActions), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Society_populateActions_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(SG_Orc), nameof(SG_Orc.getActions), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(SG_Orc_getActions_Transpiler)));

            // Religion UI Screen modification
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bPrev), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bNext), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));

            // Repeat Completed Challenge tweaks
            harmony.Patch(original: AccessTools.Method(typeof(PopupChallengeComplete), nameof(PopupChallengeComplete.populate), new Type[] { typeof(Unit), typeof(Challenge), typeof(int), typeof(string), typeof(string) }), transpiler: new HarmonyMethod(patchType, nameof(PopupChallengeComplete_populate_Transpiler)));

            // Trade Route Fixes
            harmony.Patch(original: AccessTools.Method(typeof(ManagerTrade), nameof(ManagerTrade.checkTradeNetwork), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(ManagerTrade_checkTradeNetwork_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(PrefabStore), nameof(PrefabStore.popUnderground), new Type[] { typeof(bool), typeof(bool) }), postfix: new HarmonyMethod(patchType, nameof(PrefabStore_popUnderground_Postfix)));

            // Pan to Holy Order Screen
            harmony.Patch(original: AccessTools.Method(typeof(PopupMsgUnified), nameof(PopupMsgUnified.dismissAgentA), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(PopupMsgUnified_dismissAgentA_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupMsgUnified), nameof(PopupMsgUnified.dismissAgentB), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(PopupMsgUnified_dismissAgentB_Postfix)));

            // Holy Order Fixes
            harmony.Patch(original: AccessTools.Method(typeof(HolyOrder), nameof(HolyOrder.turnTick), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(HolyOrder_turnTick_Transpiler)));

            // Overmind modifications
            harmony.Patch(original: AccessTools.Method(typeof(Overmind), nameof(Overmind.getThreats), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_getThreats_Transpiler)));

            // Pathfinding modifications
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.adjacentMoveTo), new Type[] { typeof(Unit), typeof(Location) }), prefix: new HarmonyMethod(patchType, nameof(Map_adjacentMoveTo_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Map_adjacentMoveTo_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.moveTowards), new Type[] { typeof(Unit), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(Map_moveTowards_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.getPathTo), new Type[] { typeof(Location), typeof(Location), typeof(Unit), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Map_getPathTo_Location_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.getPathTo), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Unit), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Map_getPathTo_SocialGroup_Transpiler)));

            // Power Fixes
            harmony.Patch(original: AccessTools.Method(typeof(P_Opha_Smite), nameof(P_Opha_Smite.cast), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(P_Opha_Smite_cast_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_BringTheFood), nameof(P_Under_BringTheFood.validTarget), new Type[] { typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(P_Under_BringTheFood_validTarget_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_DefendTheHeart), nameof(P_Under_DefendTheHeart.validTarget), new Type[] { typeof(Unit) }), postfix: new HarmonyMethod(patchType, nameof(P_Under_DefendTheHeart_validTarget_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_DragBodiesUnder), nameof(P_Under_DragBodiesUnder.cast), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(P_Under_DragBodiesUnder_cast_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_Sinkholes), nameof(P_Under_Sinkholes.validTarget), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(P_Under_Sinkholes_validTarget_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_Sinkholes), nameof(P_Under_Sinkholes.lash), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(P_Under_Sinkholes_lash_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_VileSecretions), nameof(P_Under_VileSecretions.validTarget), new Type[] { typeof(Location) }), postfix: new HarmonyMethod(patchType, nameof(P_Under_VileSecretions_validTarget_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(P_Under_WithdrawAgent), nameof(P_Under_WithdrawAgent.validTarget), new Type[] { typeof(Unit) }), transpiler: new HarmonyMethod(patchType, nameof(P_Under_WithdrawAgent_validTraget_Transpiler)));

            // House Search Fix
            harmony.Patch(original: AccessTools.Method(typeof(UIScrollThreats), nameof(UIScrollThreats.checkData), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIScrollThreats_checkData_Transpiler)));

            // Dismiss Key Fix
            harmony.Patch(original: AccessTools.Method(typeof(PopupMsg), nameof(PopupMsg.dismissKeyHit), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(PopupMsg_dismissKeyHit_Prefix)));

            // Victory Point Fixes
            harmony.Patch(original: AccessTools.Method(typeof(Overmind), nameof(Overmind.computeVictoryProgress), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overming_computeVictoryProgress_Transpiler)));

            // Orc Expansion modifications
            harmony.Patch(original: AccessTools.Method(typeof(SG_Orc), nameof(SG_Orc.canSettle), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(SG_Orc_canSettle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.validFor), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_Transpiler)));

            // Culture modifications
            harmony.Patch(original: AccessTools.Method(typeof(Set_MinorHuman), nameof(Set_MinorHuman.getSprite), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Set_MinorHuman_getSprite_Transpiler)));

            // AGENT UI //
            // UIScroll_Unit (Challenge utility panel)
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.checkData), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UIScroll_Unit_checkData_Prefix)), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_checkData_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.Update), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_Update_Transpiler)));

            // RECRUITABILITY //
            // UAEN overrides
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UA_isCommandable_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_CaveSpider), nameof(UAEN_CaveSpider.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_DeepOne), nameof(UAEN_DeepOne.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Ghast), nameof(UAEN_Ghast.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_OrcUpstart), nameof(UAEN_OrcUpstart.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Vampire), nameof(UAEN_Vampire.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));

            // UAEN OVERRIDE AI //
            // Negate unit interactions.
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>), typeof(bool) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)), postfix: new HarmonyMethod(patchType, nameof(UAEN_getAttackUtility_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getBodyguardUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getDisruptUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getVisibleUnits), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Prefix)), postfix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Postfix)));

            // Override AI
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_CaveSpider), nameof(UAEN_CaveSpider.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_CaveSpider_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_DeepOne), nameof(UAEN_DeepOne.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_DeepOne_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Constructor(typeof(UAEN_DeepOne), new Type[] { typeof(Location), typeof(Society), typeof(Person) }), postfix: new HarmonyMethod(patchType, nameof(UAEN_DeepOne_ctor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Ghast), nameof(UAEN_Ghast.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_Ghast_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_OrcUpstart), nameof(UAEN_OrcUpstart.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Vampire), nameof(UAEN_Vampire.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_Vampire_turnTickAI_Prefix)));

            // Deep ones
            harmony.Patch(original: AccessTools.Method(typeof(Rt_DescendIntoTheSea), nameof(Rt_DescendIntoTheSea.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_DescendIntoTheSea_validFor_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_MaintainHumanity), nameof(Rt_MaintainHumanity.validFor), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Rt_MaintainHumanity_validFor_Postfix)));

            // Ch_Rest_InOrcCamp
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Rest_InOrcCamp), nameof(Ch_Rest_InOrcCamp.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Rest_InOrcCamp_complete_Postfix)));

            // UIE_AgentRoster
            harmony.Patch(original: AccessTools.Method(typeof(UIE_AgentRoster), nameof(UIE_AgentRoster.setTo), new Type[] { typeof(World), typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(UIE_AgentRoster_setToUA_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_AgentRoster), nameof(UIE_AgentRoster.setTo), new Type[] { typeof(World), typeof(UM) }), transpiler: new HarmonyMethod(patchType, nameof(UIE_AgentRoster_setToUM_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_AgentRoster), nameof(UIE_AgentRoster.doAgentBattle), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIE_AgentRoster_doAgentBattle_Transpiler)));

            // Check Engaging patches
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playedOpensMinions", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToAttack", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToDisrupt", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToFollow", new Type[] { typeof(Unit) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToRob", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToStartChallenge", new Type[] { typeof(Challenge) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), "playerTriesToTrade", new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(checkEngaging_BulkTranspiler)));

            harmony.Patch(original: AccessTools.Method(typeof(UIInputs), nameof(UIInputs.rightClickOnHex), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIInput_rightClickOnHex_Transpiler)));

            // --- MOD OPTIONS --- //
            // God-Sort //
            // Patches for PrefabStore
            harmony.Patch(original: AccessTools.Method(typeof(PrefabStore), nameof(PrefabStore.getScrollSetGods), new Type[] { typeof(List<God>) }), prefix: new HarmonyMethod(patchType, nameof(PrefabStore_getScrollSetGods_Prefix)));

            // Orc Horde Count //
            // Patches for ManagerMajorThreats
            harmony.Patch(original: AccessTools.Method(typeof(ManagerMajorThreats), nameof(ManagerMajorThreats.turnTick), Type.EmptyTypes), transpiler: new HarmonyMethod(patchType, nameof(ManagerMajorThreats_turnTick_Transpiler)));

            // Natural Wonder Count //
            // Patches for Map
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.placeWonders), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Map_placeWonders_Transpiler)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        // Graphical unit updated hook
        private static void GraphicalUnit_checkData_Postfix(GraphicalUnit __instance)
        {
            //Console.WriteLine("CommunityLib: Calling onGraphicalUnitUpdated hook");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                //Console.WriteLine("CommunityLib: Calling hook from " + hook.GetType().Namespace);
                hook.onGraphicalUnitUpdated(__instance);
            }
        }

        private static IEnumerable<CodeInstruction> GraphicalLink_Update_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(GraphicalLink_Update_TranspilerBody), new Type[] { typeof(GraphicalLink), typeof(float) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i+1].opcode == OpCodes.Ceq)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse)
                        {
                            Label retLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brfalse, retLabel);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Br, retLabel);

                            i++;
                            targetIndex = 0;
                        }
                    }
                    
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed GraphicalLink_Update_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void GraphicalLink_Update_TranspilerBody(GraphicalLink gLink, float alpha)
        {
            float alphaA = alpha;
            float alphaB = alpha;

            int z = GraphicalMap.z;
            if (gLink.link.a.hex.z != z)
            {
                alphaA = 0f;
            }
            gLink.lineRenderer.startColor = new Color(0.1f, 0.1f, 1f, alphaA);

            if (gLink.link.b.hex.z != z)
            {
                alphaB = 0f;
            }
            gLink.lineRenderer.endColor = new Color(0.1f, 0.1f, 1f, alphaB);
        }

        private static void GraphicalLink_Update_Postfix(GraphicalLink __instance)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onGraphicalLinkUpdated(__instance);
            }
        }

        // Mod Config button update
        private static void PopupModConfig_update_postfix(PopupModConfig __instance)
        {
            if (__instance.dirty)
            {
                if (ModCore.opt_autoRelaunch)
                {
                    __instance.bDismiss.GetComponentInChildren<Text>().text = "Accept (Restart)";
                }
                else
                {
                    __instance.bDismiss.GetComponentInChildren<Text>().text = "Accept";
                }

            }
            else
            {
                __instance.bDismiss.GetComponentInChildren<Text>().text = "Back";
            }
        }

        // Auto-Relaunch 
        private static IEnumerable<CodeInstruction> PopupModConfig_dismiss_transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(PopupModConfig_dismiss_transpilerBody), new Type[] { typeof(PopupModConfig) });

            Label skip = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Leave_S && instructionList[i + 1].opcode == OpCodes.Ldfld)
                        {
                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, skip);
                            yield return new CodeInstruction(OpCodes.Ret);
                            code = new CodeInstruction(OpCodes.Nop);
                            code.labels.Add(skip);
                            yield return code;

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed PopupModConfig_dismiss_transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool PopupModConfig_dismiss_transpilerBody(PopupModConfig __instance)
        {
            PopupModConfig.loadModConfigFromFile(__instance.modsSeen, true);

            if (!ModCore.opt_autoRelaunch)
            {
                return false;
            }

            string exePath = Path.Combine(Path.GetDirectoryName(__instance.GetType().Assembly.Location), "..", "..", "ShadowsOfForbiddenGods.exe");
            string batchFile = Path.Combine(Path.GetDirectoryName(patchType.Assembly.Location), "..", "relauncher.bat");
            try
            {
                Process.Start(batchFile, $"\"{exePath}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine("CommunityLib: Error starting relauncher process: " + ex.Message);
            }

            __instance.ui.removeBlocker(__instance.gameObject);
            if (SteamManager.s_EverInitialized)
            {
                SteamManager.shutdownSteamAPI();
            }
            Application.Quit();

            return true;
        }

        // AGent Fixes
        private static IEnumerable<CodeInstruction> UAE_Abstraction_validTarget_transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_MonarchCode = AccessTools.Field(typeof(UAE_Abstraction), nameof(UAE_Abstraction.CODE_MONARCH));
            FieldInfo FI_Settlemenet = AccessTools.Field(typeof(Location), nameof(Location.settlement));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldsfld && (FieldInfo)instructionList[i].operand == FI_MonarchCode)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Brtrue_S)
                        {
                            Label trueLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, trueLabel);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Settlemenet);
                            yield return new CodeInstruction(OpCodes.Isinst, typeof(Set_DwarvenCity));

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UAE_Abstraction_validTarget_transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Assign Killer to Miscellaneous causes of death
        private static IEnumerable<CodeInstruction> UM_HumanArmy_turnTickInner_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_GetPerson = AccessTools.PropertyGetter(typeof(Unit), nameof(Unit.person));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Callvirt)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_GetPerson);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UM_HumanArmy_turnTickInner_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Ch_SkirmishAttacking_skirmishDanger_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Battle = AccessTools.Field(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.battle));

            int targetIndex = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Callvirt)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Battle);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Ch_SkirmishAttacking_skirmishDanger_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Ch_SkirmishDefending_skirmishDanger_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Battle = AccessTools.Field(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.battle));

            int targetIndex = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Callvirt)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Battle);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Ch_SkirmishDefending_skirmishDanger_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }


        private static IEnumerable<CodeInstruction> Mg_Volcano_Complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Mg_Volcano_Complete_TranspilerBody));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex < 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Ldstr && instructionList[i + 1].opcode == OpCodes.Callvirt)
                        {
                            if (targetIndex == 1)
                            {
                                targetIndex++;

                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody);

                                i++;
                            }
                            else if (targetIndex == 2)
                            {
                                targetIndex = 0;

                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody);

                                i++;
                            }
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Mg_Volcano_Complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static Person Mg_Volcano_Complete_TranspilerBody(UA uA)
        {
            return uA.person;
        }

        private static IEnumerable<CodeInstruction> God_Snake_Awaken_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex < 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Ldstr && instructionList[i + 1].opcode == OpCodes.Callvirt)
                        {
                            targetIndex++;

                            if (targetIndex == 3)
                            {
                                targetIndex = 0;

                                yield return new CodeInstruction(OpCodes.Ldarg_0);

                                i++;
                            }
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed God_Snake_Awaken_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Person_die_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Person_die_TranspilerBody), new Type[] { typeof(object) });

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Ldarg_0 && instructionList[i + 1].opcode == OpCodes.Ldstr)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_3);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Person_die_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static object Person_die_TranspilerBody(object killer)
        {
            if (killer is Person || killer is Unit)
            {
                return killer;
            }

            return null;
        }

        // CHallenge Fixes
        private static IEnumerable<CodeInstruction> Ch_Infiltrate_getComplexity_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Sub = AccessTools.Field(typeof(Ch_Infiltrate), nameof(Ch_Infiltrate.sub));
            FieldInfo FI_Settlment = AccessTools.Field(typeof(Subsettlement), nameof(Subsettlement.settlement));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Call)
                        {
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Sub);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Settlment);

                            i += 2;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Ch_Infiltrate_getComplexity_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Ch_BuyItem_complete_Prefix(Ch_BuyItem __instance, out string __state)
        {
            __state = __instance.onSale.getName();
        }

        private static void Ch_BuyItem_complete_Postfix(Ch_BuyItem __instance, string __state)
        {
            __instance.msgString = "As soon as the " + __state + " had been sold, the merchants began offering a";
            if ("aeiouAEIOU".IndexOf(__instance.onSale.getName().First()) >= 0)
            {
                __instance.msgString += "n " + __instance.onSale.getName();
            }
            else
            {
                __instance.msgString += " " + __instance.onSale.getName();
            }
        }

        private static IEnumerable<CodeInstruction> Mg_DeathOfTheSun_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_progress = AccessTools.Method(typeof(Challenge), nameof(Challenge.getProgressPerTurn), new Type[] { typeof(UA), typeof(List<ReasonMsg>) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_1 && instructionList[i+1].opcode == OpCodes.Callvirt)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_progress);

                            i += 2;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Mg_DeathOfTheSun_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_OpportunisticEncroachment_valid_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_LayerCheck = AccessTools.Method(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_LayerComparison), new Type[] { typeof(Location), typeof(Location) });
            MethodInfo MI_GetLocation = AccessTools.PropertyGetter(typeof(Challenge), nameof(Challenge.location));

            FieldInfo FI_Settlement = AccessTools.Field(typeof(Location), nameof(Location.settlement));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            Label falseLabel = (Label)instructionList[i + 3].operand;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_GetLocation);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                            yield return new CodeInstruction(OpCodes.Call, MI_LayerCheck);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Isinst)
                        {
                            Label falseLabel = (Label)instructionList[i - 4].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, falseLabel);

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Settlement);
                            yield return new CodeInstruction(OpCodes.Isinst, typeof(Set_DwarvenCity));

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Ch_Orcs_OpportunisticEncroachment_valid_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_OpportunisticEncroachment_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_LayerCheck = AccessTools.Method(patchType, nameof(Ch_Orcs_OpportunisticEncroachment_LayerComparison), new Type[] { typeof(Location), typeof(Location) });
            MethodInfo MI_GetLocation = AccessTools.PropertyGetter(typeof(Challenge), nameof(Challenge.location));

            FieldInfo FI_Settlement = AccessTools.Field(typeof(Location), nameof(Location.settlement));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            Label falseLabel = (Label)instructionList[i + 3].operand;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_GetLocation);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_LayerCheck);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Isinst)
                        {
                            Label falseLabel = (Label)instructionList[i - 4].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, falseLabel);

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Settlement);
                            yield return new CodeInstruction(OpCodes.Isinst, typeof(Set_DwarvenCity));

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Ch_Orcs_OpportunisticEncroachment_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Ch_Orcs_OpportunisticEncroachment_LayerComparison(Location loc, Location neighbour)
        {
            if (loc.soc is SG_Orc orcs && !orcs.canGoUnderground())
            {
                if (loc.hex.z == 0 && neighbour.hex.z == 1)
                {
                    return false;
                }

                if (loc.hex.z == 1 && neighbour.hex.z == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static void Rti_Orc_AttackHere_Postfix(Rti_Orc_AttackHere __instance, ref bool __result)
        {
            if (__instance.caster == null || __instance.caster.orcs == null || __instance.caster.orcs.isGone())
            {
                __result = false;
            }
        }

        private static void Ch_Orcs_BuildMenagerie_getRestriction_Postfix(Ch_Orcs_BuildMenagerie __instance, ref string __result)
        {
            int specialisedNeighbourCount = 2;
            foreach (Location neighbour in __instance.location.getNeighbours())
            {
                if (neighbour.settlement is Set_OrcCamp camp && camp.specialism != -1)
                {
                    specialisedNeighbourCount++;
                }
            }

            __result = "Requires an infiltrated orc camp (non specialised). Requires " + (__instance.map.param.ch_orc_buildFortressCostPerNeighbour * specialisedNeighbourCount).ToString() + " <b>gold</b>. Requires you to be carrying a Manticore Trophy (obtained by killing a manticore)";
        }

        private static void Ch_Orcs_BuildMenagerie_validFor_Postfix(Ch_Orcs_BuildMenagerie __instance,  ref bool __result, UA ua)
        {
            if (__result)
            {
                int specialisedNeighbourCount = 2;
                foreach (Location neighbour in __instance.location.getNeighbours())
                {
                    if (neighbour.settlement is Set_OrcCamp camp && camp.specialism != -1)
                    {
                        specialisedNeighbourCount++;
                    }
                }

                int cost = __instance.map.param.ch_orc_buildFortressCostPerNeighbour * specialisedNeighbourCount;
                if (ua.person == null || ua.person.gold < cost)
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<CodeInstruction> Ch_Orcs_BuildShipyard_valid_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FIS_SelectedUnit = AccessTools.Field(typeof(GraphicalMap), nameof(GraphicalMap.selectedUnit));

            Label returnLabel = ilg.DefineLabel();

            int targetIndex = 1;
            bool returnCode = true;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex < 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            instructionList[i].operand = returnLabel;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0)
                        {
                            returnCode = false;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_1)
                        {
                            returnCode = true;

                            instructionList[i].labels.Add(returnLabel);

                            targetIndex = 0;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed Ch_Orcs_BuildShipyard_valid_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Ch_Orcs_BuildMines_getRestriction_Postfix(Ch_Orcs_BuildMines __instance,  ref string __result)
        {
            int specialisedNeighbourCount = 2;
            foreach (Location neighbour in __instance.location.getNeighbours())
            {
                if (neighbour.settlement is Set_OrcCamp camp && camp.specialism != -1)
                {
                    specialisedNeighbourCount++;
                }
            }

            __result = "Requires an infiltrated orc camp (non specialised). Requires " + (__instance.map.param.ch_orc_buildFortressCostPerNeighbour * specialisedNeighbourCount).ToString() + " <b>gold</b>.";
        }

        private static void Ch_Orcs_BuildMines_validFor_Postfix(Ch_Orcs_BuildMines __instance, ref bool __result, UA ua)
        {
            if (__result)
            {
                int specialisedNeighbourCount = 2;
                foreach (Location neighbour in __instance.location.getNeighbours())
                {
                    if (neighbour.settlement is Set_OrcCamp camp && camp.specialism != -1)
                    {
                        specialisedNeighbourCount++;
                    }
                }

                int cost = __instance.map.param.ch_orc_buildFortressCostPerNeighbour * specialisedNeighbourCount;
                if (ua.person == null || ua.person.gold < cost)
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<CodeInstruction> Rt_Orcs_RaidingParty_complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FIS_SelectedUnit = AccessTools.Field(typeof(GraphicalMap), nameof(GraphicalMap.selectedUnit));

            Label skipLabel = ilg.DefineLabel();

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_1 && instructionList[i+1].opcode == OpCodes.Stsfld)
                        {
                            yield return new CodeInstruction(OpCodes.Ldsfld, FIS_SelectedUnit);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel);

                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Stsfld, FIS_SelectedUnit);

                            i += 2;
                            instructionList[i].labels.Add(skipLabel);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Rt_Orcs_RaidingParty_complete_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Item Fixes
        private static void I_DarkStone_getShortDesc_Postfix(ref string __result)
        {
            __result = "The sun shines less brightly on whoever carries this stone. Increases the shadow in any human, dwarven or elven settlement this person is in by 1% every turn (works on heroes, rulers and agents)";
        }

        // Local Action Fixes
        private static void Act_FundOutpost_valid_Postfix(Act_FundOutpost __instance, ref bool __result, Person ruler, SettlementHuman settlementHuman)
        {
            if (__result && __instance.map.awarenessOfUnderground < 1d)
            {
                int targetZ = __instance.outpost.location.hex.z;
                int sourceZ = -1;
                SocialGroup sg = null;

                if (settlementHuman != null)
                {
                    sourceZ = settlementHuman.location.hex.z;
                    sg = settlementHuman.location.soc;
                }

                if (sg != null && sg == sg.map.soc_dark)
                {
                    return;
                }

                if (sg is Society soc && (soc.isDarkEmpire || soc.isOphanimControlled))
                {
                    return;
                }

                if ((targetZ == 0 && sourceZ == 1) || (targetZ == 1 && sourceZ == 0))
                {
                    if (sg != null)
                    {
                        foreach (Location neighbour in __instance.outpost.location.getNeighbours())
                        {
                            if (neighbour.hex.z == targetZ && neighbour.soc == sg)
                            {
                                __result = true;
                                return;
                            }
                        }
                    }

                    __result = false;
                }
            }
        }

        // Unit death hooks
        private static bool Unit_die_Prefix(Map map, string v, Person killer, Unit __instance)
        {
            bool result = true;

            //Console.WriteLine("CommunityLib: Intercept Unit Death");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptUnitDeath(__instance, v, killer);

                if (retValue)
                {
                    result = false;
                    //Console.WriteLine("CommunityLib: " + hook.GetType().Namespace + " has intercepted death of " + __instance.getName());
                    break;
                }
            }

            if (!result)
            {
                if (__instance.maxHp < 1)
                {
                    __instance.maxHp = 1;
                }

                if (__instance.hp < 1)
                {
                    __instance.hp = 1;
                }

                if (__instance.isDead)
                {
                    __instance.isDead = false;
                }
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> Unit_die_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Unit_die_TranspilerBody_InterceptAndStartOfUnitDeath), new Type[] { typeof(Unit), typeof(string), typeof(Person) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i == 1)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldarg_3);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Unit_die_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Unit_die_TranspilerBody_InterceptAndStartOfUnitDeath(Unit u, string v, Person killer = null)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onUnitDeath_StartOfProcess(u, v, killer);
            }
        }

        // Army Battle hooks
        private static IEnumerable<CodeInstruction> BattleArmy_cycle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            // Transpiler Bodies for Data management.
            MethodInfo MI_TranspilerBody_GatherInterceptAndStartOf = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_GatherInterceptAndStartOf), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_EndOfProcess = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_EndOfProcess), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_Victory = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_onArmyBattleVictory), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_DamageCalc = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_DamageCalculated), new Type[] { typeof(BattleArmy), typeof(int), typeof(int), typeof(int), typeof(bool) });

            FieldInfo FI_BattleArmy_Done = AccessTools.Field(typeof(BattleArmy), nameof(BattleArmy.done));

            Label retLabel = instructionList[instructionList.Count - 1].labels[0];

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i + 1].opcode == OpCodes.Ldfld && instructionList[i + 2].opcode == OpCodes.Callvirt && instructionList[i + 3].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex++;

                            // Gather intercept and start function
                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_GatherInterceptAndStartOf);
                            yield return new CodeInstruction(OpCodes.Brtrue, retLabel);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Br)
                        {
                            targetIndex++;

                            //Implements onArmyBattleVictory hook in first of two locations
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Victory);
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Stloc_S && instructionList[i - 2].opcode == OpCodes.Conv_I4)
                        {
                            targetIndex++;

                            // Implements onArmyBattleCycyle_DamageCalculated hook in first of two locations.
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc, 48);
                            yield return new CodeInstruction(OpCodes.Ldloc, 45);
                            yield return new CodeInstruction(OpCodes.Ldloc, 46);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DamageCalc);
                            yield return new CodeInstruction(OpCodes.Stloc, 48);
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Stloc_S && instructionList[i - 2].opcode == OpCodes.Conv_I4)
                        {
                            targetIndex++;

                            // Implements onArmyBattleCycyle_DamageCalculated hook in second of two locations.
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc, 56);
                            yield return new CodeInstruction(OpCodes.Ldloc, 53);
                            yield return new CodeInstruction(OpCodes.Ldloc, 54);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DamageCalc);
                            yield return new CodeInstruction(OpCodes.Stloc, 56);
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (i == instructionList.Count - 2 && instructionList[i].opcode == OpCodes.Br_S)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Victory);
                        }
                    }
                    else if (targetIndex == 6)
                    {
                        if (i == instructionList.Count - 1 && instructionList[i].opcode == OpCodes.Ret)
                        {
                            targetIndex = 0;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_EndOfProcess);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleArmy_cycle_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool BattleArmy_cycle_TranspilerBody_GatherInterceptAndStartOf(BattleArmy battle)
        {
            // Gather battle data
            ArmyBattleData data = new ArmyBattleData();
            data.attackers = new List<UM>();
            data.attackers.AddRange(battle.attackers);
            data.defenders = new List<UM>();
            data.defenders.AddRange(battle.defenders);
            data.attComs = new List<UA>();
            data.attComs.AddRange(battle.attComs);
            data.defComs = new List<UA>();
            data.defComs.AddRange(battle.defComs);

            armyBattleData_StartOfCycle = data;

            // Intercept Hook
            bool result = false;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptArmyBattleCycle(battle);

                if (retValue)
                {
                    result = true;
                }
            }

            if (result && !battle.done)
            {
                battle.done = true;
            }

            if (result)
            {
                // If intercepted
                return result;
            }

            // Start of Process hook if not intercepted
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_StartOfProcess(battle);
            }

            return result;
        }

        private static void BattleArmy_cycle_TranspilerBody_EndOfProcess(BattleArmy battle)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_EndOfProcess(battle);
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_onArmyBattleVictory(BattleArmy battle)
        {
            ArmyBattleData data = armyBattleData_StartOfCycle;
            List<UM> victorUnits = new List<UM>();
            List<UA> victorComs = new List<UA>();
            List<UM> defeatedUnits = new List<UM>();
            List<UA> defeatedComs = new List<UA>();

            if (battle.attackers.Count == 0 && battle.defenders.Count == 0)
            {
                defeatedUnits.AddRange(data.attackers);
                defeatedUnits.AddRange(data.defenders);

                defeatedComs.AddRange(data.attComs);
                defeatedComs.AddRange(data.defComs);
            }
            else if (battle.attackers.Count == 0)
            {
                victorUnits = battle.defenders;
                victorComs = battle.defComs;

                defeatedUnits = data.attackers;
                defeatedComs = data.attComs;
            }
            else if (battle.defenders.Count == 0)
            {
                victorUnits = battle.attackers;
                victorComs = battle.attComs;

                defeatedUnits = data.defenders;
                defeatedComs = data.defComs;
            }

            if (defeatedUnits.Count > 0)
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook?.onArmyBattleVictory(battle, victorUnits, victorComs, defeatedUnits, defeatedComs);
                }
            }
        }

        private static int BattleArmy_cycle_TranspilerBody_DamageCalculated(BattleArmy battle, int dmg, int unitIndex, int targetIndex, bool isAttackers)
        {
            UM unit;
            UM target;

            if (isAttackers)
            {
                unit = battle.attackers[unitIndex];
                target = battle.defenders[targetIndex];
            }
            else
            {
                unit = battle.defenders[unitIndex];
                target = battle.attackers[targetIndex];
            }

            if (unit != null && target != null)
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    dmg = hook?.onArmyBattleCycle_DamageCalculated(battle, dmg, unit, target) ?? dmg;
                }
            }

            return dmg;
        }

        private static IEnumerable<CodeInstruction> BattleArmy_unitMovesFromLocation_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_onArmyBattleRetreatOrFlee = AccessTools.Method(patchType, nameof(BattleArmy_unitMovesFromLocation_TranspilerBody_OnAmryBattleRetreatOrFlee), new Type[] { typeof(BattleArmy), typeof(Unit) });
            MethodInfo MI_TransplilerBody_onArmyBattleTerminated = AccessTools.Method(patchType, nameof(BattleArmy_unitMovesFromLocation_TranspilerBody_onArmyBattleTerminated), new Type[] { typeof(BattleArmy), typeof(UM) });
            MethodInfo MI_BattleArmy_endBattle = AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.endBattle), new Type[] { });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i + 3].opcode == OpCodes.Callvirt && instructionList[i + 4].opcode == OpCodes.Pop)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_onArmyBattleRetreatOrFlee);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Call && instructionList[i].operand as MethodInfo == MI_BattleArmy_endBattle)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TransplilerBody_onArmyBattleTerminated);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleArmy_unitMovesFromLocation_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void BattleArmy_unitMovesFromLocation_TranspilerBody_OnAmryBattleRetreatOrFlee(BattleArmy battle, Unit u)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleRetreatOrFlee(battle, u);
            }
        }

        private static void BattleArmy_unitMovesFromLocation_TranspilerBody_onArmyBattleTerminated(BattleArmy battle, UM u)
        {
            List<UM> victorUnits = new List<UM>();
            List<UA> victorComs = new List<UA>();

            if (battle.attackers.Count == 0 && battle.defenders.Count > 0)
            {
                victorUnits.AddRange(battle.defenders);
                victorComs.AddRange(battle.defComs);
            }
            else if (battle.defenders.Count == 0 && battle.attackers.Count > 0)
            {
                victorUnits.AddRange(battle.attackers);
                victorComs.AddRange(battle.attComs);
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleTerminated(battle, victorUnits, victorComs, u);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_computeAdvantage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(BattleArmy_computeAdvantage_TranspilerBody), new Type[] { typeof(BattleArmy), typeof(double) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_0 && instructionList[i + 1].opcode == OpCodes.Ldc_R8)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleArmy_computeAdvantage_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void BattleArmy_computeAdvantage_TranspilerBody(BattleArmy battle, double advantage)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_ComputeAdvantage(battle, advantage);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_allocateDamage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_allocateDamage = AccessTools.Method(patchType, nameof(BattleArmy_allocateDamage_TranspilerBody_allocateDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]) });
            MethodInfo MI_TranspilerBody_receivesDamage = AccessTools.Method(patchType, nameof(BattleArmy_allocateDamage_TranspilerBody_receivesDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]), typeof(int) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i == 1 && instructionList[i].opcode == OpCodes.Ldc_I4_0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_allocateDamage);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_1 && instructionList[i - 1].opcode == OpCodes.Nop)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_receivesDamage);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleArmy_allocateDamage_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void BattleArmy_allocateDamage_TranspilerBody_allocateDamage(BattleArmy battle, List<UM> units, int[] dmgs)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_AllocateDamage(battle, units, dmgs);
            }
        }

        private static void BattleArmy_allocateDamage_TranspilerBody_receivesDamage(BattleArmy battle, List<UM> units, int[] dmgs, int index)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                dmgs[index] = hook?.onUnitReceivesArmyBattleDamage(battle, units[index], dmgs[index]) ?? dmgs[index];
            }
        }

        // BattleAgent hooks
        private static void BattleAgents_setupBattle_Postfix(BattleAgents __instance)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onAgentBattle_Setup(__instance);
            }
        }

        private static IEnumerable<CodeInstruction> BattleAgents_step_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_Intercept = AccessTools.Method(patchType, nameof(BattleAgents_step_Intercept), new Type[] { typeof(PopupBattleAgent), typeof(BattleAgents) });
            MethodInfo MI_Reinforce = AccessTools.Method(patchType, nameof(BattleAgents_step_ReinforceFromEscort), new Type[] { typeof(UA), typeof(UM) });

            FieldInfo FI_EscortLeft = AccessTools.Field(typeof(BattleAgents), nameof(BattleAgents.escortL));
            FieldInfo FI_EscortRight = AccessTools.Field(typeof(BattleAgents), nameof(BattleAgents.escortR));

            Label retLabel = ilg.DefineLabel();

            Label noInterceptLabel = ilg.DefineLabel();
            Label interceptLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Br)
                        {
                            yield return instructionList[i];

                            i++;
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Br)
                        {
                            retLabel = (Label)instructionList[i - 1].operand;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_1);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_Intercept);
                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, noInterceptLabel);

                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, interceptLabel);

                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Stloc_1);
                            yield return new CodeInstruction(OpCodes.Br_S, retLabel);

                            code = new CodeInstruction(OpCodes.Br_S, retLabel);
                            code.labels.Add(interceptLabel);
                            yield return code;

                            code = new CodeInstruction(OpCodes.Pop);
                            code.labels.Add(noInterceptLabel);
                            yield return code;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i + 1].opcode == OpCodes.Newobj)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_EscortLeft);
                            yield return new CodeInstruction(OpCodes.Call, MI_Reinforce);

                            i += 2;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i + 1].opcode == OpCodes.Newobj)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_EscortRight);
                            yield return new CodeInstruction(OpCodes.Call, MI_Reinforce);

                            i += 2;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Ret)
                        {
                            instructionList[i].labels.Add(retLabel);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleAgents_step_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int BattleAgents_step_Intercept(PopupBattleAgent popup, BattleAgents battle)
        {
            //Console.WriteLine("CommunityLib: Calling intercept hooks for BattleAgents.step");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                if (hook.interceptAgentBattleStep(popup, battle, out bool battleOver))
                {
                    if (battleOver)
                    {
                        //Console.WriteLine("CommunityLib: Intercept used by " + hook.GetType().Namespace + ". Battle is over.");
                        return 2;
                    }

                    //Console.WriteLine("CommunityLib: Intercept used by " + hook.GetType().Namespace + ". Battle is ongoing");
                    return 1;
                }
            }

            //Console.WriteLine("CommunityLib: Intercept not used");
            return 0;
        }

        private static Minion BattleAgents_step_ReinforceFromEscort(UA ua, UM escort)
        {
            Minion result = null;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                result = hook.onAgentBattle_ReinforceFromEscort(ua, escort);

                if (result != null)
                {
                    break;
                }
            }

            if (result == null)
            {
                result = new M_Knight(ua.map);
            }

            return result;
        }

        // Agent Battle Hooks BattleAgents_AttackDownRow_Minion_Transpiler
        private static IEnumerable<CodeInstruction> BattleAgents_AttackDownRow_Minion_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_MinionAttack = AccessTools.Method(patchType, nameof(BattleAgents_AttackDownRow_Minion_TranspilerBody_MinionAttack), new Type[] { typeof(BattleAgents), typeof(PopupBattleAgent), typeof(UA), typeof(int), typeof(int) });
            MethodInfo MI_TranspilerBody_ReceiveDamage = AccessTools.Method(patchType, nameof(BattleAgents_AttackDownRow_TranspilerBody_ReceiveDamage), new Type[] { typeof(BattleAgents), typeof(PopupBattleAgent), typeof(UA), typeof(int), typeof(int) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_3 && instructionList[i - 1].opcode == OpCodes.Endfinally)
                        {
                            targetIndex++;

                            // Call Minion Attack hook
                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg, 4);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_MinionAttack);
                            yield return new CodeInstruction(OpCodes.Stloc, 4);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Endfinally)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i - 1].opcode == OpCodes.Endfinally)
                        {
                            targetIndex = 0;

                            // Call Minion Attack hook
                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg, 4);
                            yield return new CodeInstruction(OpCodes.Ldarg_3);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 19);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_MinionAttack);
                            yield return new CodeInstruction(OpCodes.Stloc, 4);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleAgents_AttackDownRow_Minion_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int BattleAgents_AttackDownRow_Minion_TranspilerBody_MinionAttack(BattleAgents battle, PopupBattleAgent popup, UA me, int dmg, int row)
        {
            if (me != null && me.minions[row] != null)
            {
                //Console.WriteLine("CommunityLib: Minion about to attack");
                UA other = battle.att;
                if (battle.att == me)
                {
                    //Console.WriteLine("CommunityLib: other is defender");
                    other = battle.def;
                }

                //Console.WriteLine("CommunityLib: Callning hooks");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    dmg = hook.onMinionAttackAboutToBePerformed(me.minions[row], other, popup, battle, dmg, row);
                }

                dmg = BattleAgents_AttackDownRow_TranspilerBody_ReceiveDamage(battle, popup, other, dmg, row);
            }

            return dmg;
        }

        private static IEnumerable<CodeInstruction> BattleAgents_AttackDownRow_Agent_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_ReceiveDamage = AccessTools.Method(patchType, nameof(BattleAgents_AttackDownRow_TranspilerBody_ReceiveDamage), new Type[] { typeof(BattleAgents), typeof(PopupBattleAgent), typeof(UA), typeof(int), typeof(int) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldarg_S && instructionList[i - 1].opcode == OpCodes.Endfinally)
                    {
                        targetIndex = 0;

                        CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                        code.labels.AddRange(instructionList[i].labels);
                        instructionList[i].labels.Clear();
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg, 6);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 2);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ReceiveDamage);
                        yield return new CodeInstruction(OpCodes.Starg_S, 2);
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed BattleAgents_AttackDownRow_Agent_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int BattleAgents_AttackDownRow_TranspilerBody_ReceiveDamage(BattleAgents battle, PopupBattleAgent popup, UA defender, int dmg, int row)
        {
            //Console.WriteLine("CommunityLib: About to receive damage");
            Minion minion = defender.minions[row];
            if (minion != null && minion.isDead)
            {
                //Console.WriteLine("CommunityLib: minion is dead");
                minion = null;
            }

            //Console.WriteLine("CommunityLib: Calling hooks");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                dmg = hook.onAgentBattle_ReceiveDamage(popup, battle, defender, minion, dmg, row);
            }

            return dmg;
        }

        private static IEnumerable<CodeInstruction> BattleAgents_retreatOrFlee_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(BattleAgents_retreatOrFlee_TranspilerBody), new Type[] { typeof(UA) });

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i + 1].opcode == OpCodes.Stloc_1)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldarg_1)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Stloc_1);
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_1 && instructionList[i - 1].opcode == OpCodes.Endfinally)
                        {
                            targetIndex = 0;
                            returnCode = true;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed BattleAgents_retreatOrFlee_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static Location BattleAgents_retreatOrFlee_TranspilerBody(UA ua)
        {
            List<Location> neighbours = ua.location.getNeighbours();

            if (neighbours.Count == 0)
            {
                return null;
            }

            if (neighbours.Count == 1)
            {
                return neighbours[0];
            }

            List<Location> layerNeighboures = neighbours.Where(loc => loc.hex.z == ua.location.hex.z).ToList();

            if (layerNeighboures.Count == 1)
            {
                return layerNeighboures[0];
            }

            if (layerNeighboures.Count > 1)
            {
                return layerNeighboures[Eleven.random.Next(layerNeighboures.Count)];
            }

            return neighbours[Eleven.random.Next(neighbours.Count)];
        }

        private static bool BattleAgents_automatic_Prefix(BattleAgents __instance)
        {
            BattleAgents battle = null;
            if (__instance.GetType().IsSubclassOf(typeof(BattleAgents)))
            {
                battle = __instance;
            }
            else
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    BattleAgents retValue = hook.onAgentBattleStarts(__instance.att, __instance.def);
                    if (retValue != null)
                    {
                        battle = retValue;
                        break;
                    }
                }
            }

            if (battle == null)
            {
                return true;
            }
            else
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    bool retValue = hook.interceptAgentBattleAutomatic(battle);
                    if (retValue)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> World_bEndTurn_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_PopBattleAgent = AccessTools.Method(patchType, nameof(World_bEndTurn_TranspilerBody_PopAgentBattle), new Type[] { typeof(UA) });

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 1)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i - 1].opcode == OpCodes.Nop && instructionList[i + 1].opcode == OpCodes.Ldloc_S)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_PopBattleAgent);

                            returnCode = false;
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i + 1].opcode == OpCodes.Nop)
                        {
                            returnCode = true;
                            targetIndex = 0;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed World_bEndTurn_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void World_bEndTurn_TranspilerBody_PopAgentBattle(UA ua)
        {
            UA other = null;
            bool amAttacker = false;
            if (ua.engaging is UA engagedAgent)
            {
                other = engagedAgent;
                amAttacker = true;
            }
            else if (ua.engagedBy is UA engagingAgent)
            {
                other = engagingAgent;
            }

            if (other != null)
            {
                BattleAgents battle = null;
                if (amAttacker)
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        BattleAgents retValue = hook.onAgentBattleStarts(ua, other);

                        if (retValue != null)
                        {
                            battle = retValue;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        BattleAgents retValue = hook.onAgentBattleStarts(other, ua);

                        if (retValue != null)
                        {
                            battle = retValue;
                            break;
                        }
                    }
                }

                if (battle == null)
                {
                    if (amAttacker)
                    {
                        battle = new BattleAgents(ua, other);
                    }
                    else
                    {
                        battle = new BattleAgents(other, ua);
                    }
                }

                ua.map.world.prefabStore.popBattle(battle);

                if (ua.engaging != null)
                {
                    ua.engaging.engagedBy = null;
                    ua.engaging = null;
                }
                else if (ua.engagedBy != null)
                {
                    ua.engagedBy.engaging = null;
                    ua.engagedBy = null;
                }
            }
        }

        // Popup Battle Agent hooks
        private static void PopupBattleAgent_populate_Postfix(PopupBattleAgent __instance)
        {
            //Console.WriteLine("CommunityLib: PopupAgentBattle Populated. Calling hooks.");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopupBattleAgent_Populate(__instance, __instance.battle);
            }
        }

        // Raze Location Hooks
        private static void Task_RazeLocation_turnTick_Prefix()
        {
            razeIsValid = false;
        }

        private static void Task_RazeLocation_turnTick_Postfix(Unit unit)
        {
            if (razeIsValid && unit is UM um)
            {
                //Console.WriteLine("CommunityLib: onRazeLocationEndOfProcess");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook?.onRazeLocation_EndOfProcess(um);
                }
            }
        }

        private static IEnumerable<CodeInstruction> Task_RazeLocation_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_Hook = AccessTools.Method(patchType, nameof(Task_RazeLocation_turnTick_TranspilerBody_Hook), new Type[] { typeof(Unit) });
            MethodInfo MI_TranspilerBody_Kill = AccessTools.Method(patchType, nameof(Task_RazeLocation_turnTick_TranspilerBody_Kill), new Type[] { typeof(UM) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i+1].opcode == OpCodes.Ldarg_1 && instructionList[i+2].opcode == OpCodes.Ldfld)
                        {
                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Hook);

                            targetIndex++;
                        }
                    }
                    if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_0 && instructionList[i+1].opcode == OpCodes.Callvirt && instructionList[i-1].opcode == OpCodes.Stfld && instructionList[i-2].opcode == OpCodes.Ldnull)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Kill);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Task_RazeLocation_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Task_RazeLocation_turnTick_TranspilerBody_Hook(Unit u)
        {
            razeIsValid = true;

            if (u is UM um)
            {
                //Console.WriteLine("CommunityLib: onRazeLocation_StartOfProcess");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook?.onRazeLocation_StartOfProcess(um);
                }
            }
        }

        private static void Task_RazeLocation_turnTick_TranspilerBody_Kill(UM um)
        {
            if (um.person != null && um.location.settlement is SettlementHuman settlementHuman && settlementHuman.ruler != null)
            {
                um.person.statistic_kills++;
            }
        }

        private static bool Settlement_FallIntoRuin_Prefix(Settlement __instance, out bool __state, string v, object killer = null)
        {
            bool result = true;

            //Console.WriteLine("CommunityLib: interceptSettlementFallIntoRuin");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptSettlementFallIntoRuin(__instance, v, killer);

                if (retValue)
                {
                    result = false;
                }
            }

            __state = result;
            if (!result)
            {
                return result;
            }

            //Console.WriteLine("CommunityLib: onSettlementFallIntoRuin_StartOfProcess");
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onSettlementFallIntoRuin_StartOfProcess(__instance, v, killer);
            }

            return result;
        }

        private static void Settlement_FallIntoRuin_Postfix(Settlement __instance, bool __state, string v, object killer = null)
        {
            if (__state)
            {
                //Console.WriteLine("CommunityLib: onSettlementFallIntoRuin_EndOfProcess");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook.onSettlementFallIntoRuin_EndOfProcess(__instance, v, killer);
                }
            }
        }

        // Settlement Hooks
        private static IEnumerable<CodeInstruction> Settlement_FallIntoRuin_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_Intercept = AccessTools.Method(patchType, nameof(Settlement_FallIntoRuin_TranspilerBody_Intercept));
            MethodInfo MI_TranspilerBody_End = AccessTools.Method(patchType, nameof(Settlement_FallIntoRuin_TranspilerBody_End));

            Label end = ilg.DefineLabel();

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Intercept);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, end);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ret)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_End);
                            CodeInstruction instruction = new CodeInstruction(OpCodes.Nop);
                            instruction.labels.Add(end);
                            yield return instruction;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Settlement_FallIntoRuin_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Settlement_FallIntoRuin_TranspilerBody_Intercept(Settlement __instance, string v, object killer)
        {
            bool result = false;

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptSettlementFallIntoRuin(__instance, v, killer);

                if (retValue)
                {
                    result = true;
                }
            }

            if (result)
            {
                return result;
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onSettlementFallIntoRuin_StartOfProcess(__instance, v, killer);
            }

            return result;
        }

        private static void Settlement_FallIntoRuin_TranspilerBody_End(Settlement __instance, string v, object killer)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onSettlementFallIntoRuin_EndOfProcess(__instance, v, killer);
            }
        }

        private static void UIE_HolyTenet_bInfluence_Postfix(UIE_HolyTenet __instance)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onPlayerInfluenceTenet(__instance.tenet.order, __instance.tenet);
            }
        }

        private static void UILeftLocation_setTo_Postfix(UILeftLocation __instance, Location loc)
        {
            if (__instance.stat_faith.text == "")
            {
                HolyOrder order = null;
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    order = hook?.onLocationViewFaithButton_GetHolyOrder(loc);

                    if (order != null)
                    {
                        break;
                    }
                }

                if (order != null)
                {
                    __instance.stat_faith.text = order.getName();
                }
            }
        }

        private static IEnumerable<CodeInstruction> UILeftLocation_bViewFaith_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UIRightLocation_bViewFaith_TranspilerBody), new Type[] { typeof(Location) });

            FieldInfo FI_SelectedLocation = AccessTools.Field(typeof(UILeftLocation), nameof(UILeftLocation.selectedLoc));

            Label retLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i == 3)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Nop && instructionList[i - 2].opcode == OpCodes.Endfinally)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_SelectedLocation);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brtrue, retLabel);
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i + 1].opcode == OpCodes.Ret)
                        {
                            targetIndex = 0;

                            instructionList[i].labels.Add(retLabel);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UILeftLocation_bViewFaith_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool UIRightLocation_bViewFaith_TranspilerBody(Location loc)
        {
            HolyOrder order = null;
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                order = hook?.onLocationViewFaithButton_GetHolyOrder(loc);

                if (order != null)
                {
                    break;
                }
            }

            if (order != null)
            {
                loc.map.world.prefabStore.popHolyOrder(order);
                return true;
            }

            return false;
        }

        private static void PrefabStore_popHolyOrder_Prefix()
        {
            popupHolyOrder_PageText = null;
        }

        private static void PopupHolyOrder_setTo_Prefix(PopupHolyOrder __instance, HolyOrder soc, int page)
        {
            if (popupHolyOrder_PageText != null)
            {
                //Console.WriteLine("CommunityLib: Has page text for previous page (" + __instance.currentPage + ")");
                if (!popupHolyOrder_PageText.text.Equals(popupHolyOrder_DefaultPageText[__instance.currentPage]))
                {
                    //Console.WriteLine("CommunityLib: Resetting page text for previous page (" + __instance.currentPage + ")");
                    popupHolyOrder_PageText.text = string.Copy(popupHolyOrder_DefaultPageText[__instance.currentPage]);
                }
            }

            Text text = (Text)__instance.pages[page].gameObject.GetComponentsInChildren(typeof(Text), false).FirstOrDefault(tC => tC is Text t &&
                    (
                        t.text.StartsWith("Holy Orders' behaviours are determined by the") ||
                        t.text.StartsWith("These are the moral tenets of this Holy Order.") ||
                        t.text.StartsWith("Holy Orders gain gold from their agents,") ||
                        t.text.StartsWith("Holy Orders can have prophecies about")
                    )
                );

            if (text != null)
            {
                popupHolyOrder_PageText = text;

                if (popupHolyOrder_DefaultPageText[page] == null)
                {
                    //Console.WriteLine("CommunityLib: Storing default page text for page " + page);
                    popupHolyOrder_DefaultPageText[page] = string.Copy(text.text);
                }

                //Console.WriteLine("CommunityLib: Running hooks for page " + page);
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    text.text = hook.onPopupHolyOrder_DisplayPageText(soc, text.text, page);
                }
            }
        }

        private static IEnumerable<CodeInstruction> PopupHolyOrder_setTo_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_ProcessIncome = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_ProcessIncome));
            MethodInfo MI_TranspilerBody_DisplayBudget = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_DisplayBudget));
            MethodInfo MI_TranspilerBody_ComputeInfluenceDark = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceDark));
            MethodInfo MI_TranspilerBody_ComputeInfluenceHuman = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceHuman));
            MethodInfo MI_TranspilerBody_DisplayStats = AccessTools.Method(patchType, nameof(PopUpHolyOrder_setTo_TranspilerBody_DisplayStats));
            MethodInfo MI_TranspilerBody_DisplayInfluenceElder = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceElder));
            MethodInfo MI_TranspilerBody_DisplayInfluenceHuman = AccessTools.Method(patchType, nameof(PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceHuman));

            // Influence Dark and Influence Good Summaries
            FieldInfo FI_PopupHolyOrder_BudgetIncome = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.budgetIncome));
            FieldInfo FI_PopupHolyOrder_stats = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.stats));
            FieldInfo FI_PopupHolyOrder_influenceDark = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceDark));
            FieldInfo FI_PopupHolyOrder_influenceDarkp0 = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceDarkp0));
            FieldInfo FI_PopupHolyOrder_influenceGood = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceGood));
            FieldInfo FI_PopupHolyOrder_influenceGoodp0 = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceGoodp0));

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_1 && instructionList[i - 2].opcode == OpCodes.Ldarg_1)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ProcessIncome);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayBudget);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (instructionList[i].operand as FieldInfo) == FI_PopupHolyOrder_BudgetIncome)
                        {
                            targetIndex++;
                            returnCode = true;

                            i -= 3;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ComputeInfluenceDark);

                            i++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_2)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ComputeInfluenceHuman);

                            i++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_6 && instructionList[i - 1].opcode == OpCodes.Ldfld && (instructionList[i - 1].operand as FieldInfo) == FI_PopupHolyOrder_stats)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayStats);
                        }
                    }
                    else if (targetIndex == 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt)
                        {
                            targetIndex++;
                            returnCode = true;
                        }
                    }
                    else if (targetIndex == 7)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_PopupHolyOrder_influenceDark)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDark);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder);
                        }
                    }
                    else if (targetIndex == 8)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Stelem_Ref)
                        {
                            targetIndex++;
                            returnCode = true;
                        }
                    }
                    else if (targetIndex == 9)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_PopupHolyOrder_influenceGood)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGood);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman);
                        }
                    }
                    else if (targetIndex == 10)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Stelem_Ref)
                        {
                            targetIndex++;
                            returnCode = true;
                        }
                    }
                    else if (targetIndex == 11)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_PopupHolyOrder_influenceDarkp0)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDarkp0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder);
                        }
                    }
                    else if (targetIndex == 12)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Stelem_Ref)
                        {
                            targetIndex++;
                            returnCode = true;
                        }
                    }
                    else if (targetIndex == 13)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_PopupHolyOrder_influenceGoodp0)
                        {
                            targetIndex++;
                            returnCode = false;

                            yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGoodp0);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman);
                        }
                    }
                    else if (targetIndex == 14)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Stelem_Ref)
                        {
                            targetIndex = 0;
                            returnCode = true;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed PopupHolyOrder_setTo_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ProcessIncome(HolyOrder order, List<ReasonMsg> msgs)
        {
            Type orderType = order.GetType();
            if (orderType.IsSubclassOf(typeof(HolyOrder)))
            {
                while (orderType.IsSubclassOf(typeof(HolyOrder)))
                {
                    MethodInfo MI_processIncome = AccessTools.DeclaredMethod(orderType, "processIncome", new Type[] { typeof(List<ReasonMsg>) });
                    if (MI_processIncome != null)
                    {
                        return (int)MI_processIncome.Invoke(order, new object[] { msgs });
                    }

                    orderType = orderType.BaseType;
                }
            }

            return order.processIncome(msgs);
        }

        private static string PopupHolyOrder_setTo_TranspilerBody_DisplayBudget(int income, PopupHolyOrder popupOrder, HolyOrder order)
        {
            string result = "";

            List<ReasonMsg> msgs = new List<ReasonMsg>() {
                new ReasonMsg("Income", income),
                new ReasonMsgMax("Gold for Acolytes", order.cashForAcolytes, order.costAcolyte),
                new ReasonMsgMax("Gold for Conversion", order.cashForPreaching, order.costPreach),
                new ReasonMsgMax("Gold for Temples", order.cashForTemples, order.costTemple)
            };

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onPopupHolyOrder_DisplayBudget(order, msgs);
            }

            Text text = (Text)popupOrder.pages[popupOrder.currentPage].gameObject.GetComponentsInChildren(typeof(Text), false).FirstOrDefault(tC => tC is Text t && t.text.StartsWith("Income:"));

            if (text != null)
            {
                text.text = "";
            }

            foreach (ReasonMsg msg in msgs)
            {
                ReasonMsgMax msgMax = msg as ReasonMsgMax;
                if (msgMax != null)
                {
                    result = string.Concat(new string[]
                    {
                        result,
                        msgMax.value.ToString(),
                        "/",
                        msgMax.max.ToString(),
                        "\n"
                    });
                }
                else
                {
                    result = string.Concat(new string[]
                    {
                        result,
                        msg.value.ToString(),
                        "\n"
                    });
                }

                if (text != null)
                {
                    text.text = string.Concat(new string[] {
                        text.text,
                        msg.msg,
                        ":",
                        "\n"
                    });
                }
            }

            return result;
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceDark(HolyOrder order, List<ReasonMsg> msgs)
        {
            Type orderType = order.GetType();
            if (orderType.IsSubclassOf(typeof(HolyOrder)))
            {
                while (orderType.IsSubclassOf(typeof(HolyOrder)))
                {
                    MethodInfo MI_computeInfluenceDark = AccessTools.DeclaredMethod(orderType, "computeInfluenceDark", new Type[] { typeof(List<ReasonMsg>) });
                    if (MI_computeInfluenceDark != null)
                    {
                        return (int)MI_computeInfluenceDark.Invoke(order, new object[] { msgs });
                    }

                    orderType = orderType.BaseType;
                }
            }

            return order.computeInfluenceDark(msgs);
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceHuman(HolyOrder order, List<ReasonMsg> msgs)
        {
            Type orderType = order.GetType();
            if (orderType.IsSubclassOf(typeof(HolyOrder)))
            {
                while (orderType.IsSubclassOf(typeof(HolyOrder)))
                {
                    MethodInfo MI_computerInluenceHuman = AccessTools.DeclaredMethod(orderType, "computeInfluenceHuman", new Type[] { typeof(List<ReasonMsg>) });
                    if (MI_computerInluenceHuman != null)
                    {
                        return (int)MI_computerInluenceHuman.Invoke(order, new object[] { msgs });
                    }

                    orderType = orderType.BaseType;
                }
            }

            return order.computeInfluenceHuman(msgs);
        }

        private static string PopUpHolyOrder_setTo_TranspilerBody_DisplayStats(HolyOrder order)
        {
            string result = "";

            List<ReasonMsg> msgs = new List<ReasonMsg>() {
                new ReasonMsg("Acolytes", order.nAcolytes),
                new ReasonMsg("Worshippers", order.nWorshippers),
                new ReasonMsg("of which Rulers", order.nWorshippingRulers)
            };

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onPopupHolyOrder_DisplayStats(order, msgs);
            }

            foreach (ReasonMsg msg in msgs)
            {
                result = string.Concat(new string[] {
                    result,
                    msg.msg,
                    ": ",
                    msg.value.ToString(),
                    "\n"
                });
            }

            return result;
        }

        private static string PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceElder(HolyOrder order, int infGain)
        {
            string s = string.Concat(new string[] {
                "Elder Influence: ",
                order.influenceElder.ToString(),
                "/",
                order.influenceElderReq.ToString(),
                " (+",
                infGain.ToString(),
                "/turn)"
            });

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                s = hook?.onPopupHolyOrder_DisplayInfluenceElder(order, s, infGain);
            }

            return s;
        }

        private static string PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceHuman(HolyOrder order, int infGain)
        {
            string s = string.Concat(new string[] {
                "Human Influence: ",
                order.influenceHuman.ToString(),
                "/",
                order.influenceHumanReq.ToString(),
                " (+",
                infGain.ToString(),
                "/turn)"
            });

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                s = hook?.onPopupHolyOrder_DisplayInfluenceHuman(order, s, infGain);
            }

            return s;
        }

        private static List<Trait> UA_getStartingTraits_Postfix(List<Trait> traits, UA __instance)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onAgentLevelup_GetTraits(__instance, traits, true);
            }

            return traits;
        }

        private static List<Trait> Trait_getAvailableTraits_Postfix(List<Trait> traits, UA ua)
        {
            if (ua == null)
            {
                return traits;
            }

            if (!ua.hasStartingTraits() || ua.hasAssignedStartingTraits)
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook?.onAgentLevelup_GetTraits(ua, traits, false);
                }
            }
            return traits;
        }

        private static IEnumerable<CodeInstruction> Person_gainItem_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Person_gainItem_TranspilerBody));

            FieldInfo FI_Person_Items = AccessTools.Field(typeof(Person), nameof(Person.items));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex < 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldloc_S)
                        {
                            targetIndex++;

                            if (targetIndex == 3)
                            {
                                targetIndex = 0;
                                Label falseLabel = (Label)instructionList[i].operand;

                                yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Dup);
                                yield return new CodeInstruction(OpCodes.Ldfld, FI_Person_Items);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                                yield return new CodeInstruction(OpCodes.Ldelem_Ref);
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldarg_2);
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            }
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Person_gainItem_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool Person_gainItem_TranspilerBody(Person person, Item item, Item newItem, bool obligateHold)
        {
            bool result = true;

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook?.interceptReplaceItem(person, item, newItem, obligateHold) ?? false;

                if (retValue)
                {
                    result = false;
                }
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> SG_ActionTakingMonster_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_populate = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_populate), new Type[] { typeof(SG_ActionTakingMonster), typeof(List<MonsterAction>) });
            MethodInfo MI_TranspilerBody_getUtility = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_getUtility), new Type[] { typeof(SG_ActionTakingMonster), typeof(MonsterAction), typeof(double), typeof(List<ReasonMsg>) });
            MethodInfo MI_TranspilerBody_onAIDecision = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_onAIDecision), new Type[] { typeof(SG_ActionTakingMonster) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldarg_0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return instructionList[i];
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_populate);

                            i++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldarg_0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return instructionList[i];
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_populate);

                            i++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i - 1].opcode == OpCodes.Stloc_S && instructionList[i - 2].opcode == OpCodes.Callvirt)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_getUtility);
                            yield return new CodeInstruction(OpCodes.Stloc_S, 9);
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Stfld)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_onAIDecision);

                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed SG_ActionTakingMonster_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> UIScroll_Locs_checkData_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_populate = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_populate), new Type[] { typeof(SG_ActionTakingMonster), typeof(List<MonsterAction>) });
            MethodInfo MI_TranspilerBody_getUtility = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_getUtility), new Type[] { typeof(SG_ActionTakingMonster), typeof(MonsterAction), typeof(double), typeof(List<ReasonMsg>) });

            MethodInfo MI_SG_ActionTakingMonster_getActions = AccessTools.Method(typeof(SG_ActionTakingMonster), nameof(SG_ActionTakingMonster.getActions), new Type[0]);
            MethodInfo MI_MonsterAction_getUtility = AccessTools.Method(typeof(MonsterAction), nameof(MonsterAction.getUtility), new Type[] { typeof(List<ReasonMsg>) });

            FieldInfo FI_SrtableAN_msgs = AccessTools.Field(typeof(UIScroll_Locs.SortableAN), nameof(UIScroll_Locs.SortableAN.msgs));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Callvirt && instructionList[i + 2].opcode == OpCodes.Stloc_S)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Ldloc_S, 80);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 80);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_SG_ActionTakingMonster_getActions);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_populate);

                        i++;
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Ldfld && instructionList[i + 1].opcode == OpCodes.Callvirt)
                    {
                        targetIndex = 0;

                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldloc, 80);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 85);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 85);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 86);
                        yield return new CodeInstruction(OpCodes.Ldfld, FI_SrtableAN_msgs);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_MonsterAction_getUtility);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 86);
                        yield return new CodeInstruction(OpCodes.Ldfld, FI_SrtableAN_msgs);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_getUtility);

                        i += 2;
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIScroll_Locs_checkData_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static List<MonsterAction> SG_ActionTakingMonster_turnTick_TranspilerBody_populate(SG_ActionTakingMonster monster, List<MonsterAction> actions)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.populatingMonsterActions(monster, actions);
            }

            return actions;
        }

        private static double SG_ActionTakingMonster_turnTick_TranspilerBody_getUtility(SG_ActionTakingMonster monster, MonsterAction action, double utility, List<ReasonMsg> reasonMsgs)
        {
            double result = utility;

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                result = hook?.onActionTakingMonster_getUtility(monster, action, utility, reasonMsgs) ?? result;
            }

            return result;
        }

        private static void SG_ActionTakingMonster_turnTick_TranspilerBody_onAIDecision(SG_ActionTakingMonster monster)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onActionTakingMonsterAIDecision(monster);
            }
        }

        // Sovereign Hooks
        private static IEnumerable<CodeInstruction> Society_processActions_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_onAiDecision = AccessTools.Method(patchType, nameof(Society_processActions_TranspilerBody_onAIDecision), new Type[] { typeof(Society), typeof(Person) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Stfld && instructionList[i - 1].opcode == OpCodes.Ldloc_S)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_onAiDecision);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Society_processActions_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Society_processActions_TranspilerBody_onAIDecision(Society society, Person sovereign)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onSovereignAIDecision(society, sovereign);
            }
        }

        // Relationship Interaction Fixes
        private static IEnumerable<CodeInstruction> Society_populateActions_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_GetSGLayers = AccessTools.Method(patchType, nameof(GetSocialGroupLayers), new Type[] { typeof(Map) });
            MethodInfo MI_TranspilerBody_Society = AccessTools.Method(patchType, nameof(Society_populateActions_TranspilerBody_Society), new Type[] { typeof(Society), typeof(SocialGroup), typeof(Dictionary <SocialGroup, HashSet<int>>) });
            MethodInfo MI_TranspilerBody_Subsettlement = AccessTools.Method(patchType, nameof(Society_populateActions_TranspilerBody_Subsettlement), new Type[] { typeof(Society), typeof(Subsettlement), typeof(Dictionary<SocialGroup, HashSet<int>>) });
            MethodInfo MI_DipRelOther = AccessTools.Method(typeof(DipRel), nameof(DipRel.other), new Type[] { typeof(SocialGroup) });

            FieldInfo FI_Map = AccessTools.Field(typeof(SocialGroup), nameof(SocialGroup.map));

            Label incrementLabelA = ilg.DefineLabel();
            Label incrementLabelB = ilg.DefineLabel();

            int dictIndex = ilg.DeclareLocal(typeof(Dictionary<SocialGroup, HashSet<int>>)).LocalIndex;

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 0 && instructionList[i].opcode == OpCodes.Nop && instructionList[i-1].opcode == OpCodes.Nop && instructionList[i-2].opcode == OpCodes.Nop)
                        {
                            CodeInstruction code = new CodeInstruction(OpCodes.Nop);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Map);
                            yield return new CodeInstruction(OpCodes.Call, MI_GetSGLayers);
                            yield return new CodeInstruction(OpCodes.Stloc_S, dictIndex);

                            targetIndex++;
                        }
                    }
                    if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Br_S && instructionList[i+1].opcode == OpCodes.Ldarg_0 && instructionList[i-1].opcode == OpCodes.Nop)
                        {
                            targetIndex++;

                            incrementLabelA = (Label)instructionList[i].operand;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5); // Current Enumeration dipRel
                            yield return new CodeInstruction(OpCodes.Ldarg_0); // this
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_DipRelOther); //DipRel.other(this)
                            yield return new CodeInstruction(OpCodes.Ldloc_S, dictIndex);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Society);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, incrementLabelA);

                            yield return new CodeInstruction(OpCodes.Ldarg_0);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Endfinally)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i + 1].opcode == OpCodes.Ldfld && instructionList[i + 2].opcode == OpCodes.Ldloc_S)
                        {
                            incrementLabelB = (Label)instructionList[i - 2].operand;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, dictIndex);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Subsettlement);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, incrementLabelB);

                            yield return new CodeInstruction(OpCodes.Nop);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Society_populateActions_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static Dictionary<SocialGroup, HashSet<int>> GetSocialGroupLayers(Map map)
        {
            Dictionary<SocialGroup, HashSet<int>> result = new Dictionary<SocialGroup, HashSet<int>>();

            foreach(Location loc in map.locations)
            {
                if (loc.soc == null)
                {
                    continue;
                }

                if (result.TryGetValue(loc.soc, out HashSet<int> layers))
                {
                    layers.Add(loc.hex.z);
                }
                else
                {
                    result.Add(loc.soc, new HashSet<int> { loc.hex.z });
                }
            }

            return result;
        }

        private static bool Society_populateActions_TranspilerBody_Society(Society soc, SocialGroup other, Dictionary<SocialGroup, HashSet<int>> sgLayers)
        {
            Map map = soc.map;

            if (soc.map == null || soc.map.awarenessOfUnderground >= 1d)
            {
                return true;
            }

            if (soc.isDarkEmpire || soc.isOphanimControlled)
            {
                return true;
            }

            if (!other.hasNormalDiplomacy())
            {
                return true;
            }

            Hex capitol = soc.getCapitalHex();
            Hex otherCapitol = other.getCapitalHex();
            if (capitol == null || otherCapitol == null)
            {
                return true;
            }

            if ((capitol.z == 0 && otherCapitol.z == 1) || (capitol.z == 1 && otherCapitol.z == 0))
            {
                if (sgLayers.TryGetValue(soc, out HashSet<int> layers) && sgLayers.TryGetValue(other, out HashSet<int> otherLayers) && layers.Intersect(otherLayers).Any())
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool Society_populateActions_TranspilerBody_Subsettlement(Society soc, Subsettlement sub, Dictionary<SocialGroup, HashSet<int>> sgLayers)
        {
            Map map = soc.map;

            if (soc.map == null || soc.map.awarenessOfUnderground >= 1d)
            {
                return true;
            }

            if (soc.isDarkEmpire || soc.isOphanimControlled)
            {
                return true;
            }

            Hex capitol = soc.getCapitalHex();
            Hex targetHex = sub.settlement.location.hex;
            if (capitol == null || targetHex == null)
            {
                return true;
            }

            if ((capitol.z == 0 && targetHex.z == 1) || (capitol.z == 1 && targetHex.z == 0))
            {
                if (sgLayers.TryGetValue(soc, out HashSet<int> layers) && layers.Contains(sub.settlement.location.hex.z))
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> SG_Orc_getActions_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_GetSGLayers = AccessTools.Method(patchType, nameof(GetSocialGroupLayers), new Type[] { typeof(Map) });
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(SG_Orc_getActions_TranspilerBody), new Type[] { typeof(SG_Orc), typeof(SocialGroup), typeof(Dictionary<SocialGroup, HashSet<int>>) });
            MethodInfo MI_getNeighbours = AccessTools.Method(typeof(SocialGroup), nameof(SocialGroup.getNeighbours), new Type[0]);
            MethodInfo MI_getItem = AccessTools.PropertyGetter(typeof(List<MA_Orc_Attack>), "Item");

            FieldInfo FI_Map = AccessTools.Field(typeof(SocialGroup), nameof(SocialGroup.map));

            int neighboursIndex = ilg.DeclareLocal(typeof(List<SocialGroup>)).LocalIndex;
            int dictIndex = ilg.DeclareLocal(typeof(Dictionary<SocialGroup, HashSet<int>>)).LocalIndex;

            Label removeLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 0 && instructionList[i].opcode == OpCodes.Nop && instructionList[i-1].opcode == OpCodes.Br)
                        {
                            CodeInstruction code = new CodeInstruction(OpCodes.Nop);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Map);
                            yield return new CodeInstruction(OpCodes.Call, MI_GetSGLayers);
                            yield return new CodeInstruction(OpCodes.Stloc_S, dictIndex);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_0 && instructionList[i + 1].opcode == OpCodes.Stloc_S && instructionList[i + 2].opcode == OpCodes.Br_S)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_getNeighbours);
                            yield return new CodeInstruction(OpCodes.Stloc_S, neighboursIndex);
                            yield return new CodeInstruction(OpCodes.Nop);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, neighboursIndex);

                            i += 2;
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S)
                        {
                            removeLabel = (Label)instructionList[i].operand;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop)
                        {
                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, neighboursIndex);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_getItem);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, dictIndex);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, removeLabel);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i + 1].opcode == OpCodes.Call && instructionList[i + 2].opcode == OpCodes.Callvirt)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, neighboursIndex);

                            i += 2;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 7)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldloc_S && instructionList[i - 2].opcode == OpCodes.Stloc_S && instructionList[i - 3].opcode == OpCodes.Ldloc_S)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 8)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld)
                        {
                            Label incrementLabel = (Label)instructionList[i - 2].operand;

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, dictIndex);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, incrementLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_0);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed SG_Orc_getActions_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool SG_Orc_getActions_TranspilerBody(SG_Orc orcs, SocialGroup other, Dictionary<SocialGroup, HashSet<int>> sgLayers)
        {
            Map map = orcs.map;

            if (orcs.canGoUnderground())
            {
                return true;
            }

            Hex capitol = orcs.getCapitalHex();
            Hex otherCapitol = other.getCapitalHex();
            if (capitol == null || otherCapitol == null)
            {
                return true;
            }

            if ((capitol.z == 0 && otherCapitol.z == 1) || (capitol.z == 1 && otherCapitol.z == 0))
            {
                if (sgLayers.TryGetValue(orcs, out HashSet<int> layers) && sgLayers.TryGetValue(other, out HashSet<int> otherLayers) && layers.Intersect(otherLayers).Any())
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        // Religion UI Screen modification
        private static IEnumerable<CodeInstruction> PopupHolyOrder_bPrevNext_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_HolyOrder_isGone = AccessTools.Method(typeof(HolyOrder), nameof(HolyOrder.isGone));

            FieldInfo FI_PopupHolyOrder_us = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.us));

            Label continueLabel;
            Label isDeadLabel = ilg.DefineLabel();
            Label isAliveLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {

                if (targetIndex == 1)
                {
                    if (i > 0 && instructionList[i - 1].opcode == OpCodes.Brfalse_S && instructionList[i].opcode == OpCodes.Nop)
                    {
                        targetIndex++;
                        continueLabel = (Label)instructionList[i - 1].operand;

                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_HolyOrder_isGone);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);
                    }
                }
                else if (targetIndex == 2)
                {
                    if (instructionList[i - 1].opcode == OpCodes.Ldloc_0 && instructionList[i].opcode == OpCodes.Ldarg_0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_us);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_HolyOrder_isGone);
                        yield return new CodeInstruction(OpCodes.Brfalse_S, isAliveLabel);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                        yield return new CodeInstruction(OpCodes.Stloc_1);
                        yield return new CodeInstruction(OpCodes.Br_S, isDeadLabel);
                        CodeInstruction code = new CodeInstruction(OpCodes.Ldloc_0);
                        code.labels.Add(isAliveLabel);
                        yield return code;
                    }
                }
                else if (targetIndex == 3)
                {
                    if (instructionList[i - 1].opcode == OpCodes.Stloc_1 && instructionList[i].opcode == OpCodes.Ldarg_0)
                    {
                        targetIndex = 0;
                        instructionList[i].labels.Add(isDeadLabel);
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed PopupHolyOrder_bPrevNext_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Repeat Completed Challenge tweaks
        private static IEnumerable<CodeInstruction> PopupChallengeComplete_populate_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_AllowMultiple = AccessTools.Method(typeof(Challenge), nameof(Challenge.allowMultipleUsers), new Type[0]);

            Label falseLabel = ilg.DefineLabel();
            Label allowMultiSkipLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {

                if (targetIndex == 1)
                {
                    if (i > 0 && instructionList[i].opcode == OpCodes.Ldarg_2 && instructionList[i+1].opcode == OpCodes.Ldfld)
                    {
                        falseLabel = (Label)instructionList[i-1].operand;

                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_AllowMultiple);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, allowMultiSkipLabel);

                        targetIndex++;
                    }
                }
                else if (targetIndex == 2)
                {
                    if (instructionList[i].opcode == OpCodes.Ldarg_2)
                    {
                        instructionList[i].labels.Add(allowMultiSkipLabel);

                        targetIndex = 0;
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed PopupChallengeComplete_populate_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Tade Route Fixes 
        private static IEnumerable<CodeInstruction> ManagerTrade_checkTradeNetwork_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(ManagerTrade_checkTradeNetwork_TranspilerBody), new Type[] { typeof(ManagerTrade) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler ManagerTrade_checkTradeNetwork_Transpiler");
        }

        private static void ManagerTrade_checkTradeNetwork_TranspilerBody(ManagerTrade tradeManager)
        {
            List<Location> endpoints = Pathfinding.getTradeRouteEndPoints(tradeManager.map);

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onGetTradeRouteEndpoints(tradeManager.map, endpoints);
            }

            bool dirty = false;
            if (endpoints.Count != tradeManager.cached.Count)
            {
                dirty = true;
            }
            else
            {
                for (int i = 0; i < endpoints.Count; i++)
                {
                    if (endpoints[i].index != tradeManager.cached[i])
                    {
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                rebuildTradeRoutes(tradeManager, endpoints);

                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook.onBuildTradeNetwork_EndOfProcess(tradeManager.map, tradeManager, endpoints);
                }
            }

            foreach (TradeRoute route in tradeManager.routes)
            {
                route.turnTick(tradeManager.map);
            }
        }

        private static void rebuildTradeRoutes(ManagerTrade tradeManager, List<Location> endpoints)
        {
            if (endpoints == null || endpoints.Count < 2)
            {
                endpoints = Pathfinding.getTradeRouteEndPoints(tradeManager.map);
            }

            tradeManager.cached.Clear();
            foreach (Location endpoint in endpoints)
            {
                tradeManager.cached.Add(endpoint.index);
            }

            foreach (TradeRoute route in tradeManager.routes.ToList())
            {
                if (!endpoints.Contains(route.start()) || !endpoints.Contains(route.end()))
                {
                    tradeManager.routes.Remove(route);
                }
            }

            List<HashSet<int>> connectedSets = findAllConnectedSets(tradeManager);
            List<Location> endpointsConected = new List<Location>();
            List<Location> endpointsDisconnected = endpoints.ToList();
            foreach (HashSet<int> connectedSet in connectedSets)
            {
                foreach (int index in connectedSet)
                {
                    Location loc = tradeManager.map.locations[index];
                    if (!endpointsConected.Contains(loc))
                    {
                        endpointsConected.Add(loc);
                    }
                }
            }

            foreach (Location endpointConnected in endpointsConected)
            {
                endpointsDisconnected.Remove(endpointConnected);
            }

            World.log($"CommunityLib: Found {endpointsDisconnected.Count} endpoints for trade routes that are not connected to a trade route.");
            while (endpointsDisconnected.Count > 0)
            {
                List<Location> newlyConnectedEndpoints = new List<Location>();
                foreach (Location endpointDisconnected in endpointsDisconnected)
                {
                    Location[] routePath = Pathfinding.getTradeRouteFrom(endpointDisconnected, null, endpoints);
                    if (routePath == null || routePath.Length < 2)
                    {
                        newlyConnectedEndpoints.Add(endpointDisconnected);
                        World.log($"CommunityLib: Trade Route from {endpointDisconnected.getName()} failed to link to anywhere. Skipping endpoint...");
                        break;
                    }
                    else
                    {
                        newlyConnectedEndpoints.Add(endpointDisconnected);
                        newlyConnectedEndpoints.Add(routePath[routePath.Length - 1]);
                        tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                        World.log($"CommunityLib: Trade Route created from {endpointDisconnected.getName()} to {routePath[routePath.Length - 1].getName()}");
                        break;
                    }
                }

                foreach (Location endpoint in newlyConnectedEndpoints)
                {
                    endpointsDisconnected.Remove(endpoint);
                }
            }

            connectedSets = findAllConnectedSets(tradeManager);

            List<List<int>> indexGroups = new List<List<int>>();
            foreach (HashSet<int> set in connectedSets)
            {
                indexGroups.Add(set.ToList());
            }

            bool loop = true;
            int i = 0;
            while (loop && i < 5 * tradeManager.map.locations.Count)
            {
                loop = false;
                i++;

                if (indexGroups.Count > 1)
                {
                    indexGroups.Shuffle();
                    List<List<int>> unifiedIndexGroups = new List<List<int>>();

                    foreach (List<int> indexGroup in indexGroups)
                    {
                        foreach (List<int> otherGroup in indexGroups)
                        {
                            if (indexGroup == otherGroup)
                            {
                                continue;
                            }

                            Location[] routePath = Pathfinding.getTradeRouteTo(tradeManager.map.locations[indexGroup[0]], tradeManager.map.locations[otherGroup[0]]);

                            if (routePath == null || routePath.Length < 2)
                            {
                                World.log($"CommunityLib: Trade Route from set {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to {tradeManager.map.locations[otherGroup[0]].getName()} (size {otherGroup.Count}) is not possible. Checking against next group...");
                                continue;
                            }

                            World.log($"CommunityLib: Adding Trade Routes to set {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) until a connection is made to another set");
                            bool connectionInvalid = false;
                            int iterations = 0;
                            while (iterations < 25)
                            {
                                iterations++;

                                Location loc = tradeManager.map.locations[indexGroup[Eleven.random.Next(indexGroup.Count)]];
                                routePath = Pathfinding.getTradeRouteFrom(loc, endpoints);

                                if (routePath == null || routePath.Length < 2)
                                {
                                    World.log($"CommunityLib: Failed Trade Route from indexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}). Skippig group...");
                                    connectionInvalid = true;
                                    break;
                                }

                                tradeManager.routes.Add(new TradeRoute(routePath.ToList()));

                                List<int> connectedGroup = indexGroups.FirstOrDefault(group => group != indexGroup && group.Contains(routePath[routePath.Length - 1].index));
                                if (connectedGroup != null)
                                {
                                    World.log($"CommunityLib: Trade Route made connecting index Group {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to {tradeManager.map.locations[connectedGroup[0]].getName()} (size {connectedGroup.Count}) after {iterations} trade route(s) were added");
                                    
                                    foreach (int index in connectedGroup)
                                    {
                                        if (!indexGroup.Contains(index))
                                        {
                                            indexGroup.Add(index);
                                        }
                                    }
                                    unifiedIndexGroups.Add(connectedGroup);

                                    loop = true;
                                }

                                if (loop)
                                {
                                    break;
                                }
                            }

                            if (connectionInvalid)
                            {
                                continue;
                            }

                            if (loop)
                            {
                                break;
                            }

                            World.log($"CommunityLib: Timed out connecting from indexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to another index group after adding {iterations} additional trade routes.");
                        }

                        if (loop)
                        {
                            break;
                        }

                        World.log($"CommunityLib: IndexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) cannot form any trade routes to other groups. Skipping Group...");
                    }

                    foreach (List<int> unifiedIndexGroup in unifiedIndexGroups)
                    {
                        indexGroups.Remove(unifiedIndexGroup);
                    }
                }
            }
        }

        private static List<HashSet<int>> findAllConnectedSets(ManagerTrade tradeManager)
        {
            List<HashSet<int>> connectedSets = new List<HashSet<int>>();
            HashSet<int> visited = new HashSet<int>();

            World.log("Rebuilding all connected sets");
            foreach (TradeRoute route in tradeManager.routes)
            {
                if (!visited.Contains(route.start().index))
                {
                    HashSet<int> set = tradeManager.buildConnectedSet(route.start().index);
                    World.log("Rebuilding connected set from " + route.start().getName(true) + " size " + set.Count.ToString());
                    connectedSets.Add(set);
                    visited.UnionWith(set);
                }

                if (!visited.Contains(route.end().index))
                {
                    HashSet<int> set = tradeManager.buildConnectedSet(route.end().index);
                    World.log("Rebuilding connected set from " + route.end().getName(true) + " size " + set.Count.ToString());
                    connectedSets.Add(set);
                    visited.UnionWith(set);
                }
            }

            return connectedSets;
        }

        private static void PrefabStore_popUnderground_Postfix(PrefabStore __instance)
        {
            rebuildTradeRoutes(__instance.world.map.tradeManager, null);
        }

        // Pan To Holy Order
        private static void PopupMsgUnified_dismissAgentA_Postfix(PopupMsgUnified __instance)
        {
            if (ModCore.opt_panToHolyOrderScreen && __instance.objA is HolyOrder order)
            {
                if (order.map.world.ui.blocker == null)
                {
                    order.map.world.prefabStore.popHolyOrder(order);
                    return;
                }
            }
        }

        private static void PopupMsgUnified_dismissAgentB_Postfix(PopupMsgUnified __instance)
        {
            if (ModCore.opt_panToHolyOrderScreen && __instance.objB is HolyOrder order)
            {
                if (order.map.world.ui.blocker == null)
                {
                    order.map.world.prefabStore.popHolyOrder(order);
                    return;
                }
            }
        }

        // Holy Order Fixes
        private static IEnumerable<CodeInstruction> HolyOrder_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_ModCoreGet = AccessTools.Method(typeof(ModCore), nameof(ModCore.Get), new Type[0]);
            MethodInfo MI_IsSubsumed = AccessTools.Method(typeof(ModCore), nameof(ModCore.checkIsUnitSubsumed), new Type[] { typeof(Unit) });


            FieldInfo FI_Prophet = AccessTools.Field(typeof(HolyOrder), nameof(HolyOrder.prophet));

            Label label = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Br_S && instructionList[i-1].opcode == OpCodes.Ldfld)
                        {
                            label = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, label);
                            yield return new CodeInstruction(OpCodes.Pop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Prophet);
                            yield return new CodeInstruction(OpCodes.Call, MI_ModCoreGet);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_IsSubsumed);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                            yield return new CodeInstruction(OpCodes.Ceq);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed HolyOrder_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Overmind modification 
        private static IEnumerable<CodeInstruction> Overmind_getThreats_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_influenceElder = AccessTools.Field(typeof(HolyOrder), nameof(HolyOrder.influenceElder));
            MethodInfo MI_TranspilerBody_HolyOrderGone = AccessTools.Method(patchType, nameof(TranspilerBody_HolyOrder_isGone));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_influenceElder)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Brfalse_S)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 46);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_HolyOrderGone);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, instructionList[i - 1].operand);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Overmind_getThreats_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> DivineEntity_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_HolyOrderGone = AccessTools.Method(patchType, nameof(TranspilerBody_HolyOrder_isGone));

            FieldInfo FI_Order = AccessTools.Field(typeof(DivineEntity), nameof(DivineEntity.order));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 2 && instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Br && instructionList[i - 2].opcode == OpCodes.Stfld)
                        {
                            targetIndex = 0;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                            code.labels.Add(instructionList[i].labels[0]);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Order);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_HolyOrderGone);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, instructionList[i - 1].labels[0]);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed DivineEntity_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool TranspilerBody_HolyOrder_isGone(HolyOrder order)
        {
            //Console.WriteLine("CommunityLib: order " + order.getName() + " isGone returns " + order.isGone().ToString());
            return order.isGone();
        }

        // Pathfinding Modifications
        private static IEnumerable<CodeInstruction> Map_getPathTo_Location_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody_Intercept = AccessTools.Method(patchType, nameof(Map_getPathTo_Location_TranspilerBody_Intercept), new Type[] { typeof(Location), typeof(Location), typeof(Unit), typeof(bool) });
            MethodInfo MI_TranspilerBody_EntranceWonder = AccessTools.Method(patchType, nameof(Map_getPathTo_TranspilerBody_EntranceWonder), new Type[] { typeof(Location), typeof(Unit) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            Label end = ilg.DefineLabel();
            instructionList[instructionList.Count - 1].labels.Add(end);

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i == 0)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldarg_3);
                            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Intercept);
                            yield return new CodeInstruction(OpCodes.Br_S, end);
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Callvirt)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_3);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_EntranceWonder);

                            i++;
                        }
                    }
                }


                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Map_getPathTo_Location_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static Location[] Map_getPathTo_Location_TranspilerBody_Intercept(Location locA, Location locB, Unit u, bool safeMove)
        {
            return Pathfinding.getPathTo(locA, locB, u ,safeMove);
        }

        private static IEnumerable<CodeInstruction> Map_getPathTo_SocialGroup_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Map_getPathTo_SocialGroup_TranspilerBody_Intercept), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Unit), typeof(bool) });
            MethodInfo MI_TranspilerBody_EntranceWonder = AccessTools.Method(patchType, nameof(Map_getPathTo_TranspilerBody_EntranceWonder), new Type[] { typeof(Location), typeof(Unit) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            Label end = ilg.DefineLabel();
            instructionList[instructionList.Count - 1].labels.Add(end);

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && i == 0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Nop);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        yield return new CodeInstruction(OpCodes.Br_S, end);
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Callvirt)
                    {
                        targetIndex = 0;
                        i++;

                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_EntranceWonder);
                    }
                }


                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Map_getPathTo_SocialGroup_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static Location[] Map_getPathTo_SocialGroup_TranspilerBody_Intercept(Location loc, SocialGroup sg, Unit u, bool safeMove)
        {
            return Pathfinding.getPathTo(loc, sg, u, safeMove);
        }

        private static List<Location> Map_getPathTo_TranspilerBody_EntranceWonder(Location loc, Unit u)
        {
            List<Location> result = loc.getNeighbours();

            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (loc.settlement is Set_MinorOther && loc.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    Location tomb = u.map.locations.FirstOrDefault(l => ModCore.Get().checkIsElderTomb(l));
                    if (tomb != null && !result.Contains(tomb))
                    {
                        result.Add(tomb);
                    }

                    if (u.homeLocation != -1)
                    {
                        Location home = u.map.locations[u.homeLocation];
                        if (!result.Contains(home))
                        {
                            result.Add(home);
                        }
                    }
                }
            }

            return result;
        }

        private static void Map_adjacentMoveTo_Prefix(Map __instance, Unit u, Location loc, Location __state)
        {
            __state = u.location;

            bool theEntrance = false;
            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (u.location.settlement is Set_MinorOther && u.location.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    Location tomb = u.map.locations.FirstOrDefault(l => ModCore.Get().checkIsElderTomb(l));

                    if (tomb != null && loc == tomb)
                    {
                        theEntrance = true;
                    }

                    if (u.homeLocation != -1 && loc == __instance.locations[u.homeLocation])
                    {
                        theEntrance = true;
                    }
                }
            }

            if (theEntrance)
            {
                u.movesTaken--;

                foreach (Unit unit in __instance.units)
                {
                    if ((unit.task is Task_AttackUnit attack && attack.target == u) || (unit.task is Task_DisruptUA disrupt && disrupt.other == u) || (unit.task is Task_AttackUnitWithEscort attackEscort && attackEscort.target == u))
                    {
                        unit.task = null;
                    }
                }
            }
        }

        private static void Map_adjacentMoveTo_Postfix(Unit u, Location __state)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onMoveTaken(u, __state, u.location);
            }

            if (u.movesTaken > u.getMaxMoves())
            {
                u.movesTaken = u.getMaxMoves();
            }
        }

        private static IEnumerable<CodeInstruction> Map_moveTowards_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldnull)
                    {
                        targetIndex = 0;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        i++;
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Map_moveTowards_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Power Fixes
        private static IEnumerable<CodeInstruction> P_Opha_Smite_cast_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_HexZ = AccessTools.Field(typeof(Hex), nameof(Hex.z));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_0 && instructionList[i-1].opcode == OpCodes.Ldfld)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_HexZ);

                            i++;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Opha_Smite_Opha_Smite_cast_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void P_Under_BringTheFood_validTarget_Postfix(ref bool __result, Unit unit)
        {
            if (__result)
            {
                if (!unit.location.properties.Any(pr => pr is Pr_Death && pr.charge >= 1.0))
                {
                    __result = false;
                }
            }
        }

        private static void P_Under_DefendTheHeart_validTarget_Postfix(ref bool __result, Unit unit)
        {
            if (__result)
            {
                if (!unit.location.units.Any(u => u != unit && u is UM && u.society is Society society && u.society != u.map.soc_dark && !society.isDarkEmpire && !society.isOphanimControlled))
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<CodeInstruction> P_Under_DragBodiesUnder_cast_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i+1].opcode == OpCodes.Ldloc_S && instructionList[i+2].opcode == OpCodes.Stloc_2)
                        {
                            i += 3;
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i+1].opcode == OpCodes.Ldloc_S && instructionList[i+2].opcode == OpCodes.Castclass)
                        {
                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                            yield return new CodeInstruction(OpCodes.Stloc_2);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Under_DragBodiesUnder_cast_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void P_Under_Sinkholes_validTarget_Postfix(ref bool __result, Location loc)
        {
            if (__result)
            {
                HashSet<Location> visited = new HashSet<Location> { loc };
                List<Location> outerLocations = new List<Location> { loc };
                if (loc.units.Any(u => u is UM && u.society != u.map.soc_dark && !(u.task is Task_InBattle)))
                {
                    return;
                }

                for (int i = 0; i < 2; i++)
                {
                    List<Location> newOuterLocations = new List<Location>();

                    foreach (Location outerLoc in outerLocations)
                    {
                        foreach (Location neighbour in outerLoc.getNeighbours())
                        {
                            if (!visited.Contains(neighbour))
                            {
                                visited.Add(neighbour);
                                newOuterLocations.Add(neighbour);

                                if (neighbour.units.Any(u => u is UM && u.society != u.map.soc_dark && !(u.task is Task_InBattle)))
                                {
                                    return;
                                }
                            }
                        }
                    }

                    outerLocations = newOuterLocations;
                }

                __result = false;
            }
        }

        private static IEnumerable<CodeInstruction> P_Under_Sinkholes_lash_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_UnitMap = AccessTools.Field(typeof(Unit), nameof(Unit.map));
            FieldInfo FI_UnitSociety = AccessTools.Field(typeof(Unit), nameof(Unit.society));
            FieldInfo FI_TheDark = AccessTools.Field(typeof(Map), nameof(Map.soc_dark));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 0)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i-1].opcode == OpCodes.Brfalse_S)
                        {
                            Label continueLabel = (Label)instructionList[i-1].operand;

                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_UnitSociety);
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_UnitMap);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_TheDark);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Under_Sinkholes_lash_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void P_Under_VileSecretions_validTarget_Postfix(ref bool __result, Location loc)
        {
            if (__result)
            {
                if (loc.hex.z == 1)
                {
                    __result = false;
                }
            }
        }

        private static IEnumerable<CodeInstruction> P_Under_WithdrawAgent_validTraget_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_UnitMap = AccessTools.Field(typeof(Unit), nameof(Unit.map));
            FieldInfo FI_UnitSociety = AccessTools.Field(typeof(Unit), nameof(Unit.society));
            FieldInfo FI_TheDark = AccessTools.Field(typeof(Map), nameof(Map.soc_dark));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_0)
                        {
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Ceq);

                            i += 2;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Under_WithdrawAgent_validTraget_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // House Search Fix
        private static IEnumerable<CodeInstruction> UIScrollThreats_checkData_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_WriteLine = AccessTools.Method(typeof(Console), nameof(Console.WriteLine), new Type[] { typeof(string) });
            MethodInfo MI_NullOrWhitespace = AccessTools.Method(typeof(string), nameof(string.IsNullOrWhiteSpace), new Type[] { typeof(string) });
            MethodInfo MI_ConcatStringS2 = AccessTools.Method(typeof(string), nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            FieldInfo FI_House = AccessTools.Field(typeof(Person), nameof(Person.house));
            FieldInfo FI_HouseName = AccessTools.Field(typeof(House), nameof(House.name));

            Label continueLabel = ilg.DefineLabel();
            Label continueLabelB = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldstr && instructionList[i].operand is string str && str == "House Finder")
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Ldloc_S)
                        {
                            continueLabel = (Label)instructionList[i - 1].operand;

                            CodeInstruction code = new CodeInstruction(OpCodes.Ldloc_S, 53);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_House);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Ldloc_S)
                        {
                            // Logging
                            /*yield return new CodeInstruction(OpCodes.Ldstr, "CommunityLib: House is ");
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 53);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_House);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_HouseName);
                            yield return new CodeInstruction(OpCodes.Call, MI_ConcatStringS2);
                            yield return new CodeInstruction(OpCodes.Call, MI_WriteLine);*/

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Endfinally)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 58);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_House);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Ceq);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabelB);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 58);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_House);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_HouseName);
                            yield return new CodeInstruction(OpCodes.Call, MI_NullOrWhitespace);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabelB);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 7)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloca_S)
                        {
                            instructionList[i].labels.Add(continueLabelB);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIScrollThreats_checkData_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Dismiss Key fix
        private static bool PopupMsg_dismissKeyHit_Prefix(PopupMsg __instance)
        {
            if (__instance.ui == null)
            {
                return false;
            }

            return true;
        }

        // Victory Point Fixes
        private static IEnumerable<CodeInstruction> Overming_computeVictoryProgress_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_isDarkEmpire = AccessTools.Field(typeof(Society), nameof(Society.isDarkEmpire));

            Label skip = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Ldc_I4_4)
                        {
                            CodeInstruction code = new CodeInstruction(OpCodes.Ldloc_S, 20);
                            code.labels.AddRange(instructionList[i].labels);
                            instructionList[i].labels.Clear();
                            yield return code;
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_isDarkEmpire);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, skip);

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i - 1].opcode == OpCodes.Stind_R8)
                        {
                            instructionList[i].labels.Add(skip);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Overming_computeVictoryProgress_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        // Orc Expansion modifications
        private static IEnumerable<CodeInstruction> SG_Orc_canSettle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(SG_Orc_canSettle_TranspilerBody));

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler SG_Orc_canSettle_Transpiler");
        }

        private static bool SG_Orc_canSettle_TranspilerBody(SG_Orc orcs, Location location)
        {
            if (location.isOcean || location.hex.getHabilitability() < location.map.opt_orcHabMult * location.map.param.orc_habRequirement)
            {
                return false;
            }

            if (location.hex.z == 1 && !orcs.canGoUnderground())
            {
                return false;
            }

            if (location.settlement != null)
            {
                if (ModCore.Get().getSettlementTypesForOrcExpanion().TryGetValue(location.settlement.GetType(), out HashSet<Type> subsettlementBlacklist))
                {
                    if (subsettlementBlacklist != null)
                    {
                        foreach (Subsettlement sub in location.settlement.subs)
                        {
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }

            return true;
        }

        private static IEnumerable<CodeInstruction> Rt_Orcs_ClaimTerritory_validFor_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_TranspilerBody), new Type[] { typeof(UA) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler Rt_Orcs_ClaimTerritory_validFor_Transpiler");
        }

        private static bool Rt_Orcs_ClaimTerritory_validFor_TranspilerBody(UA ua)
        {
            //Console.WriteLine("CommunityLib: ClaimTerritory.validFor");

            SG_Orc orcSociety = ua.society as SG_Orc;

            if (orcSociety == null)
            {
                //Console.WriteLine("CommunityLib: ua is not of orc society");
                return false;
            }

            if (ua.location.isOcean || ua.location.hex.getHabilitability() < ua.location.map.opt_orcHabMult * ua.location.map.param.orc_habRequirement)
            {
                //Console.WriteLine("CommunityLib: Location is uninhabitable");
                return false;
            }

            if (ua.location.hex.z == 1 && !orcSociety.canGoUnderground())
            {
                return false;
            }

            if (ua.location.settlement != null)
            {
                //Console.WriteLine("CommunityLib: Testing Claim Territory against Permitted Settlements");
                if (ModCore.Get().getSettlementTypesForOrcExpanion().TryGetValue(ua.location.settlement.GetType(), out HashSet<Type> subsettlementBlacklist))
                {
                    if (subsettlementBlacklist != null && subsettlementBlacklist.Count > 0)
                    {
                        //Console.WriteLine("CommunityLib: Settlement of Type " + ua.location.settlement.GetType().Name + " may be expanded onto");
                        foreach (Subsettlement sub in ua.location.settlement.subs)
                        {
                            //Console.WriteLine("CommunityLibrary: Testing subsettlement of Type " + sub.GetType().Name + " against blacklist.");
                            if (subsettlementBlacklist.Contains(sub.GetType()))
                            {
                                //Console.WriteLine("CommunityLib: Blacklisted subsettlement found");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("CommunityLib: Location does not have a Permitted Settlement");
                    return false;
                }
            }

            if (orcSociety.lastTurnLocs.Count == 0)
            {
                //Console.WriteLine("CommunityLib: Orcs have no locations");
                return true;
            }

            if (ua.location.getNeighbours().Any(l => l.soc == orcSociety && l.settlement is Set_OrcCamp))
            {
                //Console.WriteLine("CommunityLib: Location neighbours orc society");
                return true;
            }

            if (ua.location.isCoastal)
            {
                //Console.WriteLine("CommunityLib: Location is coastal");
                foreach (Location location in ua.map.locations)
                {
                    if (location.soc == orcSociety && location.settlement is Set_OrcCamp camp && camp.specialism == 5)
                    {
                        //Console.WriteLine("CommunityLib: orc society has shipyard");
                        return true;
                    }
                }
            }

            //Console.WriteLine("CommunityLib: Location not claimable");
            return false;
        }

        // Culture modifications patches.
        private static IEnumerable<CodeInstruction> Set_MinorHuman_getSprite_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Set_MinorHuman_getSprite_TranspilerBody), new Type[] { typeof(Set_MinorHuman) });

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Stloc_1);
            yield return new CodeInstruction(OpCodes.Ldloc_1);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler Set_MinorHuman_getSprite_Transpiler");
        }

        private static Sprite Set_MinorHuman_getSprite_TranspilerBody(Set_MinorHuman set)
        {
            ModCultureData cultureData = null;

            if (World.self.loadedCultures.Count > 0)
            {
                Culture culture = World.self.loadedCultures[(int)(set.map.landmassID[set.location.hex.x][set.location.hex.y] + set.map.seed / 2L) % World.self.loadedCultures.Count];
                ModCore.Get().tryGetModCultureData(culture, out cultureData);
            }

            if (set.ophanimTakeOver)
            {
                if (ModCore.opt_allowCulturalMinorSettelementGraphics && cultureData != null && cultureData.ophanimMinorSettlementIcon != null)
                {
                    return cultureData.ophanimMinorSettlementIcon;
                }
                return set.map.world.textureStore.loc_evil_ophanimMinor;
            }

            if (set.subs.Count > 0 && set.subs[0].definesSprite())
            {
                if (ModCore.opt_allowCulturalMinorSettelementGraphics && cultureData != null && cultureData.subsettlmentMinorSettlementIcons.TryGetValue(set.subs[0].GetType(), out Sprite icon) && icon != null)
                {
                    return icon;
                }
                return set.subs[0].getLocationSprite(set.map.world);
            }

            if (set.location.isCoastal)
            {
                if (ModCore.opt_allowCulturalMinorSettelementGraphics && cultureData != null && cultureData.defaultMinorSettlementCoastalIcon)
                {
                    return cultureData.defaultMinorSettlementCoastalIcon;
                }
                return set.map.world.textureStore.loc_minor_fishing;
            }

            if (ModCore.opt_allowCulturalMinorSettelementGraphics && cultureData != null && cultureData.defaultMinorSettlementIcon != null)
            {
                return cultureData.defaultMinorSettlementIcon;
            }
            return set.map.world.textureStore.loc_minor_farm;
        }

        // Overmind_Automatic onIsElderTomb hooks
        private static IEnumerable<CodeInstruction> Overmind_Automatic_ai_testDark_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_checkIsElderTomb = AccessTools.Method(patchType, nameof(checkIsElderTomb_TranspilerBody), new Type[] { typeof(Location) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                            i += 4;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Overmind_Automatic_ai_testDark_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Overmind_Automatic_ai_testMagic_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_checkIsElderTomb = AccessTools.Method(patchType, nameof(checkIsElderTomb_TranspilerBody), new Type[] { typeof(Location) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                            i += 4;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Overmind_Automatic_ai_testMagic_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> Overmind_Automatic_checkSpawnAgent_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_checkIsElderTomb = AccessTools.Method(patchType, nameof(checkIsElderTomb_TranspilerBody), new Type[] { typeof(Location) });

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Isinst)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                        returnCode = false;
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Stloc_S)
                    {
                        targetIndex = 0;
                        returnCode = true;
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed Overmind_Automatic_checkSpawnAgent_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool checkIsElderTomb_TranspilerBody(Location location)
        {
            return ModCore.Get().checkIsElderTomb(location);
        }

        // onAgentIsRecruitable
        private static IEnumerable<CodeInstruction> PopupAgentCreation_populate_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(PopupAgentCreation_populate_TranspilerBody), new Type[] { typeof(Unit), typeof(bool) });

            Label afterChosenCheck = ilg.DefineLabel();
            Label nextUnit = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Isinst)
                    {
                        targetIndex++;

                        instructionList[i + 3].operand = nextUnit;

                        for (int j = instructionList.Count - 1; j >= 0; j--)
                        {
                            if (instructionList[j].opcode == OpCodes.Nop && instructionList[j - 1].opcode == OpCodes.Nop && instructionList[j - 2].opcode == OpCodes.Nop)
                            {
                                instructionList[j].labels.Add(nextUnit);
                                break;
                            }
                        }
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Brfalse && instructionList[i - 1].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Nop)
                    {
                        targetIndex++;

                        instructionList[i].operand = afterChosenCheck;
                    }

                    if (targetIndex == 3 && instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i - 1].opcode == OpCodes.Br && instructionList[i + 1].opcode == OpCodes.Callvirt)
                    {
                        targetIndex++;

                        instructionList[i].labels.Add(afterChosenCheck);
                    }

                    if (targetIndex == 4 && instructionList[i].opcode == OpCodes.Ceq)
                    {
                        targetIndex++;
                    }

                    if (targetIndex == 5 && instructionList[i].opcode == OpCodes.Ldloc_S)
                    {
                        targetIndex = 0;

                        yield return new CodeInstruction(OpCodes.Ldloc_S, 6);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 15);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        yield return new CodeInstruction(OpCodes.Stloc_S, 15);
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed PopupAgentCreation_populate_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool PopupAgentCreation_populate_TranspilerBody(Unit unit, bool result)
        {
            //Console.WriteLine("CommunityLib: Checking if unit is recruitable");
            if (unit is UA ua && !ua.isCommandable())
            {
                if (!(ua is UAG) && !(ua is UAA))
                {
                    result = false;
                }

                //Console.WriteLine("CommunityLib: Unit is noncommandable agent");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    result = hook.onAgentIsRecruitable(ua, result);
                }
            }
            else
            {
                return false;
            }

            //Console.WriteLine("CommunityLib: result is " + result);
            return result;
        }

        private static IEnumerable<CodeInstruction> P_Eternity_CreateAgent_createAgent_transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(P_Eternity_CreateAgent_createAgent_TranspilerBody), new Type[] { typeof(Curse), typeof(Person), typeof(Location), typeof(string) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i - 1].opcode == OpCodes.Pop)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Stloc_0);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Eternity_CreateAgent_createAgent_transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> P_Eternity_CreateAgentReusable_createAgent_transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(P_Eternity_CreateAgent_createAgent_TranspilerBody), new Type[] { typeof(Curse), typeof(Person), typeof(Location), typeof(string) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Dup && instructionList[i - 1].opcode == OpCodes.Nop)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 10);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Stloc_0);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_Eternity_CreateAgentReusable_createAgent_transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static string P_Eternity_CreateAgent_createAgent_TranspilerBody(Curse curse, Person person, Location loc, string text)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                if (hook != null)
                {
                    text = hook.onBrokenMakerPowerCreatesAgent_ProcessCurse(curse, person, loc, text);
                }
            }

            return text;
        }

        private static IEnumerable<CodeInstruction> UA_distanceDivisor_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UA_distanceDivisor_TranspilerBody), new Type[] { typeof(UA), typeof(Challenge), typeof(int) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldc_R8)
                    {
                        targetIndex = 0;

                        CodeInstruction code = new CodeInstruction(OpCodes.Ldarg_0);
                        code.labels.AddRange(instructionList[i].labels);
                        instructionList[i].labels.Clear();
                        yield return code;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        yield return new CodeInstruction(OpCodes.Stloc_0);
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UA_distanceDivisor_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int UA_distanceDivisor_TranspilerBody(UA ua, Challenge c, int distance)
        {
            if (!ModCore.opt_usePreciseDistanceDivisor)
            {
                return distance;
            }

            if (distance > 0 && !(c is Ritual))
            {
                Location[] pathTo;

                if (lastPath != null && lastPath.Item1 == ua && lastPath.Item2 == c.location && lastPath.Item3 == ua.map.turn)
                {
                    pathTo = lastPath.Item4;
                }
                else
                {
                    pathTo = ua.map.getPathTo(ua.location, c.location);
                    lastPath = new Tuple<Unit, Location, int, Location[]>(ua, c.location, ua.map.turn, pathTo);
                }

                if (pathTo == null || pathTo.Length < 2)
                {
                    distance = (int)Math.Ceiling((double)distance / ua.getMaxMoves());
                }
                else
                {
                    distance = (int)Math.Ceiling((double)pathTo.Length - 1 / ua.getMaxMoves());
                }

                if (distance > 0 && ua != null)
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        distance = hook.onUnitAI_GetsDistanceToLocation(ua, c.location, pathTo, distance);
                    }
                }

                distance = Math.Max(0, distance);
            }

            return distance;
        }

        private static void Task_AttackArmy_ctor_Postfix(Task_AttackArmy __instance, UM c, UM self)
        {
            __instance.turnsLeft = ModCore.Get().getTravelTimeTo(self, c.location) + 5;
        }

        private static void Task_AttackUnit_ctor_Postfix(Task_AttackUnit __instance, Unit c, Unit self)
        {
            __instance.turnsRemaining = ModCore.Get().getTravelTimeTo(self, c.location) + 5;
        }

        private static void Task_AttackUnitWithEscort_ctor_Postfix(Task_AttackUnitWithEscort __instance, Unit c, Unit self)
        {
            __instance.turnsRemaining = ModCore.Get().getTravelTimeTo(self, c.location) + 5;
        }

        private static void Task_Bodyguard_ctor_Postfix(Task_Bodyguard __instance, Unit c, Unit self)
        {
            __instance.turnsRemaining = ModCore.Get().getTravelTimeTo(self, c.location) + 5;
        }

        private static void Task_DisruptUA_ctor_Postfix(Task_DisruptUA __instance, Unit them, Unit us)
        {
            __instance.turnsLeft = ModCore.Get().getTravelTimeTo(us, them.location) + 10;
        }

        // Prefab Store hooks
        private static void Prefab_popHolyOrder_Prefix(HolyOrder order)
        {
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPlayerOpensReligionUI(order);
            }
        }

        // Universal AI Patches
        private static void UIScroll_Unit_checkData_Prefix()
        {
            populatedUM = false;
        }

        private static IEnumerable<CodeInstruction> UIScroll_Unit_checkData_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_UA = AccessTools.Method(patchType, nameof(UI_Scroll_Unit_checkData_TranspilerBody_UA));
            MethodInfo MI_TranspilerBody_UM = AccessTools.Method(patchType, nameof(UI_Scroll_Unit_checkData_TranspilerBody_UM));
            MethodInfo MI_Unit_isCommandable = AccessTools.Method(typeof(Unit), nameof(Unit.isCommandable));

            Label skip = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 5 && instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Callvirt && instructionList[i + 1].opcode == OpCodes.Ldloc_0)
                        {
                            targetIndex++;

                            for (int j = i; j < instructionList.Count; j++)
                            {
                                if (instructionList[j].opcode == OpCodes.Brfalse)
                                {
                                    skip = (Label)instructionList[j].operand;
                                    break;
                                }
                            }

                            yield return new CodeInstruction(OpCodes.Nop);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_UA);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, skip);
                        }
                    }
                    else if (targetIndex < 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && (MethodInfo)instructionList[i].operand == MI_Unit_isCommandable)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Br_S && instructionList[i - 1].opcode == OpCodes.Nop)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_UM);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIScroll_Unit_checkData_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool UI_Scroll_Unit_checkData_TranspilerBody_UA(UIScroll_Unit ui)
        {
            UA ua = GraphicalMap.selectedUnit as UA;
            //Console.WriteLine("CommunityLib: Got unit");

            if (ua == null)
            {
                //Console.WriteLine("CommunityLib: Unit is not UA");
                return false;
            }

            if (ModCore.Get().GetAgentAI().TryGetAgentType(ua.GetType(), out AgentAI.AIData aiData) && aiData != null)
            {
                if (aiData.controlParameters.hideThoughts)
                {
                    return true;
                }

                List<UIScroll_Unit.SortableTaskBlock> blocks = new List<UIScroll_Unit.SortableTaskBlock>();
                //Console.WriteLine("CommunityLib: Got valid challenges and rituals");
                foreach (AgentAI.ChallengeData challengeData in ModCore.Get().GetAgentAI().getAllValidChallengesAndRituals(ua))
                {
                    //Console.WriteLine("CommunityLib: Iterating " + challengeData.challenge.getName());
                    SortableTaskBlock_Advanced block = new SortableTaskBlock_Advanced();
                    block.challenge = challengeData.challenge;
                    block.utility = ModCore.Get().GetAgentAI().getChallengeUtility(challengeData, ua, aiData, aiData.controlParameters, block.msgs);
                    block.challengeData = challengeData;
                    blocks.Add(block);

                    if (challengeData.challenge is Ritual)
                    {
                        block.location = challengeData.location;
                    }
                    //Console.WriteLine("CommunityLib: Added " + challengeData.challenge.getName());
                }
                foreach (AgentAI.TaskData taskData in ModCore.Get().GetAgentAI().getAllValidTasks(ua))
                {
                    SortableTaskBlock_Advanced blockTask = new SortableTaskBlock_Advanced();
                    blockTask.challenge = null;
                    blockTask.taskType = taskData.aiTask.taskType;
                    blockTask.utility = ModCore.Get().GetAgentAI().checkTaskUtility(taskData, ua, aiData, aiData.controlParameters, blockTask.msgs);
                    blockTask.taskData = taskData;

                    switch (taskData.targetCategory)
                    {
                        case AITask.TargetCategory.Location:
                            if (taskData.targetLocation != null)
                            {
                                blockTask.location = taskData.targetLocation;
                            }
                            break;
                        case AITask.TargetCategory.SocialGroup:
                            if (taskData.targetSocialGroup != null)
                            {
                                blockTask.socialGroup = taskData.targetSocialGroup;
                            }
                            break;
                        case AITask.TargetCategory.Unit:
                            if (taskData.targetUnit != null)
                            {
                                blockTask.unit = taskData.targetUnit;
                            }
                            break;
                        default:
                            break;
                    }

                    blocks.Add(blockTask);
                }
                List<Unit> visibleUnits = ua.getVisibleUnits();
                if (visibleUnits != null)
                {
                    foreach (Unit unit in visibleUnits)
                    {
                        //Console.WriteLine("CommunityLib: Iterating " + unit.getName());
                        if (unit is UA agent)
                        {
                            //Console.WriteLine("CommunityLib: Unit is UA");
                            UIScroll_Unit.SortableTaskBlock blockAttack = new UIScroll_Unit.SortableTaskBlock();
                            blockAttack.unitToAttack = unit;
                            blockAttack.utility = ua.getAttackUtility(unit, blockAttack.msgs, aiData.controlParameters.includeDangerousFoe);
                            if (blockAttack.utility >= -1000)
                            {
                                blocks.Add(blockAttack);
                            }
                            //Console.WriteLine("CommunityLib: Added attack " + unit.getName());
                            if (ua != ua.map.awarenessManager.getChosenOne())
                            {
                                UIScroll_Unit.SortableTaskBlock blockGuard = new UIScroll_Unit.SortableTaskBlock();
                                blockGuard.unitToGuard = unit;
                                blockGuard.utility = ua.getBodyguardUtility(unit, blockGuard.msgs);
                                if (blockGuard.utility >= -1000)
                                {
                                    blocks.Add(blockGuard);
                                }
                                //Console.WriteLine("CommunityLib: Added Guard" + unit.getName());
                            }
                            if (agent.task is Task_PerformChallenge performChallenge && performChallenge.challenge.isChannelled())
                            {
                                UIScroll_Unit.SortableTaskBlock blockDisrupt = new UIScroll_Unit.SortableTaskBlock();
                                blockDisrupt.unitToDisrupt = unit;
                                blockDisrupt.utility = ua.getDisruptUtility(unit, blockDisrupt.msgs);
                                if (blockDisrupt.utility >= -1000)
                                {
                                    blocks.Add(blockDisrupt);
                                }
                                //Console.WriteLine("CommunityLib: Added Disrupt " + unit.getName());
                            }
                        }
                    }
                }

                //Console.WriteLine("CommunityLib: Created Blocks");
                blocks.Sort(ui);
                int num = 10;
                HashSet<Type> hashSet = new HashSet<Type>();
                foreach (UIScroll_Unit.SortableTaskBlock block in blocks)
                {
                    bool compression = ui.tHeroCompression.isOn;

                    if (block is SortableTaskBlock_Advanced advBlock)
                    {
                        if (compression)
                        {
                            if (advBlock.challenge != null)
                            {
                                if (hashSet.Contains(block.challenge.GetType()))
                                {
                                    continue;
                                }
                                hashSet.Add(block.challenge.GetType());
                            }
                            else if (advBlock.taskType != null)
                            {
                                if (hashSet.Contains(advBlock.taskType))
                                {
                                    continue;
                                }
                                hashSet.Add(advBlock.taskType);
                            }
                        }

                        GameObject gO = UnityEngine.Object.Instantiate(ui.master.world.prefabStore.uieChallengePerceptionBox, ui.listContent);

                        UIE_ChallengeTask task = gO.AddComponent<UIE_ChallengeTask>();
                        UIE_ChallengePerception perception = gO.GetComponent<UIE_ChallengePerception>();

                        task.transform.position = perception.transform.position;
                        task.transform.localScale = perception.transform.localScale;

                        task.title = perception.title;
                        //Console.WriteLine("CommunityLib: " + perception.title.transform.parent.name);
                        perception.title.transform.SetParent(task.transform);
                        //Console.WriteLine("CommunityLib: " + perception.title.transform.parent.name);
                        perception.title = null;

                        task.backColour = perception.backColour;
                        perception.backColour.transform.SetParent(task.transform);
                        perception.backColour = null;

                        task.iconBack = perception.iconBack;
                        perception.iconBack.transform.SetParent(task.transform);
                        perception.iconBack = null;

                        task.icon = perception.icon;
                        perception.icon.transform.SetParent(task.transform);
                        perception.icon = null;

                        task.button = perception.button;
                        perception.button.transform.SetParent(task.transform);
                        task.button.onClick.RemoveListener(perception.clickGOTO);
                        task.button.onClick.AddListener(task.clickGOTO);
                        perception.button = null;

                        task.tUtility = perception.tUtility;
                        perception.tUtility.transform.SetParent(task.transform);
                        perception.tUtility = null;

                        task.tLoc = perception.tLoc;
                        perception.tLoc.transform.SetParent(task.transform);
                        perception.tLoc = null;

                        UnityEngine.Object.Destroy(perception);
                        task.setTo(ui.master.world, advBlock, ua);
                    }
                    else
                    {
                        if (compression && block.challenge != null)
                        {
                            if (hashSet.Contains(block.challenge.GetType()))
                            {
                                continue;
                            }
                            hashSet.Add(block.challenge.GetType());
                        }
                        GameObject gO = UnityEngine.Object.Instantiate<GameObject>(ui.master.world.prefabStore.uieChallengePerceptionBox, ui.listContent);
                        gO.GetComponent<UIE_ChallengePerception>().setTo(ui.master.world, block);
                    }
                    num--;
                }
                //Console.WriteLine("CommunityLib: Instantiated Blocks");
                return true;
            }

            return false;
        }

        private static void UI_Scroll_Unit_checkData_TranspilerBody_UM(UIScroll_Unit ui)
        {
            if (populatedUM)
            {
                return;
            }
            else
            {
                populatedUM = true;
            }

            //Console.WriteLine("CommunityLib: UI_Scroll_Unit_checkData_TranspilerBody_UM");

            UM um = GraphicalMap.selectedUnit as UM;

            List<Hooks.TaskData> data = new List<Hooks.TaskData>();

            if (um != null && um.isCommandable())
            {
                //Console.WriteLine("CommunityLib: UM is commandable");
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    //Console.WriteLine("CommunityLib: calling onUIScroll_unit_populateUM hook for " + hook.GetType().Namespace);
                    List<Hooks.TaskData> retData = hook?.onUIScroll_Unit_populateUM(um);
                    //Console.WriteLine("CommunityLib: " + (retData?.ToString() ?? "ret data is null") + " " + (retData?.Count.ToString() ?? ""));

                    if (retData != null && retData.Count > 0)
                    {
                        //Console.WriteLine("CommunityLib: Recieved " + retData.Count + " TaskData objects");
                        data.AddRange(retData);
                    }
                }

                if (data.Count > 0)
                {
                    foreach (Hooks.TaskData taskData in data)
                    {
                        GameObject block = GameObject.Instantiate<GameObject>(ui.master.world.prefabStore.uieChallengeBox, ui.listContent);
                        UIE_Challenge challenge = block.GetComponent<UIE_Challenge>();

                        if (taskData.challenge != null)
                        {
                            challenge.setTo(ui.master.world, taskData.challenge, um);
                        }
                        else
                        {
                            challenge.title.text = taskData.title;
                            challenge.icon.sprite = taskData.icon;

                            if (taskData.profileGain != 0 || taskData.menaceGain != 0)
                            {
                                challenge.tStats.text = taskData.profileGain.ToString() + "\n" + taskData.menaceGain.ToString();
                            }
                            else
                            {
                                challenge.tStats.text = "";
                            }

                            challenge.backColour.color = taskData.backColor;
                            challenge.claimedBy.text = "";
                            challenge.tComplexity.text = "";
                            challenge.special = taskData.special;

                            if (taskData.enabled)
                            {
                                challenge.disabledMask.enabled = false;
                            }
                            else
                            {
                                challenge.disabledMask.enabled = true;
                            }

                            challenge.target = taskData.targetUA;
                            challenge.targetUM = taskData.targetUM;

                            challenge.button.onClick.AddListener(delegate ()
                            {
                                taskData.onClick(ui.master.world, um, challenge);
                            });
                        }
                    }

                    ui.textTabDesc.text = "Commandable Military can, among other tasks, Raze Human Settlements and cause heroes to retreat";
                }
                else
                {
                    ui.textTabDesc.text = "Commandable Military can Raze Human Settlements or cause heroes to retreat";
                }

            }
        }

        private static IEnumerable<CodeInstruction> UIScroll_Unit_Update_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_TimeStats = AccessTools.Method(patchType, nameof(UIScroll_Unit_Update_TranspilerBody_TimeStats));
            MethodInfo MI_TranspilerBody_Popout = AccessTools.Method(patchType, nameof(UIScroll_Unit_Update_TranspilerBody_Popout));

            FieldInfo FI_UIScroll_Unit_Master = AccessTools.Field(typeof(UIScroll_Unit), nameof(UIScroll_Unit.master));
            FieldInfo FI_UIMaster_World = AccessTools.Field(typeof(UIMaster), nameof(UIMaster.world));
            FieldInfo FI_UIScorll_challengePopout = AccessTools.Field(typeof(UIScroll_Unit), nameof(UIScroll_Unit.challengePopout));

            Label interceptUM = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (i > 2 && instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Brfalse && instructionList[i - 2].opcode == OpCodes.Ldloc_S && instructionList[i + 2].opcode == OpCodes.Ldfld && instructionList[i + 2].operand as FieldInfo == FI_UIScroll_Unit_Master && instructionList[i + 3].opcode == OpCodes.Ldfld && instructionList[i + 3].operand as FieldInfo == FI_UIMaster_World)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_TimeStats);
                            yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i - 1].operand);
                        }
                    }
                    else if (targetIndex < 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_UIScorll_challengePopout)
                        {
                            targetIndex++;
                        }

                        if (targetIndex == 6)
                        {
                            targetIndex++;

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Popout);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, interceptUM);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                    }
                    else if (targetIndex == 7)
                    {
                        if (instructionList[i].opcode == OpCodes.Br)
                        {
                            targetIndex = 0;

                            instructionList[i].labels.Add(interceptUM);
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIScroll_Unit_Update_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static bool UIScroll_Unit_Update_TranspilerBody_TimeStats()
        {
            if (GraphicalMap.selectedUnit is UA ua && ModCore.Get().GetAgentAI().TryGetAgentType(ua.GetType(), out AgentAI.AIData aiData) && aiData != null)
            {
                if (!aiData.controlParameters.valueTimeCost)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool UIScroll_Unit_Update_TranspilerBody_Popout(UIScroll_Unit __instance, UIE_Challenge challenge)
        {
            bool result = false;

            if (challenge.challenge != null)
            {
                return result;
            }

            UM um = GraphicalMap.selectedUnit as UM;
            if (um == null)
            {
                return result;
            }

            // Partially reconstruct TaskData
            Hooks.TaskData taskData = new Hooks.TaskData();
            taskData.challenge = challenge.challenge;
            taskData.title = challenge.title.text;
            taskData.icon = challenge.icon.sprite;
            taskData.backColor = challenge.backColour.color;

            if (challenge.disabledMask.enabled)
            {
                taskData.enabled = false;
            }
            else
            {
                taskData.enabled = true;
            }

            taskData.special = challenge.special;
            taskData.targetUA = challenge.target;
            taskData.targetUM = challenge.targetUM;

            // Prepopulate popoutData
            Hooks.TaskData_Popout popoutData = new Hooks.TaskData_Popout();
            popoutData.title = challenge.title.text;
            popoutData.icon = challenge.icon.sprite;

            if (challenge.target != null)
            {
                popoutData.iconBackground = challenge.target.getPortraitBackground();
            }
            else if (challenge.targetUM != null)
            {
                popoutData.iconBackground = challenge.targetUM.getPortraitBackground();
            }
            else
            {
                popoutData.iconBackground = __instance.master.world.iconStore.standardBack;
            }

            popoutData.progressReasonMsgs = new List<ReasonMsg>();

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                // Call hook.
                bool retValue = hook.interceptChallengePopout(um, taskData, ref popoutData);

                if (retValue)
                {
                    __instance.challengePopout.title.text = popoutData.title;
                    __instance.challengePopout.icon.sprite = popoutData.icon;

                    if (popoutData.iconBackground == null)
                    {
                        __instance.challengePopout.iconBack.sprite = __instance.master.world.iconStore.standardBack;
                    }
                    else
                    {
                        __instance.challengePopout.iconBack.sprite = popoutData.iconBackground;
                    }

                    __instance.challengePopout.tDesc.text = popoutData.description;
                    __instance.challengePopout.tRestriction.text = popoutData.restrictions;

                    if (popoutData.profileGain != 0 || popoutData.menaceGain != 0)
                    {
                        __instance.challengePopout.tStats.text = popoutData.profileGain.ToString() + "\n" + popoutData.menaceGain.ToString() + "\n";
                    }
                    else
                    {
                        __instance.challengePopout.tStats.text = "";
                    }

                    if (popoutData.backColor == null)
                    {
                        __instance.challengePopout.backColour.color = taskData.backColor;
                    }
                    else
                    {
                        __instance.challengePopout.backColour.color = popoutData.backColor;
                    }

                    if (popoutData.complexity > 0 && popoutData.progressPerTurn > 0)
                    {
                        __instance.challengePopout.tComplexity.text = popoutData.complexity.ToString() + "\n" + popoutData.progressPerTurn.ToString() + "\n" + Math.Ceiling((double)popoutData.complexity / (double)popoutData.progressPerTurn).ToString();
                    }
                    else
                    {
                        __instance.challengePopout.tComplexity.text = "";
                    }


                    if (popoutData.progressReasonMsgs?.Count > 0)
                    {
                        string text = "";
                        foreach (ReasonMsg msg in popoutData.progressReasonMsgs)
                        {
                            text += msg.msg + " +" + msg.value.ToString() + "\n";
                        }
                    }
                    else
                    {
                        __instance.challengePopout.tProgressReasons.text = "";
                    }

                    result = true;
                    break;
                }
            }

            return result;
        }

        // RECRUITABILITY //
        // Unit
        private static bool Unit_isCommandable_Postfix(bool result, UAEN __instance)
        {
            result = __instance.corrupted;

            if (!result)
            {
                foreach (Trait trait in __instance.person.traits)
                {
                    if (trait.grantsCommand())
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        // UA
        private static bool UA_isCommandable_Postfix(bool result, UA __instance)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null && intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
            {
                if (__instance.GetType() == droneType)
                {
                    result = __instance.corrupted;

                    if (!result)
                    {
                        foreach (Trait trait in __instance.person.traits)
                        {
                            if (trait.grantsCommand())
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        // UAEN OVERRIDE AI //
        // Negate unit interactions.
        private static bool UAEN_UnitInteraction_Prefix(UA __instance, ref double __result)
        {
            switch (__instance)
            {
                case UAEN_CaveSpider _:
                    __result = double.MinValue;
                    return false;
                case UAEN_DeepOne _:
                    __result = double.MinValue;
                    return false;
                case UAEN_Ghast _:
                    __result = double.MinValue;
                    return false;
                case UAEN_OrcUpstart _:
                    __result = double.MinValue;
                    return false;
                case UAEN_Vampire _:
                    __result = double.MinValue;
                    return false;
                default:
                    break;
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord != null)
            {
                if (intDataCord.typeDict.TryGetValue("Haematophage", out Type haematophageType) && haematophageType != null)
                {
                    if (__instance.GetType() == haematophageType)
                    {
                        __result = double.MinValue;
                        return false;
                    }
                }
            }

            return true;
        }

        private static double UAEN_getAttackUtility_Postfix(double utility, UA __instance, Unit other, List<ReasonMsg> reasons, bool includeDangerousFoe)
        {
            if (__instance is UAEN_CaveSpider)
            {
                return UAEN_CaveSpider_getAttackUtility(utility, __instance, other, reasons, includeDangerousFoe);
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord != null)
            {
                if (intDataCord.typeDict.TryGetValue("Haematophage", out Type haematophageType) && haematophageType != null)
                {
                    if (__instance.GetType() == haematophageType)
                    {
                        return UAEN_Cordyceps_Haematophage_getAttackUtility(utility, __instance, other, reasons, includeDangerousFoe);
                    }
                }
            }

            return utility;
        }

        public static double UAEN_CaveSpider_getAttackUtility(double utility, UA ua, Unit other, List<ReasonMsg> reasonMsgs, bool includeDangerousFoe)
        {
            if (other is UA && !(other is UAEN_CaveSpider))
            {
                utility = 100.0;
                reasonMsgs?.Add(new ReasonMsg("Base", utility));
                //Console.WriteLine("CommunityLib: Attack Utility for " + other.getName() + " is 100.0");
            }

            return utility;
        }

        public static double UAEN_Cordyceps_Haematophage_getAttackUtility(double utility, UA ua, Unit other, List<ReasonMsg> reasonMsgs, bool includeDangerousFoe)
        {
            if (other is UAG target && !other.isCommandable())
            {
                double attackStrength = target.getStatAttack();
                if (target.minions[0] != null)
                {
                    attackStrength += target.minions[0].getAttack();
                }

                if (ua.minions[0] != null || attackStrength < ua.hp)
                {
                    utility = 80.0;
                    reasonMsgs?.Add(new ReasonMsg("Base", utility));
                }
            }

            return utility;
        }

        private static bool UA_getVisibleUnits_Prefix(UA __instance, ref List<Unit> __result, out bool __state)
        {
            bool result = true;

            if (__result == null)
            {
                __result = new List<Unit>();
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                bool retValue = hook.interceptGetVisibleUnits(__instance, __result);

                if (retValue)
                {
                    result = false;
                }
            }

            __state = result;
            return result;
        }

        private static List<Unit> UA_getVisibleUnits_Postfix(List<Unit> visibleUnits, UA __instance, bool __state)
        {
            if (__state)
            {
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook.getVisibleUnits_EndOfProcess(__instance, visibleUnits);
                }
            }

            return visibleUnits;
        }

        private static bool UAEN_CaveSpider_turnTickAI_Prefix(UAEN_CaveSpider __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAEN_CaveSpider)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_DeepOne_turnTickAI_Prefix(UAEN_DeepOne __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAEN_DeepOne)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static void UAEN_DeepOne_ctor_Postfix(UAEN_DeepOne __instance)
        {
            Location location = __instance.map.locations[0];
            if (__instance.homeLocation != -1)
            {
                location = __instance.map.locations[__instance.homeLocation];
            }

            __instance.rituals.Add(new Rt_DeepOnes_TravelBeneath(location));
        }

        private static bool UAEN_Ghast_turnTickAI_Prefix(UAEN_Ghast __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAEN_Ghast)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_OrcUpstart_turnTickAI_Prefix(UAEN_OrcUpstart __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAEN_OrcUpstart)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_Vampire_turnTickAI_Prefix(UAEN_Vampire __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAEN_Vampire)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UA_turnTickAI_Prefix(UA __instance)
        {
            if (__instance is UAA && ModCore.Get().GetAgentAI().ContainsAgentType(typeof(UAA)))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static void Ch_Rest_InOrcCamp_complete_Postfix(UA u)
        {
            u.challengesSinceRest = 0;
        }

        // UIE_AgentRoster
        private static IEnumerable<CodeInstruction> UIE_AgentRoster_setToUA_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_engaging = AccessTools.Field(typeof(Unit), nameof(Unit.engaging));

            Label falseLabel;
            Label skipLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldfld)
                        {
                            falseLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_engaging);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);

                            instructionList[i + 1].labels.Add(skipLabel);

                            i++;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIE_AgentRoster_setToUA_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> UIE_AgentRoster_setToUM_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Agent = AccessTools.Field(typeof(UIE_AgentRoster), nameof(UIE_AgentRoster.agent));
            FieldInfo FI_engaging = AccessTools.Field(typeof(Unit), nameof(Unit.engaging));

            Label falseLabel;
            Label skipLabel = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldfld)
                        {
                            falseLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Agent);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_engaging);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);

                            instructionList[i + 1].labels.Add(skipLabel);

                            i++;
                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed UIE_AgentRoster_setToUM_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> UIE_AgentRoster_doAgentBattle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UIE_AgentRoster_doAgentBattle_TranspilerBody), new Type[] { typeof(UIE_AgentRoster) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler UIE_AgentRoster_doAgentBattle_Transpiler");
        }

        private static void UIE_AgentRoster_doAgentBattle_TranspilerBody(UIE_AgentRoster rosterItem)
        {
            if (rosterItem.agent == null)
            {
                return;
            }

            if (!(rosterItem.agent is UA ua))
            {
                if (rosterItem.agent.engaging != null)
                {
                    rosterItem.agent.engaging.engagedBy = null;
                    rosterItem.agent.engaging = null;
                }
                else if (rosterItem.agent.engagedBy != null)
                {
                    rosterItem.agent.engagedBy.engaging = null;
                    rosterItem.agent.engagedBy = null;
                }

                rosterItem.ui.checkData();
                return;
            }

            UA other = null;
            bool amAttacker = false;
            if (ua.engaging is UA engagedAgent)
            {
                other = engagedAgent;
                amAttacker = true;
            }
            else if (ua.engagedBy is UA engagingAgent)
            {
                other = engagingAgent;
            }

            if (other != null)
            {
                BattleAgents battle = null;
                if (amAttacker)
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        BattleAgents retValue = hook.onAgentBattleStarts(ua, other);

                        if (retValue != null)
                        {
                            battle = retValue;
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        BattleAgents retValue = hook.onAgentBattleStarts(other, ua);

                        if (retValue != null)
                        {
                            battle = retValue;
                            break;
                        }
                    }
                }

                if (battle == null)
                {
                    if (amAttacker)
                    {
                        battle = new BattleAgents(ua, other);
                    }
                    else
                    {
                        battle = new BattleAgents(other, ua);
                    }
                }

                rosterItem.ui.world.prefabStore.popBattle(battle);

                if (rosterItem.agent.engaging != null)
                {
                    rosterItem.agent.engaging.engagedBy = null;
                    rosterItem.agent.engaging = null;
                }
                else if (rosterItem.agent.engagedBy != null)
                {
                    rosterItem.agent.engagedBy.engaging = null;
                    rosterItem.agent.engagedBy = null;
                }

                rosterItem.ui.checkData();
            }
        }

        // engaging Patches

        private static IEnumerable<CodeInstruction> checkEngaging_BulkTranspiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(checkEngaging_BulkTranspilerBody), new Type[] { typeof(UA) });

            FieldInfo FI_engaging = AccessTools.Field(typeof(Unit), nameof(Unit.engaging));

            Label skipLabel = ilg.DefineLabel();

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldfld)
                        {
                            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_engaging);

                            instructionList[i + 1].labels.Add(skipLabel);
                            targetIndex++;
                        }
                    }
                    if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt)
                        {
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                            returnCode = false;
                            targetIndex++;
                        }
                    }
                    if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_0 && instructionList[i - 1].opcode == OpCodes.Call)
                        {
                            returnCode = true;
                            targetIndex = 0;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed checkEngaging_BulkTranspiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static string checkEngaging_BulkTranspilerBody(UA ua)
        {
            if (ua.engagedBy != null)
            {
                return ua.getName() + " cannot move as they are currently under attack by " + ua.engagedBy.getName() + ". You must resolve this combat before they can act.";
            }

            if (ua.engaging != null)
            {
                return ua.getName() + " cannot move as they are currently attacking " + ua.engaging.getName() + ". You must resolve this combat before they can act.";
            }

            return "ERROR: " + ua.getName() + " is neither being attacked by or attacking anyone.";
        }

        private static IEnumerable<CodeInstruction> UIInput_rightClickOnHex_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranwspilerBody = AccessTools.Method(patchType, nameof(UIInput_rightClickOnHex_TranspilerBody), new Type[] { typeof(UA) });

            FieldInfo FI_engaging = AccessTools.Field(typeof(Unit), nameof(Unit.engaging));

            Label skipLabel = ilg.DefineLabel();

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldfld)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldfld)
                        {
                            FieldInfo FI_procU = (FieldInfo)instructionList[i - 2].operand;

                            yield return new CodeInstruction(OpCodes.Brtrue_S, skipLabel);

                            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_procU);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_engaging);

                            instructionList[i + 1].labels.Add(skipLabel);
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt)
                        {
                            yield return new CodeInstruction(OpCodes.Call, MI_TranwspilerBody);

                            returnCode = false;
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_0 && instructionList[i - 1].opcode == OpCodes.Call)
                        {
                            returnCode = true;
                            targetIndex = 0;
                        }
                    }
                }

                if (returnCode)
                {
                    yield return instructionList[i];
                }
            }

            Console.WriteLine("CommunityLib: Completed UIInput_rightClickOnHex_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static string UIInput_rightClickOnHex_TranspilerBody(Unit u)
        {
            if (u.engagedBy != null)
            {
                return u.getName() + " cannot move as they are currently under attack by " + u.engagedBy.getName() + ". You must resolve this combat before they can act.";
            }

            if (u.engaging != null)
            {
                return u.getName() + " cannot move as they are currently attacking " + u.engaging.getName() + ". You must resolve this combat before they can act.";
            }

            return "ERROR: " + u.getName() + " is neither being attacked by or attacking anyone.";
        }

        // UAEN_OverrideAI
        private static void Rt_DescendIntoTheSea_validFor_Postfix(UA ua, ref bool __result)
        {
            if (ua.person.species is Species_DeepOne)
            {
                __result = false;
            }
        }

        private static void Rt_MaintainHumanity_validFor_Postfix(UA ua, ref bool __result)
        {
            if (ua.person.species is Species_DeepOne)
            {
                __result = false;
            }
        }

        // Mod Option Patches //
        // God Sort Patches
        private static void PrefabStore_getScrollSetGods_Prefix(List<God> gods)
        {
            God[] godsArray = new God[gods.Count];

            int lastPlayedIndex = -1;
            int swwfIndex = -1;
            HashSet<Type> baseGameGodTypes = new HashSet<Type> { typeof(God_LaughingKing), typeof(God_Vinerva), typeof(God_Ophanim), typeof(God_Mammon), typeof(God_Eternity), typeof(God_Cards), typeof(God_Underground) };
            List<int> godIndexes = new List<int>();
            List<int> moddedGodIndexes = new List<int>();

            //Console.WriteLine("Community Lib: Inital God Order:");
            for (int i = 0; i < gods.Count; i++)
            {
                //Console.WriteLine("CommunityLin: " + gods[i].getName());
                godsArray[i] = gods[i];

                if (ModCore.opt_godSort_splitModded)
                {
                    if (ModCore.opt_godSort_lastPlayedFirst && gods[i].getName() == ModCore.Get().data.getSaveData().lastPlayedGod)
                    {
                        lastPlayedIndex = i;
                    }
                    else if (ModCore.opt_godSort_swwfFirst && gods[i].GetType() == typeof(God_Snake))
                    {
                        swwfIndex = i;
                        //Console.WriteLine("CommunityLin: SWWF index is " + i);
                    }
                    else
                    {
                        if (baseGameGodTypes.Contains(gods[i].GetType()))
                        {
                            godIndexes.Add(i);
                        }
                        else if (gods[i].getName() == "Cordyceps Hive Mind")
                        {
                            godIndexes.Add(i);
                        }
                        else
                        {
                            moddedGodIndexes.Add(i);
                        }
                    }
                }
                else
                {
                    if (ModCore.opt_godSort_lastPlayedFirst && gods[i].getName() == ModCore.Get().data.getSaveData().lastPlayedGod)
                    {
                        lastPlayedIndex = i;
                    }
                    else if (ModCore.opt_godSort_swwfFirst && gods[i].GetType() == typeof(God_Snake))
                    {
                        swwfIndex = i;
                        //Console.WriteLine("CommunityLin: SWWF index is " + i);
                    }
                    else
                    {
                        godIndexes.Add(i);
                    }
                }
            }

            List<int> sortBlock = new List<int>();
            sortBlock.AddRange(godIndexes);

            godIndexes.Clear();
            if (ModCore.opt_godSort_lastPlayedFirst && lastPlayedIndex != -1)
            {
                godIndexes.Add(lastPlayedIndex);
            }

            if (ModCore.opt_godSort_swwfFirst && swwfIndex != -1)
            {
                godIndexes.Add(swwfIndex);
            }

            sortGods(sortBlock, godsArray);
            godIndexes.AddRange(sortBlock);

            if (ModCore.opt_godSort_splitModded)
            {
                sortBlock.Clear();
                sortBlock.AddRange(moddedGodIndexes);

                sortGods(sortBlock, godsArray);
                godIndexes.AddRange(sortBlock);
            }

            for (int i = 0; i < gods.Count; i++)
            {
                gods[i] = godsArray[godIndexes[i]];
            }

            /*Console.WriteLine("CommunityLib: Final God Order");
            foreach (God god in gods)
            {
                Console.WriteLine("CommunityLib: " + god.getName());
            }*/
        }

        private static void sortGods(List<int> sortBlock, God[] godsArray)
        {
            List<int> bonusSortBlock = new List<int>();
            List<int> minorSortBlock = new List<int>();

            foreach (int index in sortBlock)
            {
                if (godsArray[index].getName().Contains("Bonus God: "))
                {
                    bonusSortBlock.Add(index);
                }
                else if (godsArray[index].getName().Contains("Minor God: "))
                {
                    minorSortBlock.Add(index);
                }
            }

            if (ModCore.opt_godSort_bonusLast)
            {
                foreach (int index in bonusSortBlock)
                {
                    sortBlock.Remove(index);
                }

                if (ModCore.opt_godSort_Alphabetise)
                {
                    alphabetiseGodIndexes(bonusSortBlock, godsArray);
                }
            }

            if (ModCore.opt_godSort_minorLate)
            {
                foreach (int index in minorSortBlock)
                {
                    sortBlock.Remove(index);
                }

                if (ModCore.opt_godSort_Alphabetise)
                {
                    alphabetiseGodIndexes(minorSortBlock, godsArray);
                }
            }

            if (ModCore.opt_godSort_Alphabetise)
            {
                alphabetiseGodIndexes(sortBlock, godsArray);
            }

            if (ModCore.opt_godSort_minorLate)
            {
                sortBlock.AddRange(minorSortBlock);
            }

            if (ModCore.opt_godSort_bonusLast)
            {
                sortBlock.AddRange(bonusSortBlock);
            }
        }

        private static void alphabetiseGodIndexes(List<int> godIndexes, God[] godsArray)
        {
            /*Console.WriteLine("----------");
            Console.WriteLine("CommunityLib: Alphabetising gods");
            foreach (int index in godIndexes)
            {
                Console.WriteLine("CommunityLib: " + godsArray[index].getName());
            }
            Console.WriteLine("----------");*/

            if (godIndexes.Count < 2 || godsArray.Length < 2)
            {
                //Console.WriteLine("CommunityLib: Number of gods below 2 is sorted by default.");
                return;
            }

            bool sorted = false;
            int iterations = 0;
            int maxIterations = godIndexes.Count * godIndexes.Count;
            while (!sorted && iterations < maxIterations)
            {
                iterations++;

                bool changed = false;
                for (int i = 1; i < godIndexes.Count; i++)
                {
                    string godNameA = godsArray[godIndexes[i - 1]].getName();
                    string godNameB = godsArray[godIndexes[i]].getName();

                    //Console.WriteLine("CommunityLib: Comparing names for " + godNameA + " and " + godNameB);

                    if (ModCore.opt_godSort_ignorePrefixes)
                    {
                        //Console.WriteLine("CommunityLib: Trimming Prefixes");
                        char[] deliminator = new char[] { ':' };
                        string[] splitA = godNameA.Split(deliminator, 2);
                        if (splitA.Length == 2)
                        {
                            godNameA = splitA[1];
                        }

                        string[] splitB = godNameB.Split(deliminator, 2);
                        if (splitB.Length == 2)
                        {
                            godNameB = splitB[1];
                        }

                        //Console.WriteLine("CommunityLib: Trimmed God names are: " + godNameA + " and " +godNameB);
                    }

                    godNameA = godNameA.TrimStart();
                    godNameB = godNameB.TrimStart();
                    if (StringComparer.OrdinalIgnoreCase.Compare(godNameB, godNameA) < 0)
                    {
                        //Console.WriteLine("CommunityLib: " + godNameB + " should come before " + godNameA);
                        int indexA = godIndexes[i - 1];
                        int indexB = godIndexes[i];

                        godIndexes[i - 1] = indexB;
                        godIndexes[i] = indexA;

                        changed = true;
                    }
                }

                if (!changed)
                {
                    sorted = true;
                }
            }
            
            /*Console.WriteLine("----------");
            Console.WriteLine("CommunityLib: Gods alphabetised");
            foreach (int index in godIndexes)
            {
                Console.WriteLine("CommunityLib: " + godsArray[index].getName());
            }
            Console.WriteLine("----------");*/
        }

        // Orc Horde Count Patches
        private static IEnumerable<CodeInstruction> ManagerMajorThreats_turnTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(ManagerMajorThreats_turnTick_TranspilerBody));

            FieldInfo FI_Map = AccessTools.Field(typeof(ManagerMajorThreats), nameof(ManagerMajorThreats.map));

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_2)
                        {
                            targetIndex = 0;

                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Map);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);

                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed ManagerMajorThreats_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static int ManagerMajorThreats_turnTick_TranspilerBody(Map map)
        {
            int result = ModCore.opt_targetOrcCount;

            if (ModCore.opt_DynamicOrcCount)
            {
                if (map.sizeX * map.sizeY >= 3136)
                {
                    result++;
                }
                else if (map.sizeX * map.sizeY < 1600)
                {
                    result--;
                }
            }

            return result;
        }

        // Natural Wonder Count Patches
        private static IEnumerable<CodeInstruction> Map_placeWonders_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Map_placeWonders_TranspilerBody), new Type[] { typeof(Map) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler Map_placeWonders_Transpiler");
        }

        private static void Map_placeWonders_TranspilerBody(Map map)
        {
            if (map.tutorial)
            {
                return;
            }

            if (!ModCore.Get().data.getWonderGenTypes().Contains(typeof(Sub_Wonder_DeathIsland)))
            {
                ModCore.Get().data.addWonderGenType(typeof(Sub_Wonder_DeathIsland));
            }
            if (!ModCore.Get().data.getWonderGenTypes().Contains(typeof(Sub_Wonder_Doorway)))
            {
                ModCore.Get().data.addWonderGenType(typeof(Sub_Wonder_Doorway));
            }
            if (!ModCore.Get().data.getWonderGenTypes().Contains(typeof(Sub_Wonder_PrimalFont)))
            {
                ModCore.Get().data.addWonderGenType(typeof(Sub_Wonder_PrimalFont));
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                List<Type> retValue = hook.onMapGen_PlaceWonders();

                if (retValue != null)
                {
                    foreach (Type t in retValue)
                    {
                        ModCore.Get().data.addWonderGenType(t);
                    }
                }
            }

            int i = ModCore.opt_targetNaturalWonderCount;

            if (ModCore.opt_DynamicNaturalWonderCount)
            {
                if (map.sizeX * map.sizeY >= 3136)
                {
                    i++;
                }
                else if (map.sizeX * map.sizeY < 1600)
                {
                    i--;
                }
            }

            if (i < 1 || map.seed == 0L)
            {
                i = 1;
            }

            while (i > 0 && ModCore.Get().data.getWonderGenTypes().Count > 0)
            {
                Type wonderType = ModCore.Get().data.getWonderGenTypes()[Eleven.random.Next(ModCore.Get().data.getWonderGenTypes().Count)];
                ModCore.Get().data.getWonderGenTypes().Remove(wonderType);

                if (map.seed == 0L)
                {
                    wonderType = typeof(Sub_Wonder_Doorway);
                }

                if (wonderType == typeof(Sub_Wonder_DeathIsland))
                {
                    List<Location> locations = new List<Location>();
                    Location target = null;

                    foreach (Location location in map.locations)
                    {
                        if (location.hex.z == 0 && location.isOcean && !location.getNeighbours().Any(n => !n.isOcean) && location.settlement == null)
                        {
                            locations.Add(location);
                        }
                    }

                    if (locations.Count > 0)
                    {
                        target = locations[0];
                        if (Location.indexCounter > 1)
                        {
                            target = locations[Eleven.random.Next(locations.Count)];
                        }
                    }

                    if (target != null)
                    {
                        target.settlement = new Set_MinorOther(target);
                        target.settlement.subs.Clear();
                        target.settlement.subs.Add(new Sub_Wonder_DeathIsland(target.settlement));
                    }
                }
                else if (wonderType == typeof(Sub_Wonder_Doorway))
                {
                    List<Location> locations = new List<Location>();
                    Location target = null;

                    foreach (Location location in map.locations)
                    {
                        if (location.hex.z == 0 && location.settlement == null && !location.isOcean && (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY))
                        {
                            locations.Add(location);
                        }
                    }

                    if (locations.Count > 0)
                    {
                        target = locations[0];
                        if (Location.indexCounter > 1)
                        {
                            target = locations[Eleven.random.Next(locations.Count)];
                        }
                    }

                    if (target != null)
                    {
                        target.settlement = new Set_MinorOther(target);
                        target.settlement.subs.Clear();
                        target.settlement.subs.Add(new Sub_Wonder_Doorway(target.settlement));
                    }
                }
                else if (wonderType == typeof(Sub_Wonder_PrimalFont))
                {
                    List<Location> locations = new List<Location>();
                    Location target = null;

                    foreach (Location location in map.locations)
                    {
                        if (location.hex.z == 0 && location.settlement == null && !location.isOcean)
                        {
                            locations.Add(location);
                        }
                    }

                    if (locations.Count > 0)
                    {
                        target = locations[0];
                        if (Location.indexCounter > 1)
                        {
                            target = locations[Eleven.random.Next(locations.Count)];
                        }
                    }

                    if (target != null)
                    {
                        target.settlement = new Set_MinorOther(target);
                        target.settlement.subs.Clear();
                        target.settlement.subs.Add(new Sub_Wonder_PrimalFont(target.settlement));
                    }
                }
                else
                {
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        hook.onMapGen_PlaceWonders(wonderType);
                    }
                }

                i--;
            }

            ModCore.Get().data.getWonderGenTypes().Clear();
        }
    }
}
