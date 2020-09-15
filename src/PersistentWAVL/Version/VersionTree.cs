using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Text;

namespace PersistentWAVL.Version
{
    [DebuggerDisplay("VersionNode depth = {depth} key = {key>>63-depth}")]
    internal class VersionNode : IComparable<VersionNode>
    {
        private const ulong alpha1 = 3, alpha2 = 2;

        VersionNode Left;
        VersionNode Right;
        VersionNode Parent;
        int size = 1;
        int depth = 0;
        ulong key = 1UL << 63;

        internal VersionNode()
        { }

        internal static VersionNode FromList(VersionNode parent, ReadOnlySpan<VersionNode> list, ulong right)
        {
            if (list.Length == 0)
                return null;
            var middle = list[(list.Length-1) / 2];
            if (parent == null)
            {
                middle.depth = 0;
                middle.key = 1UL << 63;
                middle.Parent = null;
            }
            else
            {
                middle.depth = parent.depth + 1;
                middle.key = parent.key + right * (1UL << (63 - middle.depth));
                middle.Parent = parent;
            }
            middle.Left = FromList(middle, list.Slice(0, (list.Length - 1) / 2 ), 0);
            middle.Right = FromList(middle, list.Slice((list.Length + 1) / 2), 1);
            middle.size = list.Length;
            return middle;
        }

        private void rebuildAt(VersionNode treeNode)
        {
            var parent = treeNode.Parent;
            var nodes = new VersionNode[treeNode.size];
            preorderListing(treeNode, nodes, 0);
            if (parent == null)
            {
                FromList(null, nodes, 1);
            }
            else if (parent.Left == treeNode)
                parent.Left = FromList(parent, nodes, 0);
            else
                parent.Right = FromList(parent, nodes, 1);

            static void preorderListing(VersionNode node, VersionNode[] list, int index)
            {
                if (node.Left != null)
                {
                    preorderListing(node.Left, list, index);
                    index += node.Left.size;
                }
                list[index] = node;
                if (node.Right != null)
                {
                    preorderListing(node.Right, list, index + 1);
                }
            }
        }

        internal VersionNode GetSuccessor() => insert(this, this.key);

        internal VersionNode insert(VersionNode treeNode, ulong key)
        {
            VersionNode current = treeNode, target = null;
            while (current.Parent != null)
                current = current.Parent;
            while (true)
            {
                if (key < current.key)
                {
                    if (current.Left != null)
                    {
                        current = current.Left;
                        continue;
                    }
                    target = current.Left = new VersionNode()
                    {
                        size = 1,
                        key = current.key,
                        depth = current.depth + 1,
                        Parent = current
                    };
                    break;
                }
                else
                {
                    if (current.Right != null)
                    {
                        current = current.Right;
                        continue;
                    }
                    target = current.Right = new VersionNode()
                    {
                        size = 1,
                        key = current.key + (1UL << 63 - current.depth),
                        depth = current.depth + 1,
                        Parent = current,
                    };
                    break;
                }
            }
            current.size++;
            while (current.Parent != null)
            {
                current.Parent.size++;
                current = current.Parent;
            }
            while (current != target)
            {
                if (current.CompareTo(target) < 0)
                {
                    if ((ulong)current.Right.size * alpha1 > (ulong)current.size * alpha2)
                    {
                        rebuildAt(current);
                        break;
                    }
                    current = current.Right;
                }
                else
                {
                    if ((ulong)current.Left.size * alpha1 > (ulong)current.size * alpha2)
                    {
                        rebuildAt(current);
                        break;
                    }
                    current = current.Left;
                }
            }
            return target;
        }

        public int CompareTo(VersionNode other) 
        {
            var d = Math.Min(depth, other.depth);
            if((key >> 63 - d) > (other.key >> 63 - d))
            {
                return 1;
            }
            if ((key >> 63 - d) < (other.key >> 63 - d))
            {
                return -1;
            }
            // Ancestor/Descendant relationship
            if(depth == other.depth)
                return 0;
            if (depth > other.depth)
                return ((key >> 63 - d - 1) & 1UL) == 1 ? 1 : -1;
            return ((other.key >> 63 - d - 1) & 1UL) == 1 ? -1 : 1;
        }
    }
}