using System;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class GameController : MonoBehaviour
    {
        private enum SelectionMode
        {
            None,
            Kill
        }

        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private ScoreView scoreView;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button undoButton;
        [SerializeField] private Button killButton;
        [SerializeField] private Button cullButton;

        [Header("Swipe Input")]
        [SerializeField] private float minimumSwipeDistance = 80f;
        [SerializeField] private float swipeDirectionThreshold = 0.5f;

        private GameManager game;
        private int bestScore;
        private SelectionMode selectionMode;
        private Vector2 pointerDownPosition;
        private bool isPointerDown;

        private void Awake()
        {
            ValidateReferences();

            game = new GameManager();
            bestScore = 0;
            selectionMode = SelectionMode.None;
        }

        private void OnEnable()
        {
            newGameButton.onClick.AddListener(StartNewGame);

            if (undoButton != null)
            {
                undoButton.onClick.AddListener(UseUndo);
            }

            if (killButton != null)
            {
                killButton.onClick.AddListener(ToggleKillSelection);
            }

            if (cullButton != null)
            {
                cullButton.onClick.AddListener(UseCull);
            }

            boardView.TileClicked += HandleTileClicked;
        }

        private void Start()
        {
            RefreshAll();
        }

        private void Update()
        {
            if (selectionMode != SelectionMode.None)
            {
                return;
            }

            HandleKeyboardInput();
            HandlePointerInput();
        }

        private void OnDisable()
        {
            newGameButton.onClick.RemoveListener(StartNewGame);

            if (undoButton != null)
            {
                undoButton.onClick.RemoveListener(UseUndo);
            }

            if (killButton != null)
            {
                killButton.onClick.RemoveListener(ToggleKillSelection);
            }

            if (cullButton != null)
            {
                cullButton.onClick.RemoveListener(UseCull);
            }

            if (boardView != null)
            {
                boardView.TileClicked -= HandleTileClicked;
            }
        }

        public void StartNewGame()
        {
            selectionMode = SelectionMode.None;

            game.StartNewGame();
            RefreshAll();
        }

        public void UseUndo()
        {
            if (selectionMode != SelectionMode.None)
            {
                Debug.Log("Cannot undo while selecting a tile.");
                return;
            }

            GameActionResult result = game.UseUndoPowerup();

            Debug.Log(result.ToString());

            RefreshAll();
        }

        public void UseCull()
        {
            if (selectionMode != SelectionMode.None)
            {
                Debug.Log("Cannot cull while selecting a tile.");
                return;
            }

            GameActionResult result = game.UseHalveAllPowerup();

            Debug.Log(result.ToString());

            RefreshAll();
        }

        private void HandleMove(Direction direction)
        {
            GameActionResult result = game.HandleMove(direction);

            Debug.Log(result.ToString());

            RefreshAll();
        }

        private void ToggleKillSelection()
        {
            if (selectionMode == SelectionMode.Kill)
            {
                selectionMode = SelectionMode.None;

                Debug.Log("Kill selection cancelled.");

                RefreshButtons();
                return;
            }

            bool hasTiles = game.Board.GetOccupiedPositions().Count > 0;

            if (!hasTiles)
            {
                Debug.Log("Cannot kill: board has no occupied tiles.");
                return;
            }

            selectionMode = SelectionMode.Kill;

            Debug.Log("Kill mode active. Click a tile to remove it.");

            RefreshButtons();
        }

        private void HandleTileClicked(CellPosition position)
        {
            if (selectionMode != SelectionMode.Kill)
            {
                return;
            }

            GameActionResult result = game.UsePopPowerup(position);

            Debug.Log(result.ToString());

            selectionMode = SelectionMode.None;

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
            bool hasTiles = game.Board.GetOccupiedPositions().Count > 0;
            bool isSelecting = selectionMode != SelectionMode.None;

            if (undoButton != null)
            {
                undoButton.interactable = !isSelecting && game.CanUndo;
            }

            if (killButton != null)
            {
                killButton.interactable = hasTiles;
            }

            if (cullButton != null)
            {
                cullButton.interactable = !isSelecting && hasTiles;
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

        private void HandleKeyboardInput()
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

        private void HandlePointerInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    BeginPointer(touch.position);
                }
                else if (
                    touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled
                )
                {
                    EndPointer(touch.position);
                }

                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                BeginPointer(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndPointer(Input.mousePosition);
            }
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            pointerDownPosition = screenPosition;
            isPointerDown = true;
        }

        private void EndPointer(Vector2 screenPosition)
        {
            if (!isPointerDown)
            {
                return;
            }

            isPointerDown = false;

            Vector2 delta = screenPosition - pointerDownPosition;

            if (delta.magnitude < minimumSwipeDistance)
            {
                return;
            }

            Direction? direction = GetSwipeDirection(delta);

            if (direction.HasValue)
            {
                HandleMove(direction.Value);
            }
        }

        private Direction? GetSwipeDirection(Vector2 delta)
        {
            Vector2 normalized = delta.normalized;

            if (Mathf.Abs(normalized.x) > Mathf.Abs(normalized.y))
            {
                if (Mathf.Abs(normalized.x) < swipeDirectionThreshold)
                {
                    return null;
                }

                return normalized.x > 0
                    ? Direction.Right
                    : Direction.Left;
            }

            if (Mathf.Abs(normalized.y) < swipeDirectionThreshold)
            {
                return null;
            }

            return normalized.y > 0
                ? Direction.Up
                : Direction.Down;
        }
    }
}