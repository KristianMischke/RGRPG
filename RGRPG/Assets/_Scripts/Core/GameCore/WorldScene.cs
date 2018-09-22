using System.Collections;
using System.Collections.Generic;
using System.Xml;
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

            /*for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isEdge = x == 0 || x == width - 1 || y == 0 || y == height - 1;
                    terrainTiles[x, y] = new TerrainTile((TerrainType)Random.Range(1, 3), !isEdge);
                }
            }*/

            TextAsset worldXMLTest = Resources.Load<TextAsset>(@"Data\WorldSceneTest");
            LoadXml(worldXMLTest.text);
        }

        public void Load(string filepath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            Load(doc);
        }

        public void LoadXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Load(doc);
        }

        public void Load(XmlDocument doc)
        {
            XmlNode docElem = doc.DocumentElement;

            GameXMLLoader.ReadXMLValue(docElem, "Width", out width);
            GameXMLLoader.ReadXMLValue(docElem, "Height", out height);

            terrainTiles = new TerrainTile[width, height];
            int i = 0;
            foreach (XmlNode terrainRowNode in GameXMLLoader.TryGetChild(docElem, "Terrain").ChildNodes)
            {
                int j = 0;
                foreach (XmlNode tileNode in terrainRowNode.ChildNodes)
                {
                    TerrainType type;
                    GameXMLLoader.ReadXMLValue(tileNode, "Type", out type);
                    bool traversable;
                    GameXMLLoader.ReadXMLValue(tileNode, "Traversable", out traversable);
                    int elevation;
                    GameXMLLoader.ReadXMLValue(tileNode, "Elevation", out elevation);
                    bool elevationRamp;
                    GameXMLLoader.ReadXMLValue(tileNode, "ElevationRamp", out elevationRamp);
                    terrainTiles[i, j] = new TerrainTile(type, traversable, elevation, elevationRamp);

                    j++;
                }
                i++;
            }
        }

        public void Save(string filepath)
        {
            using (XmlWriter writer = XmlWriter.Create(filepath, GameXMLLoader.GetDefaultXMLSettings()))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("WorldScene");
                GameXMLLoader.WriteXMLValue(writer, "Width", width);
                GameXMLLoader.WriteXMLValue(writer, "Height", height);

                writer.WriteStartElement("Terrain");
                for (int i = 0; i < terrainTiles.GetLength(0); i++)
                {
                    writer.WriteStartElement("TerrainRow");
                    for (int j = 0; j < terrainTiles.GetLength(1); j++)
                    {
                        writer.WriteStartElement("Tile");

                        GameXMLLoader.WriteXMLValue(writer, "Elevation", terrainTiles[i, j].Elevation);
                        GameXMLLoader.WriteXMLValue(writer, "ElevationRamp", terrainTiles[i, j].ElevationRamp);
                        GameXMLLoader.WriteXMLValue(writer, "Traversable", terrainTiles[i, j].Traversable);
                        GameXMLLoader.WriteXMLValue(writer, "Type", terrainTiles[i, j].Type.ToString());

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
        }

        public void AdjustPlayerMovementToCollisions(Character character, ref float dx, ref float dy)
        {

            float signedRadiusX = dx == 0 ? 0 : character.Radius * Mathf.Sign(dx);
            float signedRadiusY = dy == 0 ? 0 : character.Radius * Mathf.Sign(dy);

            int beforeTileX, beforeTileY;
            TerrainTile beforeTile;
            beforeTile = GetTileAt(character.Position.x, character.Position.y, out beforeTileX, out beforeTileY);

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

            //if (xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1)) { return null; }
            return xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1)
                ? null
                : terrainTiles[xIndex, yIndex];
        }

    }
}