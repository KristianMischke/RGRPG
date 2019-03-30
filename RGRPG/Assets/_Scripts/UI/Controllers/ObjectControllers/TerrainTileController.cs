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
        public const int WALL_HEIGHT = 20;
        public virtual int GetWallHeight { get { return WALL_HEIGHT; } }

        // Scene Object References
        public SpriteRenderer spriteRenderer; //actual tile
        public SpriteRenderer overlayRenderer;
        public SpriteRenderer propRenderer;

        // Prefabs

        // Data
        public Vector2Int tilePosition;
        public WorldScene sceneReference;

        protected TerrainTile prevTileReference; // used for checking if the tile changed and needs to undergo a visual update

        private List<SpriteRenderer> wallSprites = new List<SpriteRenderer>();

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
                if (sceneTile != null)
                {
                    if (prevTileReference == null || !prevTileReference.Equals(sceneTile))
                    {
                        // the last referece we got is not equal to the current one
                        prevTileReference = sceneTile;
                        SetSprite();
                    }
                }
            }

            transform.localPosition = new Vector3(tilePosition.x, tilePosition.y);
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        ///     Initializes the sprite associated for this tile. TODO: also initialize walls when we support buildings
        /// </summary>
        public virtual void SetSprite()
        {
            spriteRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.Type, prevTileReference.SubType);

            overlayRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.OverlayType, prevTileReference.OverlaySubType);
            overlayRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.OverlayType) && !prevTileReference.OverlayType.Contains("NONE"));

            propRenderer.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.PropType);
            propRenderer.gameObject.SetActive(!string.IsNullOrEmpty(prevTileReference.PropType) && !prevTileReference.PropType.Contains("NONE"));
            propRenderer.GetComponentInParent<PointAtCamera>().ShouldPointAtCamera = !prevTileReference.IsWall;

            if (prevTileReference.IsWall)
            {
                propRenderer.transform.parent.localPosition = new Vector3(-0.55f, 0, 0);
                propRenderer.transform.parent.localRotation = Quaternion.Euler(0, 90, -90);
            }

            if (prevTileReference.IsWall || wallSprites.Count > 0)
            {
                HandleWalls(prevTileReference.IsWall);
            }
        }

        private void HandleWalls(bool isWall)
        {
            bool createSprites = false;
            if (wallSprites.Count == 0)
            {
                createSprites = true;
            }

            for (int i = 0; i < GetWallHeight; i++)
            {
                // FRONT LEFT WALLS
                {
                    SpriteRenderer wallSprite;
                    if (createSprites)
                    {
                        GameObject g = Instantiate(spriteRenderer.gameObject, transform);
                        g.transform.SetSiblingIndex(i * 2);
                        g.transform.localPosition = new Vector3(-0.5f, 0, -0.5f - i);
                        g.transform.localRotation = Quaternion.Euler(0, 90, -90);
                        wallSprite = g.GetComponent<SpriteRenderer>();
                    }
                    else
                    {
                        wallSprite = wallSprites[i * 2];
                    }
                    wallSprite.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.Type, prevTileReference.SubType);
                    wallSprite.sortingOrder = 2;
                    wallSprite.sortingLayerName = "Default";
                    wallSprite.gameObject.SetActive(isWall);

                    if (createSprites)
                        wallSprites.Add(wallSprite);
                }

                // FRONT RIGHT WALLS
                {
                    SpriteRenderer wallSprite;
                    if (createSprites)
                    {
                        GameObject g = Instantiate(spriteRenderer.gameObject, transform);
                        g.transform.SetSiblingIndex(i * 2 + 1);
                        g.transform.localPosition = new Vector3(0, -0.5f, -0.5f - i);
                        g.transform.localRotation = Quaternion.Euler(90, 0, 0);
                        wallSprite = g.GetComponent<SpriteRenderer>();
                    }
                    else
                    {
                        wallSprite = wallSprites[i * 2 + 1];
                    }

                    wallSprite.sprite = SpriteManager.getSprite(SpriteManager.AssetType.TERRAIN, prevTileReference.Type, prevTileReference.SubType);
                    wallSprite.sortingOrder = 2;
                    wallSprite.sortingLayerName = "Default";
                    wallSprite.gameObject.SetActive(isWall);

                    if (createSprites)
                        wallSprites.Add(wallSprite);
                }
            }
        }
    }
}