using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PersistentWAVL
{
    public partial class Tree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {

        private void DeleteFromPromotionPath(List<FullNode<K, V>> path, int index)
        {
            var removed = path[index];
            var end = removed.PathStart.Base.ModPathEnd;

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

                v = v > removed.Base.ModPathEnd ? v.Left : v.Right;

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
                lowertop.ModPathEnd = end;
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
                removed.PathStart.Base.ModPathEnd = path[index + 1].Base.Key;
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

            Node<K, V>.NodeAccessor GetPromotionContinuation(FullNode<K, V> node)
            {
                if (node.Base == node.PathStart.Base.ModPathEnd)
                {
                    return null;
                }
                return node.PathStart.Base.ModPathEnd > node.Base ?
                node.Right : node.Left;
            }
        }

        /// <summary>
        /// This vertex or its 0-son must not be part of any path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        Node<K, V>.NodeAccessor PickRotationPromote(List<FullNode<K, V>> path, int i)
        {
            var z = path[i].Base;
            var x = path[i - 1].Base;

            // Node with rank decreased by parent will never be rotated.

            // We can assume that 0-son is on the path

            if (i == 1)
            {
                // New leaf was inserted.

                var leaf = path[0].Base.Left.rank == 0 ? path[0].Base.Left : path[0].Base.Right;

                if (x > z)
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

                if (y > x && x > z)
                {
                    // Right simple rotation

                    x.Left = z;
                    z.Right = y;
                    x.rank++;
                    z.rank--;

                    return ReturnUpper(y);
                }

                else if (y < x && x < z)
                {
                    // Left simple rotation

                    x.Right = z;
                    z.Left = y;
                    x.rank++;
                    z.rank--;

                    return ReturnUpper(x);
                }

                else if (x > z)
                {
                    // Right double rotation

                    y = x.Left;

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
                    y = x.Right;

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

            Node<K, V>.NodeAccessor ReturnUpper(Node<K, V>.NodeAccessor u)
            {
                if (path.Count == i + 1)
                {
                    return u;
                }
                else
                {
                    if (path[i + 1].Base > x)
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
        private Node<K, V>.NodeAccessor MovePromotionUp(List<FullNode<K, V>> path)
        {
            var lastrank = 0;
            if (path[0].Left != null) lastrank = Math.Max(lastrank, path[0].Left.rank);
            if (path[0].Right != null) lastrank = Math.Max(lastrank, path[0].Right.rank);

            for (int i = 0; i < path.Count; i++)
            {
                if (path[i].Demoted > 0)
                {
                    // This was a (2,1) vertex. We further know that if its 1 child is now a 0 child and it was demotion of the first kind

                    if (lastrank == path[i].Rank)
                    {
                        // (0,2)-vertex, rotate
                        // End new promotion path.
                        // Remove from Demotion path, implement rank decrease, handle other child, end

                        DeleteFromDemotionPath(path, i);
                        FinishPromotionPath(path, i - 2);
                        PromoteAt(i - 1);
                        return PickRotationPromote(path, i);
                    }
                    else
                    {
                        // (1,1)-vertex, remove from demotion path

                        FinishPromotionPath(path, i - 1);
                        DeleteFromDemotionPath(path, i);
                        return path.Last().Base;
                    }
                }

                else if (path[i].Promoted)
                {
                    // Cannot happen that promote would be needed.

                    if (lastrank == path[i].Rank)
                    {
                        FinishPromotionPath(path, i - 2);
                        path[i - 1].Base.rank++;
                        DeleteFromPromotionPath(path, i);
                        // A) 1-child was promoted
                        // Remove from path
                        // Rotate
                        return PickRotationPromote(path, i);
                    }
                    else
                    {
                        FinishPromotionPath(path, i - 1);
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
                    FinishPromotionPath(path, i - 1);
                    // (0,1) change to (1,2)
                    path[i].Base.rank++;
                    return PickRotationPromote(path, i + 1);
                }

                else if (lastrank < path[i].Rank)
                {
                    // Invariant holds.
                    FinishPromotionPath(path, i - 1);
                    return path.Last().Base;
                }
                else
                {
                    if (i == 0)
                    {
                        // Leaf got a child, add to promotion path candidades
                        lastrank = path[i].Rank + 1;
                        continue;
                    }
                    else if (path[i].Left == path[i - 1].Base)
                    {
                        // See the true rank of right child
                        if (path[i].Right.RankWithOwnOffset + 1 == path[i].Rank)
                        {
                            // Promote
                            lastrank = path[i].Rank + 1;
                            continue;
                        }
                        else
                        {
                            // Rotation
                            FinishPromotionPath(path, i - 2);
                            PromoteAt(i - 1);
                            return PickRotationPromote(path, i);
                        }
                    }
                    else
                    {
                        if (path[i].Left.RankWithOwnOffset + 1 == path[i].Rank)
                        {
                            // Promote
                            lastrank = path[i].Rank + 1;
                            continue;
                        }
                        else
                        {
                            // Rotation
                            FinishPromotionPath(path, i - 2);
                            PromoteAt(i - 1);
                            return PickRotationPromote(path, i);
                        }
                    }
                }
            }

            throw new ArgumentException("Empty path");

            void PromoteAt(int index)
            {
                if (index < 0) return;
                path[index].Base.rank++;
            }
        }

        private void FinishPromotionPath(List<FullNode<K, V>> path, int index)
        {
            if (index < 0) return;
            if (index == 0)
            {
                path[0].Base.rank++;
                return;
            }

            for (int i = 0; i <= index; i++)
            {
                path[i].Promoted = true;
                path[i].PathStart = path[index];
            }

            path[index].Base.ModPathEnd = path[0].Base.Key;
            path[index].Base.PromotionStart = true;
        }
    }
}
