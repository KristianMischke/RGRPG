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
        protected TerrainType type;
        protected int subType;
        protected bool traversable; // can this tile be traversed by characters?
        protected int elevation; //TODO: do we need this? terrain that is a different "height", like cliffs and plateaus
        protected bool elevationRamp; //TODO: if we decide to have elevation, then this will determine a tile that ramps up/down elevation
        protected Vector2Int position;

        protected bool isSpawn;


        public TerrainType Type { get { return type; } }
        public int SubType { get { return subType; } }
        public bool Traversable { get { return traversable; } }
        public int Elevation { get { return elevation; } }
        public bool ElevationRamp { get { return elevationRamp; } }
        public Vector2Int Position { get { return position; } }
        public bool IsSpawn { get { return isSpawn; } }

        public TerrainTile(TerrainType type, bool traversable, Vector2Int position, int subType = 0, int elevation = 0, bool elevationRamp = false, bool isSpawn = false)
        {
            this.type = type;
            this.subType = subType;
            this.traversable = traversable;
            this.position = position;
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
            this.isSpawn = isSpawn;
        }

        public TerrainTile(TerrainType type, bool traversable, int positionX, int positionY, int subType = 0, int elevation = 0, bool elevationRamp = false, bool isSpawn = false)
        {
            this.type = type;
            this.subType = subType;
            this.traversable = traversable;
            this.position = new Vector2Int(positionX, positionY);
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