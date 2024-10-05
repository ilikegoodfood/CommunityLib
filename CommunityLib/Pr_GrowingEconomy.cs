using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class Pr_GrowingEconomy : Property
    {
        public double growthRate;

        public double neighbouringGrowthRate;

        public Pr_GrowingEconomy(Location loc, double growthRate, double neighbouringGrowthRate, int maxDuration = 100)
            : base(loc)
        {
            this.growthRate = growthRate;
            this.neighbouringGrowthRate = neighbouringGrowthRate;
            charge = maxDuration;
            influences.Add(new ReasonMsg("Natural decline", -1.0));
        }

        public override string getName()
        {
            if (growthRate < 0.0)
            {
                return "Shrinking Economy";
            }

            return "Growing Economy";
        }

        public override string getDesc()
        {
            if (growthRate < 0.0)
            {
                return "This settlement has a rapidly shrinking economy, casuing it's population to have a growth rate, and effecting the population growth rate of neighbouring settlements of this society.";
            }

            return "This settlement has a rapidly growing economy, boosting its population growth rate, and effecting the population growth rate of neighbouring settlements of this society.";
        }

        public override Sprite getSprite(World world)
        {
            return world.iconStore.market;
        }

        public override void turnTick()
        {
            if (!(location.settlement is SettlementHuman settlementHuman))
            {
                location.properties.Remove(this);
                return;
            }

            if (charge <= 0.0)
            {
                location.properties.Remove(this);
                return;
            }
            influences.Add(new ReasonMsg("Natural decline", -1.0));

            int pop = settlementHuman.population;
            int maxPop = settlementHuman.getMaxPopulation();
            int food = settlementHuman.foodLastTurn;

            if (settlementHuman is Set_DwarvenCity || settlementHuman is Set_DwarvenOutpost)
            {
                if (pop >= maxPop * 0.8 || pop >= food * 0.8)
                {
                    location.properties.Remove(this);
                    return;
                }
            }
            else
            {
                if (pop >= maxPop || pop >= food)
                {
                    location.properties.Remove(this);
                }
            }

            growPop(settlementHuman, growthRate);
            foreach (Location neighbour in location.getNeighbours())
            {
                if (neighbour.soc == location.soc && neighbour.settlement is SettlementHuman settlementHuman2)
                {
                    growPop(settlementHuman2, neighbouringGrowthRate);
                }
            }
        }

        public void growPop(SettlementHuman settlementHuman, double growthRate)
        {
            int pop = settlementHuman.population;
            int maxPop = settlementHuman.getMaxPopulation();
            int food = settlementHuman.foodLastTurn;

            if (settlementHuman is Set_DwarvenCity || settlementHuman is Set_DwarvenOutpost)
            {
                if (pop < maxPop * 0.8 && pop < food * 0.8)
                {
                    settlementHuman.growingPop += growthRate;
                    int grownPops = (int)settlementHuman.growingPop;

                    if (grownPops > 0)
                    {
                        settlementHuman.population += grownPops;
                        settlementHuman.growingPop -= grownPops;
                    }
                }
            }
            else
            {
                if (pop < maxPop && pop < food)
                {
                    settlementHuman.growingPop += growthRate;
                    int grownPops = (int)settlementHuman.growingPop;

                    if (grownPops > 0)
                    {
                        settlementHuman.population += grownPops;
                        settlementHuman.growingPop -= grownPops;
                    }
                }
            }
        }

        public override double getProsperityInfluence()
        {
            return 0.05;
        }
    }
}
