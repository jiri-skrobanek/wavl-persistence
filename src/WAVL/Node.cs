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

        public static bool operator <(Node<K, V> a, Node<K, V> b) => a.CompareTo(b) < 0;

        public static bool operator >(Node<K, V> a, Node<K, V> b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Remove this vertex from the top of path it is on.
        /// </summary>
        public void CutTopIfNeeded()
        {
            if (this.ModPathEnd == null) return;

            if (PromotionStart)
            {
                // Is it short?
                var next = ModPathEnd.CompareTo(this) > 0 ? Right : Left;

                if (next == ModPathEnd)
                {
                    // Last vertex
                    next.rank++;
                }
                else
                {
                    next.PromotionStart = true;
                    next.ModPathEnd = ModPathEnd;
                }

                rank++;
            }

            if (DemotionStart2)
            {
                // Is it short?
                var next = ModPathEnd2.CompareTo(this) > 0 ? Right : Left;

                // No need to decrease rank of child, this will be done by removal from the second path

                if (next == ModPathEnd2)
                {
                    // Demote child if needed (only if ModPathEnd is this)
                    if (next.Left != null && next.Left.RankWithOwnOffset + 1 == next.rank)
                    {
                        next.Left.rank--;
                    }
                    if (next.Right != null && next.Right.RankWithOwnOffset + 1 == next.rank)
                    {
                        next.Right.rank--;
                    }

                    // Last vertex
                    next.rank--;
                }
                else
                {
                    next.DemotionStart = true;
                    next.ModPathEnd = ModPathEnd;

                    // next cannot by a demotion start already, it would be on three paths
                }

                rank--;
            }

            if (DemotionStart)
            {
                // Is it short?
                var next = ModPathEnd.CompareTo(this) > 0 ? Right : Left;
                var other = ModPathEnd.CompareTo(this) > 0 ? Left : Right;

                if (other.RankWithOwnOffset == rank - 1)
                {
                    // Demote child if needed
                    other.rank--;
                }

                if (next == ModPathEnd)
                {
                    // Demote child if needed
                    if (next.Left != null && next.Left.RankWithOwnOffset + 1 == next.rank)
                    {
                        next.Left.rank--;
                    }
                    if (next.Right != null && next.Right.RankWithOwnOffset + 1 == next.rank)
                    {
                        next.Right.rank--;
                    }

                    // Last vertex
                    next.rank--;
                }
                else if (next.DemotionStart)
                {
                    next.DemotionStart2 = true;
                    next.ModPathEnd2 = ModPathEnd;
                }
                else
                {
                    next.DemotionStart = true;
                    next.ModPathEnd = ModPathEnd;
                }

                rank--;
            }


            PromotionStart = false;
            DemotionStart = false;
            DemotionStart2 = false;
            ModPathEnd = null;
            ModPathEnd2 = null;
        }

        public int RankWithOwnOffset => rank + (DemotionStart ? -1 : 0) + (DemotionStart2 ? -1 : 0) + (PromotionStart ? 1 : 0);
    }



}