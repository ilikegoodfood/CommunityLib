using Assets.Code;
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

        public static void PatchingInit()
        {
            string harmonyID = "ILikeGoodFood.SOFG.CommunityLib_Conditional";
            harmony = new Harmony(harmonyID);

            if (Harmony.HasAnyPatches(harmonyID))
            {
                return;
            }

            //Console.WriteLine("CommunityLib: Initializing Conditional Patches");
            if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                Patching_Cordyceps(intDataCord);
            }
        }

        private static void Patching_Cordyceps(ModData.ModIntegrationData intData)
        {
            //Console.WriteLine("CommunityLib: Conditional Patch for Cordyceps");
            if (intData.methodInfoDict.TryGetValue("Drone.turnTickAI", out MethodInfo MI_turnTickAI) && MI_turnTickAI != null)
            {
                //Console.WriteLine("CommunityLib: Replacing UAEN_Drone AI");
                harmony.Patch(original: MI_turnTickAI, prefix: new HarmonyMethod(patchType, nameof(UAEN_Drone_turnTickAI_Prefix)));
            }

            // Template Patch
            // harmony.Patch(original: AccessTools.Method(typeof(), nameof(), new Type[] { typeof() }), postfix: new HarmonyMethod(patchType, nameof()));
        }

        private static bool UAEN_Drone_turnTickAI_Prefix(UA __instance)
        {
            if (ModCore.core.GetAgentAI().ContainsAgentType(__instance.GetType()))
            {
                ModCore.core.GetAgentAI().turnTickAI(__instance);
                return false;
            }
            return true;
        }
    }
}
