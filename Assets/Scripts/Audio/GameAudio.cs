using UnityEngine;
using UnityEngine.Audio;

namespace TwentyFortyEight.Audio
{
    public sealed class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        private const string MusicVolumeParameter =
            "MusicVolume";

        private const string SfxVolumeParameter =
            "SfxVolume";

        private const float MinimumDecibels = -80f;

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip[] swipeClips;
        private int lastSwipeIndex = -1;
        [SerializeField] private AudioClip mergeClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        [Header("Clip Volumes")]
        [SerializeField, Range(0f, 1f)]
        private float buttonClickVolume = 0.65f;

        [SerializeField, Range(0f, 1f)]
        private float swipeVolume = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float mergeVolume = 0.8f;

        [SerializeField, Range(0f, 1f)]
        private float winVolume = 0.9f;

        [SerializeField, Range(0f, 1f)]
        private float loseVolume = 0.9f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlayButtonClick()
        {
            PlaySfx(
                buttonClickClip,
                buttonClickVolume
            );
        }

        public void PlaySwipe()
        {
            if (
                swipeClips == null ||
                swipeClips.Length == 0
            )
            {
                return;
            }

            int selectedIndex;

            if (swipeClips.Length == 1)
            {
                selectedIndex = 0;
            }
            else
            {
                do
                {
                    selectedIndex =
                        UnityEngine.Random.Range(
                            0,
                            swipeClips.Length
                        );
                }
                while (selectedIndex == lastSwipeIndex);
            }

            lastSwipeIndex = selectedIndex;

            PlaySfx(
                swipeClips[selectedIndex],
                swipeVolume
            );
        }

        public void PlayMerge()
        {
            PlaySfx(
                mergeClip,
                mergeVolume
            );
        }

        public void PlayWin()
        {
            PlaySfx(
                winClip,
                winVolume
            );
        }

        public void PlayLose()
        {
            PlaySfx(
                loseClip,
                loseVolume
            );
        }

        public void SetMusicVolume(float linearVolume)
        {
            SetMixerVolume(
                MusicVolumeParameter,
                linearVolume
            );
        }

        public void SetSfxVolume(float linearVolume)
        {
            SetMixerVolume(
                SfxVolumeParameter,
                linearVolume
            );
        }

        private void PlaySfx(
            AudioClip clip,
            float volumeScale
        )
        {
            if (sfxSource == null || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(
                clip,
                volumeScale
            );
        }

        private void SetMixerVolume(
            string parameterName,
            float linearVolume
        )
        {
            if (audioMixer == null)
            {
                return;
            }

            float clampedVolume =
                Mathf.Clamp01(linearVolume);

            float decibels =
                clampedVolume <= 0.0001f
                    ? MinimumDecibels
                    : Mathf.Log10(clampedVolume) * 20f;

            audioMixer.SetFloat(
                parameterName,
                decibels
            );
        }
    }
}