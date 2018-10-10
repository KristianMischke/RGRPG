namespace RGRPG.Core
{



    /// <summary>
    ///     The various types of terrain in the world (will need to update often)
    /// </summary>
    public enum TerrainType
    {
        NONE = 0,

        TERRAIN_GRASS,
        TERRAIN_DIRT,
        TERRAIN_SIDEWALK,
        TERRAIN_ROCK,
        TERRAIN_WATER
    }


    public enum CharacterClassType
    {
        NONE = 0,

        ATTACKER,
        MAGE,
        SUPPORT,
        HEALER,
        THEIF
    }

    public enum CharacterType
    {
        NONE = 0,

        AUSTIN,
    }



}
