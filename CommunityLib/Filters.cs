using Assets.Code;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommunityLib
{
    public class Filters
    {
        private Cache cache;

        private Map map;

        private FiltersLocations locFilters;

        public Filters(Cache cache, Map map)
        {
            this.cache = cache;
            this.map = map;

            locFilters = new FiltersLocations(cache, this, map);
        }

        public void onTurnStart(Map map)
        {
            cache.ClearCache();
            FilterSocialGroups();
            FilterLocations();
            FilterUnits();
        }

        public void afterMapGenBeforeHistorical(Map map)
        {
            cache.ClearCache();
            FilterSocialGroups();
            FilterLocations();
            FilterUnits();
        }

        public void afterMapGenAfterHistorical(Map map)
        {
            cache.ClearCache();
            FilterSocialGroups();
            FilterLocations();
            FilterUnits();
        }

        public List<Location> getLocationsWithinDistance(Location source, double distance)
        {
            List<Location> result = new List<Location>();
            Dictionary<Location, double> dict;
            if (cache.distanceByLocationsFromLocation.TryGetValue(source, out dict) && dict != null)
            {
                foreach (KeyValuePair<Location, double> pair in dict)
                {
                    if (pair.Value <= distance)
                    {
                        result.Add(pair.Key);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Location> getLocationsWithinSteps(Location source, int steps)
        {
            List<Location> result = new List<Location>();
            List<Location>[] array;
            if (cache.locationsByStepsExclusiveFromLocation.TryGetValue(source, out array) && array != null)
            {
                steps = Math.Min(steps, array.Length);
                for (int i = 0; i <= steps; i++)
                {
                    if (array[i] != null && array[i].Count > 0)
                    {
                        result.AddRange(array[i]);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Settlement> getSettlementsWithinDistance(Location source, double distance)
        {
            List<Settlement> result = new List<Settlement>();
            Dictionary<Location, double> dict;
            if (cache.distanceByLocationsFromLocation.TryGetValue(source, out dict) && dict != null)
            {
                foreach (KeyValuePair<Location, double> pair in dict)
                {
                    if (pair.Value <= distance && pair.Key.settlement != null)
                    {
                        result.Add(pair.Key.settlement);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Settlement> getSettlementsWithinSteps(Location source, int steps)
        {
            List<Settlement> result = new List<Settlement>();
            List<Settlement>[] array;
            if (cache.settlementsByStepsExclusiveFromLocation.TryGetValue(source, out array) && array != null)
            {
                steps = Math.Min(steps, array.Length);
                for (int i = 0; i <= steps; i++)
                {
                    if (array[i] != null && array[i].Count > 0)
                    {
                        result.AddRange(array[i]);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Unit> getUnitsWithinDistance(Location source, double distance)
        {
            List<Unit> result = new List<Unit>();
            Dictionary<Location, double> dict;
            if (cache.distanceByLocationsFromLocation.TryGetValue(source, out dict) && dict != null)
            {
                foreach (KeyValuePair<Location, double> pair in dict)
                {
                    if (pair.Value <= distance && pair.Key.units != null && pair.Key.units.Count > 0)
                    {
                        result.AddRange(pair.Key.units);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Unit> getUnitsWithinSteps(Location source, int steps)
        {
            List<Unit> result = new List<Unit>();
            List<Unit>[] array;
            if (cache.unitsByStepsExclusiveFromLocation.TryGetValue(source, out array) && array != null)
            {
                steps = Math.Min(steps, array.Length);
                if (steps == 0)
                {
                    result.AddRange(source.units);
                }
                else
                {
                    for (int i = 0; i <= steps; i++)
                    {
                        if (array[i] != null && array[i].Count > 0)
                        {
                            result.AddRange(array[i]);
                        }
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public List<Set_OrcCamp> getSpecialisedOrcCamps()
        {
            List<Set_OrcCamp> result = new List<Set_OrcCamp>();

            for (int i = 1; i < cache.orcCampBySpecialism.Length; i++)
            {
                List<Set_OrcCamp> camps = cache.orcCampBySpecialism[i];

                if (camps != null && camps.Count > 0)
                {
                    result.AddRange(camps);
                }
            }

            if (result.Count > 0)
            {
                return result;
            }
            return null;
        }

        public List<Set_OrcCamp> getSpecialisedOrcCampsBySocialGroup(SocialGroup sG)
        {
            List<Set_OrcCamp> result = new List<Set_OrcCamp>();

            List<Set_OrcCamp>[] array;
            if (cache.orcCampBySocialGroupBySpecialism.TryGetValue(sG, out array) && array != null)
            {
                for (int i = 1; i < array.Length; i++)
                {
                    List<Set_OrcCamp> camps = array[i];
                    if (camps != null && camps.Count > 0)
                    {
                        result.AddRange(camps);
                    }
                }

                if (result.Count > 0)
                {
                    return result;
                }
            }

            return null;
        }

        public IList CreateList(Type myType)
        {
            Type genericListType = typeof(List<>).MakeGenericType(myType);
            return (IList)Activator.CreateInstance(genericListType);
        }

        public IDictionary CreateDictionary(Type keyType, Type valueType)
        {
            Type genericDictionaryType = typeof(Dictionary<,>).MakeGenericType(new Type[] { keyType, valueType });
            return (IDictionary)Activator.CreateInstance (genericDictionaryType);
        }

        public IList GetOrCreateKeyListPair(IDictionary dict, Type key, Type t)
        {
            if (dict.Contains(key))
            {
                return dict[key] as IList;
            }
            else
            {
                IList  value = CreateList(t);
                dict.Add(key, value);
                return value;
            }
        }

        public IList GetOrCreateKeyListPair(IDictionary dict, object key, Type t)
        {
            IList value;
            if (!dict.Contains(key))
            {
                value = CreateList(t);
                dict.Add(key, value);
            }
            else if (dict[key] == null)
            {
                value = CreateList(t);
                dict[key] = value;
            }
            else
            {
                value = dict[key] as IList;
            }
            return value;
        }

        public IDictionary CreateSubDictionary(Type keyType)
        {
            return CreateDictionary(keyType, typeof(IList));
        }

        public bool TryCreateSubDictionary(IDictionary dict, object key, Type keyType)
        {
            if (!dict.Contains(key) || dict[key] == null)
            {
                dict.Add(key, CreateSubDictionary(keyType));
                return true;
            }
            return false;
        }

        public bool TryCreateSubDictionary(IDictionary dict, Type key, Type keyType)
        {
            if (!dict.Contains(key) || dict[key] == null)
            {
                dict.Add(key, CreateSubDictionary(keyType));
                return true;
            }
            return false;
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, Type key, Type valueType, object value)
        {
            GetOrCreateKeyListPair(dict, key, valueType).Add(value);
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, object key, Type valueType, object value)
        {
            GetOrCreateKeyListPair(dict, key, valueType).Add(value);
        }

        public void FilterSocialGroups()
        {
          //Console.WriteLine("CommunityLib: Starting Social Group Processing.");
            IDictionary dict = cache.socialGroupsByType;
            IDictionary dictE = cache.socialGroupsByTypeExclusive;

            foreach (SocialGroup sG in map.socialGroups)
            {
              //Console.WriteLine("CommunityLib: Filtering Social Group " + sG.getName() + " of Type: " + sG.GetType().Name + ".");
                // Initialize universal variables
                Type tSG = sG.GetType();

                // Conduct one-off operations
                CreateAndOrAddToKeyListPair(dictE, tSG, tSG, sG);

                // Initialize loop onl-variables
                Type targetT = typeof(SocialGroup);
                bool iterateSGT = true;

              //Console.WriteLine("CommunityLib: Starting social group type loop.");
                while (iterateSGT)
                {
                    CreateAndOrAddToKeyListPair(dict, tSG, tSG, sG);

                    if (tSG == targetT)
                    {
                        iterateSGT = false;
                      //Console.WriteLine("CommunityLib: End social group type loop.");
                    }
                    else
                    {
                        tSG = tSG.BaseType;
                      //Console.WriteLine("CommunityLib: Iterate Type to " + tSG.Name + ".");
                    }
                }
              //Console.WriteLine("CommunityLib: End loop for Social Group " + sG.getName() + " of Type " + sG.GetType().Name + ".");
            }
          //Console.WriteLine("CommunityLib: Completed Social Group Processing.");
        }

        public void FilterUnits()
        {
          //Console.WriteLine("CommunityLib: Starting Unit Processing.");

            if (map.units == null || map.units.Count == 0)
            {
              //Console.WriteLine("CommunityLib: No units found.");
                return;
            }

            // Initialize universal variables
            Type tU;
            Type tSG;
            bool commandable;

            double profile;
            int visibleSteps;

            List<Unit> unitsThatCanSeeMe;
            IList unitsThatTheyCanSee = new List<Unit>();


            // Dictionaries being operated on at all level.
            IDictionary uByT = cache.unitsByType;
            IDictionary uByTE = cache.unitsByTypeExclusive;
            IDictionary uBySG = cache.unitsBySocialGroup;
            IDictionary uBySGT = cache.unitsBySocialGroupType;
            IDictionary uBySGTE = cache.unitsBySocialGroupTypeExclusive;
            IDictionary uBySGByT = cache.unitsBySocialGroupByType;
            IDictionary uBySGTByT = cache.unitsBySocialGroupTypeByType;
            IDictionary uBySGTEByT = cache.unitsBySocialGroupTypeExclusiveByType;
            IDictionary uBySGTByTE = cache.unitsBySocialGroupTypeByTypeExclusive;
            IDictionary uBySGTEByTE = cache.unitsBySocialGroupTypeExclusiveByTypeExclusive;
            IDictionary cUByT = cache.commandableUnitsByType;
            IDictionary cUByTE = cache.commandableUnitsByTypeExclusive;
            IDictionary cUBySG = cache.commandableUnitsBySocialGroup;
            IDictionary cUBySGT = cache.commandableUnitsBySocialGroupType;
            IDictionary cUBySGTE = cache.commandableUnitsBySocialGroupTypeExclusive;
            IDictionary cUBySGByT = cache.commandableUnitsBySocialGroupByType;
            IDictionary cUBySGTByT = cache.commandableUnitsBySocialGroupTypeByType;
            IDictionary cUBySGTEByT = cache.commandableUnitsBySocialGroupTypeExclusiveByType;
            IDictionary cUBySGTByTE = cache.commandableUnitsBySocialGroupTypeByTypeExclusive;
            IDictionary cUBySGTEByTE = cache.commandableUnitsBySocialGroupTypeExclusiveByTypeExclusive;

            // Distance Dictionaries
            Dictionary<Unit, Location> cUL = cache.commandableUnitLocations;
            Dictionary<Unit, IList> uVToU = cache.unitVisibleToUnits;
            Dictionary<Unit, IList> vUByU = cache.visibleUnitsByUnit;

            // Initialize loop-only variables
            bool iterateSGT;
            bool iterateUT;
            bool excludeSGT;
            Type targetTSG = typeof(SocialGroup);
            Type targetTU = typeof(Unit);

            foreach (Unit u in map.units)
            {
              //Console.WriteLine("CommunityLib: Filtering unit " + u.getName() + " of type: " + u.GetType().Name + ".");
                // Set universal variables
                tU = u.GetType();
                tSG = u.society.GetType();
                commandable = u.isCommandable();
              //Console.WriteLine("CommunityLib: Commandable = " + commandable.ToString() + ".");

                // Conduct one-off operations
                CreateAndOrAddToKeyListPair(uByTE, tU, tU, u);
                CreateAndOrAddToKeyListPair(uBySG, u.society, typeof(Unit), u);
                CreateAndOrAddToKeyListPair(uBySGTE, tSG, typeof(Unit), u);

                // TryCreate SubDictionaries
                TryCreateSubDictionary(uBySGByT, u.society, typeof(Type));
                TryCreateSubDictionary(uBySGTEByT, tSG, typeof(Type));
                TryCreateSubDictionary(uBySGTEByTE, tSG, typeof(Type));
                CreateAndOrAddToKeyListPair(uBySGTEByTE[tSG] as IDictionary, tU, tU, u);

                if (commandable)
                {
                    if (!cUL.ContainsKey(u))
                    {
                        cUL.Add(u, u.location);
                    }
                    else if (cUL[u] == null)
                    {
                        cUL[u] = u.location;
                    }

                    CreateAndOrAddToKeyListPair(cUByTE, tU, tU, u);
                    CreateAndOrAddToKeyListPair(cUBySG, u.society, typeof(Unit), u);
                    CreateAndOrAddToKeyListPair(cUBySGTE, tSG, typeof(Unit), u);

                    // TryCreate SubDictionaries
                    TryCreateSubDictionary(cUBySGByT, u.society, typeof(Type));
                    TryCreateSubDictionary(cUBySGTEByT, tSG, typeof(Type));
                    TryCreateSubDictionary(cUBySGTEByTE, tSG, typeof(Type));
                    CreateAndOrAddToKeyListPair(cUBySGTEByTE[tSG] as IDictionary, tU, tU, u);
                }

                //Console.WriteLine("CommunityLib: Starting Visibility Processing.");
                // Visibility Processing
                profile = u.profile;
                visibleSteps = Math.Max((int)Math.Floor(u.profile / 10), 0);
                //Console.WriteLine("CommunityLib: Gathering units that can see " + u.getName() + " at " + u.location.getName() + ", out to a distance of " + visibleSteps.ToString() + " steps.");
                if (!uVToU.ContainsKey(u))
                {
                    uVToU.Add(u, new List<Unit>());
                }
                else if (uVToU[u] == null)
                {
                    uVToU[u] = new List<Unit>();
                }
                uVToU[u] = getUnitsWithinSteps(u.location, visibleSteps);


                unitsThatCanSeeMe = uVToU[u] as List<Unit>;
                unitsThatCanSeeMe.Remove(u);

                if (unitsThatCanSeeMe != null && unitsThatCanSeeMe.Count() > 0)
                {


                  //Console.WriteLine("CommunityLib: Updating caches for Units that can see " + u.getName() + ".");
                    unitsThatTheyCanSee.Clear();
                    foreach (Unit unitThatSeesMe in unitsThatCanSeeMe)
                    {
                      //Console.WriteLine("CommunityLib: Updating cache for " + unitThatSeesMe.getName() + ".");
                        if (!vUByU.TryGetValue(unitThatSeesMe, out unitsThatTheyCanSee))
                        {
                            vUByU.Add(unitThatSeesMe, new List<Unit>());
                            unitsThatTheyCanSee = vUByU[unitThatSeesMe];
                        }
                        else if (unitsThatTheyCanSee == null)
                        {
                            unitsThatTheyCanSee = new List<Unit>();
                            unitsThatTheyCanSee = vUByU[unitThatSeesMe];
                        }
                        unitsThatTheyCanSee.Add(u);
                    }
                }
              //Console.WriteLine("CommunityLib: Completed Visibility Processing.");

              //Console.WriteLine("CommunityLib: Starting social group type loop.");
              //Console.WriteLine("ComminityLib: Filtering for Social Group " + u.society.name + " of Type " + tSG.Name + ".");
                // Set loop-only variables
                iterateSGT = true;
                excludeSGT = true;
                
                // Conduct Operations for all Types tSG, from obj.GetType() to targetTSG, inclusively
                while (iterateSGT)
                {
                    // The subdictionaries used in this loop were checked for, and if neccesary created, earlier in this method.
                    iterateUT = true;
                    tU = u.GetType();

                    // TryCreate SubDictionaries
                    TryCreateSubDictionary(uBySGTByT, tSG, typeof(Type));
                    TryCreateSubDictionary(uBySGTByTE, tSG, typeof(Type));

                    CreateAndOrAddToKeyListPair(uBySGT, tSG, typeof(Unit), u);
                    CreateAndOrAddToKeyListPair(uBySGTByTE[tSG] as IDictionary, tU, tU, u);

                    if (commandable)
                    {
                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(cUBySGTByT, tSG, typeof(Type));
                        TryCreateSubDictionary(cUBySGTByTE, tSG, typeof(Type));

                        CreateAndOrAddToKeyListPair(cUBySGT, tSG, typeof(Unit), u);
                        CreateAndOrAddToKeyListPair(cUBySGTByTE[tSG] as IDictionary, tU, tU, u);
                    }

                  //Console.WriteLine("CommunityLib: Starting unit type loop.");
                    // Conduct Operations for all Types tU, from obj.GetType() to targetTU, inclusively
                    while (iterateUT)
                    {
                        CreateAndOrAddToKeyListPair(uBySGTByT[tSG] as IDictionary, tU, tU, u);

                        if (excludeSGT)
                        {
                            CreateAndOrAddToKeyListPair(uByT, tU, tU, u);
                            CreateAndOrAddToKeyListPair(uBySGByT[u.society] as IDictionary, tU, tU, u);
                            CreateAndOrAddToKeyListPair(uBySGTEByT[tSG] as IDictionary, tU, tU, u);
                        }

                        if (commandable)
                        {
                            CreateAndOrAddToKeyListPair(cUBySGTByT[tSG] as IDictionary, tU, tU, u);

                            // Only run on first iteration of Social Group Type Loop
                            if (excludeSGT)
                            {
                                CreateAndOrAddToKeyListPair(cUByT, tU, tU, u);
                                CreateAndOrAddToKeyListPair(cUBySGByT[u.society] as IDictionary, tU, tU, u);
                                CreateAndOrAddToKeyListPair(cUBySGTEByT[tSG] as IDictionary, tU, tU, u);
                            }
                        }

                        if (tU == targetTU)
                        {
                            iterateUT = false;
                            excludeSGT = false;
                          //Console.WriteLine("CommunityLib: End unit type loop.");
                        }
                        else
                        {
                            tU = tU.BaseType;
                          //Console.WriteLine("CommunityLib: Iterating Type to " + tU.Name + ".");
                        }
                    }

                    if (tSG == targetTSG)
                    {
                        iterateSGT = false;
                      //Console.WriteLine("CommunityLib: End cocial group type Loop.");
                    }
                    else
                    {
                        tSG = tSG.BaseType;
                      //Console.WriteLine("CommunityLib: Iterate type to " + tSG.Name + ".");
                    }
                }
                //Console.WriteLine("CommunityLib: End Loop for unit " + u.getName() + " of Type " + u.GetType().Name + ".");
            }
            //Console.WriteLine("CommunityLib: Completed Unit Processing");
        }

        public void FilterLocations()
        {
            locFilters.FilterLocations();
        }

        public void UpdateLocationDistances()
        {
            //Console.WriteLine("CommunityLib: Starting Location Distance Update");
            if (map.locations == null || map.locations.Count == 0)
            {
                //Console.WriteLine("CommunityLib: No locations found.");
                return;
            }

            // Clear location-distance caches
            Dictionary<Location, Dictionary<Location, double>> dByLfromL = cache.distanceByLocationsFromLocation;
            Dictionary<Location, List<Location>[]> lBySEfromL = cache.locationsByStepsExclusiveFromLocation;

            dByLfromL.Clear();
            lBySEfromL.Clear();

            double distance;
            int steps;
            foreach (Location l in map.locations)
            {
                foreach (Location loc2 in map.locations)
                {
                    distance = map.getDist(l, loc2);
                    steps = map.getStepDist(l, loc2);

                    if (!dByLfromL.ContainsKey(l))
                    {
                        dByLfromL.Add(l, new Dictionary<Location, double>());
                    }
                    else if (dByLfromL[l] == null)
                    {
                        dByLfromL[l] = new Dictionary<Location, double>();
                    }
                    dByLfromL[l].Add(loc2, distance);

                    if (!lBySEfromL.ContainsKey(l))
                    {
                        lBySEfromL.Add(l, new List<Location>[125]);
                    }
                    else if (lBySEfromL[l] == null)
                    {
                        lBySEfromL[l] = new List<Location>[125];
                    }
                    if (lBySEfromL[l][steps] == null)
                    {
                        lBySEfromL[l][steps] = new List<Location>();
                    }
                    lBySEfromL[l][steps].Add(loc2);
                }
            }
        }

        public void UpdateCommandableUnitVisibility()
        {
            //Console.WriteLine("CommunityLib: Processing End-of-Turn visibility update for commandable units.");
            if (!cache.commandableUnitsByType.ContainsKey(typeof(Unit)) || cache.commandableUnitsByType[typeof(Unit)].Count == 0)
            {
                //Console.WriteLine("CommunityLib: No commandable units found.");
                return;
            }

            List<Unit> cUs = cache.commandableUnitsByType[typeof(Unit)] as List<Unit>;
            Dictionary<Unit, Location> cUL = cache.commandableUnitLocations;

            Dictionary<Unit, IList> vUToU = cache.unitVisibleToUnits;
            Dictionary<Unit, IList> vUByU = cache.visibleUnitsByUnit;

            double profile;
            int visibleSteps;
            List<Unit> unitsThatCanSeeMe = new List<Unit>();
            IList unitsThatTheyCanSee = new List<Unit>();

            foreach (Unit cU in cUs)
            {
                unitsThatCanSeeMe.Clear();

                //Console.WriteLine("CommunityLib: Removing old vision data");
                if (cU.location == cUL[cU])
                {
                    continue;
                }
                cUL[cU] = cU.location;

                if (vUToU.ContainsKey(cU) && vUToU[cU] != null)
                {
                    foreach (Unit unitThatSeesMe in vUToU[cU])
                    {
                        vUByU[unitThatSeesMe].Remove(cU);
                    }
                    vUToU[cU].Clear();
                }

                //Console.WriteLine("CommunityLib: Starting Visibility Processing");
                // Visibility Processing
                profile = cU.profile;
                visibleSteps = Math.Max((int)Math.Floor(cU.profile / 10), 0);
                //Console.WriteLine("CommunityLib: Gathering Units that can see " + cU.getName() + " at " + cU.location.getName() + ", out to a distance of " + visibleSteps.ToString() + " steps.");
                unitsThatCanSeeMe = getUnitsWithinSteps(cU.location, visibleSteps);
                unitsThatCanSeeMe.Remove(cU);

                if (unitsThatCanSeeMe != null && unitsThatCanSeeMe.Count() > 0)
                {
                    //Console.WriteLine("CommunityLib: Updating caches for Units that can see " + cU.getName() + ".");
                    unitsThatTheyCanSee.Clear();
                    foreach (Unit unitThatSeesMe in unitsThatCanSeeMe)
                    {
                        //Console.WriteLine("CommunityLib: Updating cache for " + unitThatSeesMe.getName() + ".");
                        if (!cache.visibleUnitsByUnit.TryGetValue(unitThatSeesMe, out unitsThatTheyCanSee))
                        {
                            cache.visibleUnitsByUnit.Add(unitThatSeesMe, new List<Unit>());
                            unitsThatTheyCanSee = cache.visibleUnitsByUnit[unitThatSeesMe];
                        }
                        else if (unitsThatTheyCanSee == null)
                        {
                            cache.visibleUnitsByUnit[unitThatSeesMe] = new List<Unit>();
                            unitsThatTheyCanSee = cache.visibleUnitsByUnit[unitThatSeesMe];
                        }
                        unitsThatTheyCanSee.Add(cU);
                    }
                }
            }
        }
    }
}