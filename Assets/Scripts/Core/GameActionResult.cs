using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyFortyEight.Core
{
    public sealed class GameActionResult
    {
        private readonly List<PowerupType> earnedPowerups;

        public bool Changed { get; }
        public int ScoreGained { get; }
        public int MergeCount { get; }
        public TileSpawnResult SpawnResult { get; }
        public bool ReachedTargetThisAction { get; }
        public bool GameOverThisAction { get; }
        public GameStatus Status { get; }
        public string Message { get; }

        public IReadOnlyList<PowerupType> EarnedPowerups
        {
            get
            {
                return earnedPowerups;
            }
        }

        public GameActionResult(
            bool changed,
            int scoreGained,
            int mergeCount,
            TileSpawnResult spawnResult,
            bool reachedTargetThisAction,
            bool gameOverThisAction,
            GameStatus status,
            string message,
            IReadOnlyList<PowerupType> earnedPowerups = null
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

            Changed = changed;
            ScoreGained = scoreGained;
            MergeCount = mergeCount;
            SpawnResult = spawnResult ?? TileSpawnResult.None();
            ReachedTargetThisAction = reachedTargetThisAction;
            GameOverThisAction = gameOverThisAction;
            Status = status;
            Message = message ?? string.Empty;

            this.earnedPowerups = earnedPowerups == null
                ? new List<PowerupType>()
                : new List<PowerupType>(earnedPowerups);
        }

        public static GameActionResult NoChange(
            GameStatus status,
            string message
        )
        {
            return new GameActionResult(
                changed: false,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: false,
                status: status,
                message: message
            );
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"Changed={Changed}, ");
            builder.Append($"ScoreGained={ScoreGained}, ");
            builder.Append($"MergeCount={MergeCount}, ");
            builder.Append($"ReachedTarget={ReachedTargetThisAction}, ");
            builder.Append($"GameOver={GameOverThisAction}, ");
            builder.Append($"Status={Status}, ");
            builder.Append($"Message=\"{Message}\"");

            if (earnedPowerups.Count > 0)
            {
                builder.Append(", EarnedPowerups=");

                for (int i = 0; i < earnedPowerups.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(earnedPowerups[i]);
                }
            }

            return builder.ToString();
        }
    }
}