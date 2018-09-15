using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public class WorldScene
    {
        protected int width, height;
        protected TerrainTile[,] terrainTiles;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public TerrainTile[,] TerrainTiles { get { return terrainTiles; } }

        public WorldScene(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.terrainTiles = new TerrainTile[width, height];

            Init();
        }

        public void Init()
        {
            //TODO: initialize scenes from xml file

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isEdge = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    terrainTiles[x, y] = new TerrainTile((TerrainType)Random.Range(1, 3), !isEdge);
                }
            }
        }

        public bool IsTerrainTraversable(int x, int y)
        {
            return terrainTiles[x, y].Traversable;
        }

    }
}