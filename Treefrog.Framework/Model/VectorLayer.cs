using System;
using System.Collections.Generic;
using System.Text;
using Treefrog.Framework.Imaging;
using System.Runtime.Serialization;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    /*public class VectorShape : IResource, IPropertyProvider, ICloneable, ISerializable
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(VectorShape));

        private readonly Guid _uid;

        private int _posX;
        private int _posY;
        private float _rotation;
        private float _scaleX;
        private float _scaleY;

        private PropertyManager _propertyManager;

        public VectorShape (int posX, int posY)
        {
            _uid = Guid.NewGuid();
            _posX = posX;
            _posY = posY;
            _rotation = 0;
            _scaleX = 1f;
            _scaleY = 1f;

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        public VectorShape ()
            : this(0, 0)
        { }

        public VectorShape (VectorShape shape)
            : this(shape._posX, shape._posY)
        {
            _rotation = shape._rotation;
            _scaleX = shape._scaleX;
            _scaleY = shape._scaleY;

            foreach (Property prop in shape.PropertyManager.CustomProperties) {
                PropertyManager.CustomProperties.Add(prop.Clone() as Property);
            }
        }

        private VectorShape (LevelX.VectorShapeX proxy)
            : this(proxy.X, proxy.Y)
        {
            _uid = proxy.Uid;
            _rotation = MathEx.DegToRad(proxy.Rotation);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    PropertyManager.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            //UpdateBounds();
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        [SpecialProperty]
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

        [SpecialProperty]
        public int Y
        {
            get { return _posY; }
            set
            {
                if (_posY != value) {
                    _posY = value;
                    OnPositionChanged(EventArgs.Empty);
                }
            }
        }

        [SpecialProperty(Converter = typeof(RadToDegPropertyConverter))]
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                value = value % (float)(Math.PI * 2);
                if (value < 0)
                    value += (float)(Math.PI * 2);

                if (_rotation != value) {
                    _rotation = value;

                    //UpdateBounds();
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

        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;

        protected virtual void OnPositionChanged (EventArgs e)
        {
            var ev = PositionChanged;
            if (ev != null)
                ev(this, e);
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnRotationChanged (EventArgs e)
        {
            var ev = RotationChanged;
            if (ev != null)
                ev(this, e);
            OnModified(EventArgs.Empty);
        }

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return "Shape#"; }
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
        }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var property in PropertyManager.CustomProperties)
                property.ResetModified();
        }

        public event EventHandler Modified;

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            var ev = PropertyProviderNameChanged;
            if (ev != null)
                ev(this, e);
        }

        #endregion

        #region Serialization

        public VectorShape (SerializationInfo info, StreamingContext context)
        {
            _uid = (Guid)info.GetValue("Uid", typeof(Guid));
            _posX = info.GetInt32("PosX");
            _posY = info.GetInt32("PosY");
            _rotation = info.GetSingle("Rotation");
            _scaleX = info.GetSingle("ScaleX");
            _scaleY = info.GetSingle("ScaleY");

            _propertyManager = new Framework.PropertyManager(_propertyClassManager, this);

            PropertyCollection props = info.GetValue("Properties", typeof(PropertyCollection)) as PropertyCollection;
            foreach (Property p in props)
                _propertyManager.CustomProperties.Add(p.Clone() as Property);
        }

        public void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Uid", _uid);
            info.AddValue("PosX", _posX);
            info.AddValue("PosY", _posY);
            info.AddValue("Rotation", _rotation);
            info.AddValue("ScaleX", _scaleX);
            info.AddValue("ScaleY", _scaleY);

            info.AddValue("Properties", PropertyManager.CustomProperties, typeof(PropertyCollection));
        }

        #endregion

        public static LevelX.VectorShapeX ToXProxy (VectorShape shape)
        {
            if (shape == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in shape.PropertyManager.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LevelX.VectorShapeX() {
                Uid = shape.Uid,
                X = shape.X,
                Y = shape.Y,
                Rotation = MathEx.RadToDeg(shape.Rotation),
                Properties = (props.Count > 0) ? props : null,
            };
        }

        public static VectorShape FromXProxy (LevelX.VectorShapeX proxy)
        {
            if (proxy == null)
                return null;

            //ObjectPool pool = manager.PoolFromItemKey(proxy.Class);
            //if (pool == null)
            //    return null;

            return new VectorShape(proxy);
        }
    }

    public class VectorLayer : Layer
    {
        public VectorLayer (string name, VectorLayer layer)
            : base(name, layer)
        {

        }

        public override object Clone ()
        {
            return new VectorLayer(Name, this);
        }
    }*/
}
