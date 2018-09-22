using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Enum = System.Enum;
using UnityEngine;

namespace RGRPG.Core
{
    public class GameXMLLoader
    {

        public static XmlWriterSettings GetDefaultXMLSettings()
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.NewLineChars = "\n";
            setting.Indent = true;
            setting.IndentChars = "\t";

            return setting;
        }

        public static void WriteXMLValue(XmlWriter writer, string zName, string value)
        {
            writer.WriteStartElement(zName);
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        public static void WriteXMLValue(XmlWriter writer, string zName, bool value)
        {
            WriteXMLValue(writer, zName, value ? "1" : "");
        }

        public static void WriteXMLWriteXMLValueInt(XmlWriter writer, string zName, int value)
        {
            WriteXMLValue(writer, zName, value.ToString());
        }

        public static void WriteXMLValue(XmlWriter writer, string zName, float value)
        {
            WriteXMLValue(writer, zName, value.ToString());
        }



        public static bool ReadXMLValue(XmlNode node, string zName, out string value)
        {
            value = "";
            XmlNode child;
            if (TryGetChild(node, zName, out child))
            {
                value = child.InnerText;
                return true;
            }
            else
            {                
                Debug.LogError("[XMLLoader] Failed loading value from " + zName);
                return false;
            }
        }

        public static void ReadXMLValue(XmlNode node, string zName, out bool value)
        {
            value = false;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                value = !(stringValue == "0" || stringValue == "");
            }
        }

        public static void ReadXMLValue(XmlNode node, string zName, out int value)
        {
            value = 0;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                int.TryParse(stringValue, out value);
            }
        }

        public static void ReadXMLValue(XmlNode node, string zName, out float value)
        {
            value = 0f;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                float.TryParse(stringValue, out value);
            }
        }

        public static void ReadXMLValue<T>(XmlNode node, string zName, out T value)
        {
            value = default(T);
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                value = (T)Enum.Parse(typeof(T), stringValue);
            }
        }


        public static XmlNode TryGetChild(XmlNode node, string zName)
        {
            XmlNode child;
            TryGetChild(node, zName, out child);
            return child;
        }

        public static bool TryGetChild(XmlNode node, string zName, out XmlNode child)
        {
            child = null;
            foreach(XmlNode c in node.ChildNodes)
            {
                if (c.Name == zName)
                {
                    child = c;
                    return true;
                }
            }

            return false;
        }

    }
}
