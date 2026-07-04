using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class MoveResolver
    {
        public MoveResult ResolveMove(BoardModel board, Direction direction)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            bool changed = false;
            int totalScoreGained = 0;
            int totalMergeCount = 0;
            List<int> createdMergeValues = new List<int>();

            for (int lineIndex = 0; lineIndex < board.Size; lineIndex++)
            {
                List<CellPosition> linePositions =
                    GetLinePositions(board.Size, direction, lineIndex);

                LineResult lineResult = ResolveLine(board, linePositions);

                if (lineResult.Changed)
                {
                    changed = true;
                }

                totalScoreGained += lineResult.ScoreGained;
                totalMergeCount += lineResult.MergeCount;
                createdMergeValues.AddRange(lineResult.CreatedMergeValues);
            }

            return new MoveResult(
                direction,
                changed,
                totalScoreGained,
                totalMergeCount,
                createdMergeValues
            );
        }

        private static LineResult ResolveLine(
            BoardModel board,
            List<CellPosition> positions
        )
        {
            List<LineEntry> inputEntries = CollectTilesInLine(board, positions);
            LineMergeResult mergeResult = MergeLine(inputEntries);

            bool changed = mergeResult.MergeCount > 0;

            for (int i = 0; i < positions.Count; i++)
            {
                board.ClearCell(positions[i]);
            }

            for (int i = 0; i < mergeResult.OutputSlots.Count; i++)
            {
                OutputSlot outputSlot = mergeResult.OutputSlots[i];
                CellPosition targetPosition = positions[i];

                board.SetTile(targetPosition, outputSlot.Tile);

                if (outputSlot.OriginalPosition != targetPosition)
                {
                    changed = true;
                }
            }

            return new LineResult(
                changed,
                mergeResult.ScoreGained,
                mergeResult.MergeCount,
                mergeResult.CreatedMergeValues
            );
        }

        private static List<LineEntry> CollectTilesInLine(
            BoardModel board,
            List<CellPosition> positions
        )
        {
            List<LineEntry> entries = new List<LineEntry>();

            for (int i = 0; i < positions.Count; i++)
            {
                CellPosition position = positions[i];
                TileData tile = board.GetTile(position);

                if (tile != null)
                {
                    entries.Add(new LineEntry(position, tile));
                }
            }

            return entries;
        }

        private static LineMergeResult MergeLine(List<LineEntry> inputEntries)
        {
            List<OutputSlot> outputSlots = new List<OutputSlot>();
            List<int> createdMergeValues = new List<int>();

            int scoreGained = 0;
            int mergeCount = 0;

            for (int i = 0; i < inputEntries.Count; i++)
            {
                LineEntry currentEntry = inputEntries[i];

                if (CanMergeWithPreviousOutputSlot(outputSlots, currentEntry.Tile))
                {
                    OutputSlot previousSlot = outputSlots[outputSlots.Count - 1];

                    previousSlot.Tile.Value *= 2;
                    previousSlot.HasMergedThisMove = true;

                    int createdValue = previousSlot.Tile.Value;

                    scoreGained += createdValue;
                    mergeCount++;
                    createdMergeValues.Add(createdValue);

                    continue;
                }

                outputSlots.Add(
                    new OutputSlot(currentEntry.Tile, currentEntry.Position)
                );
            }

            return new LineMergeResult(
                outputSlots,
                scoreGained,
                mergeCount,
                createdMergeValues
            );
        }

        private static bool CanMergeWithPreviousOutputSlot(
            List<OutputSlot> outputSlots,
            TileData currentTile
        )
        {
            if (outputSlots.Count == 0)
            {
                return false;
            }

            OutputSlot previousSlot = outputSlots[outputSlots.Count - 1];

            if (previousSlot.HasMergedThisMove)
            {
                return false;
            }

            return previousSlot.Tile.Value == currentTile.Value;
        }

        private static List<CellPosition> GetLinePositions(
            int boardSize,
            Direction direction,
            int lineIndex
        )
        {
            List<CellPosition> positions = new List<CellPosition>(boardSize);

            switch (direction)
            {
                case Direction.Left:
                    for (int col = 0; col < boardSize; col++)
                    {
                        positions.Add(new CellPosition(lineIndex, col));
                    }

                    break;

                case Direction.Right:
                    for (int col = boardSize - 1; col >= 0; col--)
                    {
                        positions.Add(new CellPosition(lineIndex, col));
                    }

                    break;

                case Direction.Up:
                    for (int row = 0; row < boardSize; row++)
                    {
                        positions.Add(new CellPosition(row, lineIndex));
                    }

                    break;

                case Direction.Down:
                    for (int row = boardSize - 1; row >= 0; row--)
                    {
                        positions.Add(new CellPosition(row, lineIndex));
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(direction),
                        direction,
                        "Unknown move direction."
                    );
            }

            return positions;
        }

        private readonly struct LineEntry
        {
            public CellPosition Position { get; }
            public TileData Tile { get; }

            public LineEntry(CellPosition position, TileData tile)
            {
                Position = position;
                Tile = tile ?? throw new ArgumentNullException(nameof(tile));
            }
        }

        private sealed class OutputSlot
        {
            public TileData Tile { get; }
            public CellPosition OriginalPosition { get; }
            public bool HasMergedThisMove { get; set; }

            public OutputSlot(TileData tile, CellPosition originalPosition)
            {
                Tile = tile ?? throw new ArgumentNullException(nameof(tile));
                OriginalPosition = originalPosition;
                HasMergedThisMove = false;
            }
        }

        private readonly struct LineMergeResult
        {
            public List<OutputSlot> OutputSlots { get; }
            public int ScoreGained { get; }
            public int MergeCount { get; }
            public List<int> CreatedMergeValues { get; }

            public LineMergeResult(
                List<OutputSlot> outputSlots,
                int scoreGained,
                int mergeCount,
                List<int> createdMergeValues
            )
            {
                OutputSlots = outputSlots ??
                    throw new ArgumentNullException(nameof(outputSlots));

                CreatedMergeValues = createdMergeValues ??
                    throw new ArgumentNullException(nameof(createdMergeValues));

                ScoreGained = scoreGained;
                MergeCount = mergeCount;
            }
        }

        private readonly struct LineResult
        {
            public bool Changed { get; }
            public int ScoreGained { get; }
            public int MergeCount { get; }
            public List<int> CreatedMergeValues { get; }

            public LineResult(
                bool changed,
                int scoreGained,
                int mergeCount,
                List<int> createdMergeValues
            )
            {
                CreatedMergeValues = createdMergeValues ??
                    throw new ArgumentNullException(nameof(createdMergeValues));

                Changed = changed;
                ScoreGained = scoreGained;
                MergeCount = mergeCount;
            }
        }
    }
}