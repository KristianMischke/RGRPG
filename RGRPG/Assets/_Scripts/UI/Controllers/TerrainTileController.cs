using System.Collections;
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

        public SpriteRenderer spriteRenderer; //actual tile
        public SpriteRenderer borderRenderer; //traversibley/n
        public SpriteRenderer spawnRenderer; //spawn square
        public SpriteRenderer overlayRenderer;
        // Prefabs

        // Data
        public Vector2Int tilePosition;
        public WorldScene sceneReference;

        private TerrainTile prevTileReference; // used for checking if the tile changed and needs to undergo a visual update

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {            
            if (sceneReference != null)
            {
                TerrainTile sceneTile = sceneReference.GetTileAtIndices(tilePosition);
                if (prevTileReference == null || !prevTileReference.Equals(sceneTile))
                {
                    // the last referece we got is not equal to the current one
                    prevTileReference = sceneTile;
                    SetSprite();
                }
            }

            transform.localPosition = new Vector3(tilePosition.x, tilePosition.y);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        ///     Initializes the sprite associated for this tile. TODO: also initialize walls when we support buildings
        /// </summary>
        public void SetSprite()
        {
            spriteRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.Type, prevTileReference.SubType);
            overlayRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.OverlayType);
            overlayRenderer.gameObject.SetActive(prevTileReference.OverlayType != "TERRAIN_NONE");
            borderRenderer.gameObject.SetActive(!prevTileReference.Traversable && MapEditorController.instance != null);
            spawnRenderer.gameObject.SetActive(prevTileReference.IsSpawn && MapEditorController.instance != null);
        }
    }
}