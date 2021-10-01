using System;

namespace DiffMatchPatch
{
    internal struct HalfMatchResult : IEquatable<HalfMatchResult>
    {
        public HalfMatchResult(ReadOnlyMemory<char> prefix1, ReadOnlyMemory<char> suffix1, ReadOnlyMemory<char> prefix2, ReadOnlyMemory<char> suffix2, ReadOnlyMemory<char> commonMiddle)
        {
            Prefix1 = prefix1;
            Suffix1 = suffix1;
            Prefix2 = prefix2;
            Suffix2 = suffix2;
            CommonMiddle = commonMiddle;
        }

        public HalfMatchResult Reverse()
        {
            return new HalfMatchResult(Prefix2, Suffix2, Prefix1, Suffix1, CommonMiddle);
        }

        public ReadOnlyMemory<char> Prefix1 { get; }
        public ReadOnlyMemory<char> Suffix1 { get; }
        public ReadOnlyMemory<char> CommonMiddle { get; }
        public ReadOnlyMemory<char> Prefix2 { get; }
        public ReadOnlyMemory<char> Suffix2 { get; }
        public bool IsEmpty => CommonMiddle.Length == 0;

        public static readonly HalfMatchResult Empty = new HalfMatchResult();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((HalfMatchResult)obj);
        }

        public bool Equals(HalfMatchResult other)
        {
            return Prefix1.Span.SequenceEqual(other.Prefix1.Span) 
                   && Suffix1.Span.SequenceEqual(other.Suffix1.Span) 
                   && CommonMiddle.Span.SequenceEqual(other.CommonMiddle.Span) 
                   && Prefix2.Span.SequenceEqual(other.Prefix2.Span) 
                   && Suffix2.Span.SequenceEqual(other.Suffix2.Span);
        }
        
        public override int GetHashCode()
        {
            // if this is to be supported it needs to use a hash code function that works on the contents
            // not the memory address
            throw new NotImplementedException("todo");
            unchecked
            {
                var hashCode = Prefix1.GetHashCode();
                hashCode = (hashCode * 397) ^ Suffix1.GetHashCode();
                hashCode = (hashCode * 397) ^ CommonMiddle.GetHashCode();
                hashCode = (hashCode * 397) ^ Prefix2.GetHashCode();
                hashCode = (hashCode * 397) ^ Suffix2.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(HalfMatchResult left, HalfMatchResult right) => Equals(left, right);

        public static bool operator !=(HalfMatchResult left, HalfMatchResult right) => !Equals(left, right);

        public static bool operator >(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length > right.CommonMiddle.Length;

        public static bool operator <(HalfMatchResult left, HalfMatchResult right) => left.CommonMiddle.Length < right.CommonMiddle.Length;

        public override string ToString() => $"[{Prefix1}/{Prefix2}] - {CommonMiddle} - [{Suffix1}/{Suffix2}]";
    }
}
