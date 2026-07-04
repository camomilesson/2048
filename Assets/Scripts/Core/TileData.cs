using System;

namespace TwentyFortyEight.Core
{
    public sealed class TileData
    {
        public int Id { get; }

        private int value;

        public int Value
        {
            get => value;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "Tile value must be greater than zero."
                    );
                }

                this.value = value;
            }
        }

        public TileData(int id, int value)
        {
            if (id < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    "Tile ID cannot be negative."
                );
            }

            Id = id;
            Value = value;
        }

        public override string ToString()
        {
            return $"Tile #{Id}: {Value}";
        }
    }
}