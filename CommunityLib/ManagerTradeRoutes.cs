using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class ManagerTradeRoutes
    {
        public readonly Map map;

        public readonly TradeRouteData routeData = new TradeRouteData();

        public ManagerTradeRoutes(Map map)
        {
            this.map = map;
        }

        public void checkTradeNetwork(ManagerTrade tradeManager)
        {
            List<Location> endpoints = getTradeRouteEndPoints();

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onGetTradeRouteEndpoints)
            {
                hook(tradeManager.map, endpoints);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onGetTradeRouteEndpoints(tradeManager.map, endpoints);
            }

            bool dirty = false;
            if (endpoints.Count != tradeManager.cached.Count)
            {
                dirty = true;
            }
            else
            {
                for (int i = 0; i < endpoints.Count; i++)
                {
                    if (endpoints[i].index != tradeManager.cached[i])
                    {
                        dirty = true;
                    }
                }
            }

            if (dirty)
            {
                rebuildTradeRoutes(tradeManager, endpoints);

                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook.onBuildTradeNetwork_EndOfProcess(tradeManager.map, tradeManager, endpoints);
                }
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onBuildTradeNetwork_EndOfProcess)
                {
                    hook(tradeManager.map, tradeManager, endpoints);
                }

                tradeManager.tradeDensity = new List<TradeRoute>[tradeManager.map.locations.Count];
                foreach (TradeRoute tradeRoute in tradeManager.routes)
                {
                    foreach (Location location in tradeRoute.path)
                    {
                        if (tradeManager.tradeDensity[location.index] == null)
                        {
                            tradeManager.tradeDensity[location.index] = new List<TradeRoute>();
                        }
                        tradeManager.tradeDensity[location.index].Add(tradeRoute);
                    }
                }
            }

            foreach (TradeRoute route in tradeManager.routes)
            {
                route.turnTick(tradeManager.map);
            }
        }

        private void rebuildTradeRoutes(ManagerTrade tradeManager, List<Location> endpoints)
        {
            ModCore.Get().hooks.DensifyingTradeRoutes = false;
            if (endpoints == null || endpoints.Count < 2)
            {
                endpoints = getTradeRouteEndPoints();
            }

            tradeManager.cached.Clear();
            foreach (Location endpoint in endpoints)
            {
                tradeManager.cached.Add(endpoint.index);
            }

            foreach (TradeRoute route in tradeManager.routes.ToList())
            {
                if (!endpoints.Contains(route.start()) || !endpoints.Contains(route.end()))
                {
                    World.log($"CommunityLib: Trade route from {route.start().getName()} to {route.end().getName()} is no longer valid. One or both ends are no longer valid destinations for a trade route. Removed.");
                    tradeManager.routes.Remove(route);
                }
            }

            List<HashSet<int>> connectedSets = findAllConnectedSets(tradeManager);
            List<Location> endpointsConected = new List<Location>();
            List<Location> endpointsDisconnected = endpoints.ToList();
            foreach (HashSet<int> connectedSet in connectedSets)
            {
                foreach (int index in connectedSet)
                {
                    Location loc = tradeManager.map.locations[index];
                    if (!endpointsConected.Contains(loc))
                    {
                        endpointsConected.Add(loc);
                        endpointsDisconnected.Remove(loc);
                    }
                }
            }

            World.log($"CommunityLib: Found {endpointsDisconnected.Count} endpoints for trade routes that are not connected to a trade route.");
            while (endpointsDisconnected.Count > 0)
            {
                Location[] routePath = Pathfinding.getTradeRouteFrom(endpointsDisconnected[0], endpoints);
                if (routePath == null || routePath.Length < 2)
                {
                    World.log($"CommunityLib: Failed to find Trade Route from {endpointsDisconnected[0].getName()} to another endpoint. Isolated endpoint detected.");
                    endpointsDisconnected.RemoveAt(0);
                    continue;
                }

                tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                World.log($"CommunityLib: Trade Route created from {endpointsDisconnected[0].getName()} to {routePath[routePath.Length - 1].getName()}.");
                endpointsDisconnected.RemoveAt(0);
                endpointsDisconnected.Remove(routePath[routePath.Length - 1]);
            }

            connectedSets = findAllConnectedSets(tradeManager);

            List<List<int>> indexGroups = routeData.indexGroups;
            indexGroups.Clear();
            foreach (HashSet<int> set in connectedSets)
            {
                indexGroups.Add(set.ToList());
            }

            int i = 0;
            while (indexGroups.Count > 1 && i < 5 * tradeManager.map.locations.Count)
            {
                i++;

                List<int> indexGroup = indexGroups[0];
                foreach (List<int> group in indexGroups)
                {
                    if (group.Count < indexGroup.Count)
                    {
                        indexGroup = group;
                    }
                }

                bool connectionInvalid = true;
                foreach (List<int> otherGroup in indexGroups)
                {
                    if (indexGroup == otherGroup)
                    {
                        continue;
                    }

                    Location[] routePath = Pathfinding.getTradeRouteTo(tradeManager.map.locations[indexGroup[0]], tradeManager.map.locations[otherGroup[0]]);
                    if (routePath == null || routePath.Length < 2)
                    {
                        World.log($"CommunityLib: Trade Route from set {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to {tradeManager.map.locations[otherGroup[0]].getName()} (size {otherGroup.Count}) is not possible. Checking against next group...");
                        continue;
                    }
                    connectionInvalid = false;
                    break;
                }

                if (connectionInvalid)
                {
                    World.log($"CommunityLib: Failed to find Trade Route from indexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to another index group. Isolated group detected.");
                    indexGroups.Remove(indexGroup);
                    continue;
                }

                World.log($"CommunityLib: Finding cheapest Trade Route from index group {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) that connects to another index group.");

                List<Location[]> bestRoutePaths = new List<Location[]>();
                double routeCost = 0.0;
                foreach (int index in indexGroup)
                {
                    Location loc = tradeManager.map.locations[indexGroup[Eleven.random.Next(indexGroup.Count)]];
                    Location[] routePath = Pathfinding.getTradeRouteFrom(loc, null, new List<Func<Location[], Location, bool>> { Pathfinding.delegate_TRADEVALID_MERGEREGIONS }, endpoints);
                    if (routePath == null || routePath.Length < 2)
                    {
                        World.log($"CommunityLib: Failed to find Trade Route from {tradeManager.map.locations[index].getName()} in indexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}). Isolated group detected.");
                        bestRoutePaths.Clear();
                        indexGroups.Remove(indexGroup);
                        break;
                    }

                    // Setup delegates list to exactly mimic the  original pathfinding function so that they are indistinguisable to the the hook.
                    List<Func<Location[], Location, double>> pathfindingDelegates;
                    if (ModCore.opt_realisticTradeRoutes)
                    {
                        pathfindingDelegates = new List<Func<Location[], Location, double>> { Pathfinding.delegate_TRADE_REALISTIC };
                    }
                    else
                    {
                        pathfindingDelegates = new List<Func<Location[], Location, double>> { Pathfinding.delegate_TRADE_VANILLA };
                    }

                    List<Func<Location[], Location, bool>> destinationValidityDelegates = new List<Func<Location[], Location, bool>> { Pathfinding.delegate_TRADEVALID_NODUPLICATES, Pathfinding.delegate_TRADEVALID_MERGEREGIONS };

                    foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPopulatingTradeRoutePathfindingDelegates)
                    {
                        hook(routePath[0], pathfindingDelegates, destinationValidityDelegates);
                    }
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        hook.onPopulatingTradeRoutePathfindingDelegates(routePath[0], pathfindingDelegates, destinationValidityDelegates);
                    }

                    // Since a second pass may have been required by the pathfinding system, assumethat one has for the purposes of calculating the cost.
                    foreach (var hook in ModCore.Get().HookRegistry.Delegate_onPathfindingTadeRoute_AllowSecondPass)
                    {
                        hook(routePath[0], pathfindingDelegates, destinationValidityDelegates);
                    }
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        hook.onPathfindingTadeRoute_AllowSecondPass(routePath[0], pathfindingDelegates, destinationValidityDelegates);
                    }

                    double cost = CalculateRouteCost(routePath, pathfindingDelegates);

                    if (bestRoutePaths.Count == 0 || cost <= routeCost)
                    {
                        if (cost < routeCost)
                        {
                            bestRoutePaths.Clear();
                        }

                        bestRoutePaths.Add(routePath);
                        routeCost = cost;
                    }
                }

                if (bestRoutePaths.Count == 0)
                {
                    World.log($"CommunityLib: Failed to find Trade Route from indexGroup {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to another index  group. Isolated group detected.");
                    indexGroups.Remove(indexGroup);
                    continue;
                }

                Location[] bestRoutePath = null;
                if (bestRoutePaths.Count > 1)
                {
                    bestRoutePath = bestRoutePaths[Eleven.random.Next(bestRoutePaths.Count)];
                }
                else
                {
                    bestRoutePath = bestRoutePaths[0];
                }

                tradeManager.routes.Add(new TradeRoute(bestRoutePath.ToList()));
                List<int> connectedGroup = indexGroups.FirstOrDefault(group => group != indexGroup && group.Contains(bestRoutePath[bestRoutePath.Length - 1].index));
                if (connectedGroup != null)
                {
                    World.log($"CommunityLib: Trade Route made connecting index Group {tradeManager.map.locations[indexGroup[0]].getName()} (size {indexGroup.Count}) to {tradeManager.map.locations[connectedGroup[0]].getName()} (size {connectedGroup.Count}).");

                    foreach (int index in connectedGroup)
                    {
                        if (!indexGroup.Contains(index))
                        {
                            indexGroup.Add(index);
                        }
                    }
                    indexGroups.Remove(connectedGroup);
                }
            }

            if (ModCore.opt_denseTradeRoutes)
            {
                World.log($"CommunityLib: Desnifying trade network.");
                ModCore.Get().hooks.DensifyingTradeRoutes = true;
                foreach (Location endpoint in endpoints)
                {
                    Location[] routePath = Pathfinding.getTradeRouteFrom(endpoint, endpoints);
                    if (routePath == null || routePath.Length < 2)
                    {
                        continue;
                    }

                    if (tradeManager.routes.Any(r => (r.start() == endpoint || r.end() == endpoint) && (r.start() == routePath[routePath.Length - 1] || r.end() == routePath[routePath.Length - 1])))
                    {
                        continue;
                    }

                    tradeManager.routes.Add(new TradeRoute(routePath.ToList()));
                    World.log($"CommunityLib: Densification trade route created from {endpoint.getName()} to {routePath[routePath.Length - 1].getName()}.");
                }
            }
            ModCore.Get().hooks.DensifyingTradeRoutes = false;
        }

        private List<HashSet<int>> findAllConnectedSets(ManagerTrade tradeManager)
        {
            List<HashSet<int>> connectedSets = new List<HashSet<int>>();
            HashSet<int> visited = new HashSet<int>();

            World.log("Rebuilding all connected sets");
            foreach (TradeRoute route in tradeManager.routes)
            {
                if (!visited.Contains(route.start().index))
                {
                    HashSet<int> set = tradeManager.buildConnectedSet(route.start().index);
                    World.log("Rebuilding connected set from " + route.start().getName(true) + " size " + set.Count.ToString());
                    connectedSets.Add(set);
                    visited.UnionWith(set);
                }

                if (!visited.Contains(route.end().index))
                {
                    HashSet<int> set = tradeManager.buildConnectedSet(route.end().index);
                    World.log("Rebuilding connected set from " + route.end().getName(true) + " size " + set.Count.ToString());
                    connectedSets.Add(set);
                    visited.UnionWith(set);
                }
            }

            return connectedSets;
        }

        private double CalculateRouteCost(Location[] routePath, List<Func<Location[], Location, double>> pathfindingDelegates)
        {
            double cost = 0.0;
            for (int i = 1; i < routePath.Length; i++)
            {
                foreach(Func <Location[], Location, double> pathfindingDelegate in pathfindingDelegates)
                {
                    cost += pathfindingDelegate(routePath, routePath[i]);
                }
            }

            return cost;
        }

        internal List<Location> getTradeRouteEndPoints()
        {
            List<Location> endpoints = new List<Location>();
            foreach (Location location in map.locations)
            {
                if (map.overmind.god is God_Mammon && location.settlement is Set_TombOfGods)
                {
                    endpoints.Add(location);
                    continue;
                }

                if (location.soc is Society society && location.index == society.capital && !society.isDarkEmpire && !society.isOphanimControlled)
                {
                    if (location.settlement is Set_City)
                    {
                        endpoints.Add(location);
                        continue;
                    }
                }
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onGetTradeRouteEndpoints)
            {
                hook(map, endpoints);
            }
            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onGetTradeRouteEndpoints(map, endpoints);
            }

            return endpoints;
        }
    }
}
