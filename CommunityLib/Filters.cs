using Assets.Code;
using System.Collections;
using System;
using System.Collections.Generic;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

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
                Type t = sG.GetType();

                // Conduct one-off operations
                //Console.WriteLine("CommunityLib: Conducting one-off operations");
                // Add to Exclusive.
                //Console.WriteLine("CommunityLib: Add to sGByTE");
                CreateAndOrAddToKeyListPair(dictE, t, t, sG);

                //Console.WriteLine("CommunityLib: Starting While Loop");
                // Initialize loop onl-variables
                Type targetT = typeof(SocialGroup);
                bool flag = false;

                while (flag == false)
                {
                    //Console.WriteLine("CommunityLib: Add to sGByT as " + t.Name);
                    CreateAndOrAddToKeyListPair(dict, t, t, sG);

                    if (t == targetT)
                    {
                        flag = true;
                        //Console.WriteLine("CommunityLib: End Loop");
                    }
                    else
                    {
                        t = t.BaseType;
                        //Console.WriteLine("CommunityLib: Iterate type to " + t.Name);
                    }
                }
            }
        }

        public void FilterUnits()
        {
            foreach (Unit u in map.units)
            {
                //Console.WriteLine("CommunityLib: Filtering unit " + u.getName() + " of type: " + u.GetType().Name);
                // Initialize universal variables
                Type t = u.GetType();
                bool commandable = u.isCommandable();

                // Dictionaries being operated on at all level.
                IDictionary uBySG = cache.unitsBySocialGroup;
                IDictionary uBySGE = cache.unitsBySocialGroup;
                IDictionary uBySGByT = cache.unitsBySocialGroupByType;
                IDictionary uBySGByTE = cache.unitsBySocialGroupByType;
                IDictionary cUBySG = cache.commandableUnitsBySocialGroup;
                IDictionary cUBySGE = cache.commandableUnitsBySocialGroup;
                IDictionary cUBySGByT = cache.commandableUnitsBySocialGroupByType;
                IDictionary cUBySGByTE = cache.commandableUnitsBySocialGroupByType;

                //Console.WriteLine("CommunityLib: Conducting one-off operations");
                // Conduct one-off operations
                // Add units to unitsBySocialGroups
                //Console.WriteLine("CommunityLib: Add to uBySG and uBySGE");
                CreateAndOrAddToKeyListPair(uBySG, u.society, typeof(Unit), u);
                CreateAndOrAddToKeyListPair(uBySGE, u.society, typeof(Unit), u);
                // GetOrCreate subdictionary for social group in unitsBySocialGroupsByType
                //Console.WriteLine("CommunityLib: Establishing <SocialGroup, SubDictionary<" + typeof(Type).Name + ", List<" + t.Name + ">> Pairs");
                TryCreateSubDictionary(uBySGByT, u.society, typeof(Type));
                TryCreateSubDictionary(uBySGByTE, u.society, typeof(Type));
                CreateAndOrAddToKeyListPair(uBySGByTE[u.society] as IDictionary, t, t, u);

                if (commandable)
                {
                    // Add units to commandableUnitsBySocialGroups
                    //Console.WriteLine("CommunityLib: Add to cUBySG");
                    CreateAndOrAddToKeyListPair(cUBySG, u.society, typeof(Unit), u);
                    CreateAndOrAddToKeyListPair(cUBySGE, u.society, typeof(Unit), u);
                    // GetOrCreate subdictionary for social group in commandableUnitsBySocialGroupsByType
                    //Console.WriteLine("CommunityLib: Establishing Key SG, SubDictionary Pairs for commandable");
                    TryCreateSubDictionary(cUBySGByT, u.society, typeof(Type));
                    TryCreateSubDictionary(cUBySGByTE, u.society, typeof(Type));
                    CreateAndOrAddToKeyListPair(cUBySGByTE[u.society] as IDictionary, t, t, u);
                }

                //Console.WriteLine("CommunityLib: Starting While Loop");
                // Initialize loop-only variables
                Type targetT = typeof(Unit);
                bool flag = false;

                // Conduct Operations for all Types t, from obj.GetType() to targetT, inclusively
                while (flag == false)
                {
                    // The subdictionaries used in this loop were checked for, and if neccesary created, earlier in this method.
                    // Add units to unitsByType
                    //Console.WriteLine("CommunityLib: Add to uByT");
                    CreateAndOrAddToKeyListPair(cache.unitsByType, t, t, u);
                    // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                    //Console.WriteLine("CommunityLib: Add to uBySGByT[" + u.society.name + "]");
                    CreateAndOrAddToKeyListPair(uBySGByT[u.society] as IDictionary, t, t, u);
                    if (commandable)
                    {
                        // Add units to commandableUnitsByType
                        //Console.WriteLine("CommunityLib: Add to cUByT");
                        CreateAndOrAddToKeyListPair(cache.commandableUnitsByType, t, t, u);
                        // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                        //Console.WriteLine("CommunityLib: Add to cUBySGByT[" + u.society.name + "]");
                        CreateAndOrAddToKeyListPair(cUBySGByT[u.society] as IDictionary, t, t, u);
                    }

                    // Check if Type t is targetT. End loop if it is.
                    // Else, set t to next Type up in inhertiance hierarchy
                    if (t == targetT)
                    {
                        flag = true;
                        //Console.WriteLine("CommunityLib: End Loop");
                    }
                    else
                    {
                        t = t.BaseType;
                        //Console.WriteLine("CommunityLib: Iterate type to " + t.Name);
                    }
                }
            }
        }
    }
}