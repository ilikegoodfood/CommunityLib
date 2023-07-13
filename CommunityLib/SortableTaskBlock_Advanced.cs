using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class SortableTaskBlock_Advanced : UIScroll_Unit.SortableTaskBlock
    {
        public Type taskType;

        public AgentAI.ChallengeData challengeData;

        public Location location;

        public SocialGroup socialGroup;

        public Unit unit;

        public AgentAI.TaskData taskData;
    }
}
