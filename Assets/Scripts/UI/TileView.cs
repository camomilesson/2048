using System;
using TMPro;
using TwentyFortyEight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class TileView : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Button button;

        private CellPosition position;

        public CellPosition Position
        {
            get
            {
                return position;
            }
        }

        public event Action<CellPosition> Clicked;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void Initialize(CellPosition tilePosition, TileData tile)
        {
            if (tile == null)
            {
                throw new ArgumentNullException(nameof(tile));
            }

            position = tilePosition;
            SetValue(tile.Value);
        }

        public void SetPosition(CellPosition tilePosition)
        {
            position = tilePosition;
        }

        public void SetValue(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Tile value must be greater than zero."
                );
            }

            if (valueText != null)
            {
                valueText.text = value.ToString();
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = GetBackgroundColor(value);
            }

            if (valueText != null)
            {
                valueText.color = GetTextColor(value);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void HandleClick()
        {
            Clicked?.Invoke(position);
        }

        private static Color GetBackgroundColor(int value)
        {
            switch (value)
            {
                case 2:
                    return new Color32(238, 228, 218, 255);

                case 4:
                    return new Color32(237, 224, 200, 255);

                case 8:
                    return new Color32(242, 177, 121, 255);

                case 16:
                    return new Color32(245, 149, 99, 255);

                case 32:
                    return new Color32(246, 124, 95, 255);

                case 64:
                    return new Color32(246, 94, 59, 255);

                case 128:
                    return new Color32(237, 207, 114, 255);

                case 256:
                    return new Color32(237, 204, 97, 255);

                case 512:
                    return new Color32(237, 200, 80, 255);

                case 1024:
                    return new Color32(237, 197, 63, 255);

                case 2048:
                    return new Color32(237, 194, 46, 255);

                default:
                    return new Color32(60, 58, 50, 255);
            }
        }

        private static Color GetTextColor(int value)
        {
            if (value <= 4)
            {
                return new Color32(119, 110, 101, 255);
            }

            return Color.white;
        }
    }
}