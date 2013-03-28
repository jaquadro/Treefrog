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
        private Guid _id;
        private string _name;

        private int _tileHeight;
        private int _tileWidth;

        protected TileBrush (string name, int tileWidth, int tileHeight)
        {
            _name = name;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        protected TileBrush (string name, int tileWidth, int tileHeight, Guid uid)
            : this(name, tileWidth, tileHeight)
        {
            _id = uid;
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

        public Guid Uid
        {
            get { return _id; }
            set { _id = value; }
        }

        public string GetKey ()
        {
            return Name;
        }

        public event EventHandler<NameChangingEventArgs> NameChanging;
        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            var ev = NameChanging;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            var ev = NameChanged;
            if (ev != null)
                ev(this, e);
        }

        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public bool TrySetName (string name)
        {
            if (Name != name) {
                try {
                    OnNameChanging(new NameChangingEventArgs(Name, name));
                }
                catch (NameChangeException) {
                    return false;
                }

                NameChangedEventArgs e = new NameChangedEventArgs(Name, name);
                Name = name;

                OnNameChanged(e);
                OnModified(EventArgs.Empty);
            }

            return true;
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if ((ev = Modified) != null)
                ev(this, e);
        }

        public abstract void ApplyBrush (TileGridLayer tileLayer, int x, int y);

        public abstract TextureResource MakePreview ();

        public abstract TextureResource MakePreview (int maxWidth, int maxHeight);

        public virtual void Normalize ()
        { }
    }
}
