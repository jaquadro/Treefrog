using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    public class SystemProperty : Attribute
    {

    }

    public class ObjectClassXmlProxy
    {
        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlElement]
        public Rectangle ImageBounds { get; set; }

        [XmlElement]
        public Rectangle MaskBounds { get; set; }

        [XmlElement]
        public Point Origin { get; set; }

        [XmlElement]
        public TextureResource.XmlProxy Image { get; set; }
    }

    /* INamedResource, IPropertyProvider */
    public class ObjectClass : IKeyProvider<string>, INotifyPropertyChanged, IPropertyProvider
    {
        private static string[] _reservedPropertyNames = new string[] { "Name", "Width", "Height", "OriginX", "OriginY" };

        private int _id;
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

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [SystemProperty]
        public bool CanRotate
        {
            get { return _canRotate; }
            set
            {
                if (_canRotate != value) {
                    _canRotate = value;
                    RaisePropertyChanged("CanRotate");
                }
            }
        }

        [SystemProperty]
        public bool CanScale
        {
            get { return _canScale; }
            set
            {
                if (_canScale != value) {
                    _canScale = value;
                    RaisePropertyChanged("CanScale");
                }
            }
        }

        public Rectangle ImageBounds
        {
            get { return _imageBounds; }
        }

        public Rectangle MaskBounds
        {
            get { return _maskBounds; }
            set {
                if (_maskBounds != value) {
                    _maskBounds = value;
                    RaisePropertyChanged("MaskBounds");
                }
            }
        }

        public Point Origin
        {
            get { return _origin; }
            set
            {
                if (_origin != value) {
                    _origin = value;
                    RaisePropertyChanged("Origin");
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

                    if (_imageBounds.Width != _image.Width || _imageBounds.Height != _image.Height) {
                        _imageBounds = _image.Bounds;
                        RaisePropertyChanged("ImageBounds");
                    }
                    RaisePropertyChanged("Image");
                }
            }
        }

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;
        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        public string GetKey ()
        {
            return Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
                OnPropertyProviderNameChanged(EventArgs.Empty);
                RaisePropertyChanged("Name");
            }

            return true;
        }

        /*#region INamedResource Members

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

        #endregion*/


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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void RaisePropertyChanged (string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion

        public static ObjectClassXmlProxy ToXmlProxy (ObjectClass objClass)
        {
            if (objClass == null)
                return null;

            return new ObjectClassXmlProxy()
            {
                Id = objClass.Id,
                Name = objClass._name,
                ImageBounds = objClass._imageBounds,
                MaskBounds = objClass._maskBounds,
                Origin = objClass._origin,
                Image = TextureResource.ToXmlProxy(objClass._image),
            };
        }

        public static ObjectClass FromXmlProxy (ObjectClassXmlProxy proxy)
        {
            if (proxy == null)
                return null;

            ObjectClass objClass = new ObjectClass(proxy.Name);
            objClass._id = proxy.Id;
            objClass._image = TextureResource.FromXmlProxy(proxy.Image);
            objClass._imageBounds = proxy.ImageBounds;
            objClass._maskBounds = proxy.MaskBounds;
            objClass._origin = proxy.Origin;

            return objClass;
        }
    }
}
