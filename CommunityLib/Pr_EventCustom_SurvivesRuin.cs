using Assets.Code;
using UnityEngine;

namespace CommunityLib
{
    public class Pr_EventCustom_SurvivesRuin : Pr_EventCustom
    {
        public Pr_EventCustom_SurvivesRuin(Location loc)
            : base(loc)
        {

        }

        public override bool survivesRuin()
        {
            return true;
        }
    }
}
