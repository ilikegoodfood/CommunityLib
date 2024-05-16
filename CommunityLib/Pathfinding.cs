﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;

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
            if (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY)
            {
                return 0.0;
            }
            return 10000.0;
        }

        public static double delegate_FAVOURABLE_WIND(Location[] currentPath, Location location, Unit u, List<int> targetMapLayers)
        {
            if (u != null && u.isCommandable() && location.isOcean)
            {
                return -1.0;
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

        public static Location[] getPathTo(Location locA, Location locB, Unit u = null, bool safeMove = false)
        {
            if (locA == null || locB == null)
            {
                return null;
            }

            if (locA == locB)
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, List<int>, double>>  pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>>();

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    //Console.WriteLine("CommunityLib: Added Aquaphibious delegate");
                    pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    //Console.WriteLine("CommunityLib: Added Desert only delegate");
                    pathfindingDelegates.Add(delegate_DESERT_ONLY);
                }

                if (safeMove)
                {
                    //Console.WriteLine("CommunityLib: Added safe move delegate");
                    pathfindingDelegates.Add(delegate_SAFE_MOVE);
                }
                else if (u is UM)
                {
                    pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                }
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(locA, u, expectedMapLayers, pathfindingDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { locA };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();
            paths.Enqueue(new Location[1] { locA }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                int i = 0;
                while (i < 5 * locA.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u, expectedMapLayers);
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

                            Location[] newPathArray = new Location[pair.Item.Length + 1];
                            Array.Copy(pair.Item, newPathArray, pair.Item.Length);
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
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(locA, u, pathfindingDelegates))
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
            if (loc == null)
            {
                return null;
            }

            if (loc.soc == sg)
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>>();

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    pathfindingDelegates.Add(delegate_DESERT_ONLY);
                }

                if (safeMove)
                {
                    pathfindingDelegates.Add(delegate_SAFE_MOVE);
                }
                else if (u is UM)
                {
                    pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                }
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, u, pathfindingDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { loc };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();

            paths.Enqueue(new Location[1] { loc }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u, expectedMapLayers);
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

                            Location[] newPathArray = new Location[pair.Item.Length + 1];
                            Array.Copy(pair.Item, newPathArray, pair.Item.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            if (neighbour.soc == sg)
                            {
                                destinations.Enqueue(newPathArray, newPathCost);
                            }

                            locationHashes.Add(neighbour);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }

                    if (destinations.Count > 0)
                    {
                        return destinations.Dequeue();
                    }
                }

                bool allowPass = false;
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, u, pathfindingDelegates))
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
            if (targetMapLayers == null)
            {
                targetMapLayers = new List<int>();
            }

            if (destinationValidityDelegate(new Location[0], loc, u, targetMapLayers))
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, List<int>, double>> pathfindingDelegates = new List<Func<Location[], Location, Unit, List<int>, double>>();
            List<Func<Location[], Location, Unit, List<int>, bool>> destinationValidityDelegates = new List<Func<Location[], Location, Unit, List<int>, bool>> { destinationValidityDelegate };

            if (u != null)
            {
                if (u is UA && u.isCommandable())
                {
                    pathfindingDelegates.Add(delegate_FAVOURABLE_WIND);
                }

                if (u.moveType == Unit.MoveType.AQUAPHIBIOUS)
                {
                    pathfindingDelegates.Add(delegate_AQUAPHIBIOUS);
                }
                else if (u.moveType == Unit.MoveType.DESERT_ONLY)
                {
                    pathfindingDelegates.Add(delegate_DESERT_ONLY);
                }

                if (safeMove)
                {
                    pathfindingDelegates.Add(delegate_SAFE_MOVE);
                }
                else if (u is UM)
                {
                    pathfindingDelegates.Add(delegate_AVOID_TRESSPASS);
                }
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates(loc, u, targetMapLayers, pathfindingDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { loc };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();

            paths.Enqueue(new Location[1] { loc }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u, targetMapLayers);
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

                            Location[] newPathArray = new Location[pair.Item.Length + 1];
                            Array.Copy(pair.Item, newPathArray, pair.Item.Length);
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            double newPathCost = pair.Priority + stepCost;

                            bool valid = true;
                            foreach (Func<Location[], Location, Unit, int[], bool> validDelegate in destinationValidityDelegates)
                            {
                                if (!destinationValidityDelegate(pair.Item, neighbour, u, targetMapLayers))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (valid)
                            {
                                destinations.Enqueue(newPathArray, newPathCost);
                            }

                            locationHashes.Add(neighbour);
                            paths.Enqueue(newPathArray, newPathCost);
                        }
                    }

                    if (destinations.Count > 0)
                    {
                        return destinations.Dequeue();
                    }
                }

                bool allowPass = false;
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    if (hook.onPathfinding_AllowSecondPass(loc, u, pathfindingDelegates))
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
                        Location homeLoc = u.map.locations[u.homeLocation];
                        if (!result.Contains(homeLoc))
                        {
                            result.Add(homeLoc);
                        }
                    }
                }
            }

            return result;
        }

        public static double delegate_TRADE_VANILLA(Location[] currentPath, Location location, List<int> endPointMapLayers)
        {
            double result = 0.0;
            if (ModCore.opt_realisticTradeRoutes)
            {
                if (location.soc == null)
                {
                    if (location.isOcean)
                    {
                        if ((currentPath[currentPath.Length - 1].settlement is Set_City || currentPath[currentPath.Length - 1].settlement is Set_DwarvenCity) && currentPath[currentPath.Length - 1].settlement.subs.Any(sub => sub is Sub_Docks))
                        {
                            result += 2.5;
                        }
                        else
                        {
                            result += 7.5;
                        }
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
                        if (currentPath[currentPath.Length - 1].isOcean && location.settlement.subs.Any(sub => sub is Sub_Docks))
                        {
                            result += 2.5;
                        }
                        else if (location.settlement.subs.Any(sub => sub is Sub_Market))
                        {
                            result += 2.5;
                        }
                        else
                        {
                            result += 5.0;
                        }
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
            }
            else
            {
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
            }

            return result;
        }

        public static bool delegate_TRADEVALID_NODUPLICATES(Location[] currentPath, Location location, List<int> endPointMapLayers)
        {
            return !location.map.tradeManager.routes.Any(r => (r.start() == currentPath[0] && r.end() == location) || (r.start() == location && r.end() == currentPath[0]));
        }

        public static Location[] getTradeRouteTo(Location start, Location end)
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

            // Location[] currentPath, Location location, int[] endPointMapLayers, Location Start, return double stepCost
            List<Func<Location[], Location, List<int>, double>> pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_VANILLA, delegate_TRADE_LAYERBOUND, delegate_TRADE_UNDERGROUNDAWARENESS };
            // Location[] currentPath, Location location, int[] endPointMapLayers,, Location Start, return bool destinationValid
            List<Func<Location[], Location, List<int>, bool>> destinationValidityDelegates = new List<Func<Location[], Location, List<int>, bool>> { delegate_TRADEVALID_LAYERBOUND, delegate_TRADEVALID_NODUPLICATES };

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, endPointMapLayers, pathfindingDelegates, destinationValidityDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { start };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();

            paths.Enqueue(new Location[] { start }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], null))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, endPointMapLayers);
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

                            Location[] newPathArray = new Location[pair.Item.Length + 1];
                            Array.Copy(pair.Item, newPathArray, pair.Item.Length);
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
            if (start == null)
            {
                return null;
            }

            if (endpointsAll == null || endpointsAll.Count == 0)
            {
                endpointsAll = getTradeRouteEndPoints(start.map);
            }

            if (endpointsAll.Count < 2)
            {
                return null;
            }

            // Location[] currentPath, Location location, Location Start, return double stepCost
            List<Func<Location[], Location, double>> pathfindingDelegates = new List<Func<Location[], Location, List<int>, double>> { delegate_TRADE_VANILLA };
            // Location[] currentPath, Location location, Location Start, return bool destinationValid
            List<Func<Location[], Location, bool>> destinationValidityDelegates = new List<Func<Location[], Location, List<int>, bool>> { delegate_TRADEVALID_NODUPLICATES };

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, pathfindingDelegates, destinationValidityDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { start };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();

            paths.Enqueue(new Location[] { start }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], null))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, List<int>, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, endPointMapLayers);
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

                            Location[] newPathArray = new Location[pair.Item.Length + 1];
                            Array.Copy(pair.Item, newPathArray, pair.Item.Length);
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
                                    destinations.Enqueue(new ItemPriorityPair<Location[], double>(newPathArray, newPathCost));
                                }
                            }

                            paths.Enqueue(newPathArray, newPathCost);
                            locationHashes.Add(neighbour);
                        }
                    }

                    if (destinations.Count > 0)
                    {
                        return destinations.Dequeue();
                    }
                }

                bool allowPass = false;
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

        internal static List<Location> getTradeRouteEndPoints(Map map)
        {
            List<Location> endPoints = new List<Location>();

            foreach (Location location in map.locations)
            {
                if (map.overmind.god is God_Mammon && location.settlement is Set_TombOfGods)
                {
                    endPoints.Add(location);
                    continue;
                }

                if (location.soc is Society society && location.index == society.capital && !society.isDarkEmpire && !society.isOphanimControlled)
                {
                    if (location.settlement is Set_City || location.settlement is Set_DwarvenCity)
                    {
                        endPoints.Add(location);
                        continue;
                    }
                }
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onGetTradeRouteEndpoints(map, endPoints);
            }

            return endPoints;
        }
    }
}
