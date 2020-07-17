using System;
using System.Collections.Generic;
using System.Text;

namespace WAVL
{
    public partial class Tree<K, V> where K : IComparable<K>, IEquatable<K>
    {
        public Node<K, V> Root;

        // OK
        public void Insert(K Key, V Value)
        {
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
            Node<K, V> prev = null;
            var current = Root;

            while (current.Key != Key)
            {
                prev = current;
                current = current.Key.CompareTo(K) > 0 ?
                    current.Right : current.Left;
            }

            var sub = current;

            if (current.Left == null && current.Right == null)
            {
                if (prev.Right == current) prev.Right = null;
                else prev.Left = null;

                BalancePath(GetPath(prev.Key));
            }

            else if (current.Left == null)
            {
                current.Key = current.Right.Key;
                current.Right = null;

                BalancePath(GetPath(current));
            }

            else if (current.Right == null)
            {
                current.Key = current.Left.Key;
                current.Left = null;

                BalancePath(GetPath(current));
            }

            else if (prev.Key.CompareTo(Key) > 0)
            {
                current = prev.Right;

                while (current.Right != null) { prev = current; current = current.Right; };

                sub.Key = current.Key;

                if (current.Left == null)
                {
                    prev.Right = null;
                    BalancePath(GetPath(prev));
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
                    BalancePath(GetPath(prev));
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

        public Node<K, V> BalancePath(List<FullNode<K, V>> path)
        {
            var start = path[0];

            path.Reverse();

            // Handle first vertex, it could have one of its children that was a leaf added or deleted.

            // Check if it is a leaf with non-zero rank.
            if (path[0].Rank > 0 && path[0].Left == null && path[0].Right == null)
            {
                if (path[0].Demoted > 0) DeleteFromDemotionPath(path, 0);

                if (path[0].Promoted) DeleteFromPromotionPath(path, 0);
                // Demote first vertex and remove it from path
                path[0].Base.rank--;
                path.RemoveAt(0);
            }

            var l = start.Rank - (path[0].Left == null ? -1 : path[0].Left.rank);
            var r = start.Rank - (path[0].Right == null ? -1 : path[0].Right.rank);

            if (path[0].Demoted == 1)
            {
                // Either 2-son was deleted, in which case we do a rotation
                if (l == 3 || r == 3)
                {
                    DeleteFromDemotionPath(path, 0);
                    return PickRotationDemote(path, 0);
                }

                // Or 2-son external vertex became an internal vertex
                DeleteFromDemotionPath(path, 0);
                return path.Last().Base;
            }
            if (path[0].Demoted == 2)
            {
                DeleteFromDemotionPath(path, 0);

                if (l == 3 || r == 3)
                    // 2-son was deleted
                    return PickRotationDemote(path, 0);



                // This special case is not needed, only for better comprehension
            }
            else if (path[0].Promoted)
            {
                if (l == 3 || r == 3)
                    // Either 2-son leaf was deleted
                    PickRotationPromote(path, 0);

                // Or 2-son external vertex was replaced by 1-son leaf.
                DeleteFromPromotionPath(path, 0);

                return path.Last().Base;
            }
            else if (path[0].RankDecreasedByParent)
            {
                // This node had 2 children, one was deleted. The deleted leaf node was 1 son, so parent should simply be removed from demotion path.

                // The path must begin at parent.
                DeleteFromDemotionPath(path, 1);

                return path.Last().Base;
            }
            else
            {

                if (l > 1 && r > 1)
                {
                    if (l < 3 && r < 3)
                    {
                        return path.Last().Base;
                    }
                    else if (l == 2 || r == 2)
                    {
                        return MoveDemotionUp(path);
                    }
                    else
                    {
                        return PickRotationDemote(path, 0);
                    }
                }
                else if (start.Rank + 1 == l || start.Rank + 1 == r)
                {
                    return MovePromotionUp(path);
                }
                else
                {
                    return PickRotationPromote(path, 0);
                }
            }
        }

        // Delete, discontinue
        void CutBottomFromDemotionPath(List<FullNode<K, V>> path, int index)
        {
            path[index].Base.rank--;

            if (path[index].PathStart == path[index + 1])
            {
                path[index + 1].Base.DemotionStart = false;
                path[index + 1].Base.ModPathEnd = null;
                path[index + 1].Base.rank--;
                if (path[index + 1].DemotedChild)
                {
                    if (path[index + 1].Base.Left == path[index].Base)
                    {
                        path[index + 1].Base.Right.rank--;
                    }
                    else
                    {
                        path[index + 1].Base.Left.rank--;
                    }
                }
            }
            else
            {
                path[index].PathStart.Base.ModPathEnd = path[index + 1].Base;
            }
        }

    }
}