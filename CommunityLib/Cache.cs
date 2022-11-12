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

        // Units //
        public Dictionary<Type, IList> unitsByType;
        public Dictionary<SocialGroup, List<Unit>> unitsBySocialGroup;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> unitsBySocialGroupByType;

        // Commandable Units
        public Dictionary<Type, IList> commandableUnitsByType;
        public Dictionary<SocialGroup, List<Unit>> commandableUnitsBySocialGroup;
        public Dictionary<SocialGroup, Dictionary<Type, IList>> commandableUnitsBySocialGroupByType;

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

            // Units //
            unitsByType = new Dictionary<Type, IList>();
            dicts.Add(unitsByType);
            unitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(unitsBySocialGroup);
            unitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(unitsBySocialGroupByType);
            // Commandable Units
            commandableUnitsByType = new Dictionary<Type, IList>();
            dicts.Add(commandableUnitsByType);
            commandableUnitsBySocialGroup = new Dictionary<SocialGroup, List<Unit>>();
            dicts.Add(commandableUnitsBySocialGroup);
            commandableUnitsBySocialGroupByType = new Dictionary<SocialGroup, Dictionary<Type, IList>>();
            dicts.Add(commandableUnitsBySocialGroupByType);

            // Locations //
        }
    }
}