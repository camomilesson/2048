namespace TwentyFortyEight.Core
{
    public sealed class TileMovement
    {
        public CellPosition From { get; }
        public CellPosition To { get; }

        public TileMovement(CellPosition from, CellPosition to)
        {
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
}