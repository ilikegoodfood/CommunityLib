using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SortedDictionaryProvider;

namespace CommunityLib
{
    public class UAENOverrideAI
    {
        public List<Type> customChallenges_OrcUpstart = new List<Type>();

        private Map map;

        private Cache cache;

        public UAENOverrideAI(Cache cache, Map map)
        {
            this.cache = cache;
            this.map = map;
        }

        public void OrcUpstartOverrideAI(UA uA)
        {
            //Console.WriteLine("CommunityLib: Running OrcUpstartOverrideAI.");
            List<Set_OrcCamp> orcCamps = null;
            List<Type> validChallengeTypes = new List<Type>() { typeof(Ch_OrcRaiding), typeof(Ch_Rest_InOrcCamp) };
            List<Challenge> validChallenges = new List<Challenge>();
            if (cache.settlementsBySocialGroupByType.ContainsKey(uA.society) && cache.settlementsBySocialGroupByType[uA.society] != null && cache.settlementsBySocialGroupByType[uA.society].ContainsKey(typeof(Set_OrcCamp)) && cache.settlementsBySocialGroupByType[uA.society][typeof(Set_OrcCamp)] != null)
            {
                //Console.WriteLine("CommunityLib: Getting orc camps belonging to orc upstart's social group.");
                orcCamps = cache.settlementsBySocialGroupByType[uA.society][typeof(Set_OrcCamp)] as List<Set_OrcCamp>;
            }

            if (uA.society.isGone() || orcCamps == null || orcCamps.Count == 0)
            {
                uA.die(map, "Died in the wilderness");
                return;
            }

            //Console.WriteLine("CommunityLib: Orc camps found.");
            if (map.worldPanic > map.param.panic_forFundHeroes)
            {
                //Console.WriteLine("CommunityLib: World panic high enough to recuit minions.");
                validChallengeTypes.Add(typeof(Ch_RecruitMinion));
            }
            validChallengeTypes.AddRange(customChallenges_OrcUpstart);
            //Console.WriteLine("CommunityLib: Orc Upstart has " + validChallengeTypes.Count.ToString() + " valid challenge types.");

            Set_OrcCamp orcCamp = orcCamps[Eleven.random.Next(orcCamps.Count)];
            foreach (Challenge challenge in orcCamp.location.GetChallenges())
            {
                //Console.WriteLine("CommunityLib: Checking if challenge of Type " + challenge.GetType().Name + " is of valid type.");
                if (validChallengeTypes.Contains(challenge.GetType()) && challenge.valid() && challenge.validFor(uA) && challenge.getUtility(uA, null) > 0.0)
                {
                    //Console.WriteLine("CommunityLib: Valid challenge found: " + challenge.getName() + ".");
                    validChallenges.Add(challenge);
                }
            }

            if (validChallenges.Count > 0)
            {
                uA.task = new Task_GoToPerformChallenge(validChallenges[Eleven.random.Next(validChallenges.Count)]);
                //Console.WriteLine("CommunityLib: Randomly selected " + (uA.task as Task_GoToPerformChallenge).challenge.getName() + " challenge from " + validChallenges.Count.ToString() + " valid challenges.");
            }
        }
    }
}
