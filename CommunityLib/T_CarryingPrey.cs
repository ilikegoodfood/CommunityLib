using Assets.Code;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;

namespace CommunityLib
{
    public class T_CarryingPrey : Trait
    {
        public Map Map;

        public UAEN Drone;

        public int charge = 0;

        public T_CarryingPrey(Map map, UAEN drone)
        {
            Map = map;
            Drone = drone;
        }

        public override string getName()
        {
            return $"Carrying Prey ({GetPreyValue()})";
        }

        public override string getDesc()
        {
            return $"This drone is carrying {charge} prey.";
        }

        private int GetPreyValue()
        {
            Person person = null;
            if (Drone == null)
            {
                foreach (Location location in Map.locations)
                {
                    foreach (Unit unit in location.units)
                    {
                        if (!(unit is UAEN uaen))
                        {
                            continue;
                        }

                        if (unit.person.traits.Contains(this))
                        {
                            person = unit.person;
                            Drone = uaen;
                            break;
                        }
                    }
                }

                if (Drone == null && person != null)
                {
                    person.traits.Remove(this);
                }
            }

            if (ModCore.Get().data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord != null && intDataCord.typeDict.TryGetValue("Drone", out Type droneType) && droneType != null)
            {
                if (!droneType.IsAssignableFrom(Drone.GetType()))
                {
                    charge = 0;
                    Drone = null;
                    if (person != null)
                    {
                        person.traits.Remove(this);
                    }
                }

                if (intDataCord.fieldInfoDict.TryGetValue("Drone.prey", out FieldInfo FI_prey) && FI_prey != null)
                {
                    charge = (int)FI_prey.GetValue(Drone);
                }
                else
                {
                    charge = 0;
                }
            }
            else
            {
                charge = 0;
            }

            return charge;
        }
    }
}
