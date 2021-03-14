﻿using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL
{
    /// <summary>
    /// Whenever a write is executed, check version. If version is not equal, copy the node and add slot to fat vertex.
    /// </summary>
    public partial class Node<K, V> : IComparable<Node<K, V>> where K : class, IComparable<K>
    {
        internal static List<NodeAccessor> TemporaryAccessors = new List<NodeAccessor>();

        internal static void RemoveListOfAccessors()
        {
            foreach (var a in TemporaryAccessors)
                a.Node.TemporaryAccessor = null;

            TemporaryAccessors.Clear();
        }

        internal class NodeAccessor
        {
            #region Proper fields
            internal Node<K, V> Node;
            public VersionHandle Version;
            public bool Persistent = false;
            #endregion

            public NodeAccessor(VersionHandle version, Node<K, V> node, bool permanent = false)
            {
                this.Node = node;
                this.Version = version;
                if (permanent)
                {
                    Node.PermanentAccessor = this;
                }
                else
                {
                    TemporaryAccessors.Add(this);
                    Node.TemporaryAccessor = this;
                }
            }

            public int rank
            {
                get => Node.rank;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.rank = value;
                }
            }

            public bool DemotionStart
            {
                get => Node.DemotionStart;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.DemotionStart = value;
                }
            }


            public bool DemotionStart2
            {
                get => Node.DemotionStart2;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.DemotionStart2 = value;
                }
            }

            public bool PromotionStart
            {
                get => Node.PromotionStart;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.PromotionStart = value;
                }
            }

            public K Key
            {
                get => Node.Key;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.Key = value;
                }
            }

            public V Value
            {
                get => Node.Value;
                set
                {
                    if (MustCopy)
                    {
                        Node.CopyForVersion(Version);
                    }

                    Node.Value = value;
                }
            }


            public K ModPathEnd
            {
                get => Node._modPathEnd;
                set
                {
                    if (MustCopy)
                        Node.CopyForVersion(Version);
                    Node._modPathEnd = value;
                }
            }

            public K ModPathEnd2
            {
                get => Node._modPathEnd2;
                set
                {
                    if (MustCopy)
                        Node.CopyForVersion(Version);
                    Node._modPathEnd2 = value;
                }
            }


            #region Pointers
            public NodeAccessor Left
            {
                get => Node._left?.GetNodeForVersion(Version).GetTemporaryAccessorForVersion(Version);
                set
                {
                    if (MustCopy) Node.CopyForVersion(Version);
                    Node._left = null;
                    if (value != null)
                    {
                        var inverseNode = value.Node.FatNode.GetNodeForVersion(Version);
                        // If nothing to set, end.
                        if (inverseNode._parent != Node.FatNode)
                        {
                            if (inverseNode.Version != Version)
                            {
                                inverseNode.CopyForVersion(Version);
                            }
                            inverseNode._parent = Node.FatNode;
                        }
                    }
                    Node._left = value.Node.FatNode;
                }
            }

            public NodeAccessor Right
            {
                get => Node._right?.GetNodeForVersion(Version).GetTemporaryAccessorForVersion(Version);
                set
                {
                    if (MustCopy) Node.CopyForVersion(Version);
                    Node._right = null;
                    if (value != null)
                    {
                        var inverseNode = value.Node.FatNode.GetNodeForVersion(Version);
                        // If nothing to set, end.
                        if (inverseNode._parent != Node.FatNode)
                        {
                            if (inverseNode.Version != Version)
                            {
                                inverseNode.CopyForVersion(Version);
                            }
                            inverseNode._parent = Node.FatNode;
                        }
                    }
                    Node._right = value.Node.FatNode;
                }
            }

            public NodeAccessor Parent => Node._parent?.GetNodeForVersion(Version).GetTemporaryAccessorForVersion(Version);
            #endregion

            private bool MustCopy => this.Version != Node.Version;

            #region Operators
            public static bool operator <(NodeAccessor a, NodeAccessor b) => a.Node.CompareTo(b.Node) < 0;

            public static bool operator >(NodeAccessor a, NodeAccessor b) => a.Node.CompareTo(b.Node) > 0;

            public static bool operator <(NodeAccessor a, K b) => a.Node.Key.CompareTo(b) < 0;

            public static bool operator >(NodeAccessor a, K b) => a.Node.Key.CompareTo(b) > 0;

            public static bool operator <(K a, NodeAccessor b) => a.CompareTo(b.Node.Key) < 0;

            public static bool operator >(K a, NodeAccessor b) => a.CompareTo(b.Node.Key) > 0;

            public static bool operator ==(NodeAccessor a, K b)
            {
                if (a is null && b is null) return true;
                if ((a is null) && !(b is null)) return false;
                if (!(a is null) && (b is null)) return false;
                return !(a < b || a > b);
            }

            public static bool operator !=(NodeAccessor a, K b) => !(a == b);

            public static bool operator !=(K a, NodeAccessor b) => a < b || a > b;

            public static bool operator ==(K a, NodeAccessor b) => !(a < b || a > b);

            public override bool Equals(object obj)
            {
                return obj is NodeAccessor accessor &&
                       EqualityComparer<Node<K, V>>.Default.Equals(Node, accessor.Node);
            }
            #endregion

            public int RankWithOwnOffset => rank + (DemotionStart ? -1 : 0) + (DemotionStart2 ? -1 : 0) + (PromotionStart ? 1 : 0);

            /// <summary>
            /// Remove this vertex from the top of path it is on.
            /// </summary>
            public void CutTopIfNeeded()
            {
                if (ModPathEnd == null) return;

                if (PromotionStart)
                {
                    // Is it short?
                    var next = ModPathEnd > this ? Right : Left;

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
                    var next = ModPathEnd2 > this ? Right : Left;

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
                    var next = ModPathEnd > this ? Right : Left;
                    var other = ModPathEnd > this ? Left : Right;

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

            /// <summary>
            /// Creates a permanent accessor to be stored as root in a tree for the same version as this <see cref="NodeAccessor" />.
            /// </summary>
            /// <returns>Permanent accessor to be stored as root of a tree.</returns>
            internal NodeAccessor GetPermanentAccessor()
            {
                if (Version != Node.Version)
                    Node.CopyForVersion(Version);

                return new NodeAccessor(Version, Node, true);
            }
        }
    }
}
