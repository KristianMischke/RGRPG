using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Stores all the data and functions necessary for a scene
    /// </summary>
    /// <remarks>
    ///     NOTE: there are some functions that allow the world to be manipulated,
    ///     these should primarily be used in the map editor
    /// </remarks>
    public class WorldScene
    {
        protected InfoScene myInfo;
        protected int width, height;
        protected TerrainTile[,] terrainTiles;
        protected bool isIndoors;
        protected Dictionary<string, Vector2Int> spawns;

        public InfoScene MyInfo { get { return myInfo; } }
        public string ZType { get { return myInfo.ZType; } }


        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public TerrainTile[,] TerrainTiles { get { return terrainTiles; } }
        public bool IsIndoors { get { return isIndoors; } }

        public WorldScene(InfoScene myInfo)
        {
            this.myInfo = myInfo;
            spawns = new Dictionary<string, Vector2Int>();
        }

        /// <summary>
        ///     Loads in terrain data from xml located at a specific filepath
        /// </summary>
        /// <param name="filepath">The path to the xml scene file</param>
        public void Load(string filepath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            Load(doc);
        }

        /// <summary>
        ///     Loads in terrain data from and xml string
        /// </summary>
        /// <param name="xml">The string to load from</param>
        public void LoadXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            Load(doc);
        }

        /// <summary>
        ///     Loads in terrain data from an XmlDocument (TODO: include enemy spawn data etc...)
        /// </summary>
        /// <param name="doc">Document to load</param>
        public void Load(XmlDocument doc)
        {
            XmlNode docElem = doc.DocumentElement;

            GameXMLLoader.ReadXMLValue(docElem, "Width", out width);
            GameXMLLoader.ReadXMLValue(docElem, "Height", out height);
            GameXMLLoader.ReadXMLValue(docElem, "bIsIndoors", out isIndoors);

            terrainTiles = new TerrainTile[width, height];
            int i = 0;
            foreach (XmlNode terrainRowNode in GameXMLLoader.TryGetChild(docElem, "Terrain").ChildNodes)
            {
                int j = 0;
                foreach (XmlNode tileNode in terrainRowNode.ChildNodes)
                {
                    terrainTiles[i, j] = new TerrainTile(tileNode, i, j);
                    if (!string.IsNullOrEmpty(terrainTiles[i, j].SpawnID))
                    {
                        spawns.Add(terrainTiles[i, j].SpawnID, new Vector2Int(i, j));
                    }
                    j++;
                }
                i++;
            }
        }

        /// <summary>
        ///     Saves all terrain data to an XML file
        /// </summary>
        /// <param name="filepath">Path to save the file to</param>
        public void Save(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
                return;

            using (XmlWriter writer = XmlWriter.Create(filepath, GameXMLLoader.GetDefaultXMLSettings()))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("WorldScene");
                GameXMLLoader.WriteXMLValue(writer, "Width", width);
                GameXMLLoader.WriteXMLValue(writer, "Height", height);
                GameXMLLoader.WriteXMLValue(writer, "bIsIndoors", isIndoors);

                writer.WriteStartElement("Terrain");
                for (int i = 0; i < terrainTiles.GetLength(0); i++)
                {
                    writer.WriteStartElement("TerrainRow");
                    for (int j = 0; j < terrainTiles.GetLength(1); j++)
                    {
                        terrainTiles[i, j].WriteXml(writer);
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

        public Vector2Int getSpawnPos(string spawnID)
        {
            Vector2Int tile = new Vector2Int(-2, -2);
            spawns.TryGetValue(spawnID, out tile);
            return tile;
        }

        public void LoadDefaultEntities(Game game, List<Enemy> entities)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string entityType = terrainTiles[x, y].EntityType;

                    if (!string.IsNullOrEmpty(entityType))
                    {
                        entities.Add(new Enemy(game, game.Infos.Get<InfoCharacter>(entityType), new Vector2(x, y), new List<ICharacterAction> { new AttackAction(10, 25) }));
                    }
                }
            }
        }

        /// <summary>
        ///     Given a character and desired movement, adjusts that movement based on terrain tile collisions
        /// </summary>
        /// <param name="character">The character that is trying to move</param>
        /// <param name="dx">The desired x amount to move</param>
        /// <param name="dy">The desired y amount to move</param>
        public void AdjustPlayerMovementToCollisions(Character character, ref float dx, ref float dy)
        {

            float signedRadiusX = dx == 0 ? 0 : character.Radius * Mathf.Sign(dx);
            float signedRadiusY = dy == 0 ? 0 : character.Radius * Mathf.Sign(dy);

            // Get the current tile the character is on
            int beforeTileX, beforeTileY;
            GetTileAt(character.Position.x, character.Position.y, out beforeTileX, out beforeTileY);

            // Get the next X tile
            int nextTileX, ignoreY;
            TerrainTile nextTileH = GetTileAt(character.Position.x + signedRadiusX + dx, character.Position.y, out nextTileX, out ignoreY);
            bool collisionX = nextTileH == null || !nextTileH.Traversable;

            // Get the next Y tile
            int ignoreX, nextTileY;
            TerrainTile nextTileV = GetTileAt(character.Position.x, character.Position.y + signedRadiusY + dy, out ignoreX, out nextTileY);
            bool collisionY = nextTileV == null || !nextTileV.Traversable;

            // if there is a collision in the x direction, then adjust dx accordingly
            if (collisionX && beforeTileX != nextTileX)
            {
                float beforeRadiusPosX = character.Position.x + signedRadiusX;
                float borderX = Mathf.Min(beforeTileX, nextTileX) + 0.5f;
                dx = borderX - beforeRadiusPosX;
            }

            // if there is a collision in the y direction, then adjust dy accordingly
            if (collisionY && beforeTileY != nextTileY)
            {
                float beforeRadiusPosY = character.Position.y + signedRadiusY;
                float borderY = Mathf.Min(beforeTileY, nextTileY) + 0.5f;
                dy = borderY - beforeRadiusPosY;
            }
        }


        /// <summary>
        ///     Gets the tile at a given location in world coordinates
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The tile at the given position, or null if there is no tile there</returns>
        public TerrainTile GetTileAt(Vector2 position)
        {
            return GetTileAt(position.x, position.y);
        }

        /// <summary>
        ///     Gets the tile at a given location in world coordinates
        /// </summary>
        /// <param name="x">The x coordinate to look at</param>
        /// <param name="y">The y coordinate to look at</param>
        /// <returns>The tile at the given position, or null if there is no tile there</returns>
        public TerrainTile GetTileAt(float x, float y)
        {
            int xIndex, yIndex;
            return GetTileAt(x, y, out xIndex, out yIndex);
        }

        /// <summary>
        ///     Gets the tile at a given location in world coordinates
        /// </summary>
        /// <param name="x">The x coordinate to look at</param>
        /// <param name="y">The y coordinate to look at</param>
        /// <param name="xIndex">Passes out the xIndex of the tile</param>
        /// <param name="yIndex">Passes out the yIndex of the tile</param>
        /// <returns>The tile at the given position, or null if there is no tile there</returns>
        public TerrainTile GetTileAt(float x, float y, out int xIndex, out int yIndex)
        {
            xIndex = (int)(x + 0.5f);
            yIndex = (int)(y + 0.5f);

            //if (xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1)) { return null; }
            return xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1)
                ? null
                : terrainTiles[xIndex, yIndex];
        }

        /// <summary>
        ///     Gets the tile at a given x and y indices
        /// </summary>
        /// <param name="position"></param>
        /// <returns>The tile at the given position, or null if there is no tile there</returns>
        public TerrainTile GetTileAtIndices(Vector2Int position)
        {
            return GetTileAtIndices(position.x, position.y);
        }

        /// <summary>
        ///     Gets the tile at a given x and y indices
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <returns>The tile at the given position, or null if there is no tile there</returns>
        public TerrainTile GetTileAtIndices(int xIndex, int yIndex)
        {
            return xIndex < 0 || yIndex < 0 || xIndex >= terrainTiles.GetLength(0) || yIndex >= terrainTiles.GetLength(1)
                ? null
                : terrainTiles[xIndex, yIndex];
        }



        ///////////////////////////////////////////////////////
        //       SETTERS FOR PURPOSE OF MAP EDITOR BELOW
        //////////////////////////////////////////////////////



        /// <summary>
        ///     Sets the tile at the position. (safely fails if out of bounds)
        /// </summary>
        /// <param name="x">The xIndex of the target tile</param>
        /// <param name="y">The yIndex of the target tile</param>
        /// <param name="t">The new value of the tile</param>
        public void SetTile(int x, int y, TerrainTile t)
        {
            if (x >= 0 && x < terrainTiles.GetLength(0) && y >= 0 && y < terrainTiles.GetLength(1))
            {
                terrainTiles[x, y] = t;
            }
        }

        /// <summary>
        ///     Sets the tile at the position. (safely fails if out of bounds)
        /// </summary>
        /// <param name="position">the x and y indices of the target tile</param>
        /// <param name="t">the new value of the tile</param>
        public void SetTile(Vector2Int position, TerrainTile t)
        {
            if (position.x >= 0 && position.x < terrainTiles.GetLength(0) && position.y >= 0 && position.y < terrainTiles.GetLength(1))
            {
                terrainTiles[position.x, position.y] = t;
            }
        }

        /// <summary>
        ///     Given new dimensions, expands/shrinks the width and height of the terrain (used in <see cref="Controllers.MapEditorController"/>
        /// </summary>
        /// <param name="width">New width for the map</param>
        /// <param name="height">New height for the map</param>
        /// <param name="expandUp">Whether or not the map should add blank tiles to the top of the map or not</param>
        /// <param name="expandRight">Whether or not the map should add blank tiles to the right of the map or not</param>
        public void AdjustDimensions(int width, int height, string fillTileType, bool expandUp = true, bool expandRight = true)
        {
            if (width > 0 && height > 0)
            {
                int dW = width - this.width;
                int dH = height - this.height;

                this.width = width;
                this.height = height;

                TerrainTile[,] newTiles = new TerrainTile[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int oldX = x - (expandRight ? 0 : dW);
                        int oldY = y - (expandUp ? 0 : dH);
                        if (oldX >= 0 && oldY >= 0 && oldX < terrainTiles.GetLength(0) && oldY < terrainTiles.GetLength(1))
                        {
                            // if the old tile is in the bounds of the the new area, then add it
                            newTiles[x, y] = terrainTiles[oldX, oldY];
                        }
                        else
                        {
                            // otherwise create a new tile
                            newTiles[x, y] = new TerrainTile(fillTileType, true, x, y);
                        }
                    }
                }

                terrainTiles = newTiles;
            }
        }

        public void SetIndoors(bool indoors)
        {
            this.isIndoors = indoors;
        }
    }
}