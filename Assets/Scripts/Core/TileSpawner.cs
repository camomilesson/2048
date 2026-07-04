using System;
using System.Collections.Generic;

namespace TwentyFortyEight.Core
{
    public sealed class TileSpawner
    {
        public const int DefaultSmallTileValue = 2;
        public const int DefaultLargeTileValue = 4;
        public const double DefaultLargeTileChance = 0.1;
        public const int DefaultInitialTileCount = 2;

        private readonly Random random;

        public int SmallTileValue { get; }
        public int LargeTileValue { get; }
        public double LargeTileChance { get; }

        public TileSpawner(
            Random random = null,
            int smallTileValue = DefaultSmallTileValue,
            int largeTileValue = DefaultLargeTileValue,
            double largeTileChance = DefaultLargeTileChance
        )
        {
            ValidateTileValue(smallTileValue, nameof(smallTileValue));
            ValidateTileValue(largeTileValue, nameof(largeTileValue));

            if (largeTileChance < 0 || largeTileChance > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(largeTileChance),
                    "Large tile chance must be between 0 and 1."
                );
            }

            this.random = random ?? new Random();

            SmallTileValue = smallTileValue;
            LargeTileValue = largeTileValue;
            LargeTileChance = largeTileChance;
        }

        public TileSpawnResult SpawnRandomTile(BoardModel board)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            List<CellPosition> emptyPositions = board.GetEmptyPositions();

            if (emptyPositions.Count == 0)
            {
                return TileSpawnResult.None();
            }

            int randomIndex = random.Next(emptyPositions.Count);
            CellPosition position = emptyPositions[randomIndex];

            int value = ChooseTileValue();

            TileData tile = board.SpawnTile(position, value);

            return TileSpawnResult.Success(position, tile);
        }

        public int SpawnInitialTiles(BoardModel board)
        {
            return SpawnInitialTiles(board, DefaultInitialTileCount);
        }

        public int SpawnInitialTiles(BoardModel board, int count)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Initial tile count cannot be negative."
                );
            }

            int spawnedCount = 0;

            for (int i = 0; i < count; i++)
            {
                TileSpawnResult result = SpawnRandomTile(board);

                if (!result.Spawned)
                {
                    break;
                }

                spawnedCount++;
            }

            return spawnedCount;
        }

        private int ChooseTileValue()
        {
            double roll = random.NextDouble();

            if (roll < LargeTileChance)
            {
                return LargeTileValue;
            }

            return SmallTileValue;
        }

        private static void ValidateTileValue(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Tile value must be greater than zero."
                );
            }

            if (!IsPowerOfTwo(value))
            {
                throw new ArgumentException(
                    "Tile value must be a power of two.",
                    parameterName
                );
            }
        }

        private static bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }
    }
}