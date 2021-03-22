using System;

namespace PersistentWAVL
{
    /// <summary>
    /// Temporary record used during one operation to hold computed values about an instance of <see cref="Node{K, V}"/>.
    /// </summary>
    public partial class Tree<K, V> : ITree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        public class FullNode
        {
            internal FullNode(Node.NodeAccessor @base)
            {
                Base = @base;
            }

            internal Node.NodeAccessor Base;

            /// <summary>
            /// The number of demotion paths this vertex is on
            /// </summary>
            public int Demoted = 0;

            public bool Promoted = false;

            /// <summary>
            /// By demote of second kind
            /// </summary>
            public bool DemotedChild = false;

            /// <summary>
            /// By demote of second kind
            /// </summary>
            public bool RankDecreasedByParent = false;

            public FullNode PathStart, PathStart2;

            public int Rank => Base.rank - Demoted + (Promoted ? 1 : 0) + (RankDecreasedByParent ? -1 : 0);

            public K Key => Base.Key;

            public int CompareTo(FullNode other) => this.Key.CompareTo(other.Key);

            internal Node.NodeAccessor Left => Base.Left;

            internal Node.NodeAccessor Right => Base.Right;
        }
    }
}