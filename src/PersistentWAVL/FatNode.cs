using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistentWAVL
{
    /// <summary>
    /// Holds a collection of versions of one vertex.
    /// </summary>
    public partial class Tree<K, V> : ITree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        public class FatNode
        {
            public const int SizeLimit = 11;

            public LinkedList<Node> Slots = new LinkedList<Node>();

            public FatNode Previous;
            public FatNode Next;

            public static ISet<FatNode> CheckScheduled = new HashSet<FatNode>();

            internal static int VertexIdCounter = 0;

            internal int VertexId;

            public FatNode(Node node)
            {
                VertexId = VertexIdCounter++;
                Slots.AddFirst(node);
            }

            private FatNode(FatNode previous)
            {
                Previous = previous;
                Next = previous.Next;
                previous.Next = this;
                if (!(Next is null)) Next.Previous = this;

                VertexId = previous.VertexId;
            }

            public Node GetNodeForVersion(VersionHandle version)
            {
                // We consider that fields apply to own version and some later. 
                return Slots.Last(x => x.Version <= version);
            }

            /// <summary>
            /// Restore invariants of all fat nodes.
            /// </summary>
            public static void FinishUpdate()
            {
                while (CheckScheduled.Any())
                {
                    var n = CheckScheduled.First();
                    CheckScheduled.Remove(n);

                    CheckOne(n);
                }

            }

            private static void CheckOne(FatNode n)
            {
                VersionHandle leftHandle = null;
                Node overlappingLeft = null;

                // Split overlapping slots

                var current = n.Slots.First;

                while (!(current is null))
                {
                    var next = current.Next.Value?.Version;
                    if (next is null && !(n.Next is null))
                    {
                        next = n.Next.Slots.First.Value.Version;
                    }

                    var cn = current.Value;
                    var nextVersion = current.Next.Value.Version;

                    var llow = cn._left?.Slots.First.Value;
                    var lnext = cn._left?.Next?.Slots.First.Value;

                    var rlow = cn._right?.Slots.First.Value;
                    var rnext = cn._right?.Next?.Slots.First.Value;

                    var plow = cn._parent?.Slots.First.Value;
                    var pnext = cn._parent?.Next?.Slots.First.Value;

                    if (!(next is null)
                        && (lnext is null || lnext.Version > next)
                        && (rnext is null || rnext.Version > next)
                        && (pnext is null || pnext.Version > next))
                    {
                        // No overlapping detected for this slot.
                        current = current.Next;
                    }

                    Node changeLeft()
                    {
                        var nn = cn.DuplicateSlot();
                        nn.Version = lnext.Version;
                        nn._left = lnext.FatNode;
                        n.Slots.AddAfter(current, nn);
                        return nn;
                    }

                    Node changeRight()
                    {
                        var nn = cn.DuplicateSlot();
                        nn.Version = rnext.Version;
                        nn._right = rnext.FatNode;
                        n.Slots.AddAfter(current, nn);
                        return nn;
                    }

                    Node changeParent()
                    {
                        var nn = cn.DuplicateSlot();
                        nn.Version = pnext.Version;
                        nn._parent = pnext.FatNode;
                        n.Slots.AddAfter(current, nn);
                        return nn;
                    }


                    if ((lnext is null) && (rnext is null) && (pnext is null))
                        // Every pointer is nonoverlapping
                        ;

                    // Some duplication of this slot is needed.
                    else if (!(lnext is null) && (rnext is null) && (pnext is null))
                    {
                        changeLeft();
                    }
                    else if ((lnext is null) && !(rnext is null) && (pnext is null))
                    {
                        changeRight();
                    }
                    else if ((lnext is null) && (rnext is null) && !(pnext is null))
                    {
                        var nn = cn.DuplicateSlot();
                        nn.Version = pnext.Version;
                        nn._parent = pnext.FatNode;
                        n.Slots.AddAfter(current, nn);
                    }
                    else if (!(lnext is null) && !(rnext is null) && (pnext is null))
                    {
                        if (lnext.Version < rnext.Version)
                        {
                            changeLeft();
                        }
                        else if (lnext.Version > rnext.Version)
                        {
                            changeRight();
                        }
                        else
                        {
                            var nn = changeLeft();
                            nn._right = rnext.FatNode;
                        }
                    }
                    else if (!(lnext is null) && (rnext is null) && !(pnext is null))
                    {
                        if (lnext.Version < pnext.Version)
                        {
                            changeLeft();
                        }
                        else if (lnext.Version > pnext.Version)
                        {
                            changeParent();
                        }
                        else
                        {
                            var nn = changeLeft();
                            nn._parent = pnext.FatNode;
                        }
                    }
                    else if ((lnext is null) && !(rnext is null) && !(pnext is null))
                    {
                        if (lnext.Version < pnext.Version)
                        {
                            changeRight();
                        }
                        else if (lnext.Version > pnext.Version)
                        {
                            changeParent();
                        }
                        else
                        {
                            var nn = changeRight();
                            nn._right = pnext.FatNode;
                        }
                    }
                    else // if (!(lnext is null) && !(rnext is null) && !(pnext is null))
                    {
                        if (lnext.Version == pnext.Version && lnext.Version == rnext.Version)
                        {
                            var nn = changeLeft();
                            nn._right = rnext.FatNode;
                            nn._parent = pnext.FatNode;
                        }
                        else if (lnext.Version < pnext.Version && lnext.Version < rnext.Version)
                        {
                            changeLeft();
                        }
                        else if (rnext.Version < lnext.Version && rnext.Version < pnext.Version)
                        {
                            changeRight();
                        }
                        else if (pnext.Version < lnext.Version && pnext.Version < rnext.Version)
                        {
                            changeParent();
                        }
                        else if (lnext.Version > pnext.Version && lnext.Version > rnext.Version)
                        {
                            var nn = changeRight();
                            nn._right = pnext.FatNode;
                        }
                        else if (rnext.Version > lnext.Version && rnext.Version > pnext.Version)
                        {
                            var nn = changeLeft();
                            nn._parent = pnext.FatNode;
                        }
                        else if (pnext.Version > lnext.Version && pnext.Version > rnext.Version)
                        {
                            var nn = changeLeft();
                            nn._right = rnext.FatNode;
                        }
                    }

                    current = current.Next;
                }

                // Make new fat nodes

                if (n.Slots.Count <= SizeLimit)
                    // Nothing more to be done.
                    return;

                var count = 1 + 2 * (n.Slots.Count / SizeLimit);
                var equalSplit = n.Slots.Count / count;
                var extras = n.Slots.Count % count;

                var slotList = n.Slots;
                n.Slots = new LinkedList<Node>();
                var fn = n;

                var currentSlot = slotList.First;

                var size = equalSplit + (extras-- > 0 ? 1 : 0);

                for (int j = 0; j < size; j++)
                {
                    fn.Slots.AddLast(currentSlot);
                    currentSlot = currentSlot.Next;
                }

                for (int i = 1; i < count; i++)
                {
                    fn = new FatNode(fn);

                    size = equalSplit + (extras-- > 0 ? 1 : 0);

                    for (int j = 0; j < size; j++)
                    {
                        fn.Slots.AddLast(currentSlot);
                        currentSlot.Value.FatNode = fn;
                        currentSlot = currentSlot.Next;
                    }
                }

                // Make pointers proper

                var currentFN = n.Next;
                for (int i = 1; i < count; i++)
                {
                    var firstVersion = currentFN.Slots.First.Value.Version;

                    foreach (var s in currentFN.Slots)
                    {
                        if (!(s._left is null))
                        {
                            foreach (var u in s._left.Slots)
                            {
                                if (u._parent is null) continue;
                                if (u._parent.VertexId == n.VertexId)
                                {
                                    // See if it points to improper version of this vertex.
                                    var higherVersion = u._parent.Next?.Slots.First.Value.Version;
                                    if (!(higherVersion is null) && higherVersion <= u.Version)
                                    {
                                        u._parent = currentFN;
                                    }
                                }
                            }
                        }

                        if (!(s._right is null))
                        {
                            foreach (var u in s._right.Slots)
                            {
                                if (u._parent is null) continue;
                                if (u._parent.VertexId == n.VertexId)
                                {
                                    // See if it points to improper version of this vertex.
                                    var higherVersion = u._parent.Next?.Slots.First.Value.Version;
                                    if (!(higherVersion is null) && higherVersion <= u.Version)
                                    {
                                        u._parent = currentFN;
                                    }
                                }
                            }
                        }

                        if (!(s._parent is null))
                        {
                            foreach (var u in s._parent.Slots)
                            {
                                if (u._left is null) continue;
                                if (u._left.VertexId == n.VertexId)
                                {
                                    // See if it points to improper version of this vertex.
                                    var higherVersion = u._left.Next?.Slots.First.Value.Version;
                                    if (!(higherVersion is null) && higherVersion <= u.Version)
                                    {
                                        u._left = currentFN;
                                    }
                                }
                            }

                            foreach (var u in s._parent.Slots)
                            {
                                if (u._right is null) continue;
                                if (u._right.VertexId == n.VertexId)
                                {
                                    // See if it points to improper version of this vertex.
                                    var higherVersion = u._right.Next?.Slots.First.Value.Version;
                                    if (!(higherVersion is null) && higherVersion <= u.Version)
                                    {
                                        u._right = currentFN;
                                    }
                                }
                            }
                        }
                    }

                    currentFN = currentFN.Next;
                }
            }
        }
    }
}
