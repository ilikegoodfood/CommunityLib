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

        public ulong sortScore = 0;

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

        public void ProcessScore()
        {
            ulong score = 0;

            if (!ModCore.opt_godSort)
            {
                sortScore = 0;
                return;
            }

            // Higher bit positions = higher priority tier
            if (ModCore.opt_godSort_lastPlayedFirst)
            {
                score |= (isLastPlayed ? 0L : 1UL) << 60;
            }

            if (ModCore.opt_godSort_swwfFirst)
            {
                score |= (isSWWF ? 0UL : 1UL) << 55;
            }

            if (ModCore.opt_godSort_splitModded)
            {
                score |= (isVanilla ? 0UL : 1UL) << 50;
            }

            if (ModCore.opt_godSort_minorLate)
            {
                score |= (isMinorGod ? 1UL : 0UL) << 45;
            }

            if (ModCore.opt_godSort_bonusLast)
            {
                score |= (isBonusGod ? 1UL : 0UL) << 40;
            }

            if (ModCore.opt_godSort_Random && randomOrderKey >= 0)
            {
                // Map the random 32-bit int into bits 10-41
                score |= ((ulong)randomOrderKey) << 10;
            }

            sortScore = score;
        }

        public int CompareTo(GodSortData other)
        {
            if (!ModCore.opt_godSort)
            {
                return 0;
            }

            // Handle last played god first
            int scoreComparison = sortScore.CompareTo(other.sortScore);
            if (scoreComparison != 0)
            {
                return scoreComparison;
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
