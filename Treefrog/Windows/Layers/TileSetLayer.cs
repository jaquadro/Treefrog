using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Layers;

namespace Treefrog.Windows.Layers
{
    public class TileSetLayer : RenderLayer
    {
        private TileSetLayerPresenter _model;

        public new TileSetLayerPresenter Model
        {
            get { return _model; }
            set
            {
                _model = value;
                base.Model = value;
            }
        }

        public override int DependentHeight
        {
            get
            {
                int width = LevelGeometry.ViewportBounds.Width;
                int tilesWide = Math.Max(1, width / _model.TileWidth);
                return ((_model.Layer.Count + tilesWide - 1) / tilesWide) * _model.TileHeight;
            }
        }

        public override int DependentWidth
        {
            get
            {
                int height = LevelGeometry.ViewportBounds.Height;
                int tilesHigh = Math.Max(1, height / _model.TileHeight);
                return ((_model.Layer.Count + tilesHigh - 1) / tilesHigh) * _model.TileWidth;
            }
        }
    }
}
