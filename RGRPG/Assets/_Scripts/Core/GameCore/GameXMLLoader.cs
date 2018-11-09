using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Enum = System.Enum;
using UnityEngine;

namespace RGRPG.Core
{
    /// <summary>
    ///     Class with a bunch of static methods for making reading and writing XML easier
    /// </summary>
    public class GameXMLLoader
    {
        /// <summary>
        ///     Returns the XmlWriterSettings that we use for our files
        /// </summary>
        /// <returns></returns>
        public static XmlWriterSettings GetDefaultXMLSettings()
        {
            XmlWriterSettings setting = new XmlWriterSettings();
            setting.ConformanceLevel = ConformanceLevel.Auto;
            setting.NewLineChars = "\n";
            setting.Indent = true;
            setting.IndentChars = "\t";

            return setting;
        }

        /// <summary>
        ///     Writes the given string to the given XmlWriter
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="zName">The name of the xml element</param>
        /// <param name="value">The value of the xml element</param>
        public static void WriteXMLValue(XmlWriter writer, string zName, string value)
        {
            writer.WriteStartElement(zName);
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        /// <summary>
        ///     Writes the given bool to the given XmlWriter
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="zName">The name of the xml element</param>
        /// <param name="value">The value of the xml element</param>
        public static void WriteXMLValue(XmlWriter writer, string zName, bool value)
        {
            WriteXMLValue(writer, zName, value ? "1" : "");
        }

        /// <summary>
        ///     Writes the given int to the given XmlWriter
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="zName">The name of the xml element</param>
        /// <param name="value">The value of the xml element</param>
        public static void WriteXMLWriteXMLValueInt(XmlWriter writer, string zName, int value)
        {
            WriteXMLValue(writer, zName, value.ToString());
        }

        /// <summary>
        ///     Writes the given float to the given XmlWriter
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="zName">The name of the xml element</param>
        /// <param name="value">The value of the xml element</param>
        public static void WriteXMLValue(XmlWriter writer, string zName, float value)
        {
            WriteXMLValue(writer, zName, value.ToString());
        }


        /// <summary>
        ///     Tries to read a string value from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        /// <returns>True if successfully got the value</returns>
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

        /// <summary>
        ///     Tries to read a bool value from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        public static void ReadXMLValue(XmlNode node, string zName, out bool value)
        {
            value = false;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                value = !(stringValue == "0" || stringValue == "");
            }
        }

        /// <summary>
        ///     Tries to read a int value from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        public static void ReadXMLValue(XmlNode node, string zName, out int value)
        {
            value = 0;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                int.TryParse(stringValue, out value);
            }
        }

        /// <summary>
        ///     Tries to read a float value from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        public static void ReadXMLValue(XmlNode node, string zName, out float value)
        {
            value = 0f;
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                float.TryParse(stringValue, out value);
            }
        }

        /// <summary>
        ///     Tries to read a list of values from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        public static void ReadXMLStringList(XmlNode node, string zName, out List<string> value)
        {
            value = null;

            XmlNode listRoot;
            if (TryGetChild(node, zName, out listRoot))
            {
                value = new List<string>();

                foreach(XmlNode child in listRoot.ChildNodes)
                {
                    if (child.Name == "Item")
                    {
                        string childValue = child.InnerText;
                        value.Add(childValue);
                    }
                }
            }
        }

        /// <summary>
        ///     Tries to read an Enum of type T value from the given xml tag within the given node
        /// </summary>
        /// <param name="node">The node to get the child value from</param>
        /// <param name="zName">The name of the child node you're looking for</param>
        /// <param name="value">The value of the child node you're looking for</param>
        public static void ReadXMLValue<T>(XmlNode node, string zName, out T value)
        {
            value = default(T);
            string stringValue;
            if (ReadXMLValue(node, zName, out stringValue))
            {
                value = (T)Enum.Parse(typeof(T), stringValue);
            }
        }

        /// <summary>
        ///     Tries to get the child node of the given name
        /// </summary>
        /// <param name="node">The node to look in</param>
        /// <param name="zName">The node you are looking for</param>
        /// <returns>The target node, or null if none was found</returns>
        public static XmlNode TryGetChild(XmlNode node, string zName)
        {
            XmlNode child;
            TryGetChild(node, zName, out child);
            return child;
        }

        /// <summary>
        ///     Tries to get the child node of the given name
        /// </summary>
        /// <param name="node">The node to look in</param>
        /// <param name="zName">The node you are looking for</param>
        /// <param name="child">The target node</param>
        /// <returns>True if it was found, else false</returns>
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
