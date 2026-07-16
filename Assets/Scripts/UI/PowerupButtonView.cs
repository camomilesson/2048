using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace TwentyFortyEight.UI
{
    public sealed class PowerupButtonView : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button button;
        [SerializeField] private GameObject chargeBadgeRoot;
        [SerializeField] private TextMeshProUGUI chargeText;
        [SerializeField] private bool hideBadgeWhenZero;

        [Header("Attention")]
        [SerializeField] private float attentionScale = 1.06f;
        [SerializeField] private float attentionHalfCycle = 0.45f;

        private Coroutine attentionRoutine;
        private Vector3 baseScale;
        private bool attentionActive;

        public Button Button
        {
            get
            {
                return button;
            }
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            baseScale = transform.localScale;
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        public void SetChargeCount(int chargeCount)
        {
            if (chargeCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(chargeCount),
                    "Charge count cannot be negative."
                );
            }

            if (chargeText != null)
            {
                chargeText.text = chargeCount.ToString();
            }

            if (chargeBadgeRoot != null)
            {
                bool shouldShowBadge = !hideBadgeWhenZero || chargeCount > 0;
                chargeBadgeRoot.SetActive(shouldShowBadge);
            }
        }

        public void SetAttention(bool active)
        {
            if (attentionActive == active)
            {
                return;
            }

            attentionActive = active;

            if (attentionRoutine != null)
            {
                StopCoroutine(attentionRoutine);
                attentionRoutine = null;
            }

            transform.localScale =
                baseScale;

            if (active)
            {
                attentionRoutine =
                    StartCoroutine(
                        PlayAttentionPulse()
                    );
            }
        }

        private IEnumerator PlayAttentionPulse()
        {
            while (attentionActive)
            {
                yield return ScaleBetween(
                    baseScale,
                    baseScale * attentionScale
                );

                yield return ScaleBetween(
                    baseScale * attentionScale,
                    baseScale
                );
            }

            transform.localScale =
                baseScale;

            attentionRoutine = null;
        }

        private IEnumerator ScaleBetween(
            Vector3 from,
            Vector3 to
        )
        {
            float duration =
                Mathf.Max(
                    0.01f,
                    attentionHalfCycle
                );

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        elapsed / duration
                    );

                float eased =
                    Mathf.SmoothStep(
                        0f,
                        1f,
                        progress
                    );

                transform.localScale =
                    Vector3.LerpUnclamped(
                        from,
                        to,
                        eased
                    );

                yield return null;
            }

            transform.localScale = to;
        }

        private void OnDisable()
        {
            attentionActive = false;

            if (attentionRoutine != null)
            {
                StopCoroutine(attentionRoutine);
                attentionRoutine = null;
            }

            transform.localScale =
                baseScale;
        }
    }
}