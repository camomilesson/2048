using System;
using System.Text;
using TMPro;
using TwentyFortyEight.Stats;
using UnityEngine;
using UnityEngine.UI;

namespace TwentyFortyEight.UI
{
    public sealed class StatsScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI statsBodyText;
        [SerializeField] private Button backButton;

        public event Action BackClicked;

        private GameObject Root
        {
            get
            {
                return root != null ? root : gameObject;
            }
        }

        private void OnEnable()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }
        }

        private void OnDisable()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        public void Show(StatsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Root.SetActive(true);

            if (statsBodyText != null)
            {
                statsBodyText.text = BuildStatsText(data);
            }
        }

        public void Hide()
        {
            Root.SetActive(false);
        }

        private void HandleBackClicked()
        {
            BackClicked?.Invoke();
        }

        private static string BuildStatsText(StatsData data)
        {
            int finishedGames = data.GamesWon + data.GamesLost;

            string winRate = finishedGames == 0
                ? "—"
                : $"{data.GamesWon * 100f / finishedGames:0.#}%";

            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Games started: {data.GamesStarted}");
            builder.AppendLine($"Games won: {data.GamesWon}");
            builder.AppendLine($"Games lost: {data.GamesLost}");
            builder.AppendLine($"Win rate: {winRate}");
            builder.AppendLine();
            builder.AppendLine($"Best score: {data.BestScore}");
            builder.AppendLine($"Highest tile: {data.HighestTile}");
            builder.AppendLine();
            builder.AppendLine($"Total moves: {data.TotalMoves}");
            builder.AppendLine($"Total merges: {data.TotalMerges}");
            builder.AppendLine();
            builder.AppendLine($"Undo uses: {data.UndoUses}");
            builder.AppendLine($"Kill uses: {data.KillUses}");
            builder.AppendLine($"Nuke uses: {data.NukeUses}");

            return builder.ToString();
        }
    }
}