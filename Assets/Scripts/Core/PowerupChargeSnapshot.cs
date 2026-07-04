using System;

namespace TwentyFortyEight.Core
{
    public sealed class PowerupChargeSnapshot
    {
        public int UndoCharges { get; }
        public int KillCharges { get; }
        public int NukeCharges { get; }

        public PowerupChargeSnapshot(
            int undoCharges,
            int killCharges,
            int nukeCharges
        )
        {
            ValidateChargeCount(undoCharges, nameof(undoCharges));
            ValidateChargeCount(killCharges, nameof(killCharges));
            ValidateChargeCount(nukeCharges, nameof(nukeCharges));

            UndoCharges = undoCharges;
            KillCharges = killCharges;
            NukeCharges = nukeCharges;
        }

        private static void ValidateChargeCount(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Charge count cannot be negative."
                );
            }
        }
    }
}