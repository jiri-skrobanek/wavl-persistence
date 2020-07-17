using System;
using System.Collections.Generic;
using System.Linq;

namespace WAVL
{
    public class Node<K, V> : IComparable<Node<K, V>> where K : IComparable<K>
    {
        // It is maintained that Path2 must wholly encompass path1 if it exists.

        public int rank = 0;

        public bool DemotionStart = false;
        public bool DemotionStart2 = false;

        public bool PromotionStart = false;

        public Node<K, V> ModPathEnd = null;
        public Node<K, V> ModPathEnd2 = null;

        public K Key { get; set; }

        public V Value { get; set; }

        public Node<K, V> Left;

        public Node<K, V> Right;

        public Node(K Key, V Value)
        {
            this.Key = Key;
            this.Value = Value;
        }

        public override string ToString()
        {
            return $"<{Key.ToString()}:{Value.ToString()}>";
        }

        public int CompareTo(Node<K, V> other) => Key.CompareTo(other.Key);

        /// <summary>
        /// Remove this vertex from the top of path it is on.
        /// </summary>
        public void CutTopIfNeeded()
        {
            if (this.ModPathEnd == null) return;

            // Is it short?

            var next = this.ModPathEnd.CompareTo(this) > 0 ? this.Right : this.Left;

            if (next == this.ModPathEnd)
            {
                // Last vertex
                next.rank += this.PromotionStart ? 1 : -1;
            }
            else
            {
                next.DemotionStart = this.DemotionStart;
                next.PromotionStart = this.PromotionStart;
                next.ModPathEnd = this.ModPathEnd;
            }

            this.rank += this.PromotionStart ? 1 : -1;
            this.PromotionStart = false;
            this.DemotionStart = false;
            this.ModPathEnd = null;
        }
    }



}