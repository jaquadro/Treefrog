using System;
using System.Collections.Generic;
using System.Xml;

namespace Treefrog.Framework
{
    /// <summary>
    /// A collection of helper methods for parsing XML trees.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Read all top-level elements in a sub-tree, invoking an action on each element.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> to parse from.</param>
        /// <param name="action">A no-return function that takes an <see cref="XmlReader"/> and element name as parameters.</param>
        /// <remarks>This helper method automatically advances to the next element after executing an element.  If the action itself
        /// advances the reader, use <see cref="SwitchAllAdvance"/> instead.</remarks>
        /// <seealso cref="SwitchAllAdvance"/>
        public static void SwitchAll (XmlReader reader, Action<XmlReader, string> action)
        {
            if (reader.IsStartElement()) {
                using (XmlReader subReader = reader.ReadSubtree()) {
                    while (subReader.Read()) {
                        if (subReader.IsStartElement()) {
                            action(subReader, subReader.Name);
                        }
                    }
                }
            }
            else {
                while (reader.Read()) {
                    if (reader.IsStartElement()) {
                        action(reader, reader.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Read all top-level elements in a sub-tree, invoking an action on each element and conditionally advancing to the next element.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> to parse from.</param>
        /// <param name="func">A function that takes an <see cref="XmlReader"/> and element name as parameters, and returns a value indicating whether to advance to the next element.</param>
        /// <remarks>If the action method returns <c>true</c>, this helper method will advance the reader to the next element.  Otherwise, it will resume
        /// parsing from the reader without automatically advancing.</remarks>
        /// <seealso cref="SwitchAll"/>
        public static void SwitchAllAdvance (XmlReader reader, Func<XmlReader, string, bool> func)
        {
            using (XmlReader subReader = reader.ReadSubtree()) {
                while (!subReader.EOF) {
                    if (subReader.IsStartElement()) {
                        if (func(subReader, subReader.Name)) {
                            subReader.Read();
                        }
                    }
                    else {
                        subReader.Read();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a key/value set of all attributes in the element, and checks that an instance of each required key exists in the set.
        /// </summary>
        /// <param name="reader">An <see cref="XmlReader"/> pointing to the element to fetch attributes from.</param>
        /// <param name="reqAttribs">A list of all attributes which are required to be present in the element.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> containing the key/value pairs of all the attributes in the element.</returns>
        public static Dictionary<string, string> CheckAttributes (XmlReader reader, List<string> reqAttribs)
        {
            Dictionary<string, string> attribs = new Dictionary<string, string>();

            if (reader.HasAttributes) {
                while (reader.MoveToNextAttribute()) {
                    attribs[reader.Name] = reader.Value;
                }
                reader.MoveToElement();
            }

            foreach (string name in reqAttribs) {
                if (!attribs.ContainsKey(name)) {
                    throw new Exception("Required attribute '" + name + "' missing in tag '" + reader.Name + "'");
                }
            }

            return attribs;
        }
    }
}
