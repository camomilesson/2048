using System;

namespace TwentyFortyEight.Core
{
    public sealed class PowerupCharges
    {
        public const int DefaultMaxUndoCharges = 3;
        public const int DefaultMaxKillCharges = 2;
        public const int DefaultMaxNukeCharges = 1;

        private int undoCharges;
        private int killCharges;
        private int nukeCharges;

        public int MaxUndoCharges { get; }
        public int MaxKillCharges { get; }
        public int MaxNukeCharges { get; }

        public int UndoCharges
        {
            get
            {
                return undoCharges;
            }
        }

        public int KillCharges
        {
            get
            {
                return killCharges;
            }
        }

        public int NukeCharges
        {
            get
            {
                return nukeCharges;
            }
        }

        public PowerupCharges(
            int maxUndoCharges = DefaultMaxUndoCharges,
            int maxKillCharges = DefaultMaxKillCharges,
            int maxNukeCharges = DefaultMaxNukeCharges
        )
        {
            ValidateMaxCharges(maxUndoCharges, nameof(maxUndoCharges));
            ValidateMaxCharges(maxKillCharges, nameof(maxKillCharges));
            ValidateMaxCharges(maxNukeCharges, nameof(maxNukeCharges));

            MaxUndoCharges = maxUndoCharges;
            MaxKillCharges = maxKillCharges;
            MaxNukeCharges = maxNukeCharges;

            Reset();
        }

        public void Reset()
        {
            undoCharges = 0;
            killCharges = 0;
            nukeCharges = 0;
        }

        public int GetCharges(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Undo:
                    return undoCharges;

                case PowerupType.Kill:
                    return killCharges;

                case PowerupType.Nuke:
                    return nukeCharges;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown powerup type."
                    );
            }
        }

        public int GetMaxCharges(PowerupType type)
        {
            switch (type)
            {
                case PowerupType.Undo:
                    return MaxUndoCharges;

                case PowerupType.Kill:
                    return MaxKillCharges;

                case PowerupType.Nuke:
                    return MaxNukeCharges;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown powerup type."
                    );
            }
        }

        public bool CanUse(PowerupType type)
        {
            return GetCharges(type) > 0;
        }

        public bool TrySpend(PowerupType type)
        {
            if (!CanUse(type))
            {
                return false;
            }

            switch (type)
            {
                case PowerupType.Undo:
                    undoCharges--;
                    return true;

                case PowerupType.Kill:
                    killCharges--;
                    return true;

                case PowerupType.Nuke:
                    nukeCharges--;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown powerup type."
                    );
            }
        }

        public bool TryAdd(PowerupType type)
        {
            return TryAdd(type, 1);
        }

        public bool TryAdd(PowerupType type, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(amount),
                    "Amount cannot be negative."
                );
            }

            if (amount == 0)
            {
                return true;
            }

            int currentCharges = GetCharges(type);
            int maxCharges = GetMaxCharges(type);

            if (currentCharges >= maxCharges)
            {
                return false;
            }

            int newCharges = Math.Min(currentCharges + amount, maxCharges);

            switch (type)
            {
                case PowerupType.Undo:
                    undoCharges = newCharges;
                    return true;

                case PowerupType.Kill:
                    killCharges = newCharges;
                    return true;

                case PowerupType.Nuke:
                    nukeCharges = newCharges;
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "Unknown powerup type."
                    );
            }
        }

        private static void ValidateMaxCharges(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Maximum charges cannot be negative."
                );
            }
        }

        public PowerupChargeSnapshot CreateSnapshot()
        {
            return new PowerupChargeSnapshot(
                undoCharges,
                killCharges,
                nukeCharges
            );
        }

        public void RestoreSnapshot(PowerupChargeSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            undoCharges = Math.Min(snapshot.UndoCharges, MaxUndoCharges);
            killCharges = Math.Min(snapshot.KillCharges, MaxKillCharges);
            nukeCharges = Math.Min(snapshot.NukeCharges, MaxNukeCharges);
        }
    }
}