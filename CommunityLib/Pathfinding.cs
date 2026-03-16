using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;

namespace CommunityLib
{
    public static class Pathfinding
    {
        #region Pathfinding Delegates
        public static double delegate_AQUAPHIBIOUS(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.isOcean || location.isCoastal)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_AQUATIC(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.isOcean)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_AVOID_TRESSPASS(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (u == null || location.soc == null || location.soc == u.society || u.society.getRel(location.soc).state != DipRel.dipState.hostile)
            {
                return 0.0;
            }

            return 30.0;
        }

        public static double delegate_DESERT_ONLY(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY || location.hex.terrain == Hex.terrainType.VOLCANO)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_FAVOURABLE_WIND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (u != null && u.isCommandable() && location.isOcean)
            {
                return -0.5;
            }
            return 0.0;
        }

        public static double delegate_LANDLOCKED(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.isOcean)
            {
                return 10000.0;
            }
            return 0.0;
        }

        public static double delegate_LAYERBOUND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (targetMapLayers == null || targetMapLayers.Count == 0 || targetMapLayers.Contains(location.hex.z))
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_SAFE_MOVE(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (u == null || location.soc == null || !location.soc.hostileTo(u) || u.isCommandable())
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_SHADOWBOUND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
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

        public static double delegate_SHADOW_ONLY(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.getShadow() < 0.5)
            {
                return 10000.0;
            }
            return 0.0;
        }

        public static double delegate_IMMOBILE(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            return 10000.0;
        }

        public static double delegate_TRAIT_WITHTHEWIND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            Location currentLoc = currentPath[currentPath.Length - 1];
            if (location.isOcean && (location.hex.x - currentLoc.hex.x) + (location.hex.y - currentLoc.hex.y) > 0)
            {
                return -5.0;
            }
            return 0.0;
        }

        public static double delegate_DWARVEN_EXPANSION(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (location.hex.z != 1)
            {
                return 10000.0;
            }

            if (!location.isOcean && !(location.soc is Soc_Dwarves) && !(location.settlement is Set_DwarvenCity) && !(location.settlement is Set_DwarvenOutpost))
            {
                return 10000.0;
            }

            return 0.0;
        }
        #endregion

        #region Mod-Specific Pathfinding Delegates
        public static double delegate_LIVINGVOID_AVOIDVOID(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (u != null && !u.isCommandable() && u != u.map.awarenessManager.chosenOne)
            {
                if (u.map.tempMap[location.hex.x][location.hex.y] < -999f)
                {
                    return -10000.0;
                }
            }

            return 0.0;
        }
        #endregion

        #region Destination Validity Delegates
        public static bool delegate_VALID_LAYERBOUND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            return targetMapLayers == null || targetMapLayers.Count == 0 || targetMapLayers.Contains(location.hex.z);
        }
        #endregion

        #region GetNeighbours Delegates
        public static List<Location> delegate_NEIGHBOURS_VANILLA(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            return location.getNeighbours();
        }

        public static List<Location> delegate_NEIGHBOURS_THEENTRACE(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
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

        public static Location[] getPathTo(Location locA, Location locB, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, Unit u = null, bool safeMove = false)
        {
            return getPathTo(locA, locB, pathfindingDelegates, null, u, safeMove);
        }

        public static Location[] getPathTo(Location locA, Location locB, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<int>, List<Location>>> getNeighboursDelegates, Unit u = null, bool safeMove = false)
        {
            if (locA == null || locB == null)
            {
                return null;
            }

            if (locA == locB)
            {
                return new Location[0];
            }

            List<int> expectedMapLayers = new List<int> { locA.hex.z };

            if (locA.hex.z != locB.hex.z)
            {
                expectedMapLayers.Add(locB.hex.z);
            }

            if (pathfindingDelegates == null)
            {
                pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>> { delegate_LAYERBOUND };
            }
            else if (!pathfindingDelegates.Contains(delegate_LAYERBOUND))
            {
                pathfindingDelegates.Add(delegate_LAYERBOUND);
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<int>, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
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

                if (u is UM)
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
                hook(locA, locB, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(locA, locB, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { locA, 0.0 } };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[1] { locA }, 0.0);

                int i = 0;
                while (i < 5 * locA.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    Location currentLocation = pair.Value[pair.Value.Length - 1];
                    if (currentLocation == locB)
                    {
                        return pair.Value;
                    }

                    if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                    {
                        continue;
                    }

                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResults = getNeighbourDelegate(pair.Value, currentLocation, u, expectedMapLayers);
                        if (neighbourResults != null)
                        {
                            foreach (Location neighbour in neighbourResults)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        double stepCost = 10.0;
                        foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                        {
                            double cost = pathfindingDelegate(pair.Value, neighbour, u, expectedMapLayers);
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

                        if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                        {
                            locationHashes.Add(neighbour, newPathCost);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(locA, locB, u, expectedMapLayers, pathfindingDelegates,getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(locA, locB, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates))
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
            return getPathTo(loc, sg, u, null, null, null, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, int targetMapLayer, bool safeMove = false)
        {
            if (targetMapLayer < 0)
            {
                return getPathTo(loc, sg, u, null, null, null, safeMove);
            }
            else
            {
                return getPathTo(loc, sg, u, null, null, new List<int> { targetMapLayer }, safeMove);
            }
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<int> targetMapLayers, bool safeMove = false)
        {
            return getPathTo(loc, sg, u, null, null, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<int> targetMapLayers, bool safeMove = false)
        {
            return getPathTo(loc, sg, u, pathfindingDelegates, null, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<int>, List<Location>>> getNeighboursDelegates, List<int> targetMapLayers, bool safeMove = false)
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
                pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>> { delegate_LAYERBOUND };
            }
            else if (!pathfindingDelegates.Contains(delegate_LAYERBOUND))
            {
                pathfindingDelegates.Add(delegate_LAYERBOUND);
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<int>, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
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
                
                if (u is UM)
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

            List<int> expectedMapLayers = new List<int> { loc.hex.z };
            if (sg != null)
            {
                foreach (Location location in sg.lastTurnLocs)
                {
                    if (!expectedMapLayers.Contains(location.hex.z))
                    {
                        expectedMapLayers.Add(location.hex.z);
                    }
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingPathfindingDelegates)
            {
                hook(loc, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { loc, 0.0 } };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[1] { loc }, 0.0);

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    Location currentLocation = pair.Value[pair.Value.Length - 1];

                    if (currentLocation.soc == sg)
                    {
                        List<Location[]> destinationPaths = new List<Location[]>();
                        ValuePriorityPair<Location[], double> potentialPair;
                        while (paths.Count > 0)
                        {
                            potentialPair = paths.DequeueWithPriority();
                            if (potentialPair.Priority > pair.Priority)
                            {
                                break;
                            }

                            if (potentialPair.Value[potentialPair.Value.Length - 1].soc == sg)
                            {
                                destinationPaths.Add(potentialPair.Value);
                            }
                        }

                        if (destinationPaths.Count == 1)
                        {
                            return pair.Value;
                        }
                        else
                        {
                            return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                        }
                    }

                    if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                    {
                        continue;
                    }

                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResults = getNeighbourDelegate(pair.Value, currentLocation, u, expectedMapLayers);
                        if (neighbourResults != null)
                        {
                            foreach (Location neighbour in neighbourResults)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        double stepCost = 10.0;
                        foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                        {
                            double cost = pathfindingDelegate(pair.Value, neighbour, u, expectedMapLayers);
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

                        if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                        {
                            locationHashes.Add(neighbour, newPathCost);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(loc, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates))
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

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, List<int>, bool> destinationValidityDelegate, Unit u, List<int> targetMapLayers, bool safeMove)
        {
            return getPathTo(loc, destinationValidityDelegate, null, null, u, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, List<int>, bool> destinationValidityDelegate, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, Unit u, List<int> targetMapLayers, bool safeMove)
        {
            return getPathTo(loc, destinationValidityDelegate, pathfindingDelegates, null, u, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, List<int>, bool> destinationValidityDelegate, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<int>, List<Location>>> getNeighboursDelegates, Unit u, List<int> targetMapLayers, bool safeMove)
        {
            if (targetMapLayers == null)
            {
                targetMapLayers = new List<int>();
            }

            if (destinationValidityDelegate(new Location[0], loc, u, targetMapLayers))
            {
                return new Location[0];
            }

            if (pathfindingDelegates == null)
            {
                pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>> { delegate_LAYERBOUND };
            }
            else if (!pathfindingDelegates.Contains(delegate_LAYERBOUND))
            {
                pathfindingDelegates.Add(delegate_LAYERBOUND);
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<int>, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
            }
            else if (!getNeighboursDelegates.Contains(delegate_NEIGHBOURS_VANILLA))
            {
                getNeighboursDelegates.Add(delegate_NEIGHBOURS_VANILLA);
            }

            List<Func<Location[], Location, Unit, List<int>, bool>> destinationValidityDelegates = new List<Func<Location[], Location, Unit, List<int>, bool>> { destinationValidityDelegate };

            if (targetMapLayers != null && targetMapLayers.Count > 0)
            {
                destinationValidityDelegates.Add(delegate_VALID_LAYERBOUND);
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
                
                if (u is UM)
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
                hook(loc, null, u, targetMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, null, u, targetMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }

            HashSet<Location> neighbours = new HashSet<Location>();
            for (int pass = 0; pass < 2; pass++)
            {
                Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { loc, 0.0 } };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { loc }, 0.0);

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    Location currentLocation = pair.Value[pair.Value.Length - 1];

                    bool isValid = true;
                    foreach (Func<Location[], Location, Unit, List<int>, bool> validityDelegtae in destinationValidityDelegates)
                    {
                        if (!validityDelegtae(pair.Value, currentLocation, u, targetMapLayers))
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        List<Location[]> destinationPaths = new List<Location[]>();
                        ValuePriorityPair<Location[], double> potentialPair;
                        while (paths.Count > 0)
                        {
                            potentialPair = paths.DequeueWithPriority();
                            if (potentialPair.Priority > pair.Priority)
                            {
                                break;
                            }

                            isValid = true;
                            Location currentPotentialLocation = potentialPair.Value[potentialPair.Value.Length - 1];
                            foreach (Func<Location[], Location, Unit, List<int>, bool> validityDelegtae in destinationValidityDelegates)
                            {
                                if (!validityDelegtae(pair.Value, currentPotentialLocation, u, targetMapLayers))
                                {
                                    isValid = false;
                                    break;
                                }
                            }
                            if (isValid)
                            {
                                destinationPaths.Add(potentialPair.Value);
                            }
                        }

                        if (destinationPaths.Count == 1)
                        {
                            return pair.Value;
                        }
                        else
                        {
                            return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                        }
                    }

                    if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                    {
                        continue;
                    }

                    neighbours.Clear();
                    foreach (var getNeighbourDelegate in getNeighboursDelegates)
                    {
                        List<Location> neighbourResults = getNeighbourDelegate(pair.Value, currentLocation, u, targetMapLayers);
                        if (neighbourResults != null)
                        {
                            foreach (Location neighbour in neighbourResults)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }

                    foreach (Location neighbour in neighbours)
                    {
                        double stepCost = 10.0;
                        foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                        {
                            double cost = pathfindingDelegate(pair.Value, neighbour, u, targetMapLayers);
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

                        if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                        {
                            locationHashes.Add(neighbour, newPathCost);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
                {
                    if (hook(loc, null, u, targetMapLayers, pathfindingDelegates, getNeighboursDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, null, u, targetMapLayers, pathfindingDelegates, getNeighboursDelegates))
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

        public static Dictionary<Location, Location[]> getPathsFrom(Location locA, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, Unit, List<int>, List<Location>>> getNeighboursDelegates, Unit u = null, bool safeMove = false)
        {
            if (locA == null)
            {
                return null;
            }

            Dictionary<Location, Location[]> allPaths = new Dictionary<Location, Location[]> { { locA, new Location[0] } };
            if (locA.getNeighbours().Count == 0)
            {
                return allPaths;
            }

            List<int> expectedMapLayers = new List<int> { locA.hex.z };

            if (pathfindingDelegates == null)
            {
                pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>> { delegate_LAYERBOUND };
            }
            else if (!pathfindingDelegates.Contains(delegate_LAYERBOUND))
            {
                pathfindingDelegates.Add(delegate_LAYERBOUND);
            }

            if (getNeighboursDelegates == null)
            {
                getNeighboursDelegates = new List<Func<Location[], Location, Unit, List<int>, List<Location>>> { delegate_NEIGHBOURS_VANILLA };
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

                if (u is UM)
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
                hook(locA, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(locA, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates);
            }

            Dictionary<Location, int> pathVariantCount = new Dictionary<Location, int>();
            HashSet<Location> neighbours = new HashSet<Location>();
            Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { locA, 0.0 } };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
            paths.Enqueue(new Location[1] { locA }, 0.0);

            int i = 0;
            while (i < 5 * locA.map.locations.Count && paths.Count > 0)
            {
                i++;

                ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                Location currentLocation = pair.Value[pair.Value.Length - 1];

                if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                {
                    continue;
                }

                neighbours.Clear();
                foreach (var getNeighbourDelegate in getNeighboursDelegates)
                {
                    List<Location> neighbourResults = getNeighbourDelegate(pair.Value, currentLocation, u, expectedMapLayers);
                    if (neighbourResults != null)
                    {
                        foreach (Location neighbour in neighbourResults)
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }

                foreach (Location neighbour in neighbours)
                {
                    double stepCost = 10.0;
                    foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                    {
                        double cost = pathfindingDelegate(pair.Value, neighbour, u, expectedMapLayers);
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

                    if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                    {
                        locationHashes.Add(neighbour, newPathCost);
                        allPaths.Add(neighbour, newPathArray);
                        pathVariantCount.Add(neighbour, 1);
                        paths.Enqueue(newPathArray, newPathCost);
                    }
                    else if (cachedPathCost > newPathCost)
                    {
                        if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            allPaths[neighbour] = newPathArray;
                            pathVariantCount[neighbour] = 1;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                            pathVariantCount[neighbour]++;
                            if (Eleven.random.Next(pathVariantCount[neighbour]) == 0)
                            {
                                allPaths[neighbour] = newPathArray;
                            }
                        }
                        
                    }
                }
            }

            bool allowPass = false;
            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfinding_AllowSecondPass)
            {
                if (hook(locA, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates))
                {
                    allowPass = true;
                }
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                if (hook.onPathfinding_AllowSecondPass(locA, null, u, expectedMapLayers, pathfindingDelegates, getNeighboursDelegates))
                {
                    allowPass = true;
                }
            }

            if (!allowPass)
            {
                return allPaths;
            }

            locationHashes.Clear();
            paths.Clear();
            paths.Enqueue(new Location[1] { locA }, 0.0);
            HashSet<Location> previouslyFound = allPaths.Keys.ToHashSet();

            i = 0;
            while (i < 5 * locA.map.locations.Count && paths.Count > 0)
            {
                i++;

                ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                Location currentLocation = pair.Value[pair.Value.Length - 1];

                if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                {
                    continue;
                }

                neighbours.Clear();
                foreach (var getNeighbourDelegate in getNeighboursDelegates)
                {
                    List<Location> neighbourResults = getNeighbourDelegate(pair.Value, currentLocation, u, expectedMapLayers);
                    if (neighbourResults != null)
                    {
                        foreach (Location neighbour in neighbourResults)
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }

                foreach (Location neighbour in neighbours)
                {
                    double stepCost = 10.0;
                    foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                    {
                        double cost = pathfindingDelegate(pair.Value, neighbour, u, expectedMapLayers);
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

                    if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                    {
                        locationHashes.Add(neighbour, newPathCost);
                        paths.Enqueue(newPathArray, newPathCost);

                        if (!previouslyFound.Contains(neighbour))
                        {
                            allPaths.Add(neighbour, newPathArray);
                            pathVariantCount.Add(neighbour, 1);
                        }
                    }
                    else if (cachedPathCost > newPathCost)
                    {
                        if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);

                            if (!previouslyFound.Contains(neighbour))
                            {
                                allPaths[neighbour] = newPathArray;
                                pathVariantCount[neighbour] = 1;
                            }
                        }
                        else
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);

                            if (!previouslyFound.Contains(neighbour))
                            {
                                pathVariantCount[neighbour]++;
                                if (Eleven.random.Next(pathVariantCount[neighbour]) == 0)
                                {
                                    allPaths[neighbour] = newPathArray;
                                }
                            }
                        }

                    }
                }
            }

            return allPaths;
        }

        #region Trade Route Pathfnding Delegates
        public static double delegate_TRADE_VANILLA(Location[] currentPath, Location location, List<int> targetMapLayers)
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
                if (location.settlement is Set_City || location.settlement is Set_DwarvenCity)
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

            // Even when not layerbound, there is preference to sticking to the layer or layers that the end points are on.
            if (targetMapLayers != null && targetMapLayers.Count > 0 && !targetMapLayers.Contains(location.hex.z))
            {
                result += 5.0;
            }

            return result;
        }

        public static double delegate_TRADE_REALISTIC(Location[] currentPath, Location location, List<int> targetMapLayers)
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
                    else if (location.settlement is Set_City || location.settlement is Set_DwarvenCity)
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

            // Even when not layerbound, there is preference to sticking to the layer or layers that the end points are on.
            if (targetMapLayers != null && targetMapLayers.Count > 0 && !targetMapLayers.Contains(location.hex.z))
            {
                result += 5.0;
            }

            return result;
        }

        public static double delegate_TRADE_LAYERBOUND(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            if (targetMapLayers == null || targetMapLayers.Count == 0 || targetMapLayers.Contains(location.hex.z))
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_TRADE_UNDERGROUNDAWARENESS(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            Location start = currentPath[0];
            if (start.map.awarenessOfUnderground < 1.0)
            {
                if (start.map.overmind.god is God_Mammon && start.settlement is Set_TombOfGods)
                {
                    return 0.0;
                }

                if ((start.hex.z == 0 && location.hex.z == 1) || (start.hex.z == 1 && location.hex.z == 0))
                {
                    //Console.WriteLine($"CommunityLib: Location {location.getName()} ({location.hex.z}) violates Underground Awareness rules for trade route originating from {currentPath[0].getName()} ({currentPath[0].hex.z})");
                    return 10000.0;
                }
            }

            return 0.0;
        }
        #endregion

        #region Mod-Specific TRade Route Pathfinding Delegates
        public static double delegate_TRADE_LIVINGVOID_AVOIDVOID(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            if (location.map.tempMap[location.hex.x][location.hex.y] < -999f)
            {
                return -10000.0;
            }

            return 0.0;
        }
        #endregion

        #region Trade Route Validity Delegates
        public static bool delegate_TRADEVALID_NODUPLICATES(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            return !location.map.tradeManager.routes.Any(r => (r.start() == currentPath[0] && r.end() == location) || (r.start() == location && r.end() == currentPath[0]));
        }

        public static bool delegate_TRADEVALID_MERGEREGIONS(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            return ModCore.Get().tradeRouteManager.routeData.indexGroups.Any(ig => ig.Contains(location.index) && !ig.Contains(currentPath[0].index));
        }

        public static bool delegate_TRADEVALID_LAYERBOUND(Location[] currentPath, Location location, List<int> targetMapLayers)
        {
            return targetMapLayers == null || targetMapLayers.Count == 0 || targetMapLayers.Contains(location.hex.z);
        }
        #endregion

        public static Location[] getTradeRouteTo(Location start, Location end)
        {
            return getTradeRouteTo(start, end, null);
        }

        public static Location[] getTradeRouteTo(Location start, Location end, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates)
        {
            if (start == null || end == null)
            {
                return null;
            }

            if (start == end)
            {
                return new Location[0];
            }

            List<int> endPointMapLayers = new List<int> { end.hex.z };
            List<int> expectedMapLayers = new List<int> { start.hex.z };
            if (!expectedMapLayers.Contains(end.hex.z))
            {
                expectedMapLayers.Add(end.hex.z);
            }

            if (pathfindingDelegates == null)
            {
                if (ModCore.opt_realisticTradeRoutes)
                {
                    pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_REALISTIC, delegate_TRADE_LAYERBOUND, delegate_TRADE_UNDERGROUNDAWARENESS };
                }
                else
                {
                    pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_VANILLA, delegate_TRADE_LAYERBOUND, delegate_TRADE_UNDERGROUNDAWARENESS };
                }
            }
            else
            {
                if (!pathfindingDelegates.Contains(delegate_TRADE_LAYERBOUND))
                {
                    pathfindingDelegates.Add(delegate_TRADE_LAYERBOUND);
                }

                if (!pathfindingDelegates.Contains(delegate_TRADE_UNDERGROUNDAWARENESS))
                {
                    pathfindingDelegates.Add(delegate_TRADE_UNDERGROUNDAWARENESS);
                }

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
            List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates = new List<Func<Location[], Location, List<int>, bool>> { delegate_TRADEVALID_LAYERBOUND, delegate_TRADEVALID_NODUPLICATES };

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingTradeRoutePathfindingDelegates)
            {
                hook(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
            }

            for (int pass = 0; pass < 2; pass++)
            {
                Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { start, 0.0 } };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { start }, 0.0);

                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    Location currentLocation = pair.Value[pair.Value.Length - 1];

                    if (currentLocation == end)
                    {
                        bool isValid = true;
                        foreach (Func<Location[], Location, List<int>, bool> validityDelegate in destinationValidityDelegates)
                        {
                            if (!validityDelegate(pair.Value, currentLocation, expectedMapLayers))
                            {
                                isValid = false;
                            }
                        }

                        if (isValid)
                        {
                            return pair.Value;
                        }
                    }

                    if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                    {
                        continue;
                    }

                    foreach (Location neighbour in pair.Value[pair.Value.Length - 1].getNeighbours())
                    {
                        double stepCost = 0.0;
                        foreach (Func<Location[], Location, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                        {
                            double cost = pathfindingDelegate(pair.Value, neighbour, endPointMapLayers);
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

                        if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                        {
                            locationHashes.Add(neighbour, newPathCost);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }
                }

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfindingTadeRoute_AllowSecondPass)
                {
                    hook(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfindingTadeRoute_AllowSecondPass(start, endPointMapLayers, pathfindingDelegates, destinationValidityDelegates))
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
            int[] layers = new int[0];
            return getTradeRouteFrom(start, null, endpointsAll);
        }

        public static Location[] getTradeRouteFrom(Location start, int endPointMapLayer, List<Location> endpointsAll = null)
        {
            if (endPointMapLayer < 0)
            {
                return getTradeRouteFrom(start, null, endpointsAll);
            }
            else
            {
                return getTradeRouteFrom(start, new List<int> { endPointMapLayer }, endpointsAll);
            }
        }

        public static Location[] getTradeRouteFrom(Location start, List<int> endPointMapLayers, List<Location> endpointsAll = null)
        {
            return getTradeRouteFrom(start, null, null, endPointMapLayers, endpointsAll);
        }

        public static Location[] getTradeRouteFrom(Location start, List<Func<Location[], Location, List<int>, double>> pathfindingDelegates, List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates, List<int> endPointMapLayers, List<Location> endpointsAll = null)
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

            List<int> expectedMapLayers = new List<int>();
            if (endPointMapLayers != null && endPointMapLayers.Count == 0)
            {
                expectedMapLayers.Add(start.hex.z);
                foreach (Location endpoint in endpointsAll)
                {
                    if (!expectedMapLayers.Contains(endpoint.hex.z))
                    {
                        expectedMapLayers.Add(endpoint.hex.z);
                    }
                }
            }

            if (pathfindingDelegates == null)
            {
                if (ModCore.opt_realisticTradeRoutes)
                {
                    pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_REALISTIC, delegate_TRADE_LAYERBOUND, delegate_TRADE_UNDERGROUNDAWARENESS };
                }
                else
                {
                    pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_VANILLA, delegate_TRADE_LAYERBOUND, delegate_TRADE_UNDERGROUNDAWARENESS };
                }
            }
            else
            {
                if (!pathfindingDelegates.Contains(delegate_TRADE_LAYERBOUND))
                {
                    pathfindingDelegates.Add(delegate_TRADE_LAYERBOUND);
                }

                if (!pathfindingDelegates.Contains(delegate_TRADE_UNDERGROUNDAWARENESS))
                {
                    pathfindingDelegates.Add(delegate_TRADE_UNDERGROUNDAWARENESS);
                }

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
                destinationValidityDelegates = new List<Func<Location[], Location, List<int>, bool>> { delegate_TRADEVALID_LAYERBOUND, delegate_TRADEVALID_NODUPLICATES };
            }
            else
            {
                if (!destinationValidityDelegates.Contains(delegate_TRADEVALID_LAYERBOUND))
                {
                    destinationValidityDelegates.Add(delegate_TRADEVALID_LAYERBOUND);
                }

                if (!destinationValidityDelegates.Contains(delegate_TRADEVALID_NODUPLICATES))
                {
                    destinationValidityDelegates.Add(delegate_TRADEVALID_NODUPLICATES);
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingTradeRoutePathfindingDelegates)
            {
                hook(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
            }

            for (int pass = 0; pass < 2; pass++)
            {
                Dictionary<Location, double> locationHashes = new Dictionary<Location, double> { { start, 0.0 } };
                PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
                paths.Enqueue(new Location[] { start }, 0.0);

                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    Location currentLocation = pair.Value[pair.Value.Length - 1];

                    if (endpointsAll.Contains(currentLocation))
                    {
                        bool isValid = true;
                        foreach (Func<Location[], Location, List<int>, bool> validityDelegate in destinationValidityDelegates)
                        {
                            if (!validityDelegate(pair.Value, currentLocation, expectedMapLayers))
                            {
                                isValid = false;
                                break;
                            }
                        }

                        if (isValid)
                        {
                            List<Location[]> destinationPaths = new List<Location[]> { pair.Value };
                            ValuePriorityPair<Location[], double> potentialPair;
                            while (paths.Count > 0)
                            {
                                potentialPair = paths.DequeueWithPriority();
                                if (potentialPair.Priority > pair.Priority)
                                {
                                    break;
                                }

                                isValid = true;
                                Location potentialCurrentLocation = potentialPair.Value[potentialPair.Value.Length - 1];
                                foreach (Func<Location[], Location, List<int>, bool> validityDelegate in destinationValidityDelegates)
                                {
                                    if (!validityDelegate(pair.Value, potentialCurrentLocation, expectedMapLayers))
                                    {
                                        isValid = false;
                                        break;
                                    }
                                }
                                if (isValid)
                                {
                                    destinationPaths.Add(potentialPair.Value);
                                }
                            }

                            if (destinationPaths.Count == 1)
                            {
                                return pair.Value;
                            }
                            else
                            {
                                return destinationPaths[Eleven.random.Next(destinationPaths.Count)];
                            }
                            
                        }
                    }

                    if (!locationHashes.TryGetValue(currentLocation, out double cachedPathCost) || pair.Priority > cachedPathCost)
                    {
                        continue;
                    }

                    foreach (Location neighbour in pair.Value[pair.Value.Length - 1].getNeighbours())
                    {
                        double stepCost = 0.0;
                        foreach (Func<Location[], Location, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                        {
                            double cost = pathfindingDelegate(pair.Value, neighbour, endPointMapLayers);
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

                        if (!locationHashes.TryGetValue(neighbour, out cachedPathCost))
                        {
                            locationHashes.Add(neighbour, newPathCost);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                        else if (cachedPathCost > newPathCost)
                        {
                            locationHashes[neighbour] = newPathCost;
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }
                }

                //World.Log($"CommunityLib: getTradeRouteFrom {start.getName()} failed after {i}/{5 * start.map.locations.Count} iterations.");

                bool allowPass = false;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfindingTadeRoute_AllowSecondPass)
                {
                    hook(start, expectedMapLayers, pathfindingDelegates, destinationValidityDelegates);
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfindingTadeRoute_AllowSecondPass(start, endPointMapLayers, pathfindingDelegates, destinationValidityDelegates))
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
