using System;
using System.Collections;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;
using TwentyFortyEight.Stats;
using TwentyFortyEight.Persistence;

namespace TwentyFortyEight.UI
{
    public sealed class GameController : MonoBehaviour
    {
        private enum SelectionMode
        {
            None,
            Kill
        }

        [Header("Screens")]
        [SerializeField] private GameObject mainScreenRoot;
        [SerializeField] private StatsScreenView statsScreenView;

        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private ScoreView scoreView;
        [SerializeField] private GameStateOverlayView gameStateOverlayView;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private PowerupButtonView undoButtonView;
        [SerializeField] private PowerupButtonView killButtonView;
        [SerializeField] private PowerupButtonView nukeButtonView;
        [SerializeField] private Button statsButton;

        [Header("Swipe Input")]
        [SerializeField] private RectTransform swipeArea;
        [SerializeField] private float swipeAreaPadding = 20f;
        [SerializeField] private float minimumSwipeDistance = 80f;
        [SerializeField] private float swipeDirectionThreshold = 0.5f;

        private GameManager game;
        private StatsStore statsStore;
        private StatsManager statsManager;
        private GameStateStore gameStateStore;
        private SelectionMode selectionMode;
        private Vector2 pointerDownPosition;
        private bool isPointerDown;
        private bool suppressPointerInputUntilRelease;
        private bool isStatsScreenOpen;
        private bool isAnimating;

        private void Awake()
        {
            ValidateReferences();

            statsStore = new StatsStore();

            StatsData statsData = statsStore.Load();
            statsManager = new StatsManager(statsData);

            game = new GameManager();

            gameStateStore = new GameStateStore();

            bool loadedSavedGame = gameStateStore.TryLoad(out GameSnapshot savedSnapshot);

            if (loadedSavedGame)
            {
                game.RestoreFromSnapshot(savedSnapshot);
            }
            else
            {
                statsManager.RecordGameStarted();
                SaveStats();
                SaveCurrentGame();
            }

            selectionMode = SelectionMode.None;
        }

        private void OnEnable()
        {
            newGameButton.onClick.AddListener(StartNewGame);

            if (undoButtonView != null && undoButtonView.Button != null)
            {
                undoButtonView.Button.onClick.AddListener(UseUndo);
            }

            if (killButtonView != null && killButtonView.Button != null)
            {
                killButtonView.Button.onClick.AddListener(ToggleKillSelection);
            }

            if (nukeButtonView != null && nukeButtonView.Button != null)
            {
                nukeButtonView.Button.onClick.AddListener(UseNuke);
            }

            boardView.TileClicked += HandleTileClicked;

            if (gameStateOverlayView != null)
            {
                gameStateOverlayView.ContinueClicked += ContinueAfterWin;
                gameStateOverlayView.NewGameClicked += StartNewGame;
            }

            if (statsButton != null)
            {
                statsButton.onClick.AddListener(ShowStatsScreen);
            }

            if (statsScreenView != null)
            {
                statsScreenView.BackClicked += HideStatsScreen;
            }
        }

        private void Start()
        {
            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(true);
            }

            if (statsScreenView != null)
            {
                statsScreenView.Hide();
            }

            RefreshAll();
        }

        private void Update()
        {
            if (isStatsScreenOpen)
            {
                ResetPointerState();
                return;
            }

            if (isAnimating)
            {
                ResetPointerState();
                return;
            }

            if (game.Status != GameStatus.Playing)
            {
                return;
            }

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

            if (undoButtonView != null && undoButtonView.Button != null)
            {
                undoButtonView.Button.onClick.RemoveListener(UseUndo);
            }

            if (killButtonView != null && killButtonView.Button != null)
            {
                killButtonView.Button.onClick.RemoveListener(ToggleKillSelection);
            }

            if (nukeButtonView != null && nukeButtonView.Button != null)
            {
                nukeButtonView.Button.onClick.RemoveListener(UseNuke);
            }

            if (boardView != null)
            {
                boardView.TileClicked -= HandleTileClicked;
            }

            if (gameStateOverlayView != null)
            {
                gameStateOverlayView.ContinueClicked -= ContinueAfterWin;
                gameStateOverlayView.NewGameClicked -= StartNewGame;
            }

            if (statsButton != null)
            {
                statsButton.onClick.RemoveListener(ShowStatsScreen);
            }

            if (statsScreenView != null)
            {
                statsScreenView.BackClicked -= HideStatsScreen;
            }
        }

        private void SetAnimationState(bool animating)
        {
            isAnimating = animating;

            if (!animating)
            {
                RefreshButtons();
            }
        }

        public void StartNewGame()
        {
            if (isAnimating)
            {
                return;
            }

            selectionMode = SelectionMode.None;

            game.StartNewGame();

            statsManager.RecordGameStarted();

            SaveAll();

            RefreshAll();
        }

        public void UseUndo()
        {
            if (isAnimating)
            {
                return;
            }

            if (selectionMode != SelectionMode.None)
            {
                Debug.Log("Cannot undo while selecting a tile.");
                return;
            }

            GameActionResult result = game.UseUndoPowerup();

            Debug.Log(result.ToString());

            statsManager.RecordPowerupUse(
                PowerupType.Undo,
                result,
                game.Board,
                game.Score
            );

            if (result.Changed)
            {
                SaveAll();
            }

            RefreshAll();
        }

        public void UseNuke()
        {
            if (isAnimating)
            {
                return;
            }

            if (selectionMode != SelectionMode.None)
            {
                Debug.Log("Cannot nuke while selecting a tile.");
                return;
            }

            GameActionResult result = game.UseNukePowerup();

            Debug.Log(result.ToString());

            statsManager.RecordPowerupUse(
                PowerupType.Nuke,
                result,
                game.Board,
                game.Score
            );

            if (result.Changed)
            {
                SaveAll();
            }

            RefreshAll();
        }

        private void HandleMove(Direction direction)
        {
            if (isAnimating)
            {
                return;
            }

            StartCoroutine(HandleMoveRoutine(direction));
        }

        private IEnumerator HandleMoveRoutine(Direction direction)
        {
            SetAnimationState(true);
            ResetPointerState();

            GameActionResult result = game.HandleMove(direction);

            Debug.Log(result.ToString());

            statsManager.RecordMove(
                result,
                game.Board,
                game.Score
            );

            if (!result.Changed)
            {
                SetAnimationState(false);
                yield break;
            }

            UpdateBestScore();
            SaveAll();

            yield return boardView.AnimateMove(
                game.Board,
                result
            );

            scoreView.SetScores(
                game.Score,
                statsManager.Data.BestScore
            );

            RefreshGameStateOverlay();

            SetAnimationState(false);
        }

        private void ToggleKillSelection()
        {
            if (isAnimating)
            {
                return;
            }

            if (selectionMode == SelectionMode.Kill)
            {
                selectionMode = SelectionMode.None;

                Debug.Log("Kill selection cancelled.");

                RefreshButtons();
                return;
            }

            if (!game.PowerupCharges.CanUse(PowerupType.Kill))
            {
                Debug.Log("Cannot kill: no Kill charges.");
                return;
            }

            bool hasTiles = game.Board.GetOccupiedPositions().Count > 0;

            if (!hasTiles)
            {
                Debug.Log("Cannot kill: board has no occupied tiles.");
                return;
            }

            selectionMode = SelectionMode.Kill;
            isPointerDown = false;
            suppressPointerInputUntilRelease = true;

            Debug.Log("Kill mode active. Click a tile to remove it.");

            RefreshButtons();
        }

        private void HandleTileClicked(CellPosition position)
        {
            if (isAnimating)
            {
                return;
            }

            suppressPointerInputUntilRelease = true;
            isPointerDown = false;

            if (selectionMode != SelectionMode.Kill)
            {
                return;
            }

            GameActionResult result = game.UseKillPowerup(position);

            Debug.Log(result.ToString());

            statsManager.RecordPowerupUse(
                PowerupType.Kill,
                result,
                game.Board,
                game.Score
            );

            if (result.Changed)
            {
                SaveAll();
            }

            selectionMode = SelectionMode.None;

            RefreshAll();
        }

        private void RefreshAll()
        {
            UpdateBestScore();

            boardView.Refresh(game.Board);
            scoreView.SetScores(game.Score, statsManager.Data.BestScore);

            RefreshButtons();
            RefreshGameStateOverlay();
        }

        private void RefreshGameStateOverlay()
        {
            if (gameStateOverlayView == null)
            {
                return;
            }

            if (game.Status == GameStatus.Won)
            {
                gameStateOverlayView.ShowWin();
            }
            else if (game.Status == GameStatus.GameOver)
            {
                gameStateOverlayView.ShowGameOver();
            }
            else
            {
                gameStateOverlayView.Hide();
            }
        }

        private void UpdateBestScore()
        {
            int previousBestScore = statsManager.Data.BestScore;

            statsManager.SyncBestScore(game.Score);

            if (statsManager.Data.BestScore != previousBestScore)
            {
                SaveStats();
            }
        }

        private void SaveStats()
        {
            statsStore.Save(statsManager.Data);
        }

        private void SaveCurrentGame()
        {
            gameStateStore.Save(game);
        }

        private void SaveAll()
        {
            SaveStats();
            SaveCurrentGame();
        }

        private void RefreshButtons()
        {
            bool isSelecting =
                selectionMode != SelectionMode.None;

            bool canUsePowerups =
                game.Status == GameStatus.Playing ||
                game.Status == GameStatus.OutOfMoves;

            if (undoButtonView != null)
            {
                undoButtonView.SetInteractable(
                    canUsePowerups &&
                    !isSelecting &&
                    game.CanUseUndoPowerup()
                );

                undoButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Undo
                    )
                );
            }

            if (killButtonView != null)
            {
                killButtonView.SetInteractable(
                    canUsePowerups &&
                    game.CanUseKillPowerup()
                );

                killButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Kill
                    )
                );
            }

            if (nukeButtonView != null)
            {
                nukeButtonView.SetInteractable(
                    canUsePowerups &&
                    !isSelecting &&
                    game.CanUseNukePowerup()
                );

                nukeButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Nuke
                    )
                );
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

            if (swipeArea == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a Swipe Area reference."
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
            if (suppressPointerInputUntilRelease)
            {
                bool hasActiveMousePress = Input.GetMouseButton(0);
                bool hasActiveTouchPress = Input.touchCount > 0;

                if (!hasActiveMousePress && !hasActiveTouchPress)
                {
                    suppressPointerInputUntilRelease = false;
                    ResetPointerState();
                }

                return;
            }
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (suppressPointerInputUntilRelease)
                {
                    if (
                        touch.phase == TouchPhase.Ended ||
                        touch.phase == TouchPhase.Canceled
                    )
                    {
                        suppressPointerInputUntilRelease = false;
                        isPointerDown = false;
                    }

                    return;
                }

                if (touch.phase == TouchPhase.Began)
                {
                    BeginPointer(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    EndPointer(touch.position);
                }
                else if (touch.phase == TouchPhase.Canceled)
                {
                    ResetPointerState();
                }

                return;
            }

            if (suppressPointerInputUntilRelease)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    suppressPointerInputUntilRelease = false;
                    isPointerDown = false;
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

        private bool IsInsideSwipeArea(Vector2 screenPosition)
        {
            if (swipeArea == null)
            {
                return false;
            }

            Canvas canvas = swipeArea.GetComponentInParent<Canvas>();

            Camera uiCamera = null;

            if (
                canvas != null &&
                canvas.renderMode != RenderMode.ScreenSpaceOverlay
            )
            {
                uiCamera = canvas.worldCamera;
            }

            bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                swipeArea,
                screenPosition,
                uiCamera,
                out Vector2 localPosition
            );

            if (!converted)
            {
                return false;
            }

            Rect allowedRect = swipeArea.rect;

            allowedRect.xMin -= swipeAreaPadding;
            allowedRect.xMax += swipeAreaPadding;
            allowedRect.yMin -= swipeAreaPadding;
            allowedRect.yMax += swipeAreaPadding;

            return allowedRect.Contains(localPosition);
        }

        private void BeginPointer(Vector2 screenPosition)
        {
            if (!IsInsideSwipeArea(screenPosition))
            {
                ResetPointerState();
                return;
            }

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
    
        private void ResetPointerState()
        {
            pointerDownPosition = Vector2.zero;
            isPointerDown = false;
        }

        private void SuppressPointerInputUntilReleased()
        {
            ResetPointerState();
            suppressPointerInputUntilRelease = true;
        }

        public void ContinueAfterWin()
        {
            if (isAnimating)
            {
                return;
            }

            GameActionResult result = game.ContinueAfterWin();

            Debug.Log(result.ToString());

            if (result.Changed)
            {
                SaveCurrentGame();
            }

            RefreshAll();
        }

        private void ShowStatsScreen()
        {
            if (isAnimating)
            {
                return;
            }

            SaveStats();

            isStatsScreenOpen = true;
            SuppressPointerInputUntilReleased();

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(false);
            }

            if (statsScreenView != null)
            {
                statsScreenView.Show(statsManager.Data);
            }
        }

        private void HideStatsScreen()
        {
            if (statsScreenView != null)
            {
                statsScreenView.Hide();
            }

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(true);
            }

            isStatsScreenOpen = false;
            SuppressPointerInputUntilReleased();

            RefreshAll();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                return;
            }

            SaveAll();
        }

        private void OnApplicationQuit()
        {
            SaveAll();
        }
    }
}