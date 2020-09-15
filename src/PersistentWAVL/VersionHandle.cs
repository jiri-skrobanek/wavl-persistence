using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL
{
    public class VersionHandle : IComparable<VersionHandle>
    {
        public int CompareTo(VersionHandle other) => versionNode.CompareTo(other.versionNode);

        private Version.VersionNode versionNode;

        public VersionHandle GetSuccessor() => new VersionHandle { versionNode = versionNode.GetSuccessor() };

        public static VersionHandle GetNew() => new VersionHandle { versionNode = new Version.VersionNode() };

        public static bool operator >=(VersionHandle a, VersionHandle b) => a.CompareTo(b) >= 0;
        public static bool operator <=(VersionHandle a, VersionHandle b) => a.CompareTo(b) <= 0;

        public static bool operator >(VersionHandle a, VersionHandle b) => a.CompareTo(b) > 0;
        public static bool operator <(VersionHandle a, VersionHandle b) => a.CompareTo(b) < 0;
    }
}
