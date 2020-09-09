using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL
{
    /// <summary>
    /// Whenever a write is executed, check version. If version is not equal, copy the node and add slot to fat vertex.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class Node<K, V> : IComparable<Node<K, V>> where K : IComparable<K>
    {
        #region Fields
        public int rank = 0;

        public bool DemotionStart = false, DemotionStart2 = false;

        public bool PromotionStart = false;

        public K Key { get; set; }

        public V Value { get; set; }
        #endregion

        #region Pointers
        private FatNode<K, V> _modPathEnd = null, _modPathEnd2 = null;

        // 1 and 2 need not mean anything, both have to check every time
        private FatNode<K, V> _modPathStart = null, _modPathStart2 = null;

        private FatNode<K, V> _parent, _left, _right;
        #endregion

        public VersionHandle Version;

        public FatNode<K, V> FatNode;

        public Node(K Key, V Value, VersionHandle Version)
        {
            this.Key = Key;
            this.Value = Value;
            this.Version = Version;
            this.FatNode = new FatNode<K,V>(Version, this);
        }

        public Node(K Key, V Value, VersionHandle Version, FatNode<K,V> FatNode)
        {
            this.Key = Key;
            this.Value = Value;
            this.Version = Version;
            this.FatNode = FatNode;
        }

        public override string ToString() => $"<{Key.ToString()}:{Value.ToString()}>";

        public int CompareTo(Node<K, V> other) => Key.CompareTo(other.Key);

        public void CopyForVersion(VersionHandle newversion)
        {
            FatNode.Slots[Version] = (Node<K,V>)MemberwiseClone();
            Version = newversion.GetSuccessor();
            FatNode.Slots[Version] = (Node<K,V>)MemberwiseClone();
            Version = newversion;
            FatNode.Slots[Version] = this;
            
            // It should not make any difference if this is checked before or after 
            // we write changes differentiating the new slot from the old one.
            FatNode.CheckInvariant();
        }


        public class NodeAccessor
        {
            #region Proper fields
            private Node<K, V> Node;
            public VersionHandle Version;
            #endregion

            public NodeAccessor(VersionHandle version, Node<K, V> node)
            {
                this.Node = node;
                this.Version = version;
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

            #region Pointers
            public NodeAccessor ModPathEnd => new NodeAccessor(Version, Node._modPathEnd.GetNodeForVersion(Version));

            public NodeAccessor ModPathEnd2 => new NodeAccessor(Version, Node._modPathEnd2.GetNodeForVersion(Version));

            public void SetModPathEndAsFirstPathForBottom(NodeAccessor node)
            {
                if (MustCopy) Node.CopyForVersion(Version);
                Node._modPathEnd = null;
                if (node != null)
                {
                    var inverseNode = node.Node.FatNode.GetNodeForVersion(Version);
                    if (inverseNode.Version == Version)
                    {
                        inverseNode.CopyForVersion(Version);
                    }
                    inverseNode._modPathStart = Node.FatNode;
                }
                Node._modPathEnd = node.Node.FatNode;
            }

            public void SetModPathEndAsSecondPathForBottom(NodeAccessor node)
            {
                if (MustCopy) Node.CopyForVersion(Version);
                Node._modPathEnd = null;
                if (node != null)
                {
                    var inverseNode = node.Node.FatNode.GetNodeForVersion(Version);
                    if (inverseNode.Version == Version)
                    {
                        inverseNode.CopyForVersion(Version);
                    }
                    inverseNode._modPathStart2 = Node.FatNode;
                }
                Node._modPathEnd = node.Node.FatNode;
            }

            public void SetModPathEnd2AsFirstPathForBottom(NodeAccessor node)
            {
                if (MustCopy) Node.CopyForVersion(Version);
                Node._modPathEnd2 = null;
                if (node != null)
                {
                    var inverseNode = node.Node.FatNode.GetNodeForVersion(Version);
                    if (inverseNode.Version == Version)
                    {
                        inverseNode.CopyForVersion(Version);
                    }
                    inverseNode._modPathStart = Node.FatNode;
                }
                Node._modPathEnd2 = node.Node.FatNode;
            }

            public void SetModPathEnd2AsSecondPathForBottom(NodeAccessor node)
            {
                if (MustCopy) Node.CopyForVersion(Version);
                Node._modPathEnd2 = null;
                if (node != null)
                {
                    var inverseNode = node.Node.FatNode.GetNodeForVersion(Version);
                    if (inverseNode.Version == Version)
                    {
                        inverseNode.CopyForVersion(Version);
                    }
                    inverseNode._modPathStart2 = Node.FatNode;
                }
                Node._modPathEnd2 = node.Node.FatNode;
            }

            public NodeAccessor Left
            {
                get => new NodeAccessor(Version, Node._left.GetNodeForVersion(Version));
                set
                {
                    if (MustCopy) Node.CopyForVersion(Version);
                    Node._left = null;
                    if (value != null)
                    {
                        var inverseNode = value.Node.FatNode.GetNodeForVersion(Version);
                        if (inverseNode.Version == Version)
                        {
                            inverseNode.CopyForVersion(Version);
                        }
                        inverseNode._parent = Node.FatNode;
                    }
                    Node._modPathEnd2 = value.Node.FatNode;
                }
            }

            public NodeAccessor Right
            {
                get => new NodeAccessor(Version, Node._right.GetNodeForVersion(Version));
                set
                {
                    if (MustCopy) Node.CopyForVersion(Version);
                    Node._right = null;
                    if (value != null)
                    {
                        var inverseNode = value.Node.FatNode.GetNodeForVersion(Version);
                        if (inverseNode.Version == Version)
                        {
                            inverseNode.CopyForVersion(Version);
                        }
                        inverseNode._parent = Node.FatNode;
                    }
                    Node._right = value.Node.FatNode;
                }
            }
            #endregion

            private bool MustCopy => this.Version == Node.Version;

            public static bool operator <(NodeAccessor a, NodeAccessor b) => a.Node.CompareTo(b.Node) < 0;

            public static bool operator >(NodeAccessor a, NodeAccessor b) => a.Node.CompareTo(b.Node) > 0;

            public int RankWithOwnOffset => rank + (DemotionStart ? -1 : 0) + (DemotionStart2 ? -1 : 0) + (PromotionStart ? 1 : 0);

            /// <summary>
            /// Remove this vertex from the top of path it is on.
            /// </summary>
            public void CutTopIfNeeded()
            {
                if (this.ModPathEnd == null) return;

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
        }
    }
}
