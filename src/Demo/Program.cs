using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersistentWAVL.Demo
{
    [DebuggerDisplay("{val}")]
    class IntClass : IComparable<IntClass>, IEquatable<IntClass>
    {
        public int val;

        public int CompareTo([AllowNull] IntClass other) => val.CompareTo(other?.val);

        public bool Equals([AllowNull] IntClass other) => val == other?.val;

        public static implicit operator IntClass(int val) => new IntClass { val = val };

        public override int GetHashCode()
        {
            return val;
        }

        public override string ToString() => val.ToString();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var t0 = Tree<IntClass, int>.GetNew;
            var t1 = t0.Insert(100, 100);
            var t2 = t1.Insert(200, 200);
            var t3 = t2.Insert(0, 0);
            var t1point5 = t1.Insert(0, -1);

            var right1 = t1.Root.Right;

            Console.WriteLine("Hello World!");
        }
    }
}
