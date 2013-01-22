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

        private Rectangle _maskRotatedBounds;
        private Rectangle _imageRotatedBounds;

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

            UpdateBounds();
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

                    UpdateBounds();
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
                    _posX + _imageRotatedBounds.Left,
                    _posY + _imageRotatedBounds.Top,
                    _imageRotatedBounds.Width, _imageRotatedBounds.Height);
            }
        }

        public Rectangle MaskBounds
        {
            get
            {
                return new Rectangle(
                    _posX + _maskRotatedBounds.Left,
                    _posY + _maskRotatedBounds.Top,
                    _maskRotatedBounds.Width, _maskRotatedBounds.Height);
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

        private void UpdateBounds ()
        {
            _imageRotatedBounds = CalculateRectangleBounds(new Rectangle(
                _class.ImageBounds.Left - _class.Origin.X, _class.ImageBounds.Top - _class.Origin.Y,
                _class.ImageBounds.Width, _class.ImageBounds.Height), _rotation);

            _maskRotatedBounds = CalculateRectangleBounds(new Rectangle(
                _class.MaskBounds.Left - _class.Origin.X, _class.MaskBounds.Top - _class.Origin.Y,
                _class.MaskBounds.Width, _class.MaskBounds.Height), _rotation);
        }

        private static Rectangle CalculateRectangleBounds (Rectangle rect, float angle)
        {
            float st = (float)Math.Sin(angle);
            float ct = (float)Math.Cos(angle);

            float x1 = rect.Left * ct - rect.Top * st;
            float y1 = rect.Left * st + rect.Top * ct;
            float x2 = rect.Right * ct - rect.Top * st;
            float y2 = rect.Right * st + rect.Top * ct;
            float x3 = rect.Left * ct - rect.Bottom * st;
            float y3 = rect.Left * st + rect.Bottom * ct;
            float x4 = rect.Right * ct - rect.Bottom * st;
            float y4 = rect.Right * st + rect.Bottom * ct;

            int xmin = (int)Math.Floor(Math.Min(x1, Math.Min(x2, Math.Min(x3, x4))));
            int xmax = (int)Math.Ceiling(Math.Max(x1, Math.Max(x2, Math.Max(x3, x4))));
            int ymin = (int)Math.Floor(Math.Min(y1, Math.Min(y2, Math.Min(y3, y4))));
            int ymax = (int)Math.Ceiling(Math.Max(y1, Math.Max(y2, Math.Max(y3, y4))));

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
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
