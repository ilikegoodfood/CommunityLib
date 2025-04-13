using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class T_Prophet : Trait
    {
        public HolyOrder order;

        public T_Prophet(HolyOrder order)
        {
            this.order = order;
        }

        public override string getName()
        {
            if (order == null)
            {
                return "Prophet";
            }

            return $"Prophet ({order.getName()})";
        }

        public override string getDesc()
        {
            return $"This person is the prophet of the {order.getName()} holy order. Their revered status grants them great influence over it.";
        }

        public override void turnTick(Person p)
        {
            if (!(p.unit is UA) || order == null || order.prophet != p.unit)
            {
                p.traits.Remove(this);
            }
        }
    }
}
