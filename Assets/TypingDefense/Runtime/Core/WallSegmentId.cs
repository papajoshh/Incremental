using System;

namespace TypingDefense
{
    public readonly struct WallSegmentId : IEquatable<WallSegmentId>
    {
        public readonly int Ring;
        public readonly int Side; // 0=Top, 1=Bottom, 2=Left, 3=Right
        public readonly int Index;

        public WallSegmentId(int ring, int side, int index)
        {
            Ring = ring;
            Side = side;
            Index = index;
        }

        public bool Equals(WallSegmentId other) =>
            Ring == other.Ring && Side == other.Side && Index == other.Index;

        public override bool Equals(object obj) => obj is WallSegmentId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Ring, Side, Index);

        public static bool operator ==(WallSegmentId a, WallSegmentId b) => a.Equals(b);
        public static bool operator !=(WallSegmentId a, WallSegmentId b) => !a.Equals(b);
    }
}
