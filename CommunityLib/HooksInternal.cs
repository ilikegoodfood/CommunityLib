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

        public override bool interceptAgentAI(UA ua, List<AIChallenge> aiChallenges, List<AIChallenge> aiRituals, bool respectChallengeVisibility = false, bool respectUnitVisibility = false, bool respectDanger = true, bool valueTimeCost = false)
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
                return false;
            }

            if (deepOne.moveType == MoveType.NORMAL)
            {
                if (deepOne.location.isOcean)
                {
                    deepOne.moveType = MoveType.AQUAPHIBIOUS;
                    return false;
                }

                Location nearestOceanLocation = null;
                List<Location>[] array;
                if (ModCore.modCore.GetCache().oceanLocationsByStepsExclusiveFromLocation.TryGetValue(deepOne.location, out array) && array != null)
                {
                    List<Location> nearbyOceanLocations;
                    for (int i = 1; i < array.Length; i++)
                    {
                        nearbyOceanLocations = array[i];
                        if (nearbyOceanLocations != null && nearbyOceanLocations.Count > 0)
                        {
                            nearestOceanLocation = nearbyOceanLocations[Eleven.random.Next(nearbyOceanLocations.Count)];
                            break;
                        }
                    }
                }

                if (nearestOceanLocation != null)
                {
                    deepOne.task = new Task_GoToLocation(nearestOceanLocation);
                }
                else
                {
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
