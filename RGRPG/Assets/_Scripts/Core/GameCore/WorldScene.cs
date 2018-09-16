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

        public void AdjustPlayerMovementToCollisions(Character character, ref float dx, ref float dy)
        {

            float signedRadiusX = dx == 0 ? 0 : character.Radius * Mathf.Sign(dx);
            float signedRadiusY = dy == 0 ? 0 : character.Radius * Mathf.Sign(dy);

            int beforeTileX, beforeTileY;
            TerrainTile beforeTile = GetTileAt(character.Position.x, character.Position.y, out beforeTileX, out beforeTileY);

            // Get the next X tile
            int nextTileX, ignoreY;
            TerrainTile nextTileH = GetTileAt(character.Position.x + signedRadiusX + dx, character.Position.y, out nextTileX, out ignoreY);
            bool collisionX = nextTileH == null || !nextTileH.Traversable;

            // Get the next Y tile
            int ignoreX, nextTileY;
            TerrainTile nextTileV = GetTileAt(character.Position.x, character.Position.y + signedRadiusY + dy, out ignoreX, out nextTileY);            
            bool collisionY = nextTileV == null || !nextTileV.Traversable;

            if (collisionX && beforeTileX != nextTileX)
            {
                float beforeRadiusPosX = character.Position.x + signedRadiusX;
                float borderX = Mathf.Min(beforeTileX, nextTileX) + 0.5f;
                dx = borderX - beforeRadiusPosX;
            }

            if (collisionY && beforeTileY != nextTileY)
            {
                float beforeRadiusPosY = character.Position.y + signedRadiusY;
                float borderY = Mathf.Min(beforeTileY, nextTileY) + 0.5f;
                dy = borderY - beforeRadiusPosY;
            }
        }

        public TerrainTile GetTileAt(float x, float y)
        {
            int xIndex, yIndex;
            return GetTileAt(x, y, out xIndex, out yIndex);
        }

        public TerrainTile GetTileAt(float x, float y, out int xIndex, out int yIndex)
        {
            xIndex = (int)(x + 0.5f);
            yIndex = (int)(y + 0.5f);

            if (xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1))
            {
                return null;
            }

            return terrainTiles[xIndex, yIndex];
        }

    }
}