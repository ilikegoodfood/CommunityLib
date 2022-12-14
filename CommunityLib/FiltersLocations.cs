using Assets.Code;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CommunityLib
{
    internal class FiltersLocations
    {
        private Cache cache;

        private Filters filters;

        private Map map;

        internal FiltersLocations(Cache cache, Filters filters, Map map)
        {
            this.cache = cache;
            this.filters = filters;
            this.map = map;
        }

        public IList CreateList(Type myType)
        {
            return filters.CreateList(myType);
        }

        public IDictionary CreateDictionary(Type keyType, Type valueType)
        {
            return filters.CreateDictionary(keyType, valueType);
        }

        public IList GetOrCreateKeyListPair(IDictionary dict, Type key, Type t)
        {
            return filters.GetOrCreateKeyListPair(dict, key, t);
        }

        public IList GetOrCreateKeyListPair(IDictionary dict, object key, Type t)
        {
            return filters.GetOrCreateKeyListPair(dict, key, t);
        }

        public IDictionary CreateSubDictionary(Type keyType)
        {
            return filters.CreateSubDictionary(keyType);
        }

        public bool TryCreateSubDictionary(IDictionary dict, object key, Type keyType)
        {
            return filters.TryCreateSubDictionary(dict, key, keyType);
        }

        public bool TryCreateSubDictionary(IDictionary dict, Type key, Type keyType)
        {
            return filters.TryCreateSubDictionary(dict, key, keyType);
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, Type key, Type valueType, object value)
        {
            filters.CreateAndOrAddToKeyListPair(dict, key, valueType, value);
        }

        public void CreateAndOrAddToKeyListPair(IDictionary dict, object key, Type valueType, object value)
        {
            filters.CreateAndOrAddToKeyListPair(dict, key, valueType, value);
        }

        public void FilterLocations()
        {
            //Console.WriteLine("CommunityLib: Starting Location Processing");
            if (map.locations == null || map.locations.Count == 0)
            {
                //Console.WriteLine("CommunityLib: No locations found.");
                return;
            }

            // Initialize universal variables
            Settlement s;
            SocialGroup sG;
            Type tL;
            Type tS;
            Type tSub;
            Type tP;
            Type tSG;

            double distance;
            int steps;
            List<Location>[] locationArray;
            List<Settlement>[] settlementArray;
            List<Unit> units;

            // Dictionaries being operated on at all level.
            // Location Dictionaries //
            IDictionary lByT = cache.locationsByType;
            IDictionary lByTE = cache.locationsByTypeExclusive;
            IDictionary lBySG = cache.locationsBySocialGroup;
            // Locations - With Social Groups
            IDictionary lBySGT = cache.locationsBySocialGroupType;
            IDictionary lBySGTE = cache.locationsBySocialGroupTypeExclusive;
            IDictionary lBySGByT = cache.locationsBySocialGroupByType;
            IDictionary lBySGTByT = cache.locationsBySocialGroupTypeByType;
            IDictionary lBySGTEByT = cache.locationsBySocialGroupTypeExclusiveByType;
            IDictionary lBySGTByTE = cache.locationsBySocialGroupTypeByTypeExclusive;
            IDictionary lBySGTEByTE = cache.locationsBySocialGroupTypeExclusiveByTypeExclusive;
            // Locations - Without Social Groups
            IDictionary lWoSGByT = cache.locationsWithoutSocialGroupByType;
            IDictionary lWoSGByTE = cache.locationsWithoutSocialGroupByTypeExclusive;
            // Locations - Without Settlements
            IDictionary lWoSByT = cache.locationsWithoutSettlementByType;
            IDictionary lWoSByTE = cache.locationsWithoutSettlementByTypeExclusive;
            IDictionary lWoSBySG = cache.locationsWithoutSettlementBySocialGroup;
            IDictionary lWoSBySGT = cache.locationsWithoutSettlementBySocialGroupType;
            IDictionary lWoSBySGTE = cache.locationsWithoutSettlementBySocialGroupTypeExclusive;
            // Location - Without Social Groups and Settlements
            IDictionary lWoSG_SByT = cache.locationsWithoutSocialGroupOrSettlementByType;
            IDictionary lWoSG_SByTE = cache.locationsWithoutSocialGroupOrSettlementByTypeExclusive;
            // Settlement Dictionaries
            List<Settlement> settlements = cache.settlements;
            IDictionary sByT = cache.settlementsByType;
            IDictionary sByTE = cache.settlementsByTypeExclusive;
            IDictionary sByLT = cache.settlementsByLocationType;
            IDictionary sByLTE = cache.settlementsByLocationTypeExclusive;
            IDictionary sByLTByT = cache.settlementsByLocationTypeByType;
            IDictionary sByLTEByT = cache.settlementsByLocationTypeExclusiveByType;
            IDictionary sByLTByTE = cache.settlementsByLocationTypeByTypeExclusive;
            IDictionary sByLTEByTE = cache.settlementsByLocationTypeExclusiveByTypeExclusive;
            // Settlements - With Social Groups
            IDictionary sBySG = cache.settlementsBySocialGroup;
            IDictionary sBySGT = cache.settlementsBySocialGroupType;
            IDictionary sBySGTE = cache.settlementsBySocialGroupTypeExclusive;
            IDictionary sBySGByT = cache.settlementsBySocialGroupByType;
            IDictionary sBySGTByT = cache.settlementsBySocialGroupTypeByType;
            IDictionary sBySGTEByT = cache.settlementsBySocialGroupTypeExclusiveByType;
            IDictionary sBySGTByTE = cache.settlementsBySocialGroupTypeByTypeExclusive;
            IDictionary sBySGTEByTE = cache.settlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Settlements - Without Social Groups
            IDictionary sWoSGByT = cache.settlementsWithoutSocialGroupByType;
            IDictionary sWoSGByTE = cache.settlementsWithoutSocialGroupByTypeExclusive;
            // Specific Settlements
            List<Set_City>[] cityByL = cache.cityByLevel;
            IDictionary cityBySGByL = cache.cityBySocialGroupByLevel;
            IDictionary cityBySGTByL = cache.cityBySocialGroupTypeByLevel;
            IDictionary cityBySGTEByL = cache.cityBySocialGroupTypeExclusiveByLevel;
            List<Set_City>[] cityWoSGByL = cache.cityWithoutSocialGroupByLevel;
            List<Set_MinorHuman>[] minorHumanByL = cache.minorHumanByLevel;
            IDictionary minorHumanBySGByL = cache.minorHumanBySocialGroupByLevel;
            IDictionary minorHumanBySGTByL = cache.minorHumanBySocialGroupTypeByLevel;
            IDictionary minorHumanBySGTEByL = cache.minorHumanBySocialGroupTypeExclusiveByLevel;
            List<Set_MinorHuman>[] minorHumanWoSGByL = cache.minorHumanWithoutSocialGroupByLevel;
            List<Set_OrcCamp>[] orcCampByS = cache.orcCampBySpecialism;
            IDictionary orcCampBySGByS = cache.orcCampBySocialGroupBySpecialism;
            IDictionary orcCampBySGTByS = cache.orcCampBySocialGroupTypeBySpecialism;
            IDictionary orcCampBySGTEByS = cache.orcCampBySocialGroupTypeExclusiveBySpecialism;
            List<Set_OrcCamp>[] orcCampWoSGByS = cache.orcCampWithoutSocialGroupBySpecialism;
            // Subsettlement Dictionaries
            List<Subsettlement> subsettlements = cache.subsettlements;
            IDictionary ssByT = cache.subsettlementsByType;
            IDictionary ssByTE = cache.subsettlementsByTypeExclusive;
            IDictionary ssByLT = cache.subsettlementsByLocationType;
            IDictionary ssByLTE = cache.subsettlementsByLocationTypeExclusive;
            IDictionary ssByLTByT = cache.subsettlementsByLocationTypeByType;
            IDictionary ssByLTEByT = cache.subsettlementsByLocationTypeExclusiveByType;
            IDictionary ssByLTByTE = cache.subsettlementsByLocationTypeByTypeExclusive;
            IDictionary ssByLTEByTE = cache.subsettlementsByLocationTypeExclusiveByTypeExclusive;
            IDictionary ssByST = cache.subsettlementsBySettlementType;
            IDictionary ssBySTE = cache.subsettlementsBySettlementTypeExclusive;
            IDictionary ssBySTByT = cache.subsettlementsBySettlementTypeByType;
            IDictionary ssBySTEByT = cache.subsettlementsBySettlementTypeExclusiveByType;
            IDictionary ssBySTByTE = cache.subsettlementsBySettlementTypeByTypeExclusive;
            IDictionary ssBySTEByTE = cache.subsettlementsBySettlementTypeExclusiveByTypeExclusive;
            // Subsettlements - With Social Groups
            IDictionary ssBySG = cache.subsettlementsBySocialGroup;
            IDictionary ssBySGT = cache.subsettlementsBySocialGroupType;
            IDictionary ssBySGTE = cache.subsettlementsBySocialGroupTypeExclusive;
            IDictionary ssBySGByT = cache.subsettlementsBySocialGroupByType;
            IDictionary ssBySGTByT = cache.subsettlementsBySocialGroupTypeByType;
            IDictionary ssBySGTEByT = cache.subsettlementsBySocialGroupTypeExclusiveByType;
            IDictionary ssBySGTByTE = cache.subsettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary ssBySGTEByTE = cache.subsettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Subsettlements - Without Social Groups
            IDictionary ssWoSGByT = cache.subsettlementsWithoutSocialGroupByType;
            IDictionary ssWoSGByTE = cache.subsettlementsWithoutSocialGroupByTypeExclusive;
            // Property Dictionaries
            List<Property> properties = cache.properties;
            IDictionary pByT = cache.propertiesByType;
            IDictionary pByTE = cache.propertiesByTypeExclusive;
            IDictionary pByLT = cache.propertiesByLocationType;
            IDictionary pByLTE = cache.propertiesByLocationTypeExclusive;
            IDictionary pByLTByT = cache.propertiesByLocationTypeByType;
            IDictionary pByLTEByT = cache.propertiesByLocationTypeExclusiveByType;
            IDictionary pByLTByTE = cache.propertiesByLocationTypeByTypeExclusive;
            IDictionary pByLTEByTE = cache.propertiesByLocationTypeExclusiveByTypeExclusive;
            IDictionary pByST = cache.propertiesBySettlementType;
            IDictionary pBySTE = cache.propertiesBySettlementTypeExclusive;
            IDictionary pBySTByT = cache.propertiesBySettlementTypeByType;
            IDictionary pBySTEByT = cache.propertiesBySettlementTypeExclusiveByType;
            IDictionary pBySTByTE = cache.propertiesBySettlementTypeByTypeExclusive;
            IDictionary pBySTEByTE = cache.propertiesBySettlementTypeExclusiveByTypeExclusive;
            // Properties - With Social Groups
            IDictionary pBySG = cache.propertiesBySocialGroup;
            IDictionary pBySGT = cache.propertiesBySocialGroupType;
            IDictionary pBySGTE = cache.propertiesBySocialGroupTypeExclusive;
            IDictionary pBySGByT = cache.propertiesBySocialGroupByType;
            IDictionary pBySGTByT = cache.propertiesBySocialGroupTypeByType;
            IDictionary pBySGTEByT = cache.propertiesBySocialGroupTypeExclusiveByType;
            IDictionary pBySGTByTE = cache.propertiesBySocialGroupTypeByTypeExclusive;
            IDictionary pBySGTEByTE = cache.propertiesBySocialGroupTypeExclusiveByTypeExclusive;
            // Properties - Without Social Groups
            IDictionary pWoSGByT = cache.propertiesWithoutSocialGroupByType;
            IDictionary pWoSGByTE = cache.propertiesWithoutSocialGroupByTypeExclusive;
            // Properties - Without Settlements
            IDictionary pWoSByT = cache.propertiesWithoutSettlementByType;
            IDictionary pWoSByTE = cache.propertiesWithoutSettlementByTypeExclusive;
            IDictionary pWoSBySG = cache.propertiesWithoutSettlementBySocialGroup;
            IDictionary pWoSBySGT = cache.propertiesWithoutSettlementBySocialGroupType;
            IDictionary pWoSBySGTE = cache.propertiesWithoutSettlementBySocialGroupTypeExclusive;
            // Propertes - Without Social Groups and Settlements
            IDictionary pWoSG_SByT = cache.propertiesWithoutSocialGroupOrSettlementByType;
            IDictionary pWoSG_SByTE = cache.propertiesWithoutSocialGroupOrSettlementByTypeExclusive;

            // Coastal Location Dictionaries //
            List<Location> coastalLocations = cache.coastalLocations;
            IDictionary cLByT = cache.coastalLocationsByType;
            IDictionary cLByTE = cache.coastalLocationsByTypeExclusive;
            // Locations - With Social Groups
            IDictionary cLBySG = cache.coastalLocationsBySocialGroup;
            IDictionary cLBySGT = cache.coastalLocationsBySocialGroupType;
            IDictionary cLBySGTE = cache.coastalLocationsBySocialGroupTypeExclusive;
            IDictionary cLBySGByT = cache.coastalLocationsBySocialGroupByType;
            IDictionary cLBySGTByT = cache.coastalLocationsBySocialGroupTypeByType;
            IDictionary cLBySGTEByT = cache.coastalLocationsBySocialGroupTypeExclusiveByType;
            IDictionary cLBySGTByTE = cache.coastalLocationsBySocialGroupTypeByTypeExclusive;
            IDictionary cLBySGTEByTE = cache.coastalLocationsBySocialGroupTypeExclusiveByTypeExclusive;
            // Locations - Without Social Groups
            IDictionary cLWoSGByT = cache.coastalLocationsWithoutSocialGroupByType;
            IDictionary cLWoSGByTE = cache.coastalLocationsWithoutSocialGroupByTypeExclusive;
            // Locations - Without Settlements
            IDictionary cLWoSByT = cache.coastalLocationsWithoutSettlementByType;
            IDictionary cLWoSByTE = cache.coastalLocationsWithoutSettlementByTypeExclusive;
            IDictionary cLWoSBySG = cache.coastalLocationsWithoutSettlementBySocialGroup;
            IDictionary cLWoSBySGT = cache.coastalLocationsWithoutSettlementBySocialGroupType;
            IDictionary cLWoSBySGTE = cache.coastalLocationsWithoutSettlementBySocialGroupTypeExclusive;
            // Location - Without Social Groups and Settlements
            IDictionary cLWoSG_SByT = cache.coastalLocationsWithoutSocialGroupOrSettlementByType;
            IDictionary cLWoSG_SByTE = cache.coastalLocationsWithoutSocialGroupOrSettlementByTypeExclusive;
            // Coastal Settlement Dictionaries
            List<Settlement> coastalSettlements = cache.coastalSettlements;
            IDictionary cSByT = cache.coastalSettlementsByType;
            IDictionary cSByTE = cache.coastalSettlementsByTypeExclusive;
            IDictionary cSByLT = cache.coastalSettlementsByLocationType;
            IDictionary cSByLTE = cache.coastalSettlementsByLocationTypeExclusive;
            IDictionary cSByLTByT = cache.coastalSettlementsByLocationTypeByType;
            IDictionary cSByLTEByT = cache.coastalSettlementsByLocationTypeExclusiveByType;
            IDictionary cSByLTByTE = cache.coastalSettlementsByLocationTypeByTypeExclusive;
            IDictionary cSByLTEByTE = cache.coastalSettlementsByLocationTypeExclusiveByTypeExclusive;
            // Settlements - With Social Groups
            IDictionary cSBySG = cache.coastalSettlementsBySocialGroup;
            IDictionary cSBySGT = cache.coastalSettlementsBySocialGroupType;
            IDictionary cSBySGTE = cache.coastalSettlementsBySocialGroupTypeExclusive;
            IDictionary cSBySGByT = cache.coastalSettlementsBySocialGroupByType;
            IDictionary cSBySGTByT = cache.coastalSettlementsBySocialGroupTypeByType;
            IDictionary cSBySGTEByT = cache.coastalSettlementsBySocialGroupTypeExclusiveByType;
            IDictionary cSBySGTByTE = cache.coastalSettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary cSBySGTEByTE = cache.coastalSettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Settlements - Without Social Groups
            IDictionary cSWoSGByT = cache.coastalSettlementsWithoutSocialGroupByType;
            IDictionary cSWoSGByTE = cache.coastalSettlementsWithoutSocialGroupByTypeExclusive;
            // Specific Settlements
            List<Set_City>[] cCityByL = cache.coastalCityByLevel;
            IDictionary cCityBySGByL = cache.coastalCityBySocialGroupByLevel;
            IDictionary cCityBySGTByL = cache.coastalCityBySocialGroupTypeByLevel;
            IDictionary cCityBySGTEByL = cache.coastalCityBySocialGroupTypeExclusiveByLevel;
            List<Set_City>[] cCityWoSGByL = cache.coastalCityWithoutSocialGroupByLevel;
            List<Set_MinorHuman>[] cMinorHumanByL = cache.coastalMinorHumanByLevel;
            IDictionary cMinorHumanBySGByL = cache.coastalMinorHumanBySocialGroupByLevel;
            IDictionary cMinorHumanBySGTByL = cache.coastalMinorHumanBySocialGroupTypeByLevel;
            IDictionary cMinorHumanBySGTEByL = cache.coastalMinorHumanBySocialGroupTypeExclusiveByLevel;
            List<Set_MinorHuman>[] cMinorHumanWoSGByL = cache.coastalMinorHumanWithoutSocialGroupByLevel;
            List<Set_OrcCamp>[] cOrcCampByS = cache.coastalOrcCampBySpecialism;
            IDictionary cOrcCampBySGByS = cache.coastalOrcCampBySocialGroupBySpecialism;
            IDictionary cOrcCampBySGTByS = cache.coastalOrcCampBySocialGroupTypeBySpecialism;
            IDictionary cOrcCampBySGTEByS = cache.coastalOrcCampBySocialGroupTypeExclusiveBySpecialism;
            List<Set_OrcCamp>[] cOrcCampWoSGByS = cache.coastalOrcCampWithoutSocialGroupBySpecialism;
            // Coastal Subsettlement Dictionaries
            List<Subsettlement> coastalSubsettlements = cache.coastalSubsettlements;
            IDictionary cSsByT = cache.coastalSubsettlementsByType;
            IDictionary cSsByTE = cache.coastalSubsettlementsByTypeExclusive;
            IDictionary cSsByLT = cache.coastalSubsettlementsByLocationType;
            IDictionary cSsByLTE = cache.coastalSubsettlementsByLocationTypeExclusive;
            IDictionary cSsByLTByT = cache.coastalSubsettlementsByLocationTypeByType;
            IDictionary cSsByLTEByT = cache.coastalSubsettlementsByLocationTypeExclusiveByType;
            IDictionary cSsByLTByTE = cache.coastalSubsettlementsByLocationTypeByTypeExclusive;
            IDictionary cSsByLTEByTE = cache.coastalSubsettlementsByLocationTypeExclusiveByTypeExclusive;
            IDictionary cSsByST = cache.coastalSubsettlementsBySettlementType;
            IDictionary cSsBySTE = cache.coastalSubsettlementsBySettlementTypeExclusive;
            IDictionary cSsBySTByT = cache.coastalSubsettlementsBySettlementTypeByType;
            IDictionary cSsBySTEByT = cache.coastalSubsettlementsBySettlementTypeExclusiveByType;
            IDictionary cSsBySTByTE = cache.coastalSubsettlementsBySettlementTypeByTypeExclusive;
            IDictionary cSsBySTEByTE = cache.coastalSubsettlementsBySettlementTypeExclusiveByTypeExclusive;
            // Subsettlements - With Social Groups
            IDictionary cSsBySG = cache.coastalSubsettlementsBySocialGroup;
            IDictionary cSsBySGT = cache.coastalSubsettlementsBySocialGroupType;
            IDictionary cSsBySGTE = cache.coastalSubsettlementsBySocialGroupTypeExclusive;
            IDictionary cSsBySGByT = cache.coastalSubsettlementsBySocialGroupByType;
            IDictionary cSsBySGTByT = cache.coastalSubsettlementsBySocialGroupTypeByType;
            IDictionary cSsBySGTEByT = cache.coastalSubsettlementsBySocialGroupTypeExclusiveByType;
            IDictionary cSsBySGTByTE = cache.coastalSubsettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary cSsBySGTEByTE = cache.coastalSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Subsettlements - Without Social Groups
            IDictionary cSsWoSGByT = cache.coastalSubsettlementsWithoutSocialGroupByType;
            IDictionary cSsWoSGByTE = cache.coastalSubsettlementsWithoutSocialGroupByTypeExclusive;
            // Coastal Property Dictionaries
            List<Property> coastalProperties = cache.coastalProperties;
            IDictionary cPByT = cache.coastalPropertiesByType;
            IDictionary cPByTE = cache.coastalPropertiesByTypeExclusive;
            IDictionary cPByLT = cache.coastalPropertiesByLocationType;
            IDictionary cPByLTE = cache.coastalPropertiesByLocationTypeExclusive;
            IDictionary cPByLTByT = cache.coastalPropertiesByLocationTypeByType;
            IDictionary cPByLTEByT = cache.coastalPropertiesByLocationTypeExclusiveByType;
            IDictionary cPByLTByTE = cache.coastalPropertiesByLocationTypeByTypeExclusive;
            IDictionary cPByLTEByTE = cache.coastalPropertiesByLocationTypeExclusiveByTypeExclusive;
            IDictionary cPByST = cache.coastalPropertiesBySettlementType;
            IDictionary cPBySTE = cache.coastalPropertiesBySettlementTypeExclusive;
            IDictionary cPBySTByT = cache.coastalPropertiesBySettlementTypeByType;
            IDictionary cPBySTEByT = cache.coastalPropertiesBySettlementTypeExclusiveByType;
            IDictionary cPBySTByTE = cache.coastalPropertiesBySettlementTypeByTypeExclusive;
            IDictionary cPBySTEByTE = cache.coastalPropertiesBySettlementTypeExclusiveByTypeExclusive;
            // Properties - With Social Groups
            IDictionary cPBySG = cache.coastalPropertiesBySocialGroup;
            IDictionary cPBySGT = cache.coastalPropertiesBySocialGroupType;
            IDictionary cPBySGTE = cache.coastalPropertiesBySocialGroupTypeExclusive;
            IDictionary cPBySGByT = cache.coastalPropertiesBySocialGroupByType;
            IDictionary cPBySGTByT = cache.coastalPropertiesBySocialGroupTypeByType;
            IDictionary cPBySGTEByT = cache.coastalPropertiesBySocialGroupTypeExclusiveByType;
            IDictionary cPBySGTByTE = cache.coastalPropertiesBySocialGroupTypeByTypeExclusive;
            IDictionary cPBySGTEByTE = cache.coastalPropertiesBySocialGroupTypeExclusiveByTypeExclusive;
            // Properties - Without Social Groups
            IDictionary cPWoSGByT = cache.coastalPropertiesWithoutSocialGroupByType;
            IDictionary cPWoSGByTE = cache.coastalPropertiesWithoutSocialGroupByTypeExclusive;
            // Properties - Without Settlements
            IDictionary cPWoSByT = cache.coastalPropertiesWithoutSettlementByType;
            IDictionary cPWoSByTE = cache.coastalPropertiesWithoutSettlementByTypeExclusive;
            IDictionary cPWoSBySG = cache.coastalPropertiesWithoutSettlementBySocialGroup;
            IDictionary cPWoSBySGT = cache.coastalPropertiesWithoutSettlementBySocialGroupType;
            IDictionary cPWoSBySGTE = cache.coastalPropertiesWithoutSettlementBySocialGroupTypeExclusive;
            // Propertes - Without Social Groups and Settlements
            IDictionary cPWoSG_SByT = cache.coastalPropertiesWithoutSocialGroupOrSettlementByType;
            IDictionary cPWoSG_SByTE = cache.coastalPropertiesWithoutSocialGroupOrSettlementByTypeExclusive;

            // Ocean Location Dictionaries //
            List<Location> oceanLocations = cache.coastalLocations;
            IDictionary oLByT = cache.oceanLocationsByType;
            IDictionary oLByTE = cache.oceanLocationsByTypeExclusive;
            // Locations - With Social Groups
            IDictionary oLBySG = cache.oceanLocationsBySocialGroup;
            IDictionary oLBySGT = cache.oceanLocationsBySocialGroupType;
            IDictionary oLBySGTE = cache.oceanLocationsBySocialGroupTypeExclusive;
            IDictionary oLBySGByT = cache.oceanLocationsBySocialGroupByType;
            IDictionary oLBySGTByT = cache.oceanLocationsBySocialGroupTypeByType;
            IDictionary oLBySGTEByT = cache.oceanLocationsBySocialGroupTypeExclusiveByType;
            IDictionary oLBySGTByTE = cache.oceanLocationsBySocialGroupTypeByTypeExclusive;
            IDictionary oLBySGTEByTE = cache.oceanLocationsBySocialGroupTypeExclusiveByTypeExclusive;
            // Locations - Without Social Groups
            IDictionary oLWoSGByT = cache.oceanLocationsWithoutSocialGroupByType;
            IDictionary oLWoSGByTE = cache.oceanLocationsWithoutSocialGroupByTypeExclusive;
            // Locations - Without Settlements
            IDictionary oLWoSByT = cache.oceanLocationsWithoutSettlementByType;
            IDictionary oLWoSByTE = cache.oceanLocationsWithoutSettlementByTypeExclusive;
            IDictionary oLWoSBySG = cache.oceanLocationsWithoutSettlementBySocialGroup;
            IDictionary oLWoSBySGT = cache.oceanLocationsWithoutSettlementBySocialGroupType;
            IDictionary oLWoSBySGTE = cache.oceanLocationsWithoutSettlementBySocialGroupTypeExclusive;
            // Location - Without Social Groups and Settlements
            IDictionary oLWoSG_SByT = cache.oceanLocationsWithoutSocialGroupOrSettlementByType;
            IDictionary oLWoSG_SByTE = cache.oceanLocationsWithoutSocialGroupOrSettlementByTypeExclusive;
            // Ocean Settlement Dictionaries
            List<Settlement> oceanSettlements = cache.oceanSettlements;
            IDictionary oSByT = cache.oceanSettlementsByType;
            IDictionary oSByTE = cache.oceanSettlementsByTypeExclusive;
            IDictionary oSByLT = cache.oceanSettlementsByLocationType;
            IDictionary oSByLTE = cache.oceanSettlementsByLocationTypeExclusive;
            IDictionary oSByLTByT = cache.oceanSettlementsByLocationTypeByType;
            IDictionary oSByLTEByT = cache.oceanSettlementsByLocationTypeExclusiveByType;
            IDictionary oSByLTByTE = cache.oceanSettlementsByLocationTypeByTypeExclusive;
            IDictionary oSByLTEByTE = cache.oceanSettlementsByLocationTypeExclusiveByTypeExclusive;
            // Settlements - With Social Groups
            IDictionary oSBySG = cache.oceanSettlementsBySocialGroup;
            IDictionary oSBySGT = cache.oceanSettlementsBySocialGroupType;
            IDictionary oSBySGTE = cache.oceanSettlementsBySocialGroupTypeExclusive;
            IDictionary oSBySGByT = cache.oceanSettlementsBySocialGroupByType;
            IDictionary oSBySGTByT = cache.oceanSettlementsBySocialGroupTypeByType;
            IDictionary oSBySGTEByT = cache.oceanSettlementsBySocialGroupTypeExclusiveByType;
            IDictionary oSBySGTByTE = cache.oceanSettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary oSBySGTEByTE = cache.oceanSettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Settlements - Without Social Groups
            IDictionary oSWoSGByT = cache.oceanSettlementsWithoutSocialGroupByType;
            IDictionary oSWoSGByTE = cache.oceanSettlementsWithoutSocialGroupByTypeExclusive;
            // Specific Settlements
            List<Set_City>[] oCityByL = cache.oceanCityByLevel;
            IDictionary oCityBySGByL = cache.oceanCityBySocialGroupByLevel;
            IDictionary oCityBySGTByL = cache.oceanCityBySocialGroupTypeByLevel;
            IDictionary oCityBySGTEByL = cache.oceanCityBySocialGroupTypeExclusiveByLevel;
            List<Set_City>[] oCityWoSGByL = cache.oceanCityWithoutSocialGroupByLevel;
            List<Set_MinorHuman>[] oMinorHumanByL = cache.oceanMinorHumanByLevel;
            IDictionary oMinorHumanBySGByL = cache.oceanMinorHumanBySocialGroupByLevel;
            IDictionary oMinorHumanBySGTByL = cache.oceanMinorHumanBySocialGroupTypeByLevel;
            IDictionary oMinorHumanBySGTEByL = cache.oceanMinorHumanBySocialGroupTypeExclusiveByLevel;
            List<Set_MinorHuman>[] oMinorHumanWoSGByL = cache.oceanMinorHumanWithoutSocialGroupByLevel;
            List<Set_OrcCamp>[] oOrcCampByS = cache.oceanOrcCampBySpecialism;
            IDictionary oOrcCampBySGByS = cache.oceanOrcCampBySocialGroupBySpecialism;
            IDictionary oOrcCampBySGTByS = cache.oceanOrcCampBySocialGroupTypeBySpecialism;
            IDictionary oOrcCampBySGTEByS = cache.oceanOrcCampBySocialGroupTypeExclusiveBySpecialism;
            List<Set_OrcCamp>[] oOrcCampWoSGByS = cache.oceanOrcCampWithoutSocialGroupBySpecialism;
            // Ocean Subsettlement Dictionaries
            List<Subsettlement> oceanSubsettlements = cache.oceanSubsettlements;
            IDictionary oSsByT = cache.oceanSubsettlementsByType;
            IDictionary oSsByTE = cache.oceanSubsettlementsByTypeExclusive;
            IDictionary oSsByLT = cache.oceanSubsettlementsByLocationType;
            IDictionary oSsByLTE = cache.oceanSubsettlementsByLocationTypeExclusive;
            IDictionary oSsByLTByT = cache.oceanSubsettlementsByLocationTypeByType;
            IDictionary oSsByLTEByT = cache.oceanSubsettlementsByLocationTypeExclusiveByType;
            IDictionary oSsByLTByTE = cache.oceanSubsettlementsByLocationTypeByTypeExclusive;
            IDictionary oSsByLTEByTE = cache.oceanSubsettlementsByLocationTypeExclusiveByTypeExclusive;
            IDictionary oSsByST = cache.oceanSubsettlementsBySettlementType;
            IDictionary oSsBySTE = cache.oceanSubsettlementsBySettlementTypeExclusive;
            IDictionary oSsBySTByT = cache.oceanSubsettlementsBySettlementTypeByType;
            IDictionary oSsBySTEByT = cache.oceanSubsettlementsBySettlementTypeExclusiveByType;
            IDictionary oSsBySTByTE = cache.oceanSubsettlementsBySettlementTypeByTypeExclusive;
            IDictionary oSsBySTEByTE = cache.oceanSubsettlementsBySettlementTypeExclusiveByTypeExclusive;
            // Subsettlements - With Social Groups
            IDictionary oSsBySG = cache.oceanSubsettlementsBySocialGroup;
            IDictionary oSsBySGT = cache.oceanSubsettlementsBySocialGroupType;
            IDictionary oSsBySGTE = cache.oceanSubsettlementsBySocialGroupTypeExclusive;
            IDictionary oSsBySGByT = cache.oceanSubsettlementsBySocialGroupByType;
            IDictionary oSsBySGTByT = cache.oceanSubsettlementsBySocialGroupTypeByType;
            IDictionary oSsBySGTEByT = cache.oceanSubsettlementsBySocialGroupTypeExclusiveByType;
            IDictionary oSsBySGTByTE = cache.oceanSubsettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary oSsBySGTEByTE = cache.oceanSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Subsettlements - Without Social Groups
            IDictionary oSsWoSGByT = cache.oceanSubsettlementsWithoutSocialGroupByType;
            IDictionary oSsWoSGByTE = cache.oceanSubsettlementsWithoutSocialGroupByTypeExclusive;
            // Ocean Property Dictionaries
            List<Property> oceanProperties = cache.oceanProperties;
            IDictionary oPByT = cache.oceanPropertiesByType;
            IDictionary oPByTE = cache.oceanPropertiesByTypeExclusive;
            IDictionary oPByLT = cache.oceanPropertiesByLocationType;
            IDictionary oPByLTE = cache.oceanPropertiesByLocationTypeExclusive;
            IDictionary oPByLTByT = cache.oceanPropertiesByLocationTypeByType;
            IDictionary oPByLTEByT = cache.oceanPropertiesByLocationTypeExclusiveByType;
            IDictionary oPByLTByTE = cache.oceanPropertiesByLocationTypeByTypeExclusive;
            IDictionary oPByLTEByTE = cache.oceanPropertiesByLocationTypeExclusiveByTypeExclusive;
            IDictionary oPByST = cache.oceanPropertiesBySettlementType;
            IDictionary oPBySTE = cache.oceanPropertiesBySettlementTypeExclusive;
            IDictionary oPBySTByT = cache.oceanPropertiesBySettlementTypeByType;
            IDictionary oPBySTEByT = cache.oceanPropertiesBySettlementTypeExclusiveByType;
            IDictionary oPBySTByTE = cache.oceanPropertiesBySettlementTypeByTypeExclusive;
            IDictionary oPBySTEByTE = cache.oceanPropertiesBySettlementTypeExclusiveByTypeExclusive;
            // Properties - With Social Groups
            IDictionary oPBySG = cache.oceanPropertiesBySocialGroup;
            IDictionary oPBySGT = cache.oceanPropertiesBySocialGroupType;
            IDictionary oPBySGTE = cache.oceanPropertiesBySocialGroupTypeExclusive;
            IDictionary oPBySGByT = cache.oceanPropertiesBySocialGroupByType;
            IDictionary oPBySGTByT = cache.oceanPropertiesBySocialGroupTypeByType;
            IDictionary oPBySGTEByT = cache.oceanPropertiesBySocialGroupTypeExclusiveByType;
            IDictionary oPBySGTByTE = cache.oceanPropertiesBySocialGroupTypeByTypeExclusive;
            IDictionary oPBySGTEByTE = cache.oceanPropertiesBySocialGroupTypeExclusiveByTypeExclusive;
            // Properties - Without Social Groups
            IDictionary oPWoSGByT = cache.oceanPropertiesWithoutSocialGroupByType;
            IDictionary oPWoSGByTE = cache.oceanPropertiesWithoutSocialGroupByTypeExclusive;
            // Properties - Without Settlements
            IDictionary oPWoSByT = cache.oceanPropertiesWithoutSettlementByType;
            IDictionary oPWoSByTE = cache.oceanPropertiesWithoutSettlementByTypeExclusive;
            IDictionary oPWoSBySG = cache.oceanPropertiesWithoutSettlementBySocialGroup;
            IDictionary oPWoSBySGT = cache.oceanPropertiesWithoutSettlementBySocialGroupType;
            IDictionary oPWoSBySGTE = cache.oceanPropertiesWithoutSettlementBySocialGroupTypeExclusive;
            // Propertes - Without Social Groups and Settlements
            IDictionary oPWoSG_SByT = cache.oceanPropertiesWithoutSocialGroupOrSettlementByType;
            IDictionary oPWoSG_SByTE = cache.oceanPropertiesWithoutSocialGroupOrSettlementByTypeExclusive;

            // Terrestrial Location Dictionaries //
            List<Location> terrestrialLocations = cache.terrestrialLocations;
            IDictionary tLByT = cache.terrestrialLocationsByType;
            IDictionary tLByTE = cache.terrestrialLocationsByTypeExclusive;
            // Locations - With Social Groups
            IDictionary tLBySG = cache.terrestrialLocationsBySocialGroup;
            IDictionary tLBySGT = cache.terrestrialLocationsBySocialGroupType;
            IDictionary tLBySGTE = cache.terrestrialLocationsBySocialGroupTypeExclusive;
            IDictionary tLBySGByT = cache.terrestrialLocationsBySocialGroupByType;
            IDictionary tLBySGTByT = cache.terrestrialLocationsBySocialGroupTypeByType;
            IDictionary tLBySGTEByT = cache.terrestrialLocationsBySocialGroupTypeExclusiveByType;
            IDictionary tLBySGTByTE = cache.terrestrialLocationsBySocialGroupTypeByTypeExclusive;
            IDictionary tLBySGTEByTE = cache.terrestrialLocationsBySocialGroupTypeExclusiveByTypeExclusive;
            // Locations - Without Social Groups
            IDictionary tLWoSGByT = cache.terrestrialLocationsWithoutSocialGroupByType;
            IDictionary tLWoSGByTE = cache.terrestrialLocationsWithoutSocialGroupByTypeExclusive;
            // Locations - Without Settlements
            IDictionary tLWoSByT = cache.terrestrialLocationsWithoutSettlementByType;
            IDictionary tLWoSByTE = cache.terrestrialLocationsWithoutSettlementByTypeExclusive;
            IDictionary tLWoSBySG = cache.terrestrialLocationsWithoutSettlementBySocialGroup;
            IDictionary tLWoSBySGT = cache.terrestrialLocationsWithoutSettlementBySocialGroupType;
            IDictionary tLWoSBySGTE = cache.terrestrialLocationsWithoutSettlementBySocialGroupTypeExclusive;
            // Location - Without Social Groups and Settlements
            IDictionary tLWoSG_SByT = cache.terrestrialLocationsWithoutSocialGroupOrSettlementByType;
            IDictionary tLWoSG_SByTE = cache.terrestrialLocationsWithoutSocialGroupOrSettlementByTypeExclusive;
            // Terrestrial Settlement Dictionaries
            List<Settlement> terrestrialSettlements = cache.terrestrialSettlements;
            IDictionary tSByT = cache.terrestrialSettlementsByType;
            IDictionary tSByTE = cache.terrestrialSettlementsByTypeExclusive;
            IDictionary tSByLT = cache.terrestrialSettlementsByLocationType;
            IDictionary tSByLTE = cache.terrestrialSettlementsByLocationTypeExclusive;
            IDictionary tSByLTByT = cache.terrestrialSettlementsByLocationTypeByType;
            IDictionary tSByLTEByT = cache.terrestrialSettlementsByLocationTypeExclusiveByType;
            IDictionary tSByLTByTE = cache.terrestrialSettlementsByLocationTypeByTypeExclusive;
            IDictionary tSByLTEByTE = cache.terrestrialSettlementsByLocationTypeExclusiveByTypeExclusive;
            // Settlements - With Social Groups
            IDictionary tSBySG = cache.terrestrialSettlementsBySocialGroup;
            IDictionary tSBySGT = cache.terrestrialSettlementsBySocialGroupType;
            IDictionary tSBySGTE = cache.terrestrialSettlementsBySocialGroupTypeExclusive;
            IDictionary tSBySGByT = cache.terrestrialSettlementsBySocialGroupByType;
            IDictionary tSBySGTByT = cache.terrestrialSettlementsBySocialGroupTypeByType;
            IDictionary tSBySGTEByT = cache.terrestrialSettlementsBySocialGroupTypeExclusiveByType;
            IDictionary tSBySGTByTE = cache.terrestrialSettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary tSBySGTEByTE = cache.terrestrialSettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Settlements - Without Social Groups
            IDictionary tSWoSGByT = cache.terrestrialSettlementsWithoutSocialGroupByType;
            IDictionary tSWoSGByTE = cache.terrestrialSettlementsWithoutSocialGroupByTypeExclusive;
            // Specific Settlements
            List<Set_City>[] tCityByL = cache.terrestrialCityByLevel;
            IDictionary tCityBySGByL = cache.terrestrialCityBySocialGroupByLevel;
            IDictionary tCityBySGTByL = cache.terrestrialCityBySocialGroupTypeByLevel;
            IDictionary tCityBySGTEByL = cache.terrestrialCityBySocialGroupTypeExclusiveByLevel;
            List<Set_City>[] tCityWoSGByL = cache.terrestrialCityWithoutSocialGroupByLevel;
            List<Set_MinorHuman>[] tMinorHumanByL = cache.terrestrialMinorHumanByLevel;
            IDictionary tMinorHumanBySGByL = cache.terrestrialMinorHumanBySocialGroupByLevel;
            IDictionary tMinorHumanBySGTByL = cache.terrestrialMinorHumanBySocialGroupTypeByLevel;
            IDictionary tMinorHumanBySGTEByL = cache.terrestrialMinorHumanBySocialGroupTypeExclusiveByLevel;
            List<Set_MinorHuman>[] tMinorHumanWoSGByL = cache.terrestrialMinorHumanWithoutSocialGroupByLevel;
            List<Set_OrcCamp>[] tOrcCampByS = cache.terrestrialOrcCampBySpecialism;
            IDictionary tOrcCampBySGByS = cache.terrestrialOrcCampBySocialGroupBySpecialism;
            IDictionary tOrcCampBySGTByS = cache.terrestrialOrcCampBySocialGroupTypeBySpecialism;
            IDictionary tOrcCampBySGTEByS = cache.terrestrialOrcCampBySocialGroupTypeExclusiveBySpecialism;
            List<Set_OrcCamp>[] tOrcCampWoSGByS = cache.terrestrialOrcCampWithoutSocialGroupBySpecialism;
            // Terrestrial Subsettlement Dictionaries
            List<Subsettlement> terrestrialSubsettlements = cache.terrestrialSubsettlements;
            IDictionary tSsByT = cache.terrestrialSubsettlementsByType;
            IDictionary tSsByTE = cache.terrestrialSubsettlementsByTypeExclusive;
            IDictionary tSsByLT = cache.terrestrialSubsettlementsByLocationType;
            IDictionary tSsByLTE = cache.terrestrialSubsettlementsByLocationTypeExclusive;
            IDictionary tSsByLTByT = cache.terrestrialSubsettlementsByLocationTypeByType;
            IDictionary tSsByLTEByT = cache.terrestrialSubsettlementsByLocationTypeExclusiveByType;
            IDictionary tSsByLTByTE = cache.terrestrialSubsettlementsByLocationTypeByTypeExclusive;
            IDictionary tSsByLTEByTE = cache.terrestrialSubsettlementsByLocationTypeExclusiveByTypeExclusive;
            IDictionary tSsByST = cache.terrestrialSubsettlementsBySettlementType;
            IDictionary tSsBySTE = cache.terrestrialSubsettlementsBySettlementTypeExclusive;
            IDictionary tSsBySTByT = cache.terrestrialSubsettlementsBySettlementTypeByType;
            IDictionary tSsBySTEByT = cache.terrestrialSubsettlementsBySettlementTypeExclusiveByType;
            IDictionary tSsBySTByTE = cache.terrestrialSubsettlementsBySettlementTypeByTypeExclusive;
            IDictionary tSsBySTEByTE = cache.terrestrialSubsettlementsBySettlementTypeExclusiveByTypeExclusive;
            // Subsettlements - With Social Groups
            IDictionary tSsBySG = cache.terrestrialSubsettlementsBySocialGroup;
            IDictionary tSsBySGT = cache.terrestrialSubsettlementsBySocialGroupType;
            IDictionary tSsBySGTE = cache.terrestrialSubsettlementsBySocialGroupTypeExclusive;
            IDictionary tSsBySGByT = cache.terrestrialSubsettlementsBySocialGroupByType;
            IDictionary tSsBySGTByT = cache.terrestrialSubsettlementsBySocialGroupTypeByType;
            IDictionary tSsBySGTEByT = cache.terrestrialSubsettlementsBySocialGroupTypeExclusiveByType;
            IDictionary tSsBySGTByTE = cache.terrestrialSubsettlementsBySocialGroupTypeByTypeExclusive;
            IDictionary tSsBySGTEByTE = cache.terrestrialSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive;
            // Subsettlements - Without Social Groups
            IDictionary tSsWoSGByT = cache.subsettlementsWithoutSocialGroupByType;
            IDictionary tSsWoSGByTE = cache.subsettlementsWithoutSocialGroupByTypeExclusive;
            // Terrestrial Property Dictionaries
            List<Property> terrestrialProperties = cache.terrestrialProperties;
            IDictionary tPByT = cache.terrestrialPropertiesByType;
            IDictionary tPByTE = cache.terrestrialPropertiesByTypeExclusive;
            IDictionary tPByLT = cache.terrestrialPropertiesByLocationType;
            IDictionary tPByLTE = cache.terrestrialPropertiesByLocationTypeExclusive;
            IDictionary tPByLTByT = cache.terrestrialPropertiesByLocationTypeByType;
            IDictionary tPByLTEByT = cache.terrestrialPropertiesByLocationTypeExclusiveByType;
            IDictionary tPByLTByTE = cache.terrestrialPropertiesByLocationTypeByTypeExclusive;
            IDictionary tPByLTEByTE = cache.terrestrialPropertiesByLocationTypeExclusiveByTypeExclusive;
            IDictionary tPByST = cache.terrestrialPropertiesBySettlementType;
            IDictionary tPBySTE = cache.terrestrialPropertiesBySettlementTypeExclusive;
            IDictionary tPBySTByT = cache.terrestrialPropertiesBySettlementTypeByType;
            IDictionary tPBySTEByT = cache.terrestrialPropertiesBySettlementTypeExclusiveByType;
            IDictionary tPBySTByTE = cache.terrestrialPropertiesBySettlementTypeByTypeExclusive;
            IDictionary tPBySTEByTE = cache.terrestrialPropertiesBySettlementTypeExclusiveByTypeExclusive;
            // Properties - With Social Groups
            IDictionary tPBySG = cache.terrestrialPropertiesBySocialGroup;
            IDictionary tPBySGT = cache.terrestrialPropertiesBySocialGroupType;
            IDictionary tPBySGTE = cache.terrestrialPropertiesBySocialGroupTypeExclusive;
            IDictionary tPBySGByT = cache.terrestrialPropertiesBySocialGroupByType;
            IDictionary tPBySGTByT = cache.terrestrialPropertiesBySocialGroupTypeByType;
            IDictionary tPBySGTEByT = cache.terrestrialPropertiesBySocialGroupTypeExclusiveByType;
            IDictionary tPBySGTByTE = cache.terrestrialPropertiesBySocialGroupTypeByTypeExclusive;
            IDictionary tPBySGTEByTE = cache.terrestrialPropertiesBySocialGroupTypeExclusiveByTypeExclusive;
            // Properties - Without Social Groups
            IDictionary tPWoSGByT = cache.terrestrialPropertiesWithoutSocialGroupByType;
            IDictionary tPWoSGByTE = cache.terrestrialPropertiesWithoutSocialGroupByTypeExclusive;
            // Properties - Without Settlements
            IDictionary tPWoSByT = cache.terrestrialPropertiesWithoutSettlementByType;
            IDictionary tPWoSByTE = cache.terrestrialPropertiesWithoutSettlementByTypeExclusive;
            IDictionary tPWoSBySG = cache.terrestrialPropertiesWithoutSettlementBySocialGroup;
            IDictionary tPWoSBySGT = cache.terrestrialPropertiesWithoutSettlementBySocialGroupType;
            IDictionary tPWoSBySGTE = cache.terrestrialPropertiesWithoutSettlementBySocialGroupTypeExclusive;
            // Propertes - Without Social Groups and Settlements
            IDictionary tPWoSG_SByT = cache.terrestrialPropertiesWithoutSocialGroupOrSettlementByType;
            IDictionary tPWoSG_SByTE = cache.terrestrialPropertiesWithoutSocialGroupOrSettlementByTypeExclusive;


            // Distance Dictionaries //
            // Location Distance Dictionaries
            Dictionary<Location, Dictionary<Location, double>> dByLfromL = cache.distanceByLocationsFromLocation;
            Dictionary<Location, List<Location>[]> lBySEfromL = cache.locationsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Location>[]> cLBySEfromL = cache.locationsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Location>[]> oLBySEfromL = cache.locationsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Location>[]> tLBySEfromL = cache.locationsByStepsExclusiveFromLocation;
            // Settlement Distance Dictionaries
            Dictionary<Location, List<Settlement>[]> sBySEfromL = cache.settlementsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Settlement>[]> cSBySEfromL = cache.coastalSettlementsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Settlement>[]> oSBySEfromL = cache.coastalSettlementsByStepsExclusiveFromLocation;
            Dictionary<Location, List<Settlement>[]> tSBySEfromL = cache.terrestrialSettlementsByStepsExclusiveFromLocation;
            // Unit Distance Dictionaries
            Dictionary<Location, List<Unit>[]> uBySEfromL = cache.unitsByStepsExclusiveFromLocation;

            // Initialize loop-only variables
            bool iterateSGT;
            bool iterateLT;
            bool iterateST;
            bool iterateSsT;
            bool iteratePT;
            bool isFirstSGTIteration;
            bool isFirstLTIteration;
            bool isFirstSTIteration;
            bool isFirstSsTIteration;
            bool isFirstPTIteration;
            bool isInitialLTIteration;
            bool isInitialSTIteration;
            bool isInitialSsTIteration;
            bool isInitialPTIteration;
            Type targetTSG = typeof(SocialGroup);
            Type targetTL = typeof(Location);
            Type targetTS = typeof(Settlement);
            Type targetTSub = typeof(Subsettlement);
            Type targetTP = typeof(Property);

            foreach (Location l in map.locations)
            {
                //Console.WriteLine("CommunityLib: Filtering Location " + l.getName() + " of Type " + l.GetType().Name + ".");
                // Set universal variables
                s = l.settlement;
                sG = l.soc;
                tS = null;
                tSG = null;
                tL = l.GetType();

                if (s != null)
                {
                    tS = s.GetType();
                }
                if (sG != null)
                {
                    tSG = sG.GetType();
                }

                //Console.WriteLine("CommunityLib: Starting Distance Computations.");
                distance = 0;
                steps = 0;

                foreach (Location loc2 in map.locations)
                {
                    distance = map.getDist(l, loc2);
                    steps = Math.Min(map.getStepDist(l, loc2), 125);

                    if (!dByLfromL.ContainsKey(l))
                    {
                        dByLfromL.Add(l, new Dictionary<Location, double>());
                    }
                    else if (dByLfromL[l] == null)
                    {
                        dByLfromL[l] = new Dictionary<Location, double>();
                    }
                    dByLfromL[l].Add(loc2, distance);

                    if (!lBySEfromL.TryGetValue(l, out locationArray))
                    {
                        lBySEfromL.Add(l, new List<Location>[125]);
                        locationArray = lBySEfromL[l];
                    }
                    else if (locationArray == null)
                    {
                        lBySEfromL[l] = new List<Location>[125];
                        locationArray = lBySEfromL[l];
                    }
                    if (locationArray[steps] == null)
                    {
                        locationArray[steps] = new List<Location>();
                    }
                    locationArray[steps].Add(loc2);

                    if (loc2.isOcean)
                    {
                        oceanLocations.Add(l);

                        if (!oLBySEfromL.TryGetValue(l, out locationArray))
                        {
                            oLBySEfromL.Add(l, new List<Location>[125]);
                            locationArray = oLBySEfromL[l];
                        }
                        else if (locationArray == null)
                        {
                            oLBySEfromL[l] = new List<Location>[125];
                            locationArray = lBySEfromL[l];
                        }
                        if (locationArray[steps] == null)
                        {
                            locationArray[steps] = new List<Location>();
                        }
                        locationArray[steps].Add(loc2);
                    }
                    else
                    {
                        if (!tLBySEfromL.TryGetValue(l, out locationArray))
                        {
                            tLBySEfromL.Add(l, new List<Location>[125]);
                            locationArray = tLBySEfromL[l];
                        }
                        else if (locationArray == null)
                        {
                            tLBySEfromL[l] = new List<Location>[125];
                            locationArray = tLBySEfromL[l];
                        }
                        if (locationArray[steps] == null)
                        {
                            locationArray[steps] = new List<Location>();
                        }
                        locationArray[steps].Add(loc2);

                        if (loc2.isCoastal)
                        {
                            coastalLocations.Add(l);

                            if (!cLBySEfromL.TryGetValue(l, out locationArray))
                            {
                                cLBySEfromL.Add(l, new List<Location>[125]);
                                locationArray = cLBySEfromL[l];
                            }
                            else if (locationArray == null)
                            {
                                cLBySEfromL[l] = new List<Location>[125];
                                locationArray = cLBySEfromL[l];
                            }
                            if (locationArray[steps] == null)
                            {
                                locationArray[steps] = new List<Location>();
                            }
                            locationArray[steps].Add(loc2);
                        }
                    }

                    s = loc2.settlement;
                    if (s != null)
                    {
                        if (!sBySEfromL.TryGetValue(l, out settlementArray))
                        {
                            sBySEfromL.Add(l, new List<Settlement>[125]);
                            settlementArray = sBySEfromL[l];
                        }
                        else if (settlementArray == null)
                        {
                            sBySEfromL[l] = new List<Settlement>[125];
                            settlementArray = sBySEfromL[l];
                        }
                        if (settlementArray[steps] == null)
                        {
                            settlementArray[steps] = new List<Settlement>();
                        }
                        settlementArray[steps].Add(s);

                        if (loc2.isOcean)
                        {
                            if (!oSBySEfromL.TryGetValue(l, out settlementArray))
                            {
                                oSBySEfromL.Add(l, new List<Settlement>[125]);
                                settlementArray = oSBySEfromL[l];
                            }
                            else if (settlementArray == null)
                            {
                                oSBySEfromL[l] = new List<Settlement>[125];
                                settlementArray = oSBySEfromL[l];
                            }
                            if (settlementArray[steps] == null)
                            {
                                settlementArray[steps] = new List<Settlement>();
                            }
                            settlementArray[steps].Add(s);
                        }
                        else
                        {
                            if (!tSBySEfromL.TryGetValue(l, out settlementArray))
                            {
                                tSBySEfromL.Add(l, new List<Settlement>[125]);
                                settlementArray = tSBySEfromL[l];
                            }
                            else if (settlementArray == null)
                            {
                                tSBySEfromL[l] = new List<Settlement>[125];
                                settlementArray = tSBySEfromL[l];
                            }
                            if (settlementArray[steps] == null)
                            {
                                settlementArray[steps] = new List<Settlement>();
                            }
                            settlementArray[steps].Add(s);

                            if (loc2.isCoastal)
                            {
                                cache.coastalLocations.Add(l);

                                if (!cSBySEfromL.TryGetValue(l, out settlementArray))
                                {
                                    cSBySEfromL.Add(l, new List<Settlement>[125]);
                                    settlementArray = cSBySEfromL[l];
                                }
                                else if (settlementArray == null)
                                {
                                    cSBySEfromL[l] = new List<Settlement>[125];
                                    settlementArray = cSBySEfromL[l];
                                }
                                if (settlementArray[steps] == null)
                                {
                                    settlementArray[steps] = new List<Settlement>();
                                }
                                settlementArray[steps].Add(s);
                            }
                        }
                    }

                    units = loc2.units;
                    if (units != null && units.Count > 0)
                    {
                        if (!uBySEfromL.ContainsKey(l))
                        {
                            uBySEfromL.Add(l, new List<Unit>[125]);
                        }
                        else if (uBySEfromL[l] == null)
                        {
                            uBySEfromL[l] = new List<Unit>[125];
                        }
                        if (uBySEfromL[l][steps] == null)
                        {
                            uBySEfromL[l][steps] = new List<Unit>();
                        }
                        uBySEfromL[l][steps].AddRange(units);
                    }
                }
                //Console.WriteLine("CommunityLib: Distance calculations complete.");

                // Reset variable s to l.settlement after distance calculations uses it for loc2.settlement.
                s = l.settlement;

                isFirstLTIteration = true;
                isFirstSTIteration = true;
                isFirstSsTIteration = true;
                isFirstPTIteration = true;

                //Console.WriteLine("CommunityLib: Starting loops for Location " + l.name + " of Type " + l.GetType().Name + ".");
                if (sG != null)
                {
                    iterateSGT = true;
                    isFirstSGTIteration = true;

                    //Console.WriteLine("CommunityLib: Starting scoial group type loop.");
                    while (iterateSGT)
                    {
                        //Console.WriteLine("CommunityLib: Location belongs to Social Group " + sG.name + " of Type " + tSG.Name + ".");
                        tL = l.GetType();
                        iterateLT = true;
                        isInitialLTIteration = true;

                        //Console.WriteLine("CommunityLib: Starting location type loop.");
                        while (iterateLT)
                        {
                            //Console.WriteLine("CommunityLib: Location is of Type " + tL.Name + ".");

                            // Very first instace. Fires once.
                            if (isFirstLTIteration)
                            {
                                //Console.WriteLine("CommunityLib: Is first iteration of location type loop.");
                                CreateAndOrAddToKeyListPair(lByTE, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySG, sG, typeof(Location), l);
                                CreateAndOrAddToKeyListPair(lBySGTE, tSG, typeof(Location), l);

                                TryCreateSubDictionary(lBySGByT, sG, typeof(Type));
                                TryCreateSubDictionary(lBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(lBySGTEByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(lBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                                if (l.isOcean)
                                {
                                    oceanLocations.Add(l);
                                    CreateAndOrAddToKeyListPair(oLByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLBySG, sG, typeof(Location), l);
                                    CreateAndOrAddToKeyListPair(oLBySGTE, tSG, typeof(Location), l);

                                    TryCreateSubDictionary(oLBySGByT, sG, typeof(Type));
                                    TryCreateSubDictionary(oLBySGTEByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(oLBySGTEByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(oLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                                }
                                else
                                {
                                    terrestrialLocations.Add(l);
                                    CreateAndOrAddToKeyListPair(tLByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLBySG, sG, typeof(Location), l);
                                    CreateAndOrAddToKeyListPair(tLBySGTE, tSG, typeof(Location), l);

                                    TryCreateSubDictionary(tLBySGByT, sG, typeof(Type));
                                    TryCreateSubDictionary(tLBySGTEByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(tLBySGTEByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(tLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                                    if (l.isCoastal)
                                    {
                                        coastalLocations.Add(l);
                                        CreateAndOrAddToKeyListPair(cLByTE, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLBySG, sG, typeof(Location), l);
                                        CreateAndOrAddToKeyListPair(cLBySGTE, tSG, typeof(Location), l);

                                        TryCreateSubDictionary(cLBySGByT, sG, typeof(Type));
                                        TryCreateSubDictionary(cLBySGTEByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(cLBySGTEByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(cLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                                    }
                                }
                            }

                            // First instance of loop. Fires once per iterator.
                            if (isInitialLTIteration)
                            {
                                //Console.WriteLine("CommunityLib: Is initial iteration of location type loop.");
                                CreateAndOrAddToKeyListPair(lBySGT, tSG, typeof(Location), l);

                                TryCreateSubDictionary(lBySGTByT, tSG, typeof(Type));
                                TryCreateSubDictionary(lBySGTByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(lBySGTByTE[tSG] as IDictionary, tL, tL, l);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oLBySGT, tSG, typeof(Location), l);

                                    TryCreateSubDictionary(oLBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(oLBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(oLBySGTByTE[tSG] as IDictionary, tL, tL, l);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tLBySGT, tSG, typeof(Location), l);

                                    TryCreateSubDictionary(tLBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(tLBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(tLBySGTByTE[tSG] as IDictionary, tL, tL, l);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cLBySGT, tSG, typeof(Location), l);

                                        TryCreateSubDictionary(cLBySGTByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(cLBySGTByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(cLBySGTByTE[tSG] as IDictionary, tL, tL, l);
                                    }
                                }
                            }

                            // Fires each iteration, but only for first iteration of social group loop.
                            // Iterates location type.
                            if (isFirstSGTIteration)
                            {
                                //Console.WriteLine("CommunityLib: Is first iteration of social group type loop.");
                                CreateAndOrAddToKeyListPair(lByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGByT[sG] as IDictionary, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oLByT, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLBySGByT[sG] as IDictionary, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tLByT, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLBySGByT[sG] as IDictionary, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cLByT, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLBySGByT[sG] as IDictionary, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                    }
                                }
                            }

                            // Fires each iteration.
                            // Iterates social group type and location type.
                            CreateAndOrAddToKeyListPair(lBySGTByT[tSG] as IDictionary, tL, tL, l);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oLBySGTByT[tSG] as IDictionary, tL, tL, l);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tLBySGTByT[tSG] as IDictionary, tL, tL, l);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cLBySGTByT[tSG] as IDictionary, tL, tL, l);
                                }
                            }

                            if (s != null)
                            {
                                tS = s.GetType();
                                iterateST = true;
                                isInitialSTIteration = true;

                                //Console.WriteLine("CommunityLib: Starting settlement type loop.");
                                while (iterateST)
                                {
                                    //Console.WriteLine("CommunityLib: Settlement " + s.name + " is of Type " + tS.Name + ".");
                                    // Very first instace. Fires only once.
                                    // Iterates nothing.
                                    if (isFirstSTIteration)
                                    {
                                        settlements.Add(s);
                                        CreateAndOrAddToKeyListPair(sByTE, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(sBySG, sG, typeof(Settlement), s);
                                        CreateAndOrAddToKeyListPair(sBySGTE, tSG, typeof(Settlement), s);

                                        TryCreateSubDictionary(sByLTEByT, tL, typeof(Type));
                                        TryCreateSubDictionary(sByLTEByTE, tL, typeof(Type));
                                        TryCreateSubDictionary(sBySGByT, sG, typeof(Type));
                                        TryCreateSubDictionary(sBySGTEByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(sBySGTEByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(sByLTEByTE[tL] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(sBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            oceanSettlements.Add(s);
                                            CreateAndOrAddToKeyListPair(oSByTE, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(oSBySG, sG, typeof(Settlement), s);
                                            CreateAndOrAddToKeyListPair(oSBySGTE, tSG, typeof(Settlement), s);

                                            TryCreateSubDictionary(oSByLTEByT, tL, typeof(Type));
                                            TryCreateSubDictionary(oSByLTEByTE, tL, typeof(Type));
                                            TryCreateSubDictionary(oSBySGByT, sG, typeof(Type));
                                            TryCreateSubDictionary(oSBySGTEByT, tSG, typeof(Type));
                                            TryCreateSubDictionary(oSBySGTEByTE, tSG, typeof(Type));

                                            CreateAndOrAddToKeyListPair(oSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(oSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            terrestrialSettlements.Add(s);
                                            CreateAndOrAddToKeyListPair(tSByTE, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(tSBySG, sG, typeof(Settlement), s);
                                            CreateAndOrAddToKeyListPair(tSBySGTE, tSG, typeof(Settlement), s);

                                            TryCreateSubDictionary(tSByLTEByT, tL, typeof(Type));
                                            TryCreateSubDictionary(tSByLTEByTE, tL, typeof(Type));
                                            TryCreateSubDictionary(tSBySGByT, sG, typeof(Type));
                                            TryCreateSubDictionary(tSBySGTEByT, tSG, typeof(Type));
                                            TryCreateSubDictionary(tSBySGTEByTE, tSG, typeof(Type));

                                            CreateAndOrAddToKeyListPair(tSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(tSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                coastalSettlements.Add(s);
                                                CreateAndOrAddToKeyListPair(cSByTE, tS, tS, s);
                                                CreateAndOrAddToKeyListPair(cSBySG, sG, typeof(Settlement), s);
                                                CreateAndOrAddToKeyListPair(cSBySGTE, tSG, typeof(Settlement), s);

                                                TryCreateSubDictionary(cSByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(cSByLTEByTE, tL, typeof(Type));
                                                TryCreateSubDictionary(cSBySGByT, sG, typeof(Type));
                                                TryCreateSubDictionary(cSBySGTEByT, tSG, typeof(Type));
                                                TryCreateSubDictionary(cSBySGTEByTE, tSG, typeof(Type));

                                                CreateAndOrAddToKeyListPair(cSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                                CreateAndOrAddToKeyListPair(cSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    // Fires each iteration, but only for first iteration of location type and social group type loops.
                                    // Iterates settlement type
                                    if (isFirstSGTIteration && isFirstLTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(sByT, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(sByLTE, tL, typeof(Settlement), s);
                                        CreateAndOrAddToKeyListPair(sByLTEByT[tL] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(sBySGByT[sG] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(sBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSByT, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(oSByLTE, tL, typeof(Settlement), s);
                                            CreateAndOrAddToKeyListPair(oSByLTEByT[tL] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(oSBySGByT[sG] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(oSBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSByT, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(tSByLTE, tL, typeof(Settlement), s);
                                            CreateAndOrAddToKeyListPair(tSByLTEByT[tL] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(tSBySGByT[sG] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(tSBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSByT, tS, tS, s);
                                                CreateAndOrAddToKeyListPair(cSByLTE, tL, typeof(Settlement), s);
                                                CreateAndOrAddToKeyListPair(cSByLTEByT[tL] as IDictionary, tS, tS, s);
                                                CreateAndOrAddToKeyListPair(cSBySGByT[sG] as IDictionary, tS, tS, s);
                                                CreateAndOrAddToKeyListPair(cSBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    // Fires once each iteration, but only for first iteration of social group type loop.
                                    // Iterates location type
                                    if (isFirstSGTIteration && isInitialSTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(sByLT, tL, typeof(Settlement), s);

                                        TryCreateSubDictionary(sByLTByT, tL, typeof(Type));
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSByLT, tL, typeof(Settlement), s);

                                            TryCreateSubDictionary(oSByLTByT, tL, typeof(Type));
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSByLT, tL, typeof(Settlement), s);

                                            TryCreateSubDictionary(tSByLTByT, tL, typeof(Type));
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSByLT, tL, typeof(Settlement), s);

                                                TryCreateSubDictionary(cSByLTByT, tL, typeof(Type));
                                            }
                                        }
                                    }

                                    // Fire once each iteration of social group type loop.
                                    // Iterates social group type.
                                    if (isInitialLTIteration && isInitialSTIteration)
                                    {
                                        TryCreateSubDictionary(sBySGTByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(sBySGTByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(sBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            TryCreateSubDictionary(oSBySGTByT, tSG, typeof(Type));
                                            TryCreateSubDictionary(oSBySGTByTE, tSG, typeof(Type));

                                            CreateAndOrAddToKeyListPair(oSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            TryCreateSubDictionary(tSBySGTByT, tSG, typeof(Type));
                                            TryCreateSubDictionary(tSBySGTByTE, tSG, typeof(Type));

                                            CreateAndOrAddToKeyListPair(tSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                TryCreateSubDictionary(cSBySGTByT, tSG, typeof(Type));
                                                TryCreateSubDictionary(cSBySGTByTE, tSG, typeof(Type));

                                                CreateAndOrAddToKeyListPair(cSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    // Fires each iteration, but only for first iteration of social group type loop.
                                    // Iterates location type and settlement type.
                                    if (isFirstSGTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(sByLTByT[tL] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSByLTByT[tL] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSByLTByT[tL] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSByLTByT[tL] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    // Fires each iteration, but only for intitial iterations of location type loop.
                                    // Iterates social group type and settlement type.
                                    if (isInitialLTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(sBySGTByT[tSG] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSBySGTByT[tSG] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSBySGTByT[tSG] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSBySGTByT[tSG] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    // Fires once each iteration.
                                    // Iterates social group type and location type.
                                    if (isInitialSTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(sBySGT, tSG, typeof(Settlement), s);

                                        TryCreateSubDictionary(sByLTByTE, tL, typeof(Type));

                                        CreateAndOrAddToKeyListPair(sByLTByTE[tL] as IDictionary, tS, tS, s);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSBySGT, tSG, typeof(Settlement), s);

                                            TryCreateSubDictionary(oSByLTByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(oSByLTByTE[tL] as IDictionary, tS, tS, s);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSBySGT, tSG, typeof(Settlement), s);

                                            TryCreateSubDictionary(tSByLTByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(tSByLTByTE[tL] as IDictionary, tS, tS, s);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSBySGT, tSG, typeof(Settlement), s);

                                                TryCreateSubDictionary(cSByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(cSByLTByTE[tL] as IDictionary, tS, tS, s);
                                            }
                                        }
                                    }

                                    //Console.WriteLine("CommunityLib: Performing specific settlement tests.");
                                    if (tS == typeof(Set_City))
                                    {
                                        //Console.WriteLine("CommunityLib: Sorting 'Set_City' by level.");
                                        Set_City city = s as Set_City;
                                        int level = city.getLevel();

                                        if (isFirstSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Is first iteration of settlement type loop.");
                                            if (cityByL[level] == null)
                                            {
                                                cityByL[level] = new List<Set_City>();
                                            }
                                            cityByL[level].Add(city);

                                            if (!cityBySGByL.Contains(sG))
                                            {
                                                cityBySGByL.Add(sG, new List<Set_City>[6]);
                                            }
                                            else if (cityBySGByL[sG] == null)
                                            {
                                                cityBySGByL[sG] = new List<Set_City>[6];
                                            }
                                            List<Set_City>[] array = cityBySGByL[sG] as List<Set_City>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_City>();
                                            }
                                            array[level].Add(city);

                                            if (!cityBySGTEByL.Contains(tSG))
                                            {
                                                cityBySGTEByL.Add(tSG, new List<Set_City>[6]);
                                            }
                                            else if (cityBySGTEByL[tSG] == null)
                                            {
                                                cityBySGTEByL[tSG] = new List<Set_City>[6];
                                            }
                                            array = cityBySGTEByL[tSG] as List<Set_City>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_City>();
                                            }
                                            array[level].Add(city);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (oCityByL[level] == null)
                                                {
                                                    oCityByL[level] = new List<Set_City>();
                                                }
                                                oCityByL[level].Add(city);

                                                if (!oCityBySGByL.Contains(sG))
                                                {
                                                    oCityBySGByL.Add(sG, new List<Set_City>[6]);
                                                }
                                                else if (oCityBySGByL[sG] == null)
                                                {
                                                    oCityBySGByL[sG] = new List<Set_City>[6];
                                                }
                                                array = oCityBySGByL[sG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);

                                                if (!oCityBySGTEByL.Contains(tSG))
                                                {
                                                    oCityBySGTEByL.Add(tSG, new List<Set_City>[6]);
                                                }
                                                else if (oCityBySGTEByL[tSG] == null)
                                                {
                                                    oCityBySGTEByL[tSG] = new List<Set_City>[6];
                                                }
                                                array = oCityBySGTEByL[tSG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (tCityByL[level] == null)
                                                {
                                                    tCityByL[level] = new List<Set_City>();
                                                }
                                                tCityByL[level].Add(city);

                                                if (!tCityBySGByL.Contains(sG))
                                                {
                                                    tCityBySGByL.Add(sG, new List<Set_City>[6]);
                                                }
                                                else if (tCityBySGByL[sG] == null)
                                                {
                                                    tCityBySGByL[sG] = new List<Set_City>[6];
                                                }
                                                array = tCityBySGByL[sG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);

                                                if (!tCityBySGTEByL.Contains(tSG))
                                                {
                                                    tCityBySGTEByL.Add(tSG, new List<Set_City>[6]);
                                                }
                                                else if (tCityBySGTEByL[tSG] == null)
                                                {
                                                    tCityBySGTEByL[tSG] = new List<Set_City>[6];
                                                }
                                                array = tCityBySGTEByL[tSG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);
                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (cCityByL[level] == null)
                                                    {
                                                        cCityByL[level] = new List<Set_City>();
                                                    }
                                                    cCityByL[level].Add(city);

                                                    if (!cCityBySGByL.Contains(sG))
                                                    {
                                                        cCityBySGByL.Add(sG, new List<Set_City>[6]);
                                                    }
                                                    else if (cCityBySGByL[sG] == null)
                                                    {
                                                        cCityBySGByL[sG] = new List<Set_City>[6];
                                                    }
                                                    array = cCityBySGByL[sG] as List<Set_City>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_City>();
                                                    }
                                                    array[level].Add(city);

                                                    if (!cCityBySGTEByL.Contains(tSG))
                                                    {
                                                        cCityBySGTEByL.Add(tSG, new List<Set_City>[6]);
                                                    }
                                                    else if (cCityBySGTEByL[tSG] == null)
                                                    {
                                                        cCityBySGTEByL[tSG] = new List<Set_City>[6];
                                                    }
                                                    array = cCityBySGTEByL[tSG] as List<Set_City>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_City>();
                                                    }
                                                    array[level].Add(city);
                                                }
                                            }
                                        }

                                        // Fire once each iteration of social group type loop.
                                        // Iterates social group type.
                                        if (isInitialLTIteration && isInitialSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Iterating social group type " + tSG.Name + ".");
                                            if (!cityBySGTByL.Contains(tSG))
                                            {
                                                cityBySGTByL.Add(tSG, new List<Set_City>[6]);
                                            }
                                            else if (cityBySGTByL[tSG] == null)
                                            {
                                                cityBySGTByL[tSG] = new List<Set_City>[6];
                                            }
                                            List<Set_City>[] array = cityBySGTByL[tSG] as List<Set_City>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_City>();
                                            }
                                            array[level].Add(city);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (!oCityBySGTByL.Contains(tSG))
                                                {
                                                    oCityBySGTByL.Add(tSG, new List<Set_City>[6]);
                                                }
                                                else if (oCityBySGTByL[tSG] == null)
                                                {
                                                    oCityBySGTByL[tSG] = new List<Set_City>[6];
                                                }
                                                array = oCityBySGTByL[tSG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (!tCityBySGTByL.Contains(tSG))
                                                {
                                                    tCityBySGTByL.Add(tSG, new List<Set_City>[6]);
                                                }
                                                else if (tCityBySGTByL[tSG] == null)
                                                {
                                                    tCityBySGTByL[tSG] = new List<Set_City>[6];
                                                }
                                                array = tCityBySGTByL[tSG] as List<Set_City>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_City>();
                                                }
                                                array[level].Add(city);

                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (!cCityBySGTByL.Contains(tSG))
                                                    {
                                                        cCityBySGTByL.Add(tSG, new List<Set_City>[6]);
                                                    }
                                                    else if (cCityBySGTByL[tSG] == null)
                                                    {
                                                        cCityBySGTByL[tSG] = new List<Set_City>[6];
                                                    }
                                                    array = cCityBySGTByL[tSG] as List<Set_City>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_City>();
                                                    }
                                                    array[level].Add(city);
                                                }
                                            }
                                        }
                                    }
                                    else if (tS == typeof(Set_MinorHuman))
                                    {
                                        //Console.WriteLine("CommunityLib: Sorting 'Set_MinorHuman' by level.");
                                        Set_MinorHuman mHuman = (s as Set_MinorHuman);
                                        int level = mHuman.getLevel();

                                        if (isFirstSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Is first iteration of settlement type loop.");
                                            if (minorHumanByL[level] == null)
                                            {
                                                minorHumanByL[level] = new List<Set_MinorHuman>();
                                            }
                                            minorHumanByL[level].Add(mHuman);

                                            if (!minorHumanBySGByL.Contains(sG))
                                            {
                                                minorHumanBySGByL.Add(sG, new List<Set_MinorHuman>[6]);
                                            }
                                            else if (minorHumanBySGByL[sG] == null)
                                            {
                                                minorHumanBySGByL[sG] = new List<Set_MinorHuman>[6];
                                            }
                                            List<Set_MinorHuman>[] array = minorHumanBySGByL[sG] as List<Set_MinorHuman>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_MinorHuman>();
                                            }
                                            array[level].Add(mHuman);

                                            if (!minorHumanBySGTEByL.Contains(tSG))
                                            {
                                                minorHumanBySGTEByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                            }
                                            else if (minorHumanBySGTEByL[tSG] == null)
                                            {
                                                minorHumanBySGTEByL[tSG] = new List<Set_MinorHuman>[6];
                                            }
                                            array = minorHumanBySGTEByL[tSG] as List<Set_MinorHuman>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_MinorHuman>();
                                            }
                                            array[level].Add(mHuman);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (oMinorHumanByL[level] == null)
                                                {
                                                    oMinorHumanByL[level] = new List<Set_MinorHuman>();
                                                }
                                                oMinorHumanByL[level].Add(mHuman);

                                                if (!oMinorHumanBySGByL.Contains(sG))
                                                {
                                                    oMinorHumanBySGByL.Add(sG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (oMinorHumanBySGByL[sG] == null)
                                                {
                                                    oMinorHumanBySGByL[sG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = oMinorHumanBySGByL[sG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);

                                                if (!oMinorHumanBySGTEByL.Contains(tSG))
                                                {
                                                    oMinorHumanBySGTEByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (oMinorHumanBySGTEByL[tSG] == null)
                                                {
                                                    oMinorHumanBySGTEByL[tSG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = oMinorHumanBySGTEByL[tSG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (tMinorHumanByL[level] == null)
                                                {
                                                    tMinorHumanByL[level] = new List<Set_MinorHuman>();
                                                }
                                                tMinorHumanByL[level].Add(mHuman);

                                                if (!tMinorHumanBySGByL.Contains(sG))
                                                {
                                                    tMinorHumanBySGByL.Add(sG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (tMinorHumanBySGByL[sG] == null)
                                                {
                                                    tMinorHumanBySGByL[sG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = tMinorHumanBySGByL[sG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);

                                                if (!tMinorHumanBySGTEByL.Contains(tSG))
                                                {
                                                    tMinorHumanBySGTEByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (tMinorHumanBySGTEByL[tSG] == null)
                                                {
                                                    tMinorHumanBySGTEByL[tSG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = tMinorHumanBySGTEByL[tSG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);

                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (cMinorHumanByL[level] == null)
                                                    {
                                                        cMinorHumanByL[level] = new List<Set_MinorHuman>();
                                                    }
                                                    cMinorHumanByL[level].Add(mHuman);

                                                    if (!cMinorHumanBySGByL.Contains(sG))
                                                    {
                                                        cMinorHumanBySGByL.Add(sG, new List<Set_MinorHuman>[6]);
                                                    }
                                                    else if (cMinorHumanBySGByL[sG] == null)
                                                    {
                                                        cMinorHumanBySGByL[sG] = new List<Set_MinorHuman>[6];
                                                    }
                                                    array = cMinorHumanBySGByL[sG] as List<Set_MinorHuman>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_MinorHuman>();
                                                    }
                                                    array[level].Add(mHuman);

                                                    if (!cMinorHumanBySGTEByL.Contains(tSG))
                                                    {
                                                        cMinorHumanBySGTEByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                    }
                                                    else if (cMinorHumanBySGTEByL[tSG] == null)
                                                    {
                                                        cMinorHumanBySGTEByL[tSG] = new List<Set_MinorHuman>[6];
                                                    }
                                                    array = cMinorHumanBySGTEByL[tSG] as List<Set_MinorHuman>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_MinorHuman>();
                                                    }
                                                    array[level].Add(mHuman);
                                                }
                                            }
                                        }

                                        // Fire once each iteration of social group type loop.
                                        // Iterates social group type.
                                        if (isInitialLTIteration && isInitialSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Iterating social group type to " + tSG.Name + ".");
                                            if (!minorHumanBySGTByL.Contains(tSG))
                                            {
                                                minorHumanBySGTByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                            }
                                            else if (minorHumanBySGTByL[tSG] == null)
                                            {
                                                minorHumanBySGTByL[tSG] = new List<Set_MinorHuman>[6];
                                            }
                                            List<Set_MinorHuman>[] array = minorHumanBySGTByL[tSG] as List<Set_MinorHuman>[];
                                            if (array[level] == null)
                                            {
                                                array[level] = new List<Set_MinorHuman>();
                                            }
                                            array[level].Add(mHuman);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (!oMinorHumanBySGTByL.Contains(tSG))
                                                {
                                                    oMinorHumanBySGTByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (oMinorHumanBySGTByL[tSG] == null)
                                                {
                                                    oMinorHumanBySGTByL[tSG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = oMinorHumanBySGTByL[sG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (!tMinorHumanBySGTByL.Contains(tSG))
                                                {
                                                    tMinorHumanBySGTByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                }
                                                else if (tMinorHumanBySGTByL[tSG] == null)
                                                {
                                                    tMinorHumanBySGTByL[tSG] = new List<Set_MinorHuman>[6];
                                                }
                                                array = tMinorHumanBySGTByL[tSG] as List<Set_MinorHuman>[];
                                                if (array[level] == null)
                                                {
                                                    array[level] = new List<Set_MinorHuman>();
                                                }
                                                array[level].Add(mHuman);

                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (!cMinorHumanBySGTByL.Contains(tSG))
                                                    {
                                                        cMinorHumanBySGTByL.Add(tSG, new List<Set_MinorHuman>[6]);
                                                    }
                                                    else if (cMinorHumanBySGTByL[tSG] == null)
                                                    {
                                                        cMinorHumanBySGTByL[tSG] = new List<Set_MinorHuman>[6];
                                                    }
                                                    array = cMinorHumanBySGTByL[tSG] as List<Set_MinorHuman>[];
                                                    if (array[level] == null)
                                                    {
                                                        array[level] = new List<Set_MinorHuman>();
                                                    }
                                                    array[level].Add(mHuman);
                                                }
                                            }
                                        }
                                    }
                                    else if (tS == typeof(Set_OrcCamp))
                                    {
                                        //Console.WriteLine("CommunityLib: Sorting 'Set_OrcCamp' by specialism.");
                                        Set_OrcCamp orcCamp = (s as Set_OrcCamp);
                                        int specialism = orcCamp.specialism;

                                        if (isFirstSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Is first iteration of settlement type.");
                                            if (orcCampByS[specialism] == null)
                                            {
                                                orcCampByS[specialism] = new List<Set_OrcCamp>();
                                            }
                                            orcCampByS[specialism].Add(orcCamp);

                                            if (!orcCampBySGByS.Contains(sG))
                                            {
                                                orcCampBySGByS.Add(sG, new List<Set_OrcCamp>[6]);
                                            }
                                            else if (orcCampBySGByS[sG] == null)
                                            {
                                                orcCampBySGByS[sG] = new List<Set_OrcCamp>[6];
                                            }
                                            List<Set_OrcCamp>[] array = orcCampBySGByS[sG] as List<Set_OrcCamp>[];
                                            if (array[specialism] == null)
                                            {
                                                array[specialism] = new List<Set_OrcCamp>();
                                            }
                                            array[specialism].Add(orcCamp);

                                            if (!orcCampBySGTEByS.Contains(tSG))
                                            {
                                                orcCampBySGTEByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                            }
                                            else if (orcCampBySGTEByS[tSG] == null)
                                            {
                                                orcCampBySGTEByS[tSG] = new List<Set_OrcCamp>[6];
                                            }
                                            array = orcCampBySGTEByS[tSG] as List<Set_OrcCamp>[];
                                            if (array[specialism] == null)
                                            {
                                                array[specialism] = new List<Set_OrcCamp>();
                                            }
                                            array[specialism].Add(orcCamp);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (oOrcCampByS[specialism] == null)
                                                {
                                                    oOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                                }
                                                oOrcCampByS[specialism].Add(orcCamp);

                                                if (!oOrcCampBySGByS.Contains(sG))
                                                {
                                                    oOrcCampBySGByS.Add(sG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (oOrcCampBySGByS[sG] == null)
                                                {
                                                    oOrcCampBySGByS[sG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = oOrcCampBySGByS[sG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);

                                                if (!oOrcCampBySGTEByS.Contains(tSG))
                                                {
                                                    oOrcCampBySGTEByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (oOrcCampBySGTEByS[tSG] == null)
                                                {
                                                    oOrcCampBySGTEByS[tSG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = oOrcCampBySGTEByS[tSG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (tOrcCampByS[specialism] == null)
                                                {
                                                    tOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                                }
                                                tOrcCampByS[specialism].Add(orcCamp);

                                                if (!tOrcCampBySGByS.Contains(sG))
                                                {
                                                    tOrcCampBySGByS.Add(sG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (tOrcCampBySGByS[sG] == null)
                                                {
                                                    tOrcCampBySGByS[sG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = tOrcCampBySGByS[sG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);

                                                if (!tOrcCampBySGTEByS.Contains(tSG))
                                                {
                                                    tOrcCampBySGTEByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (tOrcCampBySGTEByS[tSG] == null)
                                                {
                                                    tOrcCampBySGTEByS[tSG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = tOrcCampBySGTEByS[tSG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);
                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (cOrcCampByS[specialism] == null)
                                                    {
                                                        cOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                                    }
                                                    cOrcCampByS[specialism].Add(orcCamp);

                                                    if (!cOrcCampBySGByS.Contains(sG))
                                                    {
                                                        cOrcCampBySGByS.Add(sG, new List<Set_OrcCamp>[6]);
                                                    }
                                                    else if (cOrcCampBySGByS[sG] == null)
                                                    {
                                                        cOrcCampBySGByS[sG] = new List<Set_OrcCamp>[6];
                                                    }
                                                    array = cOrcCampBySGByS[sG] as List<Set_OrcCamp>[];
                                                    if (array[specialism] == null)
                                                    {
                                                        array[specialism] = new List<Set_OrcCamp>();
                                                    }
                                                    array[specialism].Add(orcCamp);

                                                    if (!cOrcCampBySGTEByS.Contains(tSG))
                                                    {
                                                        cOrcCampBySGTEByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                    }
                                                    else if (cOrcCampBySGTEByS[tSG] == null)
                                                    {
                                                        cOrcCampBySGTEByS[tSG] = new List<Set_OrcCamp>[6];
                                                    }
                                                    array = cOrcCampBySGTEByS[tSG] as List<Set_OrcCamp>[];
                                                    if (array[specialism] == null)
                                                    {
                                                        array[specialism] = new List<Set_OrcCamp>();
                                                    }
                                                    array[specialism].Add(orcCamp);
                                                }
                                            }
                                        }

                                        // Fire once each iteration of social group type loop.
                                        // Iterates social group type.
                                        if (isInitialLTIteration && isInitialSTIteration)
                                        {
                                            //Console.WriteLine("CommunityLib: Iterating social group type to " + tSG.Name + ".");
                                            if (!orcCampBySGTByS.Contains(tSG))
                                            {
                                                orcCampBySGTByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                            }
                                            else if (orcCampBySGTByS[tSG] == null)
                                            {
                                                orcCampBySGTByS[tSG] = new List<Set_OrcCamp>[6];
                                            }
                                            List<Set_OrcCamp>[] array = orcCampBySGTByS[tSG] as List<Set_OrcCamp>[];
                                            if (array[specialism] == null)
                                            {
                                                array[specialism] = new List<Set_OrcCamp>();
                                            }
                                            array[specialism].Add(orcCamp);

                                            if (l.isOcean)
                                            {
                                                //Console.WriteLine("CommunityLib: Is ocean.");
                                                if (!oOrcCampBySGTByS.Contains(tSG))
                                                {
                                                    oOrcCampBySGTByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (oOrcCampBySGTByS[tSG] == null)
                                                {
                                                    oOrcCampBySGTByS[tSG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = oOrcCampBySGTByS[tSG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);
                                            }
                                            else
                                            {
                                                //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                if (!tOrcCampBySGTByS.Contains(tSG))
                                                {
                                                    tOrcCampBySGTByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                }
                                                else if (tOrcCampBySGTByS[tSG] == null)
                                                {
                                                    tOrcCampBySGTByS[tSG] = new List<Set_OrcCamp>[6];
                                                }
                                                array = tOrcCampBySGTByS[tSG] as List<Set_OrcCamp>[];
                                                if (array[specialism] == null)
                                                {
                                                    array[specialism] = new List<Set_OrcCamp>();
                                                }
                                                array[specialism].Add(orcCamp);

                                                if (l.isCoastal)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is coastal.");
                                                    if (!cOrcCampBySGTByS.Contains(tSG))
                                                    {
                                                        cOrcCampBySGTByS.Add(tSG, new List<Set_OrcCamp>[6]);
                                                    }
                                                    else if (cOrcCampBySGTByS[tSG] == null)
                                                    {
                                                        cOrcCampBySGTByS[tSG] = new List<Set_OrcCamp>[6];
                                                    }
                                                    array = cOrcCampBySGTByS[tSG] as List<Set_OrcCamp>[];
                                                    if (array[specialism] == null)
                                                    {
                                                        array[specialism] = new List<Set_OrcCamp>();
                                                    }
                                                    array[specialism].Add(orcCamp);
                                                }
                                            }
                                        }
                                    }

                                    // Subsettlements with Social Group
                                    if (s.subs != null && s.subs.Count > 0)
                                    {
                                        //Console.WriteLine("CommunityLib: Starting Subsettlement Loop");
                                        foreach (Subsettlement sub in s.subs)
                                        {
                                            tSub = sub.GetType();
                                            iterateSsT = true;
                                            isInitialSsTIteration = true;

                                            //Console.WriteLine("CommunityLib: Starting subsettlement loop for Subsettlement " + sub.getName() + " of type " + tSub.Name + ".");

                                            while (iterateSsT)
                                            {
                                                isFirstSsTIteration = isFirstSGTIteration && isFirstLTIteration && isFirstSTIteration && isInitialSsTIteration;

                                                // Very first instance. Fires only once.
                                                // Iterates nothing.
                                                if (isFirstSsTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Is first iteration of subsettlement type.");
                                                    subsettlements.Add(sub);
                                                    CreateAndOrAddToKeyListPair(ssByTE, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssByLTE, tL, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(ssBySTE, tS, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(ssBySG, sG, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(ssBySGTE, tSG, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(ssByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(ssByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(ssBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(ssBySTEByTE, tS, typeof(Type));
                                                    TryCreateSubDictionary(ssBySGByT, sG, typeof(Type));
                                                    TryCreateSubDictionary(ssBySGTEByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(ssBySGTEByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(ssByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        oceanSubsettlements.Add(sub);
                                                        CreateAndOrAddToKeyListPair(oSsByTE, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsByLTE, tL, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySTE, tS, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySG, sG, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySGTE, tSG, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(oSsByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(oSsByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySTEByTE, tS, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySGByT, sG, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySGTEByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySGTEByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        terrestrialSubsettlements.Add(sub);
                                                        CreateAndOrAddToKeyListPair(tSsByTE, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsByLTE, tL, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySTE, tS, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySG, sG, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySGTE, tSG, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(tSsByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(tSsByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySTEByTE, tS, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySGByT, sG, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySGTEByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySGTEByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            coastalSubsettlements.Add(sub);
                                                            CreateAndOrAddToKeyListPair(cSsByTE, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsByLTE, tL, typeof(Subsettlement), sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySTE, tS, typeof(Subsettlement), sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySG, sG, typeof(Subsettlement), sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySGTE, tSG, typeof(Subsettlement), sub);

                                                            TryCreateSubDictionary(cSsByLTEByT, tL, typeof(Type));
                                                            TryCreateSubDictionary(cSsByLTEByTE, tL, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySTEByT, tS, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySTEByTE, tS, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySGByT, sG, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySGTEByT, tSG, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySGTEByTE, tSG, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for intial iterations of settlement type and location type loops.
                                                // Iterates social group type.
                                                if (isInitialLTIteration && isInitialSTIteration && isInitialSsTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating social group type.");
                                                    CreateAndOrAddToKeyListPair(ssBySGT, tSG, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(ssBySGTByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(ssBySGTByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(ssBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsBySGT, tSG, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(oSsBySGTByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySGTByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsBySGT, tSG, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(tSsBySGTByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySGTByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsBySGT, tSG, typeof(Subsettlement), sub);

                                                            TryCreateSubDictionary(cSsBySGTByT, tSG, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySGTByTE, tSG, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for initial iterations of settlement type, in first iteration of social group type loop.
                                                // Iterates location type.
                                                if (isFirstSGTIteration && isInitialSTIteration && isInitialSsTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating location type.");
                                                    CreateAndOrAddToKeyListPair(ssByLT, tL, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(ssByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(ssByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(ssByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsByLT, tL, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(oSsByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(oSsByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsByLT, tL, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(tSsByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(tSsByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsByLT, tL, typeof(Subsettlement), sub);

                                                            TryCreateSubDictionary(cSsByLTByT, tL, typeof(Type));
                                                            TryCreateSubDictionary(cSsByLTByTE, tL, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for first iteration of location type and social group type loops.
                                                // Iterates settlement type.
                                                if (isFirstSGTIteration && isFirstLTIteration && isInitialSsTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating settlement type.");
                                                    CreateAndOrAddToKeyListPair(ssByST, tS, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(ssBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(ssBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(ssBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsByST, tS, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(oSsBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(oSsBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsByST, tS, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(tSsBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(tSsBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsByST, tS, typeof(Subsettlement), sub);

                                                            TryCreateSubDictionary(cSsBySTByT, tS, typeof(Type));
                                                            TryCreateSubDictionary(cSsBySTByTE, tS, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for first iteration of settlement type, location type, and social group type loops.
                                                // Iterates subsettlement type.
                                                if (isFirstSGTIteration && isFirstLTIteration && isFirstSTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating subsettlement type.");
                                                    CreateAndOrAddToKeyListPair(ssByT, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssBySGByT[sG] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(ssBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsByT, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySGByT[sG] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(oSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsByT, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySGByT[sG] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(tSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsByT, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySGByT[sG] as IDictionary, tSub, tSub, sub);
                                                            CreateAndOrAddToKeyListPair(cSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for initial iterations of location type and settlement type loops.
                                                // Iterates social group type and subsettlement type.
                                                if (isInitialLTIteration && isInitialSTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating social group type and subsettlement type.");
                                                    CreateAndOrAddToKeyListPair(ssBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for initial iterations of settlement type loop, of first iteration of social group type loop.
                                                // Iterates location type and subsettlement type.
                                                if (isFirstSGTIteration && isInitialSTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating location type and subsettlement type.");
                                                    CreateAndOrAddToKeyListPair(ssByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for first iteration of location type and social group type loops.
                                                // Iterates settlement type and subsettlement type.
                                                if (isFirstSGTIteration && isFirstLTIteration)
                                                {
                                                    //Console.WriteLine("CommunityLib: Iterating settlement type and subsettlement type.");
                                                    CreateAndOrAddToKeyListPair(ssBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                    if (l.isOcean)
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is ocean.");
                                                        CreateAndOrAddToKeyListPair(oSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                    }
                                                    else
                                                    {
                                                        //Console.WriteLine("CommunityLib: Is terrestrial.");
                                                        CreateAndOrAddToKeyListPair(tSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                        if (l.isCoastal)
                                                        {
                                                            //Console.WriteLine("CommunityLib: Is coastal.");
                                                            CreateAndOrAddToKeyListPair(cSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                        }
                                                    }
                                                }

                                                if (tSub == targetTSub)
                                                {
                                                    iterateSsT = false;
                                                    //Console.WriteLine("CommunityLib: End subsettlement type loop.");
                                                }
                                                else
                                                {
                                                    isInitialSsTIteration = false;
                                                    tSub = tSub.BaseType;
                                                    //Console.WriteLine("CommunityLib: Iterating Type to " + tSub.Name + ".");
                                                }
                                            }
                                        }
                                    }

                                    // Properties with Social Group and Settlement
                                    if (l.properties != null && l.properties.Count > 0)
                                    {
                                        //Console.WriteLine("CommunityLib: Starting Properties Loop");
                                        foreach (Property p in l.properties)
                                        {
                                            tP = p.GetType();
                                            iteratePT = true;
                                            isInitialPTIteration = true;

                                            //Console.WriteLine("CommunityLib: Starting property loop for Property " + p.getName() + " of type " + tP.Name + ".");

                                            while (iteratePT)
                                            {
                                                isFirstPTIteration = isFirstSGTIteration && isFirstLTIteration && isFirstSTIteration && isInitialPTIteration;

                                                // Very first instance. Fires only once.
                                                // Iterates nothing.
                                                if (isFirstLTIteration)
                                                {
                                                    properties.Add(p);
                                                    CreateAndOrAddToKeyListPair(pByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(pBySTE, tS, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(pBySG, sG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(pBySGTE, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(pByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(pByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(pBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(pBySTEByTE, tS, typeof(Type));
                                                    TryCreateSubDictionary(pBySGByT, sG, typeof(Type));
                                                    TryCreateSubDictionary(pBySGTEByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(pBySGTEByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(pByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        oceanProperties.Add(p);
                                                        CreateAndOrAddToKeyListPair(oPByTE, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPByLTE, tL, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(oPBySTE, tS, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(oPBySG, sG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(oPBySGTE, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(oPByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(oPByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(oPBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(oPBySTEByTE, tS, typeof(Type));
                                                        TryCreateSubDictionary(oPBySGByT, sG, typeof(Type));
                                                        TryCreateSubDictionary(oPBySGTEByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(oPBySGTEByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        terrestrialProperties.Add(p);
                                                        CreateAndOrAddToKeyListPair(tPByTE, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPByLTE, tL, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(tPBySTE, tS, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(tPBySG, sG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(tPBySGTE, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(tPByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(tPByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(tPBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(tPBySTEByTE, tS, typeof(Type));
                                                        TryCreateSubDictionary(tPBySGByT, sG, typeof(Type));
                                                        TryCreateSubDictionary(tPBySGTEByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(tPBySGTEByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            coastalProperties.Add(p);
                                                            CreateAndOrAddToKeyListPair(cPByTE, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPByLTE, tL, typeof(Property), p);
                                                            CreateAndOrAddToKeyListPair(cPBySTE, tS, typeof(Property), p);
                                                            CreateAndOrAddToKeyListPair(cPBySG, sG, typeof(Property), p);
                                                            CreateAndOrAddToKeyListPair(cPBySGTE, tSG, typeof(Property), p);

                                                            TryCreateSubDictionary(cPByLTEByT, tL, typeof(Type));
                                                            TryCreateSubDictionary(cPByLTEByTE, tL, typeof(Type));
                                                            TryCreateSubDictionary(cPBySTEByT, tS, typeof(Type));
                                                            TryCreateSubDictionary(cPBySTEByTE, tS, typeof(Type));
                                                            TryCreateSubDictionary(cPBySGByT, sG, typeof(Type));
                                                            TryCreateSubDictionary(cPBySGTEByT, tSG, typeof(Type));
                                                            TryCreateSubDictionary(cPBySGTEByTE, tSG, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for intial iterations of settlement type and location type loops.
                                                // Iterates social group type.
                                                if (isInitialLTIteration && isInitialSTIteration && isInitialPTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pBySGT, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(pBySGTByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(pBySGTByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(pBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPBySGT, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(oPBySGTByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(oPBySGTByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPBySGT, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(tPBySGTByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(tPBySGTByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPBySGT, tSG, typeof(Property), p);

                                                            TryCreateSubDictionary(cPBySGTByT, tSG, typeof(Type));
                                                            TryCreateSubDictionary(cPBySGTByTE, tSG, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for initial iteration of settlement type loop, for first iteration of social group type loop.
                                                // Iterates location type.
                                                if (isFirstSGTIteration && isInitialSTIteration && isInitialPTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(pByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(pByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(pByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPByLT, tL, typeof(Property), p);

                                                        TryCreateSubDictionary(oPByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(oPByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPByLT, tL, typeof(Property), p);

                                                        TryCreateSubDictionary(tPByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(tPByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPByLT, tL, typeof(Property), p);

                                                            TryCreateSubDictionary(cPByLTByT, tL, typeof(Type));
                                                            TryCreateSubDictionary(cPByLTByTE, tL, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires once each iteration, but only for first iteration of location type and social group type loops.
                                                // Iterates settlement type.
                                                if (isFirstSGTIteration && isFirstLTIteration && isInitialPTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pByST, tS, typeof(Property), p);

                                                    TryCreateSubDictionary(pBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(pBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(pBySTByTE[tS] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPByST, tS, typeof(Property), p);

                                                        TryCreateSubDictionary(oPBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(oPBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(oPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPByST, tS, typeof(Property), p);

                                                        TryCreateSubDictionary(tPBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(tPBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(tPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPByST, tS, typeof(Property), p);

                                                            TryCreateSubDictionary(cPBySTByT, tS, typeof(Type));
                                                            TryCreateSubDictionary(cPBySTByTE, tS, typeof(Type));

                                                            CreateAndOrAddToKeyListPair(cPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for first iteration of settlement type, location type and social group type loops.
                                                // Iterates property type.
                                                if (isFirstSGTIteration && isFirstLTIteration && isFirstSTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pBySTEByT[tS] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pBySGByT[sG] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(pBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPByT, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPBySGByT[sG] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(oPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPBySGByT[sG] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(tPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPBySGByT[sG] as IDictionary, tP, tP, p);
                                                            CreateAndOrAddToKeyListPair(cPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for initial iterations of location type and settlement type loops.
                                                // Iterates social group type and property type.
                                                if (isInitialLTIteration && isInitialSTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for initial iterations of settlement type loop, of first iteration of social group type loop.
                                                // Iterates location type and property type.
                                                if (isFirstSGTIteration && isInitialSTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pByLTByT[tL] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPByLTByT[tL] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPByLTByT[tL] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPByLTByT[tL] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                // Fires each iteration, but only for first iteration of location type and social group type loops.
                                                // Iterates settlement type and property type.
                                                if (isFirstSGTIteration && isFirstLTIteration)
                                                {
                                                    CreateAndOrAddToKeyListPair(pBySTByT[tS] as IDictionary, tP, tP, p);
                                                    if (l.isOcean)
                                                    {
                                                        CreateAndOrAddToKeyListPair(oPBySTByT[tS] as IDictionary, tP, tP, p);
                                                    }
                                                    else
                                                    {
                                                        CreateAndOrAddToKeyListPair(tPBySTByT[tS] as IDictionary, tP, tP, p);
                                                        if (l.isCoastal)
                                                        {
                                                            CreateAndOrAddToKeyListPair(cPBySTByT[tS] as IDictionary, tP, tP, p);
                                                        }
                                                    }
                                                }

                                                if (tP == targetTP)
                                                {
                                                    iteratePT = false;
                                                    //Console.WriteLine("CommunityLib: End property type loop.");
                                                }
                                                else
                                                {
                                                    isInitialPTIteration = false;
                                                    tP = tP.BaseType;
                                                    //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name + ".");
                                                }
                                            }
                                        }
                                    }

                                    if (tS == targetTS)
                                    {
                                        iterateST = false;
                                        //Console.WriteLine("CommunityLib: End settlement type loop.");
                                    }
                                    else
                                    {
                                        isFirstSTIteration = false;
                                        isInitialSTIteration = false;
                                        tS = tS.BaseType;
                                        //Console.WriteLine("CommunityLib: Iterating Type to " + tS.Name + ".");
                                    }
                                }
                            }
                            else
                            {
                                //Console.WriteLine("CommunityLib: Location does not have settlement.");
                                if (isFirstLTIteration)
                                {
                                    //Console.WriteLine("CommunityLib: Is first iteration of location type loop.");
                                    CreateAndOrAddToKeyListPair(lWoSByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(lWoSBySG, sG, typeof(Location), l);
                                    CreateAndOrAddToKeyListPair(lWoSBySGTE, tSG, typeof(Location), l);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oLWoSByTE, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(oLWoSBySG, sG, typeof(Location), l);
                                        CreateAndOrAddToKeyListPair(oLWoSBySGTE, tSG, typeof(Location), l);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tLWoSByTE, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(tLWoSBySG, sG, typeof(Location), l);
                                        CreateAndOrAddToKeyListPair(tLWoSBySGTE, tSG, typeof(Location), l);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cLWoSByTE, tL, tL, l);
                                            CreateAndOrAddToKeyListPair(cLWoSBySG, sG, typeof(Location), l);
                                            CreateAndOrAddToKeyListPair(cLWoSBySGTE, tSG, typeof(Location), l);
                                        }
                                    }
                                }

                                // Iterates social group type.
                                if (isInitialLTIteration)
                                {
                                    //Console.WriteLine("CommunityLib: Is initial iteration of location type loop.");
                                    CreateAndOrAddToKeyListPair(lWoSBySGT, tSG, typeof(Location), l);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oLWoSBySGT, tSG, typeof(Location), l);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tLWoSBySGT, tSG, typeof(Location), l);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cLWoSBySGT, tSG, typeof(Location), l);
                                        }
                                    }
                                }

                                // Iterates Location Type
                                if (isFirstSGTIteration)
                                {
                                    //Console.WriteLine("CommunityLib: Is first iteration of social group type loop.");
                                    CreateAndOrAddToKeyListPair(lWoSByT, tL, tL, l);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oLWoSByT, tL, tL, l);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tLWoSByT, tL, tL, l);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cLWoSByT, tL, tL, l);
                                        }
                                    }
                                }

                                // Properties without Settlement
                                if (l.properties != null && l.properties.Count > 0)
                                {
                                    //Console.WriteLine("CommunityLib: Starting Properties Loop");
                                    foreach (Property p in l.properties)
                                    {
                                        tP = p.GetType();
                                        iteratePT = true;
                                        isInitialPTIteration = true;

                                        //Console.WriteLine("CommunityLib: Starting property loop for Property " + p.getName() + " of type " + tP.Name + ".");

                                        while (iteratePT)
                                        {
                                            isFirstPTIteration = isFirstSGTIteration && isFirstLTIteration && isInitialPTIteration;

                                            // Very first instance. Fires only once.
                                            // Iterates nothing.
                                            if (isFirstLTIteration)
                                            {
                                                properties.Add(p);
                                                CreateAndOrAddToKeyListPair(pByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pByLTE, tL, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pBySG, sG, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pBySGTE, tSG, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pWoSByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pWoSBySG, sG, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pWoSBySGTE, tSG, typeof(Property), p);

                                                TryCreateSubDictionary(pByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(pByLTEByTE, tL, typeof(Type));
                                                TryCreateSubDictionary(pBySGByT, sG, typeof(Type));
                                                TryCreateSubDictionary(pBySGTEByT, tSG, typeof(Type));
                                                TryCreateSubDictionary(pBySGTEByTE, tSG, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    oceanProperties.Add(p);
                                                    CreateAndOrAddToKeyListPair(oPByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPBySG, sG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPBySGTE, tSG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPWoSByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPWoSBySG, sG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPWoSBySGTE, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(oPByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPBySGByT, sG, typeof(Type));
                                                    TryCreateSubDictionary(oPBySGTEByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(oPBySGTEByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    terrestrialProperties.Add(p);
                                                    CreateAndOrAddToKeyListPair(tPByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPBySG, sG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPBySGTE, tSG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPWoSByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPWoSBySG, sG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPWoSBySGTE, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(tPByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPBySGByT, sG, typeof(Type));
                                                    TryCreateSubDictionary(tPBySGTEByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(tPBySGTEByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        coastalProperties.Add(p);
                                                        CreateAndOrAddToKeyListPair(cPByTE, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPByLTE, tL, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPBySG, sG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPBySGTE, tSG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPWoSByTE, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPWoSBySG, sG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPWoSBySGTE, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(cPByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPBySGByT, sG, typeof(Type));
                                                        TryCreateSubDictionary(cPBySGTEByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(cPBySGTEByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for intial iterations of settlement type and location type loops.
                                            // Iterates social group type.
                                            if (isInitialLTIteration && isInitialLTIteration && isInitialPTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pBySGT, tSG, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pWoSBySGT, tSG, typeof(Property), p);

                                                TryCreateSubDictionary(pBySGTByT, tSG, typeof(Type));
                                                TryCreateSubDictionary(pBySGTByTE, tSG, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPBySGT, tSG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPWoSBySGT, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(oPBySGTByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(oPBySGTByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPBySGT, tSG, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPWoSBySGT, tSG, typeof(Property), p);

                                                    TryCreateSubDictionary(tPBySGTByT, tSG, typeof(Type));
                                                    TryCreateSubDictionary(tPBySGTByTE, tSG, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPBySGT, tSG, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPWoSBySGT, tSG, typeof(Property), p);

                                                        TryCreateSubDictionary(cPBySGTByT, tSG, typeof(Type));
                                                        TryCreateSubDictionary(cPBySGTByTE, tSG, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for first iteration of social group type loop.
                                            // Iterates location type.
                                            if (isFirstSGTIteration && isInitialPTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByLT, tL, typeof(Property), p);

                                                TryCreateSubDictionary(pByLTByT, tL, typeof(Type));
                                                TryCreateSubDictionary(pByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pByLTByTE[tL] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(oPByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(tPByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByLT, tL, typeof(Property), p);

                                                        TryCreateSubDictionary(cPByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for first iteration of location type and social group type loops.
                                            // Iterates property type.
                                            if (isFirstSGTIteration && isFirstLTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pByLTEByT[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pBySGByT[sG] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pWoSByT, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPBySGByT[sG] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPWoSByT, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPBySGByT[sG] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPWoSByT, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPBySGByT[sG] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPWoSByT, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for initial iterations of location type and settlement type loops.
                                            // Iterates social group type and property type.
                                            if (isInitialLTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPBySGTByT[tSG] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for initial iterations of settlement type loop, of first iteration of social group type loop.
                                            // Iterates location type and property type.
                                            if (isFirstSGTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByLTByT[tL] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByLTByT[tL] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByLTByT[tL] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByLTByT[tL] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            if (tP == targetTP)
                                            {
                                                iteratePT = false;
                                                //Console.WriteLine("CommunityLib: End property type loop.");
                                            }
                                            else
                                            {
                                                isInitialPTIteration = false;
                                                tP = tP.BaseType;
                                                //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name + ".");
                                            }
                                        }
                                    }
                                }
                            }

                            if (tL == targetTL)
                            {
                                iterateLT = false;
                                //Console.WriteLine("CommunityLib: End location type loop.");
                            }
                            else
                            {
                                isFirstLTIteration = false;
                                isInitialLTIteration = false;
                                tL = tL.BaseType;
                                //Console.WriteLine("CommunityLib: Iterating Type to " + tL.Name + ".");
                            }
                        }

                        if (tSG == targetTSG)
                        {
                            iterateSGT = false;
                            //Console.WriteLine("CommunityLib: End social group type loop.");
                        }
                        else
                        {
                            isFirstSGTIteration = false;
                            tSG = tSG.BaseType;
                            //Console.WriteLine("CommunityLib: Iterating Type to " + tSG.Name + ".");
                        }
                    }
                }
                else
                {
                    iterateLT = true;

                    //Console.WriteLine("CommunityLib: Starting location type loop.");
                    while (iterateLT)
                    {
                        //Console.WriteLine("CommunityLib: Location is of Type " + tL.Name + ".");

                        // Very first iteration. Only fires once.
                        if (isFirstLTIteration)
                        {
                            //Console.WriteLine("CommunityLib: Is first iteration of location type loop");
                            CreateAndOrAddToKeyListPair(lByTE, tL, tL, l);
                            CreateAndOrAddToKeyListPair(lWoSGByTE, tL, tL, l);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oLByTE, tL, tL, l);
                                CreateAndOrAddToKeyListPair(oLWoSGByTE, tL, tL, l);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tLByTE, tL, tL, l);
                                CreateAndOrAddToKeyListPair(tLWoSGByTE, tL, tL, l);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cLByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(cLWoSGByTE, tL, tL, l);
                                }
                            }
                        }

                        // Fires each iteration.
                        CreateAndOrAddToKeyListPair(lByT, tL, tL, l);
                        CreateAndOrAddToKeyListPair(lWoSGByT, tL, tL, l);
                        if (l.isOcean)
                        {
                            CreateAndOrAddToKeyListPair(oLByT, tL, tL, l);
                            CreateAndOrAddToKeyListPair(oLWoSGByT, tL, tL, l);
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(tLByT, tL, tL, l);
                            CreateAndOrAddToKeyListPair(tLWoSGByT, tL, tL, l);
                            if (l.isCoastal)
                            {
                                CreateAndOrAddToKeyListPair(cLByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(cLWoSGByT, tL, tL, l);
                            }
                        }

                        if (s != null)
                        {
                            tS = s.GetType();
                            iterateST = true;
                            isInitialSTIteration = true;

                            //Console.WriteLine("CommunityLib: Starting settlement type loop.");
                            while (iterateST)
                            {
                                //Console.WriteLine("CommunityLib: Settlement " + s.name + " is of Type " + tS.Name + ".");
                                // Very first instace. Fires once.
                                if (isFirstSTIteration)
                                {
                                    settlements.Add(s);
                                    CreateAndOrAddToKeyListPair(sByTE, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sWoSGByTE, tS, tS, s);

                                    TryCreateSubDictionary(sByLTEByT, tL, typeof(Type));
                                    TryCreateSubDictionary(sByLTEByTE, tL, typeof(Type));

                                    CreateAndOrAddToKeyListPair(sByLTEByTE[tL] as IDictionary, tS, tS, s);
                                    if (l.isOcean)
                                    {
                                        oceanSettlements.Add(s);
                                        CreateAndOrAddToKeyListPair(oSByTE, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(oSWoSGByTE, tS, tS, s);

                                        TryCreateSubDictionary(oSByLTEByT, tL, typeof(Type));
                                        TryCreateSubDictionary(oSByLTEByTE, tL, typeof(Type));

                                        CreateAndOrAddToKeyListPair(oSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                    }
                                    else
                                    {
                                        terrestrialSettlements.Add(s);
                                        CreateAndOrAddToKeyListPair(tSByTE, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(tSWoSGByTE, tS, tS, s);

                                        TryCreateSubDictionary(tSByLTEByT, tL, typeof(Type));
                                        TryCreateSubDictionary(tSByLTEByTE, tL, typeof(Type));

                                        CreateAndOrAddToKeyListPair(tSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                        if (l.isCoastal)
                                        {
                                            coastalSettlements.Add(s);
                                            CreateAndOrAddToKeyListPair(cSByTE, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(cSWoSGByTE, tS, tS, s);

                                            TryCreateSubDictionary(cSByLTEByT, tL, typeof(Type));
                                            TryCreateSubDictionary(cSByLTEByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(cSByLTEByTE[tL] as IDictionary, tS, tS, s);
                                        }
                                    }
                                }

                                // Fires once each iteration.
                                if (isInitialSTIteration)
                                {
                                    CreateAndOrAddToKeyListPair(sByLT, tL, typeof(Settlement), s);

                                    TryCreateSubDictionary(sByLTByT, tL, typeof(Type));
                                    TryCreateSubDictionary(sByLTByTE, tL, typeof(Type));

                                    CreateAndOrAddToKeyListPair(sByLTByTE[tL] as IDictionary, tS, tS, s);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oSByLT, tL, typeof(Settlement), s);

                                        TryCreateSubDictionary(oSByLTByT, tL, typeof(Type));
                                        TryCreateSubDictionary(oSByLTByTE, tL, typeof(Type));

                                        CreateAndOrAddToKeyListPair(oSByLTByTE[tL] as IDictionary, tS, tS, s);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tSByLT, tL, typeof(Settlement), s);

                                        TryCreateSubDictionary(tSByLTByT, tL, typeof(Type));
                                        TryCreateSubDictionary(tSByLTByTE, tL, typeof(Type));

                                        CreateAndOrAddToKeyListPair(tSByLTByTE[tL] as IDictionary, tS, tS, s);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cSByLT, tL, typeof(Settlement), s);

                                            TryCreateSubDictionary(cSByLTByT, tL, typeof(Type));
                                            TryCreateSubDictionary(cSByLTByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(cSByLTByTE[tL] as IDictionary, tS, tS, s);
                                        }
                                    }
                                }

                                // Fires each iteration, but only for first iteration of location type loop.
                                if (isFirstLTIteration)
                                {
                                    CreateAndOrAddToKeyListPair(sByT, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sByLTE, tL, typeof(Settlement), s);
                                    CreateAndOrAddToKeyListPair(sByLTEByT[tL] as IDictionary, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sWoSGByT, tS, tS, s);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oSByT, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(oSByLTE, tL, typeof(Settlement), s);
                                        CreateAndOrAddToKeyListPair(oSByLTEByT[tL] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(oSWoSGByT, tS, tS, s);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tSByT, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(tSByLTE, tL, typeof(Settlement), s);
                                        CreateAndOrAddToKeyListPair(tSByLTEByT[tL] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(tSWoSGByT, tS, tS, s);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cSByT, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(cSByLTE, tL, typeof(Settlement), s);
                                            CreateAndOrAddToKeyListPair(cSByLTEByT[tL] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(cSWoSGByT, tS, tS, s);
                                        }
                                    }
                                }

                                // Fire each iteration
                                CreateAndOrAddToKeyListPair(sByLTByT[tL] as IDictionary, tS, tS, s);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oSByLTByT[tL] as IDictionary, tS, tS, s);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tSByLTByT[tL] as IDictionary, tS, tS, s);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cSByLTByT[tL] as IDictionary, tS, tS, s);
                                    }
                                }

                                if (tS == typeof(Set_City))
                                {
                                    //Console.WriteLine("CommunityLib: Sorting 'Set_City' by level.");
                                    Set_City city = s as Set_City;
                                    int level = city.getLevel();

                                    if (isFirstSTIteration)
                                    {
                                        if (cityByL[level] == null)
                                        {
                                            cityByL[level] = new List<Set_City>();
                                        }
                                        cityByL[level].Add(city);

                                        if (cityWoSGByL[level] == null)
                                        {
                                            cityWoSGByL[level] = new List<Set_City>();
                                        }
                                        cityWoSGByL[level].Add(city);

                                        if (l.isOcean)
                                        {
                                            if (oCityByL[level] == null)
                                            {
                                                oCityByL[level] = new List<Set_City>();
                                            }
                                            oCityByL[level].Add(city);

                                            if (oCityWoSGByL[level] == null)
                                            {
                                                oCityWoSGByL[level] = new List<Set_City>();
                                            }
                                            oCityWoSGByL[level].Add(city);
                                        }
                                        else
                                        {
                                            if (tCityByL[level] == null)
                                            {
                                                tCityByL[level] = new List<Set_City>();
                                            }
                                            tCityByL[level].Add(city);

                                            if (tCityWoSGByL[level] == null)
                                            {
                                                tCityWoSGByL[level] = new List<Set_City>();
                                            }
                                            tCityWoSGByL[level].Add(city);

                                            if (l.isCoastal)
                                            {
                                                if (cCityByL[level] == null)
                                                {
                                                    cCityByL[level] = new List<Set_City>();
                                                }
                                                cCityByL[level].Add(city);

                                                if (cCityWoSGByL[level] == null)
                                                {
                                                    cCityWoSGByL[level] = new List<Set_City>();
                                                }
                                                cCityWoSGByL[level].Add(city);
                                            }
                                        }
                                    }
                                }
                                else if (tS == typeof(Set_MinorHuman))
                                {
                                    //Console.WriteLine("CommunityLib: Sorting 'Set_MinorHuman' by level.");
                                    Set_MinorHuman mHuman = (s as Set_MinorHuman);
                                    int level = mHuman.getLevel();

                                    if (isFirstSTIteration)
                                    {
                                        if (minorHumanByL[level] == null)
                                        {
                                            minorHumanByL[level] = new List<Set_MinorHuman>();
                                        }
                                        minorHumanByL[level].Add(mHuman);

                                        if (minorHumanWoSGByL[level] == null)
                                        {
                                            minorHumanWoSGByL[level] = new List<Set_MinorHuman>();
                                        }
                                        minorHumanWoSGByL[level].Add(mHuman);

                                        if (l.isOcean)
                                        {
                                            if (oMinorHumanByL[level] == null)
                                            {
                                                oMinorHumanByL[level] = new List<Set_MinorHuman>();
                                            }
                                            oMinorHumanByL[level].Add(mHuman);

                                            if (oMinorHumanWoSGByL[level] == null)
                                            {
                                                oMinorHumanWoSGByL[level] = new List<Set_MinorHuman>();
                                            }
                                            oMinorHumanWoSGByL[level].Add(mHuman);
                                        }
                                        else
                                        {
                                            if (tMinorHumanByL[level] == null)
                                            {
                                                tMinorHumanByL[level] = new List<Set_MinorHuman>();
                                            }
                                            tMinorHumanByL[level].Add(mHuman);

                                            if (tMinorHumanWoSGByL[level] == null)
                                            {
                                                tMinorHumanWoSGByL[level] = new List<Set_MinorHuman>();
                                            }
                                            tMinorHumanWoSGByL[level].Add(mHuman);

                                            if (l.isCoastal)
                                            {
                                                if (cMinorHumanByL[level] == null)
                                                {
                                                    cMinorHumanByL[level] = new List<Set_MinorHuman>();
                                                }
                                                cMinorHumanByL[level].Add(mHuman);

                                                if (cMinorHumanWoSGByL[level] == null)
                                                {
                                                    cMinorHumanWoSGByL[level] = new List<Set_MinorHuman>();
                                                }
                                                cMinorHumanWoSGByL[level].Add(mHuman);
                                            }
                                        }
                                    }
                                }
                                else if (tS == typeof(Set_OrcCamp))
                                {
                                    //Console.WriteLine("CommunityLib: Sorting 'Set_OrcCamp' by specialism.");
                                    Set_OrcCamp orcCamp = (s as Set_OrcCamp);
                                    int specialism = orcCamp.specialism;

                                    if (isFirstSTIteration)
                                    {
                                        if (orcCampByS[specialism] == null)
                                        {
                                            orcCampByS[specialism] = new List<Set_OrcCamp>();
                                        }
                                        orcCampByS[specialism].Add(orcCamp);

                                        if (orcCampWoSGByS[specialism] == null)
                                        {
                                            orcCampWoSGByS[specialism] = new List<Set_OrcCamp>();
                                        }
                                        orcCampWoSGByS[specialism].Add(orcCamp);

                                        if (l.isOcean)
                                        {
                                            if (oOrcCampByS[specialism] == null)
                                            {
                                                oOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                            }
                                            oOrcCampByS[specialism].Add(orcCamp);

                                            if (oOrcCampWoSGByS[specialism] == null)
                                            {
                                                oOrcCampWoSGByS[specialism] = new List<Set_OrcCamp>();
                                            }
                                            oOrcCampWoSGByS[specialism].Add(orcCamp);
                                        }
                                        else
                                        {
                                            if (tOrcCampByS[specialism] == null)
                                            {
                                                tOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                            }
                                            tOrcCampByS[specialism].Add(orcCamp);

                                            if (tOrcCampWoSGByS[specialism] == null)
                                            {
                                                tOrcCampWoSGByS[specialism] = new List<Set_OrcCamp>();
                                            }
                                            tOrcCampWoSGByS[specialism].Add(orcCamp);

                                            if (l.isCoastal)
                                            {
                                                if (cOrcCampByS[specialism] == null)
                                                {
                                                    cOrcCampByS[specialism] = new List<Set_OrcCamp>();
                                                }
                                                cOrcCampByS[specialism].Add(orcCamp);

                                                if (cOrcCampWoSGByS[specialism] == null)
                                                {
                                                    cOrcCampWoSGByS[specialism] = new List<Set_OrcCamp>();
                                                }
                                                cOrcCampWoSGByS[specialism].Add(orcCamp);
                                            }
                                        }
                                    }
                                }

                                // Subsettlements without Social Group
                                if (s.subs != null && s.subs.Count > 0)
                                {
                                    //Console.WriteLine("CommunityLib: Starting Subsettlement Loop");
                                    foreach (Subsettlement sub in s.subs)
                                    {
                                        tSub = sub.GetType();
                                        iterateSsT = true;
                                        isInitialSsTIteration = true;

                                        while (iterateSsT)
                                        {
                                            isFirstSsTIteration = isFirstLTIteration && isFirstSTIteration && isInitialSsTIteration;

                                            // Very first instance. Fires only once.
                                            // Iterates nothing.
                                            if (isFirstSsTIteration)
                                            {
                                                subsettlements.Add(sub);
                                                CreateAndOrAddToKeyListPair(ssByTE, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(ssByLTE, tL, typeof(Subsettlement), sub);
                                                CreateAndOrAddToKeyListPair(ssBySTE, tS, typeof(Subsettlement), sub);
                                                CreateAndOrAddToKeyListPair(ssWoSGByTE, tSub, tSub, sub);

                                                TryCreateSubDictionary(ssByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(ssByLTEByTE, tL, typeof(Type));
                                                TryCreateSubDictionary(ssBySTEByT, tS, typeof(Type));
                                                TryCreateSubDictionary(ssBySTEByTE, tS, typeof(Type));

                                                CreateAndOrAddToKeyListPair(ssByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(ssBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    oceanSubsettlements.Add(sub);
                                                    CreateAndOrAddToKeyListPair(oSsByTE, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(oSsByLTE, tL, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(oSsBySTE, tS, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(oSsWoSGByTE, tSub, tSub, sub);

                                                    TryCreateSubDictionary(oSsByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oSsByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(oSsBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(oSsBySTEByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(oSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    terrestrialSubsettlements.Add(sub);
                                                    CreateAndOrAddToKeyListPair(tSsByTE, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(tSsByLTE, tL, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(tSsBySTE, tS, typeof(Subsettlement), sub);
                                                    CreateAndOrAddToKeyListPair(tSsWoSGByTE, tSub, tSub, sub);

                                                    TryCreateSubDictionary(tSsByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tSsByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(tSsBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(tSsBySTEByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(tSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        coastalSubsettlements.Add(sub);
                                                        CreateAndOrAddToKeyListPair(cSsByTE, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(cSsByLTE, tL, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(cSsBySTE, tS, typeof(Subsettlement), sub);
                                                        CreateAndOrAddToKeyListPair(cSsWoSGByTE, tSub, tSub, sub);

                                                        TryCreateSubDictionary(cSsByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cSsByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(cSsBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(cSsBySTEByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cSsByLTEByTE[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(cSsBySTEByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for initial iterations of settlement type.
                                            // Iterates location type.
                                            if (isInitialSTIteration && isInitialSsTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(ssByLT, tL, typeof(Subsettlement), sub);

                                                TryCreateSubDictionary(ssByLTByT, tL, typeof(Type));
                                                TryCreateSubDictionary(ssByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(ssByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oSsByLT, tL, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(oSsByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oSsByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tSsByLT, tL, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(tSsByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tSsByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cSsByLT, tL, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(cSsByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cSsByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cSsByLTByTE[tL] as IDictionary, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for first iteration of location type loop.
                                            // Iterates settlement type.
                                            if (isFirstLTIteration && isInitialSsTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(ssByST, tS, typeof(Subsettlement), sub);

                                                TryCreateSubDictionary(ssBySTByT, tS, typeof(Type));
                                                TryCreateSubDictionary(ssBySTByTE, tS, typeof(Type));

                                                CreateAndOrAddToKeyListPair(ssBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oSsByST, tS, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(oSsBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(oSsBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tSsByST, tS, typeof(Subsettlement), sub);

                                                    TryCreateSubDictionary(tSsBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(tSsBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cSsByST, tS, typeof(Subsettlement), sub);

                                                        TryCreateSubDictionary(cSsBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(cSsBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cSsBySTByTE[tS] as IDictionary, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for first iteration of settlement type and location type loops.
                                            // Iterates subsettlement type.
                                            if (isFirstLTIteration && isFirstSTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(ssByT, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(ssByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(ssBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(ssWoSGByT, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oSsByT, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(oSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(oSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(oSsWoSGByT, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tSsByT, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(tSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(tSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(tSsWoSGByT, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cSsByT, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(cSsByLTEByT[tL] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(cSsBySTEByT[tS] as IDictionary, tSub, tSub, sub);
                                                        CreateAndOrAddToKeyListPair(cSsWoSGByT, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for initial iterations of settlement type loop.
                                            // Iterates location type and subsettlement type.
                                            if (isInitialSTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(ssByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cSsByLTByT[tL] as IDictionary, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for first iteration of location type loop.
                                            // Iterates settlement type and subsettlement type.
                                            if (isFirstLTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(ssBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cSsBySTByT[tS] as IDictionary, tSub, tSub, sub);
                                                    }
                                                }
                                            }

                                            if (tSub == targetTSub)
                                            {
                                                iterateSsT = false;
                                                //Console.WriteLine("CommunityLib: End subsettlement type loop.");
                                            }
                                            else
                                            {
                                                isInitialSsTIteration = false;
                                                tSub = tSub.BaseType;
                                                //Console.WriteLine("CommunityLib: Iterating Type to " + tSub.Name + ".");
                                            }
                                        }
                                    }
                                }

                                // Properties without SocialGroup
                                if (l.properties != null && l.properties.Count > 0)
                                {
                                    //Console.WriteLine("CommunityLib: Starting Properties Loop");
                                    foreach (Property p in l.properties)
                                    {
                                        tP = p.GetType();
                                        iteratePT = true;
                                        isInitialPTIteration = true;

                                        //Console.WriteLine("CommunityLib: Starting property loop for Property " + p.getName() + " of type " + tP.Name + ".");

                                        while (iteratePT)
                                        {
                                            isFirstPTIteration = isFirstLTIteration && isFirstSTIteration && isInitialPTIteration;

                                            // Very first instance. Fires only once.
                                            // Iterates nothing.
                                            if (isFirstLTIteration)
                                            {
                                                properties.Add(p);
                                                CreateAndOrAddToKeyListPair(pByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pByLTE, tL, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pBySTE, tS, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(pWoSGByTE, tP, tP, p);

                                                TryCreateSubDictionary(pByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(pByLTEByTE, tL, typeof(Type));
                                                TryCreateSubDictionary(pBySTEByT, tS, typeof(Type));
                                                TryCreateSubDictionary(pBySTEByTE, tS, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    oceanProperties.Add(p);
                                                    CreateAndOrAddToKeyListPair(oPByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPBySTE, tS, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(oPWoSGByTE, tP, tP, p);

                                                    TryCreateSubDictionary(oPByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(oPBySTEByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    terrestrialProperties.Add(p);
                                                    CreateAndOrAddToKeyListPair(tPByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPBySTE, tS, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(tPWoSGByTE, tP, tP, p);

                                                    TryCreateSubDictionary(tPByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPByLTEByTE, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPBySTEByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(tPBySTEByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        coastalProperties.Add(p);
                                                        CreateAndOrAddToKeyListPair(cPByTE, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPByLTE, tL, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPBySTE, tS, typeof(Property), p);
                                                        CreateAndOrAddToKeyListPair(cPWoSGByTE, tP, tP, p);

                                                        TryCreateSubDictionary(cPByLTEByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPByLTEByTE, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPBySTEByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(cPBySTEByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPBySTEByTE[tS] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for initial iteration of settlement type loop.
                                            // Iterates location type.
                                            if (isInitialSTIteration && isInitialPTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByLT, tL, typeof(Property), p);

                                                TryCreateSubDictionary(pByLTByT, tL, typeof(Type));
                                                TryCreateSubDictionary(pByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pByLTByTE[tL] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(oPByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(oPByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(tPByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(tPByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByLT, tL, typeof(Property), p);

                                                        TryCreateSubDictionary(cPByLTByT, tL, typeof(Type));
                                                        TryCreateSubDictionary(cPByLTByTE, tL, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires once each iteration, but only for first iteration of location type and social group type loops.
                                            // Iterates settlement type.
                                            if (isFirstLTIteration && isInitialPTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByST, tS, typeof(Property), p);

                                                TryCreateSubDictionary(pBySTByT, tS, typeof(Type));
                                                TryCreateSubDictionary(pBySTByTE, tS, typeof(Type));

                                                CreateAndOrAddToKeyListPair(pBySTByTE[tS] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByST, tS, typeof(Property), p);

                                                    TryCreateSubDictionary(oPBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(oPBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(oPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByST, tS, typeof(Property), p);

                                                    TryCreateSubDictionary(tPBySTByT, tS, typeof(Type));
                                                    TryCreateSubDictionary(tPBySTByTE, tS, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(tPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByST, tS, typeof(Property), p);

                                                        TryCreateSubDictionary(cPBySTByT, tS, typeof(Type));
                                                        TryCreateSubDictionary(cPBySTByTE, tS, typeof(Type));

                                                        CreateAndOrAddToKeyListPair(cPBySTByTE[tS] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for first iteration of settlement type, location type and social group type loops.
                                            // Iterates property type.
                                            if (isFirstLTIteration && isFirstSTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pByLTEByT[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pBySTEByT[tS] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(pWoSGByT, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(oPWoSGByT, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(tPWoSGByT, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPBySTEByT[tS] as IDictionary, tP, tP, p);
                                                        CreateAndOrAddToKeyListPair(cPWoSGByT, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for initial iterations of settlement type loop, of first iteration of social group type loop.
                                            // Iterates location type and property type.
                                            if (isInitialSTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pByLTByT[tL] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPByLTByT[tL] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPByLTByT[tL] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPByLTByT[tL] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            // Fires each iteration, but only for first iteration of location type and social group type loops.
                                            // Iterates settlement type and property type.
                                            if (isFirstLTIteration)
                                            {
                                                CreateAndOrAddToKeyListPair(pBySTByT[tS] as IDictionary, tP, tP, p);
                                                if (l.isOcean)
                                                {
                                                    CreateAndOrAddToKeyListPair(oPBySTByT[tS] as IDictionary, tP, tP, p);
                                                }
                                                else
                                                {
                                                    CreateAndOrAddToKeyListPair(tPBySTByT[tS] as IDictionary, tP, tP, p);
                                                    if (l.isCoastal)
                                                    {
                                                        CreateAndOrAddToKeyListPair(cPBySTByT[tS] as IDictionary, tP, tP, p);
                                                    }
                                                }
                                            }

                                            if (tP == targetTP)
                                            {
                                                iteratePT = false;
                                                //Console.WriteLine("CommunityLib: End property type loop.");
                                            }
                                            else
                                            {
                                                isInitialPTIteration = false;
                                                tP = tP.BaseType;
                                                //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name + ".");
                                            }
                                        }
                                    }
                                }

                                if (tS == targetTS)
                                {
                                    iterateST = false;
                                    //Console.WriteLine("CommunityLib: End settlement type loop.");
                                }
                                else
                                {
                                    isFirstSTIteration = false;
                                    isInitialSTIteration = false;
                                    tS = tS.BaseType;
                                    //Console.WriteLine("CommunityLib: Iterating Type to " + tS.Name + ".");
                                }
                            }
                        }
                        else
                        {
                            // Very First iteration. Fires only once.
                            if (isFirstLTIteration)
                            {
                                CreateAndOrAddToKeyListPair(lWoSByTE, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lWoSG_SByTE, tL, tL, l);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oLWoSByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLWoSG_SByTE, tL, tL, l);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tLWoSByTE, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLWoSG_SByTE, tL, tL, l);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cLWoSByTE, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLWoSG_SByTE, tL, tL, l);
                                    }
                                }
                            }

                            // Fires each iteration.
                            CreateAndOrAddToKeyListPair(lWoSByT, tL, tL, l);
                            CreateAndOrAddToKeyListPair(lWoSG_SByT, tL, tL, l);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oLWoSByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(oLWoSG_SByT, tL, tL, l);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tLWoSByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(tLWoSG_SByT, tL, tL, l);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cLWoSByT, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(cLWoSG_SByT, tL, tL, l);
                                }
                            }

                            // Properties without SocialGroup and Settlement
                            if (l.properties != null && l.properties.Count > 0)
                            {
                                //Console.WriteLine("CommunityLib: Starting Properties Loop");
                                foreach (Property p in l.properties)
                                {
                                    tP = p.GetType();
                                    iteratePT = true;
                                    isInitialPTIteration = true;

                                    //Console.WriteLine("CommunityLib: Starting property loop for Property " + p.getName() + " of type " + tP.Name + ".");

                                    while (iteratePT)
                                    {
                                        isFirstPTIteration = isFirstLTIteration && isInitialPTIteration;

                                        // Very first instance. Fires only once.
                                        // Iterates nothing.
                                        if (isFirstLTIteration)
                                        {
                                            properties.Add(p);
                                            CreateAndOrAddToKeyListPair(pByTE, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pByLTE, tL, typeof(Property), p);
                                            CreateAndOrAddToKeyListPair(pWoSGByTE, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pWoSByTE, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pWoSG_SByTE, tP, tP, p);

                                            TryCreateSubDictionary(pByLTEByT, tL, typeof(Type));
                                            TryCreateSubDictionary(pByLTEByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(pByLTEByTE[tL] as IDictionary, tP, tP, p);
                                            if (l.isOcean)
                                            {
                                                oceanProperties.Add(p);
                                                CreateAndOrAddToKeyListPair(oPByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPByLTE, tL, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(oPWoSGByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPWoSByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPWoSG_SByTE, tP, tP, p);

                                                TryCreateSubDictionary(oPByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(oPByLTEByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(oPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                            }
                                            else
                                            {
                                                terrestrialProperties.Add(p);
                                                CreateAndOrAddToKeyListPair(tPByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPByLTE, tL, typeof(Property), p);
                                                CreateAndOrAddToKeyListPair(tPWoSGByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPWoSByTE, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPWoSG_SByTE, tP, tP, p);

                                                TryCreateSubDictionary(tPByLTEByT, tL, typeof(Type));
                                                TryCreateSubDictionary(tPByLTEByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(tPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                if (l.isCoastal)
                                                {
                                                    coastalProperties.Add(p);
                                                    CreateAndOrAddToKeyListPair(cPByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPByLTE, tL, typeof(Property), p);
                                                    CreateAndOrAddToKeyListPair(cPWoSGByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPWoSByTE, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPWoSG_SByTE, tP, tP, p);

                                                    TryCreateSubDictionary(cPByLTEByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(cPByLTEByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(cPByLTEByTE[tL] as IDictionary, tP, tP, p);
                                                }
                                            }
                                        }

                                        // Fires once each iteration.
                                        // Iterates location type.
                                        if (isInitialPTIteration)
                                        {
                                            CreateAndOrAddToKeyListPair(pByLT, tL, typeof(Property), p);

                                            TryCreateSubDictionary(pByLTByT, tL, typeof(Type));
                                            TryCreateSubDictionary(pByLTByTE, tL, typeof(Type));

                                            CreateAndOrAddToKeyListPair(pByLTByTE[tL] as IDictionary, tP, tP, p);
                                            if (l.isOcean)
                                            {
                                                CreateAndOrAddToKeyListPair(oPByLT, tL, typeof(Property), p);

                                                TryCreateSubDictionary(oPByLTByT, tL, typeof(Type));
                                                TryCreateSubDictionary(oPByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(oPByLTByTE[tL] as IDictionary, tP, tP, p);
                                            }
                                            else
                                            {
                                                CreateAndOrAddToKeyListPair(tPByLT, tL, typeof(Property), p);

                                                TryCreateSubDictionary(tPByLTByT, tL, typeof(Type));
                                                TryCreateSubDictionary(tPByLTByTE, tL, typeof(Type));

                                                CreateAndOrAddToKeyListPair(tPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                if (l.isCoastal)
                                                {
                                                    CreateAndOrAddToKeyListPair(cPByLT, tL, typeof(Property), p);

                                                    TryCreateSubDictionary(cPByLTByT, tL, typeof(Type));
                                                    TryCreateSubDictionary(cPByLTByTE, tL, typeof(Type));

                                                    CreateAndOrAddToKeyListPair(cPByLTByTE[tL] as IDictionary, tP, tP, p);
                                                }
                                            }
                                        }

                                        // Fires each iteration, but only for first iteration of location type loop.
                                        // Iterates property type.
                                        if (isFirstLTIteration)
                                        {
                                            CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pByLTEByT[tL] as IDictionary, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pWoSGByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pWoSByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(pWoSG_SByT, tP, tP, p);
                                            if (l.isOcean)
                                            {
                                                CreateAndOrAddToKeyListPair(oPByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPWoSGByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPWoSByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(oPWoSG_SByT, tP, tP, p);
                                            }
                                            else
                                            {
                                                CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPWoSGByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPWoSByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(tPWoSG_SByT, tP, tP, p);
                                                if (l.isCoastal)
                                                {
                                                    CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPByLTEByT[tL] as IDictionary, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPWoSGByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPWoSByT, tP, tP, p);
                                                    CreateAndOrAddToKeyListPair(cPWoSG_SByT, tP, tP, p);
                                                }
                                            }
                                        }

                                        // Fires each iteration.
                                        // Iterates location type and property type.

                                        CreateAndOrAddToKeyListPair(pByLTByT[tL] as IDictionary, tP, tP, p);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oPByLTByT[tL] as IDictionary, tP, tP, p);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tPByLTByT[tL] as IDictionary, tP, tP, p);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cPByLTByT[tL] as IDictionary, tP, tP, p);
                                            }
                                        }

                                        if (tP == targetTP)
                                        {
                                            iteratePT = false;
                                            //Console.WriteLine("CommunityLib: End property type loop.");
                                        }
                                        else
                                        {
                                            isInitialPTIteration = false;
                                            tP = tP.BaseType;
                                            //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name + ".");
                                        }
                                    }
                                }
                            }
                        }

                        if (tL == targetTL)
                        {
                            iterateLT = false;
                            //Console.WriteLine("CommunityLib: End location type loop.");
                        }
                        else
                        {
                            isFirstLTIteration = false;
                            tL = tL.BaseType;
                            //Console.WriteLine("CommunityLib: Iterating Type to " + tL.Name + ".");
                        }
                    }
                }
            }
        }
    }
}
