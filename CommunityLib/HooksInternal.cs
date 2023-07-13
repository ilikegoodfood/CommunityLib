using Assets.Code;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using System.Linq;

namespace CommunityLib
{
    internal class HooksInternal : Hooks
    {
        internal HooksInternal(Map map)
            : base(map)
        {
            
        }

        public override bool interceptGetVisibleUnits(UA ua, List<Unit> visibleUnits)
        {
            switch (ua)
            {
                case UAEN_DeepOne _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_Ghast _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_OrcUpstart _:
                    visibleUnits = new List<Unit>();
                    return true;
                case UAEN_Vampire _:
                    visibleUnits = new List<Unit>();
                    return true;
                default:
                    break;
            }

            if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
                {
                    if (ua.GetType() == droneType)
                    {
                        visibleUnits = new List<Unit>();
                        return true;
                    }
                }

            }

            return false;
        }

        public override bool interceptAgentAI(UA ua, List<AgentAI.ChallengeData> challengeData, List<AgentAI.TaskData> taskData, List<Unit> visibleUnits, AgentAI.ControlParameters inputParamse)
        {
            switch (ua)
            {
                case UAEN_OrcUpstart upstart:
                    return interceptOrcUpstart(upstart);
                default:
                    break;
            }

            if (ModCore.core.data.tryGetModAssembly("Cordyceps", out ModData.ModIntegrationData intDataCord) && intDataCord.assembly != null)
            {
                if (intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
                {
                    if (ua.GetType() == droneType)
                    {
                        if (intDataCord.typeDict.TryGetValue("God", out Type godType) && godType != null && (ua.map.overmind.god.GetType() == godType || ua.map.overmind.god.GetType().IsSubclassOf(godType)))
                        {
                            return interceptCordycepsDrone(ua, intDataCord);
                        }
                        else
                        {
                            ua.die(ua.map, "Died in Wilderness", null);
                            return true;
                        }
                    }
                }
                
            }

            return false;
        }
        private bool interceptOrcUpstart(UAEN_OrcUpstart upstart)
        {
            if (upstart.society.checkIsGone() || upstart.society.lastTurnLocs.Count == 0)
            {
                upstart.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }

        private bool interceptCordycepsDrone(UA ua, ModData.ModIntegrationData intData)
        {
            if (intData.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null && intData.fieldInfoDict.TryGetValue("Drone.prey", out FieldInfo FI_Prey) && FI_Prey != null && intData.methodInfoDict.TryGetValue("God.eat", out MethodInfo MI_GodEat) && MI_GodEat != null)
            {
                if (intData.typeDict.TryGetValue("Hive", out Type hiveType) && hiveType != null && intData.typeDict.TryGetValue("LarvalMass", out Type larvalType) && larvalType != null)
                {
                    if (ua.location.settlement != null && (ua.location.settlement.GetType() == hiveType || ua.location.settlement.GetType().IsSubclassOf(hiveType)))
                    {
                        Property larvalMass = ua.location.properties.FirstOrDefault(pr => pr.GetType() == larvalType);
                        if (larvalMass != null)
                        {
                            larvalMass.charge += (int)FI_Prey.GetValue(ua);
                            MI_GodEat.Invoke(ua.map.overmind.god, new object[] { (int)FI_Prey.GetValue(ua) });
                            FI_Prey.SetValue(ua, 0);
                        }
                    }
                }
            }

            return false;
        }
    }
}
