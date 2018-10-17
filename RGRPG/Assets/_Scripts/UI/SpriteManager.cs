using System.Collections;
using Type = System.Type;
using Enum = System.Enum;
using System.Collections.Generic;
using UnityEngine;
using RGRPG.Core;
using System.Xml;
using System.Linq;
using System.Text.RegularExpressions;

namespace RGRPG.Controllers
{

    /// <summary>
    ///     A class with a bunch of static methods for converting game data into sprites
    /// </summary>
    public static class SpriteManager
    {

        public struct AssetData
        {
            public string zType;
            public string spriteSheetName;
            public string spriteName;
            public bool hasMultiple;
            public Sprite[] spriteSheet;

            public AssetData(string zType, string spriteSheetName, string spriteName, Sprite[] spriteSheet, bool hasMultiple)
            {
                this.zType = zType;
                this.spriteSheetName = spriteSheetName;
                this.spriteName = spriteName;
                this.spriteSheet = spriteSheet;
                this.hasMultiple = hasMultiple;
            }
        }


        public enum AssetType
        {
            NONE = 0,

            CHARACTER_WORLD,
            CHARACTER_COMBAT,
            TERRAIN
        }
        
        private static Dictionary<AssetType, Dictionary<string, AssetData>> assetDB = new Dictionary<AssetType, Dictionary<string, AssetData>>();


        /// <summary>
        ///     Loads in terrain data from xml located at a specific filepath
        /// </summary>
        /// <param name="filepath">The path to the xml scene file</param>
        public static void LoadSpriteAssets(string filepath, AssetType type)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            LoadSpriteAssets(doc, type);
        }

        /// <summary>
        ///     Loads in terrain data from and xml string
        /// </summary>
        /// <param name="xml">The string to load from</param>
        public static void LoadSpriteAssetsXml(string xml, AssetType type)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            LoadSpriteAssets(doc, type);
        }

        /// <summary>
        ///     Loads in terrain data from an XmlDocument (TODO: include enemy spawn data etc...)
        /// </summary>
        /// <param name="doc">Document to load</param>
        public static void LoadSpriteAssets(XmlDocument doc, AssetType type)
        {
            XmlNode docElem = doc.DocumentElement;

            if (!assetDB.ContainsKey(type))
            {
                assetDB.Add(type, new Dictionary<string, AssetData>());
            }

            foreach (XmlNode entry in docElem.ChildNodes)
            {
                string zType;
                GameXMLLoader.ReadXMLValue(entry, "zType", out zType);
                string spriteSheet;
                GameXMLLoader.ReadXMLValue(entry, "SpriteSheet", out spriteSheet);
                string spriteName;
                GameXMLLoader.ReadXMLValue(entry, "SpriteName", out spriteName);
                bool hasMultiple;
                GameXMLLoader.ReadXMLValue(entry, "bHasMultiple", out hasMultiple);
                
                Sprite[] sheet = Resources.LoadAll<Sprite>(spriteSheet);
                assetDB[type][zType] = new AssetData(zType, spriteSheet, spriteName, sheet, hasMultiple);
            }
        }

        public static AssetData getAssetData(AssetType type, string zType)
        {
            if (!assetDB.ContainsKey(type))
                return new AssetData();
            if (!assetDB[type].ContainsKey(zType))
                return new AssetData();

            return assetDB[type][zType];
        }

        public static Sprite[] getSpriteSheet(AssetType type, string zType)
        {
            AssetData data = getAssetData(type, zType);

            if (data.hasMultiple)
            {
                string pattern = "^(?<name>" + data.spriteName.Replace("@", @"(?<id>\d*)") + ")$";
                Regex rx = new Regex(pattern);

                List<Sprite> sprites = new List<Sprite>();

                foreach (Sprite s in data.spriteSheet)
                {
                    Match match = rx.Match(s.name);
                    GroupCollection groups = match.Groups;

                    if (match.Success)
                    {
                        sprites.Add(s);
                    }
                }

                return sprites.ToArray();
            }

            if (data.spriteSheet.Length == 1)
                return data.spriteSheet;

            return data.spriteSheet.Where(x => x.name == data.spriteName).ToArray();
        }

        public static Sprite getSprite(AssetType type, string zType, int i = 0)
        {
            AssetData data = getAssetData(type, zType);

            if (string.IsNullOrEmpty(data.spriteName))
            {
                return data.spriteSheet[0];
            }

            Sprite[] sheet = data.spriteSheet;
            if (sheet != null && sheet.Length > 0)
            {
                Debug.Log(data.spriteName.Replace("@", i.ToString()));
                return sheet.Single(x => x.name == data.spriteName.Replace("@", i.ToString()));
            }

            return null;
        }

    }
}