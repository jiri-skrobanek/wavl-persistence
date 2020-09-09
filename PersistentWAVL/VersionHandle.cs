using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL
{
    public class VersionHandle : IComparable<VersionHandle>
    {
        public int CompareTo(VersionHandle other)
        {
            throw new NotImplementedException();
        }

        private ulong Key;

        public VersionHandle GetSuccessor()
        {

        }

        public static VersionHandle GetNew()
        {

        }

        public static bool operator >=(VersionHandle a, VersionHandle b) => a.CompareTo(b) >= 0;
        public static bool operator <=(VersionHandle a, VersionHandle b) => a.CompareTo(b) <= 0;
    }
}
