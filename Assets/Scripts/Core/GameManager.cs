using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class GameManager
    {
        public const int DefaultTargetTileValue = 2048;

        private readonly MoveResolver moveResolver;
        private readonly TileSpawner tileSpawner;
        private readonly PowerupRewardSystem powerupRewardSystem;

        private GameSnapshot previousSnapshot;

        public BoardModel Board { get; }
        public PowerupCharges PowerupCharges { get; }

        public int Score { get; private set; }
        public int TargetTileValue { get; }
        public bool HasReachedTarget { get; private set; }
        public GameStatus Status { get; private set; }

        public bool CanUndo
        {
            get
            {
                return previousSnapshot != null;
            }
        }

        public GameManager(
            BoardModel board = null,
            MoveResolver moveResolver = null,
            TileSpawner tileSpawner = null,
            PowerupCharges powerupCharges = null,
            PowerupRewardSystem powerupRewardSystem = null,
            int targetTileValue = DefaultTargetTileValue
        )
        {
            if (targetTileValue <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetTileValue),
                    "Target tile value must be greater than zero."
                );
            }

            Board = board ?? new BoardModel();
            this.moveResolver = moveResolver ?? new MoveResolver();
            this.tileSpawner = tileSpawner ?? new TileSpawner();
            PowerupCharges = powerupCharges ?? new PowerupCharges();
            this.powerupRewardSystem = powerupRewardSystem ?? new PowerupRewardSystem();

            TargetTileValue = targetTileValue;

            StartNewGame();
        }

        public void StartNewGame()
        {
            Board.Clear();
            PowerupCharges.Reset();

            Score = 0;
            HasReachedTarget = false;
            Status = GameStatus.Playing;
            previousSnapshot = null;

            tileSpawner.SpawnInitialTiles(Board);
        }

        public GameActionResult HandleMove(Direction direction)
        {
            if (Status == GameStatus.GameOver)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot move: game is over."
                );
            }

            if (Status == GameStatus.OutOfMoves)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot move: no regular moves remain. Use a powerup or start a new game."
                );
            }

            if (Status == GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot move: game is waiting after win. Call ContinueAfterWin first."
                );
            }

            GameSnapshot snapshotBeforeMove = CreateGameSnapshot();

            MoveResult moveResult = moveResolver.ResolveMove(Board, direction);

            if (!moveResult.Changed)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Move did not change the board."
                );
            }

            previousSnapshot = snapshotBeforeMove;

            Score += moveResult.ScoreGained;

            IReadOnlyList<PowerupType> earnedPowerups =
                powerupRewardSystem.GrantRewardsForMergeValues(
                    moveResult.CreatedMergeValues,
                    PowerupCharges
                );

            TileSpawnResult spawnResult = tileSpawner.SpawnRandomTile(Board);

            bool reachedTargetThisAction = UpdateReachedTargetState();
            bool gameOverThisAction = UpdateGameOverState();

            return new GameActionResult(
                changed: true,
                scoreGained: moveResult.ScoreGained,
                mergeCount: moveResult.MergeCount,
                spawnResult: spawnResult,
                reachedTargetThisAction: reachedTargetThisAction,
                gameOverThisAction: gameOverThisAction,
                status: Status,
                message: "Move handled.",
                earnedPowerups: earnedPowerups
            );
        }

        public GameActionResult ContinueAfterWin()
        {
            if (Status != GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot continue: game is not in won state."
                );
            }

            Status = GameStatus.Playing;

            bool gameOverThisAction = UpdateGameOverState();

            string message;

            if (Status == GameStatus.Playing)
            {
                message = "Continuing after win.";
            }
            else if (Status == GameStatus.OutOfMoves)
            {
                message = "Continuing after win, but no regular moves remain.";
            }
            else
            {
                message = "No moves or rescue powerups remain after continuing.";
            }

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: gameOverThisAction,
                status: Status,
                message: message
            );
        }

        public bool CanUseUndoPowerup()
        {
            return
                previousSnapshot != null &&
                previousSnapshot.PowerupCharges.UndoCharges > 0;
        }

        public bool CanUseKillPowerup()
        {
            return
                PowerupCharges.CanUse(PowerupType.Kill) &&
                Board.GetOccupiedPositions().Count > 0;
        }

        public bool CanUseNukePowerup()
        {
            return
                PowerupCharges.CanUse(PowerupType.Nuke) &&
                Board.GetOccupiedPositions().Count > 0;
        }

        public GameActionResult UseUndoPowerup()
        {
            if (previousSnapshot == null)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot undo: no previous snapshot."
                );
            }

            if (previousSnapshot.PowerupCharges.UndoCharges <= 0)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot undo: no Undo charge was available before this action."
                );
            }

            RestoreGameSnapshot(previousSnapshot);

            bool spent = PowerupCharges.TrySpend(PowerupType.Undo);

            if (!spent)
            {
                throw new InvalidOperationException(
                    "Failed to spend Undo charge after restoring a snapshot that had one."
                );
            }

            previousSnapshot = null;

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: false,
                status: Status,
                message: "Undo powerup used."
            );
        }

        public GameActionResult UseKillPowerup(CellPosition position)
        {
            if (Status == GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot use kill: game is waiting after win. Call ContinueAfterWin first."
                );
            }

            if (!Board.HasTile(position))
            {
                return GameActionResult.NoChange(
                    Status,
                    $"Cannot kill: no tile at {position}."
                );
            }

            if (!PowerupCharges.CanUse(PowerupType.Kill))
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot kill: no Kill charges."
                );
            }

            // IMPORTANT: snapshot BEFORE spending Kill.
            previousSnapshot = CreateGameSnapshot();

            bool spent = PowerupCharges.TrySpend(PowerupType.Kill);

            if (!spent)
            {
                throw new InvalidOperationException(
                    "Failed to spend Kill charge after confirming one was available."
                );
            }

            Board.ClearCell(position);

            bool gameOverThisAction = UpdateGameOverState();

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: gameOverThisAction,
                status: Status,
                message: $"Killed tile at {position}."
            );
        }

        public GameActionResult UseNukePowerup()
        {
            if (Status == GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot use nuke: game is waiting after win. Call ContinueAfterWin first."
                );
            }

            if (Board.GetOccupiedPositions().Count == 0)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot nuke: board is empty."
                );
            }

            if (!PowerupCharges.CanUse(PowerupType.Nuke))
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot nuke: no Nuke charges."
                );
            }

            previousSnapshot = CreateGameSnapshot();

            bool spent = PowerupCharges.TrySpend(PowerupType.Nuke);

            if (!spent)
            {
                throw new InvalidOperationException(
                    "Failed to spend Nuke charge after confirming one was available."
                );
            }

            for (int row = 0; row < Board.Size; row++)
            {
                for (int col = 0; col < Board.Size; col++)
                {
                    TileData tile = Board.GetTile(row, col);

                    if (tile == null)
                    {
                        continue;
                    }

                    if (tile.Value == 2)
                    {
                        Board.ClearCell(row, col);
                    }
                    else
                    {
                        tile.Value /= 2;
                    }
                }
            }

            bool gameOverThisAction = UpdateGameOverState();

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: gameOverThisAction,
                status: Status,
                message: "Nuke powerup used."
            );
        }

        private bool HasUsableRescuePowerup()
        {
            return
                CanUseUndoPowerup() ||
                CanUseKillPowerup() ||
                CanUseNukePowerup();
        }

        public GameSnapshot CreateGameSnapshot()
        {
            return new GameSnapshot(
                Board.CreateSnapshot(),
                PowerupCharges.CreateSnapshot(),
                Score,
                HasReachedTarget,
                Status
            );
        }

        private void RestoreGameSnapshot(GameSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            Board.RestoreSnapshot(snapshot.Board);
            PowerupCharges.RestoreSnapshot(snapshot.PowerupCharges);

            Score = snapshot.Score;
            HasReachedTarget = snapshot.HasReachedTarget;
            Status = snapshot.Status;
        }

        private bool UpdateReachedTargetState()
        {
            if (HasReachedTarget)
            {
                return false;
            }

            int highestTile = Board.GetHighestTileValue();

            if (highestTile < TargetTileValue)
            {
                return false;
            }

            HasReachedTarget = true;
            Status = GameStatus.Won;

            return true;
        }

        private bool UpdateGameOverState()
        {
            if (Status == GameStatus.Won)
            {
                return false;
            }

            if (Board.HasAvailableMoves())
            {
                if (Status == GameStatus.GameOver || Status == GameStatus.OutOfMoves)
                {
                    Status = GameStatus.Playing;
                }

                return false;
            }

            if (HasUsableRescuePowerup())
            {
                Status = GameStatus.OutOfMoves;
                return false;
            }

            bool wasAlreadyGameOver = Status == GameStatus.GameOver;

            Status = GameStatus.GameOver;

            return !wasAlreadyGameOver;
        }
    }
}