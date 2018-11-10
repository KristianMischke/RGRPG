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
            CHARACTER_PORTRAIT,
            TERRAIN
        }
        
        private static Dictionary<AssetType, Dictionary<string, AssetData>> assetDB = new Dictionary<AssetType, Dictionary<string, AssetData>>();


        public static void LoadAsset(AssetType assetType, string zType, string filepath, string spriteName = "", bool hasMultiple = false)
        {
            if (!assetDB.ContainsKey(assetType))
            {
                assetDB.Add(assetType, new Dictionary<string, AssetData>());
            }
            if (assetDB[assetType].ContainsKey(zType))
            {
                Debug.LogError("Already added sprite of type: " + assetType.ToString() + " : " + zType);
            }

            Sprite[] sheet = Resources.LoadAll<Sprite>(filepath);
            assetDB[assetType][zType] = new AssetData(zType, filepath, spriteName, sheet, hasMultiple);
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
            if (string.IsNullOrEmpty(zType))
            {
                type = AssetType.NONE;
                zType = "NONE";
            }

            AssetData data = getAssetData(type, zType);

            if (string.IsNullOrEmpty(data.spriteName))
            {
                return data.spriteSheet[0];
            }

            Sprite[] sheet = data.spriteSheet;
            if (sheet != null && sheet.Length > 0)
            {
                //Debug.Log(data.spriteName.Replace("@", i.ToString()));
                for (int j = 0; j < sheet.Length; j++)
                {
                    Sprite x = sheet[j];
                    if (x.name == data.spriteName.Replace("@", i.ToString()))
                    {
                        return x;
                    }
                }
            }

            return null;
        }

    }
}