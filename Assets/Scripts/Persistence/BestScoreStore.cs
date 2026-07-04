using System;
using UnityEngine;

namespace TwentyFortyEight.Persistence
{
    public sealed class BestScoreStore
    {
        private const string BestScoreKey = "BestScore";

        public int LoadBestScore()
        {
            int bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            if (bestScore < 0)
            {
                return 0;
            }

            return bestScore;
        }

        public void SaveBestScore(int bestScore)
        {
            if (bestScore < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bestScore),
                    "Best score cannot be negative."
                );
            }

            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }

        public void ResetBestScore()
        {
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
        }
    }
}