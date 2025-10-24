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

            // Control debugging for bulk patches seperately due to large output volue.
            Harmony.DEBUG = false;
            BulkPatches(map);
            Harmony.DEBUG = false;

            //Console.WriteLine("CommunityLib: Initializing Conditional Patches");
            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
            {
                Patching_Cordyceps(intDataCord);
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Ixthus", out ModIntegrationData intDataIxthus))
            {
                Patching_Ixthus(intDataIxthus);
            }
        }

        private static void BulkPatches(Map map)
        {
            Assembly asm = typeof(Map).Assembly;

            List<MethodInfo> targetMethods_getChallengeUtility = new List<MethodInfo> { AccessTools.Method(typeof(UA), nameof(UA.getChallengeUtility), new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }) };
            List<MethodInfo> targetMethods_getStartingTraits = new List<MethodInfo> { AccessTools.Method(typeof(UA), nameof(UA.getStartingTraits), Type.EmptyTypes) };
            List<MethodInfo> targetMethods_turnTickAI = new List<MethodInfo> { AccessTools.Method(typeof(UA), nameof(UA.turnTickAI), Type.EmptyTypes) };
            List<MethodInfo> targetMethods_getMaxPower = new List<MethodInfo> { AccessTools.Method(typeof(God), nameof(God.getMaxPower), Type.EmptyTypes) };
            List<MethodInfo> targetMethods_getAgentCaps = new List<MethodInfo> { AccessTools.Method(typeof(God), nameof(God.getAgentCaps), Type.EmptyTypes) };

            foreach (Type t in asm.GetTypes())
            {
                if (t.IsSubclassOf(typeof(UA)))
                {
                    MethodInfo targetMethod_getChallengeUtility = AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) });
                    if (targetMethod_getChallengeUtility != null)
                    {
                        targetMethods_getChallengeUtility.Add(targetMethod_getChallengeUtility);
                    }

                    MethodInfo targetMethod_getStartingTraits = AccessTools.DeclaredMethod(t, "getStartingTraits", Type.EmptyTypes);
                    if (targetMethod_getStartingTraits != null)
                    {
                        targetMethods_getStartingTraits.Add(targetMethod_getStartingTraits);
                    }

                    MethodInfo targetMethod_turnTickAI = AccessTools.DeclaredMethod(t, "turnTickAI", Type.EmptyTypes);
                    if (targetMethod_turnTickAI != null)
                    {
                        targetMethods_turnTickAI.Add(targetMethod_turnTickAI);
                    }
                }
                else if (t.IsSubclassOf(typeof(God)))
                {
                    MethodInfo targetMethod_getAgentCaps = AccessTools.DeclaredMethod(t, "getAgentCaps", Type.EmptyTypes);
                    if (targetMethod_getAgentCaps != null)
                    {
                        targetMethods_getAgentCaps.Add(targetMethod_getAgentCaps);
                    }

                    MethodInfo targetMethod_getMaxPower = AccessTools.DeclaredMethod(t, "getMaxPower", Type.EmptyTypes);
                    if (targetMethod_getMaxPower != null)
                    {
                        targetMethods_getMaxPower.Add(targetMethod_getMaxPower);
                    }
                }
            }

            foreach(ModKernel kernel in map.mods)
            {
                asm = kernel.GetType().Assembly;

                foreach (Type t in asm.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(UA)))
                    {
                        MethodInfo targetMethod_getChallengeUtility = AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) });
                        if (targetMethod_getChallengeUtility != null)
                        {
                            targetMethods_getChallengeUtility.Add(targetMethod_getChallengeUtility);
                        }

                        MethodInfo targetMethod_getStartingTraits = AccessTools.DeclaredMethod(t, "getStartingTraits", Type.EmptyTypes);
                        if (targetMethod_getStartingTraits != null)
                        {
                            targetMethods_getStartingTraits.Add(targetMethod_getStartingTraits);
                        }

                        MethodInfo targetMethod_turnTickAI = AccessTools.DeclaredMethod(t, "turnTickAI", Type.EmptyTypes);
                        if (targetMethod_turnTickAI != null)
                        {
                            targetMethods_turnTickAI.Add(targetMethod_turnTickAI);
                        }
                    }
                    else if (t.IsSubclassOf(typeof(God)))
                    {
                        MethodInfo targetMethod_getAgentCaps = AccessTools.DeclaredMethod(t, "getAgentCaps", Type.EmptyTypes);
                        if (targetMethod_getAgentCaps != null)
                        {
                            targetMethods_getAgentCaps.Add(targetMethod_getAgentCaps);
                        }

                        MethodInfo targetMethod_getMaxPower = AccessTools.DeclaredMethod(t, "getMaxPower", Type.EmptyTypes);
                        if (targetMethod_getMaxPower != null)
                        {
                            targetMethods_getMaxPower.Add(targetMethod_getMaxPower);
                        }
                    }
                }
            }

            HarmonyMethod prefix_getChallengeUtility = new HarmonyMethod(patchType, nameof(UA_getChallengeUtility_BulkPrefix));
            foreach (MethodInfo mi in targetMethods_getChallengeUtility)
            {
                harmony.Patch(original: mi, prefix: prefix_getChallengeUtility);
            }

            HarmonyMethod postfix_getStartingTraits = new HarmonyMethod(patchType, nameof(UA_getStartingTraits_BulkPostfix));
            foreach (MethodInfo mi in targetMethods_getStartingTraits)
            {
                harmony.Patch(original: mi, postfix: postfix_getStartingTraits);
            }

            HarmonyMethod prefix_turnTickAI = new HarmonyMethod(patchType, nameof(UA_turnTickAI_BulkPrefix));
            foreach (MethodInfo mi in targetMethods_turnTickAI)
            {
                harmony.Patch(original: mi, prefix: prefix_turnTickAI);
            }

            HarmonyMethod postfix_getAgentCaps = new HarmonyMethod(patchType, nameof(God_getAgentCaps_BultPostfix));
            foreach (MethodInfo mi in targetMethods_getAgentCaps)
            {
                harmony.Patch(original: mi, postfix: postfix_getAgentCaps);
            }

            HarmonyMethod postfix_getMaxPower = new HarmonyMethod(patchType, nameof(God_getMaxPower_BulkPostfix));
            foreach (MethodInfo mi in targetMethods_getMaxPower)
            {
                harmony.Patch(original: mi, postfix: postfix_getMaxPower);
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

        private static void UA_getStartingTraits_BulkPostfix(UA __instance, ref List<Trait> __result)
        {
            if (HarmonyPatches.GettingAvailableTraits)
            {
                return;
            }

            if (__result == null)
            {
                __result = new List<Trait>();
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onAgentLevelup_GetTraits)
            {
                hook(__instance, __result, true);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onAgentLevelup_GetTraits(__instance, __result, true);
            }
        }

        private static bool UA_turnTickAI_BulkPrefix(UA __instance)
        {
            if (!ModCore.Get().GetAgentAI().IsDuringIntercept && ModCore.Get().GetAgentAI().TryGetAgentType(__instance.GetType(), out AgentAI.AIData aiData) && aiData != null)
            {
                ModCore.Get().GetAgentAI().turnTickAI(__instance);
                return false;
            }

            return true;
        }

        private static void God_getAgentCaps_BultPostfix(God __instance, ref int[] __result)
        {
            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onGetAgentCaps)
            {
                hook(__instance, __result);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook?.onGetAgentCaps(__instance, __result);
            }
        }

        private static void God_getMaxPower_BulkPostfix(God __instance, ref int __result)
        {
            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onGetMaxPower)
            {
                __result = hook(__instance, __result);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                __result = hook?.onGetMaxPower(__instance, __result) ?? __result;
            }
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
