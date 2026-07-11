using System;
using System.Collections;
using System.Collections.Generic;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Board")]
        [SerializeField] private int boardSize = 5;
        [SerializeField] private RectTransform cellGrid;
        [SerializeField] private RectTransform tileLayer;
        [SerializeField] private TileView tilePrefab;

        [Header("Animation")]
        [SerializeField] private float moveDuration = 0.14f;
        [SerializeField] private float mergeDuration = 0.12f;
        [SerializeField] private float spawnDuration = 0.1f;
        [SerializeField] private float killDuration = 0.16f;
        [SerializeField] private float mergeScale = 1.15f;

        private readonly Dictionary<CellPosition, TileView> tileViews =
            new Dictionary<CellPosition, TileView>();

        public event Action<CellPosition> TileClicked;

        public IEnumerator AnimateMove(
            BoardModel finalBoard,
            GameActionResult result
        )
        {
            ValidateReferences();

            yield return AnimateTileMovements(
                result.TileMovements
            );

            Refresh(finalBoard);

            yield return AnimateResultTiles(result);
        }

        public IEnumerator AnimateKill(
            CellPosition position,
            BoardModel finalBoard
        )
        {
            if (finalBoard == null)
            {
                throw new ArgumentNullException(
                    nameof(finalBoard)
                );
            }

            ValidateReferences();

            if (
                tileViews.TryGetValue(
                    position,
                    out TileView tileView
                ) &&
                tileView != null
            )
            {
                yield return tileView.PlayKillAnimation(
                    killDuration
                );
            }

            Refresh(finalBoard);
        }

        private IEnumerator AnimateResultTiles(
            GameActionResult result
        )
        {
            for (int i = 0; i < result.MergePositions.Count; i++)
            {
                CellPosition position =
                    result.MergePositions[i];

                if (
                    tileViews.TryGetValue(
                        position,
                        out TileView tileView
                    )
                )
                {
                    StartCoroutine(
                        tileView.PlayMergeAnimation(
                            mergeDuration,
                            mergeScale
                        )
                    );
                }
            }

            if (
                result.SpawnResult != null &&
                result.SpawnResult.Spawned &&
                tileViews.TryGetValue(
                    result.SpawnResult.Position,
                    out TileView spawnedTile
                )
            )
            {
                StartCoroutine(
                    spawnedTile.PlaySpawnAnimation(
                        spawnDuration
                    )
                );
            }

            float waitDuration = Mathf.Max(
                result.MergePositions.Count > 0
                    ? mergeDuration
                    : 0f,
                result.SpawnResult.Spawned
                    ? spawnDuration
                    : 0f
            );

            if (waitDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    waitDuration
                );
            }
        }

        private IEnumerator AnimateTileMovements(
            IReadOnlyList<TileMovement> movements
        )
        {
            Dictionary<RectTransform, Vector2>
                startingPositions =
                    new Dictionary<RectTransform, Vector2>();

            Dictionary<RectTransform, Vector2>
                targetPositions =
                    new Dictionary<RectTransform, Vector2>();

            for (int i = 0; i < movements.Count; i++)
            {
                TileMovement movement = movements[i];

                if (
                    !tileViews.TryGetValue(
                        movement.From,
                        out TileView tileView
                    )
                )
                {
                    continue;
                }

                RectTransform tileRect =
                    tileView.GetComponent<RectTransform>();

                startingPositions[tileRect] =
                    tileRect.anchoredPosition;

                targetPositions[tileRect] =
                    GetCellAnchoredPosition(
                        movement.To
                    );
            }

            if (moveDuration <= 0f)
            {
                foreach (
                    KeyValuePair<RectTransform, Vector2> entry
                    in targetPositions
                )
                {
                    entry.Key.anchoredPosition =
                        entry.Value;
                }

                yield break;
            }

            float elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / moveDuration
                    );

                float easedProgress =
                    1f - Mathf.Pow(
                        1f - progress,
                        3f
                    );

                foreach (
                    KeyValuePair<RectTransform, Vector2> entry
                    in targetPositions
                )
                {
                    RectTransform tileRect =
                        entry.Key;

                    tileRect.anchoredPosition =
                        Vector2.LerpUnclamped(
                            startingPositions[tileRect],
                            entry.Value,
                            easedProgress
                        );
                }

                yield return null;
            }

            foreach (
                KeyValuePair<RectTransform, Vector2> entry
                in targetPositions
            )
            {
                entry.Key.anchoredPosition =
                    entry.Value;
            }
        }

        private Vector2 GetCellAnchoredPosition(
            CellPosition position
        )
        {
            RectTransform cellRect =
                GetCellRect(position);

            Vector3[] worldCorners =
                new Vector3[4];

            cellRect.GetWorldCorners(worldCorners);

            Vector3 bottomLeftLocal =
                tileLayer.InverseTransformPoint(
                    worldCorners[0]
                );

            Vector3 topRightLocal =
                tileLayer.InverseTransformPoint(
                    worldCorners[2]
                );

            return new Vector2(
                (
                    bottomLeftLocal.x +
                    topRightLocal.x
                ) / 2f,
                (
                    bottomLeftLocal.y +
                    topRightLocal.y
                ) / 2f
            );
        }

        public void Refresh(BoardModel board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(
                    nameof(board)
                );
            }

            if (board.Size != boardSize)
            {
                throw new ArgumentException(
                    $"Board size {board.Size} does not match " +
                    $"BoardView size {boardSize}.",
                    nameof(board)
                );
            }

            ValidateReferences();

            // Make sure Unity has calculated the Grid Layout positions
            // before we copy cell positions.
            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                cellGrid
            );

            ClearTiles();

            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    TileData tile =
                        board.GetTile(row, col);

                    if (tile == null)
                    {
                        continue;
                    }

                    CellPosition position =
                        new CellPosition(row, col);

                    CreateTileView(position, tile);
                }
            }
        }

        public void ClearTiles()
        {
            ValidateTileLayer();

            tileViews.Clear();

            for (
                int i = tileLayer.childCount - 1;
                i >= 0;
                i--
            )
            {
                Transform child =
                    tileLayer.GetChild(i);

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(
                        child.gameObject
                    );
                }
            }
        }

        private void CreateTileView(
            CellPosition position,
            TileData tile
        )
        {
            RectTransform cellRect =
                GetCellRect(position);

            TileView tileView = Instantiate(
                tilePrefab,
                tileLayer
            );

            RectTransform tileRect =
                tileView.GetComponent<RectTransform>();

            MatchRectTransformToCell(
                tileRect,
                cellRect
            );

            tileView.Initialize(position, tile);
            tileView.Clicked += HandleTileClicked;

            tileViews[position] = tileView;
        }

        private RectTransform GetCellRect(
            CellPosition position
        )
        {
            int index =
                position.Row * boardSize +
                position.Col;

            if (
                index < 0 ||
                index >= cellGrid.childCount
            )
            {
                throw new InvalidOperationException(
                    $"No cell RectTransform found for " +
                    $"position {position}. Expected at least " +
                    $"{boardSize * boardSize} cells under CellGrid."
                );
            }

            RectTransform cellRect =
                cellGrid.GetChild(index)
                    as RectTransform;

            if (cellRect == null)
            {
                throw new InvalidOperationException(
                    $"Cell at index {index} does not " +
                    "have a RectTransform."
                );
            }

            return cellRect;
        }

        private void MatchRectTransformToCell(
            RectTransform tileRect,
            RectTransform cellRect
        )
        {
            if (tileRect == null)
            {
                throw new ArgumentNullException(
                    nameof(tileRect)
                );
            }

            if (cellRect == null)
            {
                throw new ArgumentNullException(
                    nameof(cellRect)
                );
            }

            Vector3[] worldCorners =
                new Vector3[4];

            cellRect.GetWorldCorners(worldCorners);

            Vector3 bottomLeftLocal =
                tileLayer.InverseTransformPoint(
                    worldCorners[0]
                );

            Vector3 topRightLocal =
                tileLayer.InverseTransformPoint(
                    worldCorners[2]
                );

            Vector2 size = new Vector2(
                topRightLocal.x -
                    bottomLeftLocal.x,
                topRightLocal.y -
                    bottomLeftLocal.y
            );

            Vector2 center = new Vector2(
                bottomLeftLocal.x +
                    size.x / 2f,
                bottomLeftLocal.y +
                    size.y / 2f
            );

            tileRect.anchorMin =
                new Vector2(0.5f, 0.5f);

            tileRect.anchorMax =
                new Vector2(0.5f, 0.5f);

            tileRect.pivot =
                new Vector2(0.5f, 0.5f);

            tileRect.anchoredPosition = center;
            tileRect.sizeDelta = size;
            tileRect.localScale = Vector3.one;
            tileRect.localRotation =
                Quaternion.identity;
        }

        private void HandleTileClicked(
            CellPosition position
        )
        {
            TileClicked?.Invoke(position);
        }

        private void ValidateReferences()
        {
            ValidateCellGrid();
            ValidateTileLayer();
            ValidateTilePrefab();
            ValidateCellCount();
        }

        private void ValidateCellGrid()
        {
            if (cellGrid == null)
            {
                throw new InvalidOperationException(
                    "BoardView is missing a " +
                    "CellGrid reference."
                );
            }
        }

        private void ValidateTileLayer()
        {
            if (tileLayer == null)
            {
                throw new InvalidOperationException(
                    "BoardView is missing a " +
                    "TileLayer reference."
                );
            }
        }

        private void ValidateTilePrefab()
        {
            if (tilePrefab == null)
            {
                throw new InvalidOperationException(
                    "BoardView is missing a " +
                    "TileView prefab reference."
                );
            }
        }

        private void ValidateCellCount()
        {
            int requiredCellCount =
                boardSize * boardSize;

            if (
                cellGrid.childCount <
                requiredCellCount
            )
            {
                throw new InvalidOperationException(
                    $"CellGrid has {cellGrid.childCount} children, " +
                    $"but BoardView requires at least " +
                    $"{requiredCellCount}."
                );
            }
        }

        private void OnValidate()
        {
            if (boardSize < 2)
            {
                boardSize = 2;
            }

            moveDuration =
                Mathf.Max(0f, moveDuration);

            mergeDuration =
                Mathf.Max(0f, mergeDuration);

            spawnDuration =
                Mathf.Max(0f, spawnDuration);

            killDuration =
                Mathf.Max(0f, killDuration);

            mergeScale =
                Mathf.Max(1f, mergeScale);
        }
    }
}