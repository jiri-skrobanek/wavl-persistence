using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistentWAVL
{
    public class FatNode<K, V> where K : class, IComparable<K>
    {
        public const int SizeLimit = 11;

        public SortedDictionary<VersionHandle, Node<K, V>> Slots = new SortedDictionary<VersionHandle, Node<K, V>>();

        public FatNode(VersionHandle version, Node<K, V> node)
        {
            Slots[version] = node;
        }

        private FatNode()
        { }

        public Node<K, V> GetNodeForVersion(VersionHandle version)
        {
            // We consider that fields apply to own version and some later. 

            var max = Slots.Where(x => x.Key <= version).Max(x => x.Key);
            return Slots[max];
        }

        public void CheckInvariant()
        {
            if (Slots.Count <= SizeLimit) return;

            var queue = new LinkedList<FatNode<K, V>>();

            // The order is irrelevant as the node cannot become more than 
            // twice the size limit before being split at least once
            while (queue.Any())
            {
                var first = queue.First();
                queue.RemoveFirst();

                foreach (var fn in _checkInvariant(first))
                    queue.AddLast(fn);
            }
        }

        private IEnumerable<FatNode<K,V>> _checkInvariant(FatNode<K,V> first)
        {
            if (Slots.Count <= SizeLimit) yield break;

            var slotArray = first.Slots.ToArray();
            var middle = slotArray[(first.Slots.Count + 1) / 2];

            var splittingVersion = middle.Key.GetSuccessor();

            var newNode = new FatNode<K, V>();

            var refs = new HashSet<FatNode<K, V>>();

            foreach (var slot in Slots)
            {
                refs.Add(slot.Value._left);
                refs.Add(slot.Value._right);
                refs.Add(slot.Value._parent);
            }

            foreach (var slot in Slots.Where(x => !(x.Key <= splittingVersion)))
                newNode.Slots.Add(slot.Key, slot.Value);

            foreach (var slot in newNode.Slots)
                Slots.Remove(slot.Key);

            foreach (var r in refs.Where(x => x != null))
            {
                var prior = r.GetNodeForVersion(splittingVersion);
                var splittingNode = new Node<K, V>(splittingVersion, prior);

                foreach (var slot in r.Slots)
                {
                    var val = slot.Value;
                    if (slot.Key > splittingVersion)
                    {
                        if (val._left == this)
                        {
                            val._left = newNode;
                        }

                        if (val._right == this)
                        {
                            val._right = newNode;
                        }

                        if (val._parent == this)
                        {
                            val._parent = newNode;
                        }
                    }
                }
            }

            foreach (var r in refs.Where(x => x != null))
                if (r.Slots.Count > SizeLimit) yield return r;
        }
    }
}
