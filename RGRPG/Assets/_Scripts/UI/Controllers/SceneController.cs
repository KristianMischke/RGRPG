using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    public class SceneController : MonoBehaviour
    {
        // Scene Object References
        public GameObject terrainContainer;

        // Prefabs
        public GameObject terrainTileView;

        // Data
        public WorldScene scene;

        List<List<TerrainTileController>> activeTerrainTiles;
        Pool<TerrainTileController> terrainTilePool;

        private void OnEnable()
        {
            activeTerrainTiles = new List<List<TerrainTileController>>();

            Action<TerrainTileController> activateTile = (TerrainTileController t) => {
                t.gameObject.SetActive(true);
            };
            Action<TerrainTileController> deactivateTile = (TerrainTileController t) => {
                t.gameObject.SetActive(false);
            };
            Func<TerrainTileController> newTile = () => {
                GameObject tileObject = Instantiate(terrainTileView);
                tileObject.transform.SetParent(terrainContainer.transform);

                TerrainTileController tileController = tileObject.GetComponent<TerrainTileController>();
                return tileController;
            };

            terrainTilePool = new Pool<TerrainTileController>(activateTile, deactivateTile, newTile);
        }

        // Use this for initialization
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        public void ResetScene()
        {
            if (scene == null)
                throw new System.Exception("Cannot Reset a NULL Scene");

            for (int x = 0; x < scene.Width; x++)
            {
                for (int y = 0; y < scene.Height; y++)
                {
                    TerrainTile tile = scene.TerrainTiles[x, y];

                    if (x >= activeTerrainTiles.Count)
                    {
                        activeTerrainTiles.Add(new List<TerrainTileController>());
                    }

                    if (y >= activeTerrainTiles[x].Count)
                    {
                        activeTerrainTiles[x].Add(terrainTilePool.Get()); // get new tile from the pool
                    }

                    TerrainTileController tileController = activeTerrainTiles[x][y];
                    tileController.tilePosition = new Vector2Int(x, y);
                    tileController.sceneReference = scene;
                }
            }


            // add unused objects to the pool
            for (int i = activeTerrainTiles.Count - 1; i >= 0; i--)
            {
                while (activeTerrainTiles[i].Count > scene.Height)
                {
                    terrainTilePool.Deactivate(activeTerrainTiles[i][activeTerrainTiles[i].Count - 1]);
                    activeTerrainTiles[i].RemoveAt(activeTerrainTiles[i].Count - 1);
                }

                if (i + 1 > scene.Width)
                {
                    foreach (TerrainTileController t in activeTerrainTiles[i])
                    {
                        terrainTilePool.Deactivate(t);
                    }
                    activeTerrainTiles.RemoveAt(i);
                }
            }
        }
    }
}