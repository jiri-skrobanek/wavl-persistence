using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersistentWAVL
{
    public partial class Tree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        public VersionHandle Version { get; private set; }

        public Node<K, V>.NodeAccessor Root { get; private set; }

        private Tree()
        {

        }

        public static Tree<K, V> GetNew => new Tree<K, V> { Version = VersionHandle.GetNew() };

        public Node<K,V>.NodeAccessor Find(K Key)
        {
            var current = Root;

            while(current != null && current != Key)
            {
                current = current.Key.CompareTo(Key) > 0 ? current.Left : current.Right;
            }

            return current;
        }

        public Tree<K, V> Insert(K Key, V Value)
        {
            var newVersion = Version.GetSuccessor();
            // Key must not be present in tree!

            var n = new Node<K, V>.NodeAccessor(newVersion, new Node<K, V>(Key, Value, newVersion));

            if (Root == null) return new Tree<K, V> { Root = n, Version = newVersion };

            var path = GetPath(Key);

            if (Key.CompareTo(path.Last().Base.Key) > 0)
            {
                path.Last().Base.Left = n;
            }
            else
            {
                path.Last().Base.Right = n;
            }

            var top = BalancePath(path);

            return new Tree<K, V> { Root = top, Version = newVersion };
        }

        public Tree<K, V> Delete(K Key)
        {
            // Key must be present in tree!

            var newVersion = Version.GetSuccessor();

            Node<K, V>.NodeAccessor prev = null;
            var current = Root;

            while (!current.Key.Equals(Key))
            {
                prev = current;
                current = current.Key.CompareTo(Key) > 0 ?
                    current.Right : current.Left;
            }

            var sub = current;
            Node<K, V>.NodeAccessor top;

            if (current.Left == null && current.Right == null)
            {
                if (prev == null)
                {
                    // Deleted last vertex.
                    Root = null;
                    new Tree<K, V> { Version = newVersion };
                }

                // Leaf
                if (prev.Right == current) prev.Right = null;
                else prev.Left = null;

                top = BalancePath(GetPath(prev.Key));
            }

            else if (current.Left == null)
            {
                SwapModPathEnds(current.Key, current.Right.Key);
                current.Key = current.Right.Key;
                current.Right = null;

                top = BalancePath(GetPath(current.Key));
            }

            else if (current.Right == null)
            {
                SwapModPathEnds(current.Key, current.Left.Key);
                current.Key = current.Left.Key;
                current.Left = null;

                top = BalancePath(GetPath(current.Key));
            }

            else if (prev.Key.CompareTo(Key) > 0)
            {
                current = prev.Right;

                while (current.Right != null) { prev = current; current = current.Right; };

                SwapModPathEnds(Key, current.Key);
                sub.Key = current.Key;

                if (current.Left == null)
                {
                    prev.Right = null;
                    top = BalancePath(GetPath(prev.Key));
                }
                else
                {
                    SwapModPathEnds(Key, current.Left.Key);
                    current.Key = current.Left.Key;
                    current.Left = null;
                    top = BalancePath(GetPath(current.Key));
                }
            }
            else
            {
                current = prev.Left;

                while (current.Left != null) { prev = current; current = current.Left; };

                SwapModPathEnds(Key, current.Key);
                sub.Key = current.Key;

                if (current.Right == null)
                {
                    prev.Left = null;
                    top = BalancePath(GetPath(prev.Key));
                }
                else
                {
                    SwapModPathEnds(Key, current.Right.Key);
                    current.Key = current.Right.Key;
                    current.Right = null;
                    top = BalancePath(GetPath(current.Key));
                }
            }

            return new Tree<K, V> { Root = top, Version = newVersion };

            // Update the Key in all pmodifying paths that end with the vertex subject to key change.
            void SwapModPathEnds(K first, K second)
            {
                List<Node<K, V>.NodeAccessor> nodes = new List<Node<K, V>.NodeAccessor>(2);

                var c1 = Root;
                while (c1 != null)
                {
                    if (c1.ModPathEnd == second || c1.ModPathEnd2 == second)
                        nodes.Add(c1);
                    c1 = c1.Key.CompareTo(second) > 0 ? c1.Left : c1.Right;
                }

                c1 = Root;
                while(c1 != null)
                {
                    if (c1.ModPathEnd == first)
                        c1.ModPathEnd = second;
                    if (c1.ModPathEnd2 == first)
                        c1.ModPathEnd2 = second;
                    c1 = c1.Key.CompareTo(first) > 0 ? c1.Left : c1.Right;
                }

                foreach(var node in nodes)
                {
                    if (c1.ModPathEnd == second)
                        c1.ModPathEnd = first;
                    if (c1.ModPathEnd2 == second)
                        c1.ModPathEnd2 = first;
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
            Node<K, V>.NodeAccessor bottom = null, bottom2 = null;
            FullNode<K, V> top = null, top2 = null;
            bool demotechild = false;

            while (current != null)
            {
                var v = new FullNode<K, V>(current);
                nodes.Add(v);

                if (current.PromotionStart)
                {
                    promoting = true;
                    bottom = Find(current.ModPathEnd);
                    top = v;
                }
                if (current.DemotionStart)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = Find(current.ModPathEnd);
                        top = v;
                    }
                    else
                    {
                        bottom2 = Find(current.ModPathEnd);
                        top2 = v;
                    }
                }
                if (current.DemotionStart2)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = Find(current.ModPathEnd);
                        top = v;
                    }
                    else
                    {
                        bottom2 = Find(current.ModPathEnd2);
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
        public Node<K, V>.NodeAccessor BalancePath(List<FullNode<K, V>> path)
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
                if (first.Demoted > 0)
                {
                    DeleteFromDemotionPath(path, 0);
                }
                if (first.Promoted)
                {
                    DeleteFromPromotionPath(path, 0);
                }
                // No further action needed
                return path.Last().Base;
            }

            if (first.Rank == 2 && (first.Left == null || first.Right == null))
            {
                // 3-child, demote
                return MoveDemotionUp(path);
            }

            // Demote is not required
            // Try finding 0-son, the vertex must have rank 0 and internal child.
            if (first.Rank == 0)
            {
                return MovePromotionUp(path);
            }

            // Not called on correct path, but no reason to fail.
            return path.Last().Base;
        }
    }
}
