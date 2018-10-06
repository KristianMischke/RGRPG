﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    /// <summary>
    ///     Controls the terrain tile game objects and updates their display
    /// </summary>
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
            if (firstUpdate && terrainTile != null)
            {
                SetSprite();

                firstUpdate = false;
            }

            transform.localPosition = position;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        ///     Initializes the sprite associated for this tile. TODO: also initialize walls when we support buildings
        /// </summary>
        public void SetSprite()
        {
            //TODO: also another instance of why we should probably make a SpriteLoader (see comment in CharacterController)
            //TODO: the Sprite loader could have a dictionary of Tile Types (maybe subtypes too?) to image filepaths

            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/tiles");
            Sprite[] tileSprites = Resources.LoadAll<Sprite>("Sprites/sidewalk_tile_draft2");

            Sprite image;
            switch (terrainTile.Type)
            {
                case TerrainType.Grass:
                    image = tileSprites.Single(x => x.name == "sidewalk_tile_draft2_4");
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
            tileSprites = null;
            Resources.UnloadUnusedAssets();
        }
    }
}