using System;
using TwentyFortyEight.Core;

namespace TwentyFortyEight.Stats
{
    public sealed class StatsManager
    {
        private bool hasRecordedWinForCurrentGame;
        private bool hasRecordedLossForCurrentGame;

        public StatsData Data { get; }

        public StatsManager(StatsData data = null)
        {
            Data = data ?? new StatsData();
        }

        public void RecordGameStarted()
        {
            Data.GamesStarted++;

            hasRecordedWinForCurrentGame = false;
            hasRecordedLossForCurrentGame = false;
        }

        public void RecordMove(
            GameActionResult result,
            BoardModel board,
            int currentScore
        )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!result.Changed)
            {
                return;
            }

            Data.TotalMoves++;
            Data.TotalMerges += result.MergeCount;

            UpdateBestScore(currentScore);
            UpdateHighestTile(board);

            RecordGameStateResult(result);
        }

        public void RecordPowerupUse(
            PowerupType powerupType,
            GameActionResult result,
            BoardModel board,
            int currentScore
        )
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (!result.Changed)
            {
                return;
            }

            switch (powerupType)
            {
                case PowerupType.Undo:
                    Data.UndoUses++;
                    break;

                case PowerupType.Kill:
                    Data.KillUses++;
                    break;

                case PowerupType.Nuke:
                    Data.NukeUses++;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(powerupType),
                        powerupType,
                        "Unknown powerup type."
                    );
            }

            UpdateBestScore(currentScore);
            UpdateHighestTile(board);

            RecordGameStateResult(result);
        }

        public void SyncBestScore(int bestScore)
        {
            if (bestScore < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bestScore),
                    "Best score cannot be negative."
                );
            }

            UpdateBestScore(bestScore);
        }

        private void UpdateBestScore(int currentScore)
        {
            if (currentScore > Data.BestScore)
            {
                Data.BestScore = currentScore;
            }
        }

        private void UpdateHighestTile(BoardModel board)
        {
            int highestTile = board.GetHighestTileValue();

            if (highestTile > Data.HighestTile)
            {
                Data.HighestTile = highestTile;
            }
        }

        private void RecordGameStateResult(GameActionResult result)
        {
            if (
                result.ReachedTargetThisAction &&
                !hasRecordedWinForCurrentGame
            )
            {
                Data.GamesWon++;
                hasRecordedWinForCurrentGame = true;
            }

            if (
                result.GameOverThisAction &&
                !hasRecordedLossForCurrentGame &&
                !hasRecordedWinForCurrentGame
            )
            {
                Data.GamesLost++;
                hasRecordedLossForCurrentGame = true;
            }
        }
    }
}