using System.Collections.Generic;
using System.Linq;

namespace TopoMap.Util
{
    public static class IEnumeratorExtension
    {
        public static IEnumerable<T> Drop<T>(this IEnumerable<T> enumerable, int drops)
        {
            int max = enumerable.Count();
            int countDown = max - drops;

            foreach (var v in enumerable)
            {
                if (countDown <= 0)
                {
                    yield break;
                }

                countDown--;
                yield return v;
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T[]> enumerable)
        {
            foreach (var array in enumerable)
            {
                foreach (var value in array)
                {
                    yield return value;
                }
            }
        }

        public static string StringJoin<T>(this IEnumerable<T> enumerable, string joint = ",")
        {
            return string.Join(joint, enumerable.Select(it => it.ToString()).ToArray());
        }
    }
}