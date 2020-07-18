using System;
using System.Collections.Generic;
using System.Text;

namespace WAVL
{
    public partial class Tree<K, V> where K : IComparable<K>, IEquatable<K>
    {
        public Node<K, V> Root;

        public void Insert(K Key, V Value)
        {
            // Key must not be present in tree!

            if (Root == null) { Root = new Node<K, V>(Key, Value); return; }

            var path = GetPath(Key);

            var n = new Node<K, V>(Key, Value);

            if (Key.CompareTo(path.Last().Base.Key) > 0)
            {
                path.Last().Base.Left = n;
            }
            else
            {
                path.Last().Base.Right = n;
            }

            BalancePath(path);
        }

        public void Delete(K Key)
        {
            // Key must be present in tree!

            Node<K, V> prev = null;
            var current = Root;

            while (!current.Key.Equals(Key))
            {
                prev = current;
                current = current.Key.CompareTo(Key) > 0 ?
                    current.Right : current.Left;
            }

            var sub = current;

            if (current.Left == null && current.Right == null)
            {
                if (prev == null) 
                { 
                    // Deleted last vertex.
                    Root = null; 
                    return; 
                }

                // Leaf
                if (prev.Right == current) prev.Right = null;
                else prev.Left = null;

                BalancePath(GetPath(prev.Key));
            }

            else if (current.Left == null)
            {
                current.Key = current.Right.Key;
                current.Right = null;

                BalancePath(GetPath(current.Key));
            }

            else if (current.Right == null)
            {
                current.Key = current.Left.Key;
                current.Left = null;

                BalancePath(GetPath(current.Key));
            }

            else if (prev.Key.CompareTo(Key) > 0)
            {
                current = prev.Right;

                while (current.Right != null) { prev = current; current = current.Right; };

                sub.Key = current.Key;

                if (current.Left == null)
                {
                    prev.Right = null;
                    BalancePath(GetPath(prev.Key));
                }
                else
                {
                    current.Key = current.Left.Key;
                    current.Left = null;
                    BalancePath(GetPath(current.Key));
                }
            }
            else
            {
                current = prev.Left;

                while (current.Left != null) { prev = current; current = current.Left; };

                sub.Key = current.Key;

                if (current.Right == null)
                {
                    prev.Left = null;
                    BalancePath(GetPath(prev.Key));
                }
                else
                {
                    current.Key = current.Right.Key;
                    current.Right = null;
                    BalancePath(GetPath(current.Key));
                }
            }
        }

        /// <summary>
        /// Descend from the root and locate a vertex with the given key.
        /// </summary>
        /// <returns>List of vertices on the path</returns>
        public List<FullNode<K, V>> GetPath(K Key)
        {
            List<FullNode<K, V>> nodes = new List<FullNode<K, V>>();
            var current = Root;
            int demoting = 0;
            bool promoting = false;
            Node<K, V> bottom = null, bottom2 = null;
            FullNode<K, V> top = null, top2 = null;
            bool demotechild = false;

            while (current != null)
            {
                var v = new FullNode<K, V>(current);
                nodes.Add(v);

                if (current.PromotionStart)
                {
                    promoting = true;
                    bottom = current.ModPathEnd;
                    top = v;
                }
                if (current.DemotionStart)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = current.ModPathEnd;
                        top = v;
                    }
                    else
                    {
                        bottom2 = current.ModPathEnd;
                        top2 = v;
                    }
                }
                if (current.DemotionStart2)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = current.ModPathEnd;
                        top = v;
                    }
                    else
                    {
                        bottom2 = current.ModPathEnd2;
                        top2 = v;
                    }
                }
                v.Demoted = demoting;
                v.Promoted = promoting;
                v.PathStart = top;
                v.PathStart2 = top2;
                v.RankDecreasedByParent = demotechild;

                // Identify demoted child:
                // This applies even to doubly demoted vertices
                if (demoting > 1)
                {
                    if (bottom.Key.CompareTo(current.Key) > 0)
                    {
                        v.DemotedChild = (current.rank - current.Left.rank == 1);

                        demotechild = Key.CompareTo(current.Key) > 0;
                    }
                    else
                    {
                        v.DemotedChild = (current.rank - current.Right.rank == 1);

                        demotechild = Key.CompareTo(current.Key) < 0;
                    }
                }
                else
                {
                    demotechild = false;
                }

                if (Key.CompareTo(current.Key) < 0)
                {
                    if (bottom2 != null && bottom2.Key.CompareTo(current.Key) >= 0)
                    {
                        bottom2 = null;
                        demoting--;
                    }

                    if (bottom != null && bottom.Key.CompareTo(current.Key) >= 0)
                    {
                        bottom = bottom2;
                        bottom2 = null;
                        demoting--;
                        promoting = false;
                    }
                    current = current.Left;
                }
                else
                {
                    if (bottom2 != null && bottom2.Key.CompareTo(current.Key) <= 0)
                    {
                        bottom2 = null;
                        demoting--;
                    }

                    if (bottom != null && bottom.Key.CompareTo(current.Key) <= 0)
                    {
                        bottom = bottom2;
                        bottom2 = null;
                        demoting--;
                        promoting = false;
                    }
                    current = current.Right;
                }
            }

            return nodes;
        }

        /// <summary>
        /// Choose what balacing is needed and carry it out.
        /// </summary>
        /// <param name="path">Path beginning at a parent of deleted/inserted vertex</param>
        /// <returns>new root of tree</returns>
        public Node<K, V> BalancePath(List<FullNode<K, V>> path)
        {
            path.Reverse();

            if (path.Count == 0) throw new ArgumentException("Empty walking path");
            // Handle first vertex, it could have one of its children that was a leaf added or deleted.
            var first = path[0];

            // If leaf, go to demote
            if (first.Left == null && first.Right == null)
            {
                if (first.Rank > 0)
                    return MoveDemotionUp(path);
                else return path.Last().Base;
            }

            if (first.Left != null && first.Right != null)
            {
                // Insert
                if(first.Demoted > 0)
                {
                    DeleteFromDemotionPath(path, 0);
                }
                if(first.Promoted)
                {
                    DeleteFromPromotionPath(path, 0);
                }
                // No further action needed
                return path.Last().Base;
            }
            
            if(first.Rank == 2 && (first.Left == null || first.Right == null))
            {
                // 3-child, demote
                return MoveDemotionUp(path);
            }

            // Demote is not required
            // Try finding 0-son, the vertex must have rank 0 and internal child.
            if(first.Rank == 0)
            {
                return MovePromotionUp(path);
            }

            // Not called on correct path, but no reason to fail.
            return path.Last().Base;
        }
    }
}