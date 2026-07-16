using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public enum BannerMessageType
    {
        Default,
        SwipeToMerge,
        UsePowerup,
        ChooseTile
    }

    public sealed class BannerMessageView :
        MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private Image currentBanner;
        [SerializeField] private Image incomingBanner;

        [Header("Sprites")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite swipeToMergeSprite;
        [SerializeField] private Sprite usePowerupSprite;
        [SerializeField] private Sprite chooseTileSprite;

        [Header("Animation")]
        [SerializeField] private float transitionDuration = 0.28f;
        [SerializeField] private float travelPadding = 40f;

        private BannerMessageType currentType =
            BannerMessageType.Default;

        private BannerMessageType queuedType;
        private bool hasQueuedType;
        private Coroutine transitionRoutine;

        private void Awake()
        {
            if (viewport == null)
            {
                viewport =
                    transform as RectTransform;
            }

            SetImmediate(
                BannerMessageType.Default
            );
        }

        public void Show(
            BannerMessageType type,
            bool immediate = false
        )
        {
            if (immediate)
            {
                if (transitionRoutine != null)
                {
                    StopCoroutine(
                        transitionRoutine
                    );

                    transitionRoutine = null;
                }

                hasQueuedType = false;
                SetImmediate(type);
                return;
            }

            if (
                transitionRoutine == null &&
                type == currentType
            )
            {
                return;
            }

            if (transitionRoutine != null)
            {
                queuedType = type;
                hasQueuedType = true;
                return;
            }

            transitionRoutine =
                StartCoroutine(
                    TransitionTo(type)
                );
        }

        private IEnumerator TransitionTo(
            BannerMessageType nextType
        )
        {
            if (
                currentBanner == null ||
                incomingBanner == null
            )
            {
                SetImmediate(nextType);
                transitionRoutine = null;
                yield break;
            }

            Canvas.ForceUpdateCanvases();

            float travelDistance =
                GetTravelDistance();

            RectTransform currentRect =
                currentBanner.rectTransform;

            RectTransform incomingRect =
                incomingBanner.rectTransform;

            currentRect.anchoredPosition =
                Vector2.zero;

            incomingRect.anchoredPosition =
                new Vector2(
                    travelDistance,
                    0f
                );

            incomingBanner.sprite =
                GetSprite(nextType);

            incomingBanner.gameObject.SetActive(
                true
            );

            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float progress =
                    transitionDuration <= 0f
                        ? 1f
                        : Mathf.Clamp01(
                            elapsed /
                            transitionDuration
                        );

                float eased =
                    1f - Mathf.Pow(
                        1f - progress,
                        3f
                    );

                /*
                 * Old banner flies left.
                 */
                currentRect.anchoredPosition =
                    Vector2.LerpUnclamped(
                        Vector2.zero,
                        new Vector2(
                            -travelDistance,
                            0f
                        ),
                        eased
                    );

                /*
                 * New banner flies in from the right.
                 */
                incomingRect.anchoredPosition =
                    Vector2.LerpUnclamped(
                        new Vector2(
                            travelDistance,
                            0f
                        ),
                        Vector2.zero,
                        eased
                    );

                yield return null;
            }

            currentRect.anchoredPosition =
                new Vector2(
                    -travelDistance,
                    0f
                );

            incomingRect.anchoredPosition =
                Vector2.zero;

            Image previousCurrent =
                currentBanner;

            currentBanner =
                incomingBanner;

            incomingBanner =
                previousCurrent;

            incomingBanner.gameObject.SetActive(
                false
            );

            incomingBanner.rectTransform
                .anchoredPosition =
                new Vector2(
                    travelDistance,
                    0f
                );

            currentType = nextType;
            transitionRoutine = null;

            if (hasQueuedType)
            {
                BannerMessageType pending =
                    queuedType;

                hasQueuedType = false;

                if (pending != currentType)
                {
                    Show(pending);
                }
            }
        }

        private void SetImmediate(
            BannerMessageType type
        )
        {
            if (currentBanner == null)
            {
                return;
            }

            currentBanner.sprite =
                GetSprite(type);

            currentBanner.gameObject.SetActive(
                true
            );

            currentBanner.rectTransform
                .anchoredPosition =
                Vector2.zero;

            if (incomingBanner != null)
            {
                incomingBanner.gameObject.SetActive(
                    false
                );

                incomingBanner.rectTransform
                    .anchoredPosition =
                    new Vector2(
                        GetTravelDistance(),
                        0f
                    );
            }

            currentType = type;
        }

        private Sprite GetSprite(
            BannerMessageType type
        )
        {
            switch (type)
            {
                case BannerMessageType.Default:
                    return defaultSprite;

                case BannerMessageType.SwipeToMerge:
                    return swipeToMergeSprite != null
                        ? swipeToMergeSprite
                        : defaultSprite;

                case BannerMessageType.UsePowerup:
                    return usePowerupSprite != null
                        ? usePowerupSprite
                        : defaultSprite;

                case BannerMessageType.ChooseTile:
                    return chooseTileSprite != null
                        ? chooseTileSprite
                        : defaultSprite;

                default:
                    return defaultSprite;
            }
        }

        private float GetTravelDistance()
        {
            if (viewport == null)
            {
                return 1000f;
            }

            return
                viewport.rect.width +
                travelPadding;
        }

        private void OnDisable()
        {
            if (transitionRoutine != null)
            {
                StopCoroutine(
                    transitionRoutine
                );

                transitionRoutine = null;
            }

            hasQueuedType = false;
        }
    }
}