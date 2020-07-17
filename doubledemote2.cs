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

        public int CompareTo(FullNode<K, V> other) => node.Key.CompareTo(other.Key);

        public Node<K, V> Left => Base.Left;

        public Node<K, V> Right => Base.Right;
    }

    public class Tree<K, V> where K : IComparable<K>
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
                if (prev.Right == current) prev.Right == null;
                else prev.Left == null;

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
                        bottom2 == null;
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
                        bottom2 == null;
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
            if (path[0].Rank > 0 && path[0].Left == null && path[0].Right)
            {
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
                // 2-son was deleted
                DeleteFromDemotionPath(path, 0);
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

        private void DeleteFromPromotionPath(List<FullNode<K, V>> path, int index)
        {
            // Only actual nodes are updated, no changes done to fullnodes, this is not needed as rebalancing will not proceed to other nodes.

            // TODO: change path

            var ex = path[index];

            var end = ex.PathStart.Base.ModPathEnd;


            int lowerlen = 0;

            for (int i = index; i > 0; i--)
            {
                if (GetPromotionContinuation(path[i]) == path[i - 1].Base) lowerlen++; else break;
            }

            // Now decide if the length of the new path will be at least two.

            int newlen = 0;

            var v = path[index].Base;

            while (true)
            {
                if (removed.Base.ModPathEnd == v) break;

                v = v.CompareTo(removed.Base.ModPathEnd) > 0 ? v.Left : v.Right;

                if (v == null) break;
                newlen++;
            }

            if (newlen == 0)
            {
                // bottom was removed, no action needed
            }
            else if (newlen == 1)
            {
                if (index > 0 && path[index - 1].Base == removed.Base.ModPathEnd)
                {
                    WritePromotion(index - 1);
                }
                else
                {
                    var other = GetPromotionContinuation(path[index]);
                    other.rank++;
                }
            }
            else
            {
                // Update the whole segment on path
                for (int i = 0; i < lowerlen; i++)
                {
                    path[index - i].PathStart = path[index - 1];
                }

                // Update the top outside of path
                var lowertop = GetPromotionContinuation(path[index]);

                lowertop.PromotionStart = true;
                lowertop.ModPathEnd = removed.PathStart.Base.ModPathEnd;
            }

            // Upper part
            if (removed.PathStart == removed)
            {
                // Nothing to do here.
            }
            else if (removed.PathStart == path[index + 1])
            {
                WritePromotion(index + 1);
            }
            else
            {
                removed.PathStart.Base.ModPathEnd = path[index + 1].Base;
            }

            WritePromotion(index);

            void WritePromotion(int pos)
            {
                path[pos].Promoted = false;
                path[pos].PathStart = null;
                path[pos].Base.rank++;
                path[pos].Base.ModPathEnd = null;
                path[pos].Base.PromotionStart = false;
            }

            Node<K, V> GetPromotionContinuation(FullNode<K, V> node)
            {
                if (node.Base == node.PathStart.Base.ModPathEnd)
                {
                    return null;
                }
                return node.PathStart.Base.ModPathEnd.CompareTo(node) > 0 ?
                node.Right : node.Left;
            }
        }

        /// <summary>
        /// Removes a vertex from a demotion path, its rank is overwritten by the decreased value.
        /// If it has a child that should have its value changed, this is also performed. 
        /// Updates are written to all full nodes along the path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="index"></param>
        private void DeleteFromDemotionPath(List<FullNode<K, V>> path, int index)
        {
            var removed = path[index];
            // The length of demotion path
            int newlen = 0, newlen2 = 0;
            // The length of the demotion path of walking path
            int lowerlen = 0, lowerlen2 = 0;

            // Determine if two or more vertices need to be updated.
            for (int i = index; i > 0; i--)
            {
                if (GetDemotionContinuation(path[i]) == path[i - 1].Base) lowerlen++; else break;
            }

            var v = path[index].Base;
            while (true)
            {
                if (removed.Base.ModPathEnd == v) break;

                v = v.CompareTo(removed.Base.ModPathEnd) > 0 ? v.Left : v.Right;

                if (v == null) break;
                newlen++;
            }

            if (path[index].Demoted == 1)
            {
                // No attention has to be paid to other paths being to short in this case
                // Demotion continues down the path

                // Now decide if the length of the new path will be at least two.

                if (newlen == 0)
                {
                    // bottom was removed, no action needed
                }
                else if (newlen == 1)
                {
                    if (index > 0 && path[index - 1].Base == removed.Base.ModPathEnd)
                    {
                        WriteDemotion(index - 1);
                    }
                    else
                    {
                        var other = GetDemotionContinuation(path[index]);
                        other.rank--;

                        // Demote child if needed

                        if (other.rank == other.Right?.rank)
                        {
                            other.Right.rank--;
                        }
                        if (other.rank == other.Left?.rank)
                        {
                            other.Left.rank--;
                        }
                    }
                }
                else
                {
                    // Update the whole segment on path
                    for (int i = 0; i < lowerlen; i++)
                    {
                        if (path[index - i].PathStart == path[index])
                        {
                            path[index - i].PathStart = path[index - 1];
                        }
                        else
                        {
                            path[index - i].PathStart2 = path[index - 1];
                        }
                    }

                    // Update the top outside of path

                    var lowertop = GetDemotionContinuation(path[index]);

                    lowertop.DemotionStart2 = lowertop.DemotionStart;
                    lowertop.DemotionStart = true;
                    if (lowertop.ModPathEnd == null)
                    {
                        lowertop.ModPathEnd2 = removed.PathStart.Base.ModPathEnd;
                    }
                    else
                    {
                        lowertop.ModPathEnd = removed.PathStart.Base.ModPathEnd;
                    }
                }

                // Upper part
                if (removed.PathStart == removed)
                {
                    // Nothing to do here.
                }
                else if (removed.PathStart == path[index + 1])
                {
                    WriteDemotion(index + 1);
                }
                else
                {
                    if (removed.PathStart.Demoted == 1)
                    {
                        removed.PathStart.Base.ModPathEnd = path[index + 1].Base;
                    }
                    else
                    {
                        removed.PathStart.Base.ModPathEnd2 = path[index + 1].Base;
                    }
                }

                WriteDemotion(index);
                return;

                void WriteDemotion(int pos)
                {
                    DemoteChildIfNeeded(pos);
                    path[pos].Demoted = false;
                    path[pos].DemotedChild = false;
                    path[pos].PathStart = null;
                    path[pos].Base.rank--;
                    path[pos].Base.ModPathEnd = null;
                }
            }

            // Also determine second path
            v = path[index].Base;
            while (true)
            {
                if (removed.Base.ModPathEnd2 == v) break;

                v = v.CompareTo(removed.Base.ModPathEnd2) > 0 ? v.Left : v.Right;

                if (v == null) break;
                newlen2++;
            }
            for (int i = index; i > 0; i--)
            {
                if (GetDemotionContinuation2(path[i]) == path[i - 1].Base) lowerlen++; else break;
            }

            if (newlen == 0)
            {
                // bottom was removed, no action needed
            }
            else if (newlen == 0 && newlen2 == 1)
            {
                // TODO

                if (index > 0 && path[index - 1].Base == removed.Base.ModPathEnd)
                {
                    WriteDemotion(index - 1);
                }
                else
                {
                    var other = GetDemotionContinuation(path[index]);
                    other.rank--;

                    // Demote child if needed

                    if (other.rank == other.Right?.rank)
                    {
                        other.Right.rank--;
                    }
                    if (other.rank == other.Left?.rank)
                    {
                        other.Left.rank--;
                    }
                }
            }
            else if(newlen == 1 && newlen2 == 1)
            {
                // TODO
            }
            else if(newlen == 1 && newlen2 == 3)
            { 
                // TODO
            }
            else if (newlen == 1 && newlen2 == 3)
            {
                // TODO
            }
            else
            {
                // TODO

                // Update the whole segment on path
                for (int i = 0; i < lowerlen; i++)
                {
                    path[index - i].PathStart = path[index - 1];
                }

                // Update the top outside of path

                var lowetop = GetDemotionContinuation(path[index]);

                lowertop.DemotionStart = true;
                lowertop.ModPathEnd = removed.PathStart.Base.ModPathEnd;
            }

            // Upper part
            if (removed.PathStart2 == path[index])
            {
                // No action needed
            }
            else if (removed.PathStart == path[index] && removed.PathStart2 == path[index + 1])
            {
                DemoteChildIfNeeded(index + 1);
                path[index + 1].Demoted = 0;
                path[index + 1].DemotedChild = false;
                path[index + 1].PathStart = null;
                path[index + 1].PathStart2 = null;
                path[index + 1].Base.rank--;
                path[index + 1].Base.DemotionStart = false;
                path[index + 1].Base.DemotionStart2 = false;
                path[index + 1].Base.ModPathEnd = null;
                path[index + 1].Base.ModPathEnd2 = null;
            }
            else if (removed.PathStart == path[index + 1] && removed.PathStart2 == path[index + 1])
            {
                DemoteChildIfNeeded(index + 1);
                path[index + 1].Demoted = 0;
                path[index + 1].DemotedChild = false;
                path[index + 1].PathStart = null;
                path[index + 1].PathStart2 = null;
                path[index + 1].Base.rank -= 2;
                path[index + 1].Base.DemotionStart = false;
                path[index + 1].Base.DemotionStart2 = false;
                path[index + 1].Base.ModPathEnd = null;
                path[index + 1].Base.ModPathEnd2 = null;
            }
            else if (removed.PathStart == path[index + 1] && removed.PathStart2 == path[index + 2])
            {
                DemoteChildIfNeeded(index + 1);
                path[index + 1].Demoted = 0;
                path[index + 1].DemotedChild = false;
                path[index + 1].PathStart = null;
                path[index + 1].PathStart2 = null;
                path[index + 1].Base.rank -= 2;
                path[index + 1].Base.DemotionStart = false;
                path[index + 1].Base.DemotionStart2 = false;
                path[index + 1].Base.ModPathEnd = null;
                path[index + 1].Base.ModPathEnd2 = null;

                DemoteChildIfNeeded(index + 2);
                path[index + 2].Demoted = 0;
                path[index + 2].DemotedChild = false;
                path[index + 2].PathStart = null;
                path[index + 2].PathStart2 = null;
                path[index + 2].Base.rank--;
                path[index + 2].Base.DemotionStart = false;
                path[index + 2].Base.DemotionStart2 = false;
                path[index + 2].Base.ModPathEnd = null;
                path[index + 2].Base.ModPathEnd2 = null;
            }
            else if (removed.PathStart == path[index + 1])
            {
                DemoteChildIfNeeded(index + 1);
                path[index + 1].Demoted--;
                path[index + 1].DemotedChild = false;
                path[index + 1].PathStart = null;
                path[index + 1].PathStart2 = null;
                path[index + 1].Base.rank -= 2;
                path[index + 1].Base.DemotionStart = false;
                path[index + 1].Base.DemotionStart2 = false;
                path[index + 1].Base.ModPathEnd = null;
                path[index + 1].Base.ModPathEnd2 = null;

                if (removed.PathStart2.Base.ModPathEnd2 == removed.Base)
                {
                    removed.PathStart2.Base.ModPathEnd = path[index + 2].Base;
                }
                else
                {
                    removed.PathStart2.Base.ModPathEnd2 = path[index + 2].Base;
                }
            }
            else
            {
                // Update path endings

                if (removed.PathStart.Base.ModPathEnd == removed.Base)
                {
                    removed.PathStart.Base.ModPathEnd = path[index + 1].Base;
                }
                else
                {
                    removed.PathStart.Base.ModPathEnd2 = path[index + 1].Base;
                }
                if (removed.PathStart2.Base.ModPathEnd2 == removed.Base)
                {
                    removed.PathStart2.Base.ModPathEnd = path[index + 1].Base;
                }
                else
                {
                    removed.PathStart2.Base.ModPathEnd2 = path[index + 1].Base;
                }
            }


            // Update rank at index

            void WriteDemotion(int pos)
            {
                DemoteChildIfNeeded(pos);
                path[pos].Demoted--;
                path[pos].DemotedChild = false;
                path[pos].PathStart = null;
                path[pos].Base.rank--;
                path[pos].Base.ModPathEnd = null;
            }

            Node<K, V> GetDemotionContinuation(FullNode<K, V> node)
            {
                if (node.Base == path[index].PathStart.Base.ModPathEnd)
                {
                    return null;
                }
                return path[index].PathStart.Base.ModPathEnd.CompareTo(node) > 0 ?
                node.Right : node.Left;
            }

            Node<K, V> GetDemotionContinuation2(FullNode<K, V> node)
            {
                if (node.Base == path[index].PathStart.Base.ModPathEnd2)
                {
                    return null;
                }
                return path[index].PathStart.Base.ModPathEnd2.CompareTo(node) > 0 ?
                node.Right : node.Left;
            }

            void DemoteChildIfNeeded(int pos)
            {
                if (path[pos].DemotedChild)
                {
                    var child = path[pos].PathStart.Key.CompareTo(path[pos].Key) > 0 ?
                        path[pos].Base.Right : path[pos].Base.Left;

                    child.rank--;

                    if (pos > 0 && path[pos - 1].Base == child)
                    {

                        path[pos - 1].RankDecreasedByParent == false;
                    }
                }
            }
        }

        private Node<K, V> MoveDemotionUp(List<FullNode<K, V>> path)
        {
            // Value of previous node after demotion
            var lastrank = -1;

            if (path[0].Left != null || path[0].Right == null)
            {

            }
            else
            {
                lastrank = Math.Min(path[0].Right.rank, path[0].Left.rank);
            }

            for (int i = 0; i < path.Count; i++)
            {
                var current = path[i];

                if (current.Promoted)
                {
                    if (current.Rank - lastrank == 2)
                    {
                        FinishDemotionPath(i - 1);
                        // Remove from promotion path and end
                        DeleteFromPromotionPath(path, i);
                        return path.Last().Base;
                    }
                    else
                    {
                        // Rotation will be needed.
                        FinishDemotionPath(i - 2);
                        path[i - 1].Base.rank--;
                        DeleteFromPromotionPath(path, i);
                        return PickRotationDemote(path, i);
                    }
                }
                else if (current.Demoted)
                {
                    if (current.Rank == lastrank + 2)
                    {
                        FinishDemotionPath(i - 1);
                        // 1-son was demoted
                        DeleteFromDemotionPath(path, i);
                        return path.Last().Base;
                    }
                    else
                    {
                        FinishDemotionPath(i - 2);
                        path[i - 1].Base.rank--;
                        DeleteFromDemotionPath(path, i);
                        return PickRotationDemote(path, i);
                    }
                }
                else if (current.RankDecreasedByParent)
                {
                    FinishDemotionPath(i - 1);
                    DeleteFromDemotionPath(path, i + 1);
                    return path.Last().Base;
                }
                else if (current.Rank == lastrank + 3)
                {
                    lastrank = current.Rank - 1;

                    // TODO
                }
                else
                {
                    FinishDemotionPath(i - 1);
                    return path.Last().Base;
                }
            }

            // Top of the path has been reached

            FinishDemotionPath(path.Count - 1);
            return path.Last().Base;

            void FinishDemotionPath(int pos)
            {
                if (pos < 0) return;
                if (pos == 0)
                {
                    path[0].Base.rank--;
                    if (path[0].Base.Left.rank == path[0].Rank) path[0].Base.Left.rank--;
                    if (path[0].Base.Right.rank == path[0].Rank) path[0].Base.Right.rank--;
                    return;
                }

                for (int i = 0; i <= pos; i++)
                {
                    path[i].Demoted = true;
                    path[i].DemotedChild = (path[i].Base.Right?.rank == path[i].Rank) || (path[i].Base.Left?.rank == path[i].Rank);
                    path[i].PathStart = path[pos];
                }

                path[pos].Base.ModPathEnd = path[0].Base;
                path[pos].Base.DemotionStart = true;
            }
        }

        /// <summary>
        /// It assumed that all new modifying paths are complete. 
        /// Further, the invariant should be violated only by such a change that it can be fixed by a rotation at this vertex.
        /// Updates are not written to path
        /// This vertex or its 3-son must not be part of any path
        /// </summary>
        /// <returns>New top of the tree</returns>
        private Node<K, V> PickRotationDemote(List<FullNode<K, V>> path, int i)
        {
            var z = path[i].Base;

            if (z.rank == z.Left.rank + 3)
            {
                var y = z.Right;

                // Left rotation

                // Remove right child from path
                y.CutTopIfNeeded();

                if (y.Right != null)
                {
                    var diff = y.rank - y.Right.rank + (y.Right.PromotionStart ? -1 : 0) + (y.Right.DemotionStart ? 1 : 0);

                    if (diff == 1)
                    {
                        // Single rotation
                        z.Right = y.Left;
                        y.Left = z;
                        z.rank--;
                        y.rank++;

                        return ReturnUpper(y);
                    }
                }

                // Double rotation

                var v = y.Left;

                v.CutTopIfNeeded();

                y.Left = v.Right;
                v.Right = y;
                z.Right = v.Left;
                v.Left = z;

                y.rank--;
                z.rank -= 2;
                v.rank += 2;

                return ReturnUpper(v);
            }
            else
            {
                var y = z.Left;

                // Right rotation

                // Remove left child from path
                y.CutTopIfNeeded();

                if (y.Left != null)
                {
                    var diff = y.rank - y.Left.rank + (y.Left.PromotionStart ? -1 : 0) + (y.Left.DemotionStart ? 1 : 0);

                    if (diff == 1)
                    {
                        // Single rotation
                        z.Left = y.Right;
                        y.Right = z;
                        z.rank--;
                        y.rank++;

                        return ReturnUpper(y);
                    }
                }

                // Double rotation

                var v = y.Right;

                v.CutTopIfNeeded();

                y.Right = v.Left;
                v.Left = y;
                z.Left = v.Right;
                v.Right = z;

                y.rank--;
                z.rank -= 2;
                v.rank += 2;

                return ReturnUpper(v);
            }

            Node<K, V> ReturnUpper(Node<K, V> u)
            {
                if (path.Count = i + 1)
                {
                    return u;
                }
                else
                {
                    if (path[i + 1].CompareTo(x) > 0)
                    {
                        path[i + 1].Base.Left = u;
                    }
                    else
                    {
                        path[i + 1].Base.Right = u;
                    }

                    return path.Last().Base;
                }
            }
        }

        /// <summary>
        /// This vertex or its 0-son must not be part of any path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        Node<K, V> PickRotationPromote(List<FullNode<K, V>> path, int i)
        {
            var z = path[i].Base;
            var x = path[i - 1].Base;

            // Node with rank decreased by parent will never be rotated.

            // We can assume that 0-son is on the path

            if (i == 1)
            {
                // New leaf was inserted.

                var leaf = path[0].Base.Left.rank == 0 ? path[0].Base.Left : path[0].Base.Right;

                if (x.CompareTo(z) > 0)
                {
                    // Right rotation
                    if (x.Right == leaf)
                    {
                        // single
                        x.Left = z;
                        z.Right = null;
                        x.rank++;
                        z.rank--;

                        return ReturnUpper(x);
                    }

                    // double

                    leaf.rank++;
                    z.rank--;
                    x.rank--;

                    leaf.Right = x;
                    leaf.Left = z;
                    x.Left = null;
                    z.Right = null;

                    return ReturnUpper(leaf);
                }
                else
                {
                    // Left rotation
                    if (x.Left == leaf)
                    {
                        // single
                        x.Right = z;
                        z.Left = null;
                        x.rank++;
                        z.rank--;

                        return ReturnUpper(x);
                    }

                    // double

                    leaf.rank++;
                    z.rank--;
                    x.rank--;

                    leaf.Left = x;
                    leaf.Right = z;
                    x.Right = null;
                    z.Left = null;

                    return ReturnUpper(leaf);
                }
            }
            else
            {
                // We can deduce type of rotation from path:

                var y = path[i - 2].Base;

                if (y.CompareTo(x) > 0 && x.CompareTo(z) > 0)
                {
                    // Right simple rotation

                    x.Left = z;
                    z.Right = y;
                    x.rank++;
                    z.rank--;

                    return ReturnUpper(y);
                }

                else if (y.CompareTo(x) < 0 && x.CompareTo(z) < 0)
                {
                    // Left simple rotation

                    x.Right = z;
                    z.Left = y;
                    x.rank++;
                    z.rank--;

                    return ReturnUpper(x);
                }

                else if (x.CompareTo(z) > 0)
                {
                    // Right double rotation

                    var y = x.Left;

                    y.CutTopIfNeeded();

                    z.Right = y.Left;
                    x.Left = y.Right;
                    y.Right = x;
                    y.Left = z;

                    x.rank--;
                    z.rank--;
                    y.rank++;

                    return ReturnUpper(y);
                }

                else
                {
                    // Left double rotation
                    var y = x.Right;

                    y.CutTopIfNeeded();

                    z.Left = y.Right;
                    x.Right = y.Left;
                    y.Left = x;
                    y.Right = z;

                    x.rank--;
                    z.rank--;
                    y.rank++;

                    return ReturnUpper(y);
                }
            }

            Node<K, V> ReturnUpper(Node<K, V> u)
            {
                if (path.Count = i + 1)
                {
                    return u;
                }
                else
                {
                    if (path[i + 1].CompareTo(x) > 0)
                    {
                        path[i + 1].Base.Left = u;
                    }
                    else
                    {
                        path[i + 1].Base.Right = u;
                    }

                    return path.Last().Base;
                }
            }
        }

        /// <summary>
        /// Promote the first vertex and continue upwards.
        /// </summary>
        /// <param name="path"></param>
        Node<K, V> MovePromotionUp(List<FullNode<K, V>> path)
        {
            var lastrank = 0;
            if (path[0].Left != null) lastrank = Math.Max(lastrank, path[0].Left.rank);
            if (path[0].Right != null) lastrank = Math.Max(lastrank, path[0].Right.rank);

            var promotedRankChild = path[i - 1].Rank + 1;

            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].Demoted)
                {
                    // This was a (2,1) vertex. We further know that if its 1 child is now a 0 child and it was demotion of the first kind

                    if (lastrank == path[i].Rank)
                    {
                        // (0,2)-vertex, rotate
                        // End new promotion path.
                        // Remove from Demotion path, implement rank decrease, handle other child, end

                        DeleteFromDemotionPath(path, i);
                        FinishPromotionPath(i - 2);
                        path[i - 1].Base.rank++;
                        return PickRotationPromote(path, i);
                    }
                    else
                    {
                        // (1,1)-vertex, remove from demotion path

                        FinishPromotionPath(i - 1);
                        DeleteFromDemotionPath(path, i);
                        return path.Last().Base;
                    }
                }


                else if (path[i].Promoted)
                {
                    // Cannot happen that promote would be needed.

                    if (promotedRankChild == path[i].Rank)
                    {
                        FinishPromotionPath(i - 2);
                        path[i - 1].Base.rank++;
                        DeleteFromPromotionPath(path, i);
                        // A) 1-child was promoted
                        // Remove from path
                        // Rotate
                        return PickRotationPromote(path, i);
                    }
                    else
                    {
                        FinishPromotionPath(i - 1);
                        // B) 2-child was promoted

                        // Split path in two
                        // finish new path, end
                        DeleteFromPromotionPath(path, i);
                        return path.Last().Base;
                    }



                }

                else if (path[i].RankDecreasedByParent)
                {
                    // Remove parent from demoting path, do rotation at parent
                    DeleteFromPromotionPath(path, i + 1);
                    FinishPromotionPath(i - 1);
                    path[i].Base.rank++;
                    return PickRotationPromote(path, i + 1);
                }

                else if (lastrank < path[i].Rank)
                {
                    // Invariant holds.
                    FinishPromotionPath(i - 1);
                    return path.Last().Base;
                }
                else
                {
                    // Rotation or promotion

                    var other = path[i - 1]
                    }
            }


        }

        void FinishPromotionPath(int index)
        {

        }
    }
}