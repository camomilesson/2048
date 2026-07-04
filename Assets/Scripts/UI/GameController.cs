using System;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class GameController : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private ScoreView scoreView;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button killButton;
        [SerializeField] private Button cullButton;

        private GameManager game;
        private int bestScore;

        private void Awake()
        {
            ValidateReferences();

            game = new GameManager();
            bestScore = 0;
        }

        private void OnEnable()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(StartNewGame);
            }

            if (undoButton != null)
            {
                undoButton.onClick.AddListener(UseUndo);
            }

            if (killButton != null)
            {
                killButton.onClick.AddListener(UseKillPlaceholder);
            }

            if (cullButton != null)
            {
                cullButton.onClick.AddListener(UseCull);
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void OnDisable()
        {
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(StartNewGame);
            }

            if (undoButton != null)
            {
                undoButton.onClick.RemoveListener(UseUndo);
            }

            if (killButton != null)
            {
                killButton.onClick.RemoveListener(UseKillPlaceholder);
            }

            if (cullButton != null)
            {
                cullButton.onClick.RemoveListener(UseCull);
            }
        }

        public void StartNewGame()
        {
            game.StartNewGame();
            RefreshAll();
        }

        public void UseUndo()
        {
            GameActionResult result = game.UseUndoPowerup();

            Debug.Log(result.ToString());

            RefreshAll();
        }

        public void UseCull()
        {
            GameActionResult result = game.UseHalveAllPowerup();

            Debug.Log(result.ToString());

            RefreshAll();
        }

        private void UseKillPlaceholder()
        {
            // Temporary placeholder until we implement tile selection.
            // For now, remove the first occupied tile so we can verify
            // that button → GameManager → BoardView flow works.
            var occupiedPositions = game.Board.GetOccupiedPositions();

            if (occupiedPositions.Count == 0)
            {
                Debug.Log("Kill placeholder: no occupied tiles to remove.");
                return;
            }

            CellPosition position = occupiedPositions[0];
            GameActionResult result = game.UsePopPowerup(position);

            Debug.Log(result.ToString());

            RefreshAll();
        }

        private void RefreshAll()
        {
            UpdateBestScore();

            boardView.Refresh(game.Board);
            scoreView.SetScores(game.Score, bestScore);

            RefreshButtons();
        }

        private void UpdateBestScore()
        {
            if (game.Score > bestScore)
            {
                bestScore = game.Score;
            }
        }

        private void RefreshButtons()
        {
            if (undoButton != null)
            {
                undoButton.interactable = game.CanUndo;
            }

            bool hasTiles = game.Board.GetOccupiedPositions().Count > 0;

            if (killButton != null)
            {
                killButton.interactable = hasTiles;
            }

            if (cullButton != null)
            {
                cullButton.interactable = hasTiles;
            }
        }

        private void ValidateReferences()
        {
            if (boardView == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a BoardView reference."
                );
            }

            if (scoreView == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a ScoreView reference."
                );
            }

            if (newGameButton == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a New Game button reference."
                );
            }
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
        }

        private void HandleMove(Direction direction)
        {
            GameActionResult result = game.HandleMove(direction);

            Debug.Log(result.ToString());

            RefreshAll();
        }
    }
}