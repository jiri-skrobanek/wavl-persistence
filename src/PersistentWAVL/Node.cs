using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentWAVL
{
    /// <summary>
    /// Whenever a write is executed, check version. If version is not equal, copy the node and add slot to fat vertex.
    /// </summary>
    public partial class Tree<K, V> : ITree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        public partial class Node
        {
            #region Fields
            public int rank = 0;

            public bool DemotionStart = false, DemotionStart2 = false;

            public bool PromotionStart = false;

            public K Key { get; set; }

            public V Value { get; set; }

            private K _modPathEnd = null, _modPathEnd2 = null;
            #endregion

            #region Pointers
            internal FatNode _parent, _left, _right;
            #endregion

            /// <summary>
            /// This field points to an accessor that is used by the BST algorithm to work with its appropriate version of this vertex.
            /// </summary>
            internal NodeAccessor TemporaryAccessor;

            /// <summary>
            /// This field points to an accessor that is used by <see cref="Tree{K, V}"/> to store root.
            /// Version of that accessor must match version of this <see cref="Node{K, V}"/>
            /// </summary>
            internal NodeAccessor PermanentAccessor;

            internal VersionHandle Version;

            internal FatNode FatNode;

            public Node(K Key, V Value, VersionHandle Version)
            {
                this.Key = Key;
                this.Value = Value;
                this.Version = Version;
                this.FatNode = new FatNode(Version, this);
            }

            public Node(K Key, V Value, VersionHandle Version, FatNode FatNode)
            {
                this.Key = Key;
                this.Value = Value;
                this.Version = Version;
                this.FatNode = FatNode;
            }

            /// <summary>
            /// Copy constructor, adds to the fat vertex
            /// </summary>
            public Node(VersionHandle Version, Node node)
            {
                node.FatNode.Slots.Add(Version, this);

                DemotionStart = node.DemotionStart;
                DemotionStart2 = node.DemotionStart2;
                FatNode = node.FatNode;
                Key = node.Key;
                PromotionStart = node.PromotionStart;
                rank = node.rank;
                Value = node.Value;
                this.Version = Version;
                _left = node._left;
                _modPathEnd = node._modPathEnd;
                _modPathEnd2 = node._modPathEnd2;
                _parent = node._parent;
                _right = node._right;
            }

            public override string ToString() => $"<{Key.ToString()}:{Value.ToString()}>";

            public int CompareTo(Node other) => Key.CompareTo(other.Key);

            public void CopyForVersion(VersionHandle newversion)
            {
                // We assume that permanent accessor for newly created version is not set yet.
                var old = (Node)MemberwiseClone();
                FatNode.Slots[Version] = old;
                old.TemporaryAccessor = null;
                if (!(PermanentAccessor is null))
                {
                    PermanentAccessor.Node = old;
                    PermanentAccessor = null;
                }


                Version = newversion.GetSuccessor();
                var undo = (Node)MemberwiseClone();
                FatNode.Slots[Version] = undo;
                undo.TemporaryAccessor = null;


                Version = newversion;
                FatNode.Slots[Version] = this;

                // It should not make any difference if this is checked before or after 
                // we write changes differentiating the new slot from the old one.
                FatNode.CheckInvariant();
            }

            internal NodeAccessor GetTemporaryAccessorForVersion(VersionHandle version)
            {
                if (TemporaryAccessor is null)
                    return new NodeAccessor(version, this);

                return TemporaryAccessor;
            }
        }
    }
}