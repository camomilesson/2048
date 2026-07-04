using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class PowerupButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject chargeBadgeRoot;
        [SerializeField] private TextMeshProUGUI chargeText;
        [SerializeField] private bool hideBadgeWhenZero;

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
    }
}