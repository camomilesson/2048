using System;
using UnityEngine;

namespace TwentyFortyEight.Settings
{
    public sealed class GameSettingsStore
    {
        private const string SettingsKey =
            "TwentyFortyEight.Settings";

        public GameSettingsData Load()
        {
            string json = PlayerPrefs.GetString(
                SettingsKey,
                string.Empty
            );

            if (string.IsNullOrWhiteSpace(json))
            {
                return new GameSettingsData();
            }

            GameSettingsData data;

            try
            {
                data =
                    JsonUtility.FromJson<GameSettingsData>(
                        json
                    );
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "Failed to load settings. " +
                    $"Using defaults. {exception.Message}"
                );

                data = new GameSettingsData();
            }

            if (data == null)
            {
                data = new GameSettingsData();
            }

            Sanitize(data);

            return data;
        }

        public void Save(GameSettingsData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(
                    nameof(data)
                );
            }

            Sanitize(data);

            string json =
                JsonUtility.ToJson(data);

            PlayerPrefs.SetString(
                SettingsKey,
                json
            );

            PlayerPrefs.Save();
        }

        private static void Sanitize(
            GameSettingsData data
        )
        {
            data.MusicVolume =
                Mathf.Clamp01(data.MusicVolume);

            data.SfxVolume =
                Mathf.Clamp01(data.SfxVolume);
        }
    }
}