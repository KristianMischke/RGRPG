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
            Action<XmlDocument, Dictionary<string, InfoBase>, Dictionary<Type, List<InfoBase>>> loadNodes;

            public InfoFile(string path, Action<XmlDocument, Dictionary<string, InfoBase>, Dictionary<Type, List<InfoBase>>> LoadNodes)
            {
                document = new XmlDocument();
                document.Load(path);
                loadNodes = LoadNodes;
            }

            public void LoadNodes(Dictionary<string, InfoBase> infoObjects, Dictionary<Type, List<InfoBase>> infoObjectsList)
            {
                loadNodes(document, infoObjects, infoObjectsList);
            }

        }

        List<InfoFile> infoFiles = new List<InfoFile>();

        Dictionary<string, InfoBase> infoObjects = new Dictionary<string, InfoBase>();
        Dictionary<Type, List<InfoBase>> infoObjectsList = new Dictionary<Type, List<InfoBase>>();

        public GameInfos()
        {
            InitInfoFiles();
            //LoadModdedInfos();
            LoadInfos();
        }

        void InitInfoFiles()
        {
            infoFiles.Add(new InfoFile(Application.dataPath.ToString() + INFO_PATH + "InfoCharacters" + ".xml", LoadNodes<InfoCharacter>));
            infoFiles.Add(new InfoFile(Application.dataPath.ToString() + INFO_PATH + "InfoActions" + ".xml", LoadNodes<InfoAction>));
        }

        void LoadInfos()
        {
            foreach (InfoFile infoFile in infoFiles)
            {
                infoFile.LoadNodes(infoObjects, infoObjectsList);
            }
        }

        void LoadNodes<T>(XmlDocument document, Dictionary<string, InfoBase> infoObjects, Dictionary<Type, List<InfoBase>> infoObjectsList) where T : InfoBase, new()
        {
            foreach (XmlNode entry in document.DocumentElement.ChildNodes)
            {
                T obj = new T();
                obj.LoadInfo(entry);

                if (infoObjects.ContainsKey(obj.ZType))
                    Debug.LogError("Info type " + obj.ZType + " already exists!");
                else
                {
                    infoObjects[obj.ZType] = obj;

                    if (!infoObjectsList.ContainsKey(typeof(T)))
                    {
                        infoObjectsList[typeof(T)] = new List<InfoBase>();
                    }

                    infoObjectsList[typeof(T)].Add(obj);
                }
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

        public List<T> GetAll<T>() where T : InfoBase
        {
            List<T> list = new List<T>();

            List<InfoBase> baseList = infoObjectsList[typeof(T)];

            foreach (InfoBase i in baseList)
            {
                if(i.ZType != "NONE")
                    list.Add(i as T);
            }

            return list;
        }

    }
}