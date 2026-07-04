using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class MoveResult
    {
        private readonly List<int> createdMergeValues;

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

        public MoveResult(
            Direction direction,
            bool changed,
            int scoreGained,
            int mergeCount,
            IReadOnlyList<int> createdMergeValues
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