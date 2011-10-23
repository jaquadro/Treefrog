using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Security.Cryptography;

namespace Treefrog.Framework.Model
{
    public abstract class Tile
    {
        protected TilePool _pool;
        private int _id;
        private NamedResourceCollection<Property> _properties;

        protected List<DependentTile> _dependents;

        protected Tile (int id, TilePool pool)
        {
            _id = id;
            _pool = pool;
            _properties = new NamedResourceCollection<Property>();
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

        public NamedResourceCollection<Property> Properties
        {
            get { return _properties; }
        }

        public virtual void Update (byte[] textureData)
        {
            foreach (DependentTile tile in _dependents) {
                tile.UpdateFromBase(textureData);
            }
        }

        public abstract void Draw (SpriteBatch spritebatch, Rectangle dest);
    }

    public class PhysicalTile : Tile
    {
        public PhysicalTile (int id, TilePool pool)
            : base(id, pool)
        { }

        public override void Update (byte[] textureData)
        {
            _pool.SetTileTextureData(Id, textureData);
            base.Update(textureData);
        }

        public override void Draw (SpriteBatch spritebatch, Rectangle dest)
        {
            _pool.DrawTile(spritebatch, Id, dest);
        }
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

        public override void Update (byte[] textureData)
        {
            _base.Update(_transform.InverseTransform(textureData, _pool.TileWidth, _pool.TileHeight));
        }

        public virtual void UpdateFromBase (byte[] textureData)
        {
            byte[] xform = _transform.Transform(textureData, _pool.TileWidth, _pool.TileHeight);
            _pool.SetTileTextureData(Id, xform);
        }

        public override void Draw (SpriteBatch spritebatch, Rectangle dest)
        {
            _pool.DrawTile(spritebatch, Id, dest);
        }
    }
}