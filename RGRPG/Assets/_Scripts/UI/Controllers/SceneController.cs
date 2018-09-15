using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ResetScene()
        {
            if (scene == null)
                throw new System.Exception("Cannot Reset a NULL Scene");

            //TODO: when switching scenes, need to clear out all the old tiles (or reuse them)

            for(int x = 0; x < scene.Width; x++)
            {
                for (int y = 0; y < scene.Height; y++)
                {
                    TerrainTile tile = scene.TerrainTiles[x, y];

                    GameObject tileObject = Instantiate(terrainTileView);
                    tileObject.transform.SetParent(terrainContainer.transform);

                    TerrainTileController tileController = tileObject.GetComponent<TerrainTileController>();
                    tileController.terrainTile = tile;
                    tileController.position = new Vector2(x, y);
                }
            }

        }
    }
}