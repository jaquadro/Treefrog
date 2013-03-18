using System;
using System.Xml;
using System.Xml.Serialization;

namespace Treefrog.Framework
{
    public class AbstractXmlSerializer<TBase> : IXmlSerializable
    {
        private AbstractXmlSerializer () { }

        public AbstractXmlSerializer (TBase data)
        {
            Data = data;
        }

        public TBase Data { get; set; }

        public static implicit operator TBase (AbstractXmlSerializer<TBase> val)
        {
            return val.Data;
        }

        public static implicit operator AbstractXmlSerializer<TBase> (TBase val)
        {
            return val == null ? null : new AbstractXmlSerializer<TBase>(val);
        }

        public System.Xml.Schema.XmlSchema GetSchema ()
        {
            return null;
        }

        public void ReadXml (XmlReader reader)
        {
            string typeAttr = reader.GetAttribute("Type");
            if (typeAttr == null)
                throw new ArgumentNullException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because no 'Type' attribute was specified in the Xml.");

            Type type = Type.GetType(typeAttr);
            if (type == null)
                throw new InvalidCastException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because the type specified in the Xml was not found ('" + typeAttr + "').");

            if (!type.IsSubclassOf(typeof(TBase)))
                throw new InvalidCastException("Unable to read Xml data for Abstract Type '" + typeof(TBase).Name +
                    " because the type in the specified Xml differs ('" + type.Name + "').");

            reader.ReadStartElement();
            XmlSerializer serializer = new XmlSerializer(type);
            Data = (TBase)serializer.Deserialize(reader);
            reader.ReadEndElement();
        }

        public void WriteXml (XmlWriter writer)
        {
            Type type = Data.GetType();

            writer.WriteAttributeString("Type", type.FullName);
            new XmlSerializer(type).Serialize(writer, Data);
        }
    }
}
