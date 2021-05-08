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
                this.FatNode = new FatNode(this);
            }

            public Node(K Key, V Value, VersionHandle Version, FatNode FatNode)
            {
                this.Key = Key;
                this.Value = Value;
                this.Version = Version;
                this.FatNode = FatNode;
            }

            public override string ToString() => $"<{Key.ToString()}:{Value.ToString()}>";

            public int CompareTo(Node other) => Key.CompareTo(other.Key);

            public void CopyForVersion(VersionHandle newversion)
            {
                var me = FatNode.Slots.Find(this);

                var old = (Node)MemberwiseClone();
                old.TemporaryAccessor = null;
                if (!(PermanentAccessor is null))
                {
                    // We assume that permanent accessor for newly created version is not set yet.
                    PermanentAccessor.Node = old;
                    PermanentAccessor = null;
                }
                FatNode.Slots.AddBefore(me, old);

                var undo = (Node)MemberwiseClone();
                undo.Version = newversion.GetSuccessor();
                undo.TemporaryAccessor = null;
                FatNode.Slots.AddAfter(me, undo);

                Version = newversion;
            }

            internal Node DuplicateSlot()
            {
                var dup = (Node)MemberwiseClone();
                dup.PermanentAccessor = null;
                dup.TemporaryAccessor = null;
                return dup;
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