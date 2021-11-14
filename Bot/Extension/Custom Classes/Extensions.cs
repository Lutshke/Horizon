using System;
using System.Collections.Generic;
using System.Linq;

namespace Horizon.Extensions
{
    public static class Extensions
    {
        public static bool OneOf(this Enum enumeration, params Enum[] enums)
        {
            return enums.Contains(enumeration);
        }

        public static T Pop<T>(this List<T> list, int Position)
        {
            T item = list[Position];
            list.RemoveAt(Position);
            return item;
        }
    }
}