using System.Collections.Generic;
using TwentyFortyEight.Core;
using UnityEngine;

namespace TwentyFortyEight.Debugging
{
    public sealed class DebugGameRunner : MonoBehaviour
    {
        private GameManager game;

        private void Start()
        {
            game = new GameManager();

            Debug.Log("Debug game started.");
            LogControls();
            LogGameState();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                HandleMove(Direction.Left);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                HandleMove(Direction.Right);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                HandleMove(Direction.Up);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                HandleMove(Direction.Down);
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                UseUndo();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                UseHalveAll();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                PopFirstOccupiedTile();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                StartNewGame();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ContinueAfterWin();
            }
        }

        private void HandleMove(Direction direction)
        {
            GameActionResult result = game.HandleMove(direction);

            Debug.Log(result.ToString());
            LogGameState();
        }

        private void UseUndo()
        {
            GameActionResult result = game.UseUndoPowerup();

            Debug.Log(result.ToString());
            LogGameState();
        }

        private void UseHalveAll()
        {
            GameActionResult result = game.UseHalveAllPowerup();

            Debug.Log(result.ToString());
            LogGameState();
        }

        private void PopFirstOccupiedTile()
        {
            List<CellPosition> occupiedPositions = game.Board.GetOccupiedPositions();

            if (occupiedPositions.Count == 0)
            {
                Debug.Log("Cannot pop: board has no occupied tiles.");
                return;
            }

            CellPosition position = occupiedPositions[0];
            GameActionResult result = game.UsePopPowerup(position);

            Debug.Log(result.ToString());
            LogGameState();
        }

        private void StartNewGame()
        {
            game.StartNewGame();

            Debug.Log("New game started.");
            LogGameState();
        }

        private void ContinueAfterWin()
        {
            GameActionResult result = game.ContinueAfterWin();

            Debug.Log(result.ToString());
            LogGameState();
        }

        private void LogGameState()
        {
            Debug.Log(
                $"Score: {game.Score}\n" +
                $"Status: {game.Status}\n" +
                $"CanUndo: {game.CanUndo}\n" +
                $"Highest Tile: {game.Board.GetHighestTileValue()}\n" +
                $"Board:\n{game.Board.ToDebugString()}"
            );
        }

        private static void LogControls()
        {
            Debug.Log(
                "Debug controls:\n" +
                "Arrow Keys / WASD = Move\n" +
                "U = Undo\n" +
                "H = Halve all\n" +
                "P = Pop first occupied tile\n" +
                "N = New game\n" +
                "C = Continue after win"
            );
        }
    }
}