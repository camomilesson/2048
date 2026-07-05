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

        [Header("Visuals")]
        [SerializeField] private TileVisualLibrary tileVisualLibrary;
        [SerializeField] private Color numberTextColor =
            new Color32(70, 45, 30, 255);

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
                valueText.color = numberTextColor;
            }

            ApplySpriteForValue(value);
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void ApplySpriteForValue(int value)
        {
            if (backgroundImage == null)
            {
                return;
            }

            Sprite sprite = tileVisualLibrary != null
                ? tileVisualLibrary.GetSpriteForValue(value)
                : null;

            if (sprite != null)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.color = Color.white;
                backgroundImage.type = Image.Type.Simple;
                backgroundImage.preserveAspect = true;
                return;
            }

            backgroundImage.sprite = null;
            backgroundImage.color = GetFallbackBackgroundColor(value);
        }

        private void HandleClick()
        {
            Clicked?.Invoke(position);
        }

        private static Color GetFallbackBackgroundColor(int value)
        {
            switch (value)
            {
                case 2: return new Color32(238, 228, 218, 255);
                case 4: return new Color32(237, 224, 200, 255);
                case 8: return new Color32(242, 177, 121, 255);
                case 16: return new Color32(245, 149, 99, 255);
                case 32: return new Color32(246, 124, 95, 255);
                case 64: return new Color32(246, 94, 59, 255);
                case 128: return new Color32(237, 207, 114, 255);
                case 256: return new Color32(237, 204, 97, 255);
                case 512: return new Color32(237, 200, 80, 255);
                case 1024: return new Color32(237, 197, 63, 255);
                case 2048: return new Color32(237, 194, 46, 255);
                default: return new Color32(60, 58, 50, 255);
            }
        }
    }
}