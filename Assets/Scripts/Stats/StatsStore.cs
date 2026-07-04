using System;
using UnityEngine;

namespace TwentyFortyEight.Stats
{
    public sealed class StatsStore
    {
        private const string StatsKey = "TwentyFortyEight.Stats";
        private const string LegacyBestScoreKey = "BestScore";

        public StatsData Load()
        {
            string json = PlayerPrefs.GetString(StatsKey, string.Empty);

            StatsData data;

            if (string.IsNullOrWhiteSpace(json))
            {
                data = new StatsData();
                ApplyLegacyBestScore(data);
                return data;
            }

            try
            {
                data = JsonUtility.FromJson<StatsData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Failed to load stats. Creating fresh stats. {exception.Message}"
                );

                data = new StatsData();
            }

            if (data == null)
            {
                data = new StatsData();
            }

            Sanitize(data);
            ApplyLegacyBestScore(data);

            return data;
        }

        public void Save(StatsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Sanitize(data);

            string json = JsonUtility.ToJson(data);

            PlayerPrefs.SetString(StatsKey, json);
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(StatsKey);
            PlayerPrefs.DeleteKey(LegacyBestScoreKey);
            PlayerPrefs.Save();
        }

        private static void ApplyLegacyBestScore(StatsData data)
        {
            if (!PlayerPrefs.HasKey(LegacyBestScoreKey))
            {
                return;
            }

            int legacyBestScore = PlayerPrefs.GetInt(LegacyBestScoreKey, 0);

            if (legacyBestScore > data.BestScore)
            {
                data.BestScore = legacyBestScore;
            }
        }

        private static void Sanitize(StatsData data)
        {
            data.GamesStarted = Math.Max(0, data.GamesStarted);
            data.GamesWon = Math.Max(0, data.GamesWon);
            data.GamesLost = Math.Max(0, data.GamesLost);

            data.BestScore = Math.Max(0, data.BestScore);
            data.HighestTile = Math.Max(0, data.HighestTile);

            data.TotalMoves = Math.Max(0, data.TotalMoves);
            data.TotalMerges = Math.Max(0, data.TotalMerges);

            data.UndoUses = Math.Max(0, data.UndoUses);
            data.KillUses = Math.Max(0, data.KillUses);
            data.NukeUses = Math.Max(0, data.NukeUses);
        }
    }
}