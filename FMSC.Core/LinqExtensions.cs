using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMSC.Core
{
    public static class LinqExtensions
    {
        public static bool HasAtLeast<T>(this IEnumerable<T> source, int amount, Func<T, bool> predicate = null)
        {
            if (amount < 1)
                throw new ArgumentException("amount must be at least 1");

            int count = 0;

            if (predicate != null)
            {
                foreach (T item in source)
                {
                    if (predicate(item))
                        count++;

                    if (count == amount)
                        return true;
                }
            }
            else
            {
                foreach (T item in source)
                {
                    count++;

                    if (count == amount)
                        return true;
                }
            }
            
            return false;
        }
    }
}
