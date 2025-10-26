using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        #region InternalCaches
        private Dictionary<string, ModIntegrationData> modIntegrationData;

        private Dictionary<Culture, ModCultureData> modCultureData;

        private List<Func<Person, Location, UA>> reviveAgentCreationFunctons;

        private Dictionary<Soc_Dwarves, int> dwarfExpansionCooldowns;

        private Dictionary<PopupEvent, Tuple<EventData, EventContext, string>> eventPopupData;

        private Dictionary<Tuple<ModKernel, int>, MapMaskData> mapMaskData;
        #endregion

        #region Collections
        private HashSet<Type> locusTypes;

        private HashSet<Type> magicTraitTypes;

        private HashSet<Type> wonderTypes;

        private HashSet<Type> naturalWonderTypes;

        private HashSet<Type> vampireTypes;
        #endregion

        public bool isPlayerTurn = false;

        public Dictionary<HolyOrder, List<ReasonMsg>> influenceGainElder;

        public Dictionary<HolyOrder, List<ReasonMsg>> influenceGainHuman;

        private bool _acceleratedTime = false;

        private bool _brokenMakerSleeps = false;

        public int BrokenMakerSleepDuration = 50;

        public bool VinervaDefeatable = false;

        public ModData()
        {
            initializeModIntegrationData();
            initializeModCultureData();
            initializeReviveAgentCreationFunctions();
            initializeDwarfExpansionCooldowns();
            initializeEventPopupData();

            // Colections
            initializeLocusTypes();
            initialiseMagicTraitTypes();
            initializeNaturalWonderTypes();
            InitializeWonderTypes();
            initializeVampireTypes();

            initializeInfluenceGain();
        }

        #region Individual Initialisers
        private void initializeModIntegrationData()
        {
            if (modIntegrationData == null)
            {
                modIntegrationData = new Dictionary<string, ModIntegrationData>();
            }
        }

        private void initializeModCultureData()
        {
            if (modCultureData == null)
            {
                modCultureData = new Dictionary<Culture, ModCultureData>();
            }
        }

        private void initializeReviveAgentCreationFunctions()
        {
            if (reviveAgentCreationFunctons == null)
            {
                reviveAgentCreationFunctons = new List<Func<Person, Location, UA>>();
            }
        }

        private void initializeDwarfExpansionCooldowns()
        {
            if (dwarfExpansionCooldowns == null)
            {
                dwarfExpansionCooldowns = new Dictionary<Soc_Dwarves, int>();
            }
        }

        private void initializeEventPopupData()
        {
            if (eventPopupData == null)
            {
                eventPopupData = new Dictionary<PopupEvent, Tuple<EventData, EventContext, string>>();
            }
        }

        internal void initialiseMapMaskData()
        {
            if (mapMaskData == null)
            {
                mapMaskData = new Dictionary<Tuple<ModKernel, int>, MapMaskData>();
            }

            if (mapMaskData.Count == 0)
            {
                Dictionary<UIKeybinds.Action, MapMaskManager.maskType> maskMappings = UIInputs.getMaskMappings();
                foreach (UIKeybinds.Action action in maskMappings.Keys)
                {
                    StringBuilder textBuilder = new StringBuilder();
                    int assignedID = (int)maskMappings[action];
                    string title = map.masker.getTitleText((MapMaskManager.maskType)assignedID);
                    foreach (char c in title)
                    {
                        if (c == '(')
                        {
                            break;
                        }
                        textBuilder.Append(c);
                    }

                    textBuilder.Replace("View: ", "");
                    textBuilder.Append($" ({UIKeybinds.mappings[action].ToString()})");
                    textBuilder.Replace("Alpha", "");

                    string buttonLabel = textBuilder.ToString();

                    mapMaskData.Add(new Tuple<ModKernel, int>(null, assignedID), new MapMaskData(null, title, buttonLabel, "", assignedID));
                }
            }
        }

        private void initializeLocusTypes()
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

        public void InitializeWonderTypes()
        {
            if (wonderTypes == null)
            {
                wonderTypes = new HashSet<Type>();
            }
        }

        private void initializeNaturalWonderTypes()
        {
            if (naturalWonderTypes == null)
            {
                naturalWonderTypes = new HashSet<Type> { typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont) };
                return;
            }

            if (!naturalWonderTypes.Contains(typeof(Sub_Wonder_DeathIsland)))
            {
                naturalWonderTypes.Add(typeof(Sub_Wonder_DeathIsland));
            }

            if (!naturalWonderTypes.Contains(typeof(Sub_Wonder_Doorway)))
            {
                naturalWonderTypes.Add(typeof(Sub_Wonder_Doorway));
            }

            if (!naturalWonderTypes.Contains(typeof(Sub_Wonder_PrimalFont)))
            {
                naturalWonderTypes.Add(typeof(Sub_Wonder_PrimalFont));
            }
        }

        private void initializeVampireTypes()
        {
            if(vampireTypes == null)
            {
                vampireTypes = new HashSet<Type>();
            }
        }

        private void initializeInfluenceGain()
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
        #endregion

        public void clean()
        {
            if (isClean)
            {
                return;
            }

            map = null;
            modIntegrationData?.Clear();
            modCultureData?.Clear();
            reviveAgentCreationFunctons?.Clear();
            dwarfExpansionCooldowns?.Clear();
            eventPopupData?.Clear();
            mapMaskData?.Clear();

            // Collections
            locusTypes?.Clear();
            magicTraitTypes?.Clear();
            wonderTypes?.Clear();
            naturalWonderTypes?.Clear();
            vampireTypes?.Clear();

            influenceGainElder?.Clear();
            influenceGainHuman?.Clear();

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

            initializeModIntegrationData();
            initializeModCultureData();
            initializeReviveAgentCreationFunctions();
            initializeDwarfExpansionCooldowns();

            // Collections
            initializeLocusTypes();
            initialiseMagicTraitTypes();
            initializeNaturalWonderTypes();
            initializeVampireTypes();

            initializeInfluenceGain();
            initialiseHidenThoughts();

            // Broken Maker Handling
            _acceleratedTime = map.acceleratedTime;
            _brokenMakerSleeps = false;
            BrokenMakerSleepDuration = 50;
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
            eventPopupData.Clear();

            if (dwarfExpansionCooldowns.Count > 0)
            {
                foreach (KeyValuePair<Soc_Dwarves, int> kvp in dwarfExpansionCooldowns.ToList())
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

            if (map.acceleratedTime != _acceleratedTime)
            {
                _acceleratedTime = map.acceleratedTime;

                if (_acceleratedTime)
                {
                    World.Log($"CommunityLib: Transition to accelerated time detected on turn {map.turn}.");
                    _brokenMakerSleeps = true;
                    foreach (var hook in ModCore.Get().HookRegistry.Delegate_onBrokenMakerSleeps_StartOfProcess)
                    {
                        hook(map);
                    }
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        hook.onBrokenMakerSleeps_StartOfProcess(map);
                    }
                }
                else
                {
                    World.Log($"CommunityLib: Transition from accelerated time detected on turn {map.turn}.");
                }
            }

            if (_brokenMakerSleeps)
            {
                BrokenMakerSleepDuration--;
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_onBrokenMakerSleeps_TurnTick)
                {
                    hook(map);
                }
                foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                {
                    hook.onBrokenMakerSleeps_TurnTick(map);
                }

                if (BrokenMakerSleepDuration <= 0)
                {
                    World.Log($"CommunityLib: End of BrokenMaker sleep cycle detected on turn {map.turn}.");
                    _brokenMakerSleeps = false;
                    BrokenMakerSleepDuration = 50;

                    foreach (var hook in ModCore.Get().HookRegistry.Delegate_onBrokenMakerSleeps_EndOfProcess)
                    {
                        hook(map);
                    }
                    foreach (Hooks hook in ModCore.Get().GetRegisteredHooks())
                    {
                        hook.onBrokenMakerSleeps_EndOfProcess(map);
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

            initializeModIntegrationData();

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

            initializeModCultureData();

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
            initializeReviveAgentCreationFunctions();

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

        internal void addEventPopupData(PopupEvent popup, EventData data, EventContext ctx, string forceMessage = null)
        {
            initializeEventPopupData();
            Tuple<EventData, EventContext, string> popupData = new Tuple<EventData, EventContext, string>(data, ctx, forceMessage);
            if (!eventPopupData.ContainsKey(popup))
            {
                eventPopupData.Add(popup, popupData);
            }
        }

        internal bool tryGetEventPopupData(PopupEvent popup, out EventData data, out EventContext ctx, out string forceMessage)
        {
            initializeEventPopupData();
            if (eventPopupData.TryGetValue(popup, out Tuple<EventData, EventContext, string> popupData))
            {
                data = popupData.Item1;
                ctx = popupData.Item2;
                forceMessage = popupData.Item3;
                return true;
            }

            data = null;
            ctx = default;
            forceMessage = null;
            return false;
        }

        internal int addMapMaskData(ModKernel maskingMod, string title, string buttonLabel, string description)
        {
            if (maskingMod == null)
            {
                return -1;
            }

            initialiseMapMaskData();

            int nextAvailableID = -1;
            foreach (MapMaskData data in mapMaskData.Values)
            {
                if (data.MaskingMod == maskingMod && data.Title == title)
                {
                    return -1;
                }

                if (nextAvailableID <= data.AssignedID)
                {
                    nextAvailableID = data.AssignedID + 1;
                }
            }

            if (nextAvailableID == -1)
            {
                return nextAvailableID;
            }

            mapMaskData.Add(new Tuple<ModKernel, int>(maskingMod, nextAvailableID), new MapMaskData(maskingMod, title, buttonLabel, description, nextAvailableID));
            map.world.ui.checkData();
            return nextAvailableID;
        }

        internal bool TryGetMapMaskData(ModKernel maskingMod, int id, out MapMaskData data)
        {
            initialiseMapMaskData();
            return mapMaskData.TryGetValue(new Tuple<ModKernel, int>(maskingMod, id), out data);
        }

        internal IEnumerable<MapMaskData> EnumerateMapMaskData
        {
            get
            {
                if (mapMaskData != null)
                {
                    foreach (MapMaskData data in mapMaskData.Values)
                    {
                        yield return data;
                    }
                }
            }
        }

        internal void addLocusType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Property)))
            {
                return;
            }

            initializeLocusTypes();

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
                if (trait is T_MasteryBlood || trait is T_MasteryDeath || trait is T_MasteryGeomancy || trait is T_MagicMastery)
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
                if (trait is T_MasteryBlood || trait is T_MasteryDeath || trait is T_MasteryGeomancy || trait is T_MagicMastery)
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

        internal void addWonderType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) && !t.IsSubclassOf(typeof(Subsettlement)))
            {
                return;
            }

            InitializeWonderTypes();

            wonderTypes.Add(t);
        }

        internal void addNaturalWonderType(Type t)
        {
            if (!t.IsSubclassOf(typeof(Settlement)) && !t.IsSubclassOf(typeof(Subsettlement)))
            {
                return;
            }

            initializeNaturalWonderTypes();

            naturalWonderTypes.Add(t);
        }

        internal bool isNaturalWonder(Location location)
        {
            if (location == null || location.settlement == null)
            {
                return false;
            }

            if (location.settlement is Set_NaturalWonder)
            {
                return true;
            }

            Type settlementType = location.settlement.GetType();
            foreach (Type type in naturalWonderTypes)
            {
                if (type.IsSubclassOf(typeof(Settlement)) && (settlementType == type || settlementType.IsSubclassOf(type)))
                {
                    return true;
                }
            }

            foreach (Subsettlement sub in location.settlement.subs)
            {
                if (sub is Sub_NaturalWonder)
                {
                    return true;
                }

                Type subsettlementType = sub.GetType();
                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool isNaturalWonder(Location location, out Settlement naturalWonderSettlement, out List<Subsettlement> naturalWonderSubsettlements)
        {
            bool result = false;
            naturalWonderSettlement = null;
            naturalWonderSubsettlements = new List<Subsettlement>();

            if (location == null || location.settlement == null)
            {
                return false;
            }

            if (location.settlement is Set_NaturalWonder)
            {
                naturalWonderSettlement = location.settlement;
                result = true;
            }

            if (naturalWonderSettlement == null)
            {
                Type settlementType = location.settlement.GetType();
                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Settlement)) && (settlementType == type || settlementType.IsSubclassOf(type)))
                    {
                        naturalWonderSettlement = location.settlement;
                        result = true;
                        break;
                    }
                }
            }

            foreach (Subsettlement sub in location.settlement.subs)
            {
                if (sub is Sub_NaturalWonder)
                {
                    naturalWonderSubsettlements.Add(sub);
                    result = true;
                    continue;
                }

                Type subsettlementType = sub.GetType();
                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        naturalWonderSubsettlements.Add(sub);
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        internal bool isWonder(Location location)
        {
            if (location == null || location.settlement == null)
            {
                return false;
            }

            if (location.settlement is Set_Wonder)
            {
                return true;
            }

            Type settlementType = location.settlement.GetType();
            foreach (Type type in wonderTypes)
            {
                if (type.IsSubclassOf(typeof(Settlement)) && (settlementType == type || settlementType.IsSubclassOf(type)))
                {
                    return true;
                }
            }

            foreach (Type type in naturalWonderTypes)
            {
                if (type.IsSubclassOf(typeof(Settlement)) && (settlementType == type || settlementType.IsSubclassOf(type)))
                {
                    return true;
                }
            }

            foreach (Subsettlement sub in location.settlement.subs)
            {
                if (sub is Sub_Wonder)
                {
                    return true;
                }

                Type subsettlementType = sub.GetType();
                foreach (Type type in wonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        return true;
                    }
                }

                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool isWonder(Location location, out Settlement wonderSettlement, out bool settlementIsNaturalWonder, out List<Subsettlement> wonderSubsettlements, out List<Subsettlement> naturalWonderSubsettlements)
        {
            bool result = false;
            wonderSettlement = null;
            settlementIsNaturalWonder = false;
            wonderSubsettlements = new List<Subsettlement>();
            naturalWonderSubsettlements = new List<Subsettlement>();

            if (location == null || location.settlement == null)
            {
                return false;
            }

            if (location.settlement is Set_Wonder)
            {
                result = true;
                wonderSettlement = location.settlement;

                if (location.settlement is Set_NaturalWonder)
                {
                    settlementIsNaturalWonder = true;
                }
            }

            if (wonderSettlement == null)
            {
                Type settlementType = location.settlement.GetType();

                foreach (Type type in wonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Settlement)) && (settlementType == type || settlementType.IsSubclassOf(type)))
                    {
                        wonderSettlement = location.settlement;
                        result = true;
                        break;
                    }
                }

                if (wonderSettlement == null)
                {
                    foreach (Type type in naturalWonderTypes)
                    {
                        if (type.IsSubclassOf(typeof(Settlement)) && (location.settlement.GetType() == type || location.settlement.GetType().IsSubclassOf(type)))
                        {
                            wonderSettlement = location.settlement;
                            settlementIsNaturalWonder = true;
                            result = true;
                            break;
                        }
                    }
                }
            }

            foreach (Subsettlement sub in location.settlement.subs)
            {
                if (sub is Sub_Wonder)
                {
                    result = true;

                    if (sub is Sub_NaturalWonder)
                    {
                        naturalWonderSubsettlements.Add(sub);
                    }
                    else
                    {
                        wonderSubsettlements.Add(sub);
                    }

                    continue;
                }

                Type subsettlementType = sub.GetType();
                bool found = false;
                foreach (Type type in wonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        wonderSubsettlements.Add(sub);
                        result = true;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    continue;
                }

                foreach (Type type in naturalWonderTypes)
                {
                    if (type.IsSubclassOf(typeof(Subsettlement)) && (subsettlementType == type || subsettlementType.IsSubclassOf(type)))
                    {
                        naturalWonderSubsettlements.Add(sub);
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        internal void addVampireType(Type t)
        {
            if(!t.IsSubclassOf(typeof(Unit)))
            {
                return;
            }

            initializeVampireTypes();

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
                if (type.IsAssignableFrom(u.GetType()))
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
    }
}
