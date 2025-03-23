using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class PowerDelayed : Power
    {
        public List<Pair<Location, int>> TargetDelays_Locations = new List<Pair<Location, int>>();

        public List<Pair<Unit, int>> TargetDelays_Units = new List<Pair<Unit, int>>();

        public PowerDelayed(Map map)
            : base(map)
        {

        }

        public virtual void turnTick()
        {
            List<Pair<Location, int>> expiredLocationPairs = new List<Pair<Location, int>>();
            foreach (Pair<Location, int> pair in TargetDelays_Locations)
            {
                pair.Item2--;

                if (pair.Item2 <= 0)
                {
                    expiredLocationPairs.Add(pair);
                    CastDelayed(pair.Item1);
                }
                else
                {
                    WhileDelayed(pair.Item1);
                }
            }
            foreach (Pair<Location, int> expiredPair in expiredLocationPairs)
            {
                TargetDelays_Locations.Remove(expiredPair);
            }

            List<Pair<Unit, int>> expiredUnitPairs = new List<Pair<Unit, int>>();
            foreach (Pair<Unit, int> pair in TargetDelays_Units)
            {
                pair.Item2--;

                if (pair.Item2 <= 0)
                {
                    expiredUnitPairs.Add(pair);
                    CastDelayed(pair.Item1);
                }
                else
                {
                    WhileDelayed(pair.Item1);
                }
            }
            foreach (Pair<Unit, int> expiredPair in expiredUnitPairs)
            {
                TargetDelays_Units.Remove(expiredPair);
            }
        }

        public virtual void WhileDelayed(Location location)
        {
            WhileDelayedCommon(location);
        }

        public virtual void WhileDelayed(Unit unit)
        {
            WhileDelayedCommon(unit.location);
        }

        public virtual void WhileDelayedCommon(Location location)
        {

        }

        public virtual void CastDelayed(Location location)
        {
            CastDelayedCommon(location);
        }

        public virtual void CastDelayed(Unit unit)
        {
            CastDelayedCommon(unit.location);
        }

        public virtual void CastDelayedCommon(Location location)
        {

        }
    }
}
