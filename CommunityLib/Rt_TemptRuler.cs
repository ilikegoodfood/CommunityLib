using Assets.Code;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommunityLib
{
    public class Rt_TemptRuler : Ritual
    {
        public Rt_TemptRuler(Location loc)
            : base(loc)
        {

        }

        public override string getName()
        {
            return "Tempt Ruler";
        }

        public override string getDesc()
        {
            return "The ruler of this place is weak-willed, and their sins can be turned against them by offering them Vinerva's gift.";
        }

        public override string getRestriction()
        {
            return $"This location's ruler must have a positive opinion of {Tags.getName(Tags.AMBITION)}, {Tags.getName(Tags.CRUEL)}, or {Tags.getName(Tags.GOLD)}.";
        }

        public override Sprite getSprite()
        {
            return map.world.iconStore.i_vinervaSeed;
        }

        public override int isGoodTernary()
        {
            return -1;
        }

        public override challengeStat getChallengeType()
        {
            return challengeStat.OTHER;
        }

        public override bool valid()
        {
            return true;
        }

        public override bool validFor(UA ua)
        {
            if (!ua.isCommandable() || ua.person == null || !ua.person.traits.Any(t => t is T_VinervaSeed seed && seed.level > 0))
            {
                return false;
            }

            if (!(ua.location.settlement is SettlementHuman settlementHuman) || settlementHuman.ruler == null || settlementHuman.ruler.isDead || !settlementHuman.ruler.isCastMember())
            {
                return false;
            }

            EventContext ctx = EventContext.withTwoPersons(map, ua.person, settlementHuman.ruler);
            return  EventManager.events.TryGetValue("anw.exploreRuins_VinervaPeriodic1", out EventManager.ActiveEvent activeEvent) && EventRuntime.evaluate(activeEvent.conditionalRoot, ctx);
        }

        public override bool validFor(UM ua)
        {
            return false;
        }

        public override double getComplexity()
        {
            return 1.0;
        }

        public override double getProgressPerTurnInner(UA unit, List<ReasonMsg> msgs)
        {
            msgs?.Add(new ReasonMsg("Base", 1.0));
            return 1.0;
        }

        public override void complete(UA u)
        {
            if (!u.isCommandable() || !(u.location.settlement is SettlementHuman settlementHuman) || settlementHuman.ruler == null || settlementHuman.ruler.isDead)
            {
                return;
            }

            EventContext ctx = EventContext.withTwoPersons(map, u.person, settlementHuman.ruler);
            if (EventManager.events.TryGetValue("anw.exploreRuins_VinervaPeriodic1", out EventManager.ActiveEvent activeEvent) && EventRuntime.evaluate(activeEvent.conditionalRoot, ctx))
            {
                map.world.prefabStore.popEvent(activeEvent.data, ctx, null, false);
            }
        }

        public override int[] buildPositiveTags()
        {
            return new int[]
            {
                Tags.AMBITION,
                Tags.CRUEL,
                Tags.GOLD
            };
        }
    }
}
