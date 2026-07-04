using System;

namespace TwentyFortyEight.Core
{
    public sealed class MoveResult
    {
        public Direction Direction { get; }
        public bool Changed { get; }
        public int ScoreGained { get; }
        public int MergeCount { get; }

        public MoveResult(
            Direction direction,
            bool changed,
            int scoreGained,
            int mergeCount
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