using System;
using TMPro;
using UnityEngine;

namespace TwentyFortyEight.UI
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreValueText;
        [SerializeField] private TextMeshProUGUI bestScoreValueText;

        public void SetScore(int score)
        {
            if (score < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(score),
                    "Score cannot be negative."
                );
            }

            SetText(scoreValueText, score);
        }

        public void SetBestScore(int bestScore)
        {
            if (bestScore < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bestScore),
                    "Best score cannot be negative."
                );
            }

            SetText(bestScoreValueText, bestScore);
        }

        public void SetScores(int score, int bestScore)
        {
            SetScore(score);
            SetBestScore(bestScore);
        }

        private static void SetText(TextMeshProUGUI text, int value)
        {
            if (text == null)
            {
                throw new InvalidOperationException(
                    "ScoreView is missing a TextMeshProUGUI reference."
                );
            }

            text.text = value.ToString();
        }
    }
}