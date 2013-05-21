using System;
using System.Collections.Generic;
using System.Reflection;
using Treefrog.Framework.Model;

namespace Treefrog.Framework
{
    public class PropertyClassManager
    {
        private class PackPair
        {
            public PackFunc Pack;
            public UnpackFunc Unpack;

            public PackPair (PackFunc pack, UnpackFunc unpack)
            {
                Pack = pack;
                Unpack = unpack;
            }
        }

        private class PropertyRecord
        {
            public PropertyInfo Info { get; set; }
            public SpecialPropertyAttribute Attr { get; set; }
            public PropertyConverter Converter { get; set; }
        }

        private delegate Property PackFunc (string name, object value);
        private delegate object UnpackFunc (Property property);

        private static List<Type> _stringPropertyTypes = new List<Type>() {
            typeof(string),
        };

        private static List<Type> _numberPropertyTypes = new List<Type>() {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double)
        };

        private static List<Type> _boolPropertyTypes = new List<Type>() {
            typeof(bool),
        };

        private static PackPair _packPairBool = new PackPair(PackBool, UnpackBool);
        private static PackPair _packPairNumber = new PackPair(PackNumber, UnpackNumber);
        private static PackPair _packPairString = new PackPair(PackString, UnpackString);

        private static Dictionary<Type, PackPair> _defaultTypes = new Dictionary<Type, PackPair>() {
            { typeof(bool), _packPairBool },
            { typeof(byte), _packPairNumber },
            { typeof(double), _packPairNumber },
            { typeof(float), _packPairNumber },
            { typeof(int), _packPairNumber },
            { typeof(long), _packPairNumber },
            { typeof(sbyte), _packPairNumber },
            { typeof(short), _packPairNumber },
            { typeof(string), _packPairString },
            { typeof(uint), _packPairNumber },
            { typeof(ulong), _packPairNumber },
            { typeof(ushort), _packPairNumber },
        };

        private Dictionary<string, PropertyRecord> _records = new Dictionary<string, PropertyRecord>();

        public PropertyClassManager (Type type)
        {
            foreach (PropertyInfo prop in type.GetProperties()) {
                foreach (SpecialPropertyAttribute attr in prop.GetCustomAttributes(typeof(SpecialPropertyAttribute), false)) {
                    AddProperty(prop, attr);
                }
            }
        }

        private void AddProperty (PropertyInfo info, SpecialPropertyAttribute attr)
        {
            PropertyRecord record = new PropertyRecord() {
                Info = info,
                Attr = attr,
            };

            if (attr.Converter != null && attr.Converter.IsSubclassOf(typeof(PropertyConverter)))
                record.Converter = Activator.CreateInstance(attr.Converter) as PropertyConverter;

            _records[attr.Name ?? info.Name] = record;
        }

        public bool ContainsDefinition (string name)
        {
            if (name == null)
                return false;
            return _records.ContainsKey(name);
        }

        public IEnumerable<string> RegisteredNames
        {
            get { return _records.Keys; }
        }

        public Property LookupProperty (object inst, string name)
        {
            PropertyRecord record;
            if (!_records.TryGetValue(name, out record))
                return null;

            Property packed = null;
            if (record.Converter != null)
                packed = record.Converter.Pack(name, record.Info.GetValue(inst, null));
            else {
                PackPair packPair;
                if (_defaultTypes.TryGetValue(record.Info.PropertyType, out packPair))
                    packed = packPair.Pack(name, record.Info.GetValue(inst, null));
            }

            if (packed == null)
                return null;

            if (!record.Attr.Readonly) {
                if (record.Converter != null)
                    packed.Modified += (s, e) => { UnpackCustomProperty(s, inst); };
                else if (record.Info.CanWrite) {
                    PackPair packPair;
                    if (_defaultTypes.TryGetValue(record.Info.PropertyType, out packPair))
                        packed.Modified += (s, e) => { UnpackDefaultProperty(s, inst, packPair.Unpack); };
                }
            }

            return packed;
        }

        public bool IsReadOnly (string name)
        {
            PropertyRecord record;
            if (!_records.TryGetValue(name, out record))
                return false;

            return record.Attr.Readonly || !record.Info.CanWrite;
        }

        private static Property PackString (string name, object value)
        {
            return new StringProperty(name, Convert.ToString(value));
        }

        private static Property PackNumber (string name, object value)
        {
            return new NumberProperty(name, Convert.ToSingle(value));
        }

        private static Property PackBool (string name, object value)
        {
            return new BoolProperty(name, Convert.ToBoolean(value));
        }

        private void UnpackCustomProperty (object sender, object inst)
        {
            Property property = sender as Property;
            if (property == null)
                return;

            PropertyRecord record;
            if (!_records.TryGetValue(property.Name, out record))
                return;

            object value = record.Converter.Unpack(property);

            record.Info.SetValue(inst, value, null);
        }

        private void UnpackDefaultProperty (object sender, object inst, UnpackFunc unpacker)
        {
            Property property = sender as Property;
            if (property == null)
                return;

            PropertyRecord record;
            if (!_records.TryGetValue(property.Name, out record))
                return;

            record.Info.SetValue(inst, unpacker(property), null);
        }

        private static object UnpackString (Property property)
        {
            StringProperty p = property as StringProperty;
            if (p != null)
                return p.Value;

            return null;
        }

        private static object UnpackNumber (Property property)
        {
            NumberProperty p = property as NumberProperty;
            if (p != null)
                return p.Value;

            return null;
        }

        private static object UnpackBool (Property property)
        {
            BoolProperty p = property as BoolProperty;
            if (p != null)
                return p.Value;

            return null;
        }
    }
}
