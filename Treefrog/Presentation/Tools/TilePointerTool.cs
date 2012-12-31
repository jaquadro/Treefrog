using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation.Tools
{
    public enum TileSourceType
    {
        Tile,
        Brush,
    }

    public class TilePointerTool : PointerTool
    {
        private ITilePoolListPresenter _tilePool;
        private ITileBrushManagerPresenter _brushManager;

        private CommandHistory _history;
        private MultiTileGridLayer _layer;
        private TileSourceType _sourceType;

        public TilePointerTool (CommandHistory history, MultiTileGridLayer layer)
        {
            _history = history;
            _layer = layer;
        }

        public TilePointerTool (CommandHistory history, MultiTileGridLayer layer, TileSourceType mode)
            : this(history, layer)
        {
            _sourceType = mode;
        }

        protected override void DisposeManaged ()
        {
            if (_tilePool != null) {
                _tilePool.TileSelectionChanged -= TileSelectionChangedHandler;
            }

            if (_brushManager != null) {
                _brushManager.TileBrushSelected -= TileBrushSelectedHandler;
            }

            base.DisposeManaged();
        }

        public void BindTilePoolController (ITilePoolListPresenter controller)
        {
            if (_tilePool != null) {
                _tilePool.TileSelectionChanged -= TileSelectionChangedHandler;
            }

            _tilePool = controller;

            if (_tilePool != null) {
                _tilePool.TileSelectionChanged += TileSelectionChangedHandler;
            }
        }

        public void BindTileBrushManager (ITileBrushManagerPresenter controller)
        {
            if (_brushManager != null) {
                _brushManager.TileBrushSelected -= TileBrushSelectedHandler;
            }

            _brushManager = controller;

            if (_brushManager != null) {
                _brushManager.TileBrushSelected += TileBrushSelectedHandler;
            }
        }

        private void TileSelectionChangedHandler (object sender, EventArgs e)
        {
            _sourceType = TileSourceType.Tile;
            SourceChangedCore(TileSourceType.Tile);
        }

        private void TileBrushSelectedHandler (object sender, EventArgs e)
        {
            _sourceType = TileSourceType.Brush;
            SourceChangedCore(TileSourceType.Brush);
        }

        protected virtual void SourceChangedCore (TileSourceType type)
        { }

        protected MultiTileGridLayer Layer
        {
            get { return _layer; }
        }

        protected CommandHistory History
        {
            get { return _history; }
        }

        protected TileSourceType SourceType
        {
            get { return _sourceType; }
        }

        protected Tile ActiveTile
        {
            get
            {
                if (_tilePool == null)
                    return null;

                return _tilePool.SelectedTile;
            }
        }

        protected TileBrush ActiveBrush
        {
            get
            {
                if (_brushManager == null)
                    return null;

                return _brushManager.SelectedBrush;
            }
        }

        protected bool SourceValid
        {
            get
            {
                switch (SourceType) {
                    case TileSourceType.Brush:
                        return ActiveBrush != null;
                    case TileSourceType.Tile:
                        return ActiveTile != null;
                }

                return false;
            }
        }

        protected ITilePoolListPresenter PoolPresenter
        {
            get { return _tilePool; }
        }

        protected ITileBrushManagerPresenter BrushManager
        {
            get { return _brushManager; }
        }

        protected TileCoord TileLocation (PointerEventInfo info)
        {
            return new TileCoord((int)Math.Floor(info.X / Layer.TileWidth), 
                (int)Math.Floor(info.Y / Layer.TileHeight));
        }

        protected bool TileInRange (TileCoord location)
        {
            if (location.X < Layer.TileOriginX || location.X >= (Layer.TilesWide + Layer.TileOriginX))
                return false;
            if (location.Y < Layer.TileOriginY || location.Y >= (Layer.TilesHigh + Layer.TileOriginY))
                return false;

            return true;
        }
    }
}
