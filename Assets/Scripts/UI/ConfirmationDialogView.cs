using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class ConfirmationDialogView :
        MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        [Header("Buttons")]
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;

        public event Action Confirmed;
        public event Action Cancelled;

        private GameObject Root
        {
            get
            {
                return root != null
                    ? root
                    : gameObject;
            }
        }

        private void OnEnable()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(
                    HandleCancelClicked
                );
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(
                    HandleConfirmClicked
                );
            }
        }

        private void OnDisable()
        {
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(
                    HandleCancelClicked
                );
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(
                    HandleConfirmClicked
                );
            }
        }

        public void Show(
            string title,
            string message,
            string confirmLabel
        )
        {
            Root.SetActive(true);

            // Ensure the dialog renders above every other overlay.
            Root.transform.SetAsLastSibling();

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (confirmButtonText != null)
            {
                confirmButtonText.text = confirmLabel;
            }
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        private void HandleCancelClicked()
        {
            Cancelled?.Invoke();
        }

        private void HandleConfirmClicked()
        {
            Confirmed?.Invoke();
        }
    }
}