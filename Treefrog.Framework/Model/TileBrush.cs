using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework.Model
{
    public class TileBrushRegistry : TypeRegistry<TileBrush>
    {
        public TileBrushRegistry ()
        {
            Register("Static Brush", typeof(StaticTileBrush));
            Register("Dynamic Brush", typeof(DynamicTileBrush));
        }
    }

    public abstract class TileBrush : INamedResource
    {
        private readonly Guid _uid;
        private readonly ResourceName _name;

        private int _tileHeight;
        private int _tileWidth;

        protected TileBrush (Guid uid, string name, int tileWidth, int tileHeight)
        {
            _uid = uid;
            _name = new ResourceName(this, name);

            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public virtual int TilesHigh
        {
            get { return 1; }
        }

        public virtual int TilesWide
        {
            get { return 1; }
        }

        public abstract void ApplyBrush (TileGridLayer tileLayer, int x, int y);

        public abstract TextureResource MakePreview ();

        public abstract TextureResource MakePreview (int maxWidth, int maxHeight);

        public virtual void Normalize ()
        { }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if (ev != null)
                ev(this, e);
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
    }
}
