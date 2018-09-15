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

        public bool CanPlayerMoveHere(Character character, ref float dx, ref float dy)
        {
            // factor in player radius
            Vector2 characterRadius = new Vector2(dx, dy);
            characterRadius.Normalize();
            characterRadius *= character.Radius;

            int beforeTileX, beforeTileY;
            TerrainTile beforeTile = GetTileAt(character.Position.x, character.Position.y, out beforeTileX, out beforeTileY);

            int nextTileX, nextTileY;
            TerrainTile nextTile = GetTileAt(character.Position.x + characterRadius.x + dx, character.Position.y + characterRadius.y + dy, out nextTileX, out nextTileY);

            bool collision = nextTile == null || !nextTile.Traversable;

            if (collision)
            {
                if (beforeTileX != nextTileX)
                {
                    float beforeRadiusPosX = character.Position.x + characterRadius.x;
                    float borderX = Mathf.Min(beforeTileX, nextTileX) + 0.5f;
                    dx = borderX - beforeRadiusPosX;
                }
                if (beforeTileY != nextTileY)
                {
                    float beforeRadiusPosY = character.Position.y + characterRadius.y;
                    float borderY = Mathf.Min(beforeTileY, nextTileY) + 0.5f;
                    dy = borderY - beforeRadiusPosY;
                }
            }

            return true;
            //return collision;
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