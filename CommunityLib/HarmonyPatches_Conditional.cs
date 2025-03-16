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

            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                Patching_CovensCursesCurios(intDataCCC);
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
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC) && intDataCCC.fieldInfoDict.TryGetValue("UAEN_Toad.Squash", out FieldInfo FI_Squash) && FI_Squash != null)
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

            List<CodeInstruction> returnInstructions = new List<CodeInstruction>();

            MethodInfo MI_TranspilerBody = AccessTools.Method(patchType, nameof(CCCPigeon_TurnTick_TranspilerBody), new Type[] { typeof(UA) });

            int targetIndex = 1;
            for (int i = instructionList.Count - 1; i >= 0; i--)
            {
                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (instructionList[i].opcode == OpCodes.Call)
                        {
                            targetIndex++;
                        }
                    }
                    else if (targetIndex == 2)
                    {
                        if (instructionList[i].opcode == OpCodes.Nop)
                        {
                            targetIndex = 0;
                            returnInstructions = instructionList.GetRange(i, instructionList.Count - i);
                            break;
                        }
                    }
                }
            }

            targetIndex = 1;
            for (int i = 0; i < returnInstructions.Count; i++)
            {

                returnInstructions[i].labels.Clear();

                if (targetIndex > 0)
                {
                    if (targetIndex == 1)
                    {
                        if (returnInstructions[i].opcode == OpCodes.Ret)
                        {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Call, MI_TranspilerBody);
                            yield return new CodeInstruction(OpCodes.Nop);

                            targetIndex = 0;
                        }
                    }
                }

                yield return returnInstructions[i];
            }

            Console.WriteLine("CommunityLib: Completed  CCCPigeon_TurnTick_Transpiler");
            if (targetIndex != 0)
            {
                Console.WriteLine("CommunityLib: ERROR: Transpiler failed at targetIndex " + targetIndex);
            }
        }

        private static void CCCPigeon_TurnTick_TranspilerBody(UA ua)
        {
            if (ModCore.Get().data.tryGetModIntegrationData("CovensCursesCurios", out ModIntegrationData intDataCCC))
            {
                if (intDataCCC.typeDict.TryGetValue("UAEN_Pigeon", out Type pigeonType) && pigeonType != null)
                {
                    if (ua.GetType() == pigeonType)
                    {
                        if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.returning", out FieldInfo FI_Returning) && FI_Returning != null)
                        {
                            bool returning = (bool)FI_Returning.GetValue(ua);

                            if (intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.target", out FieldInfo FI_Target) && FI_Target != null && intDataCCC.fieldInfoDict.TryGetValue("UAEN_Pigeon.owner", out FieldInfo FI_Owner) && FI_Owner != null)
                            {
                                UA target = (UA)FI_Target.GetValue(ua);
                                UA owner = (UA)FI_Owner.GetValue(ua);

                                if (!returning)
                                {
                                    if (target == null || ((!ModCore.Get().checkIsUnitSubsumed(target) || target.person.unit.isDead) && target.isDead))
                                    {
                                        FI_Returning.SetValue(ua, true);
                                        returning = true;

                                        if (ua.task is Task_GoToUnit t_goTo && t_goTo.target == target)
                                        {
                                            ua.task = null;
                                            return;
                                        }
                                    }
                                }

                                if (returning)
                                {
                                    if (ua.task is Task_GoToUnit t_goTo && t_goTo.target == owner)
                                    {
                                        if (owner == null || ((!ModCore.Get().checkIsUnitSubsumed(owner) || owner.person.unit.isDead) && owner.isDead))
                                        {
                                            if (ua.person.gold > 0 || ua.person.items.Any(i => i != null))
                                            {
                                                Pr_ItemCache pr_ItemCache = new Pr_ItemCache(ua.location);
                                                foreach (Item item in ua.person.items)
                                                {
                                                    if (item == null)
                                                    {
                                                        continue;
                                                    }

                                                    pr_ItemCache.addItemToSet(item);
                                                }
                                                pr_ItemCache.gold = ua.person.gold;
                                            }
                                            ua.task = null;
                                            ua.map.addUnifiedMessage(ua, ua.location, "Pigeon flew away", $"After loosing it's owner{(owner == null ? "" : ", " + owner.getName() + ",")} the pigeon has flown away, leaving any gold and items it was carrying behind.", "PigeonFlewAway");
                                            if (GraphicalMap.selectedUnit == ua)
                                            {
                                                GraphicalMap.selectedUnit = null;
                                            }
                                            ua.disband(ua.map, "Ownerless pigeon dissapeared into the wilds");
                                            return;
                                        }
                                    
                                        if (ua.location == owner.location)
                                        {
                                            if (ua.person.gold > 0 || ua.person.items.Any(i => i != null))
                                            {
                                                if (owner.isCommandable())
                                                {
                                                    ua.map.world.prefabStore.popItemTrade(ua.person, owner.person, "Swap Items", -1, -1);
                                                }
                                                else
                                                {
                                                    owner.person.gold += ua.person.gold;
                                                    ua.person.gold = 0;

                                                    for (int i = 0; i < ua.person.items.Length; i++)
                                                    {
                                                        if (ua.person.items[i] == null)
                                                        {
                                                            continue;
                                                        }

                                                        owner.person.gainItem(ua.person.items[i]);
                                                        ua.person.items[i] = null;
                                                    }
                                                }
                                            }

                                            if (intDataCCC.methodInfoDict.TryGetValue("UAEN_Pigeon.gainPigeon", out MethodInfo MI_gainPigeon) && MI_gainPigeon != null)
                                            {
                                                MI_gainPigeon.Invoke(ua, new object[] { owner });
                                                ua.map.addUnifiedMessage(ua, owner, "Pigeon Returned", $"A pigeon has returned to {owner.getName()} after a long journey.", "Pigeon Returns");
                                                ua.task = null;
                                                if (GraphicalMap.selectedUnit == ua)
                                                {
                                                    GraphicalMap.selectedUnit = null;
                                                }
                                                ua.disband(ua.map, "Returned to owner.");
                                                return;
                                            }

                                            World.Log($"CommunityLib: Failed to return Covens, Curses, and Curios UAEN_Pigeon to owner.");
                                            ua.task = null;
                                            if (GraphicalMap.selectedUnit == ua)
                                            {
                                                GraphicalMap.selectedUnit = null;
                                            }
                                            ua.disband(ua.map, "Failed to return Covens, Curses, and Curios UAEN_Pigeon to owner.");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    if (ua.task is Task_GoToUnit t_goTo && t_goTo.target == target)
                                    {
                                        if (ua.location == target.location)
                                        {
                                            if (target.isCommandable())
                                            {
                                                ua.map.world.prefabStore.popItemTrade(ua.person, target.person, "Swap Items", -1, -1);
                                            }
                                            else
                                            {
                                                for (int i = 0; i < ua.person.items.Length; i++)
                                                {
                                                    target.person.gold += ua.person.gold;
                                                    ua.person.gold = 0;

                                                    if (ua.person.items[i] == null)
                                                    {
                                                        continue;
                                                    }

                                                    target.person.gainItem(ua.person.items[i]);
                                                    ua.person.items[i] = null;
                                                }
                                            }

                                            ua.task = null;
                                            FI_Returning.SetValue(ua, true);
                                            returning = true;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
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
