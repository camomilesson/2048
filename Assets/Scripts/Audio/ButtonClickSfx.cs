using TwentyFortyEight.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class ButtonClickSfx : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            button.onClick.AddListener(
                PlayClickSound
            );
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(
                PlayClickSound
            );
        }

        private void PlayClickSound()
        {
            GameAudio.Instance?.PlayButtonClick();
        }
    }
}