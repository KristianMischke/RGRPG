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
        protected int elevation; //TODO: do we need this? terrain that is a different "height", like cliffs and plateaus
        protected bool elevationRamp; //TODO: if we decide to have elevation, then this will determine a tile that ramps up/down elevation
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
        public int Elevation { get { return elevation; } }
        public bool ElevationRamp { get { return elevationRamp; } }
        public Vector2Int Position { get { return position; } }
        public string SpawnID { get { return spawnID; } }
        public string TransitionScene { get { return transitionScene; } }
        public string TransitionSpawnID { get { return transitionSpawnID; } }

        public TerrainTile(string type, bool traversable, Vector2Int position, int subType = 0, string overlayType = "", int overlaySubType = 0, string propType = "", string entityType = "", int elevation = 0, bool elevationRamp = false, string spawnID = "", string transitionScene = "", string transitionSpawnID = "")
        {
            Init(type, traversable, position, subType, overlayType, overlaySubType, propType, entityType, elevation, elevationRamp, spawnID, transitionScene, transitionSpawnID);
        }

        public TerrainTile(string type, bool traversable, int positionX, int positionY, int subType = 0, string overlayType = "", int overlaySubType = 0, string propType = "", string entityType = "", int elevation = 0, bool elevationRamp = false, string spawnID = "", string transitionScene = "", string transitionSpawnID = "")
        {
            Init(type, traversable, new Vector2Int(positionX, positionY), subType, overlayType, overlaySubType, propType, entityType, elevation, elevationRamp, spawnID, transitionScene, transitionSpawnID);
        }

        public TerrainTile(XmlNode node, int positionX, int positionY)
        {
            GameXMLLoader.ReadXMLValue(node, "Type", out type);
            GameXMLLoader.ReadXMLValue(node, "SubType", out subType);
            GameXMLLoader.ReadXMLValue(node, "OverlayType", out overlayType);
            GameXMLLoader.ReadXMLValue(node, "OverlaySubType", out overlaySubType);
            GameXMLLoader.ReadXMLValue(node, "PropType", out propType);
            GameXMLLoader.ReadXMLValue(node, "EntityInitType", out entityType);
            GameXMLLoader.ReadXMLValue(node, "Traversable", out traversable);
            GameXMLLoader.ReadXMLValue(node, "Elevation", out elevation);
            GameXMLLoader.ReadXMLValue(node, "ElevationRamp", out elevationRamp);
            GameXMLLoader.ReadXMLValue(node, "SpawnID", out spawnID);

            XmlNode transitionNode = GameXMLLoader.TryGetChild(node, "Transition");
            GameXMLLoader.ReadXMLValue(transitionNode, "Scene", out transitionScene);
            GameXMLLoader.ReadXMLValue(transitionNode, "SpawnID", out transitionSpawnID);


            this.position = new Vector2Int(positionX, positionY);
        }

        private void Init(string type, bool traversable, Vector2Int position, int subType, string overlayType, int overlaySubType, string propType, string entityType, int elevation, bool elevationRamp, string spawnID, string transitionScene, string transitionSpawnID)
        {
            this.type = type;
            this.subType = subType;
            this.overlayType = overlayType;
            this.overlaySubType = overlaySubType;
            this.propType = propType;
            this.entityType = entityType;
            this.traversable = traversable;
            this.position = position;
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
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
                elevation == other.elevation &&
                elevationRamp == other.elevationRamp &&
                position == other.position &&
                spawnID == other.spawnID &&
                transitionScene == other.transitionScene &&
                transitionSpawnID == other.transitionSpawnID;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Tile");

            GameXMLLoader.WriteXMLValue(writer, "Elevation", elevation);
            GameXMLLoader.WriteXMLValue(writer, "ElevationRamp", elevationRamp);
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