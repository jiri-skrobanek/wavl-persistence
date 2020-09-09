using System;
using System.Collections.Generic;
using System.Text;

namespace WAVL
{
    public static class ListExtension
    {
        public static T Last<T>(this List<T> l) => l[l.Count - 1];
    }
}
