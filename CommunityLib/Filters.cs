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

        public IDictionary CreateDictionary(Type keyType, IList value)
        {
            Type genericDictionaryType = typeof(Dictionary<,>).MakeGenericType(new Type[] { keyType, value.GetType() });
            return (IDictionary)Activator.CreateInstance(genericDictionaryType);
        }

        public IList GetOrCreateKeyListPair(IDictionary dict, Type t)
        {
            if (dict.Contains(t))
            {
                return dict[t] as IList;
            }
            else
            {
                IList  value = CreateList(t);
                dict.Add(t, value);
                return value;
            }
        }

        public IDictionary CreateSubDictionary(Type t)
        {
            return CreateDictionary(t, CreateList(t));
        }

        public bool TryCreateSubDictionary(IDictionary dict, object key, Type t)
        {
            if (!dict.Contains(key))
            {
                dict.Add(key, CreateSubDictionary(t));
                return true;
            }
            return false;
        }

        public bool TryCreateSubDictionary(IDictionary dict, Type key, Type t)
        {
            if (!dict.Contains(key))
            {
                dict.Add(key, CreateSubDictionary(t));
                return true;
            }
            return false;
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, Type t, object value)
        {
            GetOrCreateKeyListPair(dict, t).Add(Convert.ChangeType(value, t));
        }

        public void FilterSocialGroups()
        {
            IDictionary dict = cache.socialGroupsByType;
            IDictionary dictE = cache.socialGroupsByTypeExclusive;

            foreach (SocialGroup sG in map.socialGroups)
            {
                // Initialize universal variables
                Type t = sG.GetType();

                // Conduct one-off operations
                // Add to Exclusive.
                CreateAndOrAddToKeyListPair(dictE, t, sG);

                // Initialize loop onl-variables
                Type targetT = typeof(SocialGroup);
                bool flag = false;

                while (flag == false)
                {
                    CreateAndOrAddToKeyListPair(dict, t, sG);

                    if (t == targetT)
                    {
                        flag = true;
                    }
                    else
                    {
                        t = t.BaseType;
                    }
                }
            }
        }

        public void FilterUnits()
        {
            foreach (Unit u in map.units)
            {
                // Initialize universal variables
                Type t = u.GetType();
                bool commandable = u.isCommandable();
                IList valueL;
                IDictionary valueD;

                // Dictionaries being operated on at all level.
                IDictionary uBySG = cache.unitsBySocialGroup;
                IDictionary uBySGE = cache.unitsBySocialGroup;
                IDictionary uBySGByT = cache.unitsBySocialGroupByType;
                IDictionary uBySGByTE = cache.unitsBySocialGroupByType;
                IDictionary cUBySG = cache.commandableUnitsBySocialGroup;
                IDictionary cUBySGE = cache.commandableUnitsBySocialGroup;
                IDictionary cUBySGByT = cache.commandableUnitsBySocialGroupByType;
                IDictionary cUBySGByTE = cache.commandableUnitsBySocialGroupByType;

                // Conduct one-off operations
                // Add units to unitsBySocialGroups
                CreateAndOrAddToKeyListPair(uBySG, t, u);
                CreateAndOrAddToKeyListPair(uBySGE, t, u);
                // GetOrCreate subdictionary for social group in unitsBySocialGroupsByType
                TryCreateSubDictionary(uBySGByT, u.society, t);
                TryCreateSubDictionary(uBySGByTE, u.society, t);
                CreateAndOrAddToKeyListPair(uBySGByTE[u.society] as IDictionary, t, u);

                if (commandable)
                {
                    // Add units to commandableUnitsBySocialGroups
                    CreateAndOrAddToKeyListPair(cUBySG, t, u);
                    CreateAndOrAddToKeyListPair(cUBySGE, t, u);
                    // GetOrCreate subdictionary for social group in commandableUnitsBySocialGroupsByType
                    TryCreateSubDictionary(cUBySGByT, u.society, t);
                    TryCreateSubDictionary(cUBySGByTE, u.society, t);
                    CreateAndOrAddToKeyListPair(cUBySGByTE[u.society] as IDictionary, t, u);
                }

                // Initialize loop-only variables
                Type targetT = typeof(Unit);
                bool flag = false;

                // Conduct Operations for all Types t, from obj.GetType() to targetT, inclusively
                while (flag == false)
                {
                    // The subdictionaries used in this loop were checked for, and if neccesary created, earlier in this method.
                    // Add units to unitsByType
                    CreateAndOrAddToKeyListPair(cache.unitsByType, t, u);
                    // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                    CreateAndOrAddToKeyListPair(uBySGByT[u.society] as IDictionary, t, u);
                    if (commandable)
                    {
                        // Add units to commandableUnitsByType
                        CreateAndOrAddToKeyListPair(cache.commandableUnitsByType, t, u);
                        // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                        CreateAndOrAddToKeyListPair(cUBySGByT[u.society] as IDictionary, t, u);
                    }

                    // Check if Type t is targetT. End loop if it is.
                    // Else, set t to next Type up in inhertiance hierarchy
                    if (t == targetT)
                    {
                        flag = true;
                    }
                    else
                    {
                        t = t.BaseType;
                    }
                }
            }
        }
    }
}