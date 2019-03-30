using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Stores the raw data for a single tile in the world
    /// </summary>
    public class TerrainTile
    {
        protected string type;
        protected int subType;
        protected string overlayType;
        protected int overlaySubType;
        protected string propType;
        protected string entityType; // this data stores whether or not an entity is spawned here when the game begins

        protected bool traversable; // can this tile be traversed by characters?
        protected bool isWall;      // should this tile be graphically represented as a wall?
        protected Vector2Int position;

        protected string spawnID;

        protected string transitionScene;
        protected string transitionSpawnID;

        
        public string Type { get { return type; } }
        public int SubType { get { return subType; } }
        public string OverlayType { get { return overlayType; } }
        public int OverlaySubType { get { return overlaySubType; } }
        public string PropType { get { return propType; } }
        public string EntityType { get { return entityType; } }

        public bool Traversable { get { return traversable; } }
        public bool IsWall { get { return isWall; } }
        public Vector2Int Position { get { return position; } }
        public string SpawnID { get { return spawnID; } }
        public string TransitionScene { get { return transitionScene; } }
        public string TransitionSpawnID { get { return transitionSpawnID; } }

        public bool IsTransitionTile { get { return !string.IsNullOrEmpty(transitionScene) && !string.IsNullOrEmpty(transitionSpawnID); } }

        public TerrainTile(string type, bool traversable, Vector2Int position, int subType = 0, string overlayType = "", int overlaySubType = 0, string propType = "", string entityType = "", bool isWall = false, string spawnID = "", string transitionScene = "", string transitionSpawnID = "")
        {
            Init(type, traversable, position, subType, overlayType, overlaySubType, propType, entityType, isWall, spawnID, transitionScene, transitionSpawnID);
        }

        public TerrainTile(string type, bool traversable, int positionX, int positionY, int subType = 0, string overlayType = "", int overlaySubType = 0, string propType = "", string entityType = "", bool isWall = false, string spawnID = "", string transitionScene = "", string transitionSpawnID = "")
        {
            Init(type, traversable, new Vector2Int(positionX, positionY), subType, overlayType, overlaySubType, propType, entityType, isWall, spawnID, transitionScene, transitionSpawnID);
        }

        public TerrainTile(XmlNode node, int positionX, int positionY)
        {
            GameXMLLoader.ReadXMLValue(node, "Type", out type);
            GameXMLLoader.ReadXMLValue(node, "SubType", out subType);
            GameXMLLoader.ReadXMLValue(node, "OverlayType", out overlayType);
            GameXMLLoader.ReadXMLValue(node, "OverlaySubType", out overlaySubType);
            GameXMLLoader.ReadXMLValue(node, "PropType", out propType);
            GameXMLLoader.ReadXMLValue(node, "EntityInitType", out entityType);
            if (entityType == "CHARACTER_NONE")
                entityType = "";

            GameXMLLoader.ReadXMLValue(node, "Traversable", out traversable);
            GameXMLLoader.ReadXMLValue(node, "IsWall", out isWall);
            GameXMLLoader.ReadXMLValue(node, "SpawnID", out spawnID);

            XmlNode transitionNode = GameXMLLoader.TryGetChild(node, "Transition");
            GameXMLLoader.ReadXMLValue(transitionNode, "Scene", out transitionScene);
            GameXMLLoader.ReadXMLValue(transitionNode, "SpawnID", out transitionSpawnID);


            this.position = new Vector2Int(positionX, positionY);
        }

        private void Init(string type, bool traversable, Vector2Int position, int subType, string overlayType, int overlaySubType, string propType, string entityType, bool isWall, string spawnID, string transitionScene, string transitionSpawnID)
        {
            this.type = type;
            this.subType = subType;
            this.overlayType = overlayType;
            this.overlaySubType = overlaySubType;
            this.propType = propType;
            this.entityType = entityType;
            this.traversable = traversable;
            this.position = position;
            this.isWall = isWall;
            this.spawnID = spawnID;
            this.transitionScene = transitionScene;
            this.transitionSpawnID = transitionSpawnID;
        }

        public bool Equals(TerrainTile other, bool ignoreSubType = false)
        {
            return type == other.type &&
                (ignoreSubType || subType == other.subType) &&
                overlayType == other.overlayType &&
                (ignoreSubType || overlaySubType == other.overlaySubType) &&
                propType == other.propType &&
                entityType == other.entityType &&
                traversable == other.traversable &&
                isWall == other.isWall &&
                position == other.position &&
                spawnID == other.spawnID &&
                transitionScene == other.transitionScene &&
                transitionSpawnID == other.transitionSpawnID;
        }

        public bool EqualsMapEditor(TerrainTile other, bool ignoreSubType = false)
        {
            return type == other.type &&
                (ignoreSubType || subType == other.subType) &&
                overlayType == other.overlayType &&
                (ignoreSubType || overlaySubType == other.overlaySubType) &&
                propType == other.propType &&
                entityType == other.entityType &&
                traversable == other.traversable &&
                isWall == other.isWall;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Tile");

            GameXMLLoader.WriteXMLValue(writer, "IsWall", isWall);
            GameXMLLoader.WriteXMLValue(writer, "Traversable", traversable);
            GameXMLLoader.WriteXMLValue(writer, "Type", type);
            GameXMLLoader.WriteXMLValue(writer, "SubType", subType.ToString());
            GameXMLLoader.WriteXMLValue(writer, "OverlayType", overlayType);
            GameXMLLoader.WriteXMLValue(writer, "OverlaySubType", overlaySubType.ToString());
            GameXMLLoader.WriteXMLValue(writer, "PropType", propType);
            GameXMLLoader.WriteXMLValue(writer, "EntityInitType", entityType);
            GameXMLLoader.WriteXMLValue(writer, "SpawnID", spawnID);

            writer.WriteStartElement("Transition");
            GameXMLLoader.WriteXMLValue(writer, "Scene", transitionScene);
            GameXMLLoader.WriteXMLValue(writer, "SpawnID", transitionSpawnID);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

    }
}