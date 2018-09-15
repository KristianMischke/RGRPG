using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RGRPG.Core
{
    public enum TerrainType
    {
        NONE,

        Grass,
        Rock,
        Water,

        COUNT
    }

    public class TerrainTile
    {
        protected TerrainType type;
        protected bool traversable; // can this tile be traversed by characters?
        protected int elevation; //TODO: do we need this? terrain that is a different "height", like cliffs and plateaus
        protected bool elevationRamp; //TODO: if we decide to have elevation, then this will determine a tile that ramps up/down elevation

        public TerrainType Type { get { return type; } }
        public bool Traversable { get { return traversable; } }
        public int Elevation { get { return elevation; } }
        public bool ElevationRamp { get { return elevationRamp; } }

        public TerrainTile(TerrainType type, bool traversable, int elevation = 0, bool elevationRamp = false)
        {
            this.type = type;
            this.traversable = traversable;
            this.elevation = elevation;
            this.elevationRamp = elevationRamp;
        }

    }
}