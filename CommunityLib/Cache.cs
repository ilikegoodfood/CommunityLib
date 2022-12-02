using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

namespace CommunityLib
{
    public class Cache
    {
        private List<IList> lists = new List<IList>();
        private List<IDictionary> dicts = new List<IDictionary>();

        // Social Groups //
        public Dictionary<Type, IList> socialGroupsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> socialGroupsByTypeExclusive = new Dictionary<Type, IList>();

        // Units //
        public Dictionary<Type, IList> unitsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> unitsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, List<Unit>> unitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
        public Dictionary<Type, List<Unit>> unitsBySocialGroupType = new Dictionary<Type, List<Unit>>();
        public Dictionary<Type, List<Unit>> unitsBySocialGroupTypeExclusive = new Dictionary<Type, List<Unit>>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> unitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> unitsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> unitsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> unitsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> unitsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Location, IList> unitsByLocation = new Dictionary<Location, IList>();

        // Commandable Units
        public Dictionary<Type, IList> commandableUnitsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> commandableUnitsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, List<Unit>> commandableUnitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
        public Dictionary<Type, List<Unit>> commandableUnitsBySocialGroupType = new Dictionary<Type, List<Unit>>();
        public Dictionary<Type, List<Unit>> commandableUnitsBySocialGroupTypeExclusive = new Dictionary<Type, List<Unit>>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> commandableUnitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> commandableUnitsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> commandableUnitsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> commandableUnitsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> commandableUnitsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();

        // Locations //
        public Dictionary<Type, IList> locationsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - With Social Groups
        public Dictionary<SocialGroup, IList> locationsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> locationsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> locationsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Locations - Without Social Groups
        public Dictionary<Type, IList> locationsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - Without Settlements
        public Dictionary<Type, IList> locationsWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> locationsWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Location - Without Social Groups and Settlements
        public Dictionary<Type, IList> locationsWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Settlements
        public List<Settlement> settlements = new List<Settlement>();
        public Dictionary<Type, IList> settlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - With Social Groups
        public Dictionary<SocialGroup, IList> settlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> settlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> settlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - Without Social Groups
        public Dictionary<Type, IList> settlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Specific Settlements
        public List<Set_City>[] cityByLevel = new List<Set_City>[6];
        public Dictionary<SocialGroup, List<Set_City>[]> cityBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> cityBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> cityBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_City>[]>();
        public List<Set_City>[] cityWithoutSocialGroupByLevel = new List<Set_City>[6];
        public List<Set_MinorHuman>[] minorHumanByLevel = new List<Set_MinorHuman>[6];
        public Dictionary<SocialGroup, List<Set_MinorHuman>[]> minorHumanBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> minorHumanBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> minorHumanBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public List<Set_MinorHuman>[] minorHumanWithoutSocialGroupByLevel = new List<Set_MinorHuman>[6];
        public List<Set_OrcCamp>[] orcCampBySpecialism = new List<Set_OrcCamp>[6];
        public Dictionary<SocialGroup, List<Set_OrcCamp>[]> orcCampBySocialGroupBySpecialism = new Dictionary<SocialGroup, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> orcCampBySocialGroupTypeBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> orcCampBySocialGroupTypeExclusiveBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public List<Set_OrcCamp>[] orcCampWithoutSocialGroupBySpecialism = new List<Set_OrcCamp>[6];

        // Subsettlements
        public List<Subsettlement> subsettlements = new List<Subsettlement>();
        public Dictionary<Type, IList> subsettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> subsettlementsBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - With Social Groups
        public Dictionary<SocialGroup, IList> subsettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> subsettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> subsettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> subsettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - Without Social Groups
        public Dictionary<Type, IList> subsettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> subsettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Properties
        public List<Property> properties = new List<Property>();
        public Dictionary<Type, IList> propertiesByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> propertiesBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - With Social Groups
        public Dictionary<SocialGroup, IList> propertiesBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> propertiesBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> propertiesBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> propertiesBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - Without Social Groups
        public Dictionary<Type, IList> propertiesWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Properties - Without Settlements
        public Dictionary<Type, IList> propertiesWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> propertiesWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> propertiesWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Propertes - Without Social Groups and Settlements
        public Dictionary<Type, IList> propertiesWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> propertiesWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Coastal Locations //
        public List<Location> coastalLocations = new List<Location>();
        public Dictionary<Type, IList> coastalLocationsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - With Social Groups
        public Dictionary<SocialGroup, IList> coastalLocationsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalLocationsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> coastalLocationsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalLocationsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalLocationsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalLocationsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalLocationsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Locations - Without Social Groups
        public Dictionary<Type, IList> coastalLocationsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - Without Settlements
        public Dictionary<Type, IList> coastalLocationsWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> coastalLocationsWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalLocationsWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Locations - Without Social Groups and Settlements
        public Dictionary<Type, IList> coastalLocationsWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalLocationsWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Coastal Settlements
        public List<Settlement> coastalSettlements = new List<Settlement>();
        public Dictionary<Type, IList> coastalSettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - With Social Groups
        public Dictionary<SocialGroup, IList> coastalSettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalSettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> coastalSettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - Without Social Groups
        public Dictionary<Type, IList> coastalSettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Specific Settlements
        public List<Set_City>[] coastalCityByLevel = new List<Set_City>[6];
        public Dictionary<SocialGroup, List<Set_City>[]> coastalCityBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> coastalCityBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> coastalCityBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_City>[]>();
        public List<Set_City>[] coastalCityWithoutSocialGroupByLevel = new List<Set_City>[6];
        public List<Set_MinorHuman>[] coastalMinorHumanByLevel = new List<Set_MinorHuman>[6];
        public Dictionary<SocialGroup, List<Set_MinorHuman>[]> coastalMinorHumanBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> coastalMinorHumanBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> coastalMinorHumanBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public List<Set_MinorHuman>[] coastalMinorHumanWithoutSocialGroupByLevel = new List<Set_MinorHuman>[6];
        public List<Set_OrcCamp>[] coastalOrcCampBySpecialism = new List<Set_OrcCamp>[6];
        public Dictionary<SocialGroup, List<Set_OrcCamp>[]> coastalOrcCampBySocialGroupBySpecialism = new Dictionary<SocialGroup, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> coastalOrcCampBySocialGroupTypeBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> coastalOrcCampBySocialGroupTypeExclusiveBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public List<Set_OrcCamp>[] coastalOrcCampWithoutSocialGroupBySpecialism = new List<Set_OrcCamp>[6];

        // Coastal Subsettlements
        public List<Subsettlement> coastalSubsettlements = new List<Subsettlement>();
        public Dictionary<Type, IList> coastalSubsettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> coastalSubsettlementsBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - With Social Group
        public Dictionary<SocialGroup, IList> coastalSubsettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> coastalSubsettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - Without Social Groups
        public Dictionary<Type, IList> coastalSubsettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalSubsettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Coastal Properties
        public List<Property> coastalProperties = new List<Property>();
        public Dictionary<Type, IList> coastalPropertiesByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> coastalPropertiesBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - With Social Groups
        public Dictionary<SocialGroup, IList> coastalPropertiesBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalPropertiesBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> coastalPropertiesBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> coastalPropertiesBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - Without Social Groups
        public Dictionary<Type, IList> coastalPropertiesWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Properties - Without Settlements
        public Dictionary<Type, IList> coastalPropertiesWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> coastalPropertiesWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> coastalPropertiesWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        //Properties - Without Social Groups and Settlements
        public Dictionary<Type, IList> coastalPropertiesWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> coastalPropertiesWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Ocean Locations //
        public List<Location> oceanLocations = new List<Location>();
        public Dictionary<Type, IList> oceanLocationsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - With Social Groups
        public Dictionary<SocialGroup, IList> oceanLocationsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanLocationsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> oceanLocationsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanLocationsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanLocationsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanLocationsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanLocationsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Locations - Without Social Groups
        public Dictionary<Type, IList> oceanLocationsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - Without Settlements
        public Dictionary<Type, IList> oceanLocationsWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> oceanLocationsWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanLocationsWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Location - Without Social Groups and Settlements
        public Dictionary<Type, IList> oceanLocationsWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanLocationsWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Ocean Settlements
        public List<Settlement> oceanSettlements = new List<Settlement>();
        public Dictionary<Type, IList> oceanSettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - With Social Groups
        public Dictionary<SocialGroup, IList> oceanSettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanSettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> oceanSettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - Without Social Groups
        public Dictionary<Type, IList> oceanSettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Specific Settlements
        public List<Set_City>[] oceanCityByLevel = new List<Set_City>[6];
        public Dictionary<SocialGroup, List<Set_City>[]> oceanCityBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> oceanCityBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> oceanCityBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_City>[]>();
        public List<Set_City>[] oceanCityWithoutSocialGroupByLevel = new List<Set_City>[6];
        public List<Set_MinorHuman>[] oceanMinorHumanByLevel = new List<Set_MinorHuman>[6];
        public Dictionary<SocialGroup, List<Set_MinorHuman>[]> oceanMinorHumanBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> oceanMinorHumanBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> oceanMinorHumanBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public List<Set_MinorHuman>[] oceanMinorHumanWithoutSocialGroupByLevel = new List<Set_MinorHuman>[6];
        public List<Set_OrcCamp>[] oceanOrcCampBySpecialism = new List<Set_OrcCamp>[6];
        public Dictionary<SocialGroup, List<Set_OrcCamp>[]> oceanOrcCampBySocialGroupBySpecialism = new Dictionary<SocialGroup, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> oceanOrcCampBySocialGroupTypeBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> oceanOrcCampBySocialGroupTypeExclusiveBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public List<Set_OrcCamp>[] oceanOrcCampWithoutSocialGroupBySpecialism = new List<Set_OrcCamp>[6];

        // Ocean Subsettlements
        public List<Subsettlement> oceanSubsettlements = new List<Subsettlement>();
        public Dictionary<Type, IList> oceanSubsettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> oceanSubsettlementsBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - With Social Groups
        public Dictionary<SocialGroup, IList> oceanSubsettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> oceanSubsettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - Without Social Groups
        public Dictionary<Type, IList> oceanSubsettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanSubsettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Ocean Properties
        public List<Property> oceanProperties = new List<Property>();
        public Dictionary<Type, IList> oceanPropertiesByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> oceanPropertiesBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - With Social Groups
        public Dictionary<SocialGroup, IList> oceanPropertiesBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanPropertiesBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> oceanPropertiesBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> oceanPropertiesBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - Without Social Groups
        public Dictionary<Type, IList> oceanPropertiesWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Properties - Without Settlements
        public Dictionary<Type, IList> oceanPropertiesWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> oceanPropertiesWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> oceanPropertiesWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Propertes - Without Social Groups and Settlements
        public Dictionary<Type, IList> oceanPropertiesWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> oceanPropertiesWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Terrestial Location //
        public List<Location> terrestrialLocations = new List<Location>();
        public Dictionary<Type, IList> terrestrialLocationsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - With Social Groups
        public Dictionary<SocialGroup, IList> terrestrialLocationsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialLocationsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> terrestrialLocationsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialLocationsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialLocationsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialLocationsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialLocationsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Locations - Without Social Groups
        public Dictionary<Type, IList> terrestrialLocationsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Locations - Without Settlements
        public Dictionary<Type, IList> terrestrialLocationsWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> terrestrialLocationsWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialLocationsWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Location - Without Social Groups and Settlements
        public Dictionary<Type, IList> terrestrialLocationsWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialLocationsWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Terrestrial Settlements
        public List<Settlement> terrestrialSettlements = new List<Settlement>();
        public Dictionary<Type, IList> terrestrialSettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - With Social Groups
        public Dictionary<SocialGroup, IList> terrestrialSettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> terrestrialSettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Settlements - Without Social Groups
        public Dictionary<Type, IList> terrestrialSettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Specific Settlements
        public List<Set_City>[] terrestrialCityByLevel = new List<Set_City>[6];
        public Dictionary<SocialGroup, List<Set_City>[]> terrestrialCityBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> terrestrialCityBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_City>[]>();
        public Dictionary<Type, List<Set_City>[]> terrestrialCityBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_City>[]>();
        public List<Set_City>[] terrestrialCityWithoutSocialGroupByLevel = new List<Set_City>[6];
        public List<Set_MinorHuman>[] terrestrialMinorHumanByLevel = new List<Set_MinorHuman>[6];
        public Dictionary<SocialGroup, List<Set_MinorHuman>[]> terrestrialMinorHumanBySocialGroupByLevel = new Dictionary<SocialGroup, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> terrestrialMinorHumanBySocialGroupTypeByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public Dictionary<Type, List<Set_MinorHuman>[]> terrestrialMinorHumanBySocialGroupTypeExclusiveByLevel = new Dictionary<Type, List<Set_MinorHuman>[]>();
        public List<Set_MinorHuman>[] terrestrialMinorHumanWithoutSocialGroupByLevel = new List<Set_MinorHuman>[6];
        public List<Set_OrcCamp>[] terrestrialOrcCampBySpecialism = new List<Set_OrcCamp>[6];
        public Dictionary<SocialGroup, List<Set_OrcCamp>[]> terrestrialOrcCampBySocialGroupBySpecialism = new Dictionary<SocialGroup, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> terrestrialOrcCampBySocialGroupTypeBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public Dictionary<Type, List<Set_OrcCamp>[]> terrestrialOrcCampBySocialGroupTypeExclusiveBySpecialism = new Dictionary<Type, List<Set_OrcCamp>[]>();
        public List<Set_OrcCamp>[] terrestrialOrcCampWithoutSocialGroupBySpecialism = new List<Set_OrcCamp>[6];

        // Terrestrial Subsettlements
        public List<Subsettlement> terrestrialSubsettlements = new List<Subsettlement>();
        public Dictionary<Type, IList> terrestrialSubsettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> terrestrialSubsettlementsBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - With Social Groups
        public Dictionary<SocialGroup, IList> terrestrialSubsettlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> terrestrialSubsettlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Subsettlements - Without Social Groups
        public Dictionary<Type, IList> terrestrialSubsettlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialSubsettlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Terrestrial Properties
        public List<Property> terrestrialProperties = new List<Property>();
        public Dictionary<Type, IList> terrestrialPropertiesByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesByLocationType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesByLocationTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesByLocationTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesByLocationTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesByLocationTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesByLocationTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, IList> terrestrialPropertiesBySettlementType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesBySettlementTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySettlementTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySettlementTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySettlementTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySettlementTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - With Social Groups
        public Dictionary<SocialGroup, IList> terrestrialPropertiesBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> terrestrialPropertiesBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> terrestrialPropertiesBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        // Properties - Without Social Groups
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();
        // Properties - Without Settlements
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> terrestrialPropertiesWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        // Propertes - Without Social Groups and Settlements
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSocialGroupOrSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> terrestrialPropertiesWithoutSocialGroupOrSettlementByTypeExclusive = new Dictionary<Type, IList>();

        // Distances //
        // Location Distances
        public Dictionary<Location, Dictionary<Location, double>> distanceByLocationsFromLocation = new Dictionary<Location, Dictionary<Location, double>>();
        public Dictionary<Location, List<Location>[]> locationsByStepsExclusiveFromLocation = new Dictionary<Location, List<Location>[]>();
        public Dictionary<Location, List<Location>[]> coastalLocationsByStepsExclusiveFromLocation = new Dictionary<Location, List<Location>[]>();
        public Dictionary<Location, List<Location>[]> oceanLocationsByStepsExclusiveFromLocation = new Dictionary<Location, List<Location>[]>();
        public Dictionary<Location, List<Location>[]> terrestrialLocationsByStepsExclusiveFromLocation = new Dictionary<Location, List<Location>[]>();
        // Settlement Distances
        public Dictionary<Location, List<Settlement>[]> settlementsByStepsExclusiveFromLocation = new Dictionary<Location, List<Settlement>[]>();
        public Dictionary<Location, List<Settlement>[]> coastalSettlementsByStepsExclusiveFromLocation = new Dictionary<Location, List<Settlement>[]>();
        public Dictionary<Location, List<Settlement>[]> oceanSettlementsByStepsExclusiveFromLocation = new Dictionary<Location, List<Settlement>[]>();
        public Dictionary<Location, List<Settlement>[]> terrestrialSettlementsByStepsExclusiveFromLocation = new Dictionary<Location, List<Settlement>[]>();
        // Unit Distances
        public Dictionary<Location, List<Unit>[]> unitsByStepsExclusiveFromLocation = new Dictionary<Location, List<Unit>[]>();

        // Visibility
        public Dictionary<Unit, IList> visibleUnitsByUnit = new Dictionary<Unit, IList>();
        public Dictionary<Unit, IList> unitVisibleToUnits = new Dictionary<Unit, IList>();
        public Dictionary<Unit, Location> commandableUnitLocations = new Dictionary<Unit, Location>();


        public Cache()
        {
            InitializeCache();
        }

        public void ClearCache()
        {
            foreach (IList list in lists)
            {
                list.Clear();
            }
            foreach (IDictionary dict in dicts)
            {
                dict.Clear();
            }

            Array.Clear(cityByLevel, 0, cityByLevel.Length);
            Array.Clear(cityWithoutSocialGroupByLevel, 0, cityWithoutSocialGroupByLevel.Length);
            Array.Clear(minorHumanByLevel, 0, minorHumanByLevel.Length);
            Array.Clear(minorHumanWithoutSocialGroupByLevel, 0, minorHumanWithoutSocialGroupByLevel.Length);
            Array.Clear(orcCampBySpecialism, 0, orcCampBySpecialism.Length);
            Array.Clear(orcCampWithoutSocialGroupBySpecialism, 0, orcCampWithoutSocialGroupBySpecialism.Length);
            Array.Clear(coastalCityByLevel, 0, coastalCityByLevel.Length);
            Array.Clear(coastalCityWithoutSocialGroupByLevel, 0, coastalCityWithoutSocialGroupByLevel.Length);
            Array.Clear(coastalMinorHumanByLevel, 0, coastalMinorHumanByLevel.Length);
            Array.Clear(coastalMinorHumanWithoutSocialGroupByLevel, 0, coastalMinorHumanWithoutSocialGroupByLevel.Length);
            Array.Clear(coastalOrcCampBySpecialism, 0, coastalOrcCampBySpecialism.Length);
            Array.Clear(coastalOrcCampWithoutSocialGroupBySpecialism, 0, coastalOrcCampWithoutSocialGroupBySpecialism.Length);
            Array.Clear(oceanCityByLevel, 0, oceanCityByLevel.Length);
            Array.Clear(oceanCityWithoutSocialGroupByLevel, 0, oceanCityWithoutSocialGroupByLevel.Length);
            Array.Clear(oceanMinorHumanByLevel, 0, oceanMinorHumanByLevel.Length);
            Array.Clear(oceanMinorHumanWithoutSocialGroupByLevel, 0, oceanMinorHumanWithoutSocialGroupByLevel.Length);
            Array.Clear(oceanOrcCampBySpecialism, 0, oceanOrcCampBySpecialism.Length);
            Array.Clear(oceanOrcCampWithoutSocialGroupBySpecialism, 0, oceanOrcCampWithoutSocialGroupBySpecialism.Length);
            Array.Clear(terrestrialCityByLevel, 0, terrestrialCityByLevel.Length);
            Array.Clear(terrestrialCityWithoutSocialGroupByLevel, 0, terrestrialCityWithoutSocialGroupByLevel.Length);
            Array.Clear(terrestrialMinorHumanByLevel, 0, terrestrialMinorHumanByLevel.Length);
            Array.Clear(terrestrialMinorHumanWithoutSocialGroupByLevel, 0, terrestrialMinorHumanWithoutSocialGroupByLevel.Length);
            Array.Clear(terrestrialOrcCampBySpecialism, 0, terrestrialOrcCampBySpecialism.Length);
            Array.Clear(terrestrialOrcCampWithoutSocialGroupBySpecialism, 0, terrestrialOrcCampWithoutSocialGroupBySpecialism.Length);
        }

        private void InitializeCache()
        {
            //Initialize List and Dictionary Coillections
            lists = new List<IList>();
            dicts = new List<IDictionary>();

            // === Initialization === //
            // Social Groups //
            dicts.Add(socialGroupsByType);
            dicts.Add(socialGroupsByTypeExclusive);

            // Units //
            dicts.Add(unitsByType);
            dicts.Add(unitsByTypeExclusive);
            dicts.Add(unitsBySocialGroup);
            dicts.Add(unitsBySocialGroupType);
            dicts.Add(unitsBySocialGroupTypeExclusive);
            dicts.Add(unitsBySocialGroupByType);
            dicts.Add(unitsBySocialGroupTypeByType);
            dicts.Add(unitsBySocialGroupTypeExclusiveByType);
            dicts.Add(unitsBySocialGroupTypeByTypeExclusive);
            dicts.Add(unitsBySocialGroupTypeExclusiveByTypeExclusive);
            dicts.Add(unitsByLocation);
            // Commandable Units
            dicts.Add(commandableUnitsByType);
            dicts.Add(commandableUnitsByTypeExclusive);
            dicts.Add(commandableUnitsBySocialGroup);
            dicts.Add(commandableUnitsBySocialGroupType);
            dicts.Add(commandableUnitsBySocialGroupTypeExclusive);
            dicts.Add(commandableUnitsBySocialGroupByType);
            dicts.Add(commandableUnitsBySocialGroupTypeByType);
            dicts.Add(commandableUnitsBySocialGroupTypeExclusiveByType);
            dicts.Add(commandableUnitsBySocialGroupTypeByTypeExclusive);
            dicts.Add(commandableUnitsBySocialGroupTypeExclusiveByTypeExclusive);

            // Locations //
            dicts.Add(locationsByType);
            dicts.Add(locationsByTypeExclusive);
            // Locations - With Social Groups
            dicts.Add(locationsBySocialGroup);
            dicts.Add(locationsBySocialGroupType);
            dicts.Add(locationsBySocialGroupTypeExclusive);
            dicts.Add(locationsBySocialGroupByType);
            dicts.Add(locationsBySocialGroupTypeByType);
            dicts.Add(locationsBySocialGroupTypeExclusiveByType);
            dicts.Add(locationsBySocialGroupTypeByTypeExclusive);
            dicts.Add(locationsBySocialGroupTypeExclusiveByTypeExclusive);
            // Locations - Without Social Groups
            dicts.Add(locationsWithoutSocialGroupByType);
            dicts.Add(locationsWithoutSocialGroupByTypeExclusive);
            // Locations - Without Settlements
            dicts.Add(locationsWithoutSettlementByType);
            dicts.Add(locationsWithoutSettlementByTypeExclusive);
            dicts.Add(locationsWithoutSettlementBySocialGroup);
            dicts.Add(locationsWithoutSettlementBySocialGroupType);
            dicts.Add(locationsWithoutSettlementBySocialGroupTypeExclusive);
            // Location - Without Social Groups and Settlements
            dicts.Add(locationsWithoutSocialGroupOrSettlementByType);
            dicts.Add(locationsWithoutSocialGroupOrSettlementByTypeExclusive);
            // Settlements
            lists.Add(settlements);
            dicts.Add(settlementsByType);
            dicts.Add(settlementsByTypeExclusive);
            dicts.Add(settlementsByLocationType);
            dicts.Add(settlementsByLocationTypeExclusive);
            dicts.Add(settlementsByLocationTypeByType);
            dicts.Add(settlementsByLocationTypeExclusiveByType);
            dicts.Add(settlementsByLocationTypeByTypeExclusive);
            dicts.Add(settlementsByLocationTypeExclusiveByTypeExclusive);
            // Settlements - With Social Groups
            dicts.Add(settlementsBySocialGroup);
            dicts.Add(settlementsBySocialGroupType);
            dicts.Add(settlementsBySocialGroupTypeExclusive);
            dicts.Add(settlementsBySocialGroupByType);
            dicts.Add(settlementsBySocialGroupTypeByType);
            dicts.Add(settlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(settlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(settlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Settlements - Without Social Groups
            dicts.Add(settlementsWithoutSocialGroupByType);
            dicts.Add(settlementsWithoutSocialGroupByTypeExclusive);
            // Specific Settlements
            dicts.Add(cityBySocialGroupByLevel);
            dicts.Add(cityBySocialGroupTypeByLevel);
            dicts.Add(cityBySocialGroupTypeExclusiveByLevel);
            dicts.Add(minorHumanBySocialGroupByLevel);
            dicts.Add(minorHumanBySocialGroupTypeByLevel);
            dicts.Add(minorHumanBySocialGroupTypeExclusiveByLevel);
            dicts.Add(orcCampBySocialGroupBySpecialism);
            dicts.Add(orcCampBySocialGroupTypeBySpecialism);
            dicts.Add(orcCampBySocialGroupTypeExclusiveBySpecialism);
            // Subsettlements
            lists.Add(subsettlements);
            dicts.Add(subsettlementsByType);
            dicts.Add(subsettlementsByTypeExclusive);
            dicts.Add(subsettlementsByLocationType);
            dicts.Add(subsettlementsByLocationTypeExclusive);
            dicts.Add(subsettlementsByLocationTypeByType);
            dicts.Add(subsettlementsByLocationTypeExclusiveByType);
            dicts.Add(subsettlementsByLocationTypeByTypeExclusive);
            dicts.Add(subsettlementsByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(subsettlementsBySettlementType);
            dicts.Add(subsettlementsBySettlementTypeExclusive);
            dicts.Add(subsettlementsBySettlementTypeByType);
            dicts.Add(subsettlementsBySettlementTypeExclusiveByType);
            dicts.Add(subsettlementsBySettlementTypeByTypeExclusive);
            dicts.Add(subsettlementsBySettlementTypeExclusiveByTypeExclusive);
            // Subsettlements - With Social Groups
            dicts.Add(subsettlementsBySocialGroup);
            dicts.Add(subsettlementsBySocialGroupType);
            dicts.Add(subsettlementsBySocialGroupTypeExclusive);
            dicts.Add(subsettlementsBySocialGroupByType);
            dicts.Add(subsettlementsBySocialGroupTypeByType);
            dicts.Add(subsettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(subsettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(subsettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Subsettlements - Without Social Groups
            dicts.Add(subsettlementsWithoutSocialGroupByType);
            dicts.Add(subsettlementsWithoutSocialGroupByTypeExclusive);
            // Properties
            lists.Add(properties);
            dicts.Add(propertiesByType);
            dicts.Add(propertiesByTypeExclusive);
            dicts.Add(propertiesByLocationType);
            dicts.Add(propertiesByLocationTypeExclusive);
            dicts.Add(propertiesByLocationTypeByType);
            dicts.Add(propertiesByLocationTypeExclusiveByType);
            dicts.Add(propertiesByLocationTypeByTypeExclusive);
            dicts.Add(propertiesByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(propertiesBySettlementType);
            dicts.Add(propertiesBySettlementTypeExclusive);
            dicts.Add(propertiesBySettlementTypeByType);
            dicts.Add(propertiesBySettlementTypeExclusiveByType);
            dicts.Add(propertiesBySettlementTypeByTypeExclusive);
            dicts.Add(propertiesBySettlementTypeExclusiveByTypeExclusive);
            // Properties - With Social Groups
            dicts.Add(propertiesBySocialGroup);
            dicts.Add(propertiesBySocialGroupType);
            dicts.Add(propertiesBySocialGroupTypeExclusive);
            dicts.Add(propertiesBySocialGroupByType);
            dicts.Add(propertiesBySocialGroupTypeByType);
            dicts.Add(propertiesBySocialGroupTypeExclusiveByType);
            dicts.Add(propertiesBySocialGroupTypeByTypeExclusive);
            dicts.Add(propertiesBySocialGroupTypeExclusiveByTypeExclusive);
            // Properties - Without Social Groups
            dicts.Add(propertiesWithoutSocialGroupByType);
            dicts.Add(propertiesWithoutSocialGroupByTypeExclusive);
            // Properties - Without Settlements
            dicts.Add(propertiesWithoutSettlementByType);
            dicts.Add(propertiesWithoutSettlementByTypeExclusive);
            dicts.Add(propertiesWithoutSettlementBySocialGroup);
            dicts.Add(propertiesWithoutSettlementBySocialGroupType);
            dicts.Add(propertiesWithoutSettlementBySocialGroupTypeExclusive);
            // Propertes - Without Social Groups and Settlements
            dicts.Add(propertiesWithoutSocialGroupOrSettlementByType);
            dicts.Add(propertiesWithoutSocialGroupOrSettlementByTypeExclusive);

            // Coastal Locations //
            lists.Add(coastalLocations);
            dicts.Add(coastalLocationsByType);
            dicts.Add(coastalLocationsByTypeExclusive);
            // Locations - With Social Groups
            dicts.Add(coastalLocationsBySocialGroup);
            dicts.Add(coastalLocationsBySocialGroupType);
            dicts.Add(coastalLocationsBySocialGroupTypeExclusive);
            dicts.Add(coastalLocationsBySocialGroupByType);
            dicts.Add(coastalLocationsBySocialGroupTypeByType);
            dicts.Add(coastalLocationsBySocialGroupTypeExclusiveByType);
            dicts.Add(coastalLocationsBySocialGroupTypeByTypeExclusive);
            dicts.Add(coastalLocationsBySocialGroupTypeExclusiveByTypeExclusive);
            // Locations - Without Social Groups
            dicts.Add(coastalLocationsWithoutSocialGroupByType);
            dicts.Add(coastalLocationsWithoutSocialGroupByTypeExclusive);
            // Locations - Without Settlements
            dicts.Add(coastalLocationsWithoutSettlementByType);
            dicts.Add(coastalLocationsWithoutSettlementByTypeExclusive);
            dicts.Add(coastalLocationsWithoutSettlementBySocialGroup);
            dicts.Add(coastalLocationsWithoutSettlementBySocialGroupType);
            dicts.Add(coastalLocationsWithoutSettlementBySocialGroupTypeExclusive);
            // Location - Without Social Groups and Settlements
            dicts.Add(coastalLocationsWithoutSocialGroupOrSettlementByType);
            dicts.Add(coastalLocationsWithoutSocialGroupOrSettlementByTypeExclusive);
            // Coastal Settlements
            lists.Add(coastalSettlements);
            dicts.Add(coastalSettlementsByType);
            dicts.Add(coastalSettlementsByTypeExclusive);
            dicts.Add(coastalSettlementsByLocationType);
            dicts.Add(coastalSettlementsByLocationTypeExclusive);
            dicts.Add(coastalSettlementsByLocationTypeByType);
            dicts.Add(coastalSettlementsByLocationTypeExclusiveByType);
            dicts.Add(coastalSettlementsByLocationTypeByTypeExclusive);
            dicts.Add(coastalSettlementsByLocationTypeExclusiveByTypeExclusive);
            // Settlements - With Social Groups
            dicts.Add(coastalSettlementsBySocialGroup);
            dicts.Add(coastalSettlementsBySocialGroupType);
            dicts.Add(coastalSettlementsBySocialGroupTypeExclusive);
            dicts.Add(coastalSettlementsBySocialGroupByType);
            dicts.Add(coastalSettlementsBySocialGroupTypeByType);
            dicts.Add(coastalSettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(coastalSettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(coastalSettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Settlements - Without Social Groups
            dicts.Add(coastalSettlementsWithoutSocialGroupByType);
            dicts.Add(coastalSettlementsWithoutSocialGroupByTypeExclusive);
            // Specific Settlements
            dicts.Add(coastalCityBySocialGroupByLevel);
            dicts.Add(coastalCityBySocialGroupTypeByLevel);
            dicts.Add(coastalCityBySocialGroupTypeExclusiveByLevel);
            dicts.Add(coastalMinorHumanBySocialGroupByLevel);
            dicts.Add(coastalMinorHumanBySocialGroupTypeByLevel);
            dicts.Add(coastalMinorHumanBySocialGroupTypeExclusiveByLevel);
            dicts.Add(coastalOrcCampBySocialGroupBySpecialism);
            dicts.Add(coastalOrcCampBySocialGroupTypeBySpecialism);
            dicts.Add(coastalOrcCampBySocialGroupTypeExclusiveBySpecialism);
            // Coastal Subsettlements
            lists.Add(coastalSubsettlements);
            dicts.Add(coastalSubsettlementsByType);
            dicts.Add(coastalSubsettlementsByTypeExclusive);
            dicts.Add(coastalSubsettlementsByLocationType);
            dicts.Add(coastalSubsettlementsByLocationTypeExclusive);
            dicts.Add(coastalSubsettlementsByLocationTypeByType);
            dicts.Add(coastalSubsettlementsByLocationTypeExclusiveByType);
            dicts.Add(coastalSubsettlementsByLocationTypeByTypeExclusive);
            dicts.Add(coastalSubsettlementsByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(coastalSubsettlementsBySettlementType);
            dicts.Add(coastalSubsettlementsBySettlementTypeExclusive);
            dicts.Add(coastalSubsettlementsBySettlementTypeByType);
            dicts.Add(coastalSubsettlementsBySettlementTypeExclusiveByType);
            dicts.Add(coastalSubsettlementsBySettlementTypeByTypeExclusive);
            dicts.Add(coastalSubsettlementsBySettlementTypeExclusiveByTypeExclusive);
            // Subsettlements - With Social Groups
            dicts.Add(coastalSubsettlementsBySocialGroup);
            dicts.Add(coastalSubsettlementsBySocialGroupType);
            dicts.Add(coastalSubsettlementsBySocialGroupTypeExclusive);
            dicts.Add(coastalSubsettlementsBySocialGroupByType);
            dicts.Add(coastalSubsettlementsBySocialGroupTypeByType);
            dicts.Add(coastalSubsettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(coastalSubsettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(coastalSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Subsettlements - Without Social Groups
            dicts.Add(coastalSubsettlementsWithoutSocialGroupByType);
            dicts.Add(coastalSubsettlementsWithoutSocialGroupByTypeExclusive);
            // Coastal Properties
            lists.Add(coastalProperties);
            dicts.Add(coastalPropertiesByType);
            dicts.Add(coastalPropertiesByTypeExclusive);
            dicts.Add(coastalPropertiesByLocationType);
            dicts.Add(coastalPropertiesByLocationTypeExclusive);
            dicts.Add(coastalPropertiesByLocationTypeByType);
            dicts.Add(coastalPropertiesByLocationTypeExclusiveByType);
            dicts.Add(coastalPropertiesByLocationTypeByTypeExclusive);
            dicts.Add(coastalPropertiesByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(coastalPropertiesBySettlementType);
            dicts.Add(coastalPropertiesBySettlementTypeExclusive);
            dicts.Add(coastalPropertiesBySettlementTypeByType);
            dicts.Add(coastalPropertiesBySettlementTypeExclusiveByType);
            dicts.Add(coastalPropertiesBySettlementTypeByTypeExclusive);
            dicts.Add(coastalPropertiesBySettlementTypeExclusiveByTypeExclusive);
            // Properties - With Social Groups
            dicts.Add(coastalPropertiesBySocialGroup);
            dicts.Add(coastalPropertiesBySocialGroupType);
            dicts.Add(coastalPropertiesBySocialGroupTypeExclusive);
            dicts.Add(coastalPropertiesBySocialGroupByType);
            dicts.Add(coastalPropertiesBySocialGroupTypeByType);
            dicts.Add(coastalPropertiesBySocialGroupTypeExclusiveByType);
            dicts.Add(coastalPropertiesBySocialGroupTypeByTypeExclusive);
            dicts.Add(coastalPropertiesBySocialGroupTypeExclusiveByTypeExclusive);
            // Properties - Without Social Groups
            dicts.Add(coastalPropertiesWithoutSocialGroupByType);
            dicts.Add(coastalPropertiesWithoutSocialGroupByTypeExclusive);
            // Properties - Without Settlements
            dicts.Add(coastalPropertiesWithoutSettlementByType);
            dicts.Add(coastalPropertiesWithoutSettlementByTypeExclusive);
            dicts.Add(coastalPropertiesWithoutSettlementBySocialGroup);
            dicts.Add(coastalPropertiesWithoutSettlementBySocialGroupType);
            dicts.Add(coastalPropertiesWithoutSettlementBySocialGroupTypeExclusive);
            // Propertes - Without Social Groups and Settlements
            dicts.Add(coastalPropertiesWithoutSocialGroupOrSettlementByType);
            dicts.Add(coastalPropertiesWithoutSocialGroupOrSettlementByTypeExclusive);

            // Ocean Locations //
            lists.Add(oceanLocations);
            dicts.Add(oceanLocationsByType);
            dicts.Add(oceanLocationsByTypeExclusive);
            // Locations - With Social Groups
            dicts.Add(oceanLocationsBySocialGroup);
            dicts.Add(oceanLocationsBySocialGroupType);
            dicts.Add(oceanLocationsBySocialGroupTypeExclusive);
            dicts.Add(oceanLocationsBySocialGroupByType);
            dicts.Add(oceanLocationsBySocialGroupTypeByType);
            dicts.Add(oceanLocationsBySocialGroupTypeExclusiveByType);
            dicts.Add(oceanLocationsBySocialGroupTypeByTypeExclusive);
            dicts.Add(oceanLocationsBySocialGroupTypeExclusiveByTypeExclusive);
            // Locations - Without Social Groups
            dicts.Add(oceanLocationsWithoutSocialGroupByType);
            dicts.Add(oceanLocationsWithoutSocialGroupByTypeExclusive);
            // Locations - Without Settlements
            dicts.Add(oceanLocationsWithoutSettlementByType);
            dicts.Add(oceanLocationsWithoutSettlementByTypeExclusive);
            dicts.Add(oceanLocationsWithoutSettlementBySocialGroup);
            dicts.Add(oceanLocationsWithoutSettlementBySocialGroupType);
            dicts.Add(oceanLocationsWithoutSettlementBySocialGroupTypeExclusive);
            // Location - Without Social Groups and Settlements
            dicts.Add(oceanLocationsWithoutSocialGroupOrSettlementByType);
            dicts.Add(oceanLocationsWithoutSocialGroupOrSettlementByTypeExclusive);
            // Ocean Settlements
            lists.Add(oceanSettlements);
            dicts.Add(oceanSettlementsByType);
            dicts.Add(oceanSettlementsByTypeExclusive);
            dicts.Add(oceanSettlementsByLocationType);
            dicts.Add(oceanSettlementsByLocationTypeExclusive);
            dicts.Add(oceanSettlementsByLocationTypeByType);
            dicts.Add(oceanSettlementsByLocationTypeExclusiveByType);
            dicts.Add(oceanSettlementsByLocationTypeByTypeExclusive);
            dicts.Add(oceanSettlementsByLocationTypeExclusiveByTypeExclusive);
            // Settlements - With Social Groups
            dicts.Add(oceanSettlementsBySocialGroup);
            dicts.Add(oceanSettlementsBySocialGroupType);
            dicts.Add(oceanSettlementsBySocialGroupTypeExclusive);
            dicts.Add(oceanSettlementsBySocialGroupByType);
            dicts.Add(oceanSettlementsBySocialGroupTypeByType);
            dicts.Add(oceanSettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(oceanSettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(oceanSettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Settlements - Without Social Groups
            dicts.Add(oceanSettlementsWithoutSocialGroupByType);
            dicts.Add(oceanSettlementsWithoutSocialGroupByTypeExclusive);
            // Specific Settlements
            dicts.Add(oceanCityBySocialGroupByLevel);
            dicts.Add(oceanCityBySocialGroupTypeByLevel);
            dicts.Add(oceanCityBySocialGroupTypeExclusiveByLevel);
            dicts.Add(oceanMinorHumanBySocialGroupByLevel);
            dicts.Add(oceanMinorHumanBySocialGroupTypeByLevel);
            dicts.Add(oceanMinorHumanBySocialGroupTypeExclusiveByLevel);
            dicts.Add(oceanOrcCampBySocialGroupBySpecialism);
            dicts.Add(oceanOrcCampBySocialGroupTypeBySpecialism);
            dicts.Add(oceanOrcCampBySocialGroupTypeExclusiveBySpecialism);
            // Ocean Subsettlements
            lists.Add(oceanSubsettlements);
            dicts.Add(oceanSubsettlementsByType);
            dicts.Add(oceanSubsettlementsByTypeExclusive);
            dicts.Add(oceanSubsettlementsByLocationType);
            dicts.Add(oceanSubsettlementsByLocationTypeExclusive);
            dicts.Add(oceanSubsettlementsByLocationTypeByType);
            dicts.Add(oceanSubsettlementsByLocationTypeExclusiveByType);
            dicts.Add(oceanSubsettlementsByLocationTypeByTypeExclusive);
            dicts.Add(oceanSubsettlementsByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(oceanSubsettlementsBySettlementType);
            dicts.Add(oceanSubsettlementsBySettlementTypeExclusive);
            dicts.Add(oceanSubsettlementsBySettlementTypeByType);
            dicts.Add(oceanSubsettlementsBySettlementTypeExclusiveByType);
            dicts.Add(oceanSubsettlementsBySettlementTypeByTypeExclusive);
            dicts.Add(oceanSubsettlementsBySettlementTypeExclusiveByTypeExclusive);
            // Subsettlements - With Social Groups
            dicts.Add(oceanSubsettlementsBySocialGroup);
            dicts.Add(oceanSubsettlementsBySocialGroupType);
            dicts.Add(oceanSubsettlementsBySocialGroupTypeExclusive);
            dicts.Add(oceanSubsettlementsBySocialGroupByType);
            dicts.Add(oceanSubsettlementsBySocialGroupTypeByType);
            dicts.Add(oceanSubsettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(oceanSubsettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(oceanSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Subsettlements - Without Social Groups
            dicts.Add(oceanSubsettlementsWithoutSocialGroupByType);
            dicts.Add(oceanSubsettlementsWithoutSocialGroupByTypeExclusive);
            // Ocean Properties
            lists.Add(oceanProperties);
            dicts.Add(oceanPropertiesByType);
            dicts.Add(oceanPropertiesByTypeExclusive);
            dicts.Add(oceanPropertiesByLocationType);
            dicts.Add(oceanPropertiesByLocationTypeExclusive);
            dicts.Add(oceanPropertiesByLocationTypeByType);
            dicts.Add(oceanPropertiesByLocationTypeExclusiveByType);
            dicts.Add(oceanPropertiesByLocationTypeByTypeExclusive);
            dicts.Add(oceanPropertiesByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(oceanPropertiesBySettlementType);
            dicts.Add(oceanPropertiesBySettlementTypeExclusive);
            dicts.Add(oceanPropertiesBySettlementTypeByType);
            dicts.Add(oceanPropertiesBySettlementTypeExclusiveByType);
            dicts.Add(oceanPropertiesBySettlementTypeByTypeExclusive);
            dicts.Add(oceanPropertiesBySettlementTypeExclusiveByTypeExclusive);
            // Properties - With Social Groups
            dicts.Add(oceanPropertiesBySocialGroup);
            dicts.Add(oceanPropertiesBySocialGroupType);
            dicts.Add(oceanPropertiesBySocialGroupTypeExclusive);
            dicts.Add(oceanPropertiesBySocialGroupByType);
            dicts.Add(oceanPropertiesBySocialGroupTypeByType);
            dicts.Add(oceanPropertiesBySocialGroupTypeExclusiveByType);
            dicts.Add(oceanPropertiesBySocialGroupTypeByTypeExclusive);
            dicts.Add(oceanPropertiesBySocialGroupTypeExclusiveByTypeExclusive);
            // Properties - Without Social Groups
            dicts.Add(oceanPropertiesWithoutSocialGroupByType);
            dicts.Add(oceanPropertiesWithoutSocialGroupByTypeExclusive);
            // Properties - Without Settlements
            dicts.Add(oceanPropertiesWithoutSettlementByType);
            dicts.Add(oceanPropertiesWithoutSettlementByTypeExclusive);
            dicts.Add(oceanPropertiesWithoutSettlementBySocialGroup);
            dicts.Add(oceanPropertiesWithoutSettlementBySocialGroupType);
            dicts.Add(oceanPropertiesWithoutSettlementBySocialGroupTypeExclusive);
            // Propertes - Without Social Groups and Settlements
            dicts.Add(oceanPropertiesWithoutSocialGroupOrSettlementByType);
            dicts.Add(oceanPropertiesWithoutSocialGroupOrSettlementByTypeExclusive);

            // Terrestrial Locations //
            lists.Add(terrestrialLocations);
            dicts.Add(terrestrialLocationsByType);
            dicts.Add(terrestrialLocationsByTypeExclusive);
            // Locations - With Social Groups
            dicts.Add(terrestrialLocationsBySocialGroup);
            dicts.Add(terrestrialLocationsBySocialGroupType);
            dicts.Add(terrestrialLocationsBySocialGroupTypeExclusive);
            dicts.Add(terrestrialLocationsBySocialGroupByType);
            dicts.Add(terrestrialLocationsBySocialGroupTypeByType);
            dicts.Add(terrestrialLocationsBySocialGroupTypeExclusiveByType);
            dicts.Add(terrestrialLocationsBySocialGroupTypeByTypeExclusive);
            dicts.Add(terrestrialLocationsBySocialGroupTypeExclusiveByTypeExclusive);
            // Locations - Without Social Groups
            dicts.Add(terrestrialLocationsWithoutSocialGroupByType);
            dicts.Add(terrestrialLocationsWithoutSocialGroupByTypeExclusive);
            // Locations - Without Settlements
            dicts.Add(terrestrialLocationsWithoutSettlementByType);
            dicts.Add(terrestrialLocationsWithoutSettlementByTypeExclusive);
            dicts.Add(terrestrialLocationsWithoutSettlementBySocialGroup);
            dicts.Add(terrestrialLocationsWithoutSettlementBySocialGroupType);
            dicts.Add(terrestrialLocationsWithoutSettlementBySocialGroupTypeExclusive);
            // Location - Without Social Groups and Settlements
            dicts.Add(terrestrialLocationsWithoutSocialGroupOrSettlementByType);
            dicts.Add(terrestrialLocationsWithoutSocialGroupOrSettlementByTypeExclusive);
            // Terrestrial Settlements
            lists.Add(terrestrialSettlements);
            dicts.Add(terrestrialSettlementsByType);
            dicts.Add(terrestrialSettlementsByTypeExclusive);
            dicts.Add(terrestrialSettlementsByLocationType);
            dicts.Add(terrestrialSettlementsByLocationTypeExclusive);
            dicts.Add(terrestrialSettlementsByLocationTypeByType);
            dicts.Add(terrestrialSettlementsByLocationTypeExclusiveByType);
            dicts.Add(terrestrialSettlementsByLocationTypeByTypeExclusive);
            dicts.Add(terrestrialSettlementsByLocationTypeExclusiveByTypeExclusive);
            // Settlements - With Social Groups
            dicts.Add(terrestrialSettlementsBySocialGroup);
            dicts.Add(terrestrialSettlementsBySocialGroupType);
            dicts.Add(terrestrialSettlementsBySocialGroupTypeExclusive);
            dicts.Add(terrestrialSettlementsBySocialGroupByType);
            dicts.Add(terrestrialSettlementsBySocialGroupTypeByType);
            dicts.Add(terrestrialSettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(terrestrialSettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(terrestrialSettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Settlements - Without Social Groups
            dicts.Add(terrestrialSettlementsWithoutSocialGroupByType);
            dicts.Add(terrestrialSettlementsWithoutSocialGroupByTypeExclusive);
            // Specific Settlements
            dicts.Add(terrestrialCityBySocialGroupByLevel);
            dicts.Add(terrestrialCityBySocialGroupTypeByLevel);
            dicts.Add(terrestrialCityBySocialGroupTypeExclusiveByLevel);
            dicts.Add(terrestrialMinorHumanBySocialGroupByLevel);
            dicts.Add(terrestrialMinorHumanBySocialGroupTypeByLevel);
            dicts.Add(terrestrialMinorHumanBySocialGroupTypeExclusiveByLevel);
            dicts.Add(terrestrialOrcCampBySocialGroupBySpecialism);
            dicts.Add(terrestrialOrcCampBySocialGroupTypeBySpecialism);
            dicts.Add(terrestrialOrcCampBySocialGroupTypeExclusiveBySpecialism);
            // Terrestrial Subsettlements
            lists.Add(terrestrialSubsettlements);
            dicts.Add(terrestrialSubsettlementsByType);
            dicts.Add(terrestrialSubsettlementsByTypeExclusive);
            dicts.Add(terrestrialSubsettlementsByLocationType);
            dicts.Add(terrestrialSubsettlementsByLocationTypeExclusive);
            dicts.Add(terrestrialSubsettlementsByLocationTypeByType);
            dicts.Add(terrestrialSubsettlementsByLocationTypeExclusiveByType);
            dicts.Add(terrestrialSubsettlementsByLocationTypeByTypeExclusive);
            dicts.Add(terrestrialSubsettlementsByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(terrestrialSubsettlementsBySettlementType);
            dicts.Add(terrestrialSubsettlementsBySettlementTypeExclusive);
            dicts.Add(terrestrialSubsettlementsBySettlementTypeByType);
            dicts.Add(terrestrialSubsettlementsBySettlementTypeExclusiveByType);
            dicts.Add(terrestrialSubsettlementsBySettlementTypeByTypeExclusive);
            dicts.Add(terrestrialSubsettlementsBySettlementTypeExclusiveByTypeExclusive);
            // Subsettlements - With Social Groups
            dicts.Add(terrestrialSubsettlementsBySocialGroup);
            dicts.Add(terrestrialSubsettlementsBySocialGroupType);
            dicts.Add(terrestrialSubsettlementsBySocialGroupTypeExclusive);
            dicts.Add(terrestrialSubsettlementsBySocialGroupByType);
            dicts.Add(terrestrialSubsettlementsBySocialGroupTypeByType);
            dicts.Add(terrestrialSubsettlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(terrestrialSubsettlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(terrestrialSubsettlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Subsettlements - Without Social Groups
            dicts.Add(terrestrialSubsettlementsWithoutSocialGroupByType);
            dicts.Add(terrestrialSubsettlementsWithoutSocialGroupByTypeExclusive);
            // Terrestrial Properties
            lists.Add(terrestrialProperties);
            dicts.Add(terrestrialPropertiesByType);
            dicts.Add(terrestrialPropertiesByTypeExclusive);
            dicts.Add(terrestrialPropertiesByLocationType);
            dicts.Add(terrestrialPropertiesByLocationTypeExclusive);
            dicts.Add(terrestrialPropertiesByLocationTypeByType);
            dicts.Add(terrestrialPropertiesByLocationTypeExclusiveByType);
            dicts.Add(terrestrialPropertiesByLocationTypeByTypeExclusive);
            dicts.Add(terrestrialPropertiesByLocationTypeExclusiveByTypeExclusive);
            dicts.Add(terrestrialPropertiesBySettlementType);
            dicts.Add(terrestrialPropertiesBySettlementTypeExclusive);
            dicts.Add(terrestrialPropertiesBySettlementTypeByType);
            dicts.Add(terrestrialPropertiesBySettlementTypeExclusiveByType);
            dicts.Add(terrestrialPropertiesBySettlementTypeByTypeExclusive);
            dicts.Add(terrestrialPropertiesBySettlementTypeExclusiveByTypeExclusive);
            // Properties - With Social Groups
            dicts.Add(terrestrialPropertiesBySocialGroup);
            dicts.Add(terrestrialPropertiesBySocialGroupType);
            dicts.Add(terrestrialPropertiesBySocialGroupTypeExclusive);
            dicts.Add(terrestrialPropertiesBySocialGroupByType);
            dicts.Add(terrestrialPropertiesBySocialGroupTypeByType);
            dicts.Add(terrestrialPropertiesBySocialGroupTypeExclusiveByType);
            dicts.Add(terrestrialPropertiesBySocialGroupTypeByTypeExclusive);
            dicts.Add(terrestrialPropertiesBySocialGroupTypeExclusiveByTypeExclusive);
            // Properties - Without Social Groups
            dicts.Add(terrestrialPropertiesWithoutSocialGroupByType);
            dicts.Add(terrestrialPropertiesWithoutSocialGroupByTypeExclusive);
            // Properties - Without Settlements
            dicts.Add(terrestrialPropertiesWithoutSettlementByType);
            dicts.Add(terrestrialPropertiesWithoutSettlementByTypeExclusive);
            dicts.Add(terrestrialPropertiesWithoutSettlementBySocialGroup);
            dicts.Add(terrestrialPropertiesWithoutSettlementBySocialGroupType);
            dicts.Add(terrestrialPropertiesWithoutSettlementBySocialGroupTypeExclusive);
            // Propertes - Without Social Groups and Settlements
            dicts.Add(terrestrialPropertiesWithoutSocialGroupOrSettlementByType);
            dicts.Add(terrestrialPropertiesWithoutSocialGroupOrSettlementByTypeExclusive);

            // Distances //
            // Location Distances
            dicts.Add(distanceByLocationsFromLocation);
            dicts.Add(locationsByStepsExclusiveFromLocation);
            dicts.Add(coastalLocationsByStepsExclusiveFromLocation);
            dicts.Add(oceanLocationsByStepsExclusiveFromLocation);
            dicts.Add(terrestrialLocationsByStepsExclusiveFromLocation);
            // Settlement Distances
            dicts.Add(settlementsByStepsExclusiveFromLocation);
            dicts.Add(coastalSettlementsByStepsExclusiveFromLocation);
            dicts.Add(oceanSettlementsByStepsExclusiveFromLocation);
            dicts.Add(terrestrialSettlementsByStepsExclusiveFromLocation);
            // Unit Distances
            dicts.Add(unitsByStepsExclusiveFromLocation);

            // Visibility
            dicts.Add(visibleUnitsByUnit);
            dicts.Add(unitVisibleToUnits);
            dicts.Add(commandableUnitLocations);
        }
    }
}