using System;
using UnityEngine;

namespace TwentyFortyEight.UI
{
    [CreateAssetMenu(
        fileName = "TileVisualLibrary",
        menuName = "TwentyFortyEight/Tile Visual Library"
    )]
    public sealed class TileVisualLibrary : ScriptableObject
    {
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private TileSpriteMapping[] spriteMappings;

        public Sprite GetSpriteForValue(int value)
        {
            if (spriteMappings != null)
            {
                for (int i = 0; i < spriteMappings.Length; i++)
                {
                    TileSpriteMapping mapping = spriteMappings[i];

                    if (mapping.Value == value)
                    {
                        return mapping.Sprite != null
                            ? mapping.Sprite
                            : fallbackSprite;
                    }
                }
            }

            return fallbackSprite;
        }

        private void OnValidate()
        {
            if (spriteMappings == null)
            {
                return;
            }

            for (int i = 0; i < spriteMappings.Length; i++)
            {
                spriteMappings[i].Validate();
            }
        }

        [Serializable]
        private sealed class TileSpriteMapping
        {
            [SerializeField] private int value;
            [SerializeField] private Sprite sprite;

            public int Value
            {
                get
                {
                    return value;
                }
            }

            public Sprite Sprite
            {
                get
                {
                    return sprite;
                }
            }

            public void Validate()
            {
                if (value < 2)
                {
                    value = 2;
                }
            }
        }
    }
}