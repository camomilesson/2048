using System;
using System.Collections;
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

        public void Initialize(
            CellPosition tilePosition,
            TileData tile
        )
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
            backgroundImage.color =
                GetFallbackBackgroundColor(value);
        }

        private void HandleClick()
        {
            Clicked?.Invoke(position);
        }

        private static Color GetFallbackBackgroundColor(int value)
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

        public IEnumerator PlaySpawnAnimation(float duration)
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            rectTransform.localScale = Vector3.zero;

            if (duration <= 0f)
            {
                rectTransform.localScale = Vector3.one;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / duration);

                float easedProgress =
                    1f - Mathf.Pow(1f - progress, 3f);

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        Vector3.zero,
                        Vector3.one,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.localScale = Vector3.one;
        }

        public IEnumerator PlayMergeAnimation(
            float duration,
            float peakScale
        )
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (duration <= 0f)
            {
                rectTransform.localScale = Vector3.one;
                yield break;
            }

            float halfDuration = duration / 2f;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / halfDuration);

                rectTransform.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.one * peakScale,
                    progress
                );

                yield return null;
            }

            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / halfDuration);

                rectTransform.localScale = Vector3.Lerp(
                    Vector3.one * peakScale,
                    Vector3.one,
                    progress
                );

                yield return null;
            }

            rectTransform.localScale = Vector3.one;
        }

        public IEnumerator PlayKillAnimation(float duration)
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            CanvasGroup canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;

            Vector3 startingScale =
                rectTransform.localScale;

            float startingAlpha =
                canvasGroup.alpha;

            if (duration <= 0f)
            {
                rectTransform.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / duration);

                float easedProgress =
                    Mathf.SmoothStep(0f, 1f, progress);

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        startingScale,
                        Vector3.zero,
                        easedProgress
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        startingAlpha,
                        0f,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }

        public IEnumerator PlayNukeOutAnimation(
            float duration,
            float peakScale
        )
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            CanvasGroup canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;

            Vector3 startingScale =
                rectTransform.localScale;

            float startingAlpha =
                canvasGroup.alpha;

            if (duration <= 0f)
            {
                rectTransform.localScale =
                    Vector3.one * 0.75f;

                canvasGroup.alpha = 0f;

                yield break;
            }

            float pulseDuration =
                duration * 0.4f;

            float fadeDuration =
                duration - pulseDuration;

            float elapsed = 0f;

            while (elapsed < pulseDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / pulseDuration
                    );

                float easedProgress =
                    1f - Mathf.Pow(
                        1f - progress,
                        3f
                    );

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        startingScale,
                        Vector3.one * peakScale,
                        easedProgress
                    );

                yield return null;
            }

            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / fadeDuration
                    );

                float easedProgress =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        progress
                    );

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        Vector3.one * peakScale,
                        Vector3.one * 0.75f,
                        easedProgress
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        startingAlpha,
                        0f,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.localScale =
                Vector3.one * 0.75f;

            canvasGroup.alpha = 0f;
        }

        public IEnumerator PlayNukeInAnimation(
            float duration
        )
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            CanvasGroup canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;

            rectTransform.localScale =
                Vector3.one * 0.75f;

            canvasGroup.alpha = 0f;

            if (duration <= 0f)
            {
                rectTransform.localScale =
                    Vector3.one;

                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;

                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / duration
                    );

                float easedProgress =
                    1f - Mathf.Pow(
                        1f - progress,
                        3f
                    );

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        Vector3.one * 0.75f,
                        Vector3.one,
                        easedProgress
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.localScale =
                Vector3.one;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        public IEnumerator PlayUndoOutAnimation(
            float duration,
            float slideDistance
        )
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            CanvasGroup canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;

            Vector2 startingPosition =
                rectTransform.anchoredPosition;

            Vector2 targetPosition =
                startingPosition +
                new Vector2(slideDistance, 0f);

            Vector3 startingScale =
                rectTransform.localScale;

            float startingAlpha =
                canvasGroup.alpha;

            if (duration <= 0f)
            {
                rectTransform.anchoredPosition =
                    targetPosition;

                rectTransform.localScale =
                    Vector3.one * 0.9f;

                canvasGroup.alpha = 0f;

                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / duration);

                float easedProgress =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        progress
                    );

                rectTransform.anchoredPosition =
                    Vector2.LerpUnclamped(
                        startingPosition,
                        targetPosition,
                        easedProgress
                    );

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        startingScale,
                        Vector3.one * 0.9f,
                        easedProgress
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        startingAlpha,
                        0f,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.anchoredPosition =
                targetPosition;

            rectTransform.localScale =
                Vector3.one * 0.9f;

            canvasGroup.alpha = 0f;
        }

        public IEnumerator PlayUndoInAnimation(
            float duration,
            float slideDistance
        )
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            CanvasGroup canvasGroup =
                GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup =
                    gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;

            Vector2 targetPosition =
                rectTransform.anchoredPosition;

            Vector2 startingPosition =
                targetPosition -
                new Vector2(slideDistance, 0f);

            rectTransform.anchoredPosition =
                startingPosition;

            rectTransform.localScale =
                Vector3.one * 0.9f;

            canvasGroup.alpha = 0f;

            if (duration <= 0f)
            {
                rectTransform.anchoredPosition =
                    targetPosition;

                rectTransform.localScale =
                    Vector3.one;

                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;

                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(elapsed / duration);

                float easedProgress =
                    1f - Mathf.Pow(
                        1f - progress,
                        3f
                    );

                rectTransform.anchoredPosition =
                    Vector2.LerpUnclamped(
                        startingPosition,
                        targetPosition,
                        easedProgress
                    );

                rectTransform.localScale =
                    Vector3.LerpUnclamped(
                        Vector3.one * 0.9f,
                        Vector3.one,
                        easedProgress
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        0f,
                        1f,
                        easedProgress
                    );

                yield return null;
            }

            rectTransform.anchoredPosition =
                targetPosition;

            rectTransform.localScale =
                Vector3.one;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
    }
}