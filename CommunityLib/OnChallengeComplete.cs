using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class OnChallengeComplete
    {
        public static void processChallenge(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            switch (task_PerformChallenge.challenge)
            {
                case Rt_RaidPort _:
                    spawnShipwreck_Rt_RaidPort(challenge, ua, task_PerformChallenge);
                    break;
                case Rt_RaidShipping _:
                    spawnShipwreck_Rt_RaidShipping(challenge, ua, task_PerformChallenge);
                    break;
                default:
                    break;
            }
        }

        public static void spawnShipwreck_Rt_RaidPort(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (!ModCore.opt_forceShipwrecks && !ModCore.opt_spawnShipwrecks)
            {
                return;
            }

            int wreckRoll = Eleven.random.Next(10);

            if (wreckRoll == 0)
            {
                ModCore.core.spawnShipwreck(ua.location);
            }
        }

        public static void spawnShipwreck_Rt_RaidShipping(Challenge challenge, UA ua, Task_PerformChallenge task_PerformChallenge)
        {
            if (!ModCore.opt_forceShipwrecks && !ModCore.opt_spawnShipwrecks)
            {
                return;
            }

            int wreckRoll = Eleven.random.Next(10);

            if (wreckRoll == 0)
            {
                foreach (TradeRoute route in ua.map.tradeManager.routes)
                {
                    if (route.raidingCooldown == ua.map.param.ch_raidShippingCooldown && route.path.Contains(ua.location))
                    {
                        Location wreckLocation = null;
                        List<Location> oceanLocations = route.path.FindAll(l => l.isOcean).ToList();

                        if (oceanLocations.Count == 1)
                        {
                            wreckLocation = oceanLocations[0];
                        }
                        else if (oceanLocations.Count > 1)
                        {
                            wreckLocation = oceanLocations[Eleven.random.Next(oceanLocations.Count)];
                        }

                        ModCore.core.spawnShipwreck(wreckLocation);
                    }
                }
            }
        }
    }
}
