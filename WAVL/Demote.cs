﻿using System;
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
            else if (newlen == 1 && newlen2 == 1)
            {
                // TODO
            }
            else if (newlen == 1 && newlen2 == 3)
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
                return path[index].PathStart.Base.ModPathEnd.CompareTo(node.Base) > 0 ?
                node.Right : node.Left;
            }

            Node<K, V> GetDemotionContinuation2(FullNode<K, V> node)
            {
                if (node.Base == path[index].PathStart.Base.ModPathEnd2)
                {
                    return null;
                }
                return path[index].PathStart.Base.ModPathEnd2.CompareTo(node.Base) > 0 ?
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
                else if (current.Demoted > 0)
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
                if (path.Count == i + 1)
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
    }
}