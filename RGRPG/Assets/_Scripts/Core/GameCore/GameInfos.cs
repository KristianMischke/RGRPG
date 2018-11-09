using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using UnityEngine;

namespace RGRPG.Core
{

    /// <summary>
    ///     Stores static info related to the game
    /// </summary>
    public class GameInfos
    {
        const string INFO_PATH = @"\Resources\Data\Infos\";

        class InfoFile
        {
            XmlDocument document;
            Action<XmlDocument, Dictionary<string, InfoBase>> loadNodes;

            public InfoFile(string path, Action<XmlDocument, Dictionary<string, InfoBase>> LoadNodes)
            {
                document = new XmlDocument();
                document.Load(path);
                loadNodes = LoadNodes;
            }

            public void LoadNodes(Dictionary<string, InfoBase> infoObjects)
            {
                loadNodes(document, infoObjects);
            }

        }

        List<InfoFile> infoFiles = new List<InfoFile>();

        Dictionary<string, InfoBase> infoObjects = new Dictionary<string, InfoBase>();

        public GameInfos()
        {
            InitInfoFiles();
            //LoadModdedInfos();
            LoadInfos();
        }

        void InitInfoFiles()
        {
            infoFiles.Add(new InfoFile(Application.dataPath.ToString() + INFO_PATH + "CharacterAssets" + ".xml", LoadNodes<InfoCharacter>));
        }

        void LoadInfos()
        {
            foreach (InfoFile infoFile in infoFiles)
            {
                infoFile.LoadNodes(infoObjects);
            }
        }

        void LoadNodes<T>(XmlDocument document, Dictionary<string, InfoBase> infoObjects) where T : InfoBase, new()
        {
            foreach (XmlNode entry in document.DocumentElement.ChildNodes)
            {
                T obj = new T();
                obj.LoadInfo(entry);

                if (infoObjects.ContainsKey(obj.ZType))
                    Debug.LogError("Info type " + obj.ZType + " already exists!");
                else
                    infoObjects[obj.ZType] = obj;
            }
        }

        public T Get<T>(string zType) where T : InfoBase
        {
            InfoBase returnObj;
            infoObjects.TryGetValue(zType, out returnObj);
            
            T targetType = returnObj as T;
            //Debug.Log(targetType);
            if (targetType != null)
                return targetType;
            else
                throw new System.Exception("Cannot find the entry " + zType + " as " + typeof(T).ToString());
        }

    }
}