using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommunityLib
{
    public class GodSortData : IComparable<GodSortData>
    {
        public God god = null;

        public string processedName = "";

        public bool isVanilla = false;

        public bool isBonusGod = false;

        public bool isMinorGod = false;

        public bool isLastPlayed = false;

        public bool isSWWF = false;

        public int randomOrderKey = -1;

        public GodSortData()
        {

        }

        public void ProcessName()
        {
            bool ignorePrefixes = ModCore.opt_godSort_ignorePrefixes;
            bool ignoreThe = ModCore.opt_godSort_ignoreThe;

            if (!ignorePrefixes && !ignoreThe)
            {
                return;
            }

            string[] components = processedName.Split(new string[] { ": " }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length == 2)
            {
                string prefix = components[0] + ": ";
                string name = components[1];

                if (ignoreThe && name.StartsWith("The ", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(4);  // Remove "The " from the start of the name
                }

                if (ignorePrefixes)
                {
                    processedName = name;
                }
                else
                {
                    processedName = prefix + name;
                }
            }
            else
            {
                if (ignoreThe && processedName.StartsWith("The ", StringComparison.OrdinalIgnoreCase))
                {
                    processedName = processedName.Substring(4);  // Remove "The " from the start
                }
            }
        }

        public int CompareTo(GodSortData other)
        {
            // Handle last played god first
            if (ModCore.opt_godSort_lastPlayedFirst)
            {
                if (isLastPlayed && !other.isLastPlayed) return -1;
                if (!isLastPlayed && other.isLastPlayed) return 1;
            }

            // Handle SWWF god first
            if (ModCore.opt_godSort_swwfFirst)
            {
                if (isSWWF && !other.isSWWF) return -1;
                if (!isSWWF && other.isSWWF) return 1;
            }

            // Handle modded vs vanilla splitting
            if (ModCore.opt_godSort_splitModded)
            {
                if (isVanilla && !other.isVanilla) return -1;
                if (!isVanilla && other.isVanilla) return 1;
            }

            // Handle minor gods placement
            if (ModCore.opt_godSort_minorLate)
            {
                if (isMinorGod && !other.isMinorGod) return 1;
                if (!isMinorGod && other.isMinorGod) return -1;
            }

            // Handle bonus gods placement
            if (ModCore.opt_godSort_bonusLast)
            {
                if (isBonusGod && !other.isBonusGod) return 1;
                if (!isBonusGod && other.isBonusGod) return -1;
            }

            if (ModCore.opt_godSort_Random && randomOrderKey >= 0 && other.randomOrderKey >= 0)
            {
                return randomOrderKey.CompareTo(other.randomOrderKey);
            }

            // Finally, compare alphabetically
            if (ModCore.opt_godSort_Alphabetise)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(processedName, other.processedName);
            }

            return 0;
        }
    }
}
