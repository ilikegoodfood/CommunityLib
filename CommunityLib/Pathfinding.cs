﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;
using UnityEngine;

namespace CommunityLib
{
    public static class Pathfinding
    {
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
            if (u == null || location.soc == null || !location.soc.hostileTo(u))
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

        public static bool delegate_VALID_LAYERBOUND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            return targetMapLayers == null || targetMapLayers.Count == 0 || targetMapLayers.Contains(location.hex.z);
        }

        public static Location[] getPathTo(Location locA, Location locB, Unit u = null, bool safeMove = false)
        {
            return getPathTo(locA, locB, null, u, safeMove);
        }

        public static Location[] getPathTo(Location locA, Location locB, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, Unit u = null, bool safeMove = false)
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

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    if (!pathfindingDelegates.Contains(delegate_FAVOURABLE_WIND))
                    {
                        pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
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
                hook(locA, u, expectedMapLayers, pathfindingDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(locA, u, expectedMapLayers, pathfindingDelegates);
            }

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
                    foreach (Location neighbour in getNeighboursConditional(pair.Value[pair.Value.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
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
                    if (hook(locA, u, expectedMapLayers, pathfindingDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(locA, u, expectedMapLayers, pathfindingDelegates))
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
            return getPathTo(loc, sg, u, null, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, int targetMapLayer, bool safeMove = false)
        {
            if (targetMapLayer < 0)
            {
                return getPathTo(loc, sg, u, null, safeMove);
            }
            else
            {
                return getPathTo(loc, sg, u, new List<int> { targetMapLayer }, safeMove);
            }
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<int> targetMapLayers, bool safeMove = false)
        {
            return getPathTo(loc, sg, u, null, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, SocialGroup sg, Unit u, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, List<int> targetMapLayers, bool safeMove = false)
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

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    if (!pathfindingDelegates.Contains(delegate_FAVOURABLE_WIND))
                    {
                        pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
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
                hook(loc, u, expectedMapLayers, pathfindingDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, u, expectedMapLayers, pathfindingDelegates);
            }

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
                    i++;

                    ValuePriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in getNeighboursConditional(pair.Value[pair.Value.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
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

                            if (neighbour.soc == sg && delegate_VALID_LAYERBOUND(newPathArray, neighbour, u, targetMapLayers))
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
                    if (hook(loc, u, expectedMapLayers, pathfindingDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, u, expectedMapLayers, pathfindingDelegates))
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
            return getPathTo(loc, destinationValidityDelegate, null, u, targetMapLayers, safeMove);
        }

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, List<int>, bool> destinationValidityDelegate, List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates, Unit u, List<int> targetMapLayers, bool safeMove)
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
                hook(loc, u, targetMapLayers, pathfindingDelegates);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, u, targetMapLayers, pathfindingDelegates);
            }

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
                    foreach (Location neighbour in getNeighboursConditional(pair.Value[pair.Value.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
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

                            bool valid = true;
                            foreach (Func<Location[], Location, Unit, List<int>, bool> validDelegate in destinationValidityDelegates)
                            {
                                if (!destinationValidityDelegate(pair.Value, neighbour, u, targetMapLayers))
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
                    if (hook(loc, u, targetMapLayers, pathfindingDelegates))
                    {
                        allowPass = true;
                    }
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, u, targetMapLayers, pathfindingDelegates))
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

        internal static List<Location> getNeighboursConditional(Location loc, Unit u)
        {
            List<Location> result = loc.getNeighbours();

            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (loc.settlement is Set_MinorOther && loc.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    foreach (Location location in u.map.locations)
                    {
                        if (ModCore.Get().checkIsElderTomb(location) && !result.Contains(location))
                        {
                            result.Add(location);
                        }
                    }

                    if (u.homeLocation != -1)
                    {
                        Location home = u.map.locations[u.homeLocation];
                        if (!result.Contains(home))
                        {
                            result.Add(home);
                        }
                    }
                }
            }

            return result;
        }

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

                            if (neighbour == end)
                            {
                                bool valid = true;
                                foreach (Func<Location[], Location, List<int>, bool> validDelegate in destinationValidityDelegates)
                                {
                                    if (!validDelegate(newPathArray, neighbour, endPointMapLayers))
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

                            if (endpointsAll.Contains(neighbour))
                            {
                                bool valid = true;
                                foreach(Func<Location[], Location, List<int>, bool> validDelegate in destinationValidityDelegates)
                                {
                                    if (!validDelegate(newPathArray, neighbour, endPointMapLayers))
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
