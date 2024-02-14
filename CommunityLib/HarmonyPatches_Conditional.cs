using Assets.Code;
using Assets.Code.Modding;
using DuloGames.UI;
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
        }

        private static void getChallengeUtility_BulkPatch(Map map)
        {
            Assembly asm = typeof(Map).Assembly;

            List<MethodInfo> targetMethods = new List<MethodInfo> { AccessTools.Method(typeof(UA), nameof(UA.getChallengeUtility), new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }) };

            foreach (Type t in asm.GetTypes())
            {
                if (t.IsSubclassOf(typeof(UA)) && AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }) != null)
                {
                    targetMethods.Add(AccessTools.Method(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }));
                }
            }

            foreach(ModKernel kernel in map.mods)
            {
                asm = kernel.GetType().Assembly;

                foreach (Type t in asm.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(UA)) && AccessTools.DeclaredMethod(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }) != null)
                    {
                        targetMethods.Add(AccessTools.Method(t, "getChallengeUtility", new Type[] { typeof(Challenge), typeof(List<ReasonMsg>) }));
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
            if (ModCore.Get().GetAgentAI().isAICheckingUtility())
            {
                return true;
            }

            if (ModCore.Get().GetAgentAI().TryGetAgentType(__instance.GetType(), out AgentAI.AIData? aiData) && aiData is AgentAI.AIData data)
            {
                AIChallenge aiChallenge = ModCore.Get().GetAgentAI().GetAIChallengeFromAgentType(__instance.GetType(), c.GetType());

                if (aiChallenge == null)
                {
                    if (c is Ritual)
                    {
                        if (data.controlParameters.considerAllRituals)
                        {
                            return true;
                        }
                        else
                        {
                            __result = -1.0;
                            return false;
                        }
                    }
                    else if (data.controlParameters.considerAllChallenges)
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

                __result = ModCore.Get().GetAgentAI().getChallengeUtility(cData, __instance, data, data.controlParameters, reasons);
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
    }
}
