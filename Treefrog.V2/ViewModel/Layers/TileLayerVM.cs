using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Treefrog.Framework.Model.Support;
using Treefrog.Framework.Model;
using Treefrog.Framework;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using Treefrog.ViewModel.Tools;
using Treefrog.ViewModel.Menu;
using Treefrog.Framework.Imaging;
using Treefrog.ViewModel.Commands;

namespace Treefrog.ViewModel.Layers
{
    public class TileLayerVM : LevelLayerVM, IDisposable, ITileSelectionLayer
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

        public void Dispose ()
        {
            if (_manager != null) {
                foreach (TilePool pool in _manager.Pools) {
                    pool.Modified -= HandlePoolModified;
                    pool.NameChanged -= HandlePoolNameChanged;
                }

                _manager.Pools.ResourceAdded -= HandleTilePoolAdded;
                _manager.Pools.ResourceRemoved -= HandleTilePoolRemoved;
                _manager = null;

                _textures.Clear();
            }
        }

        private void Initialize ()
        {
            _textures = new ObservableDictionary<string, TextureResource>();

            if (Level != null && Level.IsInitialized == false)
                Level.Initialized += HandleLevelDocumentInitialized;

            if (Layer != null && Layer.Level != null && Layer.Level.Project != null) {
                _manager = Layer.Level.Project.TilePoolManager;
                _manager.Pools.ResourceAdded += HandleTilePoolAdded;
                _manager.Pools.ResourceRemoved += HandleTilePoolRemoved;

                foreach (TilePool pool in _manager.Pools) {
                    _textures[pool.Name] = pool.TileSource;
                    pool.Modified += HandlePoolModified;
                    pool.NameChanged += HandlePoolNameChanged;
                } 
            }

            LevelIntialize();

            //_currentTool = new TileDrawTool(Level.CommandHistory, Layer as MultiTileGridLayer, Level.Annotations);
        }

        private void LevelIntialize ()
        {
            if (Level == null || Level.IsInitialized == false)
                return;

            Level.Initialized -= HandleLevelDocumentInitialized;

            ITileToolCollection tileToolCollection = Level.LookupToolCollection<ITileToolCollection>();
            if (tileToolCollection != null) {
                tileToolCollection.SelectedToolChanged += HandleSelectedToolChanged;
                SetTool(tileToolCollection.SelectedTool);
            }
        }

        public override void Activate ()
        {
            if (_selection != null) {
                Level.Annotations.Remove(_selection.SelectionAnnotation);
                Level.Annotations.Add(_selection.SelectionAnnotation);
            }
        }

        public override void Deactivate ()
        {
            if (_selection != null)
                Level.Annotations.Remove(_selection.SelectionAnnotation);
        }

        private void HandleLevelDocumentInitialized (object sender, EventArgs e)
        {
            LevelIntialize();
        }

        private void HandleTilePoolAdded (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _textures[e.Resource.Name] = e.Resource.TileSource;
            e.Resource.Modified += HandlePoolModified;
            e.Resource.NameChanged += HandlePoolNameChanged;
        }

        private void HandleTilePoolRemoved (object sender, NamedResourceEventArgs<TilePool> e)
        {
            _textures.Remove(e.Resource.Name);
            e.Resource.Modified -= HandlePoolModified;
            e.Resource.NameChanged -= HandlePoolNameChanged;
        }

        private void HandlePoolModified (object sender, EventArgs e)
        {
            TilePool pool = sender as TilePool;
            if (pool != null) {
                _textures.Remove(pool.Name);
                _textures.Add(pool.Name, pool.TileSource);
            }
        }

        private void HandlePoolNameChanged (object sender, NameChangedEventArgs e)
        {
            _textures.Remove(e.OldName);
            _textures.Add(e.NewName, _manager.Pools[e.NewName].TileSource);
        }

        private void HandleSelectedToolChanged (object sender, EventArgs e)
        {
            ITileToolCollection tileToolCollection = sender as ITileToolCollection;
            if (tileToolCollection != null) {
                SetTool(tileToolCollection.SelectedTool);
            }
        }

        protected new MultiTileGridLayer Layer
        {
            get { return base.Layer as MultiTileGridLayer; }
        }

        public override Vector GetCoordinates (double x, double y)
        {
            Vector pxc = base.GetCoordinates(x, y);
            pxc.X = Math.Floor(pxc.X / Layer.TileWidth);
            pxc.Y = Math.Floor(pxc.Y / Layer.TileHeight);

            return pxc;
        }

        public override Rect CoordinateBounds
        {
            get { return new Rect(0, 0, Layer.TilesWide, Layer.TilesHigh); }
        }

        private static IDictionary<TileCoord, TileStack> _emptyTileCoordHashSet = new Dictionary<TileCoord, TileStack>();

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
                        BlendColor = Colors.White,
                    };
                }

                if (_selection != null && _selection.Floating) {
                    foreach (KeyValuePair<TileCoord, TileStack> item in _selection.Tiles) {
                        foreach (Tile tile in item.Value) {
                            TileCoord scoord = tile.Pool.GetTileLocation(tile.Id);
                            TileCoord dcoord = item.Key;

                            yield return new DrawCommand()
                            {
                                Texture = tile.Pool.Name,
                                SourceRect = new Rectangle(scoord.X * tile.Width, scoord.Y * tile.Height, tile.Width, tile.Height),
                                DestRect = new Rectangle(
                                    (int)((dcoord.X + _selection.Offset.X) * tile.Width * Viewport.ZoomFactor),
                                    (int)((dcoord.Y + _selection.Offset.Y) * tile.Height * Viewport.ZoomFactor),
                                    (int)(tile.Width * Viewport.ZoomFactor),
                                    (int)(tile.Height * Viewport.ZoomFactor)),
                                BlendColor = Colors.White,
                            };
                        }
                    }
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
            if (_selection != null && tool == TileTool.Select)
                _selection.Activate();
            else if (_selection != null)
                _selection.Deactivate();

            switch (tool) {
                case TileTool.Select:
                    _currentTool = new TileSelectTool(Level.CommandHistory, Layer as MultiTileGridLayer, Level.Annotations, this);
                    break;
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

        public override void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #region Tile Selection

        TileSelection _selection;

        public void CreateTileSelection ()
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(Layer.TileWidth, Layer.TileHeight);
            if (!(_currentTool is TileSelectTool))
                _selection.Deactivate();

            Level.Annotations.Add(_selection.SelectionAnnotation);
        }

        public void CreateFloatingSelection ()
        {
            CreateTileSelection();

            _selection.Float();
        }

        public void DeleteTileSelection ()
        {
            if (_selection != null) {
                Level.Annotations.Remove(_selection.SelectionAnnotation);
                _selection = null;
            }
        }

        public void RestoreTileSelection (TileSelection selection)
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(selection);
            Level.Annotations.Add(_selection.SelectionAnnotation);
        }

        public void ClearTileSelection ()
        {
            if (_selection != null) {
                _selection.ClearTiles();
            }
        }

        public void SetTileSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.ClearTiles();
                _selection.AddTiles(Layer, tileLocations);
            }
        }

        public void AddTilesToSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.AddTiles(Layer, tileLocations);
            }
        }

        public void RemoveTilesFromSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.RemoveTiles(tileLocations);
            }
        }

        public void FloatSelection ()
        {
            if (_selection != null && _selection.Floating == false) {
                _selection.Float();
            }
        }

        public TileSelection TileSelection
        {
            get { return _selection; }
        }

        public void DefloatSelection ()
        {
            if (_selection != null && _selection.Floating == true) {
                _selection.Defloat();
            }
        }

        public void SetSelectionOffset (TileCoord offset)
        {
            if (_selection != null)
                _selection.Offset = offset;
        }

        public void MoveSelectionByOffset (TileCoord offset)
        {
            if (_selection != null) {
                _selection.Offset = new TileCoord(_selection.Offset.X + offset.X, _selection.Offset.Y + offset.Y);
            }
        }

        public bool HasSelection
        {
            get { return _selection != null; }
        }

        public TileCoord TileSelectionOffset
        {
            get { return (_selection != null) ? _selection.Offset : new TileCoord(0, 0); }
        }

        public bool TileSelectionCoverageAt (TileCoord coord)
        {
            if (_selection == null)
                return false;

            return _selection.CoverageAt(coord);
        }

        #endregion
    }

    public interface ITileSelectionLayer
    {
        bool HasSelection { get; }
        TileCoord TileSelectionOffset { get; }

        TileSelection TileSelection { get; }

        void CreateTileSelection ();
        void CreateFloatingSelection ();
        void DeleteTileSelection ();
        void RestoreTileSelection (TileSelection selection);

        void ClearTileSelection ();
        void SetTileSelection (IEnumerable<TileCoord> tileLocations);
        void AddTilesToSelection (IEnumerable<TileCoord> tileLocations);
        void RemoveTilesFromSelection (IEnumerable<TileCoord> tileLocations);

        void FloatSelection ();
        void DefloatSelection ();

        void SetSelectionOffset (TileCoord offset);
        void MoveSelectionByOffset (TileCoord offset);

        bool TileSelectionCoverageAt (TileCoord coord);
    }
}
