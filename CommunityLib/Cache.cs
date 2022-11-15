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
        public Dictionary<SocialGroup, IList> locationsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> locationsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> locationsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> locationsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();

        // Negative Filters - Social Groups
        public Dictionary<Type, IList> locationsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();

        // Negative Filters - Settlements
        public Dictionary<Type, IList> locationsWithoutSettlementByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> locationsWithoutSettlementBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSettlementBySocialGroupTypeExclusive = new Dictionary<Type, IList>();

        //NegativeFilters - Social Groups and Settlements
        public Dictionary<Type, IList> locationsWithoutSocialGroupsOrSettlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> locationsWithoutSocialGroupsOrSettlementsByTypeExclusive = new Dictionary<Type, IList>();

        // Settlements //
        public Dictionary<Type, IList> settlementsByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsByTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, IList> settlementsBySocialGroup = new Dictionary<SocialGroup, IList>();
        public Dictionary<Type, IList> settlementsBySocialGroupType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsBySocialGroupTypeExclusive = new Dictionary<Type, IList>();
        public Dictionary<SocialGroup, Dictionary<Type, IList>> settlementsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeExclusiveByType = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();
        public Dictionary<Type, Dictionary<Type, IList>> settlementsBySocialGroupTypeExclusiveByTypeExclusive = new Dictionary<Type, Dictionary<Type, IList>>();

        // Negative Filters
        public Dictionary<Type, IList> settlementsWithoutSocialGroupByType = new Dictionary<Type, IList>();
        public Dictionary<Type, IList> settlementsWithoutSocialGroupByTypeExclusive = new Dictionary<Type, IList>();


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
            dicts.Add(locationsBySocialGroup);
            dicts.Add(locationsBySocialGroupType);
            dicts.Add(locationsBySocialGroupTypeExclusive);
            dicts.Add(locationsBySocialGroupByType);
            dicts.Add(locationsBySocialGroupTypeByType);
            dicts.Add(locationsBySocialGroupTypeExclusiveByType);
            dicts.Add(locationsBySocialGroupTypeByTypeExclusive);
            dicts.Add(locationsBySocialGroupTypeExclusiveByTypeExclusive);
            // Negative Filters - Social Groups
            dicts.Add(locationsWithoutSocialGroupByType);
            dicts.Add(locationsWithoutSocialGroupByTypeExclusive);
            // Negative Filters - Settlements
            dicts.Add(locationsWithoutSettlementByType);
            dicts.Add(locationsWithoutSettlementByTypeExclusive);
            dicts.Add(locationsWithoutSettlementBySocialGroup);
            dicts.Add(locationsWithoutSettlementBySocialGroupType);
            dicts.Add(locationsWithoutSettlementBySocialGroupTypeExclusive);
            // Negative Filters - Social Groups and Settlements
            dicts.Add(locationsWithoutSocialGroupsOrSettlementsByType);
            dicts.Add(locationsWithoutSocialGroupsOrSettlementsByTypeExclusive);

            // Settlements //
            dicts.Add(settlementsByType);
            dicts.Add(settlementsByTypeExclusive);
            dicts.Add(settlementsBySocialGroup);
            dicts.Add(settlementsBySocialGroupType);
            dicts.Add(settlementsBySocialGroupTypeExclusive);
            dicts.Add(settlementsBySocialGroupByType);
            dicts.Add(settlementsBySocialGroupTypeByType);
            dicts.Add(settlementsBySocialGroupTypeExclusiveByType);
            dicts.Add(settlementsBySocialGroupTypeByTypeExclusive);
            dicts.Add(settlementsBySocialGroupTypeExclusiveByTypeExclusive);
            // Negative Filters
            dicts.Add(settlementsWithoutSocialGroupByType);
            dicts.Add(settlementsWithoutSocialGroupByTypeExclusive);
        }
    }
}