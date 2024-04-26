using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;
using UnityEngine;

namespace CommunityLib
{
    public class ModCore : ModKernel
    {
        private static ModCore core;

        public ModData data;

        public static double versionID;

        public Dictionary<Unit, Dictionary<object, Dictionary<string, double>>> randStore;

        private Map map;

        private List<Hooks> registeredHooks = new List<Hooks>();

        private Dictionary<Type, HashSet<Type>> settlementTypesForOrcExpansion = new Dictionary<Type,  HashSet<Type>>();

        private AgentAI agentAI;

        private Hooks hooks;

        public Pathfinding pathfinding;

        private UAENOverrideAI overrideAI;

        public static bool opt_spawnShipwrecks = false;

        public static bool opt_forceShipwrecks = false;

        public static bool opt_panToHolyOrderScreen = true;

        public static bool opt_ophanimFaithTomb = true;

        public static bool opt_allowCulturalMinorSettelementGraphics = true;

        public static bool opt_enhancedTradeRouteLinks = true;

        public static bool opt_forceCommunityLibraryPathfinding = true;

        public static bool opt_DynamicOrcCount = false;

        public static int opt_targetOrcCount = 2;

        public static bool opt_DynamicNaturalWonderCount = false;

        public static int opt_targetNaturalWonderCount = 1;

        public static bool opt_godSort_Alphabetise = false;

        public static bool opt_godSort_swwfFirst = true;

        public static bool opt_godSort_splitModded = true;

        public static bool opt_godSort_bonusLast = true;

        public static bool opt_godSort_minorLate = true;

        public static bool opt_godSort_ignorePrefixes = true;

        public static bool opt_godSort_lastPlayedFirst = false;

        public static bool opt_usePreciseDistanceDivisor = true;

        public static ModCore Get() => core;

        public override void onModsInitiallyLoaded()
        {
            core = this;

            HarmonyPatches.PatchingInit();

            data = new ModData();
            data.loadUserData();
        }

        public override void receiveModConfigOpts_bool(string optName, bool value)
        {
            switch(optName)
            {
                case "Dynamic Orc Horde Count":
                    opt_DynamicOrcCount = value;
                    break;
                case "Dynamic Natural Wonder Count":
                    opt_DynamicNaturalWonderCount = value;
                    break;
                case "Spawn Shipwrecks":
                    opt_spawnShipwrecks = value;
                    break;
                case "Pan To Holy Order Screen":
                    opt_panToHolyOrderScreen = value;
                    break;
                case "Show Ophanim's Faith at Elder Tomb":
                    opt_ophanimFaithTomb = value;
                    break;
                case "Enhanced Trade Route Links":
                    opt_enhancedTradeRouteLinks = value;
                    break;
                case "Force Community Library Pathfinding":
                    opt_forceCommunityLibraryPathfinding = value;
                    break;
                case "Use Precise Distance Divisor":
                    opt_usePreciseDistanceDivisor = value;
                    break;
                case "Allow Culture-Specific Minor Settlement Graphics":
                    opt_allowCulturalMinorSettelementGraphics = value;
                    break;
                case "God Sort: Alphabetise":
                    opt_godSort_Alphabetise = value;
                    break;
                case "God Sort: Keep SWWF First":
                    opt_godSort_swwfFirst = value;
                    break;
                case "God Sort: Differentiate Modded Gods":
                    opt_godSort_splitModded = value;
                    break;
                case "God Sort: Bonus Gods Last":
                    opt_godSort_bonusLast = value;
                    break;
                case "God Sort: Minor Gods After Major Gods":
                    opt_godSort_minorLate = value;
                    break;
                case "God Sort: Ignore God-Name Prefixes":
                    opt_godSort_ignorePrefixes = value;
                    break;
                case "God Sort: Last Played First":
                    opt_godSort_lastPlayedFirst = value;
                    break;
                default:
                    break;
            }
        }

        public override void receiveModConfigOpts_int(string optName, int value)
        {
            switch (optName)
            {
                case "Target Orc Horde Count":
                    opt_targetOrcCount = value;
                    break;
                case "Target Natural Wonder Count":
                    opt_targetNaturalWonderCount = value;
                    break;
                default:
                    break;
            }
        }

        public override void onStartGamePresssed(Map map, List<God> gods)
        {
            data.clean();
        }

        public override void beforeMapGen(Map map)
        {
            opt_forceShipwrecks = false;
            this.map = map;
            data.map = map;
            data.isClean = false;

            data.getSaveData().lastPlayedGod = map.overmind.god.getName();
            data.saveUserData();

            // Set local variables;
            randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();

            //Initialize subclasses.
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit(map);

            pathfinding = new Pathfinding();

            agentAI = new AgentAI(map);

            overrideAI = new UAENOverrideAI(map);

            hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
            eventModifications();
        }

        public override void afterLoading(Map map)
        {
            core = this;
            Get().map = map;

            if (Get().data == null)
            {
                Get().data = new ModData();
            }
            Get().data.onLoad(map);
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit(map);

            // Set local variables
            if (Get().randStore == null)
            {
                Get().randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();
            }

            //Initialize subclasses.
            if (Get().pathfinding == null)
            {
                Get().pathfinding = new Pathfinding();
            }

            agentAI = new AgentAI(map);

            overrideAI = new UAENOverrideAI(map);

            hooks = new HooksInternal(map);
            RegisterHooks(hooks);

            orcExpansionDefaults();
            eventModifications();

            updateSaveGameVersion(map);
        }

        private void getModKernels (Map map)
        {
            foreach (ModKernel kernel in map.mods)
            {
                switch (kernel.GetType().Namespace)
                {
                    case "ShadowsInsectGod.Code":
                        Console.WriteLine("CommunityLib: Cordyceps is Enabled");
                        data.addModIntegrationData("Cordyceps", new ModIntegrationData(kernel.GetType().Assembly, kernel));

                        if (data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.assembly != null)
                        {
                            Type godType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.God_Insect", false);
                            if (godType != null)
                            {
                                intDataCord.typeDict.Add("God", godType);
                                intDataCord.methodInfoDict.Add("God.eat", AccessTools.Method(godType, "eat", new Type[] { typeof(int) }));
                                intDataCord.fieldInfoDict.Add("God.phHome", AccessTools.Field(godType, "phHome"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Cordyceps god Type (ShadowsInsectGod.Code.God_Insect)");
                            }

                            Type droneType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UAEN_Drone", false);
                            if (droneType != null)
                            {
                                intDataCord.typeDict.Add("Drone", droneType);
                                intDataCord.methodInfoDict.Add("Drone.turnTickAI", AccessTools.Method(droneType, "turnTickAI", new Type[0]));
                                intDataCord.fieldInfoDict.Add("Drone.prey", AccessTools.Field(droneType, "prey"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Drone agent Type (ShadowsInsectGod.Code.UAEN_Drone)");
                            }

                            Type hiveType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Set_Hive", false);
                            if (hiveType != null)
                            {
                                intDataCord.typeDict.Add("Hive", hiveType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Hive settlement Type (ShadowsInsectGod.Code.Set_Hive)");
                            }

                            Type larvalType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Pr_LarvalMass", false);
                            if (larvalType != null)
                            {
                                intDataCord.typeDict.Add("LarvalMass", larvalType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Larval mass property Type (ShadowsInsectGod.Code.Pr_LarvalMass)");
                            }

                            Type phFeedType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Pr_Pheromone_Feeding", false);
                            if (phFeedType != null)
                            {
                                intDataCord.typeDict.Add("phFeed", phFeedType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Feeding pheromone property Type (ShadowsInsectGod.Code.Pr_Pheromone_Feeding)");
                            }

                            Type seekType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_SeekPrey", false);
                            if (seekType != null)
                            {
                                intDataCord.typeDict.Add("SeekTask", seekType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Seek prey task Type (ShadowsInsectGod.Code.Task_SeekPrey)");
                            }

                            Type exploreType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_Explore", false);
                            if (exploreType != null)
                            {
                                intDataCord.typeDict.Add("ExploreTask", exploreType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Explore task Type (ShadowsInsectGod.Code.Task_Explore)");
                            }

                            Type homeType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_GoHome", false);
                            if (homeType != null)
                            {
                                intDataCord.typeDict.Add("GoHomeTask", homeType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Go Home task Type (ShadowsInsectGod.Code.Task_GoHome)");
                            }

                            Type haematophageType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.UAEN_Haematophage", false);
                            if (droneType != null)
                            {
                                intDataCord.typeDict.Add("Haematophage", haematophageType);
                                intDataCord.constructorInfoDict.Add("Haematophage", AccessTools.Constructor(haematophageType, new Type[] { typeof(Location), typeof(SocialGroup), typeof(Person) }));
                                intDataCord.methodInfoDict.Add("Haematophage.turnTickAI", AccessTools.Method(haematophageType, "turnTickAI", new Type[0]));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Haematophage agent Type (ShadowsInsectGod.Code.UAEN_Haematophage)");
                            }

                            Type slowHealingType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.Task_SlowHealing", false);
                            if (seekType != null)
                            {
                                intDataCord.typeDict.Add("SlowHealTask", slowHealingType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Slow healing task Type (ShadowsInsectGod.Code.Task_SlowHealing)");
                            }
                        }
                        break;
                    case "CovenExpansion":
                        Console.WriteLine("CommunityLib: Covens, Curses, and Curios is Enabled");
                        ModIntegrationData intDataCCC = new ModIntegrationData(kernel.GetType().Assembly, kernel);
                        Get().data.addModIntegrationData("CovensCursesCurios", intDataCCC);

                        if (Get().data.tryGetModIntegrationData("CovensCursesCurios", out intDataCCC))
                        {
                            Type magicTraitType = intDataCCC.assembly.GetType("CovenExpansion.T_curseWeaving", false);
                            if (magicTraitType != null)
                            {
                                intDataCCC.typeDict.Add("Curseweaving", magicTraitType);
                                Get().registerMagicType(magicTraitType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Curseweaving trait Type (CovenExpansion.T_curseWeaving)");
                            }

                            Type heroicBootsType = intDataCCC.assembly.GetType("CovenExpansion.I_heroicBoot", false);
                            if (heroicBootsType != null)
                            {
                                intDataCCC.typeDict.Add("HeroicBoots", heroicBootsType);
                                intDataCCC.methodInfoDict.Add("HeroicBoots.turnTick", AccessTools.Method(heroicBootsType, "turnTick", new Type[] { typeof(Person) }));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Heroic boots item Type (CovenExpansion.I_heroicBoot)");
                            }
                        }
                        break;
                    case "Wonderblunder_DeepOnes":
                        Console.WriteLine("CommunityLib: DeepOnesPlus is Enabled");
                        ModIntegrationData intDataDOPlus = new ModIntegrationData(kernel.GetType().Assembly, kernel);
                        data.addModIntegrationData("DeepOnesPlus", intDataDOPlus);

                        if (data.tryGetModIntegrationData("DeepOnesPlus", out intDataDOPlus))
                        {
                            Type kernelType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.Modcore", false);
                            if (kernelType != null)
                            {
                                intDataDOPlus.typeDict.Add("Kernel", kernelType);

                                MethodInfo getAbyssalItem = kernelType.GetMethod("getItemFromAbyssalPool", new Type[] { typeof(Map), typeof(UA) });
                                if (getAbyssalItem != null)
                                {
                                    intDataDOPlus.methodInfoDict.Add("getAbyssalItem", getAbyssalItem);
                                }
                                else
                                {
                                    Console.WriteLine("CommunityLib: Failed to get getAbyssalItem method from kernel Type (Wonderblunder_DeepOnes.Modcore.getItemFromAbyssalPool)");
                                }
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get kernel Type (Wonderblunder_DeepOnes.Modcore)");
                            }

                            Type abyssalLocusType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.Pr_AbyssalLocus");
                            if (abyssalLocusType != null)
                            {
                                intDataDOPlus.typeDict.Add("AbyssalLocus", abyssalLocusType);
                                Get().registerLocusType(abyssalLocusType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Abyssal locus property Type (Wonderblunder_DeepOnes.Pr_AbyssalLocus)");
                            }
                        }
                        break;
                    case "God_Flesh":
                        Console.WriteLine("CommunityLib: Escamrak is Enabled");
                        ModIntegrationData intDataEscam = new ModIntegrationData(kernel.GetType().Assembly, kernel);
                        Get().data.addModIntegrationData("Escamrak", intDataEscam);

                        if (Get().data.tryGetModIntegrationData("Escamrak", out intDataEscam))
                        {
                            Type corruptedLocusType = intDataEscam.assembly.GetType("God_Flesh.Pr_CorruptedLocus", false);
                            if (corruptedLocusType != null)
                            {
                                intDataEscam.typeDict.Add("CorruptedLocus", corruptedLocusType);
                                Get().registerLocusType(corruptedLocusType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Corrupted locus property Type (God_Flesh.Pr_CorruptedLocus)");
                            }

                            Type fleshcraftingTraitType = intDataEscam.assembly.GetType("God_Flesh.T_FleshKnowledge", false);
                            if (fleshcraftingTraitType != null)
                            {
                                intDataEscam.typeDict.Add("FleshcraftingTraitType", fleshcraftingTraitType);
                                Get().registerMagicType(fleshcraftingTraitType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Fleshcrafting trait Type (God_Flesh.T_FleshKnowledge)");
                            }
                        }
                        break;
                    case "LivingCharacter":
                        Console.WriteLine("CommunityLib: Living Characters is Enabled");
                        ModIntegrationData intDataLC = new ModIntegrationData(kernel.GetType().Assembly, kernel);
                        Get().data.addModIntegrationData("LivingCharacters", intDataLC);

                        if (Get().data.tryGetModIntegrationData("LivingCharacters", out intDataLC))
                        {
                            Type vampireNobeType = intDataLC.assembly.GetType("LivingCharacters.UAEN_Chars_VampireNoble", false);
                            if (vampireNobeType != null)
                            {
                                intDataLC.typeDict.Add("Vampire", vampireNobeType);
                                Get().registerVampireType(vampireNobeType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Vampire noble agent Type (LivingCharacters.UAEN_Chars_VampireNoble)");
                            }
                        }
                        break;
                    case "ShadowsLib":
                        Console.WriteLine("CommunityLib: Ixthus is Enabled");
                        ModIntegrationData intDataIxthus = new ModIntegrationData(kernel.GetType().Assembly, kernel);
                        Get().data.addModIntegrationData("Ixthus", intDataIxthus);

                        if (Get().data.tryGetModIntegrationData("ixthus", out intDataLC))
                        {
                            Type settlementCryptType = intDataIxthus.assembly.GetType("ShadowsLib.Set_Crypt", false);
                            if (settlementCryptType != null)
                            {
                                intDataIxthus.typeDict.Add("Crypt", settlementCryptType);
                                intDataIxthus.methodInfoDict.Add("Set_Crypt.turnTick", AccessTools.Method(settlementCryptType, "turnTick", new Type[0]));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Settement crypt settlement Type (ShadowsLib.Set_Crypt)");
                            }
                        }
                        break;
                    default:
                        break;
                }
                
                // Code Template for modifying blacklist, non-dependent.
                /*MethodInfo MI_registerSettlementTypeForOrcAxpansion = kernel.GetType().GetMethod("tryGetSettlementTypeForOrcExpansion", new Type[] { typeof(Type), typeof(HashSet<Type>) });

                // If adding a subsettlement blacklist, replace the second parameter "null" with "new HashSet<Type> { typeof(Some Type) }"
                object[] parameters = new object[] { typeof(Set_MinorOther), null };
                MI_registerSettlementTypeForOrcAxpansion.Invoke(kernel, parameters);*/
            }
        }

        public void orcExpansionDefaults()
        {
            registerSettlementTypeForOrcExpansion(typeof(Set_CityRuins));
            registerSettlementTypeForOrcExpansion(typeof(Set_Shipwreck));
            registerSettlementTypeForOrcExpansion(typeof(Set_MinorOther), new HashSet<Type> { typeof(Sub_WitchCoven), typeof(Sub_Wonder_DeathIsland), typeof(Sub_Wonder_Doorway), typeof(Sub_Wonder_PrimalFont), typeof(Sub_Temple) });
        }

        public void eventModifications()
        {
            Dictionary<string, EventRuntime.Field> fields = EventRuntime.fields;
            Dictionary<string, EventRuntime.Property> properties = EventRuntime.properties;

            if (fields.ContainsKey("is_elder_tomb"))
            {
                fields["is_elder_tomb"] = new EventRuntime.TypedField<bool>((EventContext c) => checkIsElderTomb(c.location));
            }

            if (fields.ContainsKey("is_city_ruins"))
            {
                fields["is_city_ruins"] = new EventRuntime.TypedField<bool>((EventContext c) => c.location != null && c.location.settlement != null && c.location.settlement is Set_CityRuins && !(c.location.settlement is Set_Shipwreck));
            }

            if (properties.ContainsKey("TELEPORT_TO_ELDER_TOMB"))
            {
                properties["TELEPORT_TO_ELDER_TOMB"] = new EventRuntime.TypedProperty<string>(delegate (EventContext c, string v)
                {
                    Location location = null;

                    foreach (Location loc in c.map.locations)
                    {
                        if (checkIsElderTomb(loc))
                        {
                            location = loc;
                            break;
                        }
                    }

                    if (location == null)
                    {
                        List<Location> locations = new List<Location>();
                        foreach (Location loc in c.map.locations)
                        {
                            if (!loc.isOcean)
                            {
                                locations.Add(loc);
                            }
                        }

                        if (locations.Count == 1)
                        {
                            location = locations[0];
                        }
                        if (locations.Count > 1)
                        {
                            location = locations[Eleven.random.Next(locations.Count)];
                        }
                    }

                    if (location != null)
                    {
                        c.unit.location.units.Remove(c.unit);
                        c.unit.location = location;
                        location.units.Add(c.unit);
                    }
                });
            }

            if (!properties.ContainsKey("CREATE_SHIPWRECK"))
            {
                properties.Add(
                        "CREATE_SHIPWRECK",
                        new EventRuntime.TypedProperty<string>(delegate (EventContext c, string _)
                        {
                            spawnShipwreck(c.location);
                        })
                    );
            }

            if (!properties.ContainsKey("PLUNDER_SHIPWRECK"))
            {
                properties.Add(
                        "PLUNDER_SHIPWRECK",
                        new EventRuntime.TypedProperty<int>(delegate (EventContext c, int v)
                        {
                            if (c.location.settlement != null)
                            {
                                Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                                if (wreck != null)
                                {
                                    wreck.integrity -= v * wreck.integrityLossPlunder;
                                    if (wreck.integrity <= 0.0)
                                    {
                                        wreck.removeWreck();
                                    }
                                }
                            }
                        })
                    );
            }

            if (!properties.ContainsKey("INCREASE_SHIPWRECK_ALLURE"))
            {
                properties.Add(
                    "INCREASE_SHIPWRECK_ALLURE",
                    new EventRuntime.TypedProperty<int>(delegate (EventContext c, int v)
                    {
                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.allure += v;
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("REINFORCE_SHIPRECK"))
            {
                properties.Add(
                    "REINFORCE_SHIPRECK",
                    new EventRuntime.TypedProperty<bool>(delegate (EventContext c, bool v)
                    {
                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.reinforced = v;
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("DESTROY_SHIPWRECK"))
            {
                properties.Add(
                    "DESTROY_SHIPWRECK",
                    new EventRuntime.TypedProperty<string>(delegate (EventContext c, string v)
                    {
                        if (v == "")
                        {
                            v = "Shipwreck Destroyed by Event Outcome";
                        }

                        if (c.location.settlement != null)
                        {
                            Sub_Shipwreck wreck = (Sub_Shipwreck)c.location.settlement.subs.FirstOrDefault(sub => sub is Sub_Shipwreck);
                            if (wreck != null)
                            {
                                wreck.removeWreck(v);
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("REVIVE_PERSON"))
            {
                {
                    properties.Add(
                        "REVIVE_PERSON",
                        new EventRuntime.TypedProperty<bool>(delegate (EventContext c, bool v)
                        {
                            revivePerson(c.person, v);
                        })
                    );
                }
            }
        }

        public void updateSaveGameVersion(Map map)
        {
            if (opt_targetOrcCount == 0)
            {
                opt_targetOrcCount = 2;
            }

            if (opt_targetNaturalWonderCount == 0)
            {
                opt_targetNaturalWonderCount = 1;
            }
        }

        public override bool interceptDeath(Person person, string v, object killer)
        {
            //Console.WriteLine("OrcsPlus: intercepting person death (ModKernel.interceptDeath)");
            for (int i = 0; i < person.items.Length; i++)
            {
                if (person.items[i] is I_Test_DeathSave)
                {
                    person.items[i] = null;

                    EventManager.ActiveEvent activeEvent = null;
                    EventContext ctx = EventContext.withPerson(person.map, person);

                    foreach (EventManager.ActiveEvent aEvent in EventManager.events.Values)
                    {
                        if (aEvent.type == EventData.Type.INERT && aEvent.data.id.Contains("revive_deathSaveTest"))
                        {
                            activeEvent = aEvent;
                        }
                    }

                    if (activeEvent == null)
                    {
                        person.map.world.prefabStore.popMsg("UNABLE TO FIND VALID REVIVE EVENT FOR REVIVE TEST ITEM HELD BY " + person.getName(), true, true);
                    }
                    else
                    {
                        person.map.world.prefabStore.popEvent(activeEvent.data, ctx, null, true);
                    }
                }
            }

            return false;
        }

        public override string interceptCombatOutcomeEvent(string currentlyChosenEvent, UA victor, UA defeated, BattleAgents battleAgents)
        {
            //Console.WriteLine("OrcsPlus: intercepting agent battle outcome event (ModKernel.interceptCombatOutcomeEvent)");
            if (defeated.person != null && defeated.person.items.Any(i => i is I_Test_DeathSave))
            {
                if (currentlyChosenEvent == victor.getEventID_combatDAL())
                {
                    return victor.getEventID_combatDAR();
                }
                if (currentlyChosenEvent == victor.getEventID_combatDDL())
                {
                    return victor.getEventID_combatDDR();
                }
            }

            return currentlyChosenEvent;
        }

        public void revivePerson(Person victim, bool v)
        {
            //Console.WriteLine("CommunityLib: Reviving Person");
            if (victim == null)
            {
                //Console.WriteLine("CommunityLib: victim is null");
                return;
            }
            //Console.WriteLine("CommunityLib: victim is " + victim.getFullName());

            Map map = victim.map;
            Unit unit = victim.unit;
            UA agent = unit as UA;
            Location location = null;
            SettlementHuman rulerSettlement = null;

            victim.isDead = false;

            if (!map.persons.Contains(victim))
            {
                //Console.WriteLine("CommunityLib: re-adding to map.person");
                map.persons.Add(victim);
            }

            if (!victim.society.people.Contains(victim.index))
            {
                //Console.WriteLine("CommunityLib: re-adding to society.people (" + victim.society.getName() + ")");
                victim.society.people.Add(victim.index);
            }

            if (unit != null)
            {
                //Console.WriteLine("CommunityLib: victim has unit");
                location = unit.location;

                if (agent != null)
                {
                    //Console.WriteLine("CommunityLib: victim has agent");
                }
                else if (unit is UM um)
                {
                    //Console.WriteLine("CommunityLib: victim is commanding military unit");
                    if (!um.isDead && map.units.Contains(um) && um.location.units.Contains(um))
                    {
                        //Console.WriteLine("CommunityLib: reassigning person to military unit");
                        unit.person = victim;
                    }
                    else
                    {
                        if (unit is UM_OrcRaiders raiders)
                        {
                            //Console.WriteLine("CommunityLib: getting subsumed agent from orc raiders");
                            agent = raiders.subsumedUnit;
                        }
                    }
                }
            }

            if (victim.rulerOf > -1 && victim.rulerOf < map.locations.Count)
            {
                //Console.WriteLine("CommunityLib: victim is ruler");
                Location rulerLocation = map.locations[victim.rulerOf];
                if (location == null)
                {
                    location = rulerLocation;
                }

                rulerSettlement = rulerLocation.settlement as SettlementHuman;
                if (rulerSettlement != null)
                {
                    //Console.WriteLine("CommunityLib: victim's settlement still exists");
                    if (rulerSettlement.ruler != null && rulerSettlement.ruler != victim)
                    {
                        //Console.WriteLine("CommunityLib: victim has been deposed");
                        rulerSettlement = null;
                    }
                    else
                    {
                        //Console.WriteLine("CommunityLib: re-assigning victim as ruler of " + rulerSettlement.getName());
                        rulerSettlement.ruler = victim;
                        rulerSettlement.rulerIndex = victim.index;
                    }
                }
            }

            if ((unit == null || unit.isDead || unit is UM) && agent == null && location != null && rulerSettlement == null)
            {
                //Console.WriteLine("CommunityLib: victim needs new agent");
                foreach (Hooks hook in Get().GetRegisteredHooks())
                {
                    UA retValue = hook.onRevivePerson_CreateAgent(victim, location);

                    if (retValue != null)
                    {
                        agent = retValue;
                        break;
                    }
                }

                if (agent == null)
                {
                    foreach (Func<Person, Location, UA> func in Get().data.iterateReviveAgentCreationFunctions())
                    {
                        UA retValue = func(victim, location);

                        if (retValue != null)
                        {
                            agent = retValue;
                            break;
                        }
                    }
                }

                victim.rulerOf = -1;
                if (unit is UM um)
                {
                    um.person = null;
                }


                if (agent == null)
                {
                    //Console.WriteLine("CommunityLib: No mod supplied a custom agent. Getting defaults");
                    if (victim.society is HolyOrder hO)
                    {
                        //Console.WriteLine("CommunityLib: Victim become acolyte");
                        agent = new UAA(location, hO, victim);
                    }
                    else if (victim.species == map.species_human)
                    {
                        //Console.WriteLine("CommunityLib: victim is human");
                        if (victim.stat_command + victim.stat_might >= victim.stat_lore + victim.stat_intrigue)
                        {
                            //Console.WriteLine("CommunityLib: victim becomes Warrior");
                            agent = new UAG_Warrior(location, victim.society, victim);
                        }
                        else
                        {
                            //Console.WriteLine("CommunityLib: Victim becomes Mage");
                            agent = new UAG_Mage(location, victim.society, victim);
                        }
                    }
                    else if (victim.species == map.species_elf)
                    {
                        //Console.WriteLine("CommunityLib: victim is elf, becomes Warrior");
                        agent = new UAG_Warrior(location, victim.society, victim);
                    }
                    else if (victim.species == map.species_deepOne)
                    {
                        //Console.WriteLine("CommunityLib: victim is DeepOne");
                        agent = new UAEN_DeepOne(location, map.soc_dark, victim);
                    }
                    else if (victim.species == map.species_orc && unit != null && unit.society is SG_Orc orcs)
                    {
                        //Console.WriteLine("CommunityLib: victim is orc commanding a military unit, becomes upstart");
                        agent = new UAEN_OrcUpstart(location, orcs, victim);
                    }
                }
            }

            if (agent != null && location != null)
            {
                //Console.WriteLine("CommunityLib: victim's agent is of type " + agent.GetType().Name + " and is at " + location.getName());

                agent.isDead = false;
                agent.person = victim;

                agent.location = location;
                location.units.Add(agent);

                agent.hp = 1;
                if (v)
                {
                    //Console.WriteLine("CommunityLib: victim's HP is restored to full");
                    agent.hp = agent.maxHp;
                }

                if (!location.units.Contains(agent))
                {
                    //Console.WriteLine("CommunityLib: re-adding to location.units");
                    location.units.Add(agent);
                }

                if (!map.units.Contains(agent))
                {
                    //Console.WriteLine("CommunityLib: re-adding to map.units");
                    map.units.Add(agent);
                }

                if (agent.isCommandable())
                {
                    //Console.WriteLine("CommunityLib: victim is commandable");
                    if (!map.overmind.agents.Contains(agent))
                    {
                        //Console.WriteLine("CommunityLib: re-adding to overmind");
                        map.overmind.agents.Add(agent);
                        map.overmind.calculateAgentsUsed();
                    }
                }
            }

            foreach (Hooks hook in Get().GetRegisteredHooks())
            {
                hook.onRevivePerson_EndOfProcess(victim, location);
            }
            //Console.WriteLine("CommunityLib: Revival complete");
        }

        public override void onTurnStart(Map map)
        {
            Get().data.onTurnStart(map);
        }

        public override void onTurnEnd(Map map)
        {
            cleanRandStore(map);

            if (!map.burnInComplete)
            {
                if (map.awarenessOfUnderground > 0.0)
                {
                    Console.WriteLine($"CommunityLib: Awareness of Underground was greater than 0 ({map.awarenessOfUnderground})");
                    map.awarenessOfUnderground = 0.0;
                }
            }

            Get().data.onTurnEnd(map);
        }

        public override void onChallengeComplete(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            OnChallengeComplete.processChallenge(challenge, ua, task_PerformChallenge);
        }

        public override void onCheatEntered(string command)
        {
            Cheats.parseCheat(command, map);
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            int result = 0;

            if (order.isGone())
            {
                Get().data.influenceGainElder.Remove(order);
                return 0;
            }

            if (Get().data.influenceGainElder.TryGetValue(order, out List<ReasonMsg> reasons) && reasons.Count > 0)
            {
                foreach (ReasonMsg reason in reasons)
                {
                    if (msgs != null)
                    {
                        ReasonMsg msg = msgs.FirstOrDefault(m => m.msg == reason.msg);
                        if (msg != null)
                        {
                            msg.value += reason.value;
                        }
                        else
                        {
                            msgs.Add(reason);
                        }
                    }

                    result += (int)Math.Floor(reason.value);
                }
            }

            return result;
        }

        public override int adjustHolyInfluenceGood(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            int result = 0;
            if (order.isGone())
            {
                Get().data.influenceGainHuman.Remove(order);
                return 0;
            }

            if (Get().data.influenceGainHuman.TryGetValue(order, out List<ReasonMsg> reasons) && reasons.Count > 0)
            {
                foreach (ReasonMsg reason in reasons)
                {
                    if (msgs != null)
                    {
                        ReasonMsg msg = msgs.FirstOrDefault(m => m.msg == reason.msg);
                        if (msg != null)
                        {
                            msg.value += reason.value;
                        }
                        else
                        {
                            msgs.Add(reason);
                        }
                    }

                    result += (int)Math.Floor(reason.value);
                }
            }

            return result;
        }

        public void AddInfluenceGainElder(HolyOrder order, ReasonMsg msg)
        {
            if (!Get().data.influenceGainElder.TryGetValue(order, out List<ReasonMsg> influenceGain) || influenceGain == null)
            {
                influenceGain = new List<ReasonMsg>();
                Get().data.influenceGainElder.Add(order, influenceGain);
            }

            ReasonMsg gainMsg = influenceGain.FirstOrDefault(m => m.msg == msg.msg);
            if (gainMsg != null)
            {
                gainMsg.value += msg.value;
            }
            else
            {
                influenceGain.Add(msg);
            }
        }

        public void AddInfluenceGainHuman(HolyOrder order, ReasonMsg msg)
        {
            if (!Get().data.influenceGainHuman.TryGetValue(order, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg>();
                Get().data.influenceGainHuman.Add(order, influenceGain);
            }

            ReasonMsg gainMsg = influenceGain.FirstOrDefault(m => m.msg == msg.msg);
            if (gainMsg != null)
            {
                gainMsg.value += msg.value;
            }
            else
            {
                influenceGain.Add(msg);
            }
        }

        public override void onGraphicalHexUpdated(GraphicalHex graphicalHex)
        {
            if (opt_ophanimFaithTomb)
            {
                if (graphicalHex.map.masker.mask == MapMaskManager.maskType.RELIGION)
                {
                    if (map.overmind.god is God_Ophanim opha && opha.faith != null)
                    {
                        Location location = graphicalHex.hex.location;
                        if (location != null && location.settlement is Set_TombOfGods)
                        {
                            HolyOrder targetOrder = graphicalHex.map.world.ui.uiScrollables.scrollable_threats.targetOrder;
                            if (targetOrder == null || targetOrder == opha.faith)
                            {
                                Color colour = opha.faith.color;
                                if (colour.a > 0f && colour.r > 0f && colour.g > 0f && colour.b > 0f)
                                {
                                    graphicalHex.terrainLayer.color = colour;
                                    graphicalHex.locLayer.color = colour;
                                    graphicalHex.mask.enabled = false;
                                }
                            }
                            else
                            {
                                graphicalHex.mask.color = new Color(0f, 0f, 0f, 0.75f);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the instance of the AgentAI.
        /// </summary>
        /// <returns></returns>
        public AgentAI GetAgentAI()
        {
            return agentAI;
        }

        /// <summary>
        /// Registers an instance of the Hooks class to the Community Library. Only registered instances will be called by the hooks included in the Comunity Library.
        /// </summary>
        /// <param name="hook"></param>
        public void RegisterHooks(Hooks hook)
        {
            if (hook != null && !registeredHooks.Contains(hook))
            {
                registeredHooks.Add(hook);
            }
        }

        public List<Hooks> GetRegisteredHooks()
        {
            return registeredHooks;
        }

        /// <summary>
        /// Safely checks for a value in randStore. If none exists, it sets the value to the new value.
        /// </summary>
        /// <param name="ua"></param>
        /// <param name="innerKey"></param>
        /// <param name="stringKey"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public double tryGetRand(Unit outerKey, object innerKey, string stringKey, double newValue)
        {
            if (outerKey == null || innerKey == null || stringKey == null || stringKey == "")
            {
                return -1.0;
            }

            if (!randStore.ContainsKey(outerKey))
            {
                randStore.Add(outerKey, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!randStore[outerKey].ContainsKey(innerKey))
            {
                randStore[outerKey].Add(innerKey, new Dictionary<string, double>());
            }
            if (!randStore[outerKey][innerKey].ContainsKey(stringKey))
            {
                randStore[outerKey][innerKey].Add(stringKey, newValue);
            }

            return randStore[outerKey][innerKey][stringKey];
        }

        /// <summary>
        /// Safely sets the value to randStore.
        /// </summary>
        /// <param name="outerKey"></param>
        /// <param name="innerKey"></param>
        /// <param name="stringKey"></param>
        /// <param name="value"></param>
        public void setRand(Unit outerKey, object innerKey, string stringKey, double value)
        {
            if (outerKey == null || innerKey == null || stringKey == null || stringKey == "")
            {
                return;
            }

            if (!randStore.ContainsKey(outerKey))
            {
                randStore.Add(outerKey, new Dictionary<object, Dictionary<string, double>>());
            }
            if (!randStore[outerKey].ContainsKey(innerKey))
            {
                randStore[outerKey].Add(innerKey, new Dictionary<string, double>());
            }
            if (!randStore[outerKey][innerKey].ContainsKey(stringKey))
            {
                randStore[outerKey][innerKey].Add(stringKey, value);
            }
            else
            {
                randStore[outerKey][innerKey][stringKey] = value;
            }
        }

        internal void cleanRandStore(Map map)
        {
            List<Unit> outerKeys = new List<Unit>();
            List<Tuple<Unit, Unit>> innerKeys = new List<Tuple<Unit, Unit>>();
            foreach (Unit outerKey in randStore.Keys)
            {
                if (outerKey.isDead || !map.units.Contains(outerKey))
                {
                    outerKeys.Add(outerKey);
                }
                else
                {
                    foreach (object innerKey in randStore[outerKey])
                    {
                        if (innerKey is Unit unit && (unit.isDead || !map.units.Contains(outerKey)))
                        {
                            innerKeys.Add(new Tuple<Unit, Unit>(outerKey, unit));
                        }
                    }
                }
            }

            foreach (Unit unit in outerKeys)
            {
                randStore.Remove(unit);
            }

            foreach (Tuple<Unit, Unit> tuple in innerKeys)
            {
                randStore[tuple.Item1].Remove(tuple.Item2);
            }
        }

        public void registerSettlementTypeForOrcExpansion(Type t, HashSet<Type> subsettlementBlacklist = null)
        {
            if (!t.IsSubclassOf(typeof(Settlement)))
            {
                return;
            }

            if (settlementTypesForOrcExpansion.TryGetValue(t, out HashSet<Type> blacklist))
            {
                if (subsettlementBlacklist != null)
                {
                    blacklist.UnionWith(subsettlementBlacklist);
                }
            }
            else
            {
                if (subsettlementBlacklist == null)
                {
                    subsettlementBlacklist = new HashSet<Type>();
                }

                settlementTypesForOrcExpansion.Add(t, subsettlementBlacklist);
            }
            
            return;
        }

        public bool tryGetSettlementTypeForOrcExpansion(Type t, out HashSet<Type> subsettlementBlacklist) => settlementTypesForOrcExpansion.TryGetValue(t, out subsettlementBlacklist);

        internal Dictionary<Type, HashSet<Type>> getSettlementTypesForOrcExpanion() => settlementTypesForOrcExpansion;

        public void registerModCultureData(Culture culture, ModCultureData modCultureData)
        {
            if (!data.GetModCultureData().ContainsKey(culture))
            {
                data.addCultureData(culture, modCultureData);
            }
        }

        public bool tryGetModCultureData(Culture culture, out ModCultureData modCultureData)
        {
            return data.tryGetModCultureData(culture, out modCultureData);
        }

        public bool checkIsElderTomb(Location location)
        {
            if (location.settlement is Set_TombOfGods)
            {
                return true;
            }

            foreach (Hooks hook in GetRegisteredHooks())
            {
                if (hook.onEvent_IsLocationElderTomb(location))
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkIsUnitSubsumed(Unit u)
        {
            if (u.isDead && u.person != null && !u.person.isDead && u.person.unit != u && !u.person.unit.isDead)
            {
                foreach (Hooks hook in Get().GetRegisteredHooks())
                {
                    if (hook.isUnitSubsumed(u, u.person.unit))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void registerLocusType(Type type) => Get().data.addLocusType(type);

        public bool checkHasLocus(Location location) => Get().data.isLocusType(location);

        public void registerMagicType(Type type) => Get().data.addMagicTraitType(type);

        public bool checkKnowsMagic(Person person) => Get().data.knowsMagic(person);

        public void registerNaturalWonderType(Type type) => Get().data.addNaturalWonderType(type);

        public bool checkIsNaturalWonder(Location location) => Get().data.isNaturalWonder(location);

        public void registerVampireType(Type type) => Get().data.addVampireType(type);

        public bool checkIsVampire(Unit unit) => Get().data.isVampireType(unit);

        public void registerReviveAgentCreationFunction(Func<Person, Location, UA> func) => Get().data.addReviveAgentCreationFunction(func);

        public int getTravelTimeTo(Unit u, Location location)
        {
            if (u == null || location == null)
            {
                return -1;
            }

            int travelTime;

            Location[] path = pathfinding.getPathTo(u.location, location, u);
            if (path == null || path.Length < 2)
            {
                travelTime = (int)Math.Ceiling(u.map.getStepDist(u.location, location) / (double)u.getMaxMoves());
            }
            else
            {
                travelTime = (int)Math.Ceiling((path.Length - 1) / (double)u.getMaxMoves());
            }

            if (travelTime > 0)
            {
                foreach (Hooks hook in GetRegisteredHooks())
                {
                    travelTime = hook.onUnitAI_GetsDistanceToLocation(u, location, path, travelTime);
                }
            }

            return Math.Max(0, travelTime);
        }

        public void forceShipwrecks()
        {
            opt_forceShipwrecks = true;
        }

        public void spawnShipwreck(Location location)
        {
            if (location.settlement == null)
            {
                location.settlement = new Set_Shipwreck(location);
            }
            else
            {
                Sub_Shipwreck wreck = location.settlement.subs.OfType<Sub_Shipwreck>().FirstOrDefault();

                if (wreck == null)
                {
                    wreck = new Sub_Shipwreck(location.settlement, location);
                    location.settlement.subs.Add(wreck);
                }
                else
                {
                    wreck.integrity += Eleven.random.Next(6) + Eleven.random.Next(6);
                }
            }
        }
    }
}
