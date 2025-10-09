using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class T_VinervaSeed : Trait
    {
        public Rt_TemptRuler TemptRulerChallenge;

        public T_VinervaSeed()
            : base ()
        {
            level = 1;
        }

        public override string getName()
        {
            return $"Vinerva's Seed ({level})";
        }

        public override string getDesc()
        {
            return $"This character carries {level} dormant {(level > 1 ? "fragments" : "fragment")} of the elder being known as Vinerva. She tempts humanity to sin, offering them help in their times of need, or ways to achieve their wildest dreams. However, her fruit is poisoned, and her gifts are treacherous. They could tempt a ruler with Vinerva's gifts, someone who likes {Tags.getName(Tags.AMBITION)}, {Tags.getName(Tags.CRUEL)}, or {Tags.getName(Tags.GOLD)}, to explaoit their sin.";
        }

        public override int getMaxLevel()
        {
            return 100;
        }

        public override void onAcquire(Person person)
        {
            if (person.unit is UA ua)
            {
                if (TemptRulerChallenge == null)
                {
                    TemptRulerChallenge = new Rt_TemptRuler(ua.location);
                }

                ua.rituals.Add(TemptRulerChallenge);
            }
        }

        public override void turnTick(Person p)
        {
            if (level <= 0)
            {
                if (p.unit != null && TemptRulerChallenge != null)
                {
                    p.unit.rituals.Remove(TemptRulerChallenge);
                }

                p.traits.Remove(this);
                return;
            }

            if (p.unit is UA ua)
            {
                if (TemptRulerChallenge == null)
                {
                    TemptRulerChallenge = new Rt_TemptRuler(ua.location);
                    ua.rituals.Add(TemptRulerChallenge);
                }
                else if (!ua.rituals.Contains(TemptRulerChallenge))
                {
                    ua.rituals.Add(TemptRulerChallenge);
                }
            }
        }
    }
}
