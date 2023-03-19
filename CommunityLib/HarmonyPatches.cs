using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
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

        private static bool patched = false;

        public static ArmyBattleData armyBattleData_StartOfCycle;

        public static Text budgetLabels = null;

        /// <summary>
        /// Initialises variables in this class that are required to perform patches, then executes harmony patches.
        /// </summary>
        /// <param name="core"></param>
        public static void PatchingInit()
        {
            if (patched)
            {
                return;
            }
            else
            {
                patched = true;
            }

            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("ILikeGoodFood.SOFG.CommunityLib");

            if (Harmony.HasAnyPatches(harmony.Id))
            {
                return;
            }

            // FIXES //
            // Assign Killer to Miscellaneous causes of death
            harmony.Patch(original: AccessTools.Method(typeof(UM_HumanArmy), nameof(UM_HumanArmy.turnTickInner)), transpiler: new HarmonyMethod(patchType, nameof(UM_HumanArmy_turnTickInner_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.skirmishDanger)), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishAttacking_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.skirmishDanger)), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishDefending_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Mg_Volcano), nameof(Mg_Volcano.complete), new Type[] { typeof(UA) }), transpiler: new HarmonyMethod(patchType, nameof(Mg_Volcano_Complete_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(God_Snake), nameof(God_Snake.awaken)), transpiler: new HarmonyMethod(patchType, nameof(God_Snake_Awaken_Transpiler)));

            // HOOKS //
            // Unit death hooks
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.die)), transpiler: new HarmonyMethod(patchType, nameof(Unit_die_Transpiler)));
            // Army Battle hooks
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.cycle)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_cycle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.unitMovesFromLocation)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_unitMovesFromLocation_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.computeAdvantage)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_computeAdvantage_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), "allocateDamage", new Type[] { typeof(List<UM>), typeof(int[]) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_allocateDamage_Transpiler)));
            // Settlement destruction hooks
            harmony.Patch(original: AccessTools.Method(typeof(Settlement), nameof(Settlement.fallIntoRuin), new Type[] { typeof(string), typeof(object) }), transpiler: new HarmonyMethod(patchType, nameof(Settlement_FallIntoRuin_Transpiler)));
            // Religion UI Screen modification
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bPrev)), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bNext)), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            // Religion UI Screen Hooks
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.setTo), new Type[] { typeof(HolyOrder), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_setTo_Transpiler)));
            // LevelUp Traits Hook
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getStartingTraits)), postfix: new HarmonyMethod(patchType, nameof(UA_getStartingTraits_Postfix)));
            harmony.Patch(original: AccessTools.Method(typeof(Trait), nameof(Trait.getAvailableTraits), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Trait_getAvailableTraits_Postfix)));

            // AGENT AI //
            // UIScroll_Unit (Challenge utility panel)
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.checkData), new Type[] { }), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_checkData_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(UIScroll_Unit), nameof(UIScroll_Unit.Update), new Type[] { }), transpiler: new HarmonyMethod(patchType, nameof(UIScroll_Unit_Update_Transpiler)));

            // UAEN OVERRIDE AI //
            // Negate unit interactions.
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getAttackUtility)), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getBodyguardUtility)), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getDisruptUtility)), prefix: new HarmonyMethod(patchType, nameof(UAEN_UnitInteraction_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UA), nameof(UA.getVisibleUnits)), prefix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Prefix)), postfix: new HarmonyMethod(patchType, nameof(UA_getVisibleUnits_Postfix)));
            // Override AI
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_DeepOne), nameof(UAEN_DeepOne.turnTickAI), new Type[] {  }), prefix: new HarmonyMethod(patchType, nameof(UAEN_DeepOne_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Ghast), nameof(UAEN_Ghast.turnTickAI), new Type[] {  }), prefix: new HarmonyMethod(patchType, nameof(UAEN_Ghast_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_OrcUpstart), nameof(UAEN_OrcUpstart.turnTickAI), new Type[] {  }), prefix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_turnTickAI_Prefix)));
            harmony.Patch(original: AccessTools.Method(typeof(UAEN_Vampire), nameof(UAEN_Vampire.turnTickAI), new Type[] {  }), prefix: new HarmonyMethod(patchType, nameof(UAEN_OrcUpstart_turnTickAI_Prefix)));
            // Ch_Rest_InOrcCamp
            harmony.Patch(original: AccessTools.Method(typeof(Ch_Rest_InOrcCamp), nameof(Ch_Rest_InOrcCamp.complete), new Type[] { typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(Ch_Rest_InOrcCamp_complete_Postfix)));
            // Rt_DeepOneReproduce
            harmony.Patch(original: AccessTools.Constructor(typeof(Rt_DeepOneReproduce), new Type[] { typeof(Location), typeof(UA) }), postfix: new HarmonyMethod(patchType, nameof(ctor_Rt_DeepOneReproduce_Postfix)));

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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook?.interceptUnitDeath(u, v, killer) ?? false;

                if (retValue)
                {
                    result = true;
                }
            }

            return result;
        }

        private static void Unit_die_TranspilerBody_StartOfProcess(Unit u, string v, Person killer = null)
        {
            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onUnitDeath_StartOfProcess(u, v, killer);
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

            FieldInfo FI_BattleArmy_Done = AccessTools.Field(typeof(BattleArmy), nameof(BattleArmy.done));

            Label retLabel = instructionList[instructionList.Count - 1].labels[0];

            bool dataGathered = false;
            bool hooksIntecerptAndStartOfProcess = false;
            bool hookVictory = false;
            int hookVictoryCount = 0;

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

            HarmonyPatches.armyBattleData_StartOfCycle = data;
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
            ArmyBattleData data = HarmonyPatches.armyBattleData_StartOfCycle;
            Tuple<List<UM>, List<UA>> victors;
            List<UM> victorUnits = new List<UM>();
            List<UA> victorComs = new List<UA>();
            List<UM> defeatedUnits = new List<UM>();
            List<UA> defeatedComs= new List<UA>();

            if (battle.attackers.Count == 0 && battle.defenders.Count > 0)
            {
                victors = armyBattleData_StartOfCycle.GetDefenders();
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
            else if (battle.defenders.Count == 0 && battle.attackers.Count > 0)
            {
                victors = armyBattleData_StartOfCycle.GetAttackers();
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

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                hook?.onArmyBattleVictory(battle, victorUnits, victorComs, defeatedUnits, defeatedComs);
            }

            armyBattleData_StartOfCycle.Clear();
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

        // Settlement Hooks
        private static IEnumerable<CodeInstruction> Settlement_FallIntoRuin_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_Intercept = AccessTools.Method(patchType, nameof(Settlement_FallIntoRuin_TranspilerBody_Intercept));
            MethodInfo MI_TranspilerBody_End = AccessTools.Method(patchType, nameof(Settlement_FallIntoRuin_TranspilerBody_End));

            int targetIndex = -2;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex == -2 && instructionList[i].opcode == OpCodes.Ldarg_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_Intercept);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, instructionList[instructionList.Count-1].labels[0]);
                    targetIndex++;
                }
                else if (targetIndex == -1 && instructionList[i].opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody_End);
                }

                yield return instructionList[i];
            }
        }

        private static bool Settlement_FallIntoRuin_TranspilerBody_Intercept(Settlement __instance, string v, object killer)
        {
            bool result = false;

            foreach (Hooks hook in ModCore.core.GetRegisteredHooks())
            {
                bool retValue = hook?.interceptSettlementFallIntoRuin(__instance, v, killer) ?? false;

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
                hook?.onSettlementFallIntoRuin_StartOfProcess(__instance, v, killer);
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

        // Religion UI Screen modification
        private static IEnumerable<CodeInstruction> PopupHolyOrder_bPrevNext_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_HolyOrder_isGone = AccessTools.Method(typeof(HolyOrder), nameof(HolyOrder.isGone));

            Label isFalseLabel;

            bool found = false;

            for (int i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];

                if (!found && instructionList[i].opcode == OpCodes.Brfalse_S)
                {
                    found = true;
                    isFalseLabel = (Label)instructionList[i].operand;

                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Callvirt, MI_HolyOrder_isGone);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, isFalseLabel);
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

        private static IEnumerable<CodeInstruction> UIScroll_Unit_checkData_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UI_Scroll_Unit_checkData_TranspilerBody));

            Label noAI = ilg.DefineLabel();
            Label skip = ilg.DefineLabel();

            bool found = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!found && i > 5)
                {
                    if (instructionList[i].opcode == OpCodes.Nop && instructionList[i - 1].opcode == OpCodes.Callvirt && instructionList[i + 1].opcode == OpCodes.Ldloc_0)
                    {
                        for (int j = i; j < instructionList.Count; j++)
                        {
                            if (instructionList[j].opcode == OpCodes.Brfalse)
                            {
                                skip = (Label)instructionList[j].operand;
                                break;
                            }
                        }

                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                        yield return new CodeInstruction(OpCodes.Brtrue_S, skip);
                        yield return new CodeInstruction(OpCodes.Nop, noAI);
                        found = true;
                    }
                }

                yield return instructionList[i];
            }
        }

        private static bool UI_Scroll_Unit_checkData_TranspilerBody(UIScroll_Unit ui)
        {
            UA ua = GraphicalMap.selectedUnit as UA;
            //Console.WriteLine("CommunityLib: Got unit");
            
            if (ua == null)
            {
                //Console.WriteLine("CommunityLib: Unit is not UA");
                return false;
            }

            if (ModCore.core.GetAgentAI().TryGetAgentType(ua.GetType(), out List<AIChallenge> _, out AgentAI.ControlParameters? control))
            {
                //Console.WriteLine("CommunityLib: Got registered AI");
                if (control == null)
                {
                    //Console.WriteLine("CommunityLib: cotnrol is null");
                    return false;
                }
                AgentAI.ControlParameters controlParams = (AgentAI.ControlParameters)control;

                List<UIScroll_Unit.SortableTaskBlock> blocks = new List<UIScroll_Unit.SortableTaskBlock>();
                //Console.WriteLine("CommunityLib: Got valid challenges and rituals");
                foreach (AgentAI.ChallengeData challengeData in ModCore.core.GetAgentAI().getAllValidChallengesAndRituals(ua))
                {
                    //Console.WriteLine("CommunityLib: Iterating " + challengeData.challenge.getName());
                    UIScroll_Unit.SortableTaskBlock block = new UIScroll_Unit.SortableTaskBlock();
                    block.challenge = challengeData.challenge;
                    block.utility = ModCore.core.GetAgentAI().getChallengeUtility(challengeData, ua, controlParams, block.msgs);
                    blocks.Add(block);
                    //Console.WriteLine("CommunityLib: Added " + challengeData.challenge.getName());
                }
                List<Unit> visibleUnits = ua.getVisibleUnits();
                if (visibleUnits?.Count > 0)
                {
                    foreach (Unit unit in visibleUnits)
                    {
                        //Console.WriteLine("CommunityLib: Iterating " + unit.getName());
                        if (unit is UA agent)
                        {
                            //Console.WriteLine("CommunityLib: Unit is UA");
                            UIScroll_Unit.SortableTaskBlock blockAttack = new UIScroll_Unit.SortableTaskBlock();
                            blockAttack.unitToAttack = unit;
                            blockAttack.utility = ua.getAttackUtility(unit, blockAttack.msgs, controlParams.includeDangerousFoe);
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
                    num--;
                }
            }

            return false;
        }

        private static IEnumerable<CodeInstruction> UIScroll_Unit_Update_Transpiler(IEnumerable<CodeInstruction> codeInstructions, ILGenerator ilg)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(UIScroll_Unit_Update_TranspilerBody));
            FieldInfo FI_UIScroll_Unit_Master = AccessTools.Field(typeof(UIScroll_Unit), nameof(UIScroll_Unit.master));
            FieldInfo FI_UIMaster_World = AccessTools.Field(typeof(UIMaster), nameof(UIMaster.world));

            bool found = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];

                if (!found)
                {
                    if (instructionList[i].opcode == OpCodes.Brfalse && instructionList[i - 1].opcode == OpCodes.Ldloc_S)
                    {
                        if (instructionList[i + 3].opcode == OpCodes.Ldfld && instructionList[i + 3].operand as FieldInfo == FI_UIScroll_Unit_Master)
                        {
                            if (instructionList[i + 4].opcode == OpCodes.Ldfld && instructionList[i + 4].operand as FieldInfo == FI_UIMaster_World)
                            {
                                yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                                yield return new CodeInstruction(OpCodes.Brtrue, instructionList[i].operand);
                                found = true;
                            }
                        }
                    }
                }
            }
        }

        private static bool UIScroll_Unit_Update_TranspilerBody()
        {
            if (GraphicalMap.selectedUnit is UA ua && ModCore.core.GetAgentAI().TryGetAgentType(ua.GetType(), out List<AIChallenge> _, out AgentAI.ControlParameters? control))
            {
                if (control == null)
                {
                    return false;
                }
                AgentAI.ControlParameters controlParams = (AgentAI.ControlParameters)control;

                if (!controlParams.valueTimeCost)
                {
                    return true;
                }
            }

            return false;
        }

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
            if (ModCore.core.GetAgentAI().TryGetAgentType(typeof(UAEN_DeepOne)))
            {
                ModCore.core.GetAgentAI().onTurnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_Ghast_turnTickAI_Prefix(UAEN_Ghast __instance)
        {
            if (ModCore.core.GetAgentAI().TryGetAgentType(typeof(UAEN_Ghast)))
            {
                ModCore.core.GetAgentAI().onTurnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_OrcUpstart_turnTickAI_Prefix(UAEN_OrcUpstart __instance)
        {
            if (ModCore.core.GetAgentAI().TryGetAgentType(typeof(UAEN_OrcUpstart)))
            {
                ModCore.core.GetAgentAI().onTurnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static bool UAEN_Vampire_turnTickAI_Prefix(UAEN_Vampire __instance)
        {
            if (ModCore.core.GetAgentAI().TryGetAgentType(typeof(UAEN_Vampire)))
            {
                ModCore.core.GetAgentAI().onTurnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static void Ch_Rest_InOrcCamp_complete_Postfix(UA u)
        {
            u.challengesSinceRest = 0;
        }

        private static void ctor_Rt_DeepOneReproduce_Postfix(Rt_DeepOneReproduce __instance)
        {
            __instance.getPositiveTags().AddItem(Tags.DEEPONES);
        }

        private static void Template_TranspilerBody()
        {
            foreach(Hooks hook in ModCore.core.GetRegisteredHooks())
            {

            }
        }
    }
}
