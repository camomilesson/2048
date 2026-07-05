using System;
using TwentyFortyEight.Core;
using UnityEngine;

namespace TwentyFortyEight.Persistence
{
    public sealed class GameStateStore
    {
        private const string GameStateKey = "TwentyFortyEight.CurrentGame";

        public void Save(GameManager game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            GameSaveData data = GameSaveData.FromGame(game);
            string json = JsonUtility.ToJson(data);

            PlayerPrefs.SetString(GameStateKey, json);
            PlayerPrefs.Save();
        }

        public bool TryLoad(out GameSnapshot snapshot)
        {
            snapshot = null;

            string json = PlayerPrefs.GetString(GameStateKey, string.Empty);

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

                if (data == null || !data.IsValid())
                {
                    return false;
                }

                snapshot = data.ToSnapshot();
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Failed to load saved game. Starting fresh. {exception.Message}"
                );

                return false;
            }
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(GameStateKey);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class GameSaveData
        {
            public int BoardSize;
            public int Score;
            public bool HasReachedTarget;
            public GameStatus Status;

            public int UndoCharges;
            public int KillCharges;
            public int NukeCharges;

            public int[] CellValues;

            public static GameSaveData FromGame(GameManager game)
            {
                int boardSize = game.Board.Size;
                int[] cellValues = new int[boardSize * boardSize];

                for (int row = 0; row < boardSize; row++)
                {
                    for (int col = 0; col < boardSize; col++)
                    {
                        TileData tile = game.Board.GetTile(row, col);
                        int index = row * boardSize + col;

                        cellValues[index] = tile == null ? 0 : tile.Value;
                    }
                }

                return new GameSaveData
                {
                    BoardSize = boardSize,
                    Score = game.Score,
                    HasReachedTarget = game.HasReachedTarget,
                    Status = game.Status,

                    UndoCharges = game.PowerupCharges.UndoCharges,
                    KillCharges = game.PowerupCharges.KillCharges,
                    NukeCharges = game.PowerupCharges.NukeCharges,

                    CellValues = cellValues
                };
            }

            public bool IsValid()
            {
                if (BoardSize <= 0)
                {
                    return false;
                }

                if (Score < 0)
                {
                    return false;
                }

                if (UndoCharges < 0 || KillCharges < 0 || NukeCharges < 0)
                {
                    return false;
                }

                if (CellValues == null)
                {
                    return false;
                }

                return CellValues.Length == BoardSize * BoardSize;
            }

            public GameSnapshot ToSnapshot()
            {
                BoardModel board = new BoardModel(BoardSize);

                for (int row = 0; row < BoardSize; row++)
                {
                    for (int col = 0; col < BoardSize; col++)
                    {
                        int index = row * BoardSize + col;
                        int value = CellValues[index];

                        if (value <= 0)
                        {
                            continue;
                        }

                        board.SpawnTile(
                            new CellPosition(row, col),
                            value
                        );
                    }
                }

                PowerupChargeSnapshot powerupSnapshot =
                    new PowerupChargeSnapshot(
                        UndoCharges,
                        KillCharges,
                        NukeCharges
                    );

                return new GameSnapshot(
                    board.CreateSnapshot(),
                    powerupSnapshot,
                    Score,
                    HasReachedTarget,
                    Status
                );
            }
        }
    }
}