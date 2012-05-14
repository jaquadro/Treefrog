using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Security.Cryptography;
using Treefrog.Framework.Model.Collections;
using Treefrog.Framework.Imaging;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    public abstract class Tile : IPropertyProvider
    {
        protected TilePool _pool;
        private int _id;
        private PropertyCollection _properties;
        private TileProperties _predefinedProperties;

        protected List<DependentTile> _dependents;

        protected Tile (int id, TilePool pool)
        {
            _id = id;
            _pool = pool;
            _dependents = new List<DependentTile>();

            _properties = new PropertyCollection(new string[0]);
            _predefinedProperties = new Tile.TileProperties(this);

            _properties.Modified += CustomProperties_Modified;
        }

        public int Id
        {
            get { return _id; }
        }

        public TilePool Pool
        {
            get { return _pool; }
        }

        public int Height
        {
            get { return _pool.TileHeight; }
        }

        public int Width
        {
            get { return _pool.TileWidth; }
        }

        //public NamedResourceCollection<Property> Properties
        //{
        //    get { return _properties; }
        //}

        public event EventHandler TextureModified = (s, e) => { };

        protected virtual void OnTextureModified (EventArgs e)
        {
            TextureModified(this, e);
            OnModified(e);
        }

        public virtual void Update (TextureResource textureData)
        {
            foreach (DependentTile tile in _dependents) {
                tile.UpdateFromBase(textureData);
            }
            OnTextureModified(EventArgs.Empty);
        }

        /*public virtual void Draw (SpriteBatch spritebatch, Rectangle dest)
        {
            Draw(spritebatch, dest, Color.White);
        }

        public abstract void Draw (SpriteBatch spritebatch, Rectangle dest, Color color);*/

        #region Events

        /// <summary>
        /// Occurs when the internal state of the Layer is modified.
        /// </summary>
        public event EventHandler Modified;

        #endregion

        #region Event Dispatchers

        /// <summary>
        /// Raises the <see cref="Modified"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnModified (EventArgs e)
        {
            if (Modified != null) {
                Modified(this, e);
            }
        }

        #endregion

        private void CustomProperties_Modified (object sender, EventArgs e)
        {
            OnModified(e);
        }

        #region IPropertyProvider Members

        private class TileProperties : PredefinedPropertyCollection
        {
            private Tile _parent;

            public TileProperties (Tile parent)
                : base(new string[0])
            {
                _parent = parent;
            }

            protected override IEnumerable<Property> PredefinedProperties ()
            {
                yield break;
            }

            protected override Property LookupProperty (string name)
            {
                return _parent.LookupProperty(name);
            }
        }

        public event EventHandler<EventArgs> PropertyProviderNameChanged = (s, e) => { };

        protected virtual void OnPropertyProviderNameChanged (EventArgs e)
        {
            PropertyProviderNameChanged(this, e);
        }

        public string PropertyProviderName
        {
            get { return "Tile " + _id; }
        }

        public PredefinedPropertyCollection PredefinedProperties
        {
            get { return _predefinedProperties; }
        }

        public PropertyCollection CustomProperties
        {
            get { return _properties; }
        }

        public PropertyCategory LookupPropertyCategory (string name)
        {
            return _properties.Contains(name) ? PropertyCategory.Custom : PropertyCategory.None;
        }

        public Property LookupProperty (string name)
        {
            return _properties.Contains(name) ? _properties[name] : null;
        }

        //public void AddCustomProperty (Property property)
        //{
        //    if (property == null) {
        //        throw new ArgumentNullException("The property is null.");
        //    }
        //    if (_properties.Contains(property.Name)) {
        //        throw new ArgumentException("A property with the same name already exists.");
        //    }

        //    _properties.Add(property);
        //}

        //public void RemoveCustomProperty (string name)
        //{
        //    if (name == null) {
        //        throw new ArgumentNullException("The name is null.");
        //    }

        //    _properties.Remove(name);
        //}

        #endregion
    }

    public class PhysicalTile : Tile
    {
        public PhysicalTile (int id, TilePool pool)
            : base(id, pool)
        { }

        public override void Update (TextureResource textureData)
        {
            _pool.SetTileTexture(Id, textureData);
            base.Update(textureData);
        }

        /*public override void Draw (SpriteBatch spritebatch, Rectangle dest, Color color)
        {
            _pool.DrawTile(spritebatch, Id, dest, color);
        }*/
    }

    public class DependentTile : Tile
    {
        Tile _base;
        TileTransform _transform;

        public DependentTile (int id, TilePool pool, Tile baseTile, TileTransform xform)
            : base(id, pool)
        {
            _base = baseTile;
            _transform = xform;
        }

        public override void Update (TextureResource textureData)
        {
            _base.Update(_transform.InverseTransform(textureData, _pool.TileWidth, _pool.TileHeight));
        }

        public virtual void UpdateFromBase (TextureResource textureData)
        {
            TextureResource xform = _transform.Transform(textureData, _pool.TileWidth, _pool.TileHeight);
            _pool.SetTileTexture(Id, xform);
        }

        /*public override void Draw (SpriteBatch spritebatch, Rectangle dest, Color color)
        {
            _pool.DrawTile(spritebatch, Id, dest, color);
        }*/
    }
}