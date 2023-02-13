using Assets.Code;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.UI;

namespace CommunityLib
{
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);

        private static ModCore mod;

        private static bool patched = false;

        public struct ArmyBattleData
        {
            public List<UM> attackers;
            public List<UA> attComs;
            public List<UM> defenders;
            public List<UA> defComs;

            public void Clear()
            {
                attackers.Clear();
                attComs.Clear();
                defenders.Clear();
                defComs.Clear();
            }
        }

        public static ArmyBattleData armyBattleData_StartOfCycle;

        /// <summary>
        /// Initialises variables in this class that are required to perform patches, then executes harmony patches.
        /// </summary>
        /// <param name="core"></param>
        public static void PatchingInit(ModCore core)
        {
            if (patched)
            {
                return;
            }
            else
            {
                patched = true;
            }

            mod = core;

            Patching();
        }

        private static void Patching()
        {
            Harmony.DEBUG = true;
            Harmony harmony = new Harmony("ILikeGoodFood.SOFG.CommunityLib");

            // Assign Killer to Miscellaneous causes of death
            harmony.Patch(original: AccessTools.Method(typeof(UM_HumanArmy), nameof(UM_HumanArmy.turnTickInner)), transpiler: new HarmonyMethod(patchType, nameof(UM_HumanArmy_turnTickInner_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishAttacking), nameof(Ch_SkirmishAttacking.skirmishDanger)), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishAttacking_skirmishDanger_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(Ch_SkirmishDefending), nameof(Ch_SkirmishDefending.skirmishDanger)), transpiler: new HarmonyMethod(patchType, nameof(Ch_SkirmishDefending_skirmishDanger_Transpiler)));
            // Unit death hooks
            harmony.Patch(original: AccessTools.Method(typeof(Unit), nameof(Unit.die)), transpiler: new HarmonyMethod(patchType, nameof(Unit_die_Transpiler)));
            // Army Battle hooks
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.cycle)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_cycle_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.unitMovesFromLocation)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_unitMovesFromLocation_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), nameof(BattleArmy.computeAdvantage)), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_computeAdvantage_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(BattleArmy), "allocateDamage", new Type[] { typeof(List<UM>), typeof(int[]) }), transpiler: new HarmonyMethod(patchType, nameof(BattleArmy_allocateDamage_Transpiler)));
            // Religion UI Screen modification
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bPrev)), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.bNext)), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_bPrevNext_Transpiler)));
            // Religion UI Screen Hooks
            harmony.Patch(original: AccessTools.Method(typeof(PopupHolyOrder), nameof(PopupHolyOrder.setTo), new Type[] { typeof(HolyOrder), typeof(int) }), transpiler: new HarmonyMethod(patchType, nameof(PopupHolyOrder_setTo_Transpiler)));
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

        // Unit death hooks
        private static IEnumerable<CodeInstruction> Unit_die_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_InterceptUnitDeath = AccessTools.Method(typeof(HarmonyPatches), nameof(Unit_die_TranspilerBody_InterceptUnitDeath), new Type[] { typeof(Unit), typeof(string), typeof(Person) });
            MethodInfo MI_TranspilerBody_StartOfProcess = AccessTools.Method(typeof(HarmonyPatches), nameof(Unit_die_TranspilerBody_StartOfProcess), new Type[] { typeof(Unit), typeof(string), typeof(Person) });

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

            foreach (Hooks hook in mod.GetRegisteredHooks())
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
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onUnitDeath_StartOfProcess(u, v, killer);
            }
        }

        // Army Battle hooks
        private static IEnumerable<CodeInstruction> BattleArmy_cycle_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            // Transpiler Bodies for Data management.
            MethodInfo MI_TranspilerBody_GatherData = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_cycle_TranspilerBody_GatherData), new Type[] { typeof(BattleArmy) });

            // Transpiler Bodies for Hooks
            MethodInfo MI_TranspilerBody_InterceptCycle = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_cycle_TranspilerBody_InterceptCycle), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_StartOfProcess = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_cycle_TranspilerBody_StartOfProcess), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_EndOfProcess = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_cycle_TranspilerBody_EndOfProcess), new Type[] { typeof(BattleArmy) });
            MethodInfo MI_TranspilerBody_Victory = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_cycle_TranspilerBody_onArmyBattleVictory), new Type[] { typeof(BattleArmy) });

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
            foreach (Hooks hook in mod.GetRegisteredHooks())
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
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_StartOfProcess(battle);
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_EndOfProcess(BattleArmy battle)
        {
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_EndOfProcess(battle);
            }
        }

        private static void BattleArmy_cycle_TranspilerBody_onArmyBattleVictory(BattleArmy battle)
        {
            List<UM> victorUnits = new List<UM>();
            List<UA> victorComs = new List<UA>();
            List<UM> defeatedUnits = new List<UM>();
            List<UA> defeatedComs= new List<UA>();
            ArmyBattleData data = HarmonyPatches.armyBattleData_StartOfCycle;

            if (battle.attackers.Count == 0 && battle.defenders.Count > 0)
            {
                victorUnits.AddRange(battle.defenders);
                victorComs.AddRange(battle.defComs);

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
                victorUnits.AddRange(battle.attackers);
                victorComs.AddRange(battle.attComs);

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

            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleVictory(battle, victorUnits, victorComs, defeatedUnits, defeatedComs);
            }

            HarmonyPatches.armyBattleData_StartOfCycle.Clear();
        }

        private static IEnumerable<CodeInstruction> BattleArmy_unitMovesFromLocation_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_onArmyBattleRetreatOrFlee = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_unitMovesFromLocation_TranspilerBody_OnAmryBattleRetreatOrFlee), new Type[] { typeof(BattleArmy), typeof(Unit) });
            MethodInfo MI_TransplilerBody_onArmyBattleTerminated = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_unitMovesFromLocation_TranspilerBody_onArmyBattleTerminated), new Type[] { typeof(BattleArmy), typeof(UM) });
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
            foreach (Hooks hook in mod.GetRegisteredHooks())
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

            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleTerminated(battle, victorUnits, victorComs, u);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_computeAdvantage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_computeAdvantage_TranspilerBody), new Type[] { typeof(BattleArmy), typeof(double) });

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
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_ComputeAdvantage(battle, advantage);
            }
        }

        private static IEnumerable<CodeInstruction> BattleArmy_allocateDamage_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody_allocateDamage = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_allocateDamage_TranspilerBody_allocateDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]) });
            MethodInfo MI_TranspilerBody_receivesDamage = AccessTools.Method(typeof(HarmonyPatches), nameof(BattleArmy_allocateDamage_TranspilerBody_receivesDamage), new Type[] { typeof(BattleArmy), typeof(List<UM>), typeof(int[]), typeof(int) });

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
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook?.onArmyBattleCycle_AllocateDamage(battle, units, dmgs);
            }
        }

        private static void BattleArmy_allocateDamage_TranspilerBody_receivesDamage(BattleArmy battle, List<UM> units, int[] dmgs, int i)
        {
            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                dmgs[i] = hook?.onUnitReceivesArmyBattleDamage(battle, units[i], dmgs[i]) ?? dmgs[i];
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

            MethodInfo MI_TranspilerBody_DisplayInfluenceElder = AccessTools.Method(typeof(HarmonyPatches), nameof(PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceElder));
            MethodInfo MI_TranspilerBody_DisplayInfluenceHuman = AccessTools.Method(typeof(HarmonyPatches), nameof(PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceHuman));
            MethodInfo MI_TranspilerBody_InfluenceStats = AccessTools.Method(typeof(HarmonyPatches), nameof(PopupHolyOrder_setTo_TranspilerBody_InfluenceStats));

            // Influence Dark and Influence Good Summaries
            FieldInfo FI_PopupHolyOrder_influenceDark = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceDark));
            FieldInfo FI_PopupHolyOrder_influenceDarkp0 = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceDarkp0));
            FieldInfo FI_PopupHolyOrder_influenceGood = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceGood));
            FieldInfo FI_PopupHolyOrder_influenceGoodp0 = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceGoodp0));
            // Influence Dark and Influence Good Stats
            FieldInfo FI_PopupHolyOrder_influenceDarkStats = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceDarkStats));
            FieldInfo FI_PopupHolyOrder_influenceGoodStats = AccessTools.Field(typeof(PopupHolyOrder), nameof(PopupHolyOrder.influenceGoodStats));


            (int, int)[] rangesDisplayInf = new (int, int)[5];

            int findIndex = -1;

            // For loop to gather all target indexes and ranges
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (findIndex == -1 && instructionList[i].opcode == OpCodes.Ldfld)
                {
                    switch (instructionList[i].operand)
                    {
                        case FieldInfo fi when fi == FI_PopupHolyOrder_influenceDark:
                            rangesDisplayInf[0] = (i, 0);
                            findIndex = 0;
                            Console.WriteLine("CommunityLib: Found transpiler target " + findIndex + " at Index " + i);
                            break;
                        case FieldInfo fi when fi == FI_PopupHolyOrder_influenceGood:
                            rangesDisplayInf[1] = (i, 0);
                            findIndex = 1;
                            Console.WriteLine("CommunityLib: Found transpiler target " + findIndex + " at Index " + i);
                            break;
                        case FieldInfo fi when fi == FI_PopupHolyOrder_influenceDarkp0:
                            rangesDisplayInf[2] = (i, 0);
                            findIndex = 2;
                            Console.WriteLine("CommunityLib: Found transpiler target " + findIndex + " at Index " + i);
                            break;
                        case FieldInfo fi when fi == FI_PopupHolyOrder_influenceGoodp0:
                            rangesDisplayInf[3] = (i, 0);
                            findIndex = 3;
                            Console.WriteLine("CommunityLib: Found transpiler target " + findIndex + " at Index " + i);
                            break;
                        case FieldInfo fi when fi == FI_PopupHolyOrder_influenceDarkStats:
                            rangesDisplayInf[4] = (i, 0);
                            findIndex = 4;
                            Console.WriteLine("CommunityLib: Found transpiler target " + findIndex + " at Index " + i);
                            break;
                        default:
                            break;
                    }
                }

                if (findIndex != -1)
                {
                    if (findIndex < 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Call && instructionList[i - 1].opcode == OpCodes.Stelem_Ref && instructionList[i + 1].opcode == OpCodes.Callvirt)
                        {
                            rangesDisplayInf[findIndex] = (rangesDisplayInf[findIndex].Item1, i);
                            Console.WriteLine("CommunityLib: Found transpiler range " + findIndex + " end at Index " + i);
                            findIndex = -1;
                        }
                    }
                    else if (findIndex == 4)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i+1].opcode == OpCodes.Ldfld && instructionList[i + 2].opcode == OpCodes.Ldfld)
                        {
                            rangesDisplayInf[findIndex] = (rangesDisplayInf[findIndex].Item1, i);
                            Console.WriteLine("CommunityLib: Found transpiler range " + findIndex + " end at Index " + i);
                            findIndex = -1;
                        }
                    }
                }
            }

            // Modify code at each target index and range in reverse order.
            /*if (rangesDisplayInf[4].Item1 != 0 && rangesDisplayInf[4].Item2 != 0)
            {
                // This section causes an Invalid IL in wrapper error when run. Targeting is correct.
                Console.WriteLine("CommunityLib: Modifying instructionList for transpiler target 4");
                instructionList.RemoveRange(rangesDisplayInf[4].Item1, rangesDisplayInf[4].Item2 - rangesDisplayInf[4].Item1);

                List<CodeInstruction> newInstructions = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Call, MI_TranspilerBody_InfluenceStats)
                };

                instructionList.InsertRange(rangesDisplayInf[4].Item1, newInstructions);
            }*/

            if (rangesDisplayInf[3].Item1 != 0 && rangesDisplayInf[3].Item2 != 0)
            {
                Console.WriteLine("CommunityLib: Modifying instructionList for transpiler target 3");
                instructionList.RemoveRange(rangesDisplayInf[3].Item1, 1 + rangesDisplayInf[3].Item2 - rangesDisplayInf[3].Item1);

                List<CodeInstruction> newInstructions = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGoodp0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman)
                };

                instructionList.InsertRange(rangesDisplayInf[3].Item1, newInstructions);
            }

            if (rangesDisplayInf[2].Item1 != 0 && rangesDisplayInf[2].Item2 != 0)
            {
                Console.WriteLine("CommunityLib: Modifying instructionList for transpiler target 2");
                instructionList.RemoveRange(rangesDisplayInf[2].Item1, 1 + rangesDisplayInf[2].Item2 - rangesDisplayInf[2].Item1);

                List<CodeInstruction> newInstructions = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDarkp0),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder)
                };

                instructionList.InsertRange(rangesDisplayInf[2].Item1, newInstructions);
            }

            if (rangesDisplayInf[1].Item1 != 0 && rangesDisplayInf[1].Item2 != 0)
            {
                Console.WriteLine("CommunityLib: Modifying instructionList for transpiler target 1");
                instructionList.RemoveRange(rangesDisplayInf[1].Item1, 1 + rangesDisplayInf[1].Item2 - rangesDisplayInf[1].Item1);

                List<CodeInstruction> newInstructions = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceGood),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceHuman)
                };

                instructionList.InsertRange(rangesDisplayInf[1].Item1, newInstructions);
            }

            if (rangesDisplayInf[0].Item1 != 0 && rangesDisplayInf[0].Item2 != 0)
            {
                Console.WriteLine("CommunityLib: Modifying instructionList for transpiler target 0");
                instructionList.RemoveRange(rangesDisplayInf[0].Item1, 1 + rangesDisplayInf[0].Item2 - rangesDisplayInf[0].Item1);

                List<CodeInstruction> newInstructions = new List<CodeInstruction> {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, FI_PopupHolyOrder_influenceDark),
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloc_S, 4),
                    new CodeInstruction(OpCodes.Call, MI_TranspilerBody_DisplayInfluenceElder)
                };

                instructionList.InsertRange(rangesDisplayInf[0].Item1, newInstructions);
            }

            for (int i = 0; i < instructionList.Count; i++)
            {
                yield return instructionList[i];
            }
        }

        private static string PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceElder(HolyOrder order, int infGain)
        {
            string s = "Elder Influence: " + order.influenceElder + "/" + order.influenceElderReq + " (+" + infGain + "/turn)";

            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                s = hook?.onPopupHolyOrder_DisplayInfluenceElder(order, s, infGain);
            }

            return s;
        }

        private static string PopupHolyOrder_setTo_TranspilerBody_DisplayInfluenceHuman(HolyOrder order, int infGain)
        {
            string s = "Human Influence: " + order.influenceHuman + "/" + order.influenceHumanReq + " (+" + infGain + "/turn)";

            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                s = hook?.onPopupHolyOrder_DisplayInfluenceHuman(order, s, infGain);
            }

            return s;
        }

        private static void PopupHolyOrder_setTo_TranspilerBody_InfluenceStats(PopupHolyOrder orderPop, HolyOrder order, List<ReasonMsg> reasonMsgsElder, List<ReasonMsg> reasonMsgsHuman)
        {
            Console.WriteLine("CommunityLib :: Influence stats are as follows; Dark: " + orderPop.influenceDarkStats + ", Good: " + orderPop.influenceGoodStats + ".");

            orderPop.influenceDarkStats.text = "";
            orderPop.influenceGoodStats.text = "";

            foreach (Hooks hook in mod.GetRegisteredHooks())
            {
                hook.onPopupHolyOrder_InfluenceElderStats(order, reasonMsgsElder);
                hook.onPopupHolyOrder_InfluenceHumanStats(order, reasonMsgsHuman);
            }

            foreach (ReasonMsg reasonMsg in reasonMsgsElder)
            {
                if (orderPop.influenceDarkStats.text.Length > 0)
                {
                    orderPop.influenceDarkStats.text += "\\n";
                }
                orderPop.influenceDarkStats.text += reasonMsg.msg + ": +" + (int)reasonMsg.value;
            }

            foreach (ReasonMsg reasonMsg in reasonMsgsHuman)
            {
                if (orderPop.influenceGoodStats.text.Length > 0)
                {
                    orderPop.influenceGoodStats.text += "\\n";
                }
                orderPop.influenceGoodStats.text += reasonMsg.msg + ": +" + (int)reasonMsg.value;
            }
        }

        private static void Template_TranspilerBody()
        {
            foreach(Hooks hook in mod.GetRegisteredHooks())
            {

            }
        }
    }
}
