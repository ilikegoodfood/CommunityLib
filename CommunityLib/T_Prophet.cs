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
        public List<HolyOrder> Orders;

        public T_Prophet(HolyOrder order)
        {
            Orders = new List<HolyOrder> { order };
        }

        public T_Prophet(List<HolyOrder> orders)
        {
            Orders = orders;
        }

        public T_Prophet(HolyOrder[] orders)
        {
            Orders = Orders.ToList();
        }

        public override string getName()
        {
            if (Orders.Count == 0)
            {
                return "Prophet";
            }
            
            if (Orders.Count == 1)
            {
                return $"Prophet ({Orders[0].getName()})";
            }

            return $"Prophet of {Orders.Count} Holy Orders";
        }

        public override string getDesc()
        {
            if (Orders.Count == 0)
            {
                return $"This person is the prophet of no holy order.";
            }
            
            if (Orders.Count == 1)
            {
                return $"This person is the prophet of the {Orders[0].getName()} holy order. Their revered status grants them great influence over it.";
            }

            if (Orders.Count == 2)
            {
                return $"This person is the prophet of the {Orders[0].getName()} and {Orders[1].getName()} holy orders. Their revered status amongst these holy orders grants them great influence over them.";
            }

            string result = $"This person is the prophet of {Orders.Count} holy orders (";
            result += string.Join(", ", Orders.Take(Orders.Count - 2).Select(o => o.getName()));
            result += $", {Orders[Orders.Count - 2].getName()}, and {Orders[Orders.Count - 1].getName()}). Their revered status amongst these holy orders grants them great influence over them.";

            return result;
        }

        public override void turnTick(Person p)
        {
            if (Orders.Count == 0)
            {
                p.traits.Remove(this);
                return;
            }

            for (int i = Orders.Count - 1; i >= 0; i--)
            {
                if (Orders[i].isGone() || Orders[i].prophet == null || Orders[i].prophet.person != p)
                {
                    Orders.RemoveAt(i);
                }
            }

            if (Orders.Count == 0)
            {
                p.traits.Remove(this);
            }
        }
    }
}
