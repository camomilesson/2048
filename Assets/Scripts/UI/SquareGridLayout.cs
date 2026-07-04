using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class SquareGridLayout : MonoBehaviour
    {
        [SerializeField] private int gridSize = 5;

        private GridLayoutGroup gridLayoutGroup;
        private RectTransform rectTransform;

        private void Awake()
        {
            gridLayoutGroup = GetComponent<GridLayoutGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (gridLayoutGroup == null || rectTransform == null)
            {
                return;
            }

            UpdateCellSize();
        }

        private void OnValidate()
        {
            if (gridSize < 1)
            {
                gridSize = 1;
            }
        }

        private void UpdateCellSize()
        {
            RectOffset padding = gridLayoutGroup.padding;
            Vector2 spacing = gridLayoutGroup.spacing;

            float availableWidth =
                rectTransform.rect.width
                - padding.left
                - padding.right
                - spacing.x * (gridSize - 1);

            float availableHeight =
                rectTransform.rect.height
                - padding.top
                - padding.bottom
                - spacing.y * (gridSize - 1);

            float cellSize = Mathf.Min(availableWidth, availableHeight) / gridSize;

            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = gridSize;
        }
    }
}