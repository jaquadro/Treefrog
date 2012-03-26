using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model.Support;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using System.Windows;
using Microsoft.Xna.Framework;

namespace Treefrog.V2.Controls.Layers
{
    public class XnaTileLayer : RenderLayer
    {
        private IEnumerable<LocatedTile> _tileSource;

        public IEnumerable<LocatedTile> TileSource
        {
            get { return _tileSource; }
            set { _tileSource = value; }
        }
    }

    public class ViewportVM : ViewModelBase
    {
        private Vector _offset;
        private Size _viewport;
        private float _zoomFactor = 1f;

        public Vector Offset
        {
            get { return _offset; }
            set { _offset = value; }
        }

        public Size Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        public Rect VisibleRegion
        {
            get 
            { 
                return new Rect(_offset.X, _offset.Y, 
                    _viewport.Width / _zoomFactor, _viewport.Height / _zoomFactor);
            }
        }
    }

    public class LevelLayerVM : ViewModelBase
    {
        private Layer _layer;
        private ViewportVM _viewport;

        public LevelLayerVM (Layer layer, ViewportVM viewport)
        {
            _layer = layer;
            _viewport = viewport ?? new ViewportVM()
            {
                Viewport = new Size(layer.LayerWidth, layer.LayerHeight),
            };
        }

        public LevelLayerVM (Layer layer)
            : this(layer, null)
        {
        }

        protected Layer Layer
        {
            get { return _layer; }
        }

        protected ViewportVM Viewport
        {
            get { return _viewport; }
        }

        public string LayerName
        {
            get { return _layer.Name; }
            set { _layer.Name = value; }
        }

        public bool IsVisible
        {
            get { return _layer.IsVisible; }
            set { _layer.IsVisible = value; }
        }

        public float Opacity
        {
            get { return _layer.Opacity; }
            set { _layer.Opacity = value; }
        }

        private void LayerNameChanged (object sender, NameChangedEventArgs e)
        {
            RaisePropertyChanged("LayerName");
        }

        private void LayerVisibilityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("IsVisible");
        }

        private void LayerOpacityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("Opacity");
        }
    }

    public class TileLayerVM : LevelLayerVM
    {
        public TileLayerVM (TileGridLayer layer, ViewportVM viewport)
            : base(layer, viewport)
        {
        }

        public TileLayerVM (TileGridLayer layer)
            : base(layer)
        {
        }

        protected new TileGridLayer Layer
        {
            get { return base.Layer as TileGridLayer; }
        }

        public IEnumerable<LocatedTile> TileSource
        {
            get
            {
                if (Layer == null)
                    yield break;

                Rect tileRegion = ComputeTileRegion();
                Rectangle castRegion = new Rectangle(
                    (int)Math.Floor(tileRegion.X),
                    (int)Math.Floor(tileRegion.Y),
                    (int)Math.Ceiling(tileRegion.Width),
                    (int)Math.Ceiling(tileRegion.Height));

                foreach (LocatedTile tile in Layer.TilesAt(castRegion))
                    yield return tile;
            }
        }

        private Rect ComputeTileRegion ()
        {
            int zoomTileWidth = (int)(Layer.TileWidth * Viewport.ZoomFactor);
            int zoomTileHeight = (int)(Layer.TileHeight * Viewport.ZoomFactor);

            Rect region = Viewport.VisibleRegion;

            Rect tileRegion = new Rect(
                region.X / Layer.TileWidth,
                region.Y / Layer.TileHeight,
                (int)(region.Width + region.X % Layer.TileWidth + Layer.TileWidth - 1) / Layer.TileWidth,
                (int)(region.Height + region.Y % Layer.TileHeight + Layer.TileHeight - 1) / Layer.TileHeight
                );

            return tileRegion;
        }
    }
}
