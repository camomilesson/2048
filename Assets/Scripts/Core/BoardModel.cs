using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyFortyEight.Core
{
    public sealed class BoardModel
    {
        public const int DefaultSize = 5;

        private readonly TileData[,] cells;
        private int nextTileId;

        public int Size { get; }

        public BoardModel(int size = DefaultSize)
        {
            if (size < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    "Board size must be at least 2."
                );
            }

            Size = size;
            cells = new TileData[Size, Size];
            nextTileId = 0;
        }

        public TileData GetTile(int row, int col)
        {
            ValidatePosition(row, col);
            return cells[row, col];
        }

        public TileData GetTile(CellPosition position)
        {
            return GetTile(position.Row, position.Col);
        }

        public bool HasTile(int row, int col)
        {
            ValidatePosition(row, col);
            return cells[row, col] != null;
        }

        public bool HasTile(CellPosition position)
        {
            return HasTile(position.Row, position.Col);
        }

        public void SetTile(int row, int col, TileData tile)
        {
            ValidatePosition(row, col);

            if (tile == null)
            {
                throw new ArgumentNullException(nameof(tile));
            }

            cells[row, col] = tile;

            if (tile.Id >= nextTileId)
            {
                nextTileId = tile.Id + 1;
            }
        }

        public void SetTile(CellPosition position, TileData tile)
        {
            SetTile(position.Row, position.Col, tile);
        }

        public void ClearCell(int row, int col)
        {
            ValidatePosition(row, col);
            cells[row, col] = null;
        }

        public void ClearCell(CellPosition position)
        {
            ClearCell(position.Row, position.Col);
        }

        public void Clear()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    cells[row, col] = null;
                }
            }

            nextTileId = 0;
        }

        public TileData SpawnTile(CellPosition position, int value)
        {
            ValidatePosition(position);

            if (HasTile(position))
            {
                throw new InvalidOperationException(
                    $"Cannot spawn tile at {position}: cell is already occupied."
                );
            }

            TileData tile = new TileData(nextTileId, value);
            nextTileId++;

            cells[position.Row, position.Col] = tile;

            return tile;
        }

        public List<CellPosition> GetEmptyPositions()
        {
            List<CellPosition> emptyPositions = new List<CellPosition>();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (cells[row, col] == null)
                    {
                        emptyPositions.Add(new CellPosition(row, col));
                    }
                }
            }

            return emptyPositions;
        }

        public List<CellPosition> GetOccupiedPositions()
        {
            List<CellPosition> occupiedPositions = new List<CellPosition>();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (cells[row, col] != null)
                    {
                        occupiedPositions.Add(new CellPosition(row, col));
                    }
                }
            }

            return occupiedPositions;
        }

        public bool HasEmptyCells()
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (cells[row, col] == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int GetHighestTileValue()
        {
            int highest = 0;

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    TileData tile = cells[row, col];

                    if (tile != null && tile.Value > highest)
                    {
                        highest = tile.Value;
                    }
                }
            }

            return highest;
        }

        public bool HasAvailableMoves()
        {
            if (HasEmptyCells())
            {
                return true;
            }

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    TileData current = cells[row, col];

                    if (current == null)
                    {
                        continue;
                    }

                    bool hasMatchingRightNeighbor =
                        col + 1 < Size &&
                        cells[row, col + 1] != null &&
                        cells[row, col + 1].Value == current.Value;

                    if (hasMatchingRightNeighbor)
                    {
                        return true;
                    }

                    bool hasMatchingBottomNeighbor =
                        row + 1 < Size &&
                        cells[row + 1, col] != null &&
                        cells[row + 1, col].Value == current.Value;

                    if (hasMatchingBottomNeighbor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public BoardSnapshot CreateSnapshot()
        {
            BoardSnapshot.CellSnapshot[,] snapshotCells =
                new BoardSnapshot.CellSnapshot[Size, Size];

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    snapshotCells[row, col] =
                        BoardSnapshot.CellSnapshot.FromTile(cells[row, col]);
                }
            }

            return new BoardSnapshot(snapshotCells);
        }

        public void RestoreSnapshot(BoardSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.Size != Size)
            {
                throw new ArgumentException(
                    $"Snapshot size {snapshot.Size} does not match board size {Size}.",
                    nameof(snapshot)
                );
            }

            int highestTileId = -1;

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    BoardSnapshot.CellSnapshot cellSnapshot = snapshot.GetCell(row, col);
                    TileData tile = cellSnapshot.ToTileData();

                    cells[row, col] = tile;

                    if (tile != null && tile.Id > highestTileId)
                    {
                        highestTileId = tile.Id;
                    }
                }
            }

            nextTileId = highestTileId + 1;
        }

        public string ToDebugString()
        {
            StringBuilder builder = new StringBuilder();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    TileData tile = cells[row, col];
                    string value = tile == null ? "." : tile.Value.ToString();

                    builder.Append(value.PadLeft(5));
                }

                if (row < Size - 1)
                {
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        private void ValidatePosition(CellPosition position)
        {
            ValidatePosition(position.Row, position.Col);
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