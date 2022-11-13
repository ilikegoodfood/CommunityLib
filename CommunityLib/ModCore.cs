using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;
using UnityEngine.Diagnostics;

namespace CommunityLib
{
    public class ModCore : Assets.Code.Modding.ModKernel
    {
        public Cache cache;
        public Filters filters;

        public override void onTurnStart(Map map)
        {
            base.onTurnStart(map);

            //Initialize subclasses.
            if (cache == null)
            {
                cache = new Cache();
            }
            else
            {
                cache.ClearCache();
            }

            if (filters == null)
            {
                filters = new Filters(cache, map);
            }

            // Begin Filtering Process.
            filters.FilterSocialGroups();
            filters.FilterUnits();
            //testRoutine(map);
        }

        private void testRoutine(Map map)
        {
            SG_Orc randOrcSG = null;
            Society randSociety = null;
            HolyOrder randHumanHolyOrder= null;
            HolyOrder_Witches randWitchHolyOrder = null;
            UA randCommandableAgent = null;
            int rand;

            if (cache.socialGroupsByType.ContainsKey(typeof(SG_Orc)))
            {
                List<SG_Orc> orcSocieties = cache.socialGroupsByType[typeof(SG_Orc)] as List<SG_Orc>;
                rand = Eleven.random.Next(orcSocieties.Count());
                randOrcSG = orcSocieties[rand];
            }

            if (cache.socialGroupsByType.ContainsKey(typeof(Society)))
            {
                List<Society> societies = cache.socialGroupsByType[typeof(Society)] as List<Society>;
                rand = Eleven.random.Next(societies.Count());
                randSociety = societies[rand];
            }

            if (cache.socialGroupsByTypeExclusive.ContainsKey(typeof(HolyOrder)))
            {
                List<HolyOrder> humanHolyOrders = cache.socialGroupsByTypeExclusive[typeof(HolyOrder)] as List<HolyOrder>;
                rand = Eleven.random.Next(humanHolyOrders.Count());
                randHumanHolyOrder = humanHolyOrders[rand];
            }

            if (cache.socialGroupsByType.ContainsKey(typeof(HolyOrder_Witches)))
            {
                List<HolyOrder_Witches> witchHolyOrders = cache.socialGroupsByType[typeof(HolyOrder_Witches)] as List<HolyOrder_Witches>;
                rand = Eleven.random.Next(witchHolyOrders.Count());
                randWitchHolyOrder = witchHolyOrders[rand];
            }

            if (cache.commandableUnitsByType.ContainsKey(typeof(UA)))
            {
                List<UA> commandableAgents = cache.commandableUnitsByType[typeof(UA)] as List<UA>;
                rand = Eleven.random.Next(commandableAgents.Count());
                randCommandableAgent = commandableAgents[rand];
            }

            if (randOrcSG != null && randSociety != null && randHumanHolyOrder != null && randWitchHolyOrder != null && randCommandableAgent != null)
            {
                map.addUnifiedMessage(randHumanHolyOrder, randWitchHolyOrder, "Cache Testing", "This is a batch of tests for the cacher. It retrieved the following items at random: Random Orc Social Group: " + randOrcSG.getName() + ". Random society: " + randSociety.getName() + ". Random Human Holy Order: " + randHumanHolyOrder.getName() + ". Random Wicth Holy Order: " + randWitchHolyOrder.getName() + ". Random Commandable Agent: " + randCommandableAgent.getName(), "CommunityLib Test");
            }
        }

        public Cache GetCache()
        {
            return cache;
        }
    }
}
