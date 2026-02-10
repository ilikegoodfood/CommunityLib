using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;

namespace CommunityLib
{
    public static class Pathfinding
    {
        #region Pathfinding Delegates
        public static double delegate_AQUAPHIBIOUS(Location[] currentPath, Location location, Unit u)
        {
            if (location.isOcean || location.isCoastal)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_AQUATIC(Location[] currentPath, Location location, Unit u)
        {
            if (location.isOcean)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_AVOID_TRESSPASS(Location[] currentPath, Location location, Unit u)
        {
            if (u == null || location.soc == null || location.soc == u.society || u.society.getRel(location.soc).state != DipRel.dipState.hostile)
            {
                return 0.0;
            }

            return 30.0;
        }

        public static double delegate_DESERT_ONLY(Location[] currentPath, Location location, Unit u)
        {
            if (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY || location.hex.terrain == Hex.terrainType.VOLCANO)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_FAVOURABLE_WIND(Location[] currentPath, Location location, Unit u)
        {
            if (u != null && u.isCommandable() && location.isOcean)
            {
                return -0.5;
            }
            return 0.0;
        }

        public static double delegate_LANDLOCKED(Location[] currentPath, Location location, Unit u)
        {
            if (location.isOcean)
            {
                return 10000.0;
            }
            return 0.0;
        }

        public static double delegate_SAFE_MOVE(Location[] currentPath, Location location, Unit u)
        {
            if (u == null || location.soc == null || !location.soc.hostileTo(u))
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_SHADOWBOUND(Location[] currentPath, Location location, Unit u)
        {
            int hp = 0;

            if (u != null)
            {
                hp = u.hp;
            }

            if (u != null && location.getShadow() < 0.5)
            {
                if (currentPath.Where(l => l.getShadow() < 0.5).Count() + 1 < hp)
                {
                    return 30.0;
                }

                return 10000.0;
            }

            return 0.0;
        }

        public static double delegate_SHADOW_ONLY(Location[] currentPath, Location location, Unit u)
        {
            if (location.getShadow() < 0.5)
            {
                return 10000.0;
            }
            return 0.0;
        }

        public static double delegate_IMMOBILE(Location[] currentPath, Location location, Unit u)
        {
            return 10000.0;
        }

        public static double delegate_TRAIT_WITHTHEWIND(Location[] currentPath, Location location, Unit u)
        {
            Location currentLoc = currentPath[currentPath.Length - 1];
            if (location.isOcean && (location.hex.x - currentLoc.hex.x) + (location.hex.y - currentLoc.hex.y) > 0)
            {
                return -5.0;
            }
            return 0.0;
        }
        #endregion

        #region GetNeighbours Delegates
        public static List<Location> delegate_NEIGHBOURS_VANILLA(Location[] currentPath, Location location, Unit u)
        {
            return location.getNeighbours();
        }

        public static List<Location> delegate_NEIGHBOURS_THEENTRACE(Location[] currentPath, Location location, Unit u)
        {
            if (!(u is UA ua) || !ua.isCommandable())
            {
                return null;
            }

            List<Location> neighbours = new List<Location>();
            if (location.settlement is Set_MinorOther && location.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
            {
                Location tomb = location.map.locations.FirstOrDefault(l => l != null && ModCore.Get().checkIsElderTomb(l));

                if (tomb != null)
                {
                    neighbours.Add(tomb);
                }

                if (u.homeLocation >= 0 && u.homeLocation < u.map.locations.Count && u.map.locations[u.homeLocation] != null)
                {
                    neighbours.Add(u.map.locations[u.homeLocation]);
                }
            }

            return neighbours;
        }
        #endregion

        public static Location[] getPathTo(Location locA, Location locB, Unit u = null, bool safeMove = false)
        {
            return getPathTo(locA, locB, null, null, u, safeMove);
        }

        public static Location[] getPathTo(Location locA, Location locB, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, Unit u = null, bool safeMove = false)
        {
            return getPathTo(locA, locB, pathfindingDelegates, null, u, safeMove);
        }

        public static Location[] getPathTo(Location locA, Location locB, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<Location>>> getNeighboursDelegates, Unit u = null, bool safeMove = false)
        {
            if (locA == null || locB == null)
            {
                return null;
            }

            if (locA == locB)
            {
                return new Location[0];
            }

            pathfindingDelegates = new List<Func<Location[], Location, Unit, double>>();


            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
            }
            else if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_VANILLA))
            {
                getNeighboursDelegates.Add(delegate_NEIGHBOURS_VANILLA);
            }

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    if (!pathfindingDelegates.Contains(delegate_FAVOURABLE_WIND))
                    {
                        pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                    }

                    if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_THEENTRACE))
                    {
                        getNeighboursDelegates.Add(delegate_NEIGHBOURS_THEENTRACE);
                    }
                }

                if (u.getMaxMoves() <= 0)
                {
                    if (!pathfindingDelegates.Contains(delegate_IMMOBILE))
                    {
                        pathfindingDelegates.Add(delegate_IMMOBILE);
                    }
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    if (!pathfindingDelegates.Contains(delegate_AQUAPHIBIOUS))
                    {
                        pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                    }
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    if (!pathfindingDelegates.Contains(delegate_DESERT_ONLY))
                    {
                        pathfindingDelegates.Add(delegate_DESERT_ONLY);
                    }
                }

                if (safeMove)
                {
                    if (!pathfindingDelegates.Contains(delegate_SAFE_MOVE))
                    {
                        pathfindingDelegates.Add(delegate_SAFE_MOVE);
                    }
                }
                else if (u is UM)
                {
                    if (!pathfindingDelegates.Contains(delegate_AVOID_TRESSPASS))
                    {
                        pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                    }
                }

                if (u.person != null && u.person.traits.Any(t => t is T_WithTheWind))
                {
                    if (!pathfindingDelegates.Contains(delegate_TRAIT_WITHTHEWIND))
                    {
                        pathfindingDelegates.Add(delegate_TRAIT_WITHTHEWIND);
                    }
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingPathfindingDelegates)
            {
                hook(locA, locB, u, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(locA, locB, u, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                HashSet<Location> locationHashes = new HashSet<Location> { locA };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[1] { locA }, 0.0);

                int i = 0;
                while (i < 5 * locA.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResukts = getNeighbourDelegate(pair.Value, pair.Value[pair.Value.Length - 1], u);
                        if (neighbourResukts != null)
                        {
                            foreach (Location neighbour in neighbourResukts)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Value, neighbour, u);
                                if (cost >= 10000.0)
                                {
                                    stepCost = cost;
                                    break;
                                }
                                stepCost += cost;
                            }

                            if (stepCost >= 10000.0)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[pair.Value.Length + 1];
                            Array.Copy(pair.Value, newPathArray, pair.Value.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            if (neighbour == locB)
                            {
                                return newPathArray;
                            }

                            double newPathCost = pair.Priority + stepCost;

                            paths.Enqueue(newPathArray, newPathCost);
                            locationHashes.Add(neighbour);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(locA, locB, u, pathfindingDelegates,getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(locA, locB, u, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }

                if (!allowPass)
                {
                    break;
                }
            }

            return null;
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, bool safeMove = false)
        {
            return getPathTo(loc, sg, u, null, null, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, bool safeMove = false)
        {
            return getPathTo(loc, sg, u, pathfindingDelegates, null, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<Location>>> getNeighboursDelegates, bool safeMove = false)
        {
            if (loc == null)
            {
                return null;
            }

            if (loc.soc == sg)
            {
                return new Location[0];
            }

            if (pathfindingDelegates == null)
            {
                pathfindingDelegates = new List<Func<Location[], Location, Unit, double>>();
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
            }
            else if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_VANILLA))
            {
                getNeighboursDelegates.Add(delegate_NEIGHBOURS_VANILLA);
            }

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    if (!pathfindingDelegates.Contains(delegate_FAVOURABLE_WIND))
                    {
                        pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                    }

                    if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_THEENTRACE))
                    {
                        getNeighboursDelegates.Add(delegate_NEIGHBOURS_THEENTRACE);
                    }
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    if (!pathfindingDelegates.Contains(delegate_AQUAPHIBIOUS))
                    {
                        pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                    }
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    if (!pathfindingDelegates.Contains(delegate_DESERT_ONLY))
                    {
                        pathfindingDelegates.Add(delegate_DESERT_ONLY);
                    }
                }

                if (safeMove)
                {
                    if (!pathfindingDelegates.Contains(delegate_SAFE_MOVE))
                    {
                        pathfindingDelegates.Add(delegate_SAFE_MOVE);
                    }
                }
                else if (u is UM)
                {
                    if (!pathfindingDelegates.Contains(delegate_AVOID_TRESSPASS))
                    {
                        pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                    }
                }

                if (u.person != null && u.person.traits.Any(t => t is T_WithTheWind))
                {
                    if (!pathfindingDelegates.Contains(delegate_TRAIT_WITHTHEWIND))
                    {
                        pathfindingDelegates.Add(delegate_TRAIT_WITHTHEWIND);
                    }
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingPathfindingDelegates)
            {
                hook(loc, null, u, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, null, u, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                HashSet<Location> locationHashes = new HashSet<Location> { loc };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[1] { loc }, 0.0);

                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResukts = getNeighbourDelegate(pair.Value, pair.Value[pair.Value.Length - 1], u);
                        if (neighbourResukts != null)
                        {
                            foreach (Location neighbour in neighbourResukts)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Value, neighbour, u);
                                if (cost >= 10000.0)
                                {
                                    stepCost = cost;
                                    break;
                                }
                                stepCost += cost;
                            }

                            if (stepCost >= 10000.0)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[pair.Value.Length + 1];
                            Array.Copy(pair.Value, newPathArray, pair.Value.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            if (neighbour.soc == sg)
                            {
                                if (destinations.Count == 0 || newPathCost < destinationPriority)
                                {
                                    destinationPriority = newPathCost;
                                }
                                destinations.Enqueue(newPathArray, newPathCost);
                            }

                            locationHashes.Add(neighbour);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }

                    if (destinations.Count > 0 && (!paths.TryPeekWithPriority(out _, out double nextPairPriority) || nextPairPriority > destinationPriority))
                    {
                        List<Location[]> destinationPaths = new List<Location[]>();
                        while (destinations.Count > 0)
                        {
                            ValuePriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPair.Priority == destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Value);
                            }
                            else
                            {
                                break;
                            }
                        }

                        return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(loc, null, u, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, null, u, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }

                if (!allowPass)
                {
                    break;
                }
            }
            return null;
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, bool> destinationValidityDelegate, Unit u, bool safeMove)
        {
            return getPathTo(loc, destinationValidityDelegate, null, null, u, safeMove);
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, bool> destinationValidityDelegate, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, Unit u, bool safeMove)
        {
            return getPathTo(loc, destinationValidityDelegate, pathfindingDelegates, null, safeMove);
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, bool> destinationValidityDelegate, List<Func<Location[], Location, Unit, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<Location>>> getNeighboursDelegates, Unit u, bool safeMove)
        {

            if (destinationValidityDelegate(new Location[0], loc, u))
            {
                return new Location[0];
            }

            if (pathfindingDelegates == null)
            {
                pathfindingDelegates = new List<Func<Location[], Location, Unit, double>> ();
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
            }
            else if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_VANILLA))
            {
                getNeighboursDelegates.Add(delegate_NEIGHBOURS_VANILLA);
            }

            List<Func<Location[], Location, Unit, bool>> destinationValidityDelegates = new List<Func<Location[], Location, Unit, bool>> { destinationValidityDelegate };

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    if (!pathfindingDelegates.Contains(delegate_FAVOURABLE_WIND))
                    {
                        pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                    }
                    if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_THEENTRACE))
                    {
                        getNeighboursDelegates.Add(delegate_NEIGHBOURS_THEENTRACE);
                    }
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    if (!pathfindingDelegates.Contains(delegate_AQUAPHIBIOUS))
                    {
                        pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                    }
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    if (!pathfindingDelegates.Contains(delegate_DESERT_ONLY))
                    {
                        pathfindingDelegates.Add(delegate_DESERT_ONLY);
                    }
                }

                if (safeMove)
                {
                    if (!pathfindingDelegates.Contains(delegate_SAFE_MOVE))
                    {
                        pathfindingDelegates.Add(delegate_SAFE_MOVE);
                    }
                }
                else if (u is UM)
                {
                    if (!pathfindingDelegates.Contains(delegate_AVOID_TRESSPASS))
                    {
                        pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                    }
                }

                if (u.person != null && u.person.traits.Any(t => t is T_WithTheWind))
                {
                    if (!pathfindingDelegates.Contains(delegate_TRAIT_WITHTHEWIND))
                    {
                        pathfindingDelegates.Add(delegate_TRAIT_WITHTHEWIND);
                    }
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingPathfindingDelegates)
            {
                hook(loc, null, u, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, null, u, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                HashSet<Location> locationHashes = new HashSet<Location> { loc };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { loc }, 0.0);

                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResukts = getNeighbourDelegate(pair.Value, pair.Value[pair.Value.Length - 1], u);
                        if (neighbourResukts != null)
                        {
                            foreach (Location neighbour in neighbourResukts)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Value, neighbour, u);
                                if (cost >= 10000.0)
                                {
                                    stepCost = cost;
                                    break;
                                }
                                stepCost += cost;
                            }

                            if (stepCost >= 10000.0)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[pair.Value.Length + 1];
                            Array.Copy(pair.Value, newPathArray, pair.Value.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            bool valid = true;
                            foreach (Func<Location[], Location, Unit, bool> validDelegate in destinationValidityDelegates)
                            {
                                if (!destinationValidityDelegate(pair.Value, neighbour, u))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (valid)
                            {
                                if (destinations.Count == 0 || newPathCost < destinationPriority)
                                {
                                    destinationPriority = newPathCost;
                                }
                                destinations.Enqueue(newPathArray, newPathCost);
                            }

                            locationHashes.Add(neighbour);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }

                    if (destinations.Count > 0 && (!paths.TryPeekWithPriority(out _, out double nextPairPriority) || nextPairPriority > destinationPriority))
                    {
                        List<Location[]> destinationPaths = new List<Location[]>();
                        while (destinations.Count > 0)
                        {
                            ValuePriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPair.Priority <= destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Value);
                            }
                            else
                            {
                                break;
                            }
                        }

                        return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(loc, null, u, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, null, u, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }

                if (!allowPass)
                {
                    break;
                }
            }

            return null;
        }

        public static double delegate_TRADE_VANILLA(Location[] currentPath, Location location)
        {
            double result = 0.0;
            Location locLast = currentPath[currentPath.Length - 1];

            if (location.soc == null)
            {
                if (location.isOcean)
                {
                    result += 7.5;
                }
                else
                {
                    result += 15.0;
                }
            }
            else if (location.soc is Society society)
            {
                if (location.settlement is Set_City)
                {
                    result += 10.0;
                }
                else
                {
                    result += 7.5;
                }
            }
            else
            {
                result += 30.0;
            }

            return result;
        }

        public static double delegate_TRADE_REALISTIC(Location[] currentPath, Location location)
        {
            double result = 0.0;
            Location locLast = currentPath[currentPath.Length - 1];
            if (location.soc == null)
            {
                if (location.isOcean != locLast.isOcean)
                {
                    if (locLast.settlement != null && locLast.settlement.subs.Any(sub => sub is Sub_Docks))
                    {
                        result += 2.5;
                    }
                    else
                    {
                        result += 30.0;
                    }
                }
                else if (location.isOcean)
                {
                    result += 15.0;
                }
                else
                {
                    result += 15.0;
                }
            }
            else
            {
                if (location.soc.isDark())
                {
                    result += 2.5;
                }

                if (location.soc is Society society)
                {
                    if (location.isOcean != locLast.isOcean && location.settlement != null && location.settlement.subs.Any(sub => sub is Sub_Docks))
                    {
                        result += 2.5;
                    }
                    else if (location.settlement.subs.Any(sub => sub is Sub_Market))
                    {
                        result += 2.5;
                    }
                    else if (location.settlement is Set_City)
                    {
                        if (location.isOcean != locLast.isOcean)
                        {
                            result += 10.0;
                        }
                        else
                        {
                            result += 5.0;
                        }
                    }
                    else if (locLast.settlement is SettlementHuman)
                    {
                        if (location.isOcean != locLast.isOcean)
                        {
                            result += 15.0;
                        }
                        else
                        {
                            result += 7.5;
                        }
                    }
                    else
                    {
                        if (location.isOcean != locLast.isOcean)
                        {
                            result += 20.0;
                        }
                        else
                        {
                            result += 10.0;
                        }
                    }
                }
                else
                {
                    if (location.isOcean != locLast.isOcean)
                    {
                        result += 45.0;
                    }
                    else
                    {
                        result += 30.0;
                    }
                }
            }

            // Consideration for low habitability. Strong avoidance of uninhabitable lands. Weak avoidance of low habitability lands. Ignored on ocean.
            if (!location.isOcean)
            {
                float habitability = location.hex.getHabilitability();
                if (habitability < currentPath[0].map.param.mapGen_minHabitabilityForHumans)
                {
                    result += 30.0;
                }
                else if (habitability < currentPath[0].map.param.mapGen_minHabitabilityForHumans * 2)
                {
                    result += 10.0;
                }
            }
            else if (location.hex.getTemperature() < 0.2)
            {
                double tempModifier = (0.2 - location.hex.getTemperature()) * 20;
                tempModifier *= tempModifier;
                result += tempModifier;
            }

            return result;
        }

        public static bool delegate_TRADEVALID_NODUPLICATES(Location[] currentPath, Location location)
        {
            return !location.map.tradeManager.routes.Any(r => (r.start() == currentPath[0] && r.end() == location) || (r.start() == location && r.end() == currentPath[0]));
        }

        public static bool delegate_TRADEVALID_MERGEREGIONS(Location[] currentPath, Location location)
        {
            return ModCore.Get().tradeRouteManager.routeData.indexGroups.Any(ig => ig.Contains(location.index) && !ig.Contains(currentPath[0].index));
        }

        public static Location[] getTradeRouteTo(Location start, Location end)
        {
            return getTradeRouteTo(start, end, null);
        }

        public static Location[] getTradeRouteTo(Location start, Location end, List<Func<Location[], Location, double>> pathfindingDelegates)
        {
            if (start == null || end == null)
            {
                return null;
            }

            if (start == end)
            {
                return new Location[0];
            }

            if (pathfindingDelegates == null)
            {
                if (ModCore.opt_realisticTradeRoutes)
                {
                    pathfindingDelegates = new List<Func<Location[], Location, double>> { delegate_TRADE_REALISTIC };
                }
                else
                {
                    pathfindingDelegates = new List<Func<Location[], Location, double>> { delegate_TRADE_VANILLA };
                }
            }
            else
            {

                if (ModCore.opt_realisticTradeRoutes)
                {
                    if (!pathfindingDelegates.Contains(delegate_TRADE_REALISTIC))
                    {
                        pathfindingDelegates.Add(delegate_TRADE_REALISTIC);
                    }
                }
                else
                {
                    if (!pathfindingDelegates.Contains(delegate_TRADE_VANILLA))
                    {
                        pathfindingDelegates.Add(delegate_TRADE_VANILLA);
                    }
                }
            }

            // Location[] currentPath, Location location, int[] endPointMapLayers,, Location Start, return bool destinationValid
            List<Func<Location[], Location, bool>> destinationValidityDelegates = new List<Func<Location[], Location, bool>> { delegate_TRADEVALID_NODUPLICATES };

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingTradeRoutePathfindingDelegates)
            {
                hook(start, pathfindingDelegates, destinationValidityDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, pathfindingDelegates, destinationValidityDelegates);
            }

            for (int pass = 0; pass < 2; pass++)
            {
                HashSet<Location> locationHashes = new HashSet<Location> { start };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { start }, 0.0);

                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in pair.Value[pair.Value.Length - 1].getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Value, neighbour);
                                if (cost >= 10000.0)
                                {
                                    stepCost = cost;
                                    break;
                                }
                                stepCost += cost;
                            }

                            if (stepCost >= 10000.0)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[pair.Value.Length + 1];
                            Array.Copy(pair.Value, newPathArray, pair.Value.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            if (neighbour == end)
                            {
                                bool valid = true;
                                foreach (Func<Location[], Location, bool> validDelegate in destinationValidityDelegates)
                                {
                                    if (!validDelegate(newPathArray, neighbour))
                                    {
                                        valid = false;
                                        break;
                                    }
                                }

                                if (valid)
                                {
                                    return newPathArray;
                                }
                                return null;
                            }

                            paths.Enqueue(newPathArray, newPathCost);
                            locationHashes.Add(neighbour);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfindingTadeRoute_AllowSecondPass)
                {
                    hook(start, pathfindingDelegates, destinationValidityDelegates);
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfindingTadeRoute_AllowSecondPass(start, pathfindingDelegates, destinationValidityDelegates))
                    {
                        allowPass = true;
                    }
                }

                if (!allowPass)
                {
                    break;
                }
            }

            return null;
        }

        public static Location[] getTradeRouteFrom(Location start, List<Location> endpointsAll = null)
        {
            return getTradeRouteFrom(start, null, null, endpointsAll);
        }

        public static Location[] getTradeRouteFrom(Location start, List<Func<Location[], Location, double>> pathfindingDelegates, List<Func<Location[], Location, bool>> destinationValidityDelegates, List<Location> endpointsAll = null)
        {
            if (start == null)
            {
                return null;
            }

            if (endpointsAll == null || endpointsAll.Count == 0)
            {
                endpointsAll = ModCore.Get().tradeRouteManager.getTradeRouteEndPoints();
            }

            if (endpointsAll.Count < 2)
            {
                return null;
            }

            if (pathfindingDelegates == null)
            {
                if (ModCore.opt_realisticTradeRoutes)
                {
                    pathfindingDelegates = new List<Func<Location[], Location, double>> { delegate_TRADE_REALISTIC };
                }
                else
                {
                    pathfindingDelegates = new List<Func<Location[], Location, double>> { delegate_TRADE_VANILLA };
                }
            }
            else
            {
                if (ModCore.opt_realisticTradeRoutes)
                {
                    if (!pathfindingDelegates.Contains(delegate_TRADE_REALISTIC))
                    {
                        pathfindingDelegates.Add(delegate_TRADE_REALISTIC);
                    }
                }
                else
                {
                    if (!pathfindingDelegates.Contains(delegate_TRADE_VANILLA))
                    {
                        pathfindingDelegates.Add(delegate_TRADE_VANILLA);
                    }
                }
            }

            // Location[] currentPath, Location location, int[] endPointMapLayers,, Location Start, return bool destinationValid
            if (destinationValidityDelegates == null)
            {
                destinationValidityDelegates = new List<Func<Location[], Location, bool>> { delegate_TRADEVALID_NODUPLICATES };
            }
            else
            {
                if (!destinationValidityDelegates.Contains(delegate_TRADEVALID_NODUPLICATES))
                {
                    destinationValidityDelegates.Add(delegate_TRADEVALID_NODUPLICATES);
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingTradeRoutePathfindingDelegates)
            {
                hook(start, pathfindingDelegates, destinationValidityDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, pathfindingDelegates, destinationValidityDelegates);
            }

            for (int pass = 0; pass < 2; pass++)
            {
                HashSet<Location> locationHashes = new HashSet<Location> { start };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { start }, 0.0);

                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in pair.Value[pair.Value.Length - 1].getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Value, neighbour);
                                if (cost >= 10000.0)
                                {
                                    stepCost = cost;
                                    break;
                                }
                                stepCost += cost;
                            }

                            if (stepCost >= 10000.0)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[pair.Value.Length + 1];
                            Array.Copy(pair.Value, newPathArray, pair.Value.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            if (endpointsAll.Contains(neighbour))
                            {
                                bool valid = true;
                                foreach (Func<Location[], Location, bool> validDelegate in destinationValidityDelegates)
                                {
                                    if (!validDelegate(newPathArray, neighbour))
                                    {
                                        valid = false;
                                        break;
                                    }
                                }

                                if (valid)
                                {
                                    //Console.WriteLine($"Found valid trade route from {start.getName()} ({start.hex.z}) to {newPathArray[newPathArray.Length - 1].getName()} ({newPathArray[newPathArray.Length - 1].hex.z})");
                                    if (destinations.Count == 0 || newPathCost < destinationPriority)
                                    {
                                        destinationPriority = newPathCost;
                                    }
                                    destinations.Enqueue(new ValuePriorityPair<Location[], double>(newPathArray, newPathCost));
                                }
                            }

                            paths.Enqueue(newPathArray, newPathCost);
                            locationHashes.Add(neighbour);
                        }
                    }

                    if (destinations.Count > 0 && (!paths.TryPeekWithPriority(out _, out double nextPairPriority) || nextPairPriority > destinationPriority))
                    {
                        List<Location[]> destinationPaths = new List<Location[]>();
                        while (destinations.Count > 0)
                        {
                            ValuePriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPriority == -1.0 || destinationPair.Priority == destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Value);
                            }
                            else
                            {
                                break;
                            }
                        }

                        return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                    }
                }

                //World.Log($"CommunityLib: getTradeRouteFrom {start.getName()} failed after {i}/{5 * start.map.locations.Count} iterations.");

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfindingTadeRoute_AllowSecondPass)
                {
                    hook(start, pathfindingDelegates, destinationValidityDelegates);
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfindingTadeRoute_AllowSecondPass(start, pathfindingDelegates, destinationValidityDelegates))
                    {
                        allowPass = true;
                    }
                }

                if (!allowPass)
                {
                    break;
                }
            }

            return null;
        }
    }
}
