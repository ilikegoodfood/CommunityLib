using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;

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
        }

        public Cache GetCache()
        {
            return cache;
        }
    }
}
