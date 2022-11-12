using Assets.Code;
using System.Collections;
using System;
using System.Collections.Generic;

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

        public void FilterSocialGroups()
        {
            foreach (SocialGroup sG in map.socialGroups)
            {
                Type t = sG.GetType();
                IList value;
                IDictionary dict = cache.socialGroupsByType;
                if (dict.Contains(t))
                {
                    value = dict[t] as IList;
                }
                else
                {
                    value = CreateList(t);
                    dict.Add(t, value);
                }
                value.Add(Convert.ChangeType(sG, t));

                bool flag = false;
                while (flag == false)
                {
                    t = t.BaseType;

                    if (dict.Contains(t))
                    {
                        value = dict[t] as IList;
                    }
                    else
                    {
                        value = CreateList(t);
                        dict.Add(t, value);
                    }
                    value.Add(Convert.ChangeType(sG, t));

                    if (t == typeof(SocialGroup))
                    {
                        flag = true;
                    }
                }
            }
        }

        public void FilterUnits()
        {
            foreach (Unit u in map.units)
            {
                // Initialize universal variable
                Type t = u.GetType();
                bool commandable = u.isCommandable();
                IList valueL;
                IDictionary valueD;

                // Dictionaries being operated on at all level.
                IDictionary uBySG = cache.unitsBySocialGroup;
                IDictionary uBySGByT = cache.unitsBySocialGroupByType;
                IDictionary cUBySG = cache.commandableUnitsBySocialGroup;
                IDictionary cUBySGByT = cache.commandableUnitsBySocialGroupByType;

                // Conduct one-off operations
                // Add units to unitsBySocialGroups
                if (uBySG.Contains(u.society))
                {
                    valueL = uBySG[u.society] as List<Unit>;
                }
                else
                {
                    valueL = new List<Unit>();
                    uBySG.Add(t, valueL);
                }
                valueL.Add(u);
                // Establish subdictionary for social group in unitsBySocialGroupsByType
                if (!uBySGByT.Contains(u.society))
                {
                    valueL = CreateList(t);
                    valueD = CreateDictionary(t, valueL);
                    uBySGByT.Add(u.society, valueD);
                }
                
                if (commandable)
                {
                    // Add units to commandableUnitsBySocialGroups
                    if (cUBySG.Contains(t))
                    {
                        valueL = cUBySG[t] as IList;
                    }
                    else
                    {
                        valueL = CreateList(t);
                        cUBySG.Add(t, valueL);
                    }
                    valueL.Add(u);
                    // Establish subdictionary for social group in commandableUnitsBySocialGroupsByType
                    if (!cUBySGByT.Contains(u.society))
                    {
                        valueL = CreateList(t);
                        valueD = CreateDictionary(t, valueL);
                        cUBySGByT.Add(u.society, valueD);
                    }
                }

                // Initialize loop-only variables
                Type targetT = typeof(Unit);
                bool flag = false;

                IDictionary uByT = cache.unitsByType;
                IDictionary cUByT = cache.commandableUnitsByType;

                // Initialize iteration specific variables
                IDictionary uBySGByT_uByT;
                IDictionary cUBySGByT_uByT;
                // Conduct Operations for all Types t, from obj.GetType() to targetT, inclusively
                while (flag == false)
                {
                    uBySGByT_uByT = uBySGByT[u.society] as IDictionary;
                    cUBySGByT_uByT = cUBySGByT[u.society] as IDictionary;
                    // Add units to unitsByType
                    if (uByT.Contains(t))
                    {
                        valueL = uByT[t] as IList;
                    }
                    else
                    {
                        valueL = CreateList(t);
                        uByT.Add(t, valueL);
                    }
                    valueL.Add(Convert.ChangeType(u, t));
                    // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                    if (uBySGByT_uByT.Contains(t))
                    {
                        valueL = uBySGByT_uByT[t] as IList;
                    }
                    else
                    {
                        valueL = CreateList(t);
                        uBySGByT_uByT.Add(t, valueL);
                    }
                    valueL.Add(Convert.ChangeType(u, t));
                    if (commandable)
                    {
                        // Add units to commandableUnitsByType
                        if (cUByT.Contains(t))
                        {
                            valueL = cUByT[t] as IList;
                        }
                        else
                        {
                            valueL = CreateList(t);
                            cUByT.Add(t, valueL);
                        }
                        valueL.Add(Convert.ChangeType(u, t));
                        // Add units to unitsBySocialGroupByType subdictionary, unitsByType
                        if (cUBySGByT_uByT.Contains(t))
                        {
                            valueL = cUBySGByT_uByT[t] as IList;
                        }
                        else
                        {
                            valueL = CreateList(t);
                            cUBySGByT_uByT.Add(t, valueL);
                        }
                        valueL.Add(Convert.ChangeType(u, t));
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