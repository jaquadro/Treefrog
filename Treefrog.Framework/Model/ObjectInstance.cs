﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    [Serializable]
    public class ObjectInstance : IResource, IPropertyProvider, ICloneable, ISerializable
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(ObjectInstance));

        private readonly Guid _uid;

        [NonSerialized]
        private ObjectClass _class;

        [NonSerialized]
        private int _classVersion;

        private int _posX;
        private int _posY;
        private float _rotation;
        private float _scaleX;
        private float _scaleY;

        private PropertyManager _propertyManager;

        private Rectangle _maskRotatedBounds;
        private Rectangle _imageRotatedBounds;

        public ObjectInstance (ObjectClass objClass, int posX, int posY)
        {
            _uid = Guid.NewGuid();
            _class = objClass;
            _posX = posX;
            _posY = posY;
            _rotation = 0;
            _scaleX = 1f;
            _scaleY = 1f;

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.PropertyParent = objClass;
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);

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

            foreach (Property prop in inst.PropertyManager.CustomProperties) {
                PropertyManager.CustomProperties.Add(prop.Clone() as Property);
            }

            UpdateBounds();
        }

        private ObjectInstance (LevelX.ObjectInstanceX proxy, ObjectClass objClass)
            : this(objClass, proxy.X, proxy.Y)
        {
            _uid = proxy.Uid;
            _rotation = MathEx.DegToRad(proxy.Rotation);

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    PropertyManager.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }

            UpdateBounds();
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public ObjectClass ObjectClass
        {
            get { return _class; }
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
            set {
                if (_posY != value) {
                    _posY = value;
                    OnPositionChanged(EventArgs.Empty);
                }            
            }
        }

        [SpecialProperty(Converter=typeof(RadToDegPropertyConverter))]
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
                CheckUpdateBounds();
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
                CheckUpdateBounds();
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
            OnModified(EventArgs.Empty);
        }

        protected virtual void OnRotationChanged (EventArgs e)
        {
            var ev = RotationChanged;
            if (ev != null)
                ev(this, e);
            OnModified(EventArgs.Empty);
        }

        private void CheckUpdateBounds ()
        {
            if (_classVersion < _class.Version)
                UpdateBounds();
        }

        private void UpdateBounds ()
        {
            _classVersion = _class.Version;

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

        public string PropertyProviderName
        {
            get { return "Object#"; }
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

        public object Clone ()
        {
            return new ObjectInstance(this);
        }

        #region Serialization

        private Guid _classId;

        public void PreSerialize ()
        {
            _classId = _class.Uid;
        }

        public void PostDeserialize (Project project)
        {
            ObjectPool pool = project.ObjectPoolManager.PoolFromItemKey(_classId);
            if (pool == null)
                throw new Exception("Invalid ObjectClass Id");

            _class = pool.GetObject(_classId);
            if (_class == null)
                throw new Exception("Invalid ObjectClass Id");

            UpdateBounds();
        }

        public ObjectInstance (SerializationInfo info, StreamingContext context)
        {
            _uid = (Guid)info.GetValue("Uid", typeof(Guid));
            _classId = (Guid)info.GetValue("ClassID", typeof(Guid));
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
            info.AddValue("ClassID", _class.Uid);
            info.AddValue("PosX", _posX);
            info.AddValue("PosY", _posY);
            info.AddValue("Rotation", _rotation);
            info.AddValue("ScaleX", _scaleX);
            info.AddValue("ScaleY", _scaleY);

            info.AddValue("Properties", PropertyManager.CustomProperties, typeof(PropertyCollection));
        }

        #endregion

        public static LevelX.ObjectInstanceX ToXProxy (ObjectInstance inst)
        {
            if (inst == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in inst.PropertyManager.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LevelX.ObjectInstanceX() {
                Uid = inst.Uid,
                Class = inst.ObjectClass.Uid,
                X = inst.X,
                Y = inst.Y,
                Rotation = MathEx.RadToDeg(inst.Rotation),
                Properties = (props.Count > 0) ? props : null,
            };
        }

        public static ObjectInstance FromXProxy (LevelX.ObjectInstanceX proxy, IObjectPoolManager manager)
        {
            if (proxy == null)
                return null;

            ObjectPool pool = manager.PoolFromItemKey(proxy.Class);
            if (pool == null)
                return null;

            ObjectClass objClass = pool.Objects[proxy.Class];
            if (objClass == null)
                return null;

            return new ObjectInstance(proxy, objClass);
        }
    }
}
