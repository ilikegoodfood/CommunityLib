using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class ArmyBattleData
    {
        public List<UM> attackers;
        public List<UA> attComs;
        public List<UM> defenders;
        public List<UA> defComs;

        public ArmyBattleData()
        {
            attackers = new List<UM>();
            attComs = new List<UA>();
            defenders = new List<UM>();
            defComs= new List<UA>();
        }

        public ArmyBattleData(List<UM> attackers, List<UA> attComs, List<UM> defenders, List<UA> defComs)
        {
            this.attackers = attackers;
            this.attComs = attComs;
            this.defenders = defenders;
            this.defComs = defComs;
        }

        public void Clear()
        {
            attackers.Clear();
            attComs.Clear();
            defenders.Clear();
            defComs.Clear();
        }

        public Tuple<List<UM>, List<UA>> GetAttackers()
        {
            return new Tuple<List<UM>, List<UA>>(attackers, attComs);
        }

        public Tuple<List<UM>, List<UA>> GetDefenders()
        {
            return new Tuple<List<UM>, List<UA>>(defenders, defComs);
        }
    }
}
