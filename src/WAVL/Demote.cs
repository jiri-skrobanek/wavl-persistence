using System;
using System.Collections.Generic;
using System.Text;

namespace WAVL
{
    public partial class Tree<K, V> where K : IComparable<K>, IEquatable<K>
    {

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
            var modpathend = removed.PathStart.Base.ModPathEnd;
            Node<K, V> modpathend2 = null;
            if (removed.Demoted == 2) { modpathend2 = removed.PathStart2.Base.DemotionStart2 ? removed.PathStart2.Base.ModPathEnd2 : removed.PathStart2.Base.ModPathEnd; }

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
                if (modpathend == v) break;

                v = v > modpathend ? v.Left : v.Right;

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
                    if (index > 0 && path[index - 1].Base == modpathend)
                    {
                        WriteDemotion(index - 1);
                    }
                    else
                    {
                        var other = GetDemotionContinuation(path[index]);
                        other.rank--;

                        // Demote child if needed
                        // It is not on a path because it is a (1,1)-vertex
                        if (other.rank == other.Right?.RankWithOwnOffset)
                        {
                            other.Right.rank--;
                        }
                        if (other.rank == other.Left?.RankWithOwnOffset)
                        {
                            other.Left.rank--;
                        }
                    }
                }
                else
                {
                    // Update the whole segment on path
                    for (int i = 1; i <= lowerlen; i++)
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
                    path[pos].Demoted = 0;
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
                if (modpathend2 == v) break;

                v = v > modpathend2 ? v.Left : v.Right;

                if (v == null) break;
                newlen2++;
            }
            for (int i = index; i > 0; i--)
            {
                if (GetDemotionContinuation2(path[i]) == path[i - 1].Base) lowerlen++; else break;
            }

            if (newlen2 == 0)
            {
                // bottom was removed, no action needed
            }
            else if (newlen == 0 && newlen2 == 1)
            {
                // Vertex below index is on one path and should be removed.

                if (index > 0 && path[index - 1].Base == modpathend2)
                {
                    // On walking path
                    DemoteChildIfNeeded(index - 1);
                    path[index - 1].Demoted = 0;
                    path[index - 1].DemotedChild = false;
                    path[index - 1].PathStart = null;
                    path[index - 1].PathStart2 = null;
                    path[index - 1].Base.rank--;
                    path[index - 1].Base.DemotionStart = false;
                    path[index - 1].Base.DemotionStart2 = false;
                    path[index - 1].Base.ModPathEnd = null;
                    path[index - 1].Base.ModPathEnd2 = null;
                }
                else
                {
                    // Not on walking path
                    var other = GetDemotionContinuation2(removed);
                    other.rank--;

                    // Demote child if needed

                    if (other.rank == other.Right?.RankWithOwnOffset)
                    {
                        other.Right.rank--;
                    }
                    if (other.rank == other.Left?.RankWithOwnOffset)
                    {
                        other.Left.rank--;
                    }
                }
            }
            else if (newlen == 1 && newlen2 == 1)
            {
                // Vertex below index is on two paths and should be removed.

                if (index > 0 && path[index - 1].Base == modpathend)
                {
                    // On walking path
                    DemoteChildIfNeeded(index - 1);
                    path[index - 1].Demoted = 0;
                    path[index - 1].DemotedChild = false;
                    path[index - 1].PathStart = null;
                    path[index - 1].PathStart2 = null;
                    path[index - 1].Base.rank -= 2;
                    path[index - 1].Base.DemotionStart = false;
                    path[index - 1].Base.DemotionStart2 = false;
                    path[index - 1].Base.ModPathEnd = null;
                    path[index - 1].Base.ModPathEnd2 = null;
                }
                else
                {
                    // Not on walking path
                    var other = GetDemotionContinuation(removed);
                    other.rank -= 2;

                    // Demote child if needed

                    if (other.rank == other.Right?.RankWithOwnOffset)
                    {
                        other.Right.rank--;
                    }
                    if (other.rank == other.Left?.RankWithOwnOffset)
                    {
                        other.Left.rank--;
                    }
                }
            }
            else if (newlen == 1)
            {
                if (index > 0 && path[index - 1].Base == modpathend2)
                {
                    // Demote first son once and update the rest of second path
                    DemoteChildIfNeeded(index - 1);
                    path[index - 1].Demoted = 1;
                    path[index - 1].DemotedChild = false;
                    path[index - 1].PathStart = path[index - 1].PathStart2;
                    path[index - 1].PathStart2 = null;
                    path[index - 1].Base.rank--;
                    path[index - 1].Base.DemotionStart = true;
                    path[index - 1].Base.ModPathEnd = modpathend2;
                    path[index - 1].Base.ModPathEnd2 = null;

                    // Update the whole segment on path
                    for (int i = 2; i <= lowerlen2; i++)
                    {
                        if (path[index - i].PathStart2 == removed.PathStart2)
                        {
                            path[index - i].PathStart2 = path[index - 1];
                        }
                        else
                        {
                            path[index - i].PathStart = path[index - 1];
                        }
                    }
                }
                else
                {
                    // Not on walking path
                    var other = GetDemotionContinuation(path[index]);
                    other.rank--;
                    other.ModPathEnd = modpathend2;
                    other.DemotionStart = true;

                    // one child must be demoted 
                    if (other > modpathend2)
                    {
                        other.Right.rank--;
                    }
                    else
                    {
                        other.Left.rank--;
                    }
                }
            }
            else
            {
                if (index > 0 && path[index - 1].Base == modpathend2)
                {
                    // Demote first son once and update the rest of second path
                    path[index - 1].Base.DemotionStart = true;
                    path[index - 1].Base.DemotionStart2 = true;
                    path[index - 1].Base.ModPathEnd = modpathend;
                    path[index - 1].Base.ModPathEnd2 = modpathend2;

                    // Update the whole segment for both paths
                    for (int i = 1; i <= lowerlen2; i++)
                    {
                        if (path[index - i].PathStart == removed.PathStart2)
                        {
                            path[index - i].PathStart = path[index - 1];
                        }
                        else
                        {
                            path[index - i].PathStart2 = path[index - 1];
                        }
                    }
                    for (int i = 1; i <= lowerlen; i++)
                    {
                        if (path[index - i].PathStart == removed.PathStart)
                        {
                            path[index - i].PathStart = path[index - 1];
                        }
                        else
                        {
                            path[index - i].PathStart2 = path[index - 1];
                        }
                    }
                }
                else
                {
                    // Update the top outside of path
                    var lowertop = GetDemotionContinuation(path[index]);

                    lowertop.DemotionStart = true;
                    lowertop.DemotionStart2 = true;
                    lowertop.ModPathEnd = modpathend;
                    lowertop.ModPathEnd2 = modpathend2;
                }
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
            else if (removed.PathStart == path[index + 1])
            {
                // Remove first path, replace by second for this vertex
                DemoteChildIfNeeded(index + 1);
                path[index + 1].Demoted--;
                path[index + 1].DemotedChild = false;
                path[index + 1].PathStart = path[index + 1].PathStart2;
                path[index + 1].PathStart2 = null;
                path[index + 1].Base.rank -= 1;
                path[index + 1].Base.DemotionStart = path[index + 1].Base.DemotionStart2;
                path[index + 1].Base.DemotionStart2 = false;
                path[index + 1].Base.ModPathEnd = path[index + 1].Base.ModPathEnd2;
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

            void WriteDemotionBoth(int pos)
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
                return path[index].PathStart.Base.ModPathEnd > node.Base ?
                node.Right : node.Left;
            }

            Node<K, V> GetDemotionContinuation2(FullNode<K, V> node)
            {
                if (node.Base == path[index].PathStart.Base.ModPathEnd2)
                {
                    return null;
                }
                return path[index].PathStart.Base.ModPathEnd2 > node.Base ?
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

                        path[pos - 1].RankDecreasedByParent = false;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a demotion path from a chain of demotions done at path
        /// </summary>
        /// <returns>New root of the tree</returns>
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
                    // Special leaf case first
                    if (path[i].Left == null && path[i].Right == null)
                    {
                        DeleteFromPromotionPath(path, 0);
                        path[0].Base.rank = 0;
                        if (path.Count == 1) return path[0].Base;
                        path.RemoveAt(0);
                        lastrank = 0;
                        continue;
                    }

                    if (current.Rank - lastrank == 2)
                    {
                        FinishDemotionPath(i - 1);
                        // Remove from promotion path and end
                        DeleteFromPromotionPath(path, i);
                        return path.Last().Base;
                    }
                    else
                    {

                        DemoteWithChild(i - 1);
                        // Rotation will be needed.
                        FinishDemotionPath(i - 2);

                        DeleteFromPromotionPath(path, i);
                        return PickRotationDemote(path, i);
                    }
                }
                else if (current.Demoted == 2)
                {
                    // Must be at least rank 2
                    // Must have come from 3-son

                    DemoteWithChild(i - 1);
                    FinishDemotionPath(i - 2);
                    DeleteFromDemotionPath(path, i);
                    return PickRotationDemote(path, i);
                }
                else if (current.Demoted == 1)
                {
                    // Special leaf case first
                    if (path[i].Left == null && path[i].Right == null)
                    {
                        DeleteFromDemotionPath(path, 0);
                        path[0].Base.rank = 0;
                        if (path.Count == 1) return path[0].Base;
                        path.RemoveAt(0);
                        lastrank = 0;
                        continue;
                    }

                    if (current.Rank == lastrank + 2)
                    {
                        FinishDemotionPath(i - 1);
                        // 1-son was demoted
                        DeleteFromDemotionPath(path, i);
                        return path.Last().Base;
                    }
                    else
                    {
                        // 3-son is marked
                        // Examine children of 1-child
                        var (l, r) = GetTypeOf1ChildVertex(current.Base);

                        if (r == l && r + 2 == current.Rank)
                        {
                            // Second demote here
                            lastrank = current.Rank - 1;
                            continue;
                        }
                        else if (i == 0)
                        {
                            // Rotation
                            DeleteFromDemotionPath(path, 0);
                            return PickRotationDemote(path, 0);
                        }
                        else
                        {
                            // Rotation
                            DemoteWithChild(i - 1);
                            FinishDemotionPath(i - 2);
                            DeleteFromDemotionPath(path, i);
                            return PickRotationDemote(path, i);
                        }
                    }
                }
                else if (current.RankDecreasedByParent)
                {
                    FinishDemotionPath(i - 1);
                    DeleteFromDemotionPath(path, i + 1);
                    return path.Last().Base;
                }
                else if (path[i].Left == null && path[i].Right == null)
                {
                    path[0].Base.rank = 0;
                    if (path.Count == 1) return path[0].Base;
                    path.RemoveAt(0);
                    lastrank = 0;
                    continue;
                }
                else if (current.Rank == lastrank + 3)
                {
                    var (l, r) = GetTypeOf1ChildVertex(current.Base);

                    if (current.Rank - l == 2 || current.Rank - r == 2)
                    {
                        // Rotate
                        DemoteWithChild(i - 1);
                        FinishDemotionPath(i - 2);
                        return PickRotationDemote(path, i);
                    }
                    else
                    {
                        // Demote
                        lastrank = current.Rank - 1;
                        continue;
                    }
                }
                else
                {
                    // Invariant holds
                    FinishDemotionPath(i - 1);
                    return path.Last().Base;
                }
            }

            // Top of the path has been reached

            FinishDemotionPath(path.Count - 1);
            return path.Last().Base;

            (int r, int l) GetTypeOf1ChildVertex(Node<K, V> node)
            {
                Node<K, V> onechild;
                if (node.Left != null && node.Left.RankWithOwnOffset > lastrank)
                    onechild = node.Left;
                else onechild = node.Right;

                int r = -1, l = -1;

                if (onechild.Right != null)
                {
                    r = onechild.Right.RankWithOwnOffset;
                }
                if (onechild.Left != null)
                {
                    l = onechild.Left.RankWithOwnOffset;
                }

                if (onechild.PromotionStart)
                {
                    if (onechild.ModPathEnd > onechild) r++;
                    else l++;
                }

                if (onechild.DemotionStart)
                {
                    if (onechild.ModPathEnd > onechild) r++;
                    else l++;
                }

                if (onechild.DemotionStart2)
                {
                    if (onechild.ModPathEnd2 > onechild) r++;
                    else l++;
                }

                return (r, l);
            }

            void DemoteWithChild(int index)
            {
                if (index < 0) return;
                FullNode<K, V> node = path[index];
                Node<K, V> onechild = null;
                if (node.Demoted == 1)
                {
                    // Demoted == 1
                    DeleteFromDemotionPath(path, index);
                }

                if (node.Left != null && node.Left.RankWithOwnOffset + 1 == node.Rank)
                    onechild = node.Left;
                if (node.Right != null && node.Right.RankWithOwnOffset + 1 == node.Rank)
                    onechild = node.Right;

                if (onechild != null)
                {
                    onechild.rank--;
                }

                node.Base.rank--;
            }

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
                    if (path[i].Demoted == 1)
                    {
                        path[i].PathStart2 = path[pos];
                        path[i].DemotedChild = true;
                    }
                    else
                    {
                        path[i].PathStart = path[pos];
                        path[i].DemotedChild = (path[i].Base.Right?.rank == path[i].Rank) || (path[i].Base.Left?.rank == path[i].Rank);
                    }

                    path[i].Demoted++;

                }

                if (path[pos].Base.ModPathEnd != null)
                {
                    path[pos].Base.ModPathEnd2 = path[0].Base;
                    path[pos].Base.DemotionStart2 = true;
                }
                else
                {
                    path[pos].Base.ModPathEnd = path[0].Base;
                    path[pos].Base.DemotionStart = true;
                }
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

            if (z.rank == z.Left.RankWithOwnOffset + 3)
            {
                var y = z.Right;

                // Left rotation

                // Remove right child from path
                y.CutTopIfNeeded();

                if (y.Right != null)
                {
                    var diff = y.rank - y.Right.RankWithOwnOffset;

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
                    var diff = y.rank - y.Left.RankWithOwnOffset;

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
                if (path.Count == i + 1)
                {
                    return u;
                }
                else
                {
                    if (path[i + 1].Base > u)
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
    }
}