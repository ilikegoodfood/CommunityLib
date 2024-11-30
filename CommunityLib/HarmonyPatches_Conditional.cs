using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class HarmonyPatches_Conditional
    {
        private static readonly Type patchType = typeof(HarmonyPatches_Conditional);

        private static Harmony harmony = null;

        public static void PatchingInit(Map map)
        {
            string harmonyID = "ILikeGoodFood.SOFG.CommunityLib_Conditional";
            harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                return;
            }

            getChallengeUtility_BulkPatch(map);

            //Console.WriteLine("CommunityLib: Initializing Conditional Patches");
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                Patching_Cordyceps(intDataCord);
            }

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null)
            {
                Patching_CovensCursesCurios(intDataCCC);
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Ixthus", out ModIntegrationData intDataIxthus) && intDataIxthus.assembly != null)
            {
                Patching_Ixthus(intDataIxthus);
            }
        }

        private static void getChallengeUtility_BulkPatch(Map map)
        {
            Assembly asm = typeof(Map).Assembly;

            List<MethodInfo> targetMethods = new List<MethodInfo> { AccessTools.Method(typeof(UA), nameof(UA.getChallengeUtility), new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }) };

            foreach (Type t in asm.GetTypes())
            {
                if (t.IsSubclassOf(typeof(UA)))
                {
                    MethodInfo targetMethod = AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) });
                    if (targetMethod != null)
                    {
                        targetMethods.Add(targetMethod);
                    }
                }
            }

            foreach(ModKernel kernel in map.mods)
            {
                asm = kernel.GetType().Assembly;

                foreach (Type t in asm.GetTypes())
                {
                    MethodInfo targetMethod = AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) });
                    if (targetMethod != null)
                    {
                        targetMethods.Add(targetMethod);
                    }
                }
            }

            HarmonyMethod prefix = new HarmonyMethod(patchType, nameof(UA_getChallengeUtility_BulkPrefix));
            foreach (MethodInfo mi in targetMethods)
            {
                harmony.Patch(original: mi, prefix: prefix);
            }
        }

        private static bool UA_getChallengeUtility_BulkPrefix(UA __instance, Challenge c, List<ReasonMsg> reasons, ref double __result)
        {
            AgentAI agentAI = ModCore.Get().GetAgentAI();

            if (agentAI == null)
            {
                return true;
            }

            if (agentAI.isAICheckingUtility())
            {
                return true;
            }

            if (agentAI.TryGetAgentType(__instance.GetType(), out AgentAI.AIData aiData) && aiData != null)
            {
                AIChallenge aiChallenge = agentAI.GetAIChallengeFromAgentType(__instance.GetType(), c.GetType());

                if (aiChallenge == null)
                {
                    if (c is Ritual)
                    {
                        if (aiData.controlParameters.considerAllRituals)
                        {
                            return true;
                        }
                        else
                        {
                            __result = -1.0;
                            return false;
                        }
                    }
                    else if (aiData.controlParameters.considerAllChallenges)
                    {
                        return true;
                    }
                    else
                    {
                        __result = -1.0;
                        return false;
                    }
                }

                AgentAI.ChallengeData cData = new AgentAI.ChallengeData
                {
                    aiChallenge = aiChallenge,
                    challenge = c,
                    location = c.location
                };

                if (c is Ritual)
                {
                    cData.location = __instance.location;
                }

                __result = agentAI.getChallengeUtility(cData, __instance, aiData, aiData.controlParameters, reasons);
                return false;
            }

            return true;
        }

        private static void Patching_Cordyceps(ModIntegrationData intData)
        {
            //Console.WriteLine("CommunityLib: Conditional Patch for Cordyceps");
            if (intData.methodInfoDict.TryGetValue("Drone.turnTickAI", out MethodInfo MI_turnTickAI) && MI_turnTickAI != null)
            {
                //Console.WriteLine("CommunityLib: Replacing UAEN_Drone AI");
                harmony.Patch(original: MI_turnTickAI, prefix: new HarmonyMethod(patchType, nameof(UAEN_Drone_turnTickAI_Prefix)));
            }

            if (intData.constructorInfoDict.TryGetValue("Haematophage", out ConstructorInfo CI_Haematophage) && CI_Haematophage != null)
            {
                harmony.Patch(original: CI_Haematophage, postfix: new HarmonyMethod(patchType, nameof(UAEN_Haematophage_ctor_Postfix)));
            }

            if (intData.methodInfoDict.TryGetValue("Haematophage.turnTickAI", out MethodInfo MI_turnTickAI2) && MI_turnTickAI2 != null)
            {
                //Console.WriteLine("CommunityLib: Replacing UAEN_Drone AI");
                harmony.Patch(original: MI_turnTickAI2, prefix: new HarmonyMethod(patchType, nameof(UAEN_Haematophage_turnTickAI_Prefix)));
            }

            if (intData.methodInfoDict.TryGetValue("SpawnDroneFromHuman.cast", out MethodInfo MI_SpawnDroneFromHuman_Cast) && MI_SpawnDroneFromHuman_Cast != null)
            {
                harmony.Patch(original: MI_SpawnDroneFromHuman_Cast, transpiler: new HarmonyMethod(patchType, nameof(P_SpawnDroneFromHuman_cast_Transpiler)));
            }

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static bool UAEN_Drone_turnTickAI_Prefix(UA __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(__instance.GetType()))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static void UAEN_Haematophage_ctor_Postfix(Location loc, UA __instance)
        {
            __instance.rituals.Add(new Rt_SlowHealing(loc));
        }

        private static bool UAEN_Haematophage_turnTickAI_Prefix(UA __instance)
        {
            if (ModCore.Get().GetAgentAI().ContainsAgentType(__instance.GetType()))
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }

        private static IEnumerable<CodeInstruction> P_SpawnDroneFromHuman_cast_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_selectedUnit = AccessTools.Field(typeof(GraphicalMap), nameof(GraphicalMap.selectedUnit));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldc_I4_M1 && instructionList[i+1].opcode == OpCodes.Stloc_1)
                        {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Stsfld, FI_selectedUnit);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed P_SpawnDroneFromHuman_cast_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void Patching_CovensCursesCurios(ModIntegrationData intData)
        {
            if (intData.methodInfoDict.TryGetValue("HeroicBoots.turnTick", out MethodInfo MI_BootsTurnTick))
            {
                harmony.Patch(original: MI_BootsTurnTick, transpiler: new HarmonyMethod(patchType, nameof(I_heroicBoots_turnTick_Transpiler)));
            }

            if (intData.methodInfoDict.TryGetValue("Kernel.afterMapGenAfterHistorical", out MethodInfo MI_afterMapGenAfterHistorical))
            {
                harmony.Patch(original: MI_afterMapGenAfterHistorical, transpiler: new HarmonyMethod(patchType, nameof(CCCKernel_afterMapGenAfterHistorical_Transpiler)));
            }

            if (intData.methodInfoDict.TryGetValue("UAEN_Toad.addChallenges", out MethodInfo MI_ToadAddChallenges) && MI_ToadAddChallenges != null)
            {
                harmony.Patch(original: MI_ToadAddChallenges, transpiler: new HarmonyMethod(patchType, nameof(CCCToad_AddChallenges_Transpiler)));
            }

            if (intData.methodInfoDict.TryGetValue("UAEN_Pigeon.turnTick", out MethodInfo MI_PigeonTurnTick) && MI_PigeonTurnTick != null)
            {
                harmony.Patch(original: MI_PigeonTurnTick, transpiler: new HarmonyMethod(patchType, nameof(CCCPigeon_TurnTick_Transpiler)));
            }
        }

        private static IEnumerable<CodeInstruction> I_heroicBoots_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            FieldInfo FI_unit = AccessTools.Field(typeof(Person), nameof(Person.unit));
            FieldInfo FI_engaging = AccessTools.Field(typeof(Unit), nameof(Unit.engaging));

            Label falseLabel;

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Brfalse && instructionList[i-1].opcode == OpCodes.Ldloc_1)
                        {
                            falseLabel = (Label)instructionList[i].operand;

                            yield return new CodeInstruction(OpCodes.Brfalse, falseLabel);

                            yield return new CodeInstruction(OpCodes.Ldarg_1);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_unit);
                            yield return new CodeInstruction(OpCodes.Ldfld, FI_engaging);
                            yield return new CodeInstruction(OpCodes.Ldnull);
                            yield return new CodeInstruction(OpCodes.Ceq);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed I_heroicBoots_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> CCCKernel_afterMapGenAfterHistorical_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            bool returnCode = true;
            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode != OpCodes.Nop)
                        {
                            returnCode = false;

                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Ldarg_0 && instructionList[i+1].opcode == OpCodes.Ldarg_1 && instructionList[i+2].opcode == OpCodes.Call)
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

            Console.WriteLine("CommunityLib: Completed CCCKernel_afterMapGenAfterHistorical_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static IEnumerable<CodeInstruction> CCCToad_AddChallenges_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(CCCToad_AddChallenges_TranspilerBody));

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler CCCToad_AddChallenges_Transpiler");
        }

        private static void CCCToad_AddChallenges_TranspilerBody(UAEN uaen, Location location, List<Challenge> standardChallenges)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.assembly != null && intDataCCC.fieldInfoDict.TryGetValue("UAEN_Toad.Squash", out FieldInfo FI_Squash) && FI_Squash != null)
            {
                Challenge squash = (Challenge)FI_Squash.GetValue(uaen);
                if (squash == null)
                {
                    squash.locationIndex = uaen.location.index;

                    if (!standardChallenges.Contains(squash))
                    {
                        standardChallenges.Add(squash);
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> CCCPigeon_TurnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_UAE_TurnTick = AccessTools.Method(typeof(UAE), nameof(UAE.turnTick), new Type[] { typeof(Map) });

            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Callvirt, MI_UAE_TurnTick);
            yield return new CodeInstruction(OpCodes.Nop);
            yield return new CodeInstruction(OpCodes.Ret);

            Console.WriteLine("CommunityLib: Completed complete function replacement transpiler CCCPigeon_TurnTick_Transpiler");
        }

        private static void Patching_Ixthus(ModIntegrationData intData)
        {
            if (intData.methodInfoDict.TryGetValue("Set_Crypt.turnTick", out MethodInfo MI_CryptTurnTick))
            {
                harmony.Patch(original: MI_CryptTurnTick, transpiler: new HarmonyMethod(patchType, nameof(Set_Crypt_turnTick_Transpiler)));
            }
        }

        private static IEnumerable<CodeInstruction> Set_Crypt_turnTick_Transpiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            List<CodeInstruction> instructionList = codeInstructions.ToList();

            MethodInfo MI_ToList = AccessTools.Method(typeof(Enumerable), nameof(Enumerable.ToList));
            MI_ToList = MI_ToList.MakeGenericMethod(typeof(Subsettlement));

            int targetIndex = 1;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Callvirt && instructionList[i+1].opcode == OpCodes.Stloc_S)
                        {
                            yield return new CodeInstruction(OpCodes.Callvirt, MI_ToList);

                            targetIndex = 0;
                        }
                    }
                }

                yield return instructionList[i];
            }

            Console.WriteLine("CommunityLib: Completed Set_Crypt_turnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }
    }
}
