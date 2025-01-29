using Assets.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    public class ModData
    {
        public Map map;

        public Rt_HiddenThoughts hiddenThoughts;

        private SaveData saveData = null;

        private string dataDirPath = "";

        private string dataFileName = "UserData.json";

        public bool isClean = true;

        private Dictionary<string, ModIntegrationData> modIntegrationData;

        private Dictionary<Culture, ModCultureData> modCultureData;

        private List<Func<Person, Location, UA>> reviveAgentCreationFunctons;

        private Dictionary<Soc_Dwarves, int> dwarfExpansionCooldowns;

        // Collections
        private HashSet<Type> locusTypes;

        private HashSet<Type> magicTraitTypes;

        private HashSet<Type> naturalWonderTypes;

        private HashSet<Type> vampireTypes;
        // end

        private List<Type> wonderGenTypes;

        public bool isPlayerTurn = false;

        public Dictionary<HolyOrder, List<ReasonMsg>> influenceGainElder;

        public Dictionary<HolyOrder, List<ReasonMsg>> influenceGainHuman;

        public ModData()
        {
            initialiseModIntegrationData();
            initialiseModCultureData();
            initialiseReviveAgentCreationFunctions();
            initialiseDwarfExpansionCooldowns();

            // Colections
            initialiseLocusTypes();
            initialiseMagicTraitTypes();
            initialiseNaturalWonderTypes();
            initialiseVampireTypes();

            initialiseWonderGenTypes();
            initialiseInfluenceGain();
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

        private void initialiseDwarfExpansionCooldowns()
        {
            if (dwarfExpansionCooldowns == null)
            {
                dwarfExpansionCooldowns = new Dictionary<Soc_Dwarves, int>();
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
                naturalWonderTypes = new HashSet<Type> { typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont) };
            }
        }

        private void initialiseVampireTypes()
        {
            if(vampireTypes == null)
            {
                vampireTypes = new HashSet<Type>();
            }
        }

        private void initialiseWonderGenTypes()
        {
            if (wonderGenTypes == null)
            {
                wonderGenTypes = new List<Type>();
            }
        }

        private void initialiseInfluenceGain()
        {
            if (influenceGainElder == null)
            {
                influenceGainElder = new Dictionary<HolyOrder, List<ReasonMsg>>();
            }

            if (influenceGainHuman == null)
            {
                influenceGainHuman = new Dictionary<HolyOrder, List<ReasonMsg>>();
            }
        }

        internal void initialiseHidenThoughts()
        {
            if (hiddenThoughts == null)
            {
                hiddenThoughts = new Rt_HiddenThoughts(map.locations[0]);
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
            dwarfExpansionCooldowns.Clear();

            // COllections
            locusTypes.Clear();
            magicTraitTypes.Clear();
            naturalWonderTypes.Clear();
            naturalWonderTypes.Add(typeof(Sub_Wonder_DeathIsland));
            naturalWonderTypes.Add(typeof(Sub_Wonder_Doorway));
            naturalWonderTypes.Add(typeof(Sub_Wonder_PrimalFont));

            vampireTypes.Clear();

            wonderGenTypes.Clear();

            influenceGainElder.Clear();
            influenceGainHuman.Clear();

            hiddenThoughts = null;

            isPlayerTurn = false;

            isClean = true;
        }

        public SaveData getSaveData() => saveData;

        public void loadUserData()
        {
            dataDirPath = Path.Combine(Path.GetDirectoryName(ModCore.Get().GetType().Assembly.Location), "..");

            if (!string.IsNullOrEmpty(dataDirPath) && Directory.Exists(dataDirPath))
            {
                string filePath = Path.Combine(dataDirPath, dataFileName);

                if (!File.Exists(filePath))
                {
                    SaveData saveData = new SaveData();
                    string jsonData = JsonUtility.ToJson(saveData);

                    File.WriteAllText(filePath, jsonData);
                    Console.WriteLine("CommunityLib: New file created: " + filePath);
                }
                else
                {
                    Console.WriteLine("CommunityLib: File already exists: " + filePath);

                    string jsonData = File.ReadAllText(filePath);
                    saveData = JsonUtility.FromJson<SaveData>(jsonData);
                }
            }
            else
            {
                Console.WriteLine("CommunityLib: Directory does not exist or is invalid: " + dataDirPath);
            }
        }

        public void saveUserData()
        {
            dataDirPath = Path.Combine(Path.GetDirectoryName(ModCore.Get().GetType().Assembly.Location), "..");

            if (!string.IsNullOrEmpty(dataDirPath) && Directory.Exists(dataDirPath))
            {
                string filePath = Path.Combine(dataDirPath, dataFileName);

                // Serialize the SaveData object to JSON and write it to the file
                string jsonData = JsonUtility.ToJson(saveData);
                File.WriteAllText(filePath, jsonData);

                Console.WriteLine("CommunityLib: User data saved to: " + filePath);
            }
            else
            {
                Console.WriteLine("CommunityLib: Directory does not exist or is invalid: " + dataDirPath);
            }
        }

        public void onLoad(Map map)
        {
            isClean = false;
            this.map = map;
            isPlayerTurn = true;

            if (saveData == null)
            {
                loadUserData();
            }
            saveData.lastPlayedGod = map.overmind.god.getName();
            saveUserData();

            initialiseModIntegrationData();
            initialiseModCultureData();
            initialiseReviveAgentCreationFunctions();
            initialiseDwarfExpansionCooldowns();

            // Collections
            initialiseLocusTypes();
            initialiseMagicTraitTypes();
            initialiseNaturalWonderTypes();
            initialiseVampireTypes();

            initialiseWonderGenTypes();

            initialiseInfluenceGain();
            initialiseHidenThoughts();
        }

        public void onTurnStart(Map map)
        {
            isPlayerTurn = true;
        }

        public void onTurnEnd(Map map)
        {
            isPlayerTurn = false;

            influenceGainElder.Clear();
            influenceGainHuman.Clear();

            if (dwarfExpansionCooldowns.Count > 0)
            {
                Dictionary<Soc_Dwarves, int> mutableDict = new Dictionary<Soc_Dwarves, int>(dwarfExpansionCooldowns);
                foreach (KeyValuePair<Soc_Dwarves, int> kvp in mutableDict)
                {
                    if (kvp.Value - 1 <= 0)
                    {
                        dwarfExpansionCooldowns.Remove(kvp.Key);
                    }
                    else
                    {
                        dwarfExpansionCooldowns[kvp.Key] = kvp.Value - 1;
                    }
                }
            }
        }

        internal void addModIntegrationData(string key, ModIntegrationData intData)
        {
            if (key == "" || intData.assembly == null)
            {
                return;
            }

            initialiseModIntegrationData();

            if (modIntegrationData.TryGetValue(key, out ModIntegrationData intData2))
            {
                if (intData2.assembly == null)
                {
                    modIntegrationData[key] = intData;
                }

                Console.WriteLine($"CommunityLib: ERROR: Key {key} is already registered.");
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

        internal void setDwarvenExpandCooldown(Soc_Dwarves soc)
        {
            if (dwarfExpansionCooldowns.TryGetValue(soc, out int cooldown))
            {
                if (cooldown < 10)
                {
                    dwarfExpansionCooldowns[soc] = 10;
                }
            }
            else
            {
                dwarfExpansionCooldowns.Add(soc, 10);
            }
        }

        internal bool tryGetDwarvenExpansionCooldown(Soc_Dwarves soc, out int cooldown)
        {
            return dwarfExpansionCooldowns.TryGetValue(soc, out cooldown);
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

        internal bool knowsMagicAdvanced(Person p, out List<Trait> magicTraits)
        {
            magicTraits = new List<Trait>();
            bool knowsMagic = false;

            foreach (Trait trait in p.traits)
            {
                if (trait is T_MasteryBlood || trait is T_MasteryDeath || trait is T_MasteryGeomancy)
                {
                    magicTraits.Add(trait);

                    if (trait.level > 0)
                    {
                        knowsMagic = true;
                    }

                    continue;
                }

                foreach (Type type in magicTraitTypes)
                {
                    if (trait.GetType() == type || trait.GetType().IsSubclassOf(type))
                    {
                        magicTraits.Add(trait);

                        if (trait.level > 0)
                        {
                            knowsMagic = true;
                        }

                        break;
                    }
                }
            }

            if (magicTraits.Count > 0)
            {
                return knowsMagic;
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

            naturalWonderTypes.Add(t);
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

            foreach (Type type in vampireTypes)
            {
                if (u.GetType() == type || u.GetType().IsSubclassOf(type))
                {
                    return true;
                }
            }

            if (u.person != null && u.person.species == u.map.species_undead && u.person.traits.Any(t => t is T_VampiricCurse || t is T_TheHunger))
            {
                return true;
            }

            return false;
        }

        internal void addWonderGenType(Type t)
        {
            initialiseWonderGenTypes();

            if (!wonderGenTypes.Contains(t))
            {
                wonderGenTypes.Add(t);
            }
        }

        internal List<Type> getWonderGenTypes() => wonderGenTypes;
    }
}
