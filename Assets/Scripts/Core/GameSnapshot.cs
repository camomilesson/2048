using System;

namespace TwentyFortyEight.Core
{
    public sealed class GameSnapshot
    {
        public BoardSnapshot Board { get; }
        public PowerupChargeSnapshot PowerupCharges { get; }
        public int Score { get; }
        public bool HasReachedTarget { get; }
        public GameStatus Status { get; }

        public GameSnapshot(
            BoardSnapshot board,
            PowerupChargeSnapshot powerupCharges,
            int score,
            bool hasReachedTarget,
            GameStatus status
        )
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(score),
                    "Score cannot be negative."
                );
            }

            Board = board ?? throw new ArgumentNullException(nameof(board));
            PowerupCharges = powerupCharges ??
                throw new ArgumentNullException(nameof(powerupCharges));

            Score = score;
            HasReachedTarget = hasReachedTarget;
            Status = status;
        }
    }
}