using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation.Tools
{
    public class TilePointerTool : PointerTool
    {
        //private TilePoolManagerService _poolManager;

        private ITilePoolListPresenter _tilePool;
        private ITileBrushManagerPresenter _brushManager;

        private CommandHistory _history;
        private MultiTileGridLayer _layer;

        public TilePointerTool (CommandHistory history, MultiTileGridLayer layer)
        {
            _history = history;
            _layer = layer;
        }

        public void BindTilePoolController (ITilePoolListPresenter controller)
        {
            _tilePool = controller;
        }

        public void BindTileBrushManager (ITileBrushManagerPresenter controller)
        {
            _brushManager = controller;
        }

        protected MultiTileGridLayer Layer
        {
            get { return _layer; }
        }

        protected CommandHistory History
        {
            get { return _history; }
        }

        protected Tile ActiveTile
        {
            get
            {
                /*if (_poolManager == null) {
                    _poolManager = ServiceContainer.Default.GetService<TilePoolManagerService>();
                    if (_poolManager == null)
                        return null;
                }

                TilePoolItemVM activeTile = _poolManager.ActiveTile;
                if (activeTile == null)
                    return null;
                return activeTile.Tile;*/

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
            return new TileCoord((int)Math.Floor(info.X / Layer.TileWidth), (int)Math.Floor(info.Y / Layer.TileHeight));
        }

        protected bool TileInRange (TileCoord location)
        {
            if (location.X < 0 || location.X >= Layer.TilesWide)
                return false;
            if (location.Y < 0 || location.Y >= Layer.TilesHigh)
                return false;

            return true;
        }
    }
}
