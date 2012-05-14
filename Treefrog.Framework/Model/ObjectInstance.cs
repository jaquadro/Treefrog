using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class ObjectInstance : IPropertyProvider, ICloneable
    {
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
    }
}
