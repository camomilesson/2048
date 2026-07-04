using System;

namespace TwentyFortyEight.Core
{
    public sealed class GameActionResult
    {
        public bool Changed { get; }
        public int ScoreGained { get; }
        public int MergeCount { get; }
        public TileSpawnResult SpawnResult { get; }
        public bool ReachedTargetThisAction { get; }
        public bool GameOverThisAction { get; }
        public GameStatus Status { get; }
        public string Message { get; }

        public GameActionResult(
            bool changed,
            int scoreGained,
            int mergeCount,
            TileSpawnResult spawnResult,
            bool reachedTargetThisAction,
            bool gameOverThisAction,
            GameStatus status,
            string message
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
            return
                $"Changed={Changed}, " +
                $"ScoreGained={ScoreGained}, " +
                $"MergeCount={MergeCount}, " +
                $"ReachedTarget={ReachedTargetThisAction}, " +
                $"GameOver={GameOverThisAction}, " +
                $"Status={Status}, " +
                $"Message=\"{Message}\"";
        }
    }
}