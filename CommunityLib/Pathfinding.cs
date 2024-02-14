using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

namespace CommunityLib
{
    public class Pathfinding
    {
        public Pathfinding()
        {

        }

        public static bool delegate_AQUAPHIBIOUS(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return location.isOcean || location.isCoastal;
        }

        public static bool delegate_AQUATIC(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return location.isOcean;
        }

        public static bool delegate_DESERT_ONLY(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY;
        }

        public static bool delegate_LANDLOCKED(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return !location.isOcean;
        }

        public static bool delegate_LayerBound(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            HashSet<int> layers = new HashSet<int> { origin.hex.z };
            if (destination.hex.z != origin.hex.z)
            {
                layers.Add(destination.hex.z);
            }

            return layers.Contains(location.hex.z);
        }

        public static bool delegate_SAFE_MOVE(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return u == null || location.soc == null || !location.soc.hostileTo(u);
        }

        public static bool delegate_SHADOWBOUND(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
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
                    return true;
                }

                return false;
            }

            return true;
        }

        public static bool delegate_SHADOW_ONLY(Location[] currentPath, Location location, Unit u, Location origin, Location destination)
        {
            return location.getShadow() >= 0.5;
        }

        public Location[] getPathTo(Location locA, Location locB, Unit u = null, bool safeMove = false)
        {
            if (locA == locB)
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, Location, Location, bool>>  pathfindingDelegates = new List<Func<Location[], Location, Unit, Location, Location, bool>> { delegate_LayerBound };

            if (u != null)
            {
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
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates_Location(locA, locB, u, pathfindingDelegates);
            }

            HashSet<Location> locationHashes = new HashSet<Location> { locA };
            List<Location> locations = new List<Location> { locA };
            List<Location[]> paths = new List<Location[]> { new Location[] { locA } };
            List<int> layers = new List<int> { locA.hex.z };

            if (locB.hex.z != locA.hex.z)
            {
                layers.Add(locB.hex.z);
            }

            int passCount = 0;
            while (passCount < 2)
            {
                passCount++;

                int i = 0;
                while (i < 128)
                {
                    List<Location> newLocations = new List<Location>();
                    List<Location[]> newPaths = new List<Location[]>();
                    i++;

                    for (int j = 0; j < locations.Count; j++)
                    {
                        foreach (Location neighbour in getNeighboursConditional(locations[j], u))
                        {
                            if (!locationHashes.Contains(neighbour))
                            {
                                bool valid = true;
                                foreach (Func<Location[], Location, Unit, Location, Location, bool> pathfindingDelegate in pathfindingDelegates)
                                {
                                    if (!pathfindingDelegate(paths[j], neighbour, u, locA, locB))
                                    {
                                        valid = false;
                                        break;
                                    }
                                }

                                if (!valid)
                                {
                                    continue;
                                }

                                Location[] newPathArray = new Location[paths[j].Length + 1];
                                for (int k = 0; k < paths[j].Length; k++)
                                {
                                    newPathArray[k] = paths[j][k];
                                }
                                newPathArray[newPathArray.Length - 1] = neighbour;

                                if (neighbour == locB)
                                {
                                    return newPathArray;
                                }

                                newLocations.Add(neighbour);
                                newPaths.Add(newPathArray);
                                locationHashes.Add(neighbour);
                            }
                        }
                    }

                    locations = newLocations;
                    paths = newPaths;
                    shuffle(locations, paths);
                }

                pathfindingDelegates.Remove(delegate_LayerBound);
            }

            return null;
        }

        public Location[] getPathTo(Location locA, SocialGroup sg, Unit u = null, bool safeMove = false)
        {
            if (locA.soc == sg)
            {
                return new Location[0];
            }

            List<Func<Location[], Location, Unit, Location, Location, bool>> pathfindingDelegates = new List<Func<Location[], Location, Unit, Location, Location, bool>>();

            if (u != null)
            {
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
            }

            foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
            {
                hook.onPopulatingPathfindingDelegates_SocialGroup(locA, sg, u, pathfindingDelegates);
            }

            Location locB = sg.getCapitalHex().location;

            HashSet<Location> locationHashes = new HashSet<Location> { locA };
            List<Location> locations = new List<Location> { locA };
            List<Location[]> paths = new List<Location[]> { new Location[] { locA } };
            List<int> layers = new List<int> { locA.hex.z };

            if (sg.getCapitalHex().z != locA.hex.z)
            {
                layers.Add(sg.getCapitalHex().z);
            }

            int i = 0;
            while (i < 128)
            {
                List<Location> newLocations = new List<Location>();
                List<Location[]> newPaths = new List<Location[]>();
                i++;

                for (int j = 0; j < locations.Count; j++)
                {
                    foreach (Location neighbour in getNeighboursConditional(locations[j], u))
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            bool valid = true;
                            foreach (Func<Location[], Location, Unit, Location, Location, bool> pathfindingDelegate in pathfindingDelegates)
                            {
                                if (!pathfindingDelegate(paths[j], neighbour, u, locA, locB))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (!valid)
                            {
                                continue;
                            }

                            Location[] newPathArray = new Location[paths[j].Length + 1];
                            for (int k = 0; k < paths[j].Length; k++)
                            {
                                newPathArray[k] = paths[j][k];
                            }
                            newPathArray[newPathArray.Length - 1] = neighbour;

                            if (neighbour.soc == sg)
                            {
                                return newPathArray;
                            }

                            newLocations.Add(neighbour);
                            newPaths.Add(newPathArray);
                            locationHashes.Add(neighbour);
                        }
                    }
                }

                locations = newLocations;
                paths = newPaths;
                shuffle(locations, paths);
            }

            return null;
        }

        private List<Location> getNeighboursConditional(Location loc, Unit u)
        {
            List<Location> result = loc.getNeighbours();

            if (u != null && u.isCommandable() && u is UA ua)
            {
                if (loc.settlement is Set_MinorOther && loc.settlement.subs.Any(sub => sub is Sub_Wonder_Doorway))
                {
                    Location tomb = null;
                    foreach (Location location in u.map.locations)
                    {
                        if (ModCore.Get().checkIsElderTomb(location))
                        {
                            tomb = location;
                        }
                    }

                    if (tomb != null)
                    {
                        result.Add(tomb);
                    }

                    if (u.homeLocation != -1)
                    {
                        result.Add(u.map.locations[u.homeLocation]);
                    }
                }
            }

            return result;
        }

        private void shuffle(List<Location> locations, List<Location[]> paths)
        {
            if (locations.Count > 0)
            {
                for (int i = 0; i < locations.Count; i++)
                {
                    int index = Eleven.random.Next(locations.Count);
                    Location valA = locations[index];
                    Location[] valB = paths[index];
                    locations[index] = locations[i];
                    locations[i] = valA;
                    paths[index] = paths[i];
                    paths[i] = valB;
                }
            }
        }
    }
}
