using System;

namespace TwentyFortyEight.Stats
{
    [Serializable]
    public sealed class StatsData
    {
        public int GamesStarted;
        public int GamesWon;
        public int GamesLost;

        public int BestScore;
        public int HighestTile;

        public int TotalMoves;
        public int TotalMerges;

        public int UndoUses;
        public int KillUses;
        public int NukeUses;

        public void Reset()
        {
            GamesStarted = 0;
            GamesWon = 0;
            GamesLost = 0;

            BestScore = 0;
            HighestTile = 0;

            TotalMoves = 0;
            TotalMerges = 0;

            UndoUses = 0;
            KillUses = 0;
            NukeUses = 0;
        }
    }
}