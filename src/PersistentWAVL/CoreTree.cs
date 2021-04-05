using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersistentWAVL
{
    /// <summary>
    /// Represents one version of the binary search tree data structure
    /// </summary>
    /// <typeparam name="K">Type of keys</typeparam>
    /// <typeparam name="V">Type of values</typeparam>
    public partial class Tree<K, V> : ITree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        public VersionHandle Version { get; private set; }

        internal Node.NodeAccessor Root { get; private set; }

        private Tree()
        {

        }

        public static Tree<K, V> GetNew => new Tree<K, V> { Version = VersionHandle.GetNew() };

        public V Find(K Key) => _find(Key).Value;

        private Node.NodeAccessor _find(K Key)
        {
            var current = Root;

            while (current != null && current != Key)
            {
                current = current.Key.CompareTo(Key) > 0 ? current.Left : current.Right;
            }

            return current;
        }

        /// <summary>
        /// Inserts an entry not present in the current version.
        /// </summary>
        /// <returns>New version with inserted vertex</returns>
        public Tree<K, V> Insert(K Key, V Value)
        {
            var newVersion = Version.GetSuccessor();
            // Key must not be present in tree!

            var n = new Node(Key, Value, newVersion).GetTemporaryAccessorForVersion(newVersion);

            if (Root is null) 
            {
                var newRoot = n.GetPermanentAccessor();
                Node.RemoveListOfAccessors();
                return new Tree<K, V> { Root = newRoot, Version = newVersion }; 
            }

            var root = Root.Node.GetTemporaryAccessorForVersion(newVersion);

            var path = GetPath(Key, root);

            if (Key.CompareTo(path.Last().Key) < 0)
            {
                path.Last().Base.Left = n;
            }
            else
            {
                path.Last().Base.Right = n;
            }

            var top = BalancePath(path).GetPermanentAccessor();

            Node.RemoveListOfAccessors();

            return new Tree<K, V> { Root = top, Version = newVersion };
        }

        /// <summary>
        /// Removes an element with specified key producing a new version.
        /// </summary>
        /// <returns>New version with deleted vertex.</returns>
        public Tree<K, V> Delete(K Key)
        {
            if (Root is null) throw new InvalidOperationException("Cannot delete from empty tree.");
            // If there is only one vertex return null. 
            // This is a technical detail and behaviour may be changed to produce empty tree with a new version.
            if (Root.Left is null && Root.Right is null) return null;

            // Key must be present in tree!

            var newVersion = Version.GetSuccessor();

            Node.NodeAccessor prev = null;

            var root = Root.Node.GetTemporaryAccessorForVersion(newVersion);

            var current = root;

            while (!current.Key.Equals(Key))
            {
                prev = current;
                current = current.Key.CompareTo(Key) < 0 ?
                    current.Right : current.Left;
            }

            var sub = current;
            Node.NodeAccessor top;

            if (current.Left == null && current.Right == null)
            {
                // Leaf
                if (prev.Right == current) prev.Right = null;
                else prev.Left = null;

                top = BalancePath(GetPath(prev.Key, root));
            }

            // Switch key and value with a leaf vertex

            else if (current.Left == null)
            {
                SwapModPathEnds(current.Key, current.Right.Key);
                current.Key = current.Right.Key;
                current.Value = current.Right.Value;
                current.Right = null;

                top = BalancePath(GetPath(current.Key, root));
            }

            else if (current.Right == null)
            {
                SwapModPathEnds(current.Key, current.Left.Key);
                current.Key = current.Left.Key;
                current.Value = current.Left.Value;
                current.Left = null;

                top = BalancePath(GetPath(current.Key, root));
            }

            else if (prev.Key.CompareTo(Key) > 0)
            {
                current = prev.Right;

                while (current.Right != null) { prev = current; current = current.Right; };

                SwapModPathEnds(Key, current.Key);
                sub.Key = current.Key;
                sub.Value = current.Value;

                if (current.Left == null)
                {
                    prev.Right = null;
                    top = BalancePath(GetPath(prev.Key, root));
                }
                else
                {
                    SwapModPathEnds(Key, current.Left.Key);
                    current.Key = current.Left.Key;
                    current.Value = current.Left.Value;
                    current.Left = null;
                    top = BalancePath(GetPath(current.Key, root));
                }
            }
            else
            {
                current = prev.Left;

                while (current.Left != null) { prev = current; current = current.Left; };

                SwapModPathEnds(Key, current.Key);
                sub.Key = current.Key;
                sub.Value = current.Value;

                if (current.Right == null)
                {
                    prev.Left = null;
                    top = BalancePath(GetPath(prev.Key, root));
                }
                else
                {
                    SwapModPathEnds(Key, current.Right.Key);
                    current.Key = current.Right.Key;
                    current.Value = current.Right.Value;
                    current.Right = null;
                    top = BalancePath(GetPath(current.Key, root));
                }
            }

            var newRoot = top.GetPermanentAccessor();

            Node.RemoveListOfAccessors();

            return new Tree<K, V> { Root = newRoot, Version = newVersion };

            // Update the Key in all modifying paths that end with the vertex subject to key change.
            void SwapModPathEnds(K first, K second)
            {
                List<Node.NodeAccessor> nodes = new List<Node.NodeAccessor>(2);

                var c1 = Root;
                while (c1 != null)
                {
                    if (c1.ModPathEnd == second || c1.ModPathEnd2 == second)
                        nodes.Add(c1);
                    c1 = c1.Key.CompareTo(second) > 0 ? c1.Left : c1.Right;
                }

                c1 = Root;
                while (c1 != null)
                {
                    if (c1.ModPathEnd == first)
                        c1.ModPathEnd = second;
                    if (c1.ModPathEnd2 == first)
                        c1.ModPathEnd2 = second;
                    c1 = c1.Key.CompareTo(first) > 0 ? c1.Left : c1.Right;
                }

                foreach (var node in nodes)
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
        /// Must be used with direct successor version of root.
        /// </summary>
        /// <returns>List of vertices on the path</returns>
        private List<FullNode> GetPath(K Key, Node.NodeAccessor rootAccessor)
        {
            List<FullNode> nodes = new List<FullNode>();
            var current = rootAccessor;
            int demoting = 0;
            bool promoting = false;
            Node.NodeAccessor bottom = null, bottom2 = null;
            FullNode top = null, top2 = null;
            bool demotechild = false;

            while (current != null)
            {
                var v = new FullNode(current);
                nodes.Add(v);

                if (current.PromotionStart)
                {
                    promoting = true;
                    bottom = _find(current.ModPathEnd);
                    top = v;
                }
                if (current.DemotionStart)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = _find(current.ModPathEnd);
                        top = v;
                    }
                    else
                    {
                        bottom2 = _find(current.ModPathEnd);
                        top2 = v;
                    }
                }
                if (current.DemotionStart2)
                {
                    demoting++;
                    if (demoting == 1)
                    {
                        bottom = _find(current.ModPathEnd);
                        top = v;
                    }
                    else
                    {
                        bottom2 = _find(current.ModPathEnd2);
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
        internal Node.NodeAccessor BalancePath(List<FullNode> path)
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
