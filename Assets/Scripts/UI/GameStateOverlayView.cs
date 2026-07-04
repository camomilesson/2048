using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class GameStateOverlayView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;

        public event Action ContinueClicked;
        public event Action NewGameClicked;

        private void Awake()
        {
            if (root == null)
            {
                root = gameObject;
            }
        }

        private void OnEnable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(HandleNewGameClicked);
            }
        }

        private void OnDisable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }

            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(HandleNewGameClicked);
            }
        }

        public void ShowWin()
        {
            Show(
                title: "You reached 2048!",
                message: "Nice. You can continue playing or start a new game.",
                showContinueButton: true
            );
        }

        public void ShowGameOver()
        {
            Show(
                title: "Game Over",
                message: "No moves remain.",
                showContinueButton: false
            );
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void Show(
            string title,
            string message,
            bool showContinueButton
        )
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(showContinueButton);
            }
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void HandleNewGameClicked()
        {
            NewGameClicked?.Invoke();
        }
    }
}