using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class ReasonMsgMax : ReasonMsg
    {
        public double max;

        public ReasonMsgMax(string v, double u, double m)
            : base(v,u)
        {
            msg = v;
            value = u;
            max = m;
        }

        public override int Compare(ReasonMsg x, ReasonMsg y)
        {
            int result = base.Compare(x, y);

            if (result == 0)
            {
                ReasonMsgMax xMax = x as ReasonMsgMax;
                ReasonMsgMax yMax = y as ReasonMsgMax;

                if (xMax != null && yMax != null)
                {
                    if (Math.Abs(xMax.max) > Math.Abs(yMax.max))
                    {
                        return 1;
                    }

                    if (Math.Abs(xMax.max) < Math.Abs(yMax.max))
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }
    }
}
