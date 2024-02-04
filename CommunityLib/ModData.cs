using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class ModData
    {
        public Map map;

        public bool isClean = true;

        private Dictionary<string, ModIntegrationData> modIntegrationData;

        private Dictionary<Culture, ModCultureData> modCultureData;

        private List<Func<Person, Location, UA>> reviveAgentCreationFunctons;

        private HashSet<Type> locusTypes;

        private HashSet<Type> magicTraitTypes;

        private HashSet<Type> naturalWonderTypes;

        private HashSet<Type> vampireTypes;

        public ModData()
        {
            initialiseModIntegrationData();
            initialiseModCultureData();
            initialiseReviveAgentCreationFunctions();
            initialiseLocusTypes();
            initialiseMagicTraitTypes();
            initialiseNaturalWonderTypes();
            initialiseVampireTypes();
        }

        private void initialiseModIntegrationData()
        {
            if (modIntegrationData == null)
            {
                modIntegrationData = new Dictionary<string, ModIntegrationData>();
            }
        }

        private void initialiseModCultureData()
        {
            if (modCultureData == null)
            {
                modCultureData = new Dictionary<Culture, ModCultureData>();
            }
        }

        private void initialiseReviveAgentCreationFunctions()
        {
            if (reviveAgentCreationFunctons == null)
            {
                reviveAgentCreationFunctons = new List<Func<Person, Location, UA>>();
            }
        }

        private void initialiseLocusTypes()
        {
            if (locusTypes == null)
            {
                locusTypes = new HashSet<Type>();
            }
        }

        private void initialiseMagicTraitTypes()
        {
            if(magicTraitTypes == null)
            {
                magicTraitTypes = new HashSet<Type>();
            }
        }

        private void initialiseNaturalWonderTypes()
        {
            if (naturalWonderTypes == null)
            {
                naturalWonderTypes = new HashSet<Type>();
            }
        }

        private void initialiseVampireTypes()
        {
            if(vampireTypes == null)
            {
                vampireTypes = new HashSet<Type>();
            }
        }

        public void clean()
        {
            if (isClean)
            {
                return;
            }

            map = null;
            modIntegrationData.Clear();
            modCultureData.Clear();
            reviveAgentCreationFunctons.Clear();
            locusTypes.Clear();
            magicTraitTypes.Clear();
            naturalWonderTypes.Clear();
            vampireTypes.Clear();

            isClean = true;
        }

        public void onLoad(Map map)
        {
            this.map = map;

            initialiseModIntegrationData();
            initialiseModCultureData();
            initialiseReviveAgentCreationFunctions();
            initialiseLocusTypes();
            initialiseMagicTraitTypes();
            initialiseNaturalWonderTypes();
            initialiseVampireTypes();
        }

        internal void addModIntegrationData(string key, ModIntegrationData intData)
        {
            if (key == "" || intData.assembly == null)
            {
                return;
            }

            initialiseModIntegrationData();

            if (modIntegrationData.TryGetValue(key, out ModIntegrationData intData2) && intData2.assembly == null)
            {
                modIntegrationData[key] = intData;
            }
            else
            {
                modIntegrationData.Add(key, intData);
            }
        }

        internal bool tryGetModIntegrationData(string key, out ModIntegrationData intData) => modIntegrationData.TryGetValue(key, out intData);

        internal void addCultureData(Culture key, ModCultureData data)
        {
            if (key == null || data == null)
            {
                return;
            }

            initialiseModCultureData();

            if (modCultureData.TryGetValue(key, out ModCultureData data2) && data2 == null)
            {
                modCultureData[key] = data;
            }
            else
            {
                modCultureData.Add(key, data);
            }
        }

        internal bool tryGetModCultureData(Culture key, out ModCultureData cultureData) => modCultureData.TryGetValue(key, out cultureData);

        internal Dictionary<Culture, ModCultureData> GetModCultureData() => modCultureData;

        internal void addReviveAgentCreationFunction(Func<Person, Location, UA> func)
        {
            initialiseReviveAgentCreationFunctions();

            if (func != null && !reviveAgentCreationFunctons.Contains(func))
            {
                reviveAgentCreationFunctons.Add(func);
            }
        }

        internal IEnumerable<Func<Person, Location, UA>> iterateReviveAgentCreationFunctions()
        {
            foreach (Func<Person, Location, UA> func in reviveAgentCreationFunctons)
            {
                yield return func;
            }
        }

        internal void addLocusType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Property)))
            {
                return;
            }

            initialiseLocusTypes();

            if (!locusTypes.Contains(t))
            {
                locusTypes.Add(t);
            }
        }

        internal bool isLocusType(Location location)
        {
            foreach (Property property in location.properties)
            {
                if (property is Pr_GeomanticLocus)
                {
                    return true;
                }

                foreach (Type type in locusTypes)
                {
                    if (property.GetType() == type || property.GetType().IsSubclassOf(type))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal void addMagicTraitType(Type t)
        {
            if(!t.IsSubclassOf(typeof(Trait)))
            {
                return;
            }

            initialiseMagicTraitTypes();

            if (!magicTraitTypes.Contains(t))
            {
                magicTraitTypes.Add(t);
            }
        }

        internal bool knowsMagic(Person p)
        {
            foreach (Trait trait in p.traits)
            {
                if (trait is T_MasteryBlood || trait is T_MasteryDeath || trait is T_MasteryGeomancy)
                {
                    return true;
                }

                foreach (Type type in magicTraitTypes)
                {
                    if (trait.GetType() == type || trait.GetType().IsSubclassOf(type))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void addNaturalWonderType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) && !t.IsSubclassOf(typeof(Subsettlement)))
            {
                return;
            }

            initialiseNaturalWonderTypes();

            if (!naturalWonderTypes.Contains(t))
            {
                naturalWonderTypes.Add(t);
            }
        }

        internal bool isNaturalWonder(Location location)
        {
            if (location.settlement != null)
            {
                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Settlement)) && (location.settlement.GetType() == type || location.settlement.GetType().IsSubclassOf(type)))
                    {
                        return true;
                    }

                    foreach (Subsettlement sub in location.settlement.subs)
                    {
                        foreach (Type type2 in naturalWonderTypes)
                        {
                            if (type2.IsSubclassOf(typeof(Subsettlement)) && (sub.GetType() == type2 || sub.GetType().IsSubclassOf(type2)))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        internal void addVampireType(Type t)
        {
            if(!t.IsSubclassOf(typeof(Unit)))
            {
                return;
            }

            initialiseVampireTypes();

            if (!vampireTypes.Contains(t))
            {
                vampireTypes.Add(t);
            }
        }

        internal bool isVampireType(Unit u)
        {
            if (u is UAE_Baroness || u is UAEN_Vampire)
            {
                return true;
            }

            if (u is UAE_Baroness || u is UAEN_Vampire)
            {
                return true;
            }

            foreach (Type type in vampireTypes)
            {
                if (u.GetType() == type || u.GetType().IsSubclassOf(type))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
