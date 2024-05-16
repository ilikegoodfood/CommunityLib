using Assets.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityLib
{
    public static class ShuffleExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            while (count > 1)
            {
                int indexRandom = Eleven.random.Next(count);

                count--;
                
                T value = list[indexRandom];
                list[indexRandom] = list[count];
                list[count] = value;
            }
        }
    }
}
