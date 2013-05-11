using System;
using System.Collections.Generic;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model.Collections;

namespace Treefrog.Framework.Model
{
    public abstract class Tile : IResource, IPropertyProvider
    {
        private static PropertyClassManager _propertyClassManager = new PropertyClassManager(typeof(Tile));

        private readonly Guid _uid;

        private TilePool _pool;
        private PropertyManager _propertyManager;

        protected List<DependentTile> _dependents;

        protected Tile ()
        {
            _uid = Guid.NewGuid();
            _dependents = new List<DependentTile>();

            _propertyManager = new PropertyManager(_propertyClassManager, this);
            _propertyManager.CustomProperties.Modified += (s, e) => OnModified(EventArgs.Empty);
        }

        protected Tile (Guid uid)
            : this()
        {
            _uid = uid;
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public TilePool Pool
        {
            get { return _pool; }
            internal set { _pool = value; }
        }

        public int Height
        {
            get { return _pool.TileHeight; }
        }

        public int Width
        {
            get { return _pool.TileWidth; }
        }

        public event EventHandler TextureModified;

        protected virtual void OnTextureModified (EventArgs e)
        {
            var ev = TextureModified;
            if (ev != null)
                ev(this, e);

            OnModified(e);
        }

        public virtual void Update (TextureResource textureData)
        {
            foreach (DependentTile tile in _dependents)
                tile.UpdateFromBase(textureData);

            OnTextureModified(EventArgs.Empty);
        }

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (Property prop in PropertyManager.CustomProperties)
                prop.ResetModified();
        }

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified;

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        #region IPropertyProvider Members

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public string PropertyProviderName
        {
            get { return "Tile " + _uid; }
        }

        public PropertyManager PropertyManager
        {
            get { return _propertyManager; }
        }

        #endregion
    }

    public class PhysicalTile : Tile
    {
        public PhysicalTile ()
            : base()
        { }

        public PhysicalTile (Guid uid)
            : base(uid)
        { }

        public override void Update (TextureResource textureData)
        {
            Pool.Tiles.SetTileTexture(Uid, textureData);
            base.Update(textureData);
        }
    }

    public class DependentTile : Tile
    {
        Tile _base;
        TileTransform _transform;

        public DependentTile (Guid uid, Tile baseTile, TileTransform xform)
            : base(uid)
        {
            _base = baseTile;
            _transform = xform;
        }

        public override void Update (TextureResource textureData)
        {
            _base.Update(_transform.InverseTransform(textureData, Pool.TileWidth, Pool.TileHeight));
        }

        public virtual void UpdateFromBase (TextureResource textureData)
        {
            TextureResource xform = _transform.Transform(textureData, Pool.TileWidth, Pool.TileHeight);
            Pool.Tiles.SetTileTexture(Uid, xform);
        }
    }
}