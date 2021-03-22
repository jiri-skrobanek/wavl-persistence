using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace PersistentWAVL.Tests
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
}