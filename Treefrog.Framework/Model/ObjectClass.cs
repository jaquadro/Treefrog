using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;
using System.Collections.Generic;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public class ObjectClass : INamedResource, INotifyPropertyChanged, IPropertyProvider
    {
        private class BatchEditBlock : ResourceReleaser
        {
            private ObjectClass _resource;

            public BatchEditBlock (ObjectClass resource)
            {
                _resource = resource;
                _resource.ModifyBlockCount++;
            }

            protected override void Dispose (bool disposing)
            {
                _resource.ModifyBlockCount--;
                if (_resource.ModifyBlockCount <= 0)
                    _resource.OnModified(EventArgs.Empty);

                base.Dispose(disposing);
            }
        }

        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(ObjectClass));

        private readonly Guid _uid;
        private readonly ResourceName _name;

        private ObjectPool _pool;

        private bool _canRotate;
        private bool _canScale;

        private TextureResource _image;

        private Point _origin;
        private Rectangle _maskBounds;
        private Rectangle _imageBounds;

        private PropertyManager _propertyManager;

        public ObjectClass (string name)
        {
            _uid = Guid.NewGuid();
            _name = new ResourceName(this, name);

            _origin = Point.Zero;

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);
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

        public ObjectClass (string name, ObjectClass template)
            : this(name)
        {
            if (template != null) {
                if (template.Image != null)
                    _image = template.Image.Crop(template.Image.Bounds);

                _canRotate = template._canRotate;
                _canScale = template._canScale;
                _imageBounds = template._imageBounds;
                _maskBounds = template._maskBounds;
                _origin = template._origin;

                foreach (var item in template.PropertyManager.CustomProperties)
                    PropertyManager.CustomProperties.Add(item.Clone() as Property);
            }
        }

        private ObjectClass (LibraryX.ObjectClassX proxy, TexturePool texturePool)
            : this(proxy.Name)
        {
            _uid = proxy.Uid;
            _image = texturePool.GetResource(proxy.Texture);
            _imageBounds = proxy.ImageBounds;
            _maskBounds = proxy.MaskBounds;
            _origin = proxy.Origin;

            if (proxy.Properties != null) {
                foreach (var propertyProxy in proxy.Properties)
                    PropertyManager.CustomProperties.Add(Property.FromXmlProxy(propertyProxy));
            }
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        internal int Version { get; private set; }

        public ObjectPool Pool
        {
            get { return _pool; }
            internal set 
            {
                if (_pool == value)
                    return;

                TexturePool oldTexturePool = null;
                if (_pool != null)
                    oldTexturePool = _pool.TexturePool;

                _pool = value;

                TexturePool newTexturePool = null;
                if (_pool != null)
                    newTexturePool = _pool.TexturePool;

                if (_image != null) {
                    if (newTexturePool != null)
                        newTexturePool.AddResource(_image);
                    if (oldTexturePool != null)
                        oldTexturePool.RemoveResource(_image.Uid);
                }
            }
        }

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
                    Version++;
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
                    Version++;
                    RaisePropertyChanged("Origin");
                }
            }
        }

        public Guid ImageId
        {
            get { return _image != null ? _image.Uid : Guid.Empty; }
        }

        public TextureResource Image
        {
            get { return _image; }
            set
            {
                if (_image != value) {
                    if (_image == null)
                        _pool.TexturePool.AddResource(value);
                    else if (value == null)
                        _pool.TexturePool.RemoveResource(_image.Uid);
                    else {
                        _pool.TexturePool.RemoveResource(_image.Uid);
                        _pool.TexturePool.AddResource(value);
                    }

                    _image = value;
                    Version++;
                    if (_image == null) {
                        _imageBounds = Rectangle.Empty;
                        RaisePropertyChanged("ImageBounds");
                    }
                    else if (_imageBounds.Width != _image.Width || _imageBounds.Height != _image.Height) {
                        _imageBounds = _image.Bounds;
                        RaisePropertyChanged("ImageBounds");
                    }
                    RaisePropertyChanged("Image");
                }
            }
        }

        private int ModifyBlockCount { get; set; }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (var property in PropertyManager.CustomProperties)
                property.ResetModified();
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            IsModified = true;
            if (ModifyBlockCount <= 0) {
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        public ResourceReleaser BeginModify ()
        {
            return new BatchEditBlock(this);
        }

        #region Name Interface

        public event EventHandler<NameChangingEventArgs> NameChanging
        {
            add { _name.NameChanging += value; }
            remove { _name.NameChanging -= value; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            add { _name.NameChanged += value; }
            remove { _name.NameChanged -= value; }
        }

        [SpecialProperty]
        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

        #region IPropertyProvider Members

        public string PropertyProviderName
        {
            get { return Name; }
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged;

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        private void NamePropertyChangedHandler (object sender, EventArgs e)
        {
            StringProperty property = sender as StringProperty;
            TrySetName(property.Value);
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
            OnModified(EventArgs.Empty);
        }

        #endregion

        public static LibraryX.ObjectClassX ToXProxy (ObjectClass objClass)
        {
            if (objClass == null)
                return null;

            List<CommonX.PropertyX> props = new List<CommonX.PropertyX>();
            foreach (Property prop in objClass.PropertyManager.CustomProperties)
                props.Add(Property.ToXmlProxyX(prop));

            return new LibraryX.ObjectClassX() {
                Uid = objClass.Uid,
                Name = objClass.Name,
                Texture = objClass._image != null ? objClass._image.Uid : Guid.Empty,
                ImageBounds = objClass._imageBounds,
                MaskBounds = objClass._maskBounds,
                Origin = objClass._origin,
                Properties = props.Count > 0 ? props : null,
            };
        }

        public static ObjectClass FromXProxy (LibraryX.ObjectClassX proxy, TexturePool texturePool)
        {
            if (proxy == null)
                return null;

            return new ObjectClass(proxy, texturePool);
        }
    }
}
