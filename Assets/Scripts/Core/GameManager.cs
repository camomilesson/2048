using System;

namespace TwentyFortyEight.Core
{
    public sealed class GameManager
    {
        public const int DefaultTargetTileValue = 2048;

        private readonly MoveResolver moveResolver;
        private readonly TileSpawner tileSpawner;

        private GameSnapshot previousSnapshot;

        public BoardModel Board { get; }
        public int Score { get; private set; }
        public int TargetTileValue { get; }
        public bool HasReachedTarget { get; private set; }
        public GameStatus Status { get; private set; }

        public bool CanUndo => previousSnapshot != null;

        public GameManager(
            BoardModel board = null,
            MoveResolver moveResolver = null,
            TileSpawner tileSpawner = null,
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
            TargetTileValue = targetTileValue;

            StartNewGame();
        }

        public void StartNewGame()
        {
            Board.Clear();

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
                message: "Move handled."
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

            if (!Board.HasAvailableMoves())
            {
                Status = GameStatus.GameOver;

                return new GameActionResult(
                    changed: true,
                    scoreGained: 0,
                    mergeCount: 0,
                    spawnResult: TileSpawnResult.None(),
                    reachedTargetThisAction: false,
                    gameOverThisAction: true,
                    status: Status,
                    message: "No moves remain after continuing."
                );
            }

            Status = GameStatus.Playing;

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: false,
                status: Status,
                message: "Continuing after win."
            );
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

            RestoreGameSnapshot(previousSnapshot);
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

        public GameActionResult UsePopPowerup(CellPosition position)
        {
            if (Status == GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot use pop: game is waiting after win. Call ContinueAfterWin first."
                );
            }

            if (!Board.HasTile(position))
            {
                return GameActionResult.NoChange(
                    Status,
                    $"Cannot pop: no tile at {position}."
                );
            }

            previousSnapshot = CreateGameSnapshot();

            Board.ClearCell(position);

            bool gameOverThisAction = false;

            return new GameActionResult(
                changed: true,
                scoreGained: 0,
                mergeCount: 0,
                spawnResult: TileSpawnResult.None(),
                reachedTargetThisAction: false,
                gameOverThisAction: gameOverThisAction,
                status: Status,
                message: $"Popped tile at {position}."
            );
        }

        public GameActionResult UseHalveAllPowerup()
        {
            if (Status == GameStatus.Won)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot use halve: game is waiting after win. Call ContinueAfterWin first."
                );
            }

            if (Board.GetOccupiedPositions().Count == 0)
            {
                return GameActionResult.NoChange(
                    Status,
                    "Cannot halve: board is empty."
                );
            }

            previousSnapshot = CreateGameSnapshot();

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
                message: "Halve-all powerup used."
            );
        }

        public GameSnapshot CreateGameSnapshot()
        {
            return new GameSnapshot(
                Board.CreateSnapshot(),
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
                if (Status == GameStatus.GameOver)
                {
                    Status = GameStatus.Playing;
                }

                return false;
            }

            bool wasAlreadyGameOver = Status == GameStatus.GameOver;

            Status = GameStatus.GameOver;

            return !wasAlreadyGameOver;
        }
    }
}