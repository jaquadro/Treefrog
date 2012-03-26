using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework.Model
{
    public class ObjectInstance : IPropertyProvider
    {
        private int _posX;
        private int _posY;
        private float _rotation;
        private float _scaleX;
        private float _scaleY;

        public ObjectInstance ()
        {
            _rotation = 0;
            _scaleX = 1f;
            _scaleY = 1f;
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
        }

        public int Y
        {
            get { return _posY; }
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
    }
}
