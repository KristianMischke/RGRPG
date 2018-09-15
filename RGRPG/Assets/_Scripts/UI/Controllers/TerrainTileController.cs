using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    public class TerrainTileController : MonoBehaviour
    {

        // Scene Object References

        public SpriteRenderer spriteRenderer;

        // Prefabs

        // Data
        public TerrainTile terrainTile;
        public Vector2 position;

        bool firstUpdate = true;

        // Use this for initialization
        void Start()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (firstUpdate)
            {
                SetSprite();

                firstUpdate = false;
            }

            transform.position = position;
        }

        public void SetSprite()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/tiles");

            Sprite image;
            switch (terrainTile.Type)
            {
                case TerrainType.Grass:
                    image = sprites.Single(x => x.name == "grass_0");
                    break;
                case TerrainType.Rock:
                    image = sprites.Single(x => x.name == "dirt_0");
                    break;
                default:
                    image = Resources.Load<Sprite>("Sprites/troll");
                    break;
            }

            if (!terrainTile.Traversable)
            {
                image = Resources.Load<Sprite>("Sprites/stone");
            }

            spriteRenderer.sprite = image;
            spriteRenderer.transform.localScale = new Vector2(1 / image.bounds.size.x, 1 / image.bounds.size.y);

            sprites = null;
            Resources.UnloadUnusedAssets();
        }
    }
}