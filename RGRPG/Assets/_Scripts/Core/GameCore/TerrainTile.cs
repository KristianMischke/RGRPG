using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     The various types of terrain in the world (will need to update often)
    /// </summary>
    public enum TerrainType
    {
        NONE,

        Grass,
        Rock,
        Water,

        COUNT
    }

    /// <summary>
    ///     Stores the raw data for a single tile in the world
    /// </summary>
    public class TerrainTile
    {
        protected TerrainType type;
        protected bool traversable; // can this tile be traversed by characters?
        protected int elevation; //TODO: do we need this? terrain that is a different "height", like cliffs and plateaus
        protected bool elevationRamp; //TODO: if we decide to have elevation, then this will determine a tile that ramps up/down elevation
        protected Vector2Int position;


        public TerrainType Type { get { return type; } }
        public bool Traversable { get { return traversable; } }
        public int Elevation { get { return elevation; } }
        public bool ElevationRamp { get { return elevationRamp; } }
        public Vector2Int Position { get { return position; } }

        public TerrainTile(TerrainType type, bool traversable, Vector2Int position, int elevation = 0, bool elevationRamp = false)
        {
            this.type = type;
            this.traversable = traversable;
            this.position = position;
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
        }

        public TerrainTile(TerrainType type, bool traversable, int positionX, int positionY, int elevation = 0, bool elevationRamp = false)
        {
            this.type = type;
            this.traversable = traversable;
            this.position = new Vector2Int(positionX, positionY);
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
        }

        public bool Equals(TerrainTile other)
        {
            return type == other.type &&
                traversable == other.traversable &&
                elevation == other.elevation &&
                elevationRamp == other.elevationRamp &&
                position == other.position;
        }

    }
}