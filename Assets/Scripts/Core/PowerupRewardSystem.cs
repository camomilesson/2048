using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class PowerupRewardSystem
    {
        public const int UndoRewardTileValue = 1024;
        public const int KillRewardTileValue = 2048;
        public const int NukeRewardTileValue = 4096;

        public IReadOnlyList<PowerupType> GrantRewardsForMergeValues(
            IReadOnlyList<int> createdMergeValues,
            PowerupCharges powerupCharges
        )
        {
            if (createdMergeValues == null)
            {
                throw new ArgumentNullException(nameof(createdMergeValues));
            }

            if (powerupCharges == null)
            {
                throw new ArgumentNullException(nameof(powerupCharges));
            }

            List<PowerupType> earnedPowerups = new List<PowerupType>();

            for (int i = 0; i < createdMergeValues.Count; i++)
            {
                int createdValue = createdMergeValues[i];

                if (!TryGetRewardForCreatedValue(createdValue, out PowerupType powerupType))
                {
                    continue;
                }

                bool added = powerupCharges.TryAdd(powerupType);

                if (added)
                {
                    earnedPowerups.Add(powerupType);
                }
            }

            return earnedPowerups;
        }

        private static bool TryGetRewardForCreatedValue(
            int createdValue,
            out PowerupType powerupType
        )
        {
            switch (createdValue)
            {
                case UndoRewardTileValue:
                    powerupType = PowerupType.Undo;
                    return true;

                case KillRewardTileValue:
                    powerupType = PowerupType.Kill;
                    return true;

                case NukeRewardTileValue:
                    powerupType = PowerupType.Nuke;
                    return true;

                default:
                    powerupType = default;
                    return false;
            }
        }
    }
}