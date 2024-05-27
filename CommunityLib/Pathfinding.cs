using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Code;

namespace CommunityLib
{
    public static class Pathfinding
    {
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
            if (location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY)
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

            List<Func<Location[], Location, Unit, double>>  pathfindingDelegates = new List<Func<Location[], Location, Unit, double>>();

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
                hook.onPopulatingPathfindingDelegates(locA, u, pathfindingDelegates);
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
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u);
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

            List<Func<Location[], Location, Unit, double>> pathfindingDelegates = new List<Func<Location[], Location, Unit, double>>();

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
                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u);
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
                            ItemPriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPair.Priority == destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Item);
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

        public static Location[] getPathTo(Location loc, Func<Location[], Location, Unit, bool> destinationValidityDelegate, Unit u, bool safeMove)
        {
            if (destinationValidityDelegate(new Location[0], loc, u))
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, double>> pathfindingDelegates = new List<Func<Location[], Location, Unit, double>>();
            List<Func<Location[], Location, Unit, bool>> destinationValidityDelegates = new List<Func<Location[], Location, Unit, bool>> { destinationValidityDelegate };

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

            paths.Enqueue(new Location[] { loc }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * loc.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in getNeighboursConditional(pair.Item[pair.Item.Length - 1], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 10.0;
                            foreach (Func<Location[], Location, Unit, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour, u);
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
                            foreach (Func<Location[], Location, Unit, bool> validDelegate in destinationValidityDelegates)
                            {
                                if (!destinationValidityDelegate(pair.Item, neighbour, u))
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
                            ItemPriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPair.Priority <= destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Item);
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
                if (location.isOcean)
                {
                    if (locLast.settlement is Set_City && locLast.settlement.subs.Any(sub => sub is Sub_Docks))
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
                if (location.settlement is Set_City)
                {
                    if (locLast.isOcean && location.settlement.subs.Any(sub => sub is Sub_Docks))
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

            return result;
        }

        public static bool delegate_TRADEVALID_NODUPLICATES(Location[] currentPath, Location location)
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

            // Location[] currentPath, Location location, int[] endPointMapLayers, Location Start, return double stepCost
            List<Func<Location[], Location, double>> pathfindingDelegates = new List<Func<Location[], Location, double>>();
            if (ModCore.opt_realisticTradeRoutes)
            {
                pathfindingDelegates.Add(delegate_TRADE_REALISTIC);
            }
            else
            {
                pathfindingDelegates.Add(delegate_TRADE_VANILLA);
            }
            // Location[] currentPath, Location location, int[] endPointMapLayers,, Location Start, return bool destinationValid
            List<Func<Location[], Location, bool>> destinationValidityDelegates = new List<Func<Location[], Location, bool>> { delegate_TRADEVALID_NODUPLICATES };

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
                    foreach (Location neighbour in pair.Item[pair.Item.Length - 1].getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour);
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

            // Location[] currentPath, Location location, int[] endPointMapLayers, Location Start, return double stepCost
            List<Func<Location[], Location, double>> pathfindingDelegates = new List<Func<Location[], Location, double>>();
            if (ModCore.opt_realisticTradeRoutes)
            {
                pathfindingDelegates.Add(delegate_TRADE_REALISTIC);
            }
            else
            {
                pathfindingDelegates.Add(delegate_TRADE_VANILLA);
            }
            // Location[] currentPath, Location location, int[] endPointMapLayers,, Location Start, return bool destinationValid
            List<Func<Location[], Location, bool>> destinationValidityDelegates = new List<Func<Location[], Location, bool>> { delegate_TRADEVALID_NODUPLICATES };

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingTradeRoutePathfindingDelegates(start, pathfindingDelegates, destinationValidityDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { start };
            PriorityQueue<Location[], double> paths = new PriorityQueue<Location[], double>();

            paths.Enqueue(new Location[] { start }, 0.0);

            for (int pass = 0; pass < 2; pass++)
            {
                PriorityQueue<Location[], double> destinations = new PriorityQueue<Location[], double>();
                double destinationPriority = -1.0;

                int i = 0;
                while (i < 5 * start.map.locations.Count && paths.Count > 0)
                {
                    i++;

                    ItemPriorityPair<Location[], double> pair = paths.DequeueWithPriority();
                    foreach (Location neighbour in pair.Item[pair.Item.Length - 1].getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            double stepCost = 0.0;
                            foreach (Func<Location[], Location, double> pathfindingDelegate in pathfindingDelegates)
                            {
                                double cost = pathfindingDelegate(pair.Item, neighbour);
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
                                foreach(Func<Location[], Location, bool> validDelegate in destinationValidityDelegates)
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
                                    destinations.Enqueue(new ItemPriorityPair<Location[], double>(newPathArray, newPathCost));
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
                            ItemPriorityPair<Location[], double> destinationPair = destinations.DequeueWithPriority();
                            if (destinationPriority == -1.0 || destinationPair.Priority == destinationPriority)
                            {
                                destinationPaths.Add(destinationPair.Item);
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
                    if (location.settlement is Set_City)
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
