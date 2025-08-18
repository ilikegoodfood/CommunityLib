using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public class MapMaskData
    {
        private readonly ModKernel _maskingMod = null;

        public ModKernel MaskingMod => _maskingMod;

        private readonly string _title = "";

        public string Title => _title;

        private readonly string _buttonLabel = "";

        public string ButtonLabel => _buttonLabel;

        private readonly string _description = "";

        public string Description => _description;

        private int _assignedID = -1;

        public int AssignedID => _assignedID;

        internal MapMaskData(ModKernel maskingMod, string title, string buttonLabel, string description, int assignedID)
        {
            _maskingMod = maskingMod;
            _title = title;
            _buttonLabel = buttonLabel;
            _description = description;
            _assignedID = assignedID;
        }
    }
}
