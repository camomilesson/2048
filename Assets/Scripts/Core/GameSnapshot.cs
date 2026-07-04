using System;

namespace TwentyFortyEight.Core
{
    public sealed class GameSnapshot
    {
        public BoardSnapshot Board { get; }
        public int Score { get; }
        public bool HasReachedTarget { get; }
        public GameStatus Status { get; }

        public GameSnapshot(
            BoardSnapshot board,
            int score,
            bool hasReachedTarget,
            GameStatus status
        )
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));

            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(score),
                    "Score cannot be negative."
                );
            }

            Score = score;
            HasReachedTarget = hasReachedTarget;
            Status = status;
        }
    }
}