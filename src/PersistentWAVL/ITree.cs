using System;

namespace PersistentWAVL
{
    public interface ITree<K, V> where K : class, IComparable<K>, IEquatable<K>
    {
        Tree<K, V> Delete(K Key);
        V Find(K Key);
        Tree<K, V> Insert(K Key, V Value);
    }
}