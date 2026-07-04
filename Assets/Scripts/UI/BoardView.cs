using System;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private int boardSize = 5;
        [SerializeField] private RectTransform cellGrid;
        [SerializeField] private RectTransform tileLayer;
        [SerializeField] private TileView tilePrefab;

        public event Action<CellPosition> TileClicked;

        public void Refresh(BoardModel board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (board.Size != boardSize)
            {
                throw new ArgumentException(
                    $"Board size {board.Size} does not match BoardView size {boardSize}.",
                    nameof(board)
                );
            }

            ValidateReferences();

            // Make sure Unity has calculated the Grid Layout positions
            // before we copy cell positions.
            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(cellGrid);

            ClearTiles();

            for (int row = 0; row < board.Size; row++)
            {
                for (int col = 0; col < board.Size; col++)
                {
                    TileData tile = board.GetTile(row, col);

                    if (tile == null)
                    {
                        continue;
                    }

                    CellPosition position = new CellPosition(row, col);
                    CreateTileView(position, tile);
                }
            }
        }

        public void ClearTiles()
        {
            ValidateTileLayer();

            for (int i = tileLayer.childCount - 1; i >= 0; i--)
            {
                Transform child = tileLayer.GetChild(i);

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void CreateTileView(CellPosition position, TileData tile)
        {
            RectTransform cellRect = GetCellRect(position);

            TileView tileView = Instantiate(tilePrefab, tileLayer);
            RectTransform tileRect = tileView.GetComponent<RectTransform>();

            MatchRectTransformToCell(tileRect, cellRect);

            tileView.Initialize(position, tile);
            tileView.Clicked += HandleTileClicked;
        }

        private RectTransform GetCellRect(CellPosition position)
        {
            int index = position.Row * boardSize + position.Col;

            if (index < 0 || index >= cellGrid.childCount)
            {
                throw new InvalidOperationException(
                    $"No cell RectTransform found for position {position}. " +
                    $"Expected at least {boardSize * boardSize} cells under CellGrid."
                );
            }

            RectTransform cellRect = cellGrid.GetChild(index) as RectTransform;

            if (cellRect == null)
            {
                throw new InvalidOperationException(
                    $"Cell at index {index} does not have a RectTransform."
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
                throw new ArgumentNullException(nameof(tileRect));
            }

            if (cellRect == null)
            {
                throw new ArgumentNullException(nameof(cellRect));
            }

            Vector3[] worldCorners = new Vector3[4];
            cellRect.GetWorldCorners(worldCorners);

            Vector3 bottomLeftLocal = tileLayer.InverseTransformPoint(worldCorners[0]);
            Vector3 topRightLocal = tileLayer.InverseTransformPoint(worldCorners[2]);

            Vector2 size = new Vector2(
                topRightLocal.x - bottomLeftLocal.x,
                topRightLocal.y - bottomLeftLocal.y
            );

            Vector2 center = new Vector2(
                bottomLeftLocal.x + size.x / 2f,
                bottomLeftLocal.y + size.y / 2f
            );

            tileRect.anchorMin = new Vector2(0.5f, 0.5f);
            tileRect.anchorMax = new Vector2(0.5f, 0.5f);
            tileRect.pivot = new Vector2(0.5f, 0.5f);
            tileRect.anchoredPosition = center;
            tileRect.sizeDelta = size;
            tileRect.localScale = Vector3.one;
            tileRect.localRotation = Quaternion.identity;
        }

        private void HandleTileClicked(CellPosition position)
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
                    "BoardView is missing a CellGrid reference."
                );
            }
        }

        private void ValidateTileLayer()
        {
            if (tileLayer == null)
            {
                throw new InvalidOperationException(
                    "BoardView is missing a TileLayer reference."
                );
            }
        }

        private void ValidateTilePrefab()
        {
            if (tilePrefab == null)
            {
                throw new InvalidOperationException(
                    "BoardView is missing a TileView prefab reference."
                );
            }
        }

        private void ValidateCellCount()
        {
            int requiredCellCount = boardSize * boardSize;

            if (cellGrid.childCount < requiredCellCount)
            {
                throw new InvalidOperationException(
                    $"CellGrid has {cellGrid.childCount} children, " +
                    $"but BoardView requires at least {requiredCellCount}."
                );
            }
        }

        private void OnValidate()
        {
            if (boardSize < 2)
            {
                boardSize = 2;
            }
        }
    }
}