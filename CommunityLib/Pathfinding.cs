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

        public bool deligate_AQUAPHIBIOUS(Location[] currentPath, Location location, Unit u)
        {
            return location.isOcean || location.isCoastal;
        }

        public bool deligate_DESERT_ONLY (Location[] currentPath, Location location, Unit u)
        {
            return location.hex.terrain == Hex.terrainType.ARID || location.hex.terrain == Hex.terrainType.DESERT || location.hex.terrain == Hex.terrainType.DRY;
        }

        public bool delegate_LANDLOCKED(Location[] currentPath, Location location, Unit u)
        {
            return !location.isOcean;
        }

        public bool deligate_SAFE_MOVE (Location[] currentPath, Location location, Unit u)
        {
            return u == null || location.soc == null || !location.soc.hostileTo(u);
        }

        public Location[] getPathTo(Location locA, Location locB, Func<Location[], Location, Unit, bool> deligate, Unit u = null)
        {
            HashSet<Location> locationHashes = new HashSet<Location> { locA };
            List<Location> locations = new List<Location> { locA };
            List<Location[]> paths = new List<Location[]> { new Location[] { locA } };

            int i = 0;
            while (i < 128)
            {
                List<Location> newLocations = new List<Location>();
                List<Location[]> newPaths = new List<Location[]>();
                i++;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location loc = locations[j];
                    foreach (Location neighbour in loc.getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            if (!deligate(paths[j], neighbour, u))
                            {
                                if (neighbour == locB)
                                {
                                    return null;
                                }

                                continue;
                            }

                            Location[] newPathArray = new Location[locations.Count + 1];
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

            return null;
        }

        public Location[] getPathTo(Location locA, SocialGroup sg, Func<Location, Unit, bool> deligate, Unit u = null)
        {
            HashSet<Location> locationHashes = new HashSet<Location> { locA };
            List<Location> locations = new List<Location> { locA };
            List<Location[]> paths = new List<Location[]> { new Location[] { locA } };

            int i = 0;
            while (i < 128)
            {
                List<Location> newLocations = new List<Location>();
                List<Location[]> newPaths = new List<Location[]>();
                i++;

                for (int j = 0; j < locations.Count; j++)
                {
                    Location loc = locations[j];
                    foreach (Location neighbour in loc.getNeighbours())
                    {
                        if (!locationHashes.Contains(neighbour))
                        {
                            if (!deligate(neighbour, u))
                            {
                                continue;
                            }
                            Location[] newPathArray = new Location[locations.Count + 1];
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

        private void shuffle (List<Location> locations, List<Location[]> paths)
        {
            if (Location.indexCounter > 0)
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
