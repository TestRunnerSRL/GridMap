using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridMap
{
    public static class CollectionTools
    {
        public static IEnumerable<(int index, T item)> Enumerate<T>(this IEnumerable<T> collection)
        {
            int counter = 0;
            foreach (var item in collection)
            {
                yield return (counter, item);
                counter++;
            }
        }
    }
}
