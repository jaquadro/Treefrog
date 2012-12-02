using System;
using System.Runtime.Serialization;
using Treefrog.Framework.Imaging;

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
        }

        public ObjectInstance (ObjectClass objClass)
            : this(objClass, 0, 0)
        {
        }

        public ObjectInstance (ObjectInstance inst)
        {
            _class = inst._class;
            _posX = inst._posX;
            _posY = inst._posY;
            _rotation = inst._rotation;
            _scaleX = inst._scaleX;
            _scaleY = inst._scaleY;
        }

        public ObjectClass ObjectClass
        {
            get { return _class; }
        }

        public float Rotation
        {
            get { return _rotation; }
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
            set { _posX = value; }
        }

        public int Y
        {
            get { return _posY; }
            set { _posY = value; }
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

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public Collections.PropertyCollection CustomProperties
        {
            get { throw new NotImplementedException(); }
        }

        public Collections.PredefinedPropertyCollection PredefinedProperties
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            throw new NotImplementedException();
        }

        public Property LookupProperty (string name)
        {
            throw new NotImplementedException();
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
