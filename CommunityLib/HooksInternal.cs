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

        public override void onUnitDeath_StartOfProcess(Unit u, string v, Person killer)
        {
            if (ModCore.opt_forceShipwrecks || ModCore.opt_SpawnShipwrecks)
            {
                if (u.location.isOcean)
                {
                    //Console.WriteLine("Orcs_Plus: Unit died in ocean");
                    int wreckRoll = Eleven.random.Next(10);

                    if (wreckRoll == 0)
                    {
                        ModCore.core.spawnShipwreck(u.location);
                    }
                }
            }
        }

        public override void onSettlementFallIntoRuin_StartOfProcess(Settlement set, string v, object killer = null)
        {
            if (ModCore.opt_forceShipwrecks || ModCore.opt_SpawnShipwrecks)
            {
                if (set is SettlementHuman settlementHuman && settlementHuman.subs.Count > 0)
                {
                    if (set.subs.Any(sub => sub is Sub_Docks))
                    {
                        ModCore.core.spawnShipwreck(set.location);
                    }
                }
                else if (set is Set_OrcCamp camp && camp.specialism == 5)
                {
                    ModCore.core.spawnShipwreck(set.location);
                }
            }
        }

        public override HolyOrder onLocationViewFaithButton_GetHolyOrder(Location loc)
        {
            if (ModCore.opt_ophanimFaithTomb)
            {
                if (loc.map.overmind.god is God_Ophanim opha && opha.faith != null)
                {
                    if (loc.settlement is Set_TombOfGods)
                    {
                        return opha.faith;
                    }
                }
            }

            return null;
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

        public override bool interceptAgentAI(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> challengeData, List<AgentAI.TaskData> taskData, List<Unit> visibleUnits)
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

        public override void onAgentAI_EndOfProcess(UA ua, AgentAI.AIData aiData, List<AgentAI.ChallengeData> validChallengeData, List<AgentAI.TaskData> validTaskData, List<Unit> visibleUnits)
        {
            if (ua is UAEN_DeepOne && (ua.task == null || (ua.task is Task_GoToLocation tLocation && ua.homeLocation != -1 && tLocation.target == ua.map.locations[ua.homeLocation])) && validChallengeData.Count == 0 && validTaskData.Count == 0)
            {
                Rt_DeepOnes_TravelBeneath travel = (Rt_DeepOnes_TravelBeneath)ua.rituals.FirstOrDefault(rt => rt is Rt_DeepOnes_TravelBeneath);
                if (travel != null)
                {
                    ua.task = new Task_PerformChallenge(travel);
                }
            }
        }

        // Test items.

        /*
        public override Location[] interceptGetPathTo_Location(Location locA, Location locB, Unit u, bool safeMove)
        {
            if (u is UM_FirstDaughter)
            {
                Console.WriteLine("CommunityLib: intercepted get path to for First Daughter");
                return ModCore.core.pathfinding.getPathTo(locA, locB, new List<Func<Location[], Location, Unit, bool>>(), u);
            }
            return null;
        }

        public override void onPopulatingPathfindingDelegates_Location(Location locA, Location locB, Unit u, List<Func<Location[], Location, Unit, bool>> pathfindingDelegates)
        {
            if (u is UM_FirstDaughter)
            {
                Console.WriteLine("CommunityLib: populating delegate for First Daughter");
                pathfindingDelegates.Add(Pathfinding.delegate_LANDLOCKED);
            }
        }*/


        /*public override string onPopupHolyOrder_DisplayPageText(HolyOrder order, string s, int pageIndex)
        {
            if (order is HolyOrder_Witches && pageIndex == 0)
            {
                return "Witches are fickle things.";
            }

            return s;
        }*/

        /*public override bool onAgentIsRecruitable(UA ua, bool result)
        {
            if (ua is UAEN_OrcUpstart)
            {
                return true;
            }

            return result;
        }*/
    }
}
