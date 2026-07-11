using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class MoveResult
    {
        private readonly List<int> createdMergeValues;

        private readonly List<TileMovement> tileMovements;
        private readonly List<CellPosition> mergePositions;

        public Direction Direction { get; }
        public bool Changed { get; }
        public int ScoreGained { get; }
        public int MergeCount { get; }

        public IReadOnlyList<int> CreatedMergeValues
        {
            get
            {
                return createdMergeValues;
            }
        }

        public IReadOnlyList<TileMovement> TileMovements
        {
            get
            {
                return tileMovements;
            }
        }

        public IReadOnlyList<CellPosition> MergePositions
        {
            get
            {
                return mergePositions;
            }
        }

        public MoveResult(
            Direction direction,
            bool changed,
            int scoreGained,
            int mergeCount,
            IReadOnlyList<int> createdMergeValues,
            IReadOnlyList<TileMovement> tileMovements,
            IReadOnlyList<CellPosition> mergePositions
        )
        {
            if (scoreGained < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(scoreGained),
                    "Score gained cannot be negative."
                );
            }

            if (mergeCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(mergeCount),
                    "Merge count cannot be negative."
                );
            }

            Direction = direction;
            Changed = changed;
            ScoreGained = scoreGained;
            MergeCount = mergeCount;

            this.createdMergeValues = createdMergeValues == null
                ? new List<int>()
                : new List<int>(createdMergeValues);
            
            this.tileMovements = tileMovements == null
                ? new List<TileMovement>()
                : new List<TileMovement>(tileMovements);

            this.mergePositions = mergePositions == null
                ? new List<CellPosition>()
                : new List<CellPosition>(mergePositions);
        }

        public override string ToString()
        {
            return
                $"Move {Direction}: " +
                $"Changed={Changed}, " +
                $"ScoreGained={ScoreGained}, " +
                $"MergeCount={MergeCount}";
        }
    }
}