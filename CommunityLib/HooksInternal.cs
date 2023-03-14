using Assets.Code;
using System;
using System.Collections.Generic;
using static Assets.Code.Unit;
using UnityEngine;

namespace CommunityLib
{
    internal class HooksInternal : Hooks
    {
        internal HooksInternal(Map map)
            : base(map)
        {
            
        }

        public override bool interceptAgentAI(UA ua, List<AIChallenge> aiChallenges, List<AIChallenge> aiRituals, AgentAI.InputParams inputParamse)
        {
            switch (ua)
            {
                case UAEN_DeepOne deepOne:
                    return interceptDeepOne(deepOne);
                case UAEN_OrcUpstart upstart:
                    return interceptOrcUpstart(upstart);
                default:
                    break;
            }

            return false;
        }

        private bool interceptDeepOne(UAEN_DeepOne deepOne)
        {
            if (deepOne == null)
            {
                //Console.WriteLine("ERROR: DeepOne is not DeepOne");
                return false;
            }

            if (deepOne.moveType == MoveType.NORMAL)
            {
                //Console.WriteLine("CommunityLib: MoveType is Normal");
                if (deepOne.location.isOcean)
                {
                    //Console.WriteLine("CommunityLib: DeepOne is at ocean location");
                    deepOne.moveType = MoveType.AQUAPHIBIOUS;
                    return true;
                }

                Location nearestOceanLocation = null;
                List<Location> nearbyOceanLocations = new List<Location>();
                int distance = 10000;
                foreach(Location loc in map.locations)
                {
                    if (loc.isOcean)
                    {
                        int stepDistance = map.getStepDist(deepOne.location, loc);
                        if (stepDistance < distance)
                        {
                            nearbyOceanLocations.Clear();
                            nearbyOceanLocations.Add(loc);
                            distance = stepDistance;
                        }
                        else if (stepDistance == distance)
                        {
                            nearbyOceanLocations.Add(loc);
                        }
                    }
                }

                if (nearbyOceanLocations.Count == 1)
                {
                    nearestOceanLocation = nearbyOceanLocations[0];
                }
                else if (nearbyOceanLocations.Count > 1)
                {
                    nearestOceanLocation = nearbyOceanLocations[Eleven.random.Next(nearbyOceanLocations.Count)];
                }

                if (nearestOceanLocation != null)
                {
                    Console.WriteLine("CommunityLib: Going to nearest ocean location");
                    deepOne.task = new Task_GoToLocation(nearestOceanLocation);
                }
                else
                {
                    Console.WriteLine("CommunityLib: Unable to reach the ocean.");
                    deepOne.die(map, "Unable to reach the ocean");
                }

                return true;
            }

            return false;
        }

        private bool interceptOrcUpstart(UAEN_OrcUpstart upstart)
        {
            if (upstart.society.isGone() || upstart.society.lastTurnLocs.Count == 0)
            {
                upstart.die(map, "Died in the wilderness", null);
                return true;
            }
            return false;
        }
    }
}
