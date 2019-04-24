using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

using RGRPG.Controllers;

namespace RGRPG.Core
{

    public class InfoBase
    {
        protected string zType = "NONE";

        public string ZType { get { return zType; } }

        public virtual void LoadInfo(GameInfos infos, XmlNode node)
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
        bool bOverworldHasDirection;
        bool bEnemy;
        bool bBoss;
        int iHealth;
        int iMagic;
        int iDefense;
        List<InfoAction> actionList;
        float portraitOffsetX;
        float portraitOffsetY;

        public string Class { get { return zClass; } }
        public string Name  { get { return zName; } }
        public bool IsEnemy { get { return bEnemy; } }
        public bool IsBoss  { get { return bBoss; } }
        public int Health   { get { return iHealth; } }
        public int Magic    { get { return iMagic; } }
        public int Defense  { get { return iDefense; } }
        public List<InfoAction> ActionList { get { return actionList; } }

        public float PortraitOffsetX { get { return portraitOffsetX; } }
        public float PortraitOffsetY { get { return portraitOffsetY; } }
        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "Class",           out zClass);
            GameXMLLoader.ReadXMLValue(node, "Name",            out zName);
            GameXMLLoader.ReadXMLValue(node, "zOverworldSheet", out zOverworldSheet);
            GameXMLLoader.ReadXMLValue(node, "zCombatSprite",   out zCombatSprite);
            GameXMLLoader.ReadXMLValue(node, "zPortraitSprite", out zPortraitSprite);
            GameXMLLoader.ReadXMLValue(node, "bOverworldHasDirection", out bOverworldHasDirection);
            GameXMLLoader.ReadXMLValue(node, "bEnemy",          out bEnemy);
            GameXMLLoader.ReadXMLValue(node, "bBoss",           out bBoss);
            GameXMLLoader.ReadXMLValue(node, "iHealth",         out iHealth);
            GameXMLLoader.ReadXMLValue(node, "iMagic",          out iMagic);
            GameXMLLoader.ReadXMLValue(node, "iDefense",        out iDefense);

            // Get the actions for this character
            List<string> actionStringList;
            GameXMLLoader.ReadXMLStringList(node, "ActionList", out actionStringList);
            actionList = new List<InfoAction>();
            foreach (string actionZtype in actionStringList)
            {
                InfoAction actionInfo = infos.Get<InfoAction>(actionZtype);
                actionList.Add(actionInfo);
            }

            GameXMLLoader.ReadXMLValue(node, "portraitOffsetX", out portraitOffsetX);
            GameXMLLoader.ReadXMLValue(node, "portraitOffsetY", out portraitOffsetY);

            string[] path = zOverworldSheet.Split('/');
            string spriteName = path[path.Length - 1] + "_&_@";
            // load assets
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_WORLD_UP, zType, zOverworldSheet, bOverworldHasDirection ? spriteName.Replace('&', 'u') : "", bOverworldHasDirection);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_WORLD_LEFT, zType, zOverworldSheet, bOverworldHasDirection ? spriteName.Replace('&', 'l') : "", bOverworldHasDirection);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_WORLD_RIGHT, zType, zOverworldSheet, bOverworldHasDirection ? spriteName.Replace('&', 'r') : "", bOverworldHasDirection);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_WORLD_DOWN, zType, zOverworldSheet, bOverworldHasDirection ? spriteName.Replace('&', 'd') : "", bOverworldHasDirection);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_COMBAT, zType, zCombatSprite);
            SpriteManager.LoadAsset(SpriteManager.AssetType.CHARACTER_PORTRAIT, zType, zPortraitSprite);
        }

        public Character GenerateCharacter(Game game, GameInfos infos, int characterID)
        {
            List<ICharacterAction> characterActions = new List<ICharacterAction>();

            // Get the actions for this character
            foreach (InfoAction actionInfo in actionList)
            {
                if (actionInfo != null)
                    characterActions.Add(actionInfo.GenerateAction());
                else
                    characterActions.Add(new DudAction());
            }

            return new Character(game, this, characterActions, characterID);
        }
    }

    public class InfoAction : InfoBase
    {
        string zName;
        string zDescription;
        string targetType;
        int targetData;
        List<string> quipList;
        string sourceParticlePrefab;
        string targetsParticlePrefab;

        Type actionType;
        List<string> actionInitParams;
        object[] parameters;
        bool didFail = false;

        public string Name { get { return zName; } }
        public string Description { get { return zDescription; } }
        public string TargetType { get { return targetType; } }
        public int TargetData { get { return targetData; } }
        public List<string> Quips { get { return quipList; } }
        public Type ActionType { get { return actionType; } }
        public string SourceParticlePrefab { get { return sourceParticlePrefab; } }
        public string TargetsParticlePrefab { get { return targetsParticlePrefab; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "Name", out zName);
            GameXMLLoader.ReadXMLValue(node, "Description", out zDescription);
            GameXMLLoader.ReadXMLValue(node, "TargetType", out targetType);
            GameXMLLoader.ReadXMLValue(node, "TargetData", out targetData);
            GameXMLLoader.ReadXMLStringList(node, "Quips", out quipList);
            GameXMLLoader.ReadXMLValue(node, "SourceParticleEffect", out sourceParticlePrefab);
            GameXMLLoader.ReadXMLValue(node, "TargetsParticleEffect", out targetsParticlePrefab);

            // Try to load the C# class node
            XmlNode sharpClassNode;
            if (GameXMLLoader.TryGetChild(node, "SharpClass", out sharpClassNode))
            {
                GameXMLLoader.ReadXMLStringList(sharpClassNode, "InitParams", out actionInitParams, true);

                // Try to load the C# class Type
                string typeName;
                GameXMLLoader.ReadXMLValue(sharpClassNode, "Type", out typeName);
                actionType = Type.GetType(typeName, false);

                // Ensure valid type
                if (actionType != null)
                {
                    // Ensure inherits from ICharacterAction
                    if (actionType.GetInterfaces().Contains(typeof(ICharacterAction)))
                    {
                        try
                        {
                            MethodInfo initMethod = actionType.GetMethod("Init");

                            // Ensure "Init" method exists
                            if (initMethod != null)
                            {
                                ParameterInfo[] initParameters = initMethod.GetParameters();

                                // Ensure Parameter count matches
                                if (initParameters.Length == actionInitParams.Count)
                                {
                                    // Try to convert info parameters to the correct type
                                    parameters = new object[initParameters.Length];

                                    if (initParameters[0].ParameterType != typeof(InfoAction))
                                        LoadFail("Parameter 0 is not of type InfoAction, but is instead type " + initParameters[0].ParameterType.ToString());

                                    parameters[0] = this; // first parameter should always be this InfoAction

                                    for (int i = 1; i < initParameters.Length; i++)
                                    {
                                        try
                                        {
                                            TypeConverter typeConverter = TypeDescriptor.GetConverter(initParameters[i].ParameterType);
                                            parameters[i] = typeConverter.ConvertFromString(actionInitParams[i]);
                                        }
                                        catch (NotSupportedException)
                                        {
                                            LoadFail("Parameter at index " + i + " with value " + actionInitParams[i] + " could not be converted to type " + initParameters[i].ParameterType.ToString());
                                        }
                                    }
                                } else LoadFail("\"Init\" method parameter count does not match infos. infos: " + actionInitParams.Count + " actual: " + initParameters.Length);

                            } else LoadFail("Class " + actionType.ToString() + " does not have a method called \"Init\"");
                        }
                        catch (AmbiguousMatchException)
                        {
                            LoadFail("Type " + actionType.ToString() + " has multiple \"Init\" methods. Must have only one to load properly");
                        }

                    } else LoadFail("Type " + actionType.ToString() + " does not inherit from ICharacterAction");

                } else LoadFail("Type " + typeName + " does not exist");

            } else LoadFail("Does not contain node for \"SharpClass\"");
        }

        private void LoadFail(string reason)
        {
            didFail = true;
            actionType = null;
            actionInitParams = null;
            UnityEngine.Debug.LogError("InfoAction " + zType + " : " + reason);
        }

        public ICharacterAction GenerateAction()
        {
            if (didFail)
                return new DudAction();

            ICharacterAction action = Activator.CreateInstance(actionType) as ICharacterAction;
            MethodInfo initMethod = actionType.GetMethod("Init");
            initMethod.Invoke(action, parameters);

            return action;
        }

        public string GetActionQuip(Character source, Character target)
        {
            Random rnd = new Random();
            string randomQuip = quipList[rnd.Next(0, quipList.Count)];

            randomQuip = randomQuip.Replace("%SOURCE%", source.Name);

            if (target != null)
                randomQuip = randomQuip.Replace("%TARGET%", target.Name);

            return randomQuip;
        }
    }

    public class InfoTargetType : InfoBase
    {
        string description;

        public string Description { get { return description; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "Description", out description);
        }
    }

    public class InfoScene : InfoBase
    {
        string zName;
        string filePath;
        bool isFirst;
        string firstSpawnID;

        public string Name { get { return zName; } }
        public string FilePath { get { return filePath; } }
        public bool IsFirst { get { return isFirst; } }
        public string FirstSpawnID { get { return firstSpawnID; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "Name", out zName);
            GameXMLLoader.ReadXMLValue(node, "File", out filePath);
            GameXMLLoader.ReadXMLValue(node, "bFirst", out isFirst);
            GameXMLLoader.ReadXMLValue(node, "firstSpawn", out firstSpawnID);
        }
    }

    public class InfoTerrain : InfoBase
    {
        string spriteSheet;
        string spriteName;
        bool hasMultiple;

        public string SpriteSheet { get { return spriteSheet; } }
        public string SpriteName { get { return spriteName; } }
        public bool HasMultiple { get { return hasMultiple; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "SpriteSheet", out spriteSheet);
            GameXMLLoader.ReadXMLValue(node, "SpriteName", out spriteName);
            GameXMLLoader.ReadXMLValue(node, "bHasMultiple", out hasMultiple);

            //load asset
            SpriteManager.LoadAsset(SpriteManager.AssetType.TERRAIN, zType, spriteSheet, SpriteName, hasMultiple);
        }
    }

    public class InfoTerrainOverlay : InfoBase
    {
        string spriteSheet;
        string spriteName;
        bool hasMultiple;

        public string SpriteSheet { get { return spriteSheet; } }
        public string SpriteName { get { return spriteName; } }
        public bool HasMultiple { get { return hasMultiple; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "SpriteSheet", out spriteSheet);
            GameXMLLoader.ReadXMLValue(node, "SpriteName", out spriteName);
            GameXMLLoader.ReadXMLValue(node, "bHasMultiple", out hasMultiple);

            //load asset
            SpriteManager.LoadAsset(SpriteManager.AssetType.TERRAIN, zType, spriteSheet, SpriteName, hasMultiple);
        }
    }

    public class InfoTerrainProp : InfoBase
    {
        string spriteSheet;
        string spriteName;
        bool hasMultiple;

        public string SpriteSheet { get { return spriteSheet; } }
        public string SpriteName { get { return spriteName; } }
        public bool HasMultiple { get { return hasMultiple; } }

        override public void LoadInfo(GameInfos infos, XmlNode node)
        {
            base.LoadInfo(infos, node);
            GameXMLLoader.ReadXMLValue(node, "SpriteSheet", out spriteSheet);
            GameXMLLoader.ReadXMLValue(node, "SpriteName", out spriteName);
            GameXMLLoader.ReadXMLValue(node, "bHasMultiple", out hasMultiple);

            //load asset
            SpriteManager.LoadAsset(SpriteManager.AssetType.TERRAIN, zType, spriteSheet, SpriteName, hasMultiple);
        }
    }


}