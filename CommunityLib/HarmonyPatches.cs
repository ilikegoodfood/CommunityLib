using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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

        public static Text budgetLabels = null;

        private static bool populatedUM;

        private static bool razeIsValid;

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

            // HOOKS //
            // Unit death hooks
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.die), new Type[] { typeof(Map), typeof(string), typeof(Person) }), transpiler: new HarmonyMethod(patchType, nameof(Unit_die_Transpiler)));

            // Army Battle hooks
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.cycle), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_cycle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.unitMovesFromLocation), new Type[] { typeof(Unit), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_unitMovesFromLocation_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.computeAdvantage), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_computeAdvantage_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), "allocateDamage", new Type[] { typeof(List<UM>), typeof(int[]) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_allocateDamage_Transpiler)));

            // Raze Location Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Task_RazeLocation), nameof(Task_RazeLocation.turnTick), new Type[] { typeof(Unit) }), prefix: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Postfix)), transpiler: new HarmonyMethod(patchType, nameof(Task_RazeLocation_turnTick_Transpiler)));

            // Settlement destruction hooks
            harmony.Patch(original: AccessTools.Method(typeof(Settlement), nameof(Settlement.fallIntoRuin), new Type[] { typeof(string), typeof(object) }), prefix: new HarmonyMethod(patchType, nameof(Settlement_FallIntoRuin_Prefix)), postfix: new HarmonyMethod(patchType, nameof(Settlement_FallIntoRuin_Postfix)));
            
            // Religion UI Screen hooks
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.setTo), new Type[] { typeof(HolyOrder), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_setTo_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_HolyTenet), nameof(UIE_HolyTenet.bInfluenceNegatively), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UIE_HolyTenet_bInfluence_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(UIE_HolyTenet), nameof(UIE_HolyTenet.bInfluencePositively), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UIE_HolyTenet_bInfluence_Postfix)));

            // LevelUp Traits Hook
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getStartingTraits), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(UA_getStartingTraits_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Trait), nameof(Trait.getAvailableTraits), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Trait_getAvailableTraits_Postfix)));

            // Gain Item Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Person), nameof(Person.gainItem), new Type[] { typeof(Item), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Person_gainItem_Transpiler)));

            // Action Taking Monster Hooks
            harmony.Patch(original: AccessTools.Method(typeof(SG_ActionTakingMonster), nameof(SG_ActionTakingMonster.turnTick), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(SG_ActionTakingMonster_turnTick_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Locs), nameof(UIScroll_Locs.checkData), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Locs_checkData_Transpiler)));

            // onIsElderTomb Hooks
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.ai_testDark), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_ai_testDark_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.ai_testMagic), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_ai_testMagic_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Overmind_Automatic), nameof(Overmind_Automatic.checkSpawnAgent), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_Automatic_checkSpawnAgent_Transpiler)));

            // OnAgentIsRecruitable
            harmony.Patch(original: AccessTools.Method(typeof(PopupAgentCreation), nameof(PopupAgentCreation.populate), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupAgentCreation_populate_Transpiler)));

            // Graphical Hex Hooks
            //harmony.Patch(original: AccessTools.Method(typeof(GraphicalHex), nameof(GraphicalHex.checkData), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(GraphicalHex_checkData_Transpiler)));

            // SYSTEM MODIFICATIONS //
            // Assign Killer to Miscellaneous causes of death
            harmony.Patch(original: AccessTools.Method(typeof(UM_HumanArmy), nameof(UM_HumanArmy.turnTickInner)), transpiler: new HarmonyMethod(patchType, nameof(UM_HumanArmy_turnTickInner_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.skirmishDanger), new Type[] { typeof(UA), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishAttacking_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.skirmishDanger), new Type[] { typeof(UA), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishDefending_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Mg_Volcano), nameof(Mg_Volcano.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Mg_Volcano_Complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(God_Snake), nameof(God_Snake.awaken), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(God_Snake_Awaken_Transpiler)));

            // Religion UI Screen modification
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bPrev), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bNext), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));

            // Overmind modifications
            harmony.Patch(original: AccessTools.Method(typeof(Overmind), nameof(Overmind.getThreats), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(Overmind_getThreats_Transpiler)));

            // Pathfinding modifications
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.adjacentMoveTo), new Type[] { typeof(Unit), typeof(Location) }), prefix: new HarmonyMethod(patchType, nameof(Map_adjacentMoveTo_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.moveTowards), new Type[] { typeof(Unit), typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(Map_moveTowards_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.getPathTo), new Type[] { typeof(Location), typeof(Location), typeof(Unit), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Map_getPathTo_Location_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Map), nameof(Map.getPathTo), new Type[] { typeof(Location), typeof(SocialGroup), typeof(Unit), typeof(bool) }), transpiler: new HarmonyMethod(patchType, nameof(Map_getPathTo_SocialGroup_Transpiler)));

            // Orc Expansion modifications
            harmony.Patch(original: AccessTools.Method(typeof(SG_Orc), nameof(SG_Orc.canSettle), new Type[] { typeof(Location) }), transpiler: new HarmonyMethod(patchType, nameof(SG_Orc_canSettle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Rt_Orcs_ClaimTerritory), nameof(Rt_Orcs_ClaimTerritory.validFor), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Rt_Orcs_ClaimTerritory_validFor_Transpiler)));

            // AGENT UI //
            // UIScroll_Unit (Challenge utility panel)
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.checkData), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UIScroll_Unit_checkData_Prefix)), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_checkData_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.Update), new Type[0]), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_Update_Transpiler)));

            // RECRUITABILITY //
            // Unit
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.isCommandable), new Type[0]), postfix: new HarmonyMethod(patchType, nameof(Unit_isCommandable_Postfix)));

            // UAEN OVERRIDE AI //
            // Negate unit interactions.
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>), typeof(bool) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getBodyguardUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getDisruptUtility), new Type[] { typeof(Unit), typeof(List<ReasonMsg>) }), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getVisibleUnits), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Prefix)), postfix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Postfix)));

            // Override AI
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_DeepOne), nameof(UAEN_DeepOne.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_DeepOne_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Ghast), nameof(UAEN_Ghast.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_Ghast_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_OrcUpstart), nameof(UAEN_OrcUpstart.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Vampire), nameof(UAEN_Vampire.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_turnTickAI_Prefix)));

            // TEST ARTICLE
            //harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.turnTickAI), new Type[0]), prefix: new HarmonyMethod(patchType, nameof(UA_turnTickAI_Prefix)));

            // Ch_Rest_InOrcCamp
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Rest_InOrcCamp), nameof(Ch_Rest_InOrcCamp.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Rest_InOrcCamp_complete_Postfix)));

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        // Assign Killer to Miscellaneous causes of death
        private static IEnumerable<CodeInstruction> UM_HumanArmy_turnTickInner_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            bool found = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found && instructionList[i].opcode == OpCodes.Ldnull && instructionList[i-1].opcode == OpCodes.Call && instructionList[i+1].opcode == OpCodes.Callvirt)
                {
                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);

                    i++;
                }

                yield return instructionList[i];
            }
        }

        private static IEnumerable<CodeInstruction> Ch_SkirmishAttacking_skirmishDanger_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Battle = AccessTools.Field(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.battle));

            bool found = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found && instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Callvirt)
                {
                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, FI_Battle);

                    i++;
                }

                yield return instructionList[i];
            }
        }

        private static IEnumerable<CodeInstruction> Ch_SkirmishDefending_skirmishDanger_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_Battle = AccessTools.Field(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.battle));

            bool found = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found && instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i + 1].opcode == OpCodes.Callvirt)
                {
                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, FI_Battle);

                    i++;
                }

                yield return instructionList[i];
            }
        }


        private static IEnumerable<CodeInstruction> Mg_Volcano_Complete_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Mg_Volcano_Complete_TranspilerBody));

            int targetCount = -2;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetCount < 0)
                {
                    if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Ldstr && instructionList[i + 1].opcode == OpCodes.Callvirt)
                    {
                        if (targetCount == -2)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody);
                            targetCount++;
                            i++;
                        }
                        else if (targetCount == -1)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody);
                            targetCount++;
                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }
        }

        private static Person Mg_Volcano_Complete_TranspilerBody(UA uA)
        {
            return uA.person;
        }

        private static IEnumerable<CodeInstruction> God_Snake_Awaken_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int targetCount = 0;
            bool found = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found)
                {
                    if (instructionList[i].opcode == OpCodes.Ldnull && instructionList[i - 1].opcode == OpCodes.Ldstr && instructionList[i + 1].opcode == OpCodes.Callvirt)
                    {
                        targetCount++;

                        if (targetCount == 2)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            found = true;
                            i++;
                        }
                    }
                }

                yield return instructionList[i];
            }
        }

        // Unit death hooks
        private static IEnumerable<CodeInstruction> Unit_die_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_InterceptUnitDeath = AccessTools.Method(patchType, nameof(Unit_die_TranspilerBody_InterceptUnitDeath), new Type[] { typeof(Unit), typeof(string), typeof(Person) });
            MethodInfo MI_TranspilerBody_StartOfProcess = AccessTools.Method(patchType, nameof(Unit_die_TranspilerBody_StartOfProcess), new Type[] { typeof(Unit), typeof(string), typeof(Person) });

            Label retLabel = instructionList[instructionList.Count - 1].labels[0];

            bool hookIntercept = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!hookIntercept && i == 1 && instructionList[i-1].opcode == OpCodes.Nop)
                {
                    hookIntercept = true;

                    // Implements interceptUnitDeath hook
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_InterceptUnitDeath);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, retLabel);
                    // Implements onUnitDeath_StartOfProces hook
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_StartOfProcess);
                }

                yield return instructionList[i];
            }
        }

        private static bool Unit_die_TranspilerBody_InterceptUnitDeath(Unit u, string v, Person killer = null)
        {
            bool result = false;

            //Console.WriteLine("CommunityLib: Intercept Unit Death");
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook.interceptUnitDeath(u, v, killer);

                if (retValue)
                {
                    result = true;
                }
            }

            return result;
        }

        private static void Unit_die_TranspilerBody_StartOfProcess(Unit u, string v, Person killer = null)
        {
            //Console.WriteLine("CommunityLib: onUnitDeath_StartOfProcess");
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook.onUnitDeath_StartOfProcess(u, v, killer);
            }
        }

        // Army Battle hooks
        private static IEnumerable<CodeInstruction> BattleArmy_cycle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            // Transpiler Bodies for Data management.
            MethodInfo MI_TranspilerBody_GatherData = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_GatherData), new Type[] { typeof(BattleArmy) });

            // Transpiler Bodies for Hooks
            MethodInfo MI_TranspilerBody_InterceptCycle = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_InterceptCycle), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_StartOfProcess = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_StartOfProcess), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_EndOfProcess = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_EndOfProcess), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_Victory = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_onArmyBattleVictory), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_DamageCalc = AccessTools.Method(patchType, nameof(BattleArmy_cycle_TranspilerBody_DamageCalculated), new Type[] { typeof(BattleArmy), typeof(int), typeof(int), typeof(int), typeof(bool) });

            FieldInfo FI_BattleArmy_Done = AccessTools.Field(typeof(BattleArmy), nameof(BattleArmy.done));

            Label retLabel = instructionList[instructionList.Count - 1].labels[0];

            bool dataGathered = false;
            bool hooksIntecerptAndStartOfProcess = false;
            bool hookVictory = false;
            int hookVictoryCount = 0;
            bool hookDamage = false;
            int hookDamageCount = 0;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!dataGathered && i > 0 && instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Nop && instructionList[i + 1].opcode == OpCodes.Ldarg_0)
                {
                    dataGathered = true;
                    // Gathers armyBattleData_StartOfCycle
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_GatherData);
                }

                if (!hooksIntecerptAndStartOfProcess && i > 0 && instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Nop && instructionList[i + 1].opcode == OpCodes.Ldarg_0)
                {
                    hooksIntecerptAndStartOfProcess = true;
                    // Implements interceptArmyBattleCycle hook
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_InterceptCycle);
                    yield return new CodeInstruction(OpCodes.Brtrue, retLabel);
                    // Implements onArmyBattleCycle_StartOfProcess hook
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_StartOfProcess);
                }

                if (!hookVictory && instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i + 1].opcode == OpCodes.Ldfld && instructionList[i + 2].opcode == OpCodes.Callvirt && instructionList[i + 3].opcode == OpCodes.Brfalse_S)
                {
                    hookVictoryCount++;
                    if (hookVictoryCount >= 2)
                    {
                        hookVictory = true;
                    }

                    //Implements onArmyBattleVictory hook int two locations
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Victory);
                }

                if (!hookDamage && i > 2 && instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i-1].opcode == OpCodes.Stloc_S && instructionList[i-2].opcode == OpCodes.Conv_I4)
                {
                    hookDamageCount++;
                    if (hookDamageCount >= 2)
                    {
                        hookDamage = true;
                    }

                    yield return new CodeInstruction(OpCodes.Ldarg_0);

                    if (hookDamageCount == 1)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc, 48);
                        yield return new CodeInstruction(OpCodes.Ldloc, 45);
                        yield return new CodeInstruction(OpCodes.Ldloc, 46);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc, 56);
                        yield return new CodeInstruction(OpCodes.Ldloc, 53);
                        yield return new CodeInstruction(OpCodes.Ldloc, 54);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    }

                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DamageCalc);
                    
                    if (hookDamageCount == 1)
                    {
                        yield return new CodeInstruction(OpCodes.Stloc, 48);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Stloc, 56);
                    }
                }

                if (i == instructionList.Count - 1 && instructionList[i].opcode == OpCodes.Ret)
                {
                    // Implements onArmyBattleCycle_EndOfproccess
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, FI_BattleArmy_Done);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, retLabel);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_EndOfProcess);
                }

                yield return instructionList[i];
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_GatherData(BattleArmy battle)
        {
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
        }

        private static bool BattleArmy_cycle_TranspilerBody_InterceptCycle(BattleArmy battle)
        {
            bool result = false;
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            return result;
        }

        private static void BattleArmy_cycle_TranspilerBody_StartOfProcess(BattleArmy battle)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_StartOfProcess(battle);
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_EndOfProcess(BattleArmy battle)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_EndOfProcess(battle);
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_onArmyBattleVictory(BattleArmy battle)
        {
            ArmyBattleData data = armyBattleData_StartOfCycle;
            Tuple<List<UM>, List<UA>> victors = null;
            List<UM> victorUnits = new List<UM>();
            List<UA> victorComs = new List<UA>();
            List<UM> defeatedUnits = new List<UM>();
            List<UA> defeatedComs= new List<UA>();

            if (battle.attackers.Count == 0 && battle.defenders.Count == 0)
            {
                victors = new Tuple<List<UM>, List<UA>>(new List<UM>(), new List<UA>());

                defeatedUnits.AddRange(data.GetAttackers().Item1);
                defeatedUnits.AddRange(data.GetDefenders().Item1);

                defeatedComs.AddRange(data.GetAttackers().Item2);
                defeatedComs.AddRange(data.GetDefenders().Item2);
            }
            else if (battle.attackers.Count == 0)
            {
                victors = data.GetDefenders();
                victorUnits = victors.Item1;
                victorComs = victors.Item2;

                foreach (UM u in data.attackers)
                {
                    if (u.isDead)
                    {
                        defeatedUnits.Add(u);
                    }
                }
                foreach (UA u in data.attComs)
                {
                    if (u.isDead)
                    {
                        defeatedComs.Add(u);
                    }
                }
            }
            else if (battle.defenders.Count == 0)
            {
                victors = data.GetAttackers();
                victorUnits = victors.Item1;
                victorComs = victors.Item2;

                foreach (UM u in data.defenders)
                {
                    if (u.isDead)
                    {
                        defeatedUnits.Add(u);
                    }
                }
                foreach (UA u in data.defComs)
                {
                    if (u.isDead)
                    {
                        defeatedComs.Add(u);
                    }
                }
            }

            if (victors != null)
            {
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            List<int> targetIndexesA = new List<int>();

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Pop && instructionList[i-1].opcode == OpCodes.Callvirt && instructionList[i-4].opcode == OpCodes.Ldarg_0)
                {
                    targetIndexesA.Add(i - 4);
                }
            }

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndexesA.Count > 0 && i == targetIndexesA[0])
                {
                    targetIndexesA.RemoveAt(0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_onArmyBattleRetreatOrFlee);
                }

                if (instructionList[i].opcode == OpCodes.Call && instructionList[i].operand as MethodInfo == MI_BattleArmy_endBattle)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TransplilerBody_onArmyBattleTerminated);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                }

                yield return instructionList[i];
            }
        }

        private static void BattleArmy_unitMovesFromLocation_TranspilerBody_OnAmryBattleRetreatOrFlee(BattleArmy battle, Unit u)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleTerminated(battle, victorUnits, victorComs, u);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_computeAdvantage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(BattleArmy_computeAdvantage_TranspilerBody), new Type[] { typeof(BattleArmy), typeof(double) });

            bool hook = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!hook && instructionList[i].opcode == OpCodes.Ldloc_0 && instructionList[i + 1].opcode == OpCodes.Ldc_R8)
                {
                    hook = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                }

                yield return instructionList[i];
            }
        }

        private static void BattleArmy_computeAdvantage_TranspilerBody(BattleArmy battle, double advantage)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_ComputeAdvantage(battle, advantage);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_allocateDamage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_allocateDamage = AccessTools.Method(patchType, nameof(BattleArmy_allocateDamage_TranspilerBody_allocateDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]) });
            MethodInfo MI_TranspilerBody_receivesDamage = AccessTools.Method(patchType, nameof(BattleArmy_allocateDamage_TranspilerBody_receivesDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]), typeof(int) });

            bool hookOnUnitDamage = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (i == 1 && instructionList[i].opcode == OpCodes.Ldc_I4_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_allocateDamage);
                }

                if (!hookOnUnitDamage && instructionList[i].opcode == OpCodes.Ldarg_1 && instructionList[i-1].opcode == OpCodes.Nop)
                {
                    hookOnUnitDamage = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_receivesDamage);
                }

                yield return instructionList[i];
            }
        }

        private static void BattleArmy_allocateDamage_TranspilerBody_allocateDamage(BattleArmy battle, List<UM> units, int[] dmgs)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_AllocateDamage(battle, units, dmgs);
            }
        }

        private static void BattleArmy_allocateDamage_TranspilerBody_receivesDamage(BattleArmy battle, List<UM> units, int[] dmgs, int i)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                dmgs[i] = hook?.onUnitReceivesArmyBattleDamage(battle, units[i], dmgs[i]) ?? dmgs[i];
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    hook?.onRazeLocation_EndOfProcess(um);
                }
            }
        }

        private static IEnumerable<CodeInstruction> Task_RazeLocation_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Task_RazeLocation_turnTick_TranspilerBody), new Type[] { typeof(Unit) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Nop && instructionList[i+1].opcode == OpCodes.Ldarg_1 && instructionList[i + 2].opcode == OpCodes.Ldfld)
                    {
                        targetIndex = 0;

                        yield return new CodeInstruction(OpCodes.Nop);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                    }
                }

                yield return instructionList[i];
            }
        }

        private static void Task_RazeLocation_turnTick_TranspilerBody(Unit u)
        {
            razeIsValid = true;

            if (u is UM um)
            {
                //Console.WriteLine("CommunityLib: onRazeLocation_StartOfProcess");
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    hook?.onRazeLocation_StartOfProcess(um);
                }
            }
        }

        private static bool Settlement_FallIntoRuin_Prefix(Settlement __instance, out bool __state, string v, object killer = null)
        {
            bool result = true;

            //Console.WriteLine("CommunityLib: interceptSettlementFallIntoRuin");
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldarg_0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Intercept);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, end);
                    }
                    
                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Ret)
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

                yield return instructionList[i];
            }
        }

        private static bool Settlement_FallIntoRuin_TranspilerBody_Intercept(Settlement __instance, string v, object killer)
        {
            bool result = false;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook.onSettlementFallIntoRuin_StartOfProcess(__instance, v, killer);
            }

            return result;
        }

        private static void Settlement_FallIntoRuin_TranspilerBody_End(Settlement __instance, string v, object killer)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onSettlementFallIntoRuin_EndOfProcess(__instance, v, killer);
            }
        }

        private static void UIE_HolyTenet_bInfluence_Postfix(UIE_HolyTenet __instance)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onPlayerInfluenceTenet(__instance.tenet.order, __instance.tenet);
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

            int targetIndex = -5;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex != 0)
                {
                    if (targetIndex == -5 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_1 && instructionList[i - 2].opcode == OpCodes.Ldarg_1)
                    {
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ProcessIncome);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayBudget);
                        targetIndex = 1;
                        i++;
                    }

                    if (targetIndex == -4 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                    {
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ComputeInfluenceDark);
                        targetIndex++;
                        i++;
                    }

                    if (targetIndex == -3 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldloc_2)
                    {
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_ComputeInfluenceHuman);
                        targetIndex++;
                        i++;
                    }

                    if (targetIndex == -2 && instructionList[i].opcode == OpCodes.Ldc_I4_6 && instructionList[i-1].opcode == OpCodes.Ldfld && (instructionList[i-1].operand as FieldInfo) == FI_PopupHolyOrder_stats)
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayStats);
                        targetIndex = 2;
                    }

                    if (targetIndex == -1 && instructionList[i].opcode == OpCodes.Ldfld)
                    {
                        switch (instructionList[i].operand)
                        {
                            case FieldInfo fi when fi == FI_PopupHolyOrder_influenceDark:
                                //Console.WriteLine("CommunityLib: Found start of target " + targetIndex + " at line " + i + ".");
                                yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDark);
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder);
                                targetIndex = 3;
                                break;
                            case FieldInfo fi when fi == FI_PopupHolyOrder_influenceGood:
                                //Console.WriteLine("CommunityLib: Found start of target " + targetIndex + " at line " + i + ".");
                                yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGood);
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman);
                                targetIndex = 4;
                                break;
                            case FieldInfo fi when fi == FI_PopupHolyOrder_influenceDarkp0:
                                //Console.WriteLine("CommunityLib: Found start of target " + targetIndex + " at line " + i + ".");
                                yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDarkp0);
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder);
                                targetIndex = 5;
                                break;
                            case FieldInfo fi when fi == FI_PopupHolyOrder_influenceGoodp0:
                                //Console.WriteLine("CommunityLib: Found start of target " + targetIndex + " at line " + i + ".");
                                yield return new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGoodp0);
                                yield return new CodeInstruction(OpCodes.Ldarg_1);
                                yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman);
                                targetIndex = 6;
                                break;
                            default:
                                break;
                        }
                    }

                    if (targetIndex > 0)
                    {
                        if (targetIndex == 1)
                        {
                            if (instructionList[i].opcode == OpCodes.Ldfld && (instructionList[i].operand as FieldInfo) == FI_PopupHolyOrder_BudgetIncome)
                            {
                                i -= 3;
                                targetIndex = -4;
                            }
                        }
                        else if (targetIndex == 2)
                        {
                            if (instructionList[i].opcode == OpCodes.Callvirt)
                            {
                                targetIndex = -1;
                            }
                        }
                        else if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Call && instructionList[i - 2].opcode == OpCodes.Stelem_Ref)
                        {
                            //Console.WriteLine("CommunityLib: Found end of target " + targetIndex + " at line " + i + ".");
                            if (targetIndex == 6)
                            {
                                targetIndex = 0;
                            }
                            else
                            {
                                targetIndex = -1;
                            }
                        }
                    }

                    if (targetIndex < 1)
                    {
                        yield return instructionList[i];
                    }
                }
                else
                {
                    yield return instructionList[i];
                }
            }
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ProcessIncome(HolyOrder order, List<ReasonMsg> msgs)
        {
            if (AccessTools.DeclaredMethod(order.GetType(), "processIncome", new Type[] { typeof(List<ReasonMsg>) }) != null)
            {
                return (int)AccessTools.DeclaredMethod(order.GetType(), "processIncome", new Type[] { typeof(List<ReasonMsg>) }).Invoke(order, new object[] { msgs });
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onPopupHolyOrder_DisplayBudget(order, msgs);
            }

            Text text;
            Component[] comps = popupOrder.pages[popupOrder.currentPage].gameObject.GetComponentsInChildren(typeof(Text), false);

            foreach (Component comp in comps)
            {
                text = comp as Text;

                if (text != null && text.text.Contains("Income:"))
                {
                    budgetLabels = text;
                    break;
                }
            }

            if (budgetLabels != null)
            {
                budgetLabels.text = "";
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

                if (budgetLabels != null)
                {
                    budgetLabels.text = string.Concat(new string[] {
                        budgetLabels.text,
                        msg.msg,
                        ":",
                        "\n"
                    });
                }
            }

            return result;
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceDark(HolyOrder order,  List<ReasonMsg> msgs)
        {
            if (AccessTools.DeclaredMethod(order.GetType(), "computeInfluenceDark", new Type[] { typeof(List<ReasonMsg>) }) != null)
            {
                return (int)AccessTools.DeclaredMethod(order.GetType(), "computeInfluenceDark", new Type[] { typeof(List<ReasonMsg>) }).Invoke(order, new object[] { msgs });
            }

            return order.computeInfluenceDark(msgs);
        }

        private static int PopupHolyOrder_setTo_TranspilerBody_ComputeInfluenceHuman(HolyOrder order,  List<ReasonMsg> msgs)
        {
            if (AccessTools.DeclaredMethod(order.GetType(), "computeInfluenceHuman", new Type[] { typeof(List<ReasonMsg>) }) != null)
            {
                return (int)AccessTools.DeclaredMethod(order.GetType(), "computeInfluenceHuman", new Type[] { typeof(List<ReasonMsg>) }).Invoke(order, new object[] { msgs });
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                s = hook?.onPopupHolyOrder_DisplayInfluenceHuman(order, s, infGain);
            }

            return s;
        }

        private static List<Trait> UA_getStartingTraits_Postfix(List<Trait> traits, UA __instance)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    hook?.onAgentLevelup_GetTraits(ua, traits, false);
                }
            }
            return traits;
        }

        private static IEnumerable<CodeInstruction> Person_gainItem_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(Person_gainItem_TranspilerBody));

            FieldInfo FI_Person_Items = AccessTools.Field(typeof(Person), nameof(Person.items));

            Label falseLabel = ilg.DefineLabel();

            int targetIndex = 1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i - 1].opcode == OpCodes.Ldloc_S)
                    {
                        targetIndex++;

                        if (targetIndex == 3)
                        {
                            targetIndex = 0;
                            falseLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Dup);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_Person_Items);
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                            yield return new CodeInstruction(OpCodes.Ldelem_Ref);
                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Brfalse_S, falseLabel);
                        }
                    }
                }

                yield return instructionList[i];
            }
        }

        private static bool Person_gainItem_TranspilerBody(Person person, Item item, Item newItem, bool obligateHold)
        {
            bool result = true;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook?.interceptReplaceItem(person, item, newItem, obligateHold) ?? false;

                if (retValue)
                {
                    result = false;
                }
            }

            return result;
        }

        private static IEnumerable<CodeInstruction> SG_ActionTakingMonster_turnTick_TranspilerWrapper(IEnumerable<CodeInstruction> codeInstructions)
        {
            //Console.WriteLine("CommunityLib: Transpiler Wrapper");

            foreach (CodeInstruction instruction in SG_ActionTakingMonster_turnTick_Transpiler(codeInstructions))
            {
                //Console.WriteLine(instruction);
                yield return instruction;
            }
        }

        private static IEnumerable<CodeInstruction> SG_ActionTakingMonster_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_populate = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_populate), new Type[] { typeof(SG_ActionTakingMonster), typeof(List<MonsterAction>) });
            MethodInfo MI_TranspilerBody_getUtility = AccessTools.Method(patchType, nameof(SG_ActionTakingMonster_turnTick_TranspilerBody_getUtility), new Type[] { typeof(SG_ActionTakingMonster), typeof(MonsterAction), typeof(double), typeof(List<ReasonMsg>) });

            MethodInfo MI_SG_ActionTakingMonster_getActions = AccessTools.Method(typeof(SG_ActionTakingMonster), nameof(SG_ActionTakingMonster.getActions), new Type[0]);

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldarg_0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_SG_ActionTakingMonster_getActions);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_populate);

                        i++;
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Callvirt && instructionList[i - 1].opcode == OpCodes.Ldarg_0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_SG_ActionTakingMonster_getActions);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_populate);

                        i++;
                    }

                    if (targetIndex == 3 && instructionList[i].opcode == OpCodes.Ldloc_S && instructionList[i-1].opcode == OpCodes.Stloc_S && instructionList[i-2].opcode == OpCodes.Callvirt)
                    {
                        targetIndex = 0;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 8);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 9);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_getUtility);
                        yield return new CodeInstruction(OpCodes.Stloc_S, 9);
                    }
                }
                
                yield return instructionList[i];
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
        }

        private static List<MonsterAction> SG_ActionTakingMonster_turnTick_TranspilerBody_populate(SG_ActionTakingMonster monster, List<MonsterAction> actions)
        {
            foreach(Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.populatingMonsterActions(monster, actions);
            }

            return actions;
        }

        private static double SG_ActionTakingMonster_turnTick_TranspilerBody_getUtility(SG_ActionTakingMonster monster, MonsterAction action, double utility, List<ReasonMsg> reasonMsgs)
        {
            double result = utility;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                result = hook?.onActionTakingMonster_getUtility(monster, action, utility, reasonMsgs) ?? result;
            }

            return result;
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

            int targetIndex = 0;

            for (int i = 0; i < instructionList.Count; i++)
            {

                if (targetIndex == 0 && i > 0 && instructionList[i - 1].opcode == OpCodes.Brfalse_S && instructionList[i].opcode == OpCodes.Nop)
                {
                    targetIndex = 1;
                    continueLabel = (Label)instructionList[i - 1].operand;

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Callvirt, MI_HolyOrder_isGone);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, continueLabel);
                }

                if (targetIndex == 2 && instructionList[i - 1].opcode == OpCodes.Stloc_1 && instructionList[i].opcode == OpCodes.Ldarg_0)
                {
                    targetIndex = 3;
                    instructionList[i].labels.Add(isDeadLabel);
                }

                if (targetIndex == 1 && instructionList[i - 1].opcode == OpCodes.Ldloc_0 && instructionList[i].opcode == OpCodes.Ldarg_0)
                {
                    targetIndex = 2;
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

                yield return instructionList[i];
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
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_influenceElder)
                    {
                        targetIndex++;
                    }

                    if (targetIndex == 3 && instructionList[i].opcode == OpCodes.Ldloc_S)
                    {
                        targetIndex = 0;

                        yield return new CodeInstruction(OpCodes.Ldloc_S, 46);
                        yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_HolyOrderGone);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, instructionList[i+1].operand);
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Ldloc_S)
                    {
                        targetIndex++;
                    }
                }

                yield return instructionList[i];
            }
        }

        private static IEnumerable<CodeInstruction> DivineEntity_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_HolyOrderGone = AccessTools.Method(patchType, nameof(TranspilerBody_HolyOrder_isGone));

            FieldInfo FI_Order = AccessTools.Field(typeof(DivineEntity), nameof(DivineEntity.order));

            Label label = ilg.DefineLabel();

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldloc_1)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, FI_Order);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_HolyOrderGone);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, label);
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Nop)
                    {
                        targetIndex = 0;

                        instructionList[i].labels.Add(label);
                    }
                }

                yield return instructionList[i];
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
                    if (targetIndex == 1 && i == 0)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Nop);
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldarg_2);
                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Intercept);
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Cgt_Un);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, end);
                        yield return new CodeInstruction(OpCodes.Pop);
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Call && instructionList[i+1].opcode == OpCodes.Callvirt)
                    {
                        targetIndex = 0;
                        i++;

                        yield return new CodeInstruction(OpCodes.Ldarg_3);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_EntranceWonder);
                    }
                }

            
                yield return instructionList[i];
            }
        }

        private static Location[] Map_getPathTo_Location_TranspilerBody_Intercept(Location locA, Location locB, Unit u, bool safeMove)
        {
            Location[] path = null;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                Location[] newPath = hook?.interceptGetPathTo_Location(locA, locB, u, safeMove) ?? null;

                if (newPath != null)
                {
                    path = newPath;
                }
            }

            return path;
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
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Ldnull);
                        yield return new CodeInstruction(OpCodes.Cgt_Un);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, end);
                        yield return new CodeInstruction(OpCodes.Pop);
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
        }

        private static Location[] Map_getPathTo_SocialGroup_TranspilerBody_Intercept(Location loc, SocialGroup sg, Unit u, bool safeMove)
        {
            Location[] path = null;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                Location[] newPath = hook?.interceptGetPathTo_SocialGroup(loc, sg, u, safeMove) ?? null;

                if (newPath != null)
                {
                    path = newPath;
                }
            }

            return path;
        }

        private static List<Location> Map_getPathTo_TranspilerBody_EntranceWonder(Location loc, Unit u)
        {
            List<Location> result = loc.getNeighbours();

            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (loc.settlement is Set_MinorOther && loc.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    Location tomb = u.map.locations.FirstOrDefault(l => l.settlement is Set_TombOfGods);
                    if (tomb != null)
                    {
                        result.Add(tomb);
                    }

                    if (u.homeLocation != -1)
                    {
                        result.Add(u.map.locations[u.homeLocation]);
                    }
                }
            }

            return result;
        }

        private static void Map_adjacentMoveTo_Prefix(Map __instance, Unit u, Location loc)
        {
            bool theEntrance = false;
            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (u.location.settlement is Set_MinorOther && u.location.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    Location tomb = __instance.locations.FirstOrDefault(l => l.settlement is Set_TombOfGods);
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

                foreach(Unit unit in __instance.units)
                {
                    if ((unit.task is Task_AttackUnit attack && attack.target == u) || (unit.task is Task_DisruptUA disrupt && disrupt.other == u) || (unit.task is Task_AttackUnitWithEscort attackEscort && attackEscort.target == u))
                    {
                        unit.task = null;
                    }
                }
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
                    if (instructionList[i].opcode == OpCodes.Ldnull)
                    {
                        targetIndex = 0;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        i++;
                    }
                }

                yield return instructionList[i];
            }
        }

        // Orc Expansion modifications
        private static IEnumerable<CodeInstruction> SG_Orc_canSettle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(SG_Orc_canSettle_TranspilerBody));

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static bool SG_Orc_canSettle_TranspilerBody(Location location)
        {
            if (location.isOcean || location.soc != null || location.hex.getHabilitability() < location.map.opt_orcHabMult * location.map.param.orc_habRequirement)
            {
                return false;
            }
            if (location.settlement != null)
            {
                if (ModCore.core.getSettlementTypesForOrcExpanion().TryGetValue(location.settlement.GetType(), out List<Type> subsettlementBlacklist))
                {
                    if (subsettlementBlacklist?.Count > 0)
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

            if (ua.location.settlement != null)
            {
                //Console.WriteLine("CommunityLib: Testing Claim Territory against Permitted Settlements");
                if (ModCore.core.getSettlementTypesForOrcExpanion().TryGetValue(ua.location.settlement.GetType(), out List<Type> subsettlementBlacklist))
                {
                    if (subsettlementBlacklist!= null && subsettlementBlacklist.Count > 0)
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
            else if (ua.location.getNeighbours().Any(l => l.soc == orcSociety))
            {
                //Console.WriteLine("CommunityLib: Location neighbours orc society");
                return true;
            }
            else if (ua.location.isCoastal)
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
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                        i += 4;
                    }
                }

                yield return instructionList[i];
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
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_3)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                        i += 4;
                    }
                }

                yield return instructionList[i];
            }
        }

        private static IEnumerable<CodeInstruction> Overmind_Automatic_checkSpawnAgent_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_checkIsElderTomb = AccessTools.Method(patchType, nameof(checkIsElderTomb_TranspilerBody), new Type[] { typeof(Location) });

            int targetIndex = 1;
            bool returnLines = true;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Ldfld && instructionList[i - 1].opcode == OpCodes.Ldloc_S && instructionList[i + 1].opcode == OpCodes.Isinst)
                    {
                        targetIndex++;

                        yield return new CodeInstruction(OpCodes.Call, MI_checkIsElderTomb);

                        returnLines = false;
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Stloc_S)
                    {
                        targetIndex = 0;
                        returnLines = true;
                    }
                }

                if (returnLines)
                {
                    yield return instructionList[i];
                }
            }
        }

        private static bool checkIsElderTomb_TranspilerBody(Location location)
        {
            return ModCore.core.checkIsElderTomb(location);
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
                            if (instructionList[j].opcode == OpCodes.Nop && instructionList[j-1].opcode == OpCodes.Nop && instructionList[j-2].opcode == OpCodes.Nop)
                            {
                                instructionList[j].labels.Add(nextUnit);
                                break;
                            }
                        }
                    }

                    if (targetIndex == 2 && instructionList[i].opcode == OpCodes.Brfalse && instructionList[i-1].opcode == OpCodes.Ldloc_S && instructionList[i+1].opcode == OpCodes.Nop)
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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


        // Graphical Hex Hooks
        /*private static IEnumerable<CodeInstruction> GraphicalHex_checkData_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(GraphicalHex_checkData_TranspilerBody), new Type[] { typeof(GraphicalHex) });

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1 && instructionList[i].opcode == OpCodes.Brfalse_S && instructionList[i-1].opcode == OpCodes.Ldsfld && instructionList[i-2].opcode == OpCodes.Brfalse_S)
                    {
                        targetIndex = 0;

                        Label skip = (Label)instructionList[i].operand;

                        yield return new CodeInstruction(OpCodes.Brfalse_S, skip);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                    }
                }

                yield return instructionList[i];
            }
        }

        private static bool GraphicalHex_checkData_TranspilerBody(GraphicalHex hex)
        {
            bool result = true;
            List<Property> priorityProperties = new List<Property>();

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook.onGraphicalHexUpdate_interceptDisplayPropertyOverlay(hex.hex.location, out List<Property> newPriorityProperties);

                if (retValue)
                {
                    result = false;
                }

                if (result)
                {
                    if (newPriorityProperties != null)
                    {
                        foreach (Property priorityProperty in newPriorityProperties)
                        {
                            if (priorityProperty.hasHexView() && priorityProperty.charge >= 45.0)
                            {
                                priorityProperties.Add(priorityProperty);
                            }
                        }
                    }
                }
            }

            if (result && priorityProperties.Count > 0)
            {
                Property priorityProperty = null;
                double charge = 0.0;

                foreach (Property property in priorityProperties)
                {
                    if (property.charge > charge)
                    {
                        priorityProperty = property;
                        charge = property.charge;
                    }
                }

                if (priorityProperty != null)
                {
                    result = false;
                    hex.cloudLayer.sprite = priorityProperty.hexViewSprite();
                    hex.cloudLayer.enabled = true;
                }
            }

            return result;
        }*/

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
                    if (targetIndex == 1 && i > 5)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Callvirt && instructionList[i + 1].opcode == OpCodes.Ldloc_0)
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

                    if (targetIndex > 2 && targetIndex < 6)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && (MethodInfo)instructionList[i].operand == MI_Unit_isCommandable)
                        {
                            targetIndex++;
                        }

                        if (targetIndex == 4 && instructionList[i].opcode == OpCodes.Ldloc_S)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_TranspilerBody_UM);
                        }

                        targetIndex = 0;
                    }
                }

                yield return instructionList[i];
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

            if (ModCore.core.GetAgentAI().TryGetAgentType(ua.GetType(), out AgentAI.AIData? aiData) && aiData is AgentAI.AIData data)
            {
                if (data.controlParameters.hideThoughts)
                {
                    return true;
                }

                List<UIScroll_Unit.SortableTaskBlock> blocks = new List<UIScroll_Unit.SortableTaskBlock>();
                //Console.WriteLine("CommunityLib: Got valid challenges and rituals");
                foreach (AgentAI.ChallengeData challengeData in ModCore.core.GetAgentAI().getAllValidChallengesAndRituals(ua))
                {
                    //Console.WriteLine("CommunityLib: Iterating " + challengeData.challenge.getName());
                    SortableTaskBlock_Advanced block = new SortableTaskBlock_Advanced();
                    block.challenge = challengeData.challenge;
                    block.utility = ModCore.core.GetAgentAI().getChallengeUtility(challengeData, ua, data, data.controlParameters, block.msgs);
                    block.challengeData = challengeData;
                    blocks.Add(block);

                    if (challengeData.challenge is Ritual)
                    {
                        block.location = challengeData.location;
                    }
                    //Console.WriteLine("CommunityLib: Added " + challengeData.challenge.getName());
                }
                foreach (AgentAI.TaskData taskData in ModCore.core.GetAgentAI().getAllValidTasks(ua))
                {
                    SortableTaskBlock_Advanced blockTask = new SortableTaskBlock_Advanced();
                    blockTask.challenge = null;
                    blockTask.taskType = taskData.aiTask.taskType;
                    blockTask.utility = ModCore.core.GetAgentAI().checkTaskUtility(taskData, ua, data, data.controlParameters, blockTask.msgs);
                    blockTask.taskData = taskData;

                    switch(taskData.targetCategory)
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
                            blockAttack.utility = ua.getAttackUtility(unit, blockAttack.msgs, data.controlParameters.includeDangerousFoe);
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

            UM um = GraphicalMap.selectedUnit as UM;

            List<Hooks.TaskData> data = new List<Hooks.TaskData>();

            if (um != null && um.isCommandable())
            {
                foreach(Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    List<Hooks.TaskData> retData = hook?.onUIScroll_Unit_populateUM(um);

                    if (retData?.Count > 0)
                    {
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

            Label interceptUA = ilg.DefineLabel();
            Label interceptUM = ilg.DefineLabel();

            int targetIndex = 1;
            int popoutCounter = 0;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 1)
                {
                    if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldfld && (FieldInfo)instructionList[i].operand == FI_UIScorll_challengePopout)
                        {
                            popoutCounter++;
                        }

                        if (popoutCounter == 4)
                        {
                            targetIndex++;
                            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Popout);
                            yield return new CodeInstruction(OpCodes.Brtrue_S, interceptUM);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                        }
                    }

                    if (targetIndex == 3)
                    {
                        if (instructionList[i].opcode == OpCodes.Br)
                        {
                            targetIndex = 0;
                            instructionList[i].labels.Add(interceptUM);
                        }
                    }
                }

                yield return instructionList[i];

                if (targetIndex == 1)
                {
                    if (instructionList[i].opcode == OpCodes.Brfalse && instructionList[i - 1].opcode == OpCodes.Ldloc_S)
                    {
                        if (instructionList[i + 3].opcode == OpCodes.Ldfld && instructionList[i + 3].operand as FieldInfo == FI_UIScroll_Unit_Master)
                        {
                            if (instructionList[i + 4].opcode == OpCodes.Ldfld && instructionList[i + 4].operand as FieldInfo == FI_UIMaster_World)
                            {
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_TimeStats);
                                yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i].operand);
                                targetIndex++;
                            }
                        }
                    }
                }
            }
        }

        private static bool UIScroll_Unit_Update_TranspilerBody_TimeStats()
        {
            if (GraphicalMap.selectedUnit is UA ua && ModCore.core.GetAgentAI().TryGetAgentType(ua.GetType(), out AgentAI.AIData? aiData) && aiData is AgentAI.AIData data)
            {
                if (!data.controlParameters.valueTimeCost)
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
        private static bool Unit_isCommandable_Postfix(bool __result, Unit __instance)
        {
            if (__instance is UA ua)
            {
                return ua.corrupted;
            }

            return __result;
        }

        // UAEN OVERRIDE AI //
        // Negate unit interactions.
        private static bool UAEN_UnitInteraction_Prefix(UA __instance, ref double __result)
        {
            switch (__instance)
            {
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
            return true;
        }

        private static bool UA_getVisibleUnits_Prefix(UA __instance, ref List<Unit> __result, out bool __state)
        {
            bool result = true;
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
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
                foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
                {
                    hook.getVisibleUnits_EndOfProcess(__instance, visibleUnits);
                }
            }

            return visibleUnits;
        }

        private static bool UAEN_DeepOne_turnTickAI_Prefix(UAEN_DeepOne __instance)
        {
            if (ModCore.core.GetAgentAI().ContainsAgentType(typeof(UAEN_DeepOne)))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_Ghast_turnTickAI_Prefix(UAEN_Ghast __instance)
        {
            if (ModCore.core.GetAgentAI().ContainsAgentType(typeof(UAEN_Ghast)))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_OrcUpstart_turnTickAI_Prefix(UAEN_OrcUpstart __instance)
        {
            if (ModCore.core.GetAgentAI().ContainsAgentType(typeof(UAEN_OrcUpstart)))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_Vampire_turnTickAI_Prefix(UAEN_Vampire __instance)
        {
            if (ModCore.core.GetAgentAI().ContainsAgentType(typeof(UAEN_Vampire)))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UA_turnTickAI_Prefix(UA __instance)
        {
            if (__instance is UAA && ModCore.core.GetAgentAI().ContainsAgentType(typeof(UAA)))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static void Ch_Rest_InOrcCamp_complete_Postfix(UA u)
        {
            u.challengesSinceRest = 0;
        }

        private static void Template_TranspilerBody()
        {
            foreach(Hooks hook in ModCore.core.GetRegisteredHooks())
            {

            }
        }
    }
}
