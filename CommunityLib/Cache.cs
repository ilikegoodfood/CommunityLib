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
        private List<IList> lists;
        private List<IDictionary> dicts;

        // Social Groups //
        public Dictionary<Type, IList> socialGroupsByType;
        public Dictionary<Type, IList> socialGroupsByTypeExclusive;

        // Units //
        public Dictionary<Type, IList> unitsByType;
        public Dictionary<Type, IList> unitsByTypeExclusive;
        public Dictionary<SocialGroup, List<Unit>> unitsBySocialGroup;
        public Dictionary<SocialGroup, List<Unit>> unitsBySocialGroupExclusive;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> unitsBySocialGroupByType;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> unitsBySocialGroupByTypeExclusive;

        // Commandable Units
        public Dictionary<Type, IList> commandableUnitsByType;
        public Dictionary<Type, IList> commandableUnitsByTypeExclusive;
        public Dictionary<SocialGroup, List<Unit>> commandableUnitsBySocialGroup;
        public Dictionary<SocialGroup, List<Unit>> commandableUnitsBySocialGroupExclusive;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> commandableUnitsBySocialGroupByType;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> commandableUnitsBySocialGroupByTypeExclusive;

        // Locations //

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
            socialGroupsByType = new Dictionary<Type, IList>();
            dicts.Add(socialGroupsByType);
            socialGroupsByTypeExclusive = new Dictionary<Type, IList>();
            dicts.Add(socialGroupsByTypeExclusive);

            // Units //
            unitsByType = new Dictionary<Type, IList>();
            dicts.Add(unitsByType);
            unitsByTypeExclusive = new Dictionary<Type, IList>();
            dicts.Add(unitsByTypeExclusive);
            unitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(unitsBySocialGroup);
            unitsBySocialGroupExclusive = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(unitsBySocialGroupExclusive);
            unitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(unitsBySocialGroupByType);
            unitsBySocialGroupByTypeExclusive = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(unitsBySocialGroupByTypeExclusive);
            // Commandable Units
            commandableUnitsByType = new Dictionary<Type, IList>();
            dicts.Add(commandableUnitsByType);
            commandableUnitsByTypeExclusive = new Dictionary<Type, IList>();
            dicts.Add(commandableUnitsByTypeExclusive);
            commandableUnitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(commandableUnitsBySocialGroup);
            commandableUnitsBySocialGroupExclusive = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(commandableUnitsBySocialGroupExclusive);
            commandableUnitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(commandableUnitsBySocialGroupByType);
            commandableUnitsBySocialGroupByTypeExclusive = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(commandableUnitsBySocialGroupByTypeExclusive);

            // Locations //
        }
    }
}