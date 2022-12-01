using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    internal class Storage__NotCompiled_
    {

        public void FilterLocationsOld()
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

                //Console.WriteLine("CommunityLib: Starting Distance Calculations for Location " + l.getName() + " of Type " + l.GetType().Name + ".");
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

                CreateAndOrAddToKeyListPair(lByTE, tL, tL, l);
                if (l.isOcean)
                {
                    oceanLocations.Add(l);
                    CreateAndOrAddToKeyListPair(oLByTE, tL, tL, l);
                }
                else
                {
                    terrestrialLocations.Add(l);
                    CreateAndOrAddToKeyListPair(tLByTE, tL, tL, l);
                    if (l.isCoastal)
                    {
                        coastalLocations.Add(l);
                        CreateAndOrAddToKeyListPair(cLByTE, tL, tL, l);
                    }
                }

                // Branch for Social Groups
                if (sG != null)
                {
                    //Console.WriteLine("CommunityLib: Location belongs to Social Group " + l.soc.name + " of Type " + tSG.Name + ".");
                    CreateAndOrAddToKeyListPair(lBySG, l.soc, typeof(Location), l);
                    CreateAndOrAddToKeyListPair(lBySGTE, tSG, typeof(Location), l);

                    // TryCreate SubDictionaries
                    TryCreateSubDictionary(lBySGByT, l.soc, typeof(Type));
                    TryCreateSubDictionary(lBySGTEByT, tSG, typeof(Type));
                    TryCreateSubDictionary(lBySGTEByTE, tSG, typeof(Type));
                    CreateAndOrAddToKeyListPair(lBySGTEByTE[tSG] as IDictionary, tL, tL, l);

                    if (l.isOcean)
                    {
                        CreateAndOrAddToKeyListPair(oLBySG, l.soc, typeof(Location), l);
                        CreateAndOrAddToKeyListPair(oLBySGTE, tSG, typeof(Location), l);

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(oLBySGByT, l.soc, typeof(Type));
                        TryCreateSubDictionary(oLBySGTEByT, tSG, typeof(Type));
                        TryCreateSubDictionary(oLBySGTEByTE, tSG, typeof(Type));
                        CreateAndOrAddToKeyListPair(oLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                    }
                    else
                    {
                        CreateAndOrAddToKeyListPair(tLBySG, l.soc, typeof(Location), l);
                        CreateAndOrAddToKeyListPair(tLBySGTE, tSG, typeof(Location), l);

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(tLBySGByT, l.soc, typeof(Type));
                        TryCreateSubDictionary(tLBySGTEByT, tSG, typeof(Type));
                        TryCreateSubDictionary(tLBySGTEByTE, tSG, typeof(Type));
                        CreateAndOrAddToKeyListPair(tLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                        if (l.isCoastal)
                        {
                            CreateAndOrAddToKeyListPair(cLBySG, l.soc, typeof(Location), l);
                            CreateAndOrAddToKeyListPair(cLBySGTE, tSG, typeof(Location), l);

                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(cLBySGByT, l.soc, typeof(Type));
                            TryCreateSubDictionary(cLBySGTEByT, tSG, typeof(Type));
                            TryCreateSubDictionary(cLBySGTEByTE, tSG, typeof(Type));
                            CreateAndOrAddToKeyListPair(cLBySGTEByTE[tSG] as IDictionary, tL, tL, l);
                        }
                    }
                }
                else
                {
                    CreateAndOrAddToKeyListPair(lWoSGByTE, tL, tL, l);
                    if (l.isOcean)
                    {
                        CreateAndOrAddToKeyListPair(oLWoSGByTE, tL, tL, l);
                    }
                    else
                    {
                        CreateAndOrAddToKeyListPair(tLWoSGByTE, tL, tL, l);
                        if (l.isCoastal)
                        {
                            CreateAndOrAddToKeyListPair(cLWoSGByTE, tL, tL, l);
                        }
                    }
                }

                // Branch for Settlements.
                if (s != null)
                {
                    //Console.WriteLine("CommunityLib: Location has settlement " + s.name + " of Type " + tS.Name + ".");
                    settlements.Add(s);
                    CreateAndOrAddToKeyListPair(sByTE, tS, tS, s);
                    CreateAndOrAddToKeyListPair(sByLTE, tL, tS, s);

                    if (l.isOcean)
                    {
                        oceanSettlements.Add(s);
                        CreateAndOrAddToKeyListPair(oSByTE, tS, tS, s);
                        CreateAndOrAddToKeyListPair(oSByLTE, tL, tS, s);
                    }
                    else
                    {
                        terrestrialSettlements.Add(s);
                        CreateAndOrAddToKeyListPair(tSByTE, tS, tS, s);
                        CreateAndOrAddToKeyListPair(tSByLTE, tL, tS, s);
                        if (l.isCoastal)
                        {
                            coastalSettlements.Add(s);
                            CreateAndOrAddToKeyListPair(cSByTE, tS, tS, s);
                            CreateAndOrAddToKeyListPair(cSByLTE, tL, tS, s);
                        }
                    }

                    if (sG != null)
                    {
                        CreateAndOrAddToKeyListPair(sBySG, l.soc, typeof(Settlement), s);
                        CreateAndOrAddToKeyListPair(sBySGTE, tSG, typeof(Settlement), s);

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(sBySGByT, l.soc, typeof(Type));
                        TryCreateSubDictionary(sBySGTEByT, tSG, typeof(Type));
                        TryCreateSubDictionary(sBySGTEByTE, tSG, typeof(Type));
                        CreateAndOrAddToKeyListPair(sBySGTEByTE[tSG] as IDictionary, tS, tS, s);

                        if (l.isOcean)
                        {
                            CreateAndOrAddToKeyListPair(oSBySG, l.soc, typeof(Settlement), s);
                            CreateAndOrAddToKeyListPair(oSBySGTE, tSG, typeof(Settlement), s);

                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(oSBySGByT, l.soc, typeof(Type));
                            TryCreateSubDictionary(oSBySGTEByT, tSG, typeof(Type));
                            TryCreateSubDictionary(oSBySGTEByTE, tSG, typeof(Type));
                            CreateAndOrAddToKeyListPair(oSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(tSBySG, l.soc, typeof(Settlement), s);
                            CreateAndOrAddToKeyListPair(tSBySGTE, tSG, typeof(Settlement), s);

                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(tSBySGByT, l.soc, typeof(Type));
                            TryCreateSubDictionary(tSBySGTEByT, tSG, typeof(Type));
                            TryCreateSubDictionary(tSBySGTEByTE, tSG, typeof(Type));
                            CreateAndOrAddToKeyListPair(tSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                            if (l.isCoastal)
                            {
                                CreateAndOrAddToKeyListPair(cSBySG, l.soc, typeof(Settlement), s);
                                CreateAndOrAddToKeyListPair(cSBySGTE, tSG, typeof(Settlement), s);

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(cSBySGByT, l.soc, typeof(Type));
                                TryCreateSubDictionary(cSBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(cSBySGTEByTE, tSG, typeof(Type));
                                CreateAndOrAddToKeyListPair(cSBySGTEByTE[tSG] as IDictionary, tS, tS, s);
                            }
                        }
                    }

                    // Subsettlement Loop
                    foreach (Subsettlement sub in s.subs)
                    {
                        tSub = sub.GetType();

                        subsettlements.Add(sub);
                        CreateAndOrAddToKeyListPair(ssByTE, tSub, tSub, sub);
                        CreateAndOrAddToKeyListPair(ssByLTE, tL, tSub, sub);
                        CreateAndOrAddToKeyListPair(ssBySTE, tS, tSub, sub);

                        if (l.isOcean)
                        {
                            oceanSubsettlements.Add(sub);
                            CreateAndOrAddToKeyListPair(oSsByTE, tSub, tSub, sub);
                            CreateAndOrAddToKeyListPair(oSsByLTE, tL, tSub, sub);
                            CreateAndOrAddToKeyListPair(oSsBySTE, tS, tSub, sub);
                        }
                        else
                        {
                            terrestrialSubsettlements.Add(sub);
                            CreateAndOrAddToKeyListPair(tSsByTE, tSub, tSub, sub);
                            CreateAndOrAddToKeyListPair(tSsByLTE, tL, tSub, sub);
                            CreateAndOrAddToKeyListPair(tSsBySTE, tS, tSub, sub);
                            if (l.isCoastal)
                            {
                                coastalSubsettlements.Add(sub);
                                CreateAndOrAddToKeyListPair(cSsByTE, tSub, tSub, sub);
                                CreateAndOrAddToKeyListPair(cSsByLTE, tL, tSub, sub);
                                CreateAndOrAddToKeyListPair(cSsBySTE, tS, tSub, sub);
                            }
                        }

                        if (sG != null)
                        {
                            CreateAndOrAddToKeyListPair(ssBySG, l.soc, typeof(Subsettlement), sub);
                            CreateAndOrAddToKeyListPair(ssBySGTE, tSG, typeof(Subsettlement), sub);

                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(ssBySGByT, l.soc, typeof(Type));
                            TryCreateSubDictionary(ssBySGTEByT, tSG, typeof(Type));
                            TryCreateSubDictionary(ssBySGTEByTE, tSG, typeof(Type));
                            CreateAndOrAddToKeyListPair(ssBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);

                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oSsBySG, l.soc, typeof(Subsettlement), sub);
                                CreateAndOrAddToKeyListPair(oSsBySGTE, tSG, typeof(Subsettlement), sub);

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(oSsBySGByT, l.soc, typeof(Type));
                                TryCreateSubDictionary(oSsBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(oSsBySGTEByTE, tSG, typeof(Type));
                                CreateAndOrAddToKeyListPair(oSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tSsBySG, l.soc, typeof(Subsettlement), sub);
                                CreateAndOrAddToKeyListPair(tSsBySGTE, tSG, typeof(Subsettlement), sub);

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(tSsBySGByT, l.soc, typeof(Type));
                                TryCreateSubDictionary(tSsBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(tSsBySGTEByTE, tSG, typeof(Type));
                                CreateAndOrAddToKeyListPair(tSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cSsBySG, l.soc, typeof(Subsettlement), sub);
                                    CreateAndOrAddToKeyListPair(cSsBySGTE, tSG, typeof(Subsettlement), sub);

                                    // TryCreate SubDictionaries
                                    TryCreateSubDictionary(cSsBySGByT, l.soc, typeof(Type));
                                    TryCreateSubDictionary(cSsBySGTEByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(cSsBySGTEByTE, tSG, typeof(Type));
                                    CreateAndOrAddToKeyListPair(cSsBySGTEByTE[tSG] as IDictionary, tSub, tSub, sub);
                                }
                            }
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(ssWoSGByTE, tSub, tSub, sub);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oSsWoSGByTE, tSub, tSub, sub);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tSsWoSGByTE, tSub, tSub, sub);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cSsWoSGByTE, tSub, tSub, sub);
                                }
                            }
                        }
                    }
                }
                else
                {
                    CreateAndOrAddToKeyListPair(lWoSByTE, tL, tL, l);
                    if (l.isOcean)
                    {
                        CreateAndOrAddToKeyListPair(oLWoSByTE, tL, tL, l);
                    }
                    else
                    {
                        CreateAndOrAddToKeyListPair(tLWoSByTE, tL, tL, l);
                        if (l.isCoastal)
                        {
                            CreateAndOrAddToKeyListPair(cLWoSByTE, tL, tL, l);
                        }
                    }

                    if (sG != null)
                    {
                        CreateAndOrAddToKeyListPair(lWoSBySG, sG, typeof(Location), l);
                        CreateAndOrAddToKeyListPair(lWoSBySGTE, tSG, typeof(Location), l);
                        if (l.isOcean)
                        {
                            CreateAndOrAddToKeyListPair(oLWoSBySG, sG, typeof(Location), l);
                            CreateAndOrAddToKeyListPair(oLWoSBySGTE, tSG, typeof(Location), l);
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(tLWoSBySG, sG, typeof(Location), l);
                            CreateAndOrAddToKeyListPair(tLWoSBySGTE, tSG, typeof(Location), l);
                            if (l.isCoastal)
                            {
                                CreateAndOrAddToKeyListPair(cLWoSBySG, sG, typeof(Location), l);
                                CreateAndOrAddToKeyListPair(cLWoSBySGTE, tSG, typeof(Location), l);
                            }
                        }
                    }
                    else
                    {
                        CreateAndOrAddToKeyListPair(lWoSG_SByTE, tL, tL, l);
                        if (l.isOcean)
                        {
                            CreateAndOrAddToKeyListPair(oLWoSG_SByTE, tL, tL, l);
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(tLWoSG_SByTE, tL, tL, l);
                            if (l.isCoastal)
                            {
                                CreateAndOrAddToKeyListPair(cLWoSG_SByTE, tL, tL, l);
                            }
                        }
                    }
                }

                if (l.properties != null && l.properties.Count > 0)
                {
                    foreach (Property p in l.properties)
                    {
                        tP = p.GetType();

                        properties.Add(p);
                        CreateAndOrAddToKeyListPair(pByTE, tP, tP, p);
                        if (l.isOcean)
                        {
                            oceanProperties.Add(p);
                            CreateAndOrAddToKeyListPair(oPByTE, tP, tP, p);
                        }
                        else
                        {
                            terrestrialProperties.Add(p);
                            CreateAndOrAddToKeyListPair(tPByTE, tP, tP, p);
                            if (l.isCoastal)
                            {
                                coastalProperties.Add(p);
                                CreateAndOrAddToKeyListPair(cPByTE, tP, tP, p);
                            }
                        }

                        // Branch for Social Groups
                        if (sG != null)
                        {
                            //Console.WriteLine("CommunityLib: Property belongs to Social Group " + l.soc.name + " of Type " + tSG.Name + ".");
                            CreateAndOrAddToKeyListPair(pBySG, l.soc, typeof(Property), p);
                            CreateAndOrAddToKeyListPair(pBySGTE, tSG, typeof(Property), p);

                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(pBySGByT, l.soc, typeof(Type));
                            TryCreateSubDictionary(pBySGTEByT, tSG, typeof(Type));
                            TryCreateSubDictionary(pBySGTEByTE, tSG, typeof(Type));
                            CreateAndOrAddToKeyListPair(pBySGTEByTE[tSG] as IDictionary, tP, tP, p);

                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oPBySG, l.soc, typeof(Property), p);
                                CreateAndOrAddToKeyListPair(oPBySGTE, tSG, typeof(Property), p);

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(oPBySGByT, l.soc, typeof(Type));
                                TryCreateSubDictionary(oPBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(oPBySGTEByTE, tSG, typeof(Type));
                                CreateAndOrAddToKeyListPair(oPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tPBySG, l.soc, typeof(Property), p);
                                CreateAndOrAddToKeyListPair(tPBySGTE, tSG, typeof(Property), p);

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(tPBySGByT, l.soc, typeof(Type));
                                TryCreateSubDictionary(tPBySGTEByT, tSG, typeof(Type));
                                TryCreateSubDictionary(tPBySGTEByTE, tSG, typeof(Type));
                                CreateAndOrAddToKeyListPair(tPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cPBySG, l.soc, typeof(Property), p);
                                    CreateAndOrAddToKeyListPair(cPBySGTE, tSG, typeof(Property), p);

                                    // TryCreate SubDictionaries
                                    TryCreateSubDictionary(cPBySGByT, l.soc, typeof(Type));
                                    TryCreateSubDictionary(cPBySGTEByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(cPBySGTEByTE, tSG, typeof(Type));
                                    CreateAndOrAddToKeyListPair(cPBySGTEByTE[tSG] as IDictionary, tP, tP, p);
                                }
                            }
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(pWoSGByTE, tP, tP, p);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oPWoSGByTE, tP, tP, p);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tPWoSGByTE, tP, tP, p);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cPWoSGByTE, tP, tP, p);
                                }
                            }
                        }

                        if (s != null)
                        {

                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(pWoSByTE, tP, tP, p);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oPWoSByTE, tP, tP, p);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tPWoSByTE, tP, tP, p);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cPWoSByTE, tP, tP, p);
                                }
                            }

                            if (sG != null)
                            {
                                CreateAndOrAddToKeyListPair(pWoSBySG, sG, typeof(Property), p);
                                CreateAndOrAddToKeyListPair(pWoSBySGTE, tSG, typeof(Property), p);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oPWoSBySG, sG, typeof(Property), p);
                                    CreateAndOrAddToKeyListPair(oPWoSBySGTE, tSG, typeof(Property), p);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tPWoSBySG, sG, typeof(Property), p);
                                    CreateAndOrAddToKeyListPair(tPWoSBySGTE, tSG, typeof(Property), p);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cPWoSBySG, sG, typeof(Property), p);
                                        CreateAndOrAddToKeyListPair(cPWoSBySGTE, tSG, typeof(Property), p);
                                    }
                                }
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(pWoSG_SByTE, tP, tP, p);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oPWoSG_SByTE, tP, tP, p);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tPWoSG_SByTE, tP, tP, p);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cPWoSG_SByTE, tP, tP, p);
                                    }
                                }
                            }
                        }
                    }
                }

                // Set loop-only variables
                iterateLT = true;
                iterateSGT = true;
                iterateST = true;
                iterateSsT = true;
                isFirstSGTIteration = true;

                if (sG != null)
                {
                    //Console.WriteLine("CommunityLib: Starting scoial group type loop.");
                    // Conduct Operations for all Types tSG, from obj.GetType() to targetTSG, inclusively
                    while (iterateSGT)
                    {
                        // The subdictionaries used in this loop were checked for, and if neccesary created, earlier in this method
                        iterateLT = true;
                        tL = l.GetType();
                        if (s != null)
                        {
                            tS = s.GetType();
                        }

                        // TryCreate SubDictionaries
                        TryCreateSubDictionary(lBySGTByT, tSG, typeof(Type));
                        TryCreateSubDictionary(lBySGTByTE, tSG, typeof(Type));

                        CreateAndOrAddToKeyListPair(lBySGT, tSG, typeof(Location), l);
                        CreateAndOrAddToKeyListPair(lBySGTByTE[tSG] as IDictionary, tSG, tL, l);

                        if (l.isOcean)
                        {
                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(oLBySGTByT, tSG, typeof(Type));
                            TryCreateSubDictionary(oLBySGTByTE, tSG, typeof(Type));

                            CreateAndOrAddToKeyListPair(oLBySGT, tSG, typeof(Location), l);
                            CreateAndOrAddToKeyListPair(oLBySGTByTE[tSG] as IDictionary, tSG, tL, l);
                        }
                        else
                        {
                            // TryCreate SubDictionaries
                            TryCreateSubDictionary(tLBySGTByT, tSG, typeof(Type));
                            TryCreateSubDictionary(tLBySGTByTE, tSG, typeof(Type));

                            CreateAndOrAddToKeyListPair(tLBySGT, tSG, typeof(Location), l);
                            CreateAndOrAddToKeyListPair(tLBySGTByTE[tSG] as IDictionary, tSG, tL, l);
                            if (l.isCoastal)
                            {
                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(cLBySGTByT, tSG, typeof(Type));
                                TryCreateSubDictionary(cLBySGTByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(cLBySGT, tSG, typeof(Location), l);
                                CreateAndOrAddToKeyListPair(cLBySGTByTE[tSG] as IDictionary, tSG, tL, l);
                            }
                        }

                        //Console.WriteLine("CommunityLib: Starting location type loop.");
                        // Conduct Operations for all Types tL, from obj.GetType() to targetTL, inclusively
                        while (iterateLT)
                        {
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

                            }
                            else
                            {
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

                            if (isFirstSGTIteration)
                            {
                                CreateAndOrAddToKeyListPair(lByT, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGByT[l.soc] as IDictionary, tL, tL, l);
                                CreateAndOrAddToKeyListPair(lBySGTEByT[tSG] as IDictionary, tL, tL, l);

                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oLByT, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLBySGByT[l.soc] as IDictionary, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(oLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tLByT, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLBySGByT[l.soc] as IDictionary, tL, tL, l);
                                    CreateAndOrAddToKeyListPair(tLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cLByT, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLBySGByT[l.soc] as IDictionary, tL, tL, l);
                                        CreateAndOrAddToKeyListPair(cLBySGTEByT[tSG] as IDictionary, tL, tL, l);
                                    }
                                }
                            }

                            if (tL == targetTL)
                            {
                                iterateLT = false;
                                //Console.WriteLine("CommunityLib: End location type loop");
                            }
                            else
                            {
                                tL = tL.BaseType;
                                //Console.WriteLine("CommunityLib: Iterating Type to " + tL.Name  + ".");
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

                            if (l.isOcean)
                            {
                                //TryCreate SubDictionaries
                                TryCreateSubDictionary(oSBySGTByT, tSG, typeof(Type));
                                TryCreateSubDictionary(oSBySGTByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(oSBySGT, tSG, typeof(Settlement), s);
                                CreateAndOrAddToKeyListPair(oSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                            }
                            else
                            {
                                //TryCreate SubDictionaries
                                TryCreateSubDictionary(tSBySGTByT, tSG, typeof(Type));
                                TryCreateSubDictionary(tSBySGTByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(tSBySGT, tSG, typeof(Settlement), s);
                                CreateAndOrAddToKeyListPair(tSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                if (l.isCoastal)
                                {
                                    //TryCreate SubDictionaries
                                    TryCreateSubDictionary(cSBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(cSBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(cSBySGT, tSG, typeof(Settlement), s);
                                    CreateAndOrAddToKeyListPair(cSBySGTByTE[tSG] as IDictionary, tS, tS, s);
                                }
                            }

                            //Console.WriteLine("CommunityLib: Starting settlement type loop.");
                            while (iterateST)
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

                                if (isFirstSGTIteration)
                                {
                                    CreateAndOrAddToKeyListPair(sByT, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sBySGByT[l.soc] as IDictionary, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(sBySGTEByT[tSG] as IDictionary, tS, tS, s);

                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oSByT, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(oSBySGByT[l.soc] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(oSBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tSByT, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(tSBySGByT[l.soc] as IDictionary, tS, tS, s);
                                        CreateAndOrAddToKeyListPair(tSBySGTEByT[tSG] as IDictionary, tS, tS, s);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cSByT, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(cSBySGByT[l.soc] as IDictionary, tS, tS, s);
                                            CreateAndOrAddToKeyListPair(cSBySGTEByT[tSG] as IDictionary, tS, tS, s);
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
                                    tS = tS.BaseType;
                                    //Console.WriteLine("CommunityLib: Iterating Type to " + tS.Name + ".");
                                }
                            }

                            if (s.subs != null && s.subs.Count > 0)
                            {
                                foreach (Subsettlement sub in s.subs)
                                {
                                    tSub = sub.GetType();
                                    iterateSsT = true;

                                    //TryCreate SubDictionaries
                                    TryCreateSubDictionary(ssBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(ssBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(ssBySGT, tSG, typeof(Subsettlement), sub);
                                    CreateAndOrAddToKeyListPair(ssBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);

                                    if (l.isOcean)
                                    {
                                        //TryCreate SubDictionaries
                                        TryCreateSubDictionary(oSsBySGTByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(oSsBySGTByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(oSsBySGT, tSG, typeof(Subsettlement), sub);
                                        CreateAndOrAddToKeyListPair(oSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                    }
                                    else
                                    {
                                        //TryCreate SubDictionaries
                                        TryCreateSubDictionary(tSsBySGTByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(tSsBySGTByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(tSsBySGT, tSG, typeof(Subsettlement), sub);
                                        CreateAndOrAddToKeyListPair(tSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                        if (l.isCoastal)
                                        {
                                            //TryCreate SubDictionaries
                                            TryCreateSubDictionary(cSsBySGTByT, tSG, typeof(Type));
                                            TryCreateSubDictionary(cSsBySGTByTE, tSG, typeof(Type));

                                            CreateAndOrAddToKeyListPair(cSsBySGT, tSG, typeof(Subsettlement), sub);
                                            CreateAndOrAddToKeyListPair(cSsBySGTByTE[tSG] as IDictionary, tSub, tSub, sub);
                                        }
                                    }

                                    //Console.WriteLine("CommunityLib: Starting subsettlement type loop.");
                                    while (iterateSsT)
                                    {
                                        CreateAndOrAddToKeyListPair(ssBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cSsBySGTByT[tSG] as IDictionary, tSub, tSub, sub);
                                            }
                                        }

                                        if (isFirstSGTIteration)
                                        {
                                            CreateAndOrAddToKeyListPair(ssByT, tSub, tSub, sub);
                                            CreateAndOrAddToKeyListPair(ssBySGByT[l.soc] as IDictionary, tSub, tSub, sub);
                                            CreateAndOrAddToKeyListPair(ssBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);

                                            if (l.isOcean)
                                            {
                                                CreateAndOrAddToKeyListPair(oSsByT, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(oSsBySGByT[l.soc] as IDictionary, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(oSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                            }
                                            else
                                            {
                                                CreateAndOrAddToKeyListPair(tSsByT, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(tSsBySGByT[l.soc] as IDictionary, tSub, tSub, sub);
                                                CreateAndOrAddToKeyListPair(tSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
                                                if (l.isCoastal)
                                                {
                                                    CreateAndOrAddToKeyListPair(cSsByT, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(cSsBySGByT[l.soc] as IDictionary, tSub, tSub, sub);
                                                    CreateAndOrAddToKeyListPair(cSsBySGTEByT[tSG] as IDictionary, tSub, tSub, sub);
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
                                            tSub = tSub.BaseType;
                                            //Console.WriteLine("CommunityLib: Iterating Type to " + tSub.Name + ".");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
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

                        if (l.properties != null && l.properties.Count > 0)
                        {
                            foreach (Property p in l.properties)
                            {
                                tP = p.GetType();
                                iteratePT = true;

                                // TryCreate SubDictionaries
                                TryCreateSubDictionary(pBySGTByT, tSG, typeof(Type));
                                TryCreateSubDictionary(pBySGTByTE, tSG, typeof(Type));

                                CreateAndOrAddToKeyListPair(pBySGT, tSG, typeof(Property), p);
                                CreateAndOrAddToKeyListPair(pBySGTByTE[tSG] as IDictionary, tP, tP, p);

                                if (l.isOcean)
                                {
                                    // TryCreate SubDictionaries
                                    TryCreateSubDictionary(oPBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(oPBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(oPBySGT, tSG, typeof(Property), p);
                                    CreateAndOrAddToKeyListPair(oPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                }
                                else
                                {
                                    // TryCreate SubDictionaries
                                    TryCreateSubDictionary(tPBySGTByT, tSG, typeof(Type));
                                    TryCreateSubDictionary(tPBySGTByTE, tSG, typeof(Type));

                                    CreateAndOrAddToKeyListPair(tPBySGT, tSG, typeof(Property), p);
                                    CreateAndOrAddToKeyListPair(tPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                    if (l.isCoastal)
                                    {
                                        // TryCreate SubDictionaries
                                        TryCreateSubDictionary(cPBySGTByT, tSG, typeof(Type));
                                        TryCreateSubDictionary(cPBySGTByTE, tSG, typeof(Type));

                                        CreateAndOrAddToKeyListPair(cPBySGT, tSG, typeof(Property), p);
                                        CreateAndOrAddToKeyListPair(cPBySGTByTE[tSG] as IDictionary, tP, tP, p);
                                    }
                                }

                                if (s != null)
                                {

                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(pWoSBySGT, tSG, typeof(Property), p);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oPWoSBySGT, tSG, typeof(Property), p);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tPWoSBySGT, tSG, typeof(Property), p);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cPWoSBySGT, tSG, typeof(Property), p);
                                        }
                                    }
                                }

                                //Console.WriteLine("CommunityLib: Starting property type loop.");
                                // Conduct Operations for all Types tL, from obj.GetType() to targetTL, inclusively
                                while (iteratePT)
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

                                    if (s != null)
                                    {

                                    }
                                    else
                                    {
                                        if (isFirstSGTIteration)
                                        {
                                            CreateAndOrAddToKeyListPair(pWoSByT, tP, tP, p);
                                            if (l.isOcean)
                                            {
                                                CreateAndOrAddToKeyListPair(oPWoSByT, tP, tP, p);
                                            }
                                            else
                                            {
                                                CreateAndOrAddToKeyListPair(tPWoSByT, tP, tP, p);
                                                if (l.isCoastal)
                                                {
                                                    CreateAndOrAddToKeyListPair(cPWoSByT, tP, tP, p);
                                                }
                                            }
                                        }
                                    }

                                    if (isFirstSGTIteration)
                                    {
                                        CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(pBySGByT[l.soc] as IDictionary, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(pBySGTEByT[tSG] as IDictionary, tP, tP, p);

                                        if (l.isOcean)
                                        {
                                            CreateAndOrAddToKeyListPair(oPByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(oPBySGByT[l.soc] as IDictionary, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(oPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                        }
                                        else
                                        {
                                            CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(tPBySGByT[l.soc] as IDictionary, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(tPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                            if (l.isCoastal)
                                            {
                                                CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(cPBySGByT[l.soc] as IDictionary, tP, tP, p);
                                                CreateAndOrAddToKeyListPair(cPBySGTEByT[tSG] as IDictionary, tP, tP, p);
                                            }
                                        }
                                    }

                                    if (tP == targetTP)
                                    {
                                        iteratePT = false;
                                        //Console.WriteLine("CommunityLib: End property type loop");
                                    }
                                    else
                                    {
                                        tP = tP.BaseType;
                                        //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name  + ".");
                                    }
                                }
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
                    //Console.WriteLine("CommunityLib: Starting location type loop.");
                    while (iterateLT)
                    {
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

                        }
                        else
                        {
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
                        }

                        if (tL == targetTL)
                        {
                            iterateLT = false;
                            //Console.WriteLine("CommunityLib: End location type loop.");
                        }
                        else
                        {
                            tL = tL.BaseType;
                            //Console.WriteLine("CommunityLib: Iterating Type to " + tL.Name + ".");
                        }
                    }

                    if (s != null)
                    {
                        CreateAndOrAddToKeyListPair(sWoSGByTE, tS, tS, s);
                        if (l.isOcean)
                        {
                            CreateAndOrAddToKeyListPair(oSWoSGByTE, tS, tS, s);
                        }
                        else
                        {
                            CreateAndOrAddToKeyListPair(tSWoSGByTE, tS, tS, s);
                            if (l.isCoastal)
                            {
                                CreateAndOrAddToKeyListPair(cSWoSGByTE, tS, tS, s);
                            }
                        }

                        //Console.WriteLine("CommunityLib: Starting settlement type loop.");
                        while (iterateST)
                        {
                            CreateAndOrAddToKeyListPair(sByT, tS, tS, s);
                            CreateAndOrAddToKeyListPair(sWoSGByT, tS, tS, s);

                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oSByT, tS, tS, s);
                                CreateAndOrAddToKeyListPair(oSWoSGByT, tS, tS, s);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tSByT, tS, tS, s);
                                CreateAndOrAddToKeyListPair(tSWoSGByT, tS, tS, s);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cSByT, tS, tS, s);
                                    CreateAndOrAddToKeyListPair(cSWoSGByT, tS, tS, s);
                                }
                            }

                            if (tS == targetTS)
                            {
                                iterateST = false;
                                //Console.WriteLine("CommunityLib: End settlement type loop.");
                            }
                            else
                            {
                                tS = tS.BaseType;
                                //Console.WriteLine("CommunityLib: Iterating Type to " + tS.Name + ".");
                            }
                        }

                        if (s.subs != null && s.subs.Count > 0)
                        {
                            foreach (Subsettlement sub in s.subs)
                            {
                                tSub = sub.GetType();
                                iterateSsT = true;

                                CreateAndOrAddToKeyListPair(ssWoSGByTE, tSub, tSub, sub);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oSsWoSGByTE, tSub, tSub, sub);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tSsWoSGByTE, tSub, tSub, sub);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cSsWoSGByTE, tSub, tSub, sub);
                                    }
                                }

                                //Console.WriteLine("CommunityLib: Starting subsettlement type loop.");
                                while (iterateSsT)
                                {
                                    CreateAndOrAddToKeyListPair(ssByT, tSub, tSub, sub);
                                    CreateAndOrAddToKeyListPair(ssWoSGByT, tSub, tSub, sub);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oSsByT, tSub, tSub, sub);
                                        CreateAndOrAddToKeyListPair(oSsWoSGByT, tSub, tSub, sub);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tSsByT, tSub, tSub, sub);
                                        CreateAndOrAddToKeyListPair(tSsWoSGByT, tSub, tSub, sub);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cSsByT, tSub, tSub, sub);
                                            CreateAndOrAddToKeyListPair(cSsWoSGByT, tSub, tSub, sub);
                                        }
                                    }

                                    if (tSub == targetTSub)
                                    {
                                        iterateSsT = false;
                                        //Console.WriteLine("CommunityLib: End subsettlement type loop.");
                                    }
                                    else
                                    {
                                        tSub = tSub.BaseType;
                                        //Console.WriteLine("CommunityLib: Iterating Type to " + tSub.Name + ".");
                                    }
                                }
                            }
                        }
                    }

                    if (l.properties != null && l.properties.Count > 0)
                    {
                        foreach (Property p in l.properties)
                        {
                            tP = p.GetType();
                            iteratePT = true;

                            CreateAndOrAddToKeyListPair(pWoSGByTE, tP, tP, p);
                            if (l.isOcean)
                            {
                                CreateAndOrAddToKeyListPair(oPWoSGByTE, tP, tP, p);
                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(tPWoSGByTE, tP, tP, p);
                                if (l.isCoastal)
                                {
                                    CreateAndOrAddToKeyListPair(cPWoSGByTE, tP, tP, p);
                                }
                            }

                            if (s != null)
                            {

                            }
                            else
                            {
                                CreateAndOrAddToKeyListPair(pWoSByTE, tP, tP, p);
                                CreateAndOrAddToKeyListPair(pWoSG_SByTE, tP, tP, p);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oPWoSByTE, tP, tP, p);
                                    CreateAndOrAddToKeyListPair(oPWoSG_SByTE, tP, tP, p);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tPWoSByTE, tP, tP, p);
                                    CreateAndOrAddToKeyListPair(tPWoSG_SByTE, tP, tP, p);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cPWoSByTE, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(cPWoSG_SByTE, tP, tP, p);
                                    }
                                }
                            }

                            //Console.WriteLine("CommunityLib: Starting property type loop.");
                            while (iteratePT)
                            {
                                CreateAndOrAddToKeyListPair(pByT, tP, tP, p);
                                CreateAndOrAddToKeyListPair(pWoSGByT, tP, tP, p);
                                if (l.isOcean)
                                {
                                    CreateAndOrAddToKeyListPair(oLByT, tP, tP, p);
                                    CreateAndOrAddToKeyListPair(oPWoSGByT, tP, tP, p);
                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(tPByT, tP, tP, p);
                                    CreateAndOrAddToKeyListPair(tPWoSGByT, tP, tP, p);
                                    if (l.isCoastal)
                                    {
                                        CreateAndOrAddToKeyListPair(cPByT, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(cPWoSGByT, tP, tP, p);
                                    }
                                }

                                if (s != null)
                                {

                                }
                                else
                                {
                                    CreateAndOrAddToKeyListPair(pWoSByT, tP, tP, p);
                                    CreateAndOrAddToKeyListPair(pWoSG_SByT, tP, tP, p);
                                    if (l.isOcean)
                                    {
                                        CreateAndOrAddToKeyListPair(oPWoSByT, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(oPWoSG_SByT, tP, tP, p);
                                    }
                                    else
                                    {
                                        CreateAndOrAddToKeyListPair(tPWoSByT, tP, tP, p);
                                        CreateAndOrAddToKeyListPair(tPWoSG_SByT, tP, tP, p);
                                        if (l.isCoastal)
                                        {
                                            CreateAndOrAddToKeyListPair(cPWoSByT, tP, tP, p);
                                            CreateAndOrAddToKeyListPair(cPWoSG_SByT, tP, tP, p);
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
                                    tP = tP.BaseType;
                                    //Console.WriteLine("CommunityLib: Iterating Type to " + tP.Name + ".");
                                }
                            }
                        }
                    }
                }
                //Console.WriteLine("CommunityLib: End Loop for location " + l.getName() + " of Type " + l.GetType().Name + ".");
            }
            //Console.WriteLine("CommunityLib: Completed Location Processing");
        }
    }
}
