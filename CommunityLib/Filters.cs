using Assets.Code;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CommunityLib
{
    public class Filters
    {
        private Cache cache;

        private Map map;

        public Filters(Cache cache, Map map)
        {
            this.cache = cache;
            this.map = map;
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
            if (dict.Contains(key))
            {
                return dict[key] as IList;
            }
            else
            {
                IList value = CreateList(t);
                dict.Add(key, value);
                return value;
            }
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
            if (!dict.Contains(key))
            {
                dict.Add(key, CreateSubDictionary(keyType));
                return true;
            }
            return false;
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, Type key, Type valueType, object value)
        {
            GetOrCreateKeyListPair(dict, key, valueType).Add(value);
            // GetOrCreateKeyListPair(dict, t).Add(Convert.ChangeType(value, t));
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, object key, Type valueType, object value)
        {
            GetOrCreateKeyListPair(dict, key, valueType).Add(value);
            // GetOrCreateKeyListPair(dict, t).Add(Convert.ChangeType(value, t));
        }

        public void FilterSocialGroups()
        {
            IDictionary dict = cache.socialGroupsByType;
            IDictionary dictE = cache.socialGroupsByTypeExclusive;

            foreach (SocialGroup sG in map.socialGroups)
            {
                //Console.WriteLine("CommunityLib: Filtering social group " + sG.getName() + " of type: " + sG.GetType().Name);
                // Initialize universal variables
                Type tSG = sG.GetType();

                // Conduct one-off operations
                CreateAndOrAddToKeyListPair(dictE, tSG, tSG, sG);

                // Initialize loop onl-variables
                Type targetT = typeof(SocialGroup);
                bool iterateSGT = true;

                //Console.WriteLine("CommunityLib: Starting Social Group Type Loop");
                while (iterateSGT)
                {
                    CreateAndOrAddToKeyListPair(dict, tSG, tSG, sG);

                    if (tSG == targetT)
                    {
                        iterateSGT = false;
                        //Console.WriteLine("CommunityLib: End Social Group Type Loop");
                    }
                    else
                    {
                        tSG = tSG.BaseType;
                        //Console.WriteLine("CommunityLib: Iterate type to " + tSG.Name);
                    }
                }
                //Console.WriteLine("CommunityLib: End Loop for Social Group " + sG.getName() + " of Type " + sG.GetType());
            }
            //Console.WriteLine("CommunityLib: Completed Social Group Processing");
        }

        public void FilterUnits()
        {
            //Console.WriteLine("CommunityLib: Starting Unit Processing.");

            // Initialize universal variables
            Type tU;
            Type tSG;
            bool commandable;

            double profile;
            int visibleSteps;

            List<Unit> unitsThatSeeMe = new List<Unit>();
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

            // Initialize loop-only variables
            bool iterateSGT;
            bool iterateUT;
            bool excludeSGT;
            Type targetTSG = typeof(SocialGroup);
            Type targetTU = typeof(Unit);

            foreach (Unit u in map.units)
            {
                //Console.WriteLine("CommunityLib: Filtering unit " + u.getName() + " of type: " + u.GetType().Name);
                // Set universal variables
                tU = u.GetType();
                tSG = u.society.GetType();
                commandable = u.isCommandable();
                //Console.WriteLine("CommunityLib: Commandable = " + commandable.ToString());

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
                    if (!cache.commandableUnitLocations.ContainsKey(u))
                    {
                        cache.commandableUnitLocations.Add(u, u.location);
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

                //Console.WriteLine("CommunityLib: Starting Visibility Processing");
                // Visibility Processing
                profile = u.profile;
                visibleSteps = (int)Math.Floor(u.profile / 10);
                unitsThatSeeMe.Clear();
                //Console.WriteLine("CommunityLib: Gathering Units that can see " + u.getName() + ".");
                foreach (Location location in cache.locationsByStepsFromLocation[u.location][visibleSteps])
                {
                    if (location.units != null && location.units.Count() > 0)
                    {
                        unitsThatSeeMe.AddRange(location.units);
                    }
                }
                unitsThatSeeMe.Remove(u);
                cache.unitVisibleToUnits.Add(u, unitsThatSeeMe);

                //Console.WriteLine("CommunityLib: Updating caches for Units that can see " + u.getName() + ".");
                unitsThatTheyCanSee.Clear();
                foreach (Unit unitThatSeesMe in unitsThatSeeMe)
                {
                    if (unitThatSeesMe == u)
                    {
                        continue;
                    }

                    //Console.WriteLine("CommunityLib: Updating cache for " + unitThatSeesMe.getName() + ".");
                    if (!cache.visibleUnitsByUnit.TryGetValue(unitThatSeesMe, out unitsThatTheyCanSee))
                    {
                        cache.visibleUnitsByUnit.Add(unitThatSeesMe, new List<Unit>());
                        unitsThatTheyCanSee = cache.visibleUnitsByUnit[unitThatSeesMe];
                    }
                    unitsThatTheyCanSee.Add(u);
                }

                //Console.WriteLine("CommunityLib: Starting Social Group Type Loop");
                //Console.WriteLine("ComminityLib: Filtering for Social Group " + u.society.name + " of type " + tSG.Name);
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

                    //Console.WriteLine("CommunityLib: Starting Unit Type Loop");
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
                            //Console.WriteLine("CommunityLib: End Unit Type Loop");
                        }
                        else
                        {
                            tU = tU.BaseType;
                            //Console.WriteLine("CommunityLib: Iterate type to " + tU.Name);
                        }
                    }

                    if (tSG == targetTSG)
                    {
                        iterateSGT = false;
                        //Console.WriteLine("CommunityLib: End Social Group Type Loop");
                    }
                    else
                    {
                        tSG = tSG.BaseType;
                        //Console.WriteLine("CommunityLib: Iterate type to " + tSG.Name);
                    }
                }
                //Console.WriteLine("CommunityLib: End Loop for unit " + u.getName() + " of Type " + u.GetType());
            }
            //Console.WriteLine("CommunityLib: Completed Unit Processing");
        }

        public void FilterLocations()
        {
            // Initialize universal variables
            Settlement s;

            Type tL;
            Type tS;
            Type tSG;

            // Dictionaries being operated on at all level.
            // Location Dictionaries
            IDictionary lByT = cache.locationsByType;
            IDictionary lByTE = cache.locationsByTypeExclusive;
            IDictionary lBySG = cache.locationsBySocialGroup;
            IDictionary lWoSGByT = cache.locationsWithoutSocialGroupByType;
            IDictionary lWoSGByTE = cache.locationsWithoutSocialGroupByTypeExclusive;
            IDictionary lBySGT = cache.locationsBySocialGroupType;
            IDictionary lBySGTE = cache.locationsBySocialGroupTypeExclusive;
            IDictionary lBySGByT = cache.locationsBySocialGroupByType;
            IDictionary lBySGTByT = cache.locationsBySocialGroupTypeByType;
            IDictionary lBySGTEByT = cache.locationsBySocialGroupTypeExclusiveByType;
            IDictionary lBySGTByTE = cache.locationsBySocialGroupTypeByTypeExclusive;
            IDictionary lBySGTEByTE = cache.locationsBySocialGroupTypeExclusiveByTypeExclusive;
            IDictionary lWoSByT = cache.locationsWithoutSettlementByType;
            IDictionary lWoSByTE = cache.locationsWithoutSettlementByTypeExclusive;
            IDictionary lWoSG_SByT = cache.locationsWithoutSocialGroupsOrSettlementsByType;
            IDictionary lWoSG_SByTE = cache.locationsWithoutSocialGroupsOrSettlementsByTypeExclusive;

            // Settlement Dictionaries
            IDictionary sByT = cache.settlementsByType;
            IDictionary sByTE = cache.settlementsByTypeExclusive;
            IDictionary sBySG = cache.settlementsBySocialGroup;
            IDictionary sWoSGByT = cache.settlementsWithoutSocialGroupByType;
            IDictionary sWoSGByTE = cache.settlementsWithoutSocialGroupByTypeExclusive;
            IDictionary sBySGT = cache.settlementsBySocialGroupType;
            IDictionary sBySGTE = cache.settlementsBySocialGroupTypeExclusive;
            IDictionary sBySGByT = cache.settlementsBySocialGroupByType;
            IDictionary sBySGTByT = cache.settlementsBySocialGroupTypeByType;
            IDictionary sBySGTEByT = cache.settlementsBySocialGroupTypeExclusiveByType;
            IDictionary sBySGTByTE = cache.settlementsBySocialGroupTypeByTypeExclusive;
            IDictionary sBySGTEByTE = cache.settlementsBySocialGroupTypeExclusiveByTypeExclusive;

            // Initialize loop-only variables
            bool iterateSGT;
            bool iterateLT;
            bool iterateST;
            bool excludeSGTForL;
            bool excludeSGTForS;
            Type targetTSG = typeof(SocialGroup);
            Type targetTL = typeof(Location);
            Type targetTS = typeof(Settlement);

            foreach (Location l in map.locations)
            {
                //Console.WriteLine("CommunityLib: Filtering location " + l.getName() + " of type: " + l.GetType().Name);
                // Set universal variables
                s = null;
                tS = null;
                tSG = null;
                tL = l.GetType();

                CreateAndOrAddToKeyListPair(lByTE, tL, tL, l);

                // Branch for Social Groups
                if (l.soc != null)
                {
                    tSG = l.soc.GetType();
                    //Console.WriteLine("CommunityLib: Location belongs to Social Group " + l.soc.name + " of Type " + tSG.Name);

                    CreateAndOrAddToKeyListPair(lBySG, l.soc, typeof(Location), l);
                    CreateAndOrAddToKeyListPair(lBySGTE, tSG, typeof(Location), l);

                    // TryCreate SubDictionaries
                    TryCreateSubDictionary(lBySGByT, l.soc, typeof(Type));
                    TryCreateSubDictionary(lBySGTEByT, tSG, typeof(Type));
                    TryCreateSubDictionary(lBySGTEByTE, tSG, typeof(Type));
                    CreateAndOrAddToKeyListPair(lBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                }
                else
                {
                    CreateAndOrAddToKeyListPair(lWoSGByTE, tL, tL, l);
                }

                // Branch for Settlements.
                if (l.settlement != null)
                {
                    s = l.settlement;
                    tS = s.GetType();
                    //Console.WriteLine("CommunityLib: Location has settlement " + s.name + " of Type " + tS.Name);

                    CreateAndOrAddToKeyListPair(sByTE, tS, tS, s);

                    if (tSG != null)
                    {
                        CreateAndOrAddToKeyListPair(sBySG, l.soc, typeof(Settlement), s);
                        CreateAndOrAddToKeyListPair(sBySGTE, tSG, typeof(Settlement), s);

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(sBySGByT, l.soc, typeof(Type));
                        TryCreateSubDictionary(sBySGTEByT, tSG, typeof(Type));
                        TryCreateSubDictionary(sBySGTEByTE, tSG, typeof(Type));
                        CreateAndOrAddToKeyListPair(sBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                    }

                    cache.settlementsByStepsFromLocation.Add(l, cache.locationsByStepsFromLocation[l]);
                    cache.settlementsByStepsExclusiveFromLocation.Add(l, cache.locationsByStepsExclusiveFromLocation[l]);
                }
                else if (tSG == null)
                {
                    CreateAndOrAddToKeyListPair(lWoSG_SByTE, tL, tL, l);
                }
                else
                {
                    CreateAndOrAddToKeyListPair(lWoSByTE, tL, tL, l);
                }

                // Set loop-only variables
                iterateLT = true;
                iterateSGT = true;
                iterateST = true;
                excludeSGTForL = true;
                excludeSGTForS = true;

                if (tSG != null)
                {
                    //Console.WriteLine("CommunityLib: Starting Scoial Group Type Loop");
                    // Conduct Operations for all Types tSG, from obj.GetType() to targetTSG, inclusively
                    while (iterateSGT)
                    {
                        // The subdictionaries used in this loop were checked for, and if neccesary created, earlier in this method
                        iterateLT = true;
                        tL = l.GetType();
                        if (s != null)
                        {
                            iterateST = true;
                            tS = s.GetType();
                        }

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(lBySGTByT, tSG, typeof(Type));
                        TryCreateSubDictionary(lBySGTByTE, tSG, typeof(Type));

                        CreateAndOrAddToKeyListPair(lBySGT, tSG, typeof(Location), l);
                        CreateAndOrAddToKeyListPair(lBySGTByTE[tSG] as IDictionary, tSG, tL, l);

                        //Console.WriteLine("CommunityLib: Starting Location Type Loop");
                        // Conduct Operations for all Types tL, from obj.GetType() to targetTL, inclusively
                        while (iterateLT)
                        {
                            CreateAndOrAddToKeyListPair(lBySGTByT[tSG] as IDictionary, tL, tL, l);

                            if (s == null)
                            {
                                CreateAndOrAddToKeyListPair(lWoSByT, tL, tL, l);
                            }

                            if (excludeSGTForL)
                            {
                                CreateAndOrAddToKeyListPair(lByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGByT[l.soc] as IDictionary, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGTEByT[tSG] as IDictionary, tL, tL, l);
                            }

                            if (tL == targetTL)
                            {
                                iterateLT = false;
                                excludeSGTForL = false;
                                //Console.WriteLine("CommunityLib: End Location Type Loop");
                            }
                            else
                            {
                                tL = tL.BaseType;
                                //Console.WriteLine("CommunityLib: Iterate type to " + tL.Name);
                            }
                        }

                        // Branch for Settlements
                        if (s != null)
                        {
                            //TryCreate SubDictionaries
                            TryCreateSubDictionary(sBySGTByT, tSG, typeof(Type));
                            TryCreateSubDictionary(sBySGTByTE, tSG, typeof(Type));

                            CreateAndOrAddToKeyListPair(sBySGT, tSG, typeof(Settlement), s);
                            CreateAndOrAddToKeyListPair(sBySGTByTE[tSG] as IDictionary, tS, tS, s);

                            //Console.WriteLine("CommunityLib: Starting Settlement Type Loop");
                            while (iterateST)
                            {
                                CreateAndOrAddToKeyListPair(sBySGTByT[tSG] as IDictionary, tS, tS, s);

                                if (excludeSGTForS)
                                {
                                    CreateAndOrAddToKeyListPair(sByT, tS, typeof(Settlement), s);
                                    CreateAndOrAddToKeyListPair(sBySGByT[l.soc] as IDictionary, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                }

                                if (tS == targetTS)
                                {
                                    iterateST = false;
                                    excludeSGTForS = false;
                                    //Console.WriteLine("CommunityLib: End Settlement Type Loop");
                                }
                                else
                                {
                                    tS = tS.BaseType;
                                    //Console.WriteLine("CommunityLib: Iterate type to " + tS.Name);
                                }
                            }
                        }

                        if (tSG == targetTSG)
                        {
                            iterateSGT = false;
                            //Console.WriteLine("CommunityLib: End Social Group Type Loop");
                        }
                        else
                        {
                            tSG = tSG.BaseType;
                            //Console.WriteLine("CommunityLib: Iterate type to " + tSG.Name);
                        }
                    }
                }
                else
                {
                    CreateAndOrAddToKeyListPair(lWoSGByTE, tL, tL, l);

                    //Console.WriteLine("CommunityLib: Starting Location Type Loop");
                    while (iterateLT)
                    {
                        CreateAndOrAddToKeyListPair(lByT, tL, tL, l);
                        CreateAndOrAddToKeyListPair(lWoSGByT, tL, tL, l);

                        if (s == null)
                        {
                            CreateAndOrAddToKeyListPair(lWoSByT, tL, tL, l);
                            CreateAndOrAddToKeyListPair(lWoSG_SByT, tL, tL, l);
                        }

                        if (tL == targetTL)
                        {
                            iterateLT = false;
                            excludeSGTForL = false;
                            //Console.WriteLine("CommunityLib: End Location Type Loop");
                        }
                        else
                        {
                            tL = tL.BaseType;
                            //Console.WriteLine("CommunityLib: Iterate type to " + tL.Name);
                        }
                    }

                    if (s != null)
                    {
                        CreateAndOrAddToKeyListPair(sWoSGByTE, tS, tS, s);

                        //Console.WriteLine("CommunityLib: Starting Settlement Type Loop");
                        while (iterateST)
                        {
                            CreateAndOrAddToKeyListPair(sByT, tS, tS, s);
                            CreateAndOrAddToKeyListPair (sWoSGByT, tS, tS, s);

                            if (tS == targetTS)
                            {
                                iterateST = false;
                                excludeSGTForS = false;
                                //Console.WriteLine("CommunityLib: End Settlement Type Loop");
                            }
                            else
                            {
                                tS = tS.BaseType;
                                //Console.WriteLine("CommunityLib: Iterate type to " + tS.Name);
                            }
                        }
                    }
                }
                //Console.WriteLine("CommunityLib: End Loop for location " + l.getName() + " of Type " + l.GetType());
            }
            //Console.WriteLine("CommunityLib: Completed Location Processing");
        }

        public void UpdateLocationDistances()
        {
            // Clear location-distance caches
            Dictionary<Location, Dictionary<Location, double>> dByLfromL = cache.distanceByLocationsFromLocation;
            Dictionary<Location, Dictionary<Location, int>> sByLfromL = cache.stepsByLocationsFromLocation;
            Dictionary<Location, Dictionary<int, IList>> lBySfromL = cache.locationsByStepsFromLocation;
            Dictionary<Location, Dictionary<int, IList>> lBySEfromL = cache.locationsByStepsExclusiveFromLocation;

            dByLfromL.Clear();
            lBySfromL.Clear();
            lBySEfromL.Clear();

            double distance;
            int steps;
            foreach (Location loc in map.locations)
            {
                foreach (Location loc2 in map.locations)
                {
                    distance = map.getDist(loc, loc2);
                    steps = map.getStepDist(loc, loc2);

                    if (!dByLfromL.ContainsKey(loc))
                    {
                        dByLfromL.Add(loc, new Dictionary<Location, double>());
                    }
                    if (!sByLfromL.ContainsKey(loc))
                    {
                        sByLfromL.Add(loc, new Dictionary<Location, int>());
                    }

                    TryCreateSubDictionary(lBySEfromL, loc, typeof(int));
                    TryCreateSubDictionary(lBySfromL, loc, typeof(int));

                    dByLfromL[loc].Add(loc2, distance);
                    sByLfromL[loc].Add(loc2, steps);

                    CreateAndOrAddToKeyListPair(lBySEfromL[loc], steps, typeof(Location), loc2);

                    while (steps <= 50)
                    {
                        CreateAndOrAddToKeyListPair(lBySfromL[loc], steps, typeof(Location), loc2);
                        steps++;
                    }
                }
            }
        }
    }
}