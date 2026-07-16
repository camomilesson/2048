using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFortyEight.Audio;
using TwentyFortyEight.Core;
using TwentyFortyEight.Persistence;
using TwentyFortyEight.Settings;
using TwentyFortyEight.Stats;
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

        private enum PendingConfirmationAction
        {
            None,
            StartNewGame,
            ClearStats
        }

        [Header("Screens")]
        [SerializeField] private GameObject mainScreenRoot;
        [SerializeField] private StatsScreenView statsScreenView;
        [SerializeField] private SettingsScreenView settingsScreenView;

        [Header("Views")]
        [SerializeField] private BoardView boardView;
        [SerializeField] private ScoreView scoreView;
        [SerializeField] private GameStateOverlayView gameStateOverlayView;
        [SerializeField] private ConfirmationDialogView confirmationDialogView;
        [SerializeField] private BannerMessageView bannerMessageView;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private PowerupButtonView undoButtonView;
        [SerializeField] private PowerupButtonView killButtonView;
        [SerializeField] private PowerupButtonView nukeButtonView;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button settingsButton;

        [Header("Initial Powerups")]
        [SerializeField, Range(0, PowerupCharges.DefaultMaxUndoCharges)]
        private int initialUndoCharges;

        [SerializeField, Range(0, PowerupCharges.DefaultMaxKillCharges)]
        private int initialKillCharges;

        [SerializeField, Range(0, PowerupCharges.DefaultMaxNukeCharges)]
        private int initialNukeCharges;

        [Header("Swipe Input")]
        [SerializeField] private RectTransform swipeArea;
        [SerializeField] private float swipeAreaPadding = 20f;
        [SerializeField] private float minimumSwipeDistance = 80f;
        [SerializeField] private float swipeDirectionThreshold = 0.5f;

        [Header("Move Buffer")]
        [SerializeField, Range(0, 4)]
        private int maxBufferedMoves = 2;

        [Header("VFX")]
        [SerializeField] private UiVfxController uiVfx;

        [Header("Audio")]
        [SerializeField] private GameAudio gameAudio;

        private readonly Queue<Direction> moveQueue =
            new Queue<Direction>();

        private GameManager game;
        private GameStateStore gameStateStore;
        private StatsStore statsStore;
        private StatsManager statsManager;
        private GameSettingsStore settingsStore;
        private GameSettingsData settingsData;

        private SelectionMode selectionMode;
        private PendingConfirmationAction pendingConfirmationAction;

        private Vector2 pointerDownPosition;
        private bool isPointerDown;
        private bool suppressPointerInputUntilRelease;

        private bool isStatsScreenOpen;
        private bool isSettingsScreenOpen;
        private bool isConfirmationDialogOpen;

        private bool isAnimating;
        private bool isProcessingMoveQueue;

        private bool recordCurrentGameOnNextChangedAction;
        private bool showSwipeHintForCurrentGame;

        private bool IsGameplayInputBlocked
        {
            get
            {
                return
                    isStatsScreenOpen ||
                    isSettingsScreenOpen ||
                    isConfirmationDialogOpen;
            }
        }

        private bool IsMainScreenVisible
        {
            get
            {
                return
                    mainScreenRoot == null ||
                    mainScreenRoot.activeInHierarchy;
            }
        }

        #region Unity lifecycle

        private void Awake()
        {
            ValidateReferences();

            statsStore = new StatsStore();
            statsManager = new StatsManager(
                statsStore.Load()
            );

            settingsStore = new GameSettingsStore();
            settingsData = settingsStore.Load();

            PowerupCharges powerupCharges =
                new PowerupCharges(
                    initialUndoCharges: initialUndoCharges,
                    initialKillCharges: initialKillCharges,
                    initialNukeCharges: initialNukeCharges
                );

            game = new GameManager(
                powerupCharges: powerupCharges
            );

            gameStateStore = new GameStateStore();

            bool loadedSavedGame =
                gameStateStore.TryLoad(
                    out GameSnapshot savedSnapshot
                );

            if (loadedSavedGame)
            {
                game.RestoreFromSnapshot(
                    savedSnapshot
                );
            }
            else
            {
                statsManager.RecordGameStarted();
                SaveAll();
            }

            selectionMode = SelectionMode.None;
            pendingConfirmationAction =
                PendingConfirmationAction.None;

            showSwipeHintForCurrentGame =
                !loadedSavedGame;

            /*
             * Clear Stats intentionally leaves the current board in place.
             * If the app was closed before the next successful action,
             * restore the deferred "current game started" bookkeeping.
             */
            recordCurrentGameOnNextChangedAction =
                loadedSavedGame &&
                HasNoRecordedStats(statsManager.Data);
        }

        private void OnEnable()
        {
            newGameButton.onClick.AddListener(
                StartNewGame
            );

            if (
                undoButtonView != null &&
                undoButtonView.Button != null
            )
            {
                undoButtonView.Button.onClick.AddListener(
                    UseUndo
                );
            }

            if (
                killButtonView != null &&
                killButtonView.Button != null
            )
            {
                killButtonView.Button.onClick.AddListener(
                    ToggleKillSelection
                );
            }

            if (
                nukeButtonView != null &&
                nukeButtonView.Button != null
            )
            {
                nukeButtonView.Button.onClick.AddListener(
                    UseNuke
                );
            }

            boardView.TileClicked +=
                HandleTileClicked;

            if (gameStateOverlayView != null)
            {
                gameStateOverlayView.ContinueClicked +=
                    ContinueAfterWin;

                gameStateOverlayView.NewGameClicked +=
                    StartNewGame;
            }

            if (statsButton != null)
            {
                statsButton.onClick.AddListener(
                    ShowStatsScreen
                );
            }

            if (statsScreenView != null)
            {
                statsScreenView.BackClicked +=
                    HideStatsScreen;
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(
                    ShowSettingsScreen
                );
            }

            if (settingsScreenView != null)
            {
                settingsScreenView.BackClicked +=
                    HideSettingsScreen;

                settingsScreenView.MusicVolumeChanged +=
                    HandleMusicVolumeChanged;

                settingsScreenView.SfxVolumeChanged +=
                    HandleSfxVolumeChanged;

                settingsScreenView.ClearStatsRequested +=
                    HandleClearStatsRequested;
            }

            if (confirmationDialogView != null)
            {
                confirmationDialogView.Confirmed +=
                    HandleConfirmationConfirmed;

                confirmationDialogView.Cancelled +=
                    HandleConfirmationCancelled;
            }
        }

        private void Start()
        {
            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(true);
            }

            statsScreenView?.Hide();
            settingsScreenView?.Hide();
            confirmationDialogView?.Hide();

            ApplyAudioSettings();

            RefreshAll(
                immediateBanner: true
            );
        }

        private void Update()
        {
            if (
                IsGameplayInputBlocked ||
                selectionMode != SelectionMode.None ||
                (isAnimating && !isProcessingMoveQueue) ||
                game.Status != GameStatus.Playing
            )
            {
                ResetPointerState();
                return;
            }

            HandleKeyboardInput();
            HandlePointerInput();
        }

        private void OnDisable()
        {
            newGameButton.onClick.RemoveListener(
                StartNewGame
            );

            if (
                undoButtonView != null &&
                undoButtonView.Button != null
            )
            {
                undoButtonView.Button.onClick.RemoveListener(
                    UseUndo
                );
            }

            if (
                killButtonView != null &&
                killButtonView.Button != null
            )
            {
                killButtonView.Button.onClick.RemoveListener(
                    ToggleKillSelection
                );
            }

            if (
                nukeButtonView != null &&
                nukeButtonView.Button != null
            )
            {
                nukeButtonView.Button.onClick.RemoveListener(
                    UseNuke
                );
            }

            if (boardView != null)
            {
                boardView.TileClicked -=
                    HandleTileClicked;

                boardView.SetKillSelectionMode(false);
            }

            if (gameStateOverlayView != null)
            {
                gameStateOverlayView.ContinueClicked -=
                    ContinueAfterWin;

                gameStateOverlayView.NewGameClicked -=
                    StartNewGame;
            }

            if (statsButton != null)
            {
                statsButton.onClick.RemoveListener(
                    ShowStatsScreen
                );
            }

            if (statsScreenView != null)
            {
                statsScreenView.BackClicked -=
                    HideStatsScreen;
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(
                    ShowSettingsScreen
                );
            }

            if (settingsScreenView != null)
            {
                settingsScreenView.BackClicked -=
                    HideSettingsScreen;

                settingsScreenView.MusicVolumeChanged -=
                    HandleMusicVolumeChanged;

                settingsScreenView.SfxVolumeChanged -=
                    HandleSfxVolumeChanged;

                settingsScreenView.ClearStatsRequested -=
                    HandleClearStatsRequested;
            }

            if (confirmationDialogView != null)
            {
                confirmationDialogView.Confirmed -=
                    HandleConfirmationConfirmed;

                confirmationDialogView.Cancelled -=
                    HandleConfirmationCancelled;
            }

            undoButtonView?.SetAttention(false);
            killButtonView?.SetAttention(false);
            nukeButtonView?.SetAttention(false);

            moveQueue.Clear();

            selectionMode = SelectionMode.None;
            pendingConfirmationAction =
                PendingConfirmationAction.None;

            isProcessingMoveQueue = false;
            isConfirmationDialogOpen = false;
            isAnimating = false;

            ResetPointerState();
        }

        private void OnApplicationPause(
            bool pauseStatus
        )
        {
            if (!pauseStatus)
            {
                return;
            }

            SaveAll();
            SaveSettings();
        }

        private void OnApplicationQuit()
        {
            SaveAll();
            SaveSettings();
        }

        #endregion

        #region New game and confirmation

        public void StartNewGame()
        {
            if (
                isAnimating ||
                isConfirmationDialogOpen
            )
            {
                return;
            }

            ClearMoveQueue();

            /*
             * Leaving Kill mode before opening a modal avoids keeping
             * a hidden board-selection state behind the dialog.
             */
            selectionMode = SelectionMode.None;
            RefreshGameplayUx();

            if (game.Status == GameStatus.GameOver)
            {
                StartNewGameImmediately();
                return;
            }

            ShowConfirmation(
                PendingConfirmationAction.StartNewGame,
                title: "Start a new game?",
                message:
                    "Your current board and score will be lost.",
                confirmLabel: "New Game"
            );
        }

        private void StartNewGameImmediately()
        {
            ClearMoveQueue();

            selectionMode = SelectionMode.None;

            game.StartNewGame();
            statsManager.RecordGameStarted();

            recordCurrentGameOnNextChangedAction =
                false;

            showSwipeHintForCurrentGame =
                true;

            SaveAll();
            RefreshAll();
        }

        private void ShowConfirmation(
            PendingConfirmationAction action,
            string title,
            string message,
            string confirmLabel
        )
        {
            if (confirmationDialogView == null)
            {
                return;
            }

            pendingConfirmationAction = action;
            isConfirmationDialogOpen = true;

            ClearMoveQueue();
            SuppressPointerInputUntilReleased();
            RefreshGameplayUx();

            confirmationDialogView.Show(
                title,
                message,
                confirmLabel
            );
        }

        private void HandleConfirmationConfirmed()
        {
            PendingConfirmationAction action =
                pendingConfirmationAction;

            CloseConfirmationDialog();

            switch (action)
            {
                case PendingConfirmationAction.StartNewGame:
                    StartNewGameImmediately();
                    break;

                case PendingConfirmationAction.ClearStats:
                    ClearStatsImmediately();
                    break;

                case PendingConfirmationAction.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(action),
                        action,
                        "Unknown confirmation action."
                    );
            }
        }

        private void HandleConfirmationCancelled()
        {
            CloseConfirmationDialog();
        }

        private void CloseConfirmationDialog()
        {
            confirmationDialogView?.Hide();

            pendingConfirmationAction =
                PendingConfirmationAction.None;

            isConfirmationDialogOpen = false;

            SuppressPointerInputUntilReleased();
            RefreshGameplayUx();
        }

        #endregion

        #region Moves

        private void RequestMove(
            Direction direction
        )
        {
            if (
                IsGameplayInputBlocked ||
                selectionMode != SelectionMode.None ||
                game.Status != GameStatus.Playing
            )
            {
                return;
            }

            /*
             * isAnimating can also mean a power-up animation.
             * Only regular move animations accept buffered moves.
             */
            if (
                isAnimating &&
                !isProcessingMoveQueue
            )
            {
                return;
            }

            if (!isProcessingMoveQueue)
            {
                moveQueue.Enqueue(direction);
                isProcessingMoveQueue = true;

                StartCoroutine(
                    ProcessMoveQueue()
                );

                return;
            }

            /*
             * maxBufferedMoves counts pending moves only.
             * The currently animating move is not in the queue.
             */
            if (
                moveQueue.Count >=
                maxBufferedMoves
            )
            {
                return;
            }

            moveQueue.Enqueue(direction);
        }

        private IEnumerator ProcessMoveQueue()
        {
            SetAnimationState(true);
            ResetPointerState();

            while (moveQueue.Count > 0)
            {
                if (
                    game.Status != GameStatus.Playing ||
                    IsGameplayInputBlocked ||
                    selectionMode != SelectionMode.None
                )
                {
                    moveQueue.Clear();
                    break;
                }

                Direction direction =
                    moveQueue.Dequeue();

                yield return
                    HandleSingleMoveRoutine(
                        direction
                    );
            }

            moveQueue.Clear();
            isProcessingMoveQueue = false;

            SetAnimationState(false);
        }

        private IEnumerator HandleSingleMoveRoutine(
            Direction direction
        )
        {
            GameActionResult result =
                game.HandleMove(direction);

            Debug.Log(result.ToString());

            PrepareStatsForChangedAction(result);

            statsManager.RecordMove(
                result,
                game.Board,
                game.Score
            );

            if (!result.Changed)
            {
                yield break;
            }

            if (showSwipeHintForCurrentGame)
            {
                showSwipeHintForCurrentGame =
                    false;

                /*
                 * The model has already calculated the new status,
                 * so this correctly prefers "Use a power-up!" if the
                 * first successful move also leaves no legal moves.
                 */
                RefreshContextualUx();
            }

            gameAudio?.PlaySwipe();

            SaveAll();

            yield return boardView.AnimateMove(
                game.Board,
                result
            );

            PlayEndStateSound(result);

            scoreView.SetScores(
                game.Score,
                statsManager.Data.BestScore
            );

            RefreshGameStateOverlay();
        }

        #endregion

        #region Power-ups

        public void UseUndo()
        {
            if (
                isAnimating ||
                selectionMode != SelectionMode.None
            )
            {
                return;
            }

            ClearMoveQueue();

            StartCoroutine(
                HandleUndoRoutine()
            );
        }

        private IEnumerator HandleUndoRoutine()
        {
            SetAnimationState(true);
            SuppressPointerInputUntilReleased();

            GameActionResult result =
                game.UseUndoPowerup();

            Debug.Log(result.ToString());

            PrepareStatsForChangedAction(result);

            statsManager.RecordPowerupUse(
                PowerupType.Undo,
                result,
                game.Board,
                game.Score
            );

            if (!result.Changed)
            {
                SetAnimationState(false);
                RefreshAll();
                yield break;
            }

            SaveAll();
            RefreshGameplayUx();

            yield return boardView.AnimateUndo(
                game.Board
            );

            scoreView.SetScores(
                game.Score,
                statsManager.Data.BestScore
            );

            RefreshGameStateOverlay();
            SetAnimationState(false);
        }

        public void UseNuke()
        {
            if (
                isAnimating ||
                selectionMode != SelectionMode.None
            )
            {
                return;
            }

            ClearMoveQueue();

            StartCoroutine(
                HandleNukeRoutine()
            );
        }

        private IEnumerator HandleNukeRoutine()
        {
            SetAnimationState(true);
            SuppressPointerInputUntilReleased();

            GameActionResult result =
                game.UseNukePowerup();

            Debug.Log(result.ToString());

            PrepareStatsForChangedAction(result);

            statsManager.RecordPowerupUse(
                PowerupType.Nuke,
                result,
                game.Board,
                game.Score
            );

            if (!result.Changed)
            {
                SetAnimationState(false);
                RefreshAll();
                yield break;
            }

            SaveAll();
            RefreshGameplayUx();

            uiVfx?.PlayNukeVfx();

            yield return boardView.AnimateNuke(
                game.Board
            );

            PlayEndStateSound(result);
            RefreshGameStateOverlay();

            SetAnimationState(false);
        }

        private void ToggleKillSelection()
        {
            if (isAnimating)
            {
                return;
            }

            ClearMoveQueue();

            if (
                selectionMode ==
                SelectionMode.Kill
            )
            {
                selectionMode =
                    SelectionMode.None;

                Debug.Log(
                    "Kill selection cancelled."
                );

                RefreshGameplayUx();
                return;
            }

            if (
                !game.PowerupCharges.CanUse(
                    PowerupType.Kill
                )
            )
            {
                return;
            }

            bool hasTiles =
                game.Board
                    .GetOccupiedPositions()
                    .Count > 0;

            if (!hasTiles)
            {
                return;
            }

            selectionMode =
                SelectionMode.Kill;

            isPointerDown = false;
            suppressPointerInputUntilRelease =
                true;

            Debug.Log(
                "Kill mode active. Click a tile to remove it."
            );

            RefreshGameplayUx();
        }

        private void HandleTileClicked(
            CellPosition position
        )
        {
            if (
                isAnimating ||
                selectionMode != SelectionMode.Kill
            )
            {
                return;
            }

            suppressPointerInputUntilRelease =
                true;

            isPointerDown = false;

            StartCoroutine(
                HandleKillRoutine(position)
            );
        }

        private IEnumerator HandleKillRoutine(
            CellPosition position
        )
        {
            SetAnimationState(true);

            selectionMode =
                SelectionMode.None;

            GameActionResult result =
                game.UseKillPowerup(position);

            Debug.Log(result.ToString());

            PrepareStatsForChangedAction(result);

            statsManager.RecordPowerupUse(
                PowerupType.Kill,
                result,
                game.Board,
                game.Score
            );

            if (!result.Changed)
            {
                SetAnimationState(false);
                RefreshAll();
                yield break;
            }

            SaveAll();
            RefreshGameplayUx();

            yield return boardView.AnimateKill(
                position,
                game.Board
            );

            PlayEndStateSound(result);
            RefreshGameStateOverlay();

            SetAnimationState(false);
        }

        #endregion

        #region UI refresh

        private void SetAnimationState(
            bool animating
        )
        {
            if (isAnimating == animating)
            {
                return;
            }

            isAnimating = animating;

            if (!animating)
            {
                RefreshGameplayUx();
            }
        }

        private void RefreshAll(
            bool immediateBanner = false
        )
        {
            boardView.Refresh(game.Board);

            scoreView.SetScores(
                game.Score,
                statsManager.Data.BestScore
            );

            RefreshGameStateOverlay();

            if (IsMainScreenVisible)
            {
                RefreshGameplayUx(
                    immediateBanner
                );
            }
        }

        private void RefreshGameplayUx(
            bool immediateBanner = false
        )
        {
            if (!IsMainScreenVisible)
            {
                return;
            }

            RefreshMenuButtons();
            RefreshPowerupButtons();
            RefreshContextualUx(
                immediateBanner
            );
        }

        private void RefreshMenuButtons()
        {
            bool interactable =
                !isConfirmationDialogOpen;

            newGameButton.interactable =
                interactable;

            if (statsButton != null)
            {
                statsButton.interactable =
                    interactable;
            }

            if (settingsButton != null)
            {
                settingsButton.interactable =
                    interactable;
            }
        }

        private void RefreshPowerupButtons()
        {
            bool canInteract =
                !IsGameplayInputBlocked;

            bool canUsePowerups =
                game.Status == GameStatus.Playing ||
                game.Status == GameStatus.OutOfMoves;

            bool isSelecting =
                selectionMode != SelectionMode.None;

            bool shouldHighlightRecovery =
                game.Status == GameStatus.OutOfMoves &&
                !IsGameplayInputBlocked;

            if (undoButtonView != null)
            {
                bool canUseUndo =
                    canUsePowerups &&
                    !isSelecting &&
                    game.CanUseUndoPowerup();

                undoButtonView.SetInteractable(
                    canInteract &&
                    canUseUndo
                );

                undoButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Undo
                    )
                );

                undoButtonView.SetAttention(
                    shouldHighlightRecovery &&
                    canUseUndo
                );
            }

            if (killButtonView != null)
            {
                bool canUseKill =
                    canUsePowerups &&
                    game.CanUseKillPowerup();

                /*
                 * Kill stays interactable while selected so pressing
                 * the same button again can cancel target selection.
                 */
                killButtonView.SetInteractable(
                    canInteract &&
                    canUseKill
                );

                killButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Kill
                    )
                );

                killButtonView.SetAttention(
                    shouldHighlightRecovery &&
                    canUseKill
                );
            }

            if (nukeButtonView != null)
            {
                bool canUseNuke =
                    canUsePowerups &&
                    !isSelecting &&
                    game.CanUseNukePowerup();

                nukeButtonView.SetInteractable(
                    canInteract &&
                    canUseNuke
                );

                nukeButtonView.SetChargeCount(
                    game.PowerupCharges.GetCharges(
                        PowerupType.Nuke
                    )
                );

                nukeButtonView.SetAttention(
                    shouldHighlightRecovery &&
                    canUseNuke
                );
            }
        }

        private void RefreshContextualUx(
            bool immediateBanner = false
        )
        {
            bool killSelectionActive =
                selectionMode ==
                SelectionMode.Kill;

            boardView.SetKillSelectionMode(
                killSelectionActive
            );

            BannerMessageType bannerType;

            /*
             * Context priority:
             * 1. An active target-selection instruction.
             * 2. Recovery from an otherwise blocked board.
             * 3. New-game onboarding.
             * 4. The normal title banner.
             */
            if (killSelectionActive)
            {
                bannerType =
                    BannerMessageType.ChooseTile;
            }
            else if (
                game.Status ==
                GameStatus.OutOfMoves
            )
            {
                bannerType =
                    BannerMessageType.UsePowerup;
            }
            else if (
                showSwipeHintForCurrentGame
            )
            {
                bannerType =
                    BannerMessageType.SwipeToMerge;
            }
            else
            {
                bannerType =
                    BannerMessageType.Default;
            }

            bannerMessageView.Show(
                bannerType,
                immediateBanner
            );
        }

        private void RefreshGameStateOverlay()
        {
            if (gameStateOverlayView == null)
            {
                return;
            }

            switch (game.Status)
            {
                case GameStatus.Won:
                    gameStateOverlayView.ShowWin();
                    break;

                case GameStatus.GameOver:
                    gameStateOverlayView.ShowGameOver();
                    break;

                default:
                    gameStateOverlayView.Hide();
                    break;
            }
        }

        private void PlayEndStateSound(
            GameActionResult result
        )
        {
            if (
                result == null ||
                gameAudio == null
            )
            {
                return;
            }

            if (result.ReachedTargetThisAction)
            {
                gameAudio.PlayWin();
            }
            else if (result.GameOverThisAction)
            {
                gameAudio.PlayLose();
            }
        }

        #endregion

        #region Stats and settings

        private void PrepareStatsForChangedAction(
            GameActionResult result
        )
        {
            if (
                result == null ||
                !result.Changed ||
                !recordCurrentGameOnNextChangedAction
            )
            {
                return;
            }

            statsManager.RecordGameStarted();

            recordCurrentGameOnNextChangedAction =
                false;
        }

        private void HandleClearStatsRequested()
        {
            if (isConfirmationDialogOpen)
            {
                return;
            }

            ShowConfirmation(
                PendingConfirmationAction.ClearStats,
                title: "Clear all stats?",
                message:
                    "Scores, game history, and power-up " +
                    "totals will be permanently reset.\n\n" +
                    "Your current board will remain.",
                confirmLabel: "Clear Stats"
            );
        }

        private void ClearStatsImmediately()
        {
            statsStore.Reset();

            statsManager =
                new StatsManager();

            recordCurrentGameOnNextChangedAction =
                true;

            SaveStats();
            RefreshAll();
        }

        private void HandleMusicVolumeChanged(
            float value
        )
        {
            if (settingsData == null)
            {
                return;
            }

            settingsData.MusicVolume =
                Mathf.Clamp01(value);

            gameAudio?.SetMusicVolume(
                settingsData.MusicVolume
            );
        }

        private void HandleSfxVolumeChanged(
            float value
        )
        {
            if (settingsData == null)
            {
                return;
            }

            settingsData.SfxVolume =
                Mathf.Clamp01(value);

            gameAudio?.SetSfxVolume(
                settingsData.SfxVolume
            );
        }

        private void ApplyAudioSettings()
        {
            if (
                gameAudio == null ||
                settingsData == null
            )
            {
                return;
            }

            gameAudio.SetMusicVolume(
                settingsData.MusicVolume
            );

            gameAudio.SetSfxVolume(
                settingsData.SfxVolume
            );
        }

        #endregion

        #region Secondary screens

        private void ShowStatsScreen()
        {
            if (isAnimating)
            {
                return;
            }

            ClearMoveQueue();
            SaveStats();

            selectionMode =
                SelectionMode.None;

            boardView.SetKillSelectionMode(
                false
            );

            isStatsScreenOpen = true;
            SuppressPointerInputUntilReleased();

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(false);
            }

            statsScreenView?.Show(
                statsManager.Data
            );
        }

        private void HideStatsScreen()
        {
            statsScreenView?.Hide();

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(true);
            }

            isStatsScreenOpen = false;
            SuppressPointerInputUntilReleased();

            RefreshAll(
                immediateBanner: true
            );
        }

        private void ShowSettingsScreen()
        {
            if (isAnimating)
            {
                return;
            }

            ClearMoveQueue();

            selectionMode =
                SelectionMode.None;

            boardView.SetKillSelectionMode(
                false
            );

            isSettingsScreenOpen = true;
            SuppressPointerInputUntilReleased();

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(false);
            }

            settingsScreenView?.Show(
                settingsData
            );
        }

        private void HideSettingsScreen()
        {
            SaveSettings();

            settingsScreenView?.Hide();

            if (mainScreenRoot != null)
            {
                mainScreenRoot.SetActive(true);
            }

            isSettingsScreenOpen = false;
            SuppressPointerInputUntilReleased();

            RefreshAll(
                immediateBanner: true
            );
        }

        #endregion

        #region Input

        private void HandleKeyboardInput()
        {
            if (
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.A)
            )
            {
                RequestMove(Direction.Left);
            }
            else if (
                Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.D)
            )
            {
                RequestMove(Direction.Right);
            }
            else if (
                Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.W)
            )
            {
                RequestMove(Direction.Up);
            }
            else if (
                Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.S)
            )
            {
                RequestMove(Direction.Down);
            }
        }

        private void HandlePointerInput()
        {
            if (suppressPointerInputUntilRelease)
            {
                bool hasActiveMousePress =
                    Input.GetMouseButton(0);

                bool hasActiveTouchPress =
                    Input.touchCount > 0;

                if (
                    !hasActiveMousePress &&
                    !hasActiveTouchPress
                )
                {
                    suppressPointerInputUntilRelease =
                        false;

                    ResetPointerState();
                }

                return;
            }

            if (Input.touchCount > 0)
            {
                Touch touch =
                    Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        BeginPointer(
                            touch.position
                        );
                        break;

                    case TouchPhase.Ended:
                        EndPointer(
                            touch.position
                        );
                        break;

                    case TouchPhase.Canceled:
                        ResetPointerState();
                        break;
                }

                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                BeginPointer(
                    Input.mousePosition
                );
            }
            else if (
                Input.GetMouseButtonUp(0)
            )
            {
                EndPointer(
                    Input.mousePosition
                );
            }
        }

        private void BeginPointer(
            Vector2 screenPosition
        )
        {
            if (
                !IsInsideSwipeArea(
                    screenPosition
                )
            )
            {
                ResetPointerState();
                return;
            }

            pointerDownPosition =
                screenPosition;

            isPointerDown = true;
        }

        private void EndPointer(
            Vector2 screenPosition
        )
        {
            if (!isPointerDown)
            {
                return;
            }

            isPointerDown = false;

            Vector2 delta =
                screenPosition -
                pointerDownPosition;

            if (
                delta.magnitude <
                minimumSwipeDistance
            )
            {
                return;
            }

            Direction? direction =
                GetSwipeDirection(delta);

            if (direction.HasValue)
            {
                RequestMove(
                    direction.Value
                );
            }
        }

        private bool IsInsideSwipeArea(
            Vector2 screenPosition
        )
        {
            Canvas canvas =
                swipeArea.GetComponentInParent<Canvas>();

            Camera uiCamera = null;

            if (
                canvas != null &&
                canvas.renderMode !=
                RenderMode.ScreenSpaceOverlay
            )
            {
                uiCamera =
                    canvas.worldCamera;
            }

            bool converted =
                RectTransformUtility
                    .ScreenPointToLocalPointInRectangle(
                        swipeArea,
                        screenPosition,
                        uiCamera,
                        out Vector2 localPosition
                    );

            if (!converted)
            {
                return false;
            }

            Rect allowedRect =
                swipeArea.rect;

            allowedRect.xMin -=
                swipeAreaPadding;

            allowedRect.xMax +=
                swipeAreaPadding;

            allowedRect.yMin -=
                swipeAreaPadding;

            allowedRect.yMax +=
                swipeAreaPadding;

            return allowedRect.Contains(
                localPosition
            );
        }

        private Direction? GetSwipeDirection(
            Vector2 delta
        )
        {
            Vector2 normalized =
                delta.normalized;

            if (
                Mathf.Abs(normalized.x) >
                Mathf.Abs(normalized.y)
            )
            {
                if (
                    Mathf.Abs(normalized.x) <
                    swipeDirectionThreshold
                )
                {
                    return null;
                }

                return normalized.x > 0f
                    ? Direction.Right
                    : Direction.Left;
            }

            if (
                Mathf.Abs(normalized.y) <
                swipeDirectionThreshold
            )
            {
                return null;
            }

            return normalized.y > 0f
                ? Direction.Up
                : Direction.Down;
        }

        private void ResetPointerState()
        {
            pointerDownPosition =
                Vector2.zero;

            isPointerDown = false;
        }

        private void SuppressPointerInputUntilReleased()
        {
            ResetPointerState();

            suppressPointerInputUntilRelease =
                true;
        }

        private void ClearMoveQueue()
        {
            moveQueue.Clear();
        }

        #endregion

        #region Continue, persistence, and validation

        public void ContinueAfterWin()
        {
            if (isAnimating)
            {
                return;
            }

            ClearMoveQueue();

            GameActionResult result =
                game.ContinueAfterWin();

            Debug.Log(result.ToString());

            if (result.Changed)
            {
                SaveCurrentGame();
            }

            RefreshAll();
        }

        private void SaveStats()
        {
            statsStore.Save(
                statsManager.Data
            );
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

        private void SaveSettings()
        {
            if (
                settingsStore == null ||
                settingsData == null
            )
            {
                return;
            }

            settingsStore.Save(
                settingsData
            );
        }

        private static bool HasNoRecordedStats(
            StatsData data
        )
        {
            if (data == null)
            {
                return true;
            }

            return
                data.GamesStarted == 0 &&
                data.GamesWon == 0 &&
                data.GamesLost == 0 &&
                data.BestScore == 0 &&
                data.HighestTile == 0 &&
                data.TotalMoves == 0 &&
                data.TotalMerges == 0 &&
                data.UndoUses == 0 &&
                data.KillUses == 0 &&
                data.NukeUses == 0;
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

            if (confirmationDialogView == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a " +
                    "ConfirmationDialogView reference."
                );
            }

            if (bannerMessageView == null)
            {
                throw new InvalidOperationException(
                    "GameController is missing a " +
                    "BannerMessageView reference."
                );
            }
        }

        #endregion
    }
}