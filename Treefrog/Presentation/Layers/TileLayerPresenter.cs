using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Support;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;
using Treefrog.Prseentation.Tools;
using SysDrawing = System.Drawing;
using SysDrawing2D = System.Drawing.Drawing2D;
using SysImaging = System.Drawing.Imaging;

namespace Treefrog.Presentation.Layers
{
    public class TileLayerPresenter : LevelLayerPresenter, ICommandSubscriber, IPointerResponder, ITileSelectionLayer
    {
        private TileLayer _layer;

        private ITilePoolListPresenter _tilePoolController;
        private ITileBrushManagerPresenter _tileBrushController;
        private TileSourceType _sourceType = TileSourceType.Tile;

        public TileLayerPresenter (LevelPresenter2 levelPresenter, TileLayer layer)
            : base(levelPresenter, layer)
        {
            _layer = layer;

            InitializeCommandManager();
        }

        public void BindTilePoolController (ITilePoolListPresenter tilePoolController)
        {
            if (_tilePoolController != null) {
                _tilePoolController.TileSelectionChanged -= TileSelectionChangedHandler;
            }

            _tilePoolController = tilePoolController;

            if (_tilePoolController != null) {
                _tilePoolController.TileSelectionChanged += TileSelectionChangedHandler;
            }
        }

        public void BindTileBrushManager (ITileBrushManagerPresenter tileBrushController)
        {
            if (_tileBrushController != null) {
                _tileBrushController.TileBrushSelected -= TileBrushSelectedHandler;
            }

            _tileBrushController = tileBrushController;

            if (_tileBrushController != null) {
                _tileBrushController.TileBrushSelected += TileBrushSelectedHandler;
            }
        }

        public override void Activate ()
        {
            if (_selection != null && LevelPresenter != null) {
                LevelPresenter.Annotations.Remove(_selection.SelectionAnnotation);
                LevelPresenter.Annotations.Add(_selection.SelectionAnnotation);
            }
        }

        public override void Deactivate ()
        {
            if (_selection != null && LevelPresenter != null) {
                LevelPresenter.Annotations.Remove(_selection.SelectionAnnotation);
            }
        }

        private void TileSelectionChangedHandler (object sender, EventArgs e)
        {
            _sourceType = TileSourceType.Tile;

            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolErase:
                case CommandKey.TileToolSelect:
                    CommandManager.Perform(CommandKey.TileToolDraw);
                    break;
            }
        }

        private void TileBrushSelectedHandler (object sender, EventArgs e)
        {
            _sourceType = TileSourceType.Brush;

            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolErase:
                case CommandKey.TileToolSelect:
                    CommandManager.Perform(CommandKey.TileToolDraw);
                    break;
            }
        }

        protected new TileLayer Layer
        {
            get { return _layer; }
        }

        protected Rectangle ComputeTileRegion ()
        {
            ILevelGeometry geometry = LevelPresenter.LevelGeometry;

            int zoomTileWidth = (int)(Layer.TileWidth * geometry.ZoomFactor);
            int zoomTileHeight = (int)(Layer.TileHeight * geometry.ZoomFactor);

            Rectangle region = geometry.VisibleBounds;
            region.Width = (int)Math.Min(region.Width, Layer.LayerWidth);
            region.Height = (int)Math.Min(region.Height, Layer.LayerHeight);

            Rectangle tileRegion = new Rectangle(
                region.X / Layer.TileWidth,
                region.Y / Layer.TileHeight,
                (int)(region.Width + region.X % Layer.TileWidth + Layer.TileWidth - 1) / Layer.TileWidth,
                (int)(region.Height + region.Y % Layer.TileHeight + Layer.TileHeight - 1) / Layer.TileHeight
                );

            return tileRegion;
        }

        #region Tile Selection

        private PointerTool _currentTool;
        private TileSelection _selection;

        public void CreateTileSelection ()
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(Layer.TileWidth, Layer.TileHeight);
            if (!(_currentTool is TileSelectTool))
                _selection.Deactivate();

            LevelPresenter.Annotations.Add(_selection.SelectionAnnotation);

            CommandManager.Invalidate(CommandKey.Cut);
            CommandManager.Invalidate(CommandKey.Copy);
            CommandManager.Invalidate(CommandKey.Delete);
            CommandManager.Invalidate(CommandKey.TileSelectionFloat);
            CommandManager.Invalidate(CommandKey.TileSelectionDefloat);
        }

        public void CreateFloatingSelection ()
        {
            CreateTileSelection();

            _selection.Float();

            CommandManager.Invalidate(CommandKey.TileSelectionFloat);
            CommandManager.Invalidate(CommandKey.TileSelectionDefloat);
        }

        public void DeleteTileSelection ()
        {
            if (_selection != null) {
                LevelPresenter.Annotations.Remove(_selection.SelectionAnnotation);
                _selection = null;

                CommandManager.Invalidate(CommandKey.Cut);
                CommandManager.Invalidate(CommandKey.Copy);
                CommandManager.Invalidate(CommandKey.Delete);
                CommandManager.Invalidate(CommandKey.TileSelectionFloat);
                CommandManager.Invalidate(CommandKey.TileSelectionDefloat);
            }
        }

        public void RestoreTileSelection (TileSelection selection)
        {
            if (_selection != null)
                DeleteTileSelection();

            _selection = new TileSelection(selection);
            LevelPresenter.Annotations.Add(_selection.SelectionAnnotation);
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
                _selection.AddTiles(Layer as MultiTileGridLayer, tileLocations);
            }
        }

        public void AddTilesToSelection (IEnumerable<TileCoord> tileLocations)
        {
            if (_selection != null) {
                _selection.AddTiles(Layer as MultiTileGridLayer, tileLocations);
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

                CommandManager.Invalidate(CommandKey.TileSelectionFloat);
                CommandManager.Invalidate(CommandKey.TileSelectionDefloat);
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

                CommandManager.Invalidate(CommandKey.TileSelectionFloat);
                CommandManager.Invalidate(CommandKey.TileSelectionDefloat);
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

        #region Commands

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.Cut, CommandCanCut, CommandCut);
            _commandManager.Register(CommandKey.Copy, CommandCanCopy, CommandCopy);
            _commandManager.Register(CommandKey.Paste, CommandCanPaste, CommandPaste);
            _commandManager.Register(CommandKey.Delete, CommandCanDelete, CommandDelete);
            _commandManager.Register(CommandKey.LayerExportRaster, CommandCanExportRaster, CommandExportRaster);
            _commandManager.Register(CommandKey.TileSelectionFloat, CommandCanFloat, CommandFloat);
            _commandManager.Register(CommandKey.TileSelectionDefloat, CommandCanDefloat, CommandDefloat);

            _commandManager.RegisterToggleGroup(CommandToggleGroup.TileTool, CommandGroupCheckTileTool, CommandGroupPerformTileTool);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolSelect);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolDraw);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolErase);
            _commandManager.RegisterToggle(CommandToggleGroup.TileTool, CommandKey.TileToolFill);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        #region Cut

        private bool CommandCanCut ()
        {
            TileSelectTool tool = _currentTool as TileSelectTool;
            if (tool != null)
                return _selection != null;
            else
                return false;
        }

        private void CommandCut ()
        {
            if (CommandCanCut()) {
                TileSelectionClipboard clip = new TileSelectionClipboard(_selection.Tiles);
                clip.CopyToClipboard();

                CompoundCommand command = new CompoundCommand();
                if (!_selection.Floating)
                    command.AddCommand(new FloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                command.AddCommand(new DeleteTileSelectionCommand(this));

                LevelPresenter.History.Execute(command);

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        #region Copy

        private bool CommandCanCopy ()
        {
            return CommandCanCut();
        }

        private void CommandCopy ()
        {
            if (CommandCanCopy()) {
                TileSelectionClipboard clip = new TileSelectionClipboard(_selection.Tiles);
                clip.CopyToClipboard();

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        #region Paste

        private bool CommandCanPaste ()
        {
            return TileSelectionClipboard.ContainsData;
        }

        private void CommandPaste ()
        {
            if (CommandCanPaste()) {
                TileSelectionClipboard clip = TileSelectionClipboard.CopyFromClipboard();
                TileSelection selection = clip.GetAsTileSelection(Layer.Level.Project, Layer.TileWidth, Layer.TileHeight);
                if (selection == null)
                    return;

                if (_selection != null) {
                    CompoundCommand command = new CompoundCommand();
                    if (_selection.Floating)
                        command.AddCommand(new DefloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                    command.AddCommand(new DeleteTileSelectionCommand(this));

                    LevelPresenter.History.Execute(command);
                }

                Command pasteCommand = new PasteFloatingSelectionCommand(this, selection, GetCenterTileOffset());
                LevelPresenter.History.Execute(pasteCommand);

                if (!(_currentTool is TileSelectTool)) {
                    CommandManager.Perform(CommandKey.TileToolSelect);
                }

                _selection.Activate();
            }
        }

        private TileCoord GetCenterTileOffset ()
        {
            Rectangle region = LevelPresenter.LevelGeometry.VisibleBounds;
            int centerViewX = (int)(region.Left + (region.Right - region.Left) / 2);
            int centerViewY = (int)(region.Top + (region.Bottom - region.Top) / 2);

            return new TileCoord(centerViewX / Layer.TileWidth, centerViewY / Layer.TileHeight);
        }

        #endregion

        #region Delete

        private bool CommandCanDelete ()
        {
            return CommandCanCut();
        }

        private void CommandDelete ()
        {
            if (CommandCanDelete()) {
                CompoundCommand command = new CompoundCommand();
                if (!_selection.Floating)
                    command.AddCommand(new FloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                command.AddCommand(new DeleteTileSelectionCommand(this));

                LevelPresenter.History.Execute(command);

                CommandManager.Invalidate(CommandKey.Paste);
            }
        }

        #endregion

        #region Float

        private bool CommandCanFloat ()
        {
            TileSelectTool tool = _currentTool as TileSelectTool;
            if (tool != null)
                return _selection != null && _selection.Floating == false;
            else
                return false;
        }

        private void CommandFloat ()
        {
            if (CommandCanFloat()) {
                Command command = new FloatTileSelectionCommand(Layer as MultiTileGridLayer, this);

                if (LevelPresenter != null)
                    LevelPresenter.History.Execute(command);
                else
                    FloatSelection();
            }
        }

        #endregion

        #region Defloat

        private bool CommandCanDefloat ()
        {
            TileSelectTool tool = _currentTool as TileSelectTool;
            if (tool != null)
                return _selection != null && _selection.Floating == true;
            else
                return false;
        }

        private void CommandDefloat ()
        {
            if (CommandCanDefloat()) {
                CompoundCommand command = new CompoundCommand();
                if (TileSelection.Floating)
                    command.AddCommand(new DefloatTileSelectionCommand(Layer as MultiTileGridLayer, this));
                command.AddCommand(new DeleteTileSelectionCommand(this));

                if (LevelPresenter != null)
                    LevelPresenter.History.Execute(command);
                else
                    DefloatSelection();
            }
        }

        #endregion

        private bool CommandGroupCheckTileTool ()
        {
            return true;
        }

        private void CommandGroupPerformTileTool ()
        {
            switch (CommandManager.SelectedCommand(CommandToggleGroup.TileTool)) {
                case CommandKey.TileToolSelect:
                    SetTool(TileTool.Select);
                    break;
                case CommandKey.TileToolDraw:
                    SetTool(TileTool.Draw);
                    break;
                case CommandKey.TileToolErase:
                    SetTool(TileTool.Erase);
                    break;
                case CommandKey.TileToolFill:
                    SetTool(TileTool.Fill);
                    break;
            }
        }

        #region Export Raster

        private bool CommandCanExportRaster ()
        {
            return Layer != null && Layer is TileGridLayer;
        }

        private void CommandExportRaster ()
        {
            if (!CommandCanExportRaster())
                return;

            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Title = "Export Layer to Image";
            ofd.Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files|*";
            ofd.OverwritePrompt = true;
            ofd.RestoreDirectory = false;

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            TileGridLayer layer = Layer as TileGridLayer;
            Rectangle levelBounds = LevelPresenter.LevelGeometry.LevelBounds;

            try {
                using (SysDrawing.Bitmap raster = new SysDrawing.Bitmap(levelBounds.Width, levelBounds.Height, SysImaging.PixelFormat.Format32bppArgb)) {
                    using (SysDrawing.Graphics gsurface = SysDrawing.Graphics.FromImage(raster)) {
                        gsurface.CompositingMode = SysDrawing2D.CompositingMode.SourceOver;
                        gsurface.InterpolationMode = SysDrawing2D.InterpolationMode.NearestNeighbor;

                        foreach (LocatedTile tile in layer.Tiles) {
                            RasterizeTile(gsurface, tile);
                        }
                    }

                    raster.Save(ofd.FileName, SysImaging.ImageFormat.Png);
                }
            }
            catch (Exception e) {
                MessageBox.Show("Layer rasterization failed with the following exception:\n\n" + e.Message, "Rasterization Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RasterizeTile (SysDrawing.Graphics surface, LocatedTile tile)
        {
            Rectangle levelBounds = LevelPresenter.LevelGeometry.LevelBounds;

            using (SysDrawing.Bitmap tileBmp = tile.Tile.Pool.GetTileTexture(tile.Tile.Id).CreateBitmap()) {
                SysDrawing.Point location = new SysDrawing.Point(tile.X * Layer.TileWidth - levelBounds.X, tile.Y * Layer.TileHeight - levelBounds.Y);
                surface.DrawImage(tileBmp, location);
            }
        }

        #endregion

        #endregion

        #region Tools

        // TODO: MultiTileGridLayer stuff should move into appropriate class
        public void SetTool (TileTool tool)
        {
            if (LevelPresenter == null)
                return;

            if (_selection != null && tool == TileTool.Select)
                _selection.Activate();
            else if (_selection != null)
                _selection.Deactivate();

            if (_currentTool != null)
                _currentTool.Dispose();

            switch (tool) {
                case TileTool.Select:
                    _currentTool = new TileSelectTool(LevelPresenter.History, Layer as MultiTileGridLayer, LevelPresenter.Annotations, this);
                    break;
                case TileTool.Draw:
                    TileDrawTool drawTool = new TileDrawTool(LevelPresenter.History, Layer as MultiTileGridLayer, LevelPresenter.Annotations);
                    drawTool.BindTilePoolController(_tilePoolController);
                    drawTool.BindTileBrushManager(_tileBrushController);
                    _currentTool = drawTool;
                    break;
                case TileTool.Erase:
                    _currentTool = new TileEraseTool(LevelPresenter.History, Layer as MultiTileGridLayer, LevelPresenter.Annotations);
                    break;
                case TileTool.Fill:
                    TileFillTool fillTool = new TileFillTool(LevelPresenter.History, Layer as MultiTileGridLayer, _sourceType);
                    fillTool.BindTilePoolController(_tilePoolController);
                    fillTool.BindTileBrushManager(_tileBrushController);
                    _currentTool = fillTool;
                    break;
                default:
                    _currentTool = null;
                    break;
            }
        }

        public override IPointerResponder PointerEventResponder
        {
            get { return this; }
        }

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
            _currentTool.StartPointerSequence(info, LevelPresenter.LevelGeometry);
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.EndPointerSequence(info, LevelPresenter.LevelGeometry);
        }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (_currentTool != null)
                _currentTool.UpdatePointerSequence(info, LevelPresenter.LevelGeometry);
        }

        public void HandlePointerPosition (PointerEventInfo info)
        {
             if (_currentTool != null)
                 _currentTool.PointerPosition(info, LevelPresenter.LevelGeometry);

             //if (CheckLayerCondition(ShouldRespondToInput) && _layer != null) {
             //    TileCoord coords = MouseToTileCoords(new Point((int)info.X, (int)info.Y));
             //    OnMouseTileMove(new TileMouseEventArgs(coords, GetTile(coords)));
             //}
        }

        public void HandlePointerLeaveField ()
        {
            if (_currentTool != null)
                _currentTool.PointerLeaveField();
        }

        #endregion
    }
}
