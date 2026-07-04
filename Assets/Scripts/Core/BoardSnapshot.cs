using System;

namespace TwentyFortyEight.Core
{
    public sealed class BoardSnapshot
    {
        public readonly struct CellSnapshot
        {
            public bool HasTile { get; }
            public int TileId { get; }
            public int Value { get; }

            public CellSnapshot(bool hasTile, int tileId, int value)
            {
                if (hasTile && value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "Occupied snapshot cells must have a positive value."
                    );
                }

                HasTile = hasTile;
                TileId = tileId;
                Value = value;
            }

            public static CellSnapshot Empty => new CellSnapshot(false, 0, 0);

            public static CellSnapshot FromTile(TileData tile)
            {
                if (tile == null)
                {
                    return Empty;
                }

                return new CellSnapshot(true, tile.Id, tile.Value);
            }

            public TileData ToTileData()
            {
                if (!HasTile)
                {
                    return null;
                }

                return new TileData(TileId, Value);
            }

            public override string ToString()
            {
                return HasTile ? $"{Value}#{TileId}" : ".";
            }
        }

        private readonly CellSnapshot[,] cells;

        public int Size { get; }

        public BoardSnapshot(CellSnapshot[,] cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            int rows = cells.GetLength(0);
            int cols = cells.GetLength(1);

            if (rows != cols)
            {
                throw new ArgumentException(
                    "Board snapshot must be square.",
                    nameof(cells)
                );
            }

            Size = rows;
            this.cells = new CellSnapshot[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    this.cells[row, col] = cells[row, col];
                }
            }
        }

        public CellSnapshot GetCell(int row, int col)
        {
            ValidatePosition(row, col);
            return cells[row, col];
        }

        public CellSnapshot GetCell(CellPosition position)
        {
            return GetCell(position.Row, position.Col);
        }

        public CellSnapshot[,] CopyCells()
        {
            CellSnapshot[,] copy = new CellSnapshot[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    copy[row, col] = cells[row, col];
                }
            }

            return copy;
        }

        private void ValidatePosition(int row, int col)
        {
            if (row < 0 || row >= Size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(row),
                    $"Row must be between 0 and {Size - 1}."
                );
            }

            if (col < 0 || col >= Size)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(col),
                    $"Column must be between 0 and {Size - 1}."
                );
            }
        }
    }
}