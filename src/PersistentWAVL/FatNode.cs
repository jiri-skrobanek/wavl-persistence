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


        public Node<K,V> GetNodeForVersion(VersionHandle version)
        {
            // We consider that fields apply to own version and spme later. 


            var max = Slots.Where(x => x.Key <= version).Max(x => x.Key);
            return Slots[max];
        }

        public void CheckInvariant()
        {
            if (Slots.Count <= SizeLimit) return;
            
            // Find middle version

            // Split in two nodes

            // Update references in other fat nodes
        }
    }
}
