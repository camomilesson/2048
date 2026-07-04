using System;

namespace TwentyFortyEight.Core
{
    public readonly struct CellPosition : IEquatable<CellPosition>
    {
        public int Row { get; }
        public int Col { get; }

        public CellPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool Equals(CellPosition other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            return obj is CellPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Col);
        }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }

        public static bool operator ==(CellPosition left, CellPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CellPosition left, CellPosition right)
        {
            return !left.Equals(right);
        }
    }
}