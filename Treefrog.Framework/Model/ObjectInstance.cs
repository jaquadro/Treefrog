using System;
using System.Runtime.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;
using System.Collections.Generic;

namespace Treefrog.Framework.Model
{
    [Serializable]
    public class ObjectInstance : IPropertyProvider, ICloneable, ISerializable
    {
        [NonSerialized]
        private ObjectClass _class;

        private int _posX;
        private int _posY;
        private float _rotation;
        private float _scaleX;
        private float _scaleY;

        public ObjectInstance (ObjectClass objClass, int posX, int posY)
        {
            _class = objClass;
            _posX = posX;
            _posY = posY;
            _rotation = 0;
            _scaleX = 1f;
            _scaleY = 1f;

            _properties = new PropertyCollection(_reservedPropertyNames);
            _predefinedProperties = new ObjectInstanceProperties(this);
        }

        public ObjectInstance (ObjectClass objClass)
            : this(objClass, 0, 0)
        {
        }

        public ObjectInstance (ObjectInstance inst)
            : this(inst._class, inst._posX, inst._posY)
        {
            _rotation = inst._rotation;
            _scaleX = inst._scaleX;
            _scaleY = inst._scaleY;

            foreach (Property prop in inst._properties) {
                _properties.Add(prop.Clone() as Property);
            }
        }

        public ObjectClass ObjectClass
        {
            get { return _class; }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                if (_rotation != value) {
                    _rotation = value;
                    OnRotationChanged(EventArgs.Empty);
                }
            }
        }

        public float ScaleX
        {
            get { return _scaleX; }
        }

        public float ScaleY
        {
            get { return _scaleY; }
        }

        public int X
        {
            get { return _posX; }
            set
            {
                if (_posX != value) {
                    _posX = value;
                    OnPositionChanged(EventArgs.Empty);
                }
            }
        }

        public int Y
        {
            get { return _posY; }
            set {
                if (_posY != value) {
                    _posY = value;
                    OnPositionChanged(EventArgs.Empty);
                }            
            }
        }

        public Point Position
        {
            get { return new Point(_posX, _posY); }
            set
            {
                if (value.X != _posX || value.Y != _posY) {
                    _posX = value.X;
                    _posY = value.Y;
                    OnPositionChanged(EventArgs.Empty);
                }
            }
        }

        public Rectangle ImageBounds
        {
            get
            {
                return new Rectangle(
                    _posX - _class.Origin.X + _class.ImageBounds.Left,
                    _posY - _class.Origin.Y + _class.ImageBounds.Top,
                    _class.ImageBounds.Width, _class.ImageBounds.Height);
            }
        }

        public Rectangle MaskBounds
        {
            get
            {
                return new Rectangle(
                    _posX - _class.Origin.X + _class.MaskBounds.Left,
                    _posY - _class.Origin.Y + _class.MaskBounds.Top,
                    _class.MaskBounds.Width, _class.MaskBounds.Height);
            }
        }

        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;

        protected virtual void OnPositionChanged (EventArgs e)
        {
            var ev = PositionChanged;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnRotationChanged (EventArgs e)
        {
            var ev = RotationChanged;
            if (ev != null)
                ev(this, e);
        }

        #region IPropertyProvider Members

        private static string[] _reservedPropertyNames = new string[] { "X", "Y", "Rotation" };

        private PropertyCollection _properties;
        private ObjectInstanceProperties _predefinedProperties;

        private class ObjectInstanceProperties : PredefinedPropertyCollection
        {
            private ObjectInstance _parent;

            public ObjectInstanceProperties (ObjectInstance parent)
                : base(_reservedPropertyNames)
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield return _parent.LookupProperty("X");
                yield return _parent.LookupProperty("Y");
                yield return _parent.LookupProperty("Rotation");
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public string PropertyProviderName
        {
            get { return "Object#"; }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public Collections.PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public Collections.PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties;  }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            switch (name) {
                case "X":
                case "Y":
                case "Rotation":
                    return PropertyCategory.Predefined;
                default:
                    return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
            }
        }

        public Property LookupProperty (string name)
        {
            Property prop;

            switch (name) {
                case "X":
                    prop = new NumberProperty("X", X);
                    prop.ValueChanged += PropertyXChanged;
                    return prop;
                case "Y":
                    prop = new NumberProperty("Y", Y);
                    prop.ValueChanged += PropertyYChanged;
                    return prop;
                case "Rotation":
                    prop = new NumberProperty("Rotation", Rotation);
                    prop.ValueChanged += PropertyRotationChanged;
                    return prop;

                default:
                    return _properties.Contains(name) ? _properties[name] : null;
            }
        }

        private void PropertyXChanged (object sender, EventArgs e)
        {
            NumberProperty property = sender as NumberProperty;
            X = (int)property.Value;
        }

        private void PropertyYChanged (object sender, EventArgs e)
        {
            NumberProperty property = sender as NumberProperty;
            Y = (int)property.Value;
        }

        private void PropertyRotationChanged (object sender, EventArgs e)
        {
            NumberProperty property = sender as NumberProperty;
            Rotation = property.Value;
        }

        #endregion

        public object Clone ()
        {
            return new ObjectInstance(this);
        }

        #region Serialization

        private int _classId;

        public void PreSerialize ()
        {
            _classId = _class.Id;
        }

        public void PostDeserialize (Project project)
        {
            ObjectPool pool = project.ObjectPoolManager.PoolFromItemKey(_classId);
            if (pool == null)
                throw new Exception("Invalid ObjectClass Id");

            _class = pool.GetObject(_classId);
            if (_class == null)
                throw new Exception("Invalid ObjectClass Id");
        }

        public ObjectInstance (SerializationInfo info, StreamingContext context)
        {
            _classId = info.GetInt32("ClassID");
            _posX = info.GetInt32("PosX");
            _posY = info.GetInt32("PosY");
            _rotation = info.GetSingle("Rotation");
            _scaleX = info.GetSingle("ScaleX");
            _scaleY = info.GetSingle("ScaleY");
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ClassID", _class.Id);
            info.AddValue("PosX", _posX);
            info.AddValue("PosY", _posY);
            info.AddValue("Rotation", _rotation);
            info.AddValue("ScaleX", _scaleX);
            info.AddValue("ScaleY", _scaleY);
        }

        #endregion
    }
}
