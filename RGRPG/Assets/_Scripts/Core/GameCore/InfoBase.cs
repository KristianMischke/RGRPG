using System.Collections;
using System.Collections.Generic;
using System.Xml;

using RGRPG.Controllers;

namespace RGRPG.Core
{

    public class InfoBase
    {
        protected string zType = "NONE";

        public string ZType { get { return zType; } }

        public virtual void LoadInfo(XmlNode node)
        {
            GameXMLLoader.ReadXMLValue(node, "zType", out zType);
        }
    }

    public class InfoCharacter : InfoBase
    {
        string zClass;
        string zName;
        string zOverworldSheet;
        string zCombatSprite;
        string zPortraitSprite;
        bool bEnemy;
        int iHealth;
        int iMagic;
        int iDefense;
        List<string> actionList;

        float portraitOffsetX;
        float portraitOffsetY;

        public string Class { get { return zClass; } }
        public string Name  { get { return zName; } }
        public bool IsEnemy { get { return bEnemy; } }
        public int Health   { get { return iHealth; } }
        public int Magic    { get { return iMagic; } }
        public int Defence  { get { return iDefense; } }
        public List<string> ActionListTypes { get { return actionList; } }

        public float PortraitOffsetX {get { return portraitOffsetX; }}
        public float PortraitOffsetY{get { return portraitOffsetY; }}
        override public void LoadInfo(XmlNode node)
        {
            base.LoadInfo(node);
            GameXMLLoader.ReadXMLValue(node, "Class",           out zClass);
            GameXMLLoader.ReadXMLValue(node, "Name",            out zName);
            GameXMLLoader.ReadXMLValue(node, "zOverworldSheet", out zOverworldSheet);
            GameXMLLoader.ReadXMLValue(node, "zCombatSprite",   out zCombatSprite);
            GameXMLLoader.ReadXMLValue(node, "zPortraitSprite", out zPortraitSprite);
            GameXMLLoader.ReadXMLValue(node, "bEnemy",          out bEnemy);
            GameXMLLoader.ReadXMLValue(node, "iHealth",         out iHealth);
            GameXMLLoader.ReadXMLValue(node, "iMagic",          out iMagic);
            GameXMLLoader.ReadXMLValue(node, "iDefense",        out iDefense);
            GameXMLLoader.ReadXMLStringList(node, "ActionList", out actionList);
            GameXMLLoader.ReadXMLValue(node, "portraitOffsetX",        out portraitOffsetX);
            GameXMLLoader.ReadXMLValue(node, "portraitOffsetY",        out portraitOffsetY);

            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_WORLD, zType, zOverworldSheet, "", true);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_COMBAT, zType, zCombatSprite);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_PORTRAIT, zType, zPortraitSprite);
        }
    }

}