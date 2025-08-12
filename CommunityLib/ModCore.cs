using Assets.Code;
using Assets.Code.Modding;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static SortedDictionaryProvider;

namespace CommunityLib
{
    public class ModCore : ModKernel
    {
        private static ModCore core;

        public static ModCore Get() => core;

        public ModData data;

        public ManagerTradeRoutes tradeRouteManager;

        public static double versionID;

        public Dictionary<Unit, Dictionary<object, Dictionary<string, double>>> randStore;

        private Map map;

        private List<Hooks> registeredHooks = new List<Hooks>();

        public HooksDelegateRegistry HookRegistry;

        private Dictionary<Type, HashSet<Type>> settlementTypesForOrcExpansion;

        private AgentAI agentAI;

        internal HooksInternal hooks;

        private UAENOverrideAI overrideAI;

        private ConditionalUAENOverrideAI conditionalOverrideAI;

        public static bool opt_autoRelaunch = false;

        public static bool opt_spawnShipwrecks = false;

        public static bool opt_forceShipwrecks = false;

        public static bool opt_panToHolyOrderScreen = true;

        public static bool opt_noFaithButtonText = true;

        public static bool opt_ophanimFaithTomb = true;

        public static bool opt_ophanimEventMessageSwap = false;

        public static bool opt_prophetTrait = false;

        public static bool opt_chosenOneDeathMessage = true;

        public static bool opt_allowCulturalMinorSettelementGraphics = true;

        public static bool opt_enhancedTradeRouteLinks = true;

        public static bool opt_darkProphets = false;

        public static bool opt_defeatableVinerva = true;

        public static bool opt_DynamicOrcCount = false;

        public static int opt_targetOrcCount = 2;

        public static bool opt_DynamicNaturalWonderCount = false;

        public static bool opt_DuplicateWonders = false;

        public static int opt_targetNaturalWonderCount = 1;

        public static bool opt_NoCountForcedWonders = false;

        public static bool opt_godSort_Alphabetise = false;

        public static bool opt_godSort_Random = false;

        public static bool opt_godSort_swwfFirst = true;

        public static bool opt_godSort_splitModded = true;

        public static bool opt_godSort_bonusLast = true;

        public static bool opt_godSort_minorLate = true;

        public static bool opt_godSort_ignorePrefixes = true;

        public static bool opt_godSort_ignoreThe = false;

        public static bool opt_godSort_lastPlayedFirst = false;

        public static bool opt_usePreciseDistanceDivisor = true;

        public static bool opt_realisticTradeRoutes = false;

        public static bool opt_denseTradeRoutes = false;

        public static bool opt_dwarven_layLow = false;

        public static bool opt_dwarven_enshadow = false;

        public static bool opt_dwarven_expansion = false;

        public static bool opt_dwarven_fortresses = false;

        public static int opt_targetDwarfCount = 1;

        public static bool opt_dynamicDwarfCount = false;

        public static int opt_wonderPriority_entrance = 1;

        public static int opt_wonderPriority_brother = 1;

        public static int opt_wonderPriority_font = 1;

        public override void onModsInitiallyLoaded()
        {
            core = this;
            settlementTypesForOrcExpansion = new Dictionary<Type, HashSet<Type>>();
            registeredHooks = new List<Hooks>();
            randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();

            HarmonyPatches.PatchingInit();

            data = new ModData();
            data.loadUserData();
        }

        public override void receiveModConfigOpts_bool(string optName, bool value)
        {
            switch(optName)
            {
                case "Auto-Relaunch on Mod List Change":
                    opt_autoRelaunch = value;
                    break;
                case "Dark Prophets":
                    opt_darkProphets = value;
                    break;
                case "Defeatable Vinerva":
                    opt_defeatableVinerva = value;
                    break;
                case "Enshadow in Dwarven Cities":
                    opt_dwarven_enshadow = value;
                    break;
                case "Lay Low in Dwarven Cities":
                    opt_dwarven_layLow = value;
                    break;
                case "Dwarven Expansion Mechanics":
                    opt_dwarven_expansion = value;
                    break;
                case "Dwarven Surface Fortresses":
                    opt_dwarven_fortresses = value;
                    break;
                case "Dynamic Dwarven Civilization Count":
                    opt_dynamicDwarfCount = value;
                    break;
                case "Dynamic Orc Horde Count":
                    opt_DynamicOrcCount = value;
                    break;
                case "Dynamic Natural Wonder Count":
                    opt_DynamicNaturalWonderCount = value;
                    break;
                case "Allow Duplicate Wonders":
                    opt_DuplicateWonders = value;
                    break;
                case "Forced Wonders Do Not Count":
                    opt_NoCountForcedWonders = value;
                    break;
                case "Spawn Shipwrecks":
                    opt_spawnShipwrecks = value;
                    break;
                case "Pan To Holy Order Screen":
                    opt_panToHolyOrderScreen = value;
                    break;
                case "Display 'No Faith' on Faith Button":
                    opt_noFaithButtonText = value;
                    break;
                case "Show Ophanim's Faith at Elder Tomb":
                    opt_ophanimFaithTomb = value;
                    break;
                case "Ophanim Perfection Event To Message":
                    opt_ophanimEventMessageSwap = value;
                    break;
                case "Prophet Trait":
                    opt_prophetTrait = value;
                    break;
                case "Chosen One Death Message":
                    opt_chosenOneDeathMessage = value;
                    break;
                case "Enhanced Trade Route Links":
                    opt_enhancedTradeRouteLinks = value;
                    break;
                case "Dense Trade Routes":
                    opt_denseTradeRoutes = value;
                    break;
                case "Realistic Trade Routes":
                    opt_realisticTradeRoutes = value;
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
                case "God Sort: Random":
                    opt_godSort_Random = value;
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
                case "God Sort: Ignore \"The\" at Start of God Names":
                    opt_godSort_ignoreThe = value;
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
                case "Target Dwarven Civilization Count":
                    opt_targetDwarfCount = value;
                    break;
                case "Target Orc Horde Count":
                    opt_targetOrcCount = value;
                    break;
                case "Target Natural Wonder Count":
                    opt_targetNaturalWonderCount = value;
                    break;
                case "Wonder Priority: The Entrance":
                    opt_wonderPriority_entrance = value;
                    break;
                case "Wonder Priority: Brother of Sleep":
                    opt_wonderPriority_brother = value;
                    break;
                case "Wonder Priority: Primordial Font":
                    opt_wonderPriority_font = value;
                    break;
                default:
                    break;
            }
        }

        public override void onStartGamePresssed(Map map, List<God> gods)
        {
            settlementTypesForOrcExpansion.Clear();
            registeredHooks.Clear();
            HookRegistry = new HooksDelegateRegistry();
            randStore.Clear();
            data.clean();
            tradeRouteManager = new ManagerTradeRoutes(map);
        }

        public override void beforeMapGen(Map map)
        {
            opt_forceShipwrecks = false;
            core = this;
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

            agentAI = new AgentAI(map);

            overrideAI = new UAENOverrideAI(map);
            conditionalOverrideAI = new ConditionalUAENOverrideAI(map);

            hooks = new HooksInternal(map);

            orcExpansionDefaults();
            eventModifications();
        }

        public override void afterMapGenBeforeHistorical(Map map)
        {
            Get().data.initialiseHidenThoughts();
        }

        public override void afterLoading(Map map)
        {
            core = this;
            this.map = map;

            if (Get().data == null)
            {
                Get().data = new ModData();
            }
            Get().data.onLoad(map);
            if (HookRegistry == null)
            {
                HookRegistry = new HooksDelegateRegistry();
            }
            getModKernels(map);
            HarmonyPatches_Conditional.PatchingInit(map);

            // Set local variables
            if (randStore == null)
            {
                randStore = new Dictionary<Unit, Dictionary<object, Dictionary<string, double>>>();
            }

            agentAI = new AgentAI(map);

            overrideAI = new UAENOverrideAI(map);
            conditionalOverrideAI = new ConditionalUAENOverrideAI(map);

            hooks = new HooksInternal(map);

            settlementTypesForOrcExpansion = new Dictionary<Type, HashSet<Type>>();
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
                    case "ProductionGod":
                        Console.WriteLine("CommunityLib: Aberrant Metal is Enabled");
                        ModIntegrationData intDataMetal = new ModIntegrationData(kernel);
                        data.addModIntegrationData("AberrantMetal", intDataMetal);

                        if (data.tryGetModIntegrationData("Adolia", out intDataMetal))
                        {
                            Type godType = intDataMetal.assembly.GetType("ProductionGod.God_FactoryGod", false);
                            if (godType != null)
                            {
                                intDataMetal.typeDict.Add("FacelessMemory", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Abberant Metal god Type (ProductionGod.God_FactoryGod)");
                            }

                            Type factoryType = intDataMetal.assembly.GetType("ProductionGod.Settlement_Factory", false);
                            if (factoryType != null)
                            {
                                intDataMetal.typeDict.Add("Factory", factoryType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Adiolia agent Type (ProductionGod.Settlement_Factory)");
                            }
                        }
                        break;
                    case "FacelessMemory":
                        Console.WriteLine("CommunityLib: Adolia is Enabled");
                        ModIntegrationData intDataAdolia = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Adolia", intDataAdolia);

                        if (data.tryGetModIntegrationData("Adolia", out intDataAdolia))
                        {
                            Type godType = intDataAdolia.assembly.GetType("FacelessMemory.God_FacelessMemory", false);
                            if (godType != null)
                            {
                                intDataAdolia.typeDict.Add("FacelessMemory", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Faceless memory god Type (FacelessMemory.God_FacelessMemory)");
                            }

                            Type adoliaType = intDataAdolia.assembly.GetType("FacelessMemory.UA_Memory_Adolia", false);
                            if (adoliaType != null)
                            {
                                intDataAdolia.typeDict.Add("Adolia", adoliaType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Adiolia agent Type (FacelessMemory.UA_Memory_Adolia)");
                            }

                            Type adoliaAgentType = intDataAdolia.assembly.GetType("FacelessMemory.UA_Memory_AdoliaAgent", false);
                            if (adoliaAgentType != null)
                            {
                                intDataAdolia.typeDict.Add("AdoliaAgent", adoliaAgentType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Adolia Agent agent Type (FacelessMemory.UA_Memory_AdoliaAgent)");
                            }
                        }
                        break;
                    case "Bandits_and_Crime":
                        Console.WriteLine("CommunityLib: Bandits and Crime is Enabled");
                        ModIntegrationData intDataBandit = new ModIntegrationData(kernel);
                        data.addModIntegrationData("BanditsAndCrime", intDataBandit);

                        if (data.tryGetModIntegrationData("BanditsAndCrime", out intDataBandit))
                        {
                            Type brigandType = intDataBandit.assembly.GetType("Bandits_and_Crime.UAE_Brigand", false);
                            if (brigandType != null)
                            {
                                intDataBandit.typeDict.Add("Brigand", brigandType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Brigand agent Type (Bandits_and_Crime.UAE_Brigand)");
                            }

                            Type lawbreakerType = intDataBandit.assembly.GetType("Bandits_and_Crime.UAE_Lawbreaker", false);
                            if (brigandType != null)
                            {
                                intDataBandit.typeDict.Add("Lawbreaker", lawbreakerType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Lawbreaker agent Type (Bandits_and_Crime.UAE_Lawbreaker)");
                            }
                        }
                        break;
                    case "God_Love":
                        Console.WriteLine("CommunityLib: Chandalor is Enabled");
                        ModIntegrationData intDataChand = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Chandalor", intDataChand);

                        if (data.tryGetModIntegrationData("Chandalor", out intDataChand))
                        {
                            Type godType = intDataChand.assembly.GetType("God_Love.God_Curse", false);
                            if (godType != null)
                            {
                                intDataChand.typeDict.Add("Chandalor", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Chandalor god Type (God_Love.God_Curse)");
                            }
                        }
                        break;
                    case "ShadowsInsectGod.Code":
                        Console.WriteLine("CommunityLib: Cordyceps is Enabled");
                        data.addModIntegrationData("Cordyceps", new ModIntegrationData(kernel));

                        if (data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord))
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

                            Type spawnDroneFromHumanType = intDataCord.assembly.GetType("ShadowsInsectGod.Code.P_SpawnDroneFromHuman", false);

                            if (spawnDroneFromHumanType != null)
                            {
                                intDataCord.typeDict.Add("SpawnDroneFromHuman", spawnDroneFromHumanType);
                                intDataCord.methodInfoDict.Add("SpawnDroneFromHuman.cast", AccessTools.Method(spawnDroneFromHumanType, "cast", new Type[] { typeof(Unit) }));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Spawn drown from human power Type (ShadowsInsectGod.Code.P_SpawnDroneFromHuman)");
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
                    case "CourtesanAgent":
                        Console.WriteLine("CommunityLib: The Courtesan is Enabled");
                        ModIntegrationData intDataCourtesan = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Courtesan", intDataCourtesan);

                        if (data.tryGetModIntegrationData("Courtesan", out intDataCourtesan))
                        {
                            Type courtesanType = intDataCourtesan.assembly.GetType("CourtesanAgent.UAE_Courtesan", false);
                            if (courtesanType != null)
                            {
                                intDataCourtesan.typeDict.Add("Courtesan", courtesanType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Courtesan agent Type (CourtesanAgent.UAE_Courtesan)");
                            }
                        }
                        break;
                    case "CovenExpansion":
                        Console.WriteLine("CommunityLib: Covens, Curses, and Curios is Enabled");
                        ModIntegrationData intDataCCC = new ModIntegrationData(kernel);
                        data.addModIntegrationData("CovensCursesCurios", intDataCCC);

                        if (data.tryGetModIntegrationData("CovensCursesCurios", out intDataCCC))
                        {
                            if (intDataCCC.typeDict.TryGetValue("Kernel", out Type kernelType))
                            {
                                MethodInfo MI_afterMapGenAfterHistorical = kernelType.GetMethod("afterMapGenAfterHistorical", new Type[] { typeof(Map) });
                                if (MI_afterMapGenAfterHistorical != null)
                                {
                                    intDataCCC.methodInfoDict.Add("Kernel.afterMapGenAfterHistorical", MI_afterMapGenAfterHistorical);
                                }
                                else
                                {
                                    Console.WriteLine("CommunityLib: Failed to get afterMapGenAfterHistorical Method from kernel Type (CovenExpansion.ModcoreCovenExpansion.afterMapGenAfterHistorical)");
                                }
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get mod kernel Type (CovenExpansion.ModcoreCovenExpansion)");
                            }

                            Type magicTraitType = intDataCCC.assembly.GetType("CovenExpansion.T_curseWeaving", false);
                            if (magicTraitType != null)
                            {
                                intDataCCC.typeDict.Add("Curseweaving", magicTraitType);
                                registerMagicType(magicTraitType);
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

                            Type toadType = intDataCCC.assembly.GetType("CovenExpansion.UAEN_Toad", false);
                            if (toadType != null)
                            {
                                intDataCCC.typeDict.Add("UAEN_Toad", toadType);
                                intDataCCC.methodInfoDict.Add("UAEN_Toad.addChallenges", toadType.GetMethod("addChallenges", new Type[] { typeof(Location), typeof(List<Challenge>) }));
                                intDataCCC.fieldInfoDict.Add("UAEN_Toad.Squash", toadType.GetField("ch_Squash"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Toad UAEN Type (CovenExpansion.UAEN_Toad)");
                            }

                            Type ribbitType = intDataCCC.assembly.GetType("CovenExpansion.Rt_Ribbit", false);
                            if (ribbitType != null)
                            {
                                intDataCCC.typeDict.Add("Rt_Ribbit", ribbitType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Ribbit ritual Type (CovenExpansion.Rt_Ribbit)");
                            }

                            Type pigeonType = intDataCCC.assembly.GetType("CovenExpansion.UAEN_Pigeon", false);
                            if (pigeonType != null)
                            {
                                intDataCCC.typeDict.Add("UAEN_Pigeon", pigeonType);
                                intDataCCC.methodInfoDict.Add("UAEN_Pigeon.turnTick", pigeonType.GetMethod("turnTick", new Type[] { typeof(Map) }));
                                intDataCCC.methodInfoDict.Add("UAEN_Pigeon.gainPigeon", pigeonType.GetMethod("gainPigeon", new Type[] { typeof(UA) }));
                                intDataCCC.fieldInfoDict.Add("UAEN_Pigeon.target", pigeonType.GetField("target"));
                                intDataCCC.fieldInfoDict.Add("UAEN_Pigeon.owner", pigeonType.GetField("owner"));
                                intDataCCC.fieldInfoDict.Add("UAEN_Pigeon.returning", pigeonType.GetField("returning"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Pigeon UAEN Type (CovenExpansion.UAEN_Pigeon)");
                            }

                            Type flyingPigeonType = intDataCCC.assembly.GetType("CovenExpansion.Rt_flyingPigeon", false);
                            if (flyingPigeonType != null)
                            {
                                intDataCCC.typeDict.Add("Rt_flyingPigeon", flyingPigeonType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get flying pigeon ritual Type (CovenExpansion.Rt_flyingPigeon)");
                            }
                        }
                        break;
                    case "Wonderblunder_DeepOnes":
                        Console.WriteLine("CommunityLib: DeepOnesPlus is Enabled");
                        ModIntegrationData intDataDOPlus = new ModIntegrationData(kernel);
                        data.addModIntegrationData("DeepOnesPlus", intDataDOPlus);

                        if (data.tryGetModIntegrationData("DeepOnesPlus", out intDataDOPlus))
                        {
                            if (intDataDOPlus.typeDict.TryGetValue("Kernel", out Type kernelType))
                            {
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
                                registerLocusType(abyssalLocusType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Abyssal locus property Type (Wonderblunder_DeepOnes.Pr_AbyssalLocus)");
                            }

                            Type fishermanType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.UAE_Wonderblunder_Fisherman");
                            if (fishermanType != null)
                            {
                                intDataDOPlus.typeDict.Add("Fisherman", fishermanType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Fisherman agent Type (Wonderblunder_DeepOnes.UAE_Wonderblunder_Fisherman)");
                            }

                            Type drownedProphetType = intDataDOPlus.assembly.GetType("Wonderblunder_DeepOnes.UAEN_DrownedProphet");
                            if (drownedProphetType != null)
                            {
                                intDataDOPlus.typeDict.Add("DrownedProphet", drownedProphetType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Drowned Prophet agent Type (Wonderblunder_DeepOnes.UAEN_DrownedProphet)");
                            }
                        }
                        break;
                    case "Delver":
                        Console.WriteLine("CommunityLib: The Delver is Enabled");
                        ModIntegrationData intDataDelv = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Delver", intDataDelv);

                        if (data.tryGetModIntegrationData("Delver", out intDataDelv))
                        {
                            Type delverAgentType = intDataDelv.assembly.GetType("Delver.UAE_Delver", false);
                            if (delverAgentType != null)
                            {
                                intDataDelv.typeDict.Add("Delver", delverAgentType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Delver agent Type (Delver.UAE_Delver)");
                            }
                        }

                        break;
                    case "Duelist":
                        Console.WriteLine("CommunityLib: The Duelist is Enabled");
                        ModIntegrationData intDataDuelist = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Duelist", intDataDuelist);

                        if (data.tryGetModIntegrationData("Duelist", out intDataDuelist))
                        {
                            Type duelistType = intDataDuelist.assembly.GetType("Duelist.UAE_Duelist", false);
                            if (duelistType != null)
                            {
                                intDataDuelist.typeDict.Add("Duelist", duelistType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Duelist agent Type (Duelist.UAE_Duelist)");
                            }
                        }
                        break;
                    case "God_Flesh":
                        Console.WriteLine("CommunityLib: Escamrak is Enabled");
                        ModIntegrationData intDataEscam = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Escamrak", intDataEscam);

                        if (data.tryGetModIntegrationData("Escamrak", out intDataEscam))
                        {
                            Type godType = intDataEscam.assembly.GetType("God_Flesh.God_Flesh", false);
                            if (godType != null)
                            {
                                intDataEscam.typeDict.Add("Escamrak", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Escamrak god Type (God_Flesh.God_Flesh)");
                            }

                            Type corruptedLocusType = intDataEscam.assembly.GetType("God_Flesh.Pr_CorruptedLocus", false);
                            if (corruptedLocusType != null)
                            {
                                intDataEscam.typeDict.Add("CorruptedLocus", corruptedLocusType);
                                registerLocusType(corruptedLocusType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Corrupted locus property Type (God_Flesh.Pr_CorruptedLocus)");
                            }

                            Type fleshcraftingTraitType = intDataEscam.assembly.GetType("God_Flesh.T_FleshKnowledge", false);
                            if (fleshcraftingTraitType != null)
                            {
                                intDataEscam.typeDict.Add("FleshcraftingTraitType", fleshcraftingTraitType);
                                registerMagicType(fleshcraftingTraitType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Fleshcrafting trait Type (God_Flesh.T_FleshKnowledge)");
                            }
                        }
                        break;
                    case "ShadowsLib":
                        Console.WriteLine("CommunityLib: Ixthus is Enabled");
                        ModIntegrationData intDataIxthus = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Ixthus", intDataIxthus);

                        if (data.tryGetModIntegrationData("Ixthus", out intDataIxthus))
                        {
                            Type ixthusType = intDataIxthus.assembly.GetType("ShadowsLib.God_KingofCups", false);
                            if (ixthusType != null)
                            {
                                intDataIxthus.typeDict.Add("Ixthus", ixthusType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Ixthus god Type (ShadowsLib.God_KingofCups)");
                            }

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

                            Type gawainType = intDataIxthus.assembly.GetType("ShadowsLib.UAE_Gawain", false);
                            if (gawainType != null)
                            {
                                intDataIxthus.typeDict.Add("Gawain", gawainType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Gawain agent Type (ShadowsLib.UAE_Gawain)");
                            }
                        }
                        break;
                    case "God_Mirror":
                        Console.WriteLine("CommunityLib: Kalastrophe is Enabled");
                        ModIntegrationData intDataKala = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Kalastrophe", intDataKala);

                        if (data.tryGetModIntegrationData("Kalastrophe", out intDataKala))
                        {
                            Type godType = intDataKala.assembly.GetType("God_Mirror.God_Mirror", false);
                            if (godType != null)
                            {
                                intDataKala.typeDict.Add("Kalastrophe", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Kalastrophe god Type (God_Mirror.God_Mirror)");
                            }
                        }
                        break;
                    case "ShadowsBloodshedGod":
                        Console.WriteLine("CommunityLib: Kishi is Enabled");
                        ModIntegrationData intDataKishi = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Kishi", intDataKishi);

                        if (data.tryGetModIntegrationData("Kishi", out intDataKishi))
                        {
                            Type godType = intDataKishi.assembly.GetType("ShadowsBloodshedGod.God_Bloodshed", false);
                            if (godType != null)
                            {
                                intDataKishi.typeDict.Add("Kishi", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Kishi god Type (ShadowsBloodshedGod.God_Bloodshed)");
                            }
                        }
                        break;
                    case "LivingCharacter":
                        Console.WriteLine("CommunityLib: Living Characters is Enabled");
                        ModIntegrationData intDataLC = new ModIntegrationData(kernel);
                        data.addModIntegrationData("LivingCharacters", intDataLC);

                        if (data.tryGetModIntegrationData("LivingCharacters", out intDataLC))
                        {
                            Type vampireNobleType = intDataLC.assembly.GetType("LivingCharacters.UAEN_Chars_VampireNoble", false);
                            if (vampireNobleType != null)
                            {
                                intDataLC.typeDict.Add("Vampire", vampireNobleType);
                                registerVampireType(vampireNobleType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Vampire noble agent Type (LivingCharacters.UAEN_Chars_VampireNoble)");
                            }
                        }
                        break;
                    case "LivingSocieties":
                        Console.WriteLine("CommunityLib: Living Socities is Enabled");
                        ModIntegrationData intDataLS = new ModIntegrationData(kernel);
                        data.addModIntegrationData("LivingSocieties", intDataLS);

                        if (data.tryGetModIntegrationData("LivingSocieties", out intDataLS))
                        {
                            MethodInfo MI_updateSettlementExternal = intDataLS.kernel.GetType().GetMethod("updateSettlementExternal", new Type[] { typeof(Society), typeof(SettlementHuman) });
                            if (MI_updateSettlementExternal != null)
                            {
                                intDataLS.methodInfoDict.Add("UpdateSettlement", MI_updateSettlementExternal);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get updateSettlementExternal function (LivingSocieties.Kernel_Socs.updateSettlementExternal)");
                            }
                        }
                        break;
                    case "LivingWilds":
                        Console.WriteLine("CommunityLib: Living Wilds is Enabled");
                        ModIntegrationData intDataLW = new ModIntegrationData(kernel);
                        data.addModIntegrationData("LivingWilds", intDataLW);

                        if (data.tryGetModIntegrationData("LivingWilds", out intDataLW))
                        {
                            Type wildSettlementType = intDataLW.assembly.GetType("LivingWilds.Set_Nature_UnoccupiedWilderness", false);
                            if (wildSettlementType != null)
                            {
                                intDataLW.typeDict.Add("WildSettlement", wildSettlementType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Unoccupied Wildernes settlement Type (LivingWilds.Set_Nature_UnoccupiedWilderness)");
                            }

                            Type werewolfInfectedType = intDataLW.assembly.GetType("LivingWilds.UAEN_Nature_WerewolfInfected", false);
                            if (werewolfInfectedType != null)
                            {
                                intDataLW.typeDict.Add("InfectedWerewolf", werewolfInfectedType);
                                intDataLW.fieldInfoDict.Add("InfectedWerewolfSubsumedUnit",werewolfInfectedType.GetField("oldUnit"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Werewolf agent Type (LivingWilds.UAEN_Nature_WerewolfInfected)");
                            }

                            Type werewolfInfectedHeroType = intDataLW.assembly.GetType("LivingWilds.UAG_Nature_WerewolfInfected", false);
                            if (werewolfInfectedHeroType != null)
                            {
                                intDataLW.typeDict.Add("InfectedWerewolfHero", werewolfInfectedHeroType);
                                intDataLW.fieldInfoDict.Add("InfectedWerewolfHeroSubsumedUnit", werewolfInfectedHeroType.GetField("oldUnit"));
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Werewolf agent Type (LivingWilds.UAG_Nature_WerewolfInfected)");
                            }
                        }
                        break;
                    case "MEKHANE":
                        Console.WriteLine("CommunityLib: Mekhane is Enabled");
                        ModIntegrationData intDataMekh = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Mehkane", intDataMekh);

                        if (data.tryGetModIntegrationData("Mekhane", out intDataMekh))
                        {
                            Type godType = intDataMekh.assembly.GetType("MEKHANE.God_MEKHANE", false);
                            if (godType != null)
                            {
                                intDataMekh.typeDict.Add("Mekhane", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Chandalor god Type (MEKHANE.God_MEKHANE)");
                            }

                            Type maxwellistType = intDataMekh.assembly.GetType("MEKHANE.UAE_Mek_Maxwellist", false);
                            if (maxwellistType != null)
                            {
                                intDataMekh.typeDict.Add("Maxwellist", maxwellistType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Maxwellist agent Type (MEKHANE.UAE_Mek_Maxwellist)");
                            }

                            Type choirType = intDataMekh.assembly.GetType("MEKHANE.UAE_Mek_MechanicalChoir", false);
                            if (choirType != null)
                            {
                                intDataMekh.typeDict.Add("MechanicalChoir", choirType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Mechanical choir agent Type (MEKHANE.UAE_Mek_MechanicalChoir)");
                            }

                            Type patriarchType = intDataMekh.assembly.GetType("MEKHANE.UAE_Mek_Patriarch", false);
                            if (patriarchType != null)
                            {
                                intDataMekh.typeDict.Add("Patriarch", patriarchType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Patriarch agent Type (MEKHANE.UAE_Mek_Patriarch)");
                            }
                        }
                        break;
                    case "ShadowsOutsiderGod":
                        Console.WriteLine("CommunityLib: Out of Gods is Enabled");
                        ModIntegrationData intDataOoGs = new ModIntegrationData(kernel);
                        data.addModIntegrationData("OutOfGods", intDataOoGs);

                        if (data.tryGetModIntegrationData("OutOfGods", out intDataOoGs))
                        {
                            Type godAType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.God_Lotus", false);
                            if (godAType != null)
                            {
                                intDataOoGs.typeDict.Add("Lotus", godAType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Lotus god Type (ShadowsOutsiderGod.God_Lotus)");
                            }

                            Type godBType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.God_Muse", false);
                            if (godBType != null)
                            {
                                intDataOoGs.typeDict.Add("Muse", godBType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Muse god Type (ShadowsOutsiderGod.God_Muse)");
                            }

                            Type godCType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.God_Museconventional", false);
                            if (godCType != null)
                            {
                                intDataOoGs.typeDict.Add("MuseConventional", godCType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Muse conventional god Type (ShadowsOutsiderGod.God_Museconventional)");
                            }

                            Type godDType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.God_Outsider", false);
                            if (godDType != null)
                            {
                                intDataOoGs.typeDict.Add("Outsider", godDType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Outsider god Type (ShadowsOutsiderGod.God_Outsider)");
                            }

                            Type godEType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.God_Paradoxis", false);
                            if (godEType != null)
                            {
                                intDataOoGs.typeDict.Add("Paradoxis", godEType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Paradoxis god Type (ShadowsOutsiderGod.God_Paradoxis)");
                            }

                            Type addictType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.UAE_Addict", false);
                            if (addictType != null)
                            {
                                intDataOoGs.typeDict.Add("Addict", addictType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Addict agent Type (ShadowsOutsiderGod.UAE_Addict)");
                            }

                            Type fakeUpstartType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.UAE_FakeUpstart", false);
                            if (fakeUpstartType != null)
                            {
                                intDataOoGs.typeDict.Add("FakeUpstart", fakeUpstartType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Fake upstart agent Type (ShadowsOutsiderGod.UAE_FakeUpstart)");
                            }

                            Type representativeType = intDataOoGs.assembly.GetType("ShadowsOutsiderGod.UAE_Representative", false);
                            if (representativeType != null)
                            {
                                intDataOoGs.typeDict.Add("Representative", representativeType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Representative agent Type (ShadowsOutsiderGod.UAE_Representative)");
                            }
                        }
                        break;
                    case "CustomVoidGod":
                        Console.WriteLine("CommunityLib: The Living Void is Enabled");
                        ModIntegrationData intDataVoid = new ModIntegrationData(kernel);
                        data.addModIntegrationData("LivingVoid", intDataVoid);

                        if (data.tryGetModIntegrationData("LivingVoid", out intDataVoid))
                        {
                            Type godType = intDataVoid.assembly.GetType("CustomVoidGod.God_Vacuum", false);
                            if (godType != null)
                            {
                                intDataVoid.typeDict.Add("LivingVoid", godType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get The living void god Type (CustomVoidGod.God_Vacuum)");
                            }
                        }
                        break;
                    case "TheOtherworlder":
                        Console.WriteLine("CommunityLib: The Otherworlder is Enabled");
                        ModIntegrationData intDataOtherworld = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Otherworlder", intDataOtherworld);

                        if (data.tryGetModIntegrationData("Otherworlder", out intDataOtherworld))
                        {
                            Type otherworlderType = intDataOtherworld.assembly.GetType("TheOtherworlder.UAE_TheOtherworlder", false);
                            if (otherworlderType != null)
                            {
                                intDataOtherworld.typeDict.Add("Otherworlder", otherworlderType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Otherworlder agent Type (TheOtherworlder.UAE_TheOtherworlder)");
                            }
                        }
                        break;
                    case "Modjam_Ratking":
                        Console.WriteLine("CommunityLib: The Rat King is Enabled");
                        ModIntegrationData intDataRatKing = new ModIntegrationData(kernel);
                        data.addModIntegrationData("RatKing", intDataRatKing);

                        if (data.tryGetModIntegrationData("RatKing", out intDataRatKing))
                        {
                            Type ratKingType = intDataRatKing.assembly.GetType("Modjam_Ratking.UAE_Wonderblunder_RatKing", false);
                            if (ratKingType != null)
                            {
                                intDataRatKing.typeDict.Add("RatKing", ratKingType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Rat king agent Type (Modjam_Ratking.UAE_Wonderblunder_RatKing)");
                            }
                        }
                        break;
                    case "ModJam_Redeemer":
                        Console.WriteLine("CommunityLib: The Redeemer is Enabled");
                        ModIntegrationData intDataRedeemer = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Redeemer", intDataRedeemer);

                        if (data.tryGetModIntegrationData("Redeemer", out intDataRedeemer))
                        {
                            Type redeemerType = intDataRedeemer.assembly.GetType("ModJam_Redeemer.UAE_Redeemer", false);
                            if (redeemerType != null)
                            {
                                intDataRedeemer.typeDict.Add("Redeemer", redeemerType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Redeemer agent Type (ModJam_Redeemer.UAE_Redeemer)");
                            }
                        }
                        break;
                    case "Whisperer":
                        Console.WriteLine("CommunityLib: The Whisperer is Enabled");
                        ModIntegrationData intDataWhisperer = new ModIntegrationData(kernel);
                        data.addModIntegrationData("Whisperer", intDataWhisperer);

                        if (data.tryGetModIntegrationData("Whisperer", out intDataWhisperer))
                        {
                            Type whispererType = intDataWhisperer.assembly.GetType("Whisperer.UAE_Whisperer", false);
                            if (whispererType != null)
                            {
                                intDataWhisperer.typeDict.Add("Whisperer", whispererType);
                                registerVampireType(whispererType);
                            }
                            else
                            {
                                Console.WriteLine("CommunityLib: Failed to get Whisperer agent Type (Whisperer.UAE_Whisperer)");
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

            if (properties.ContainsKey("CANCEL_ALL_ATTACKS"))
            {
                properties["CANCEL_ALL_ATTACKS"] = new EventRuntime.TypedProperty<string>(delegate (EventContext c, string v)
                {
                    Unit unit = c.unit;
                    foreach (Unit other in c.map.units)
                    {
                        if (other.task is Task_AttackUnit t_Attack && t_Attack.target == unit)
                        {
                            other.task = null;
                            return;
                        }

                        if (other.task is Task_AttackUnitWithEscort t_AttackWithEscort && t_AttackWithEscort.target == unit)
                        {
                            other.task = null;
                            return;
                        }

                        if (other.task is Task_DisruptUA t_Disrupt && t_Disrupt.other == unit)
                        {
                            other.task = null;
                            return;
                        }

                        if (other.task is Task_Follow t_Follow && t_Follow.target == unit)
                        {
                            other.task = null;
                            return;
                        }

                        if (other.task is Task_Bodyguard t_Guard && t_Guard.target == unit)
                        {
                            other.task = null;
                            return;
                        }
                    }
                });
            }

            // Misising God Fields
            if (!fields.ContainsKey("god_is_deathgame"))
            {
                fields.Add("god_is_deathgame", new EventRuntime.TypedField<bool>((EventContext c) => c.map.overmind.god is God_Cards));
            }

            if (!fields.ContainsKey("god_is_maker"))
            {
                fields.Add("god_is_maker", new EventRuntime.TypedField<bool>((EventContext c) => c.map.overmind.god is God_Eternity));
            }

            if (!fields.ContainsKey("god_is_cordyceps"))
            {
                fields.Add("god_is_cordyceps", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Cordyceps", out ModIntegrationData intDataCord) && intDataCord.typeDict.TryGetValue("God", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            if (!fields.ContainsKey("god_is_evilbeneath"))
            {
                fields.Add("god_is_evilbeneath", new EventRuntime.TypedField<bool>((EventContext c) => c.map.overmind.god is God_Underground));
            }

            // Missing Agent Fields
            if (!fields.ContainsKey("is_agent_aristocrat"))
            {
                fields.Add("is_agent_aristocrat", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_Aristocrat));
            }

            if (!fields.ContainsKey("is_agent_banditking"))
            {
                fields.Add("is_agent_banditking", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_BanditKing));
            }

            if (!fields.ContainsKey("is_agent_buccaneer"))
            {
                fields.Add("is_agent_buccaneer", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_Buccaneer));
            }

            if (!fields.ContainsKey("is_agent_exile"))
            {
                fields.Add("is_agent_exile", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_Exile));
            }

            if (!fields.ContainsKey("is_agent_seeker"))
            {
                fields.Add("is_agent_seeker", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_Seeker));
            }

            if (!fields.ContainsKey("is_agent_spellbinder"))
            {
                fields.Add("is_agent_spellbinder", new EventRuntime.TypedField<bool>((EventContext c) => c.unit is UAE_Spellbinder));
            }

            // Missing Location Fields
            if (!fields.ContainsKey("is_wonder"))
            {
                fields.Add("is_wonder", new EventRuntime.TypedField<bool>((EventContext c) => c.location != null && checkIsNaturalWonder(c.location)));
            }

            // Missing Map Fields
            if (!fields.ContainsKey("awareness_of_underground"))
            {
                fields.Add("awareness_of_underground", new EventRuntime.TypedField<double>((EventContext c) => c.map.awarenessOfUnderground));
            }

            // Adolia
            if (!fields.ContainsKey("is_agent_adolia"))
            {
                fields.Add("is_agent_adolia", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Adolia", out ModIntegrationData intDataAdolia) && intDataAdolia.typeDict.TryGetValue("Adolia", out Type adoliaType) && intDataAdolia.typeDict.TryGetValue("AdoliaAgent", out Type adoliaAgentType) && (c.unit.GetType() == adoliaType || c.unit.GetType().IsSubclassOf(adoliaType) || c.unit.GetType() == adoliaAgentType || c.unit.GetType().IsSubclassOf(adoliaAgentType))));
            }

            if (!fields.ContainsKey("god_is_adolia"))
            {
                fields.Add("god_is_adolia", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Adolia", out ModIntegrationData intDataAdolia) && intDataAdolia.typeDict.TryGetValue("FacelessMemory", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Banits and Crime
            if (!fields.ContainsKey("is_agent_brigand"))
            {
                fields.Add("is_agent_brigand", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("BanditsAndCrime", out ModIntegrationData intDataBandit) && intDataBandit.typeDict.TryGetValue("Bandit", out Type banditType) && (c.unit.GetType() == banditType || c.unit.GetType().IsSubclassOf(banditType))));
            }

            if (!fields.ContainsKey("is_agent_lawbreaker"))
            {
                fields.Add("is_agent_lawbreaker", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("BanditsAndCrime", out ModIntegrationData intDataBandit) && intDataBandit.typeDict.TryGetValue("Lawbreaker", out Type lawbreakerType) && (c.unit.GetType() == lawbreakerType || c.unit.GetType().IsSubclassOf(lawbreakerType))));
            }

            // Chandalor
            if (!fields.ContainsKey("god_is_chandalor"))
            {
                fields.Add("god_is_chandalor", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Chandalor", out ModIntegrationData intDataChand) && intDataChand.typeDict.TryGetValue("Chandalor", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Courtesan
            if (!fields.ContainsKey("is_agent_courtesan"))
            {
                fields.Add("is_agent_courtesan", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Courtesan", out ModIntegrationData intDataCourtesan) && intDataCourtesan.typeDict.TryGetValue("Courtesan", out Type courtesanType) && (c.unit.GetType() == courtesanType || c.unit.GetType().IsSubclassOf(courtesanType))));
            }

            // Deep Ones Plus
            if (!fields.ContainsKey("is_agent_fisherman"))
            {
                fields.Add("is_agent_fisherman", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("DeepOnesPlus", out ModIntegrationData intDataDOPlus) && intDataDOPlus.typeDict.TryGetValue("Fisherman", out Type fishermanType) && (c.unit.GetType() == fishermanType || c.unit.GetType().IsSubclassOf(fishermanType))));
            }

            if (!fields.ContainsKey("is_agent_drownedprophet"))
            {
                fields.Add("is_agent_drownedprophet", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("DeepOnesPlus", out ModIntegrationData intDataDOPlus) && intDataDOPlus.typeDict.TryGetValue("DrownedProphet", out Type drownedProphetType) && (c.unit.GetType() == drownedProphetType || c.unit.GetType().IsSubclassOf(drownedProphetType))));
            }

            // The Delver
            if (!fields.ContainsKey("is_agent_delver"))
            {
                fields.Add("is_agent_delver", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Delver", out ModIntegrationData intDataDelv) && intDataDelv.typeDict.TryGetValue("Delver", out Type delverType) && (c.unit.GetType() == delverType || c.unit.GetType().IsSubclassOf(delverType))));
            }

            // The Duelist
            if (!fields.ContainsKey("is_agent_duelist"))
            {
                fields.Add("is_agent_duelist", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Duelist", out ModIntegrationData intDataDuelist) && intDataDuelist.typeDict.TryGetValue("Duelist", out Type duelistType) && (c.unit.GetType() == duelistType || c.unit.GetType().IsSubclassOf(duelistType))));
            }

            // Escamrak
            if (!fields.ContainsKey("god_is_escamrak"))
            {
                fields.Add("god_is_escamrak", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Escamrak", out ModIntegrationData intDataEscam) && intDataEscam.typeDict.TryGetValue("Escamrak", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Ixthus
            if (!fields.ContainsKey("is_agent_gawain"))
            {
                fields.Add("is_agent_gawain", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Ixthus", out ModIntegrationData intDataIxthus) && intDataIxthus.typeDict.TryGetValue("Gawain", out Type gawainType) && (c.unit.GetType() == gawainType || c.unit.GetType().IsSubclassOf(gawainType))));
            }

            if (!fields.ContainsKey("god_is_ixthus"))
            {
                fields.Add("god_is_ixthus", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Ixthus", out ModIntegrationData intDataIxthus) && intDataIxthus.typeDict.TryGetValue("Ixthus", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Kalastrophe
            if (!fields.ContainsKey("god_is_kalastrophe"))
            {
                fields.Add("god_is_kalastrophe", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Kalastrophe", out ModIntegrationData intDataKala) && intDataKala.typeDict.TryGetValue("Kalastrophe", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Kishi
            if (!fields.ContainsKey("god_is_kishi"))
            {
                fields.Add("god_is_kishi", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Kishi", out ModIntegrationData intDataKishi) && intDataKishi.typeDict.TryGetValue("Kishi", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // MEKHANE
            if (!fields.ContainsKey("is_agent_maxwellist"))
            {
                fields.Add("is_agent_maxwellist", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Mekhane", out ModIntegrationData intDataMekh) && intDataMekh.typeDict.TryGetValue("Maxwellist", out Type maxwellistType) && (c.unit.GetType() == maxwellistType || c.unit.GetType().IsSubclassOf(maxwellistType))));
            }

            if (!fields.ContainsKey("is_agent_mechanicalchoir"))
            {
                fields.Add("is_agent_mechanicalchoir", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Mekhane", out ModIntegrationData intDataMekh) && intDataMekh.typeDict.TryGetValue("MechanicalChoir", out Type mechanicalChoirType) && (c.unit.GetType() == mechanicalChoirType || c.unit.GetType().IsSubclassOf(mechanicalChoirType))));
            }

            if (!fields.ContainsKey("is_agent_patriarch"))
            {
                fields.Add("is_agent_patriarch", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Mekhane", out ModIntegrationData intDataMekh) && intDataMekh.typeDict.TryGetValue("Patriarch", out Type patriarchType) && (c.unit.GetType() == patriarchType || c.unit.GetType().IsSubclassOf(patriarchType))));
            }

            if (!fields.ContainsKey("god_is_mekhane"))
            {
                fields.Add("god_is_mekhane", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("Mekhane", out ModIntegrationData intDataMekh) && intDataMekh.typeDict.TryGetValue("Mekhane", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // Out Of Gods
            if (!fields.ContainsKey("is_agent_addict"))
            {
                fields.Add("is_agent_addict", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Addict", out Type addictType) && (c.unit.GetType() == addictType || c.unit.GetType().IsSubclassOf(addictType))));
            }

            if (!fields.ContainsKey("is_agent_fakeupstart"))
            {
                fields.Add("is_agent_fakeupstart", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("FakeUpstart", out Type fakeUpstartType) && (c.unit.GetType() == fakeUpstartType || c.unit.GetType().IsSubclassOf(fakeUpstartType))));
            }

            if (!fields.ContainsKey("is_agent_representative"))
            {
                fields.Add("is_agent_representative", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Representative", out Type representativeType) && (c.unit.GetType() == representativeType || c.unit.GetType().IsSubclassOf(representativeType))));
            }

            if (!fields.ContainsKey("god_is_lotus"))
            {
                fields.Add("god_is_lotus", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Lotus", out Type lotusGodType) && (c.map.overmind.god.GetType() == lotusGodType || c.map.overmind.god.GetType().IsSubclassOf(lotusGodType))));
            }

            if (!fields.ContainsKey("god_is_muse"))
            {
                fields.Add("god_is_muse", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Muse", out Type museGodType) && (c.map.overmind.god.GetType() == museGodType || c.map.overmind.god.GetType().IsSubclassOf(museGodType))));
            }

            if (!fields.ContainsKey("god_is_museconventional"))
            {
                fields.Add("god_is_museconventional", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("MuseConventional", out Type museConventionalGodType) && (c.map.overmind.god.GetType() == museConventionalGodType || c.map.overmind.god.GetType().IsSubclassOf(museConventionalGodType))));
            }

            if (!fields.ContainsKey("god_is_outsider"))
            {
                fields.Add("god_is_outsider", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Outsider", out Type outsiderGodType) && (c.map.overmind.god.GetType() == outsiderGodType || c.map.overmind.god.GetType().IsSubclassOf(outsiderGodType))));
            }

            if (!fields.ContainsKey("god_is_paradoxis"))
            {
                fields.Add("god_is_paradoxis", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("OutOfGods", out ModIntegrationData intDataOoGs) && intDataOoGs.typeDict.TryGetValue("Paradoxis", out Type paradoxisGodType) && (c.map.overmind.god.GetType() == paradoxisGodType || c.map.overmind.god.GetType().IsSubclassOf(paradoxisGodType))));
            }

            // The Living Void
            if (!fields.ContainsKey("god_is_livingvoid"))
            {
                fields.Add("god_is_livingvoid", new EventRuntime.TypedField<bool>((EventContext c) => data.tryGetModIntegrationData("LivingVoid", out ModIntegrationData intDataVoid) && intDataVoid.typeDict.TryGetValue("LivingVoid", out Type godType) && (c.map.overmind.god.GetType() == godType || c.map.overmind.god.GetType().IsSubclassOf(godType))));
            }

            // The Otherworlder
            if (!fields.ContainsKey("is_agent_otherworlder"))
            {
                fields.Add("is_agent_otherworlder", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Otherworlder", out ModIntegrationData intDataOtherworlder) && intDataOtherworlder.typeDict.TryGetValue("Otherworlder", out Type otherworlderType) && (c.unit.GetType() == otherworlderType || c.unit.GetType().IsSubclassOf(otherworlderType))));
            }

            // The Rat King
            if (!fields.ContainsKey("is_agent_ratking"))
            {
                fields.Add("is_agent_ratking", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("RatKing", out ModIntegrationData intDataRatKing) && intDataRatKing.typeDict.TryGetValue("RatKing", out Type ratKingType) && (c.unit.GetType() == ratKingType || c.unit.GetType().IsSubclassOf(ratKingType))));
            }

            // The Redeemer
            if (!fields.ContainsKey("is_agent_redeemer"))
            {
                fields.Add("is_agent_redeemer", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Redeemer", out ModIntegrationData intDataRedeemer) && intDataRedeemer.typeDict.TryGetValue("Redeemer", out Type redeemerType) && (c.unit.GetType() == redeemerType || c.unit.GetType().IsSubclassOf(redeemerType))));
            }

            // The Whisperer
            if (!fields.ContainsKey("is_agent_whisperer"))
            {
                fields.Add("is_agent_whisperer", new EventRuntime.TypedField<bool>((EventContext c) => c.unit != null && data.tryGetModIntegrationData("Whisperer", out ModIntegrationData intDataWhisperer) && intDataWhisperer.typeDict.TryGetValue("Whisperer", out Type whispererType) && (c.unit.GetType() == whispererType || c.unit.GetType().IsSubclassOf(whispererType))));
            }

            // Other stat fields
            if (!fields.ContainsKey("other_stat_might"))
            {
                fields.Add("other_stat_might", new EventRuntime.TypedField<int>((EventContext c) => c._person2 != null? c._person2.stat_might : 0));
            }

            if (!fields.ContainsKey("other_stat_intrigue"))
            {
                fields.Add("other_stat_intrigue", new EventRuntime.TypedField<int>((EventContext c) => c._person2 != null ? c._person2.stat_intrigue : 0));
            }

            if (!fields.ContainsKey("other_stat_lore"))
            {
                fields.Add("other_stat_lore", new EventRuntime.TypedField<int>((EventContext c) => c._person2 != null ? c._person2.stat_lore : 0));
            }

            if (!fields.ContainsKey("other_stat_command"))
            {
                fields.Add("other_stat_command", new EventRuntime.TypedField<int>((EventContext c) => c._person2 != null ? c._person2.stat_command : 0));
            }

            if (!fields.ContainsKey("other_command_limit"))
            {
                fields.Add("other_command_limit", new EventRuntime.TypedField<int>((EventContext c) => {
                    if (c._person2 != null && c._person2.unit is UA ua)
                    {
                        return ua.getStatCommandLimit();
                    }
                    return 0;
                }));
            }

            if (!fields.ContainsKey("other_command_limit_currently_used"))
            {
                fields.Add("other_command_limit_currently_used", new EventRuntime.TypedField<int>((EventContext c) => {
                    if (c._person2 != null && c._person2.unit is UA ua)
                    {
                        return ua.getCurrentlyUsedCommand();
                    }
                    return 0;
                }));
            }

            if (!fields.ContainsKey("other_kills"))
            {
                fields.Add("other_kills", new EventRuntime.TypedField<int>((EventContext c) => c._person2 != null ? c._person2.statistic_kills : 0));
            }

            // Replace Teleport to elder tomb
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

            if (!properties.ContainsKey("GAIN_VINERVA_SEED"))
            {
                properties.Add(
                    "GAIN_VINERVA_SEED",
                    new EventRuntime.TypedProperty<string>(delegate (EventContext c, string _)
                    {
                        if (c.unit != null && c.unit.person != null)
                        {
                            T_VinervaSeed seed = (T_VinervaSeed)c.unit.person.traits.FirstOrDefault(t => t is T_VinervaSeed);
                            if (seed == null)
                            {
                                seed = new T_VinervaSeed();
                                c.unit.person.receiveTrait(seed);
                            }
                            else
                            {
                                seed.level++;
                            }
                        }
                    })
                );
            }

            if (!properties.ContainsKey("LOSE_VINERVA_SEED"))
            {
                properties.Add(
                    "LOSE_VINERVA_SEED",
                    new EventRuntime.TypedProperty<string>(delegate (EventContext c, string _)
                    {
                        if (c.unit != null && c.unit.person != null)
                        {
                            T_VinervaSeed seed = (T_VinervaSeed)c.unit.person.traits.FirstOrDefault(t => t is T_VinervaSeed);
                            if (seed != null)
                            {
                                seed.level--;
                                if (seed.level <= 0)
                                {
                                    c.unit.rituals.Remove(seed.TemptRulerChallenge);
                                    c.unit.person.traits.Remove(seed);
                                }
                            }
                        }
                    })
                );
            }

            // SHipwreck additions
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

            if (opt_targetDwarfCount == 0)
            {
                opt_targetDwarfCount = 1;
            }

            if (tradeRouteManager == null)
            {
                tradeRouteManager = new ManagerTradeRoutes(map);
            }

            if (data.BrokenMakerSleepDuration != 50)
            {
                data.BrokenMakerSleepDuration = 50;
            }

            HashSet<Person> processedProphets = new HashSet<Person>();
            foreach (SocialGroup sg in map.socialGroups)
            {
                if (sg is HolyOrder order && order.prophet != null && order.prophet.person != null)
                {
                    processedProphets.Add(order.prophet.person);
                    T_Prophet prophetTrait = null;
                    int previousTraitIndex = -1;
                    for (int i = order.prophet.person.traits.Count - 1; i >= 0; i--)
                    {
                        if (order.prophet.person.traits[i] is T_Prophet prophetTrait2)
                        {
                            prophetTrait = prophetTrait2;
                            if (previousTraitIndex != -1)
                            {
                                order.prophet.person.traits.RemoveAt(previousTraitIndex);
                            }
                            previousTraitIndex = i;
                        }
                    }

                    if (prophetTrait == null)
                    {
                        prophetTrait = new T_Prophet(order);
                        order.prophet.person.receiveTrait(prophetTrait);
                        continue;
                    }

                    if (prophetTrait.Orders == null)
                    {
                        prophetTrait.Orders = new List<HolyOrder> { order };
                    }
                    else if (!prophetTrait.Orders.Contains(order))
                    {
                        prophetTrait.Orders.Add(order);
                    }
                }
            }

            foreach (Person p in map.persons)
            {
                if (processedProphets.Contains(p))
                {
                    continue;
                }

                for (int i = p.traits.Count - 1; i >= 0; i--)
                {
                    if (p.traits[i] is T_Prophet prophet && (prophet.Orders == null || prophet.Orders.Count == 0))
                    {
                        p.traits.RemoveAt(i);
                    }
                }
            }
        }

        public override void onUIFullscreenBlockerUpdate(GameObject blocker)
        {
            if (blocker == null)
            {
                return;
            }

            PopupEvent component = blocker.GetComponent<PopupEvent>();
            if (component != null)
            {
                if (Get().data.tryGetEventPopupData(component, out EventData data, out EventContext ctx, out string msgOverride))
                {
                    if (!EventManager.events.TryGetValue(data.id, out EventManager.ActiveEvent e) || !e.willTrigger(ctx))
                    {
                        blocker.SetActive(false);
                        map.world.ui.removeBlocker(blocker);
                        return;
                    }

                    for (int i = 0; i < data.choices.Count; i++)
                    {
                        EventData.Choice c = data.choices[i];
                        bool valid = true;
                        try
                        {
                            if (c.condition != null)
                            {
                                valid = EventRuntime.evaluate(EventParser.parse(EventParser.tokenize(c.condition)), ctx);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"CommunityLib: ERROR: Exception while parsing event option conditions: {ex}");
                        }

                        if (!valid)
                        {
                            if (component.options[i].interactable == true)
                            {
                                Image img = component.options[i].GetComponent<Image>();
                                if (img != null)
                                {
                                    img.color = new Color(0f, 0f, 0f, 0.5f);
                                }
                                component.options[i].onClick.RemoveAllListeners();
                                component.options[i].interactable = false;
                            }
                        }
                        else
                        {
                            if (component.options[i].interactable == false)
                            {
                                Image img = component.options[i].GetComponent<Image>();
                                if (img != null)
                                {
                                    img.color = new Color(1f, 0.388f, 0.388f, 1f);
                                }
                                component.options[i].onClick.RemoveAllListeners();
                                component.options[i].onClick.AddListener(delegate () { component.dismiss(c, ctx); });
                                component.options[i].interactable = true;
                            }
                        }
                    }
                }
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
                foreach (var hook in HookRegistry.Delegate_onRevivePerson_CreateAgent)
                {
                    UA retValue = hook(victim, location);

                    if (retValue != null)
                    {
                        agent = retValue;
                        break;
                    }
                }
                if (agent == null)
                {
                    foreach (Hooks hook in GetRegisteredHooks())
                    {
                        UA retValue = hook.onRevivePerson_CreateAgent(victim, location);

                        if (retValue != null)
                        {
                            agent = retValue;
                            break;
                        }
                    }
                }
                
                if (agent == null)
                {
                    foreach (Func<Person, Location, UA> func in data.iterateReviveAgentCreationFunctions())
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

            foreach (var hook in HookRegistry.Delegate_onRevivePerson_EndOfProcess)
            {
                hook(victim, location);
            }
            foreach (Hooks hook in GetRegisteredHooks())
            {
                hook.onRevivePerson_EndOfProcess(victim, location);
            }
        }

        public override void onTurnStart(Map map)
        {
            data.onTurnStart(map);

            foreach (Power power in map.overmind.god.powers)
            {
                if (power is PowerDelayed delayedPower)
                {
                    delayedPower.turnTick();
                }
            }
        }

        public override void onTurnEnd(Map map)
        {
            cleanRandStore(map);

            if (!map.burnInComplete)
            {
                if (map.awarenessOfUnderground > 0.0)
                {
                    Console.WriteLine($"CommunityLib: Awareness of Underground was greater than 0 ({map.awarenessOfUnderground}). Resetting.");
                    map.awarenessOfUnderground = 0.0;
                }
            }

            data.onTurnEnd(map);
        }

        public override void onChallengeComplete(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            OnChallengeComplete.processChallenge(challenge, ua, task_PerformChallenge);
        }

        public override void onCheatEntered(string command)
        {
            ConsoleCommands.parseCommand(command, map);
        }

        public override int adjustHolyInfluenceDark(HolyOrder order, int inf, List<ReasonMsg> msgs)
        {
            int result = 0;

            if (order.isGone())
            {
                data.influenceGainElder.Remove(order);
                return 0;
            }

            if (data.influenceGainElder.TryGetValue(order, out List<ReasonMsg> reasons) && reasons.Count > 0)
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
                data.influenceGainHuman.Remove(order);
                return 0;
            }

            if (data.influenceGainHuman.TryGetValue(order, out List<ReasonMsg> reasons) && reasons.Count > 0)
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
            if (!data.influenceGainElder.TryGetValue(order, out List<ReasonMsg> influenceGain) || influenceGain == null)
            {
                influenceGain = new List<ReasonMsg>();
                data.influenceGainElder.Add(order, influenceGain);
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
            if (!data.influenceGainHuman.TryGetValue(order, out List<ReasonMsg> influenceGain))
            {
                influenceGain = new List<ReasonMsg>();
                data.influenceGainHuman.Add(order, influenceGain);
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

            foreach (var hook in HookRegistry.Delegate_onEvent_IsLocationElderTomb)
            {
                if (hook(location))
                {
                    return true;
                }
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
            //Console.WriteLine($"CommunityLib: Check Is Unit Subsumed");

            if (u == null)
            {
                //Console.WriteLine($"CommunityLib: Unit is null.");
                return false;
            }
            //Console.WriteLine($"CommunityLib: Unit is not null");

            if (u.map == null)
            {
                //Console.WriteLine($"CommunityLib: Map is null.");
                return false;
            }
            //Console.WriteLine($"CommunityLib: Map is not null");

            if (u.map.locations == null)
            {
                //Console.WriteLine($"CommunityLib: Map.locations is null.");
                return false;
            }
            //Console.WriteLine($"CommunityLib: Map.locations is not null");

            if (u.locIndex < 0 || u.locIndex >= u.map.locations.Count)
            {
                //Console.WriteLine($"CommunityLib: Unit location index ({u.locIndex}) is invalid.");
                return false;
            }
            //Console.WriteLine($"CommunityLib: Unit locIndex is in range");

            if (u.isDead && u.person != null && !u.person.isDead && u.person.unit != null && u.person.unit != u && !u.person.unit.isDead)
            {
                foreach (var hook in ModCore.Get().HookRegistry.Delegate_isUnitSubsumed)
                {
                    if (hook(u, u.person.unit))
                    {
                        return true;
                    }
                }
                foreach (Hooks hook in GetRegisteredHooks())
                {
                    if (hook.isUnitSubsumed(u, u.person.unit))
                    {
                        //Console.WriteLine($"CommunityLib: Unit is subsumed");
                        return true;
                    }
                }
            }

            //Console.WriteLine($"CommunityLib: Unit is not subsumed");
            return false;
        }


        public void registerLocusType(Type type) => data.addLocusType(type);

        public bool checkHasLocus(Location location) => data.isLocusType(location);

        public void registerMagicType(Type type) => data.addMagicTraitType(type);

        public bool checkKnowsMagic(Person person) => data.knowsMagic(person);

        public bool checkKnowsMagic(Person person, out List<Trait> magicTraits) => data.knowsMagicAdvanced(person, out magicTraits);

        public void registerNaturalWonderType(Type type) => data.addNaturalWonderType(type);

        public bool checkIsNaturalWonder(Location location) => data.isNaturalWonder(location);

        public bool checkIsNaturalWonder(Location location, out Settlement naturalWonderSettlement, out List<Subsettlement> naturalWonderSubsettlements) => data.isNaturalWonder(location, out naturalWonderSettlement, out naturalWonderSubsettlements);

        public void registerWonderType(Type type) => data.addWonderType(type);

        public bool checkIsWonder(Location location) => data.isWonder(location);

        public bool checkIsWonder(Location location, out Settlement wonderSettlement, out bool settlementIsNaturalWonder, out List<Subsettlement> wonderSubsettlements, out List<Subsettlement> naturalWonderSubsettlements) => data.isWonder(location, out wonderSettlement, out settlementIsNaturalWonder, out wonderSubsettlements, out naturalWonderSubsettlements);

        public void registerVampireType(Type type) => data.addVampireType(type);

        public bool checkIsVampire(Unit unit) => data.isVampireType(unit);

        public void registerReviveAgentCreationFunction(Func<Person, Location, UA> func) => data.addReviveAgentCreationFunction(func);

        public bool checkIsProphetPlayerAligned(HolyOrder order)
        {
            if (order == null || order.prophet == null || order.prophet == order.map.awarenessManager.chosenOne)
            {
                return false;
            }

            foreach (var hook in ModCore.Get().HookRegistry.Delegate_onCheckIsProphetPlayerAligned)
            {
                if (hook.Invoke(order, order.prophet))
                {
                    return true;
                }
            }
            foreach (Hooks hook in Get().GetRegisteredHooks())
            {
                if (hook.onCheckIsProphetPlayerAligned(order, order.prophet))
                {
                    return true;
                }
            }
            return false;
        }

        public int getTravelTimeTo(Unit u, Location location)
        {
            if (u == null || location == null)
            {
                return -1;
            }

            int travelTime;

            Location[] path = Pathfinding.getPathTo(u.location, location, u);
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
                foreach (var hook in HookRegistry.Delegate_onUnitAI_GetsDistanceToLocation)
                {
                    travelTime = hook(u, location, path, travelTime);
                }
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
