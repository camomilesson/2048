using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class UiVfxController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image flashImage;
        [SerializeField] private RectTransform boardContainer;

        [Header("Flash")]
        [SerializeField] private float flashInDuration = 0.04f;
        [SerializeField] private float flashOutDuration = 0.25f;
        [SerializeField, Range(0f, 1f)] private float maximumFlashAlpha = 0.65f;

        [Header("Board Wobble")]
        [SerializeField] private float wobbleDuration = 0.3f;
        [SerializeField] private float maximumRotation = 3f;
        [SerializeField] private float maximumScale = 1.025f;
        [SerializeField] private float wobbleFrequency = 35f;

        private Coroutine activeEffect;

        public void PlayNukeVfx()
        {
            if (activeEffect != null)
            {
                StopCoroutine(activeEffect);
                RestoreDefaults();
            }

            activeEffect = StartCoroutine(NukeVfxRoutine());
        }

        private IEnumerator NukeVfxRoutine()
        {
            Quaternion originalRotation = boardContainer.localRotation;
            Vector3 originalScale = boardContainer.localScale;

            yield return FadeFlash(0f, maximumFlashAlpha, flashInDuration);

            float elapsed = 0f;

            while (elapsed < wobbleDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = Mathf.Clamp01(elapsed / wobbleDuration);
                float strength = 1f - progress;

                float angle =
                    Mathf.Sin(elapsed * wobbleFrequency) *
                    maximumRotation *
                    strength;

                float scaleAmount =
                    Mathf.Lerp(maximumScale, 1f, progress);

                boardContainer.localRotation =
                    originalRotation * Quaternion.Euler(0f, 0f, angle);

                boardContainer.localScale =
                    originalScale * scaleAmount;

                yield return null;
            }

            boardContainer.localRotation = originalRotation;
            boardContainer.localScale = originalScale;

            yield return FadeFlash(
                maximumFlashAlpha,
                0f,
                flashOutDuration
            );

            activeEffect = null;
        }

        private IEnumerator FadeFlash(
            float startAlpha,
            float endAlpha,
            float duration
        )
        {
            Color color = flashImage.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress =
                    duration <= 0f
                        ? 1f
                        : Mathf.Clamp01(elapsed / duration);

                color.a = Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    progress
                );

                flashImage.color = color;

                yield return null;
            }

            color.a = endAlpha;
            flashImage.color = color;
        }

        private void RestoreDefaults()
        {
            if (flashImage != null)
            {
                Color color = flashImage.color;
                color.a = 0f;
                flashImage.color = color;
            }

            if (boardContainer != null)
            {
                boardContainer.localRotation = Quaternion.identity;
                boardContainer.localScale = Vector3.one;
            }
        }

        private void OnDisable()
        {
            RestoreDefaults();
        }
    }
}