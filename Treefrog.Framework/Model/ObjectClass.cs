using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class ObjectClass : INamedResource, IPropertyProvider
    {
        private static string[] _reservedPropertyNames = new string[] { "Name", "Width", "Height", "OriginX", "OriginY" };

        private string _name;

        private bool _canRotate;
        private bool _canScale;

        private int _width;
        private int _height;
        private TextureResource _image;
        private int _originX;
        private int _originY;

        private Point _origin;
        private Rectangle _maskBounds;
        private Rectangle _imageBounds;

        public ObjectClass (string name)
        {
            _name = name;
            _origin = Point.Zero;
        }

        public ObjectClass (string name, TextureResource image)
            : this(name)
        {
            _image = image;
            _imageBounds = image.Bounds;
            _maskBounds = image.Bounds;
        }

        public ObjectClass (string name, TextureResource image, Rectangle maskBounds)
            : this(name, image)
        {
            _maskBounds = maskBounds;
        }

        public ObjectClass (string name, TextureResource image, Rectangle maskBounds, Point origin)
            : this(name, image, maskBounds)
        {
            _origin = origin;
        }

        public bool CanRotate
        {
            get { return _canRotate; }
            set { _canRotate = value; }
        }

        public bool CanScale
        {
            get { return _canScale; }
            set { _canScale = value; }
        }

        //public int ImageHeight
        //{
        //    get { return _imageBounds.Height; }
        //}

        //public int ImageWidth
        //{
        //    get { return _imageBounds.Width; }
        //}

        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        public Rectangle MaskBounds
        {
            get { return _maskBounds; }
            set { _maskBounds = value; }
        }

        public Point Origin
        {
            get { return _origin; }
            set
            {
                if (_origin.X != value.X || _origin.Y != value.Y) {
                    _origin = value;

                    OnOriginChanged(EventArgs.Empty);
                    OnModified(EventArgs.Empty);
                }
            }
        }

        public TextureResource Image
        {
            get { return _image; }
            set
            {
                if (_image != value) {
                    _image = value;

                    OnImageChanged(EventArgs.Empty);

                    if (_imageBounds.Width != _image.Width || _imageBounds.Height != _image.Height) {
                        _imageBounds = _image.Bounds;

                        OnSizeChanged(EventArgs.Empty);
                    }

                    OnModified(EventArgs.Empty);
                }
            }
        }

        public event EventHandler SizeChanged = (s, e) => { };

        public event EventHandler OriginChanged = (s, e) => { };

        public event EventHandler ImageChanged = (s, e) => { };

        protected virtual void OnSizeChanged (EventArgs e)
        {
            SizeChanged(this, e);
        }

        protected virtual void OnOriginChanged (EventArgs e)
        {
            OriginChanged(this, e);
        }

        protected virtual void OnImageChanged (EventArgs e)
        {
            ImageChanged(this, e);
        }

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value) {
                    NameChangingEventArgs ea = new NameChangingEventArgs(_name, value);
                    OnNameChanging(ea);
                    if (ea.Cancel)
                        return;

                    string oldName = _name;
                    _name = value;

                    OnNameChanged(new NameChangedEventArgs(oldName, _name));
                    OnPropertyProviderNameChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler<NameChangingEventArgs> NameChanging = (s, e) => { };

        public event EventHandler<NameChangedEventArgs> NameChanged = (s, e) => { };

        public event EventHandler Modified;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            NameChanging(this, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            NameChanged(this, e);
        }

        protected virtual void OnModified (EventArgs e)
        {
            Modified(this, e);
        }

        #endregion


        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return _name; }
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
