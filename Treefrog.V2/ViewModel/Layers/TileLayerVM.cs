using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Xna.Framework;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model;
using Treefrog.Framework;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using Treefrog.V2.ViewModel.Tools;
using Treefrog.V2.ViewModel.Menu;

namespace Treefrog.V2.ViewModel.Layers
{
    public class TileLayerVM : LevelLayerVM
    {
        private TilePoolManager _manager;
        private ObservableDictionary<string, TextureResource> _textures;

        public TileLayerVM (LevelDocumentVM level, TileGridLayer layer, ViewportVM viewport)
            : base(level, layer, viewport)
        {
            Initialize();
        }

        public TileLayerVM (LevelDocumentVM level, TileGridLayer layer)
            : base(level, layer)
        {
            Initialize();
        }

        private void Initialize ()
        {
            _textures = new ObservableDictionary<string, TextureResource>();

            if (Layer != null && Layer.Level != null && Layer.Level.Project != null) {
                _manager = Layer.Level.Project.TilePoolManager;

                foreach (TilePool pool in _manager.Pools)
                    _textures[pool.Name] = pool.TileSource;
            }

            _currentTool = new TileDrawTool(Level.CommandHistory, Layer as MultiTileGridLayer, Level.Annotations);
        }

        protected new TileGridLayer Layer
        {
            get { return base.Layer as TileGridLayer; }
        }

        public override Vector GetCoordinates (double x, double y)
        {
            Vector pxc = base.GetCoordinates(x, y);
            pxc.X = (int)(pxc.X / Layer.TileWidth);
            pxc.Y = (int)(pxc.Y / Layer.TileHeight);

            return pxc;
        }

        public override IEnumerable<DrawCommand> RenderCommands
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

                foreach (LocatedTile tile in Layer.TilesAt(castRegion)) {
                    TileCoord scoord = tile.Tile.Pool.GetTileLocation(tile.Tile.Id);
                    yield return new DrawCommand()
                    {
                        Texture = tile.Tile.Pool.Name,
                        SourceRect = new Rectangle(scoord.X * tile.Tile.Width, scoord.Y * tile.Tile.Height, tile.Tile.Width, tile.Tile.Height),
                        DestRect = new Rectangle(
                            (int)(tile.X * tile.Tile.Width * Viewport.ZoomFactor), 
                            (int)(tile.Y * tile.Tile.Height * Viewport.ZoomFactor), 
                            (int)(tile.Tile.Width * Viewport.ZoomFactor), 
                            (int)(tile.Tile.Height * Viewport.ZoomFactor)),
                        BlendColor = Color.White,
                    };
                }
            }
        }

        public override ObservableDictionary<string, TextureResource> TextureSource
        {
            get
            {
                return _textures;
            }
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
            region.Width = (int)Math.Ceiling(Math.Min(region.Width, LayerWidth));
            region.Height = (int)Math.Ceiling(Math.Min(region.Height, LayerHeight));
            
            Rect tileRegion = new Rect(
                region.X / Layer.TileWidth,
                region.Y / Layer.TileHeight,
                (int)(region.Width + region.X % Layer.TileWidth + Layer.TileWidth - 1) / Layer.TileWidth,
                (int)(region.Height + region.Y % Layer.TileHeight + Layer.TileHeight - 1) / Layer.TileHeight
                );

            return tileRegion;
        }

        private PointerTool _currentTool;

        public void SetTool (TileTool tool)
        {
            switch (tool) {
                case TileTool.Draw:
                    _currentTool = new TileDrawTool(Level.CommandHistory, Layer as MultiTileGridLayer, Level.Annotations);
                    break;
                case TileTool.Erase:
                    _currentTool = new TileEraseTool(Level.CommandHistory, Layer as MultiTileGridLayer, Level.Annotations);
                    break;
                case TileTool.Fill:
                    _currentTool = new TileFillTool(Level.CommandHistory, Layer as MultiTileGridLayer);
                    break;
                default:
                    _currentTool = null;
                    break;
            }
        }

        public override void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.StartPointerSequence(info);
        }

        public override void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info);
        }

        public override void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info);
        }

        public override void HandlePointerPosition (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.PointerPosition(info);
        }
    }
}
