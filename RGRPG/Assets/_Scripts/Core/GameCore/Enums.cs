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

        CLASS_ATTACKER,
        CLASS_MAGE,
        CLASS_SUPPORT,
        CLASS_HEALER,
        CLASS_THEIF
    }

    public enum CharacterType
    {
        NONE = 0,

        // Players
        CHARACTER_AUSTIN,

        // Enemies
        CHARACTER_SQUIRREL,
        CHARACTER_GOOSE,
    }



}
