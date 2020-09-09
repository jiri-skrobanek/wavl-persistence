using System;

namespace PersistentWAVL
{
    public class FullNode<K, V> : IComparable<FullNode<K, V>> where K : IComparable<K>
    {
        public FullNode(Node<K, V>.NodeAccessor @base)
        {
            Base = @base;
        }

        public Node<K, V>.NodeAccessor Base;

        /// <summary>
        /// THe number of demotion paths this vertex is on
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

        public FullNode<K, V> PathStart, PathStart2;

        public int Rank => Base.rank - Demoted + (Promoted ? 1 : 0) + (RankDecreasedByParent ? -1 : 0);

        public K Key => Base.Key;

        public int CompareTo(FullNode<K, V> other) => this.Key.CompareTo(other.Key);

        public Node<K, V>.NodeAccessor Left => Base.Left;

        public Node<K, V>.NodeAccessor Right => Base.Right;
    }
}
