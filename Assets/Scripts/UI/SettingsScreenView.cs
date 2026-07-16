using System;
using TMPro;
using TwentyFortyEight.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class SettingsScreenView :
        MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [Header("Audio")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [SerializeField]
        private TextMeshProUGUI musicValueText;

        [SerializeField]
        private TextMeshProUGUI sfxValueText;

        [Header("Buttons")]
        [SerializeField] private Button clearStatsButton;
        [SerializeField] private Button backButton;

        public event Action<float> MusicVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action ClearStatsRequested;
        public event Action BackClicked;

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
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(
                    HandleMusicVolumeChanged
                );
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(
                    HandleSfxVolumeChanged
                );
            }

            if (clearStatsButton != null)
            {
                clearStatsButton.onClick.AddListener(
                    HandleClearStatsClicked
                );
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(
                    HandleBackClicked
                );
            }
        }

        private void OnDisable()
        {
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveListener(
                    HandleMusicVolumeChanged
                );
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveListener(
                    HandleSfxVolumeChanged
                );
            }

            if (clearStatsButton != null)
            {
                clearStatsButton.onClick.RemoveListener(
                    HandleClearStatsClicked
                );
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveListener(
                    HandleBackClicked
                );
            }
        }

        public void Show(GameSettingsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data)
                );
            }

            Root.SetActive(true);

            if (musicSlider != null)
            {
                musicSlider.SetValueWithoutNotify(
                    data.MusicVolume
                );
            }

            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(
                    data.SfxVolume
                );
            }

            UpdateMusicValueText(
                data.MusicVolume
            );

            UpdateSfxValueText(
                data.SfxVolume
            );
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        public void SetClearStatsInteractable(
            bool interactable
        )
        {
            if (clearStatsButton != null)
            {
                clearStatsButton.interactable =
                    interactable;
            }
        }

        private void HandleMusicVolumeChanged(
            float value
        )
        {
            UpdateMusicValueText(value);

            MusicVolumeChanged?.Invoke(value);
        }

        private void HandleSfxVolumeChanged(
            float value
        )
        {
            UpdateSfxValueText(value);

            SfxVolumeChanged?.Invoke(value);
        }

        private void HandleClearStatsClicked()
        {
            ClearStatsRequested?.Invoke();
        }

        private void HandleBackClicked()
        {
            BackClicked?.Invoke();
        }

        private void UpdateMusicValueText(
            float value
        )
        {
            if (musicValueText != null)
            {
                musicValueText.text =
                    FormatPercentage(value);
            }
        }

        private void UpdateSfxValueText(
            float value
        )
        {
            if (sfxValueText != null)
            {
                sfxValueText.text =
                    FormatPercentage(value);
            }
        }

        private static string FormatPercentage(
            float value
        )
        {
            int percentage =
                Mathf.RoundToInt(
                    Mathf.Clamp01(value) * 100f
                );

            return $"{percentage}%";
        }
    }
}