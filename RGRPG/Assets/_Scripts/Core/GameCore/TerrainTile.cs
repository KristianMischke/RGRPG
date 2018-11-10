using System.Collections;
using System.Collections.Generic;
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
        protected bool traversable; // can this tile be traversed by characters?
        protected int elevation; //TODO: do we need this? terrain that is a different "height", like cliffs and plateaus
        protected bool elevationRamp; //TODO: if we decide to have elevation, then this will determine a tile that ramps up/down elevation
        protected Vector2Int position;

        protected bool isSpawn;

        
        public string Type { get { return type; } }
        public int SubType { get { return subType; } }
        public string OverlayType { get { return overlayType; } }
        public bool Traversable { get { return traversable; } }
        public int Elevation { get { return elevation; } }
        public bool ElevationRamp { get { return elevationRamp; } }
        public Vector2Int Position { get { return position; } }
        public bool IsSpawn { get { return isSpawn; } }

        public TerrainTile(string type, bool traversable, Vector2Int position, int subType = 0, string overlayType = "TERRAIN_NONE", int elevation = 0, bool elevationRamp = false, bool isSpawn = false)
        {
            Init(type, traversable, position, subType, overlayType, elevation, elevationRamp, isSpawn);
        }

        public TerrainTile(string type, bool traversable, int positionX, int positionY, int subType = 0, string overlayType = "TERRAIN_NONE", int elevation = 0, bool elevationRamp = false, bool isSpawn = false)
        {
            Init(type, traversable, new Vector2Int(positionX, positionY), subType, overlayType, elevation, elevationRamp, isSpawn);
        }

        private void Init(string type, bool traversable, Vector2Int position, int subType, string overlayType, int elevation, bool elevationRamp, bool isSpawn)
        {
            this.type = type;
            this.subType = subType;
            this.overlayType = overlayType;
            this.traversable = traversable;
            this.position = position;
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
            this.isSpawn = isSpawn;
        }

        public bool Equals(TerrainTile other, bool ignoreSubType = false)
        {
            return type == other.type &&
                (ignoreSubType || subType == other.subType) &&
                traversable == other.traversable &&
                elevation == other.elevation &&
                elevationRamp == other.elevationRamp &&
                position == other.position &&
                isSpawn == other.isSpawn;
        }

    }
}