using Assets.Code;
using Assets.Code.Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CommunityLib
{
    public class UIE_Button_ModdedCompanion : MonoBehaviour
    {
        public UIE_Button pairedButton;

        public ModKernel maskingMod;

        public void setToMapMask(UIE_Button pairedButton, ModKernel maskingMod)
        {
            this.pairedButton = pairedButton;
            this.maskingMod = maskingMod;
        }
    }
}
