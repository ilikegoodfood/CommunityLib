using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommunityLib
{
    public class ModCultureData
    {
        public Sprite ophanimMinorSettlementIcon = null;

        public Sprite defaultMinorSettlementIcon = null;

        public Sprite defaultMinorSettlementCoastalIcon = null;

        public Dictionary<Type, Sprite> subsettlmentMinorSettlementIcons = new Dictionary<Type, Sprite>();

        public ModCultureData()
        {

        }
    }
}
