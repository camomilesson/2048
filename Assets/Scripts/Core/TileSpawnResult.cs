using System;

namespace TwentyFortyEight.Core
{
    public sealed class TileSpawnResult
    {
        public bool Spawned { get; }
        public CellPosition Position { get; }
        public TileData Tile { get; }

        private TileSpawnResult(bool spawned, CellPosition position, TileData tile)
        {
            Spawned = spawned;
            Position = position;
            Tile = tile;
        }

        public static TileSpawnResult None()
        {
            return new TileSpawnResult(false, new CellPosition(0, 0), null);
        }

        public static TileSpawnResult Success(CellPosition position, TileData tile)
        {
            if (tile == null)
            {
                throw new ArgumentNullException(nameof(tile));
            }

            return new TileSpawnResult(true, position, tile);
        }

        public override string ToString()
        {
            if (!Spawned)
            {
                return "No tile spawned.";
            }

            return $"Spawned tile {Tile.Value} at {Position}.";
        }
    }
}