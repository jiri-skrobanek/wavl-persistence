using System;
using System.Collections.Generic;
using System.Text;

namespace WAVL
{
    public class FullNode<K, V> : IComparable<FullNode<K, V>> where K : IComparable<K>
    {
        public FullNode(Node<K, V> @base)
        {
            Base = @base;
        }

        public Node<K, V> Base;

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

        public Node<K, V> Left => Base.Left;

        public Node<K, V> Right => Base.Right;
    }
}
