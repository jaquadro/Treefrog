using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Annotations;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Layers;
using Treefrog.Utility;
using Treefrog.Windows.Forms;

namespace Treefrog.Presentation
{
    public interface ILayerContext
    {
        ILevelGeometry Geometry { get; }
        CommandHistory History { get; }
        ObservableCollection<Annotation> Annotations { get; }
    }

    public class LevelPresenter : IDisposable, ILayerContext, IPointerResponderProvider, ICommandSubscriber, ILayerListPresenter
    {
        private bool _disposed;
        private EditorPresenter _editor;
        private Level _level;
        private LevelInfoPresenter _info;
        private ZoomState _zoom;

        private GroupLayerPresenter _rootLayer;
        private GroupLayerPresenter _rootContentLayer;
        private GridLayerPresenter _gridLayer;

        private string _selectedLayer;
        private LevelLayerPresenter _selectedLayerRef;

        private Dictionary<string, LevelLayerPresenter> _layerPresenters;

        private CommandHistory _history;
        private ObservableCollection<Annotation> _annotations;

        public LevelPresenter (EditorPresenter editor, Level level)
        {
            _editor = editor;
            _level = level;

            _zoom = new ZoomState();
            _zoom.ZoomLevelChanged += ZoomStateLevelChanged;

            _info = new LevelInfoPresenter(this);

            _layerPresenters = new Dictionary<string, LevelLayerPresenter>();

            _history = new CommandHistory();
            _history.HistoryChanged += HistoryChangedHandler;

            _annotations = new ObservableCollection<Annotation>();

            InitializeCommandManager();
            InitializeLayerHierarchy();
            InitializeLayers();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    foreach (LevelLayerPresenter layer in _layerPresenters.Values) {
                        UnbindLayerEvents(layer);
                        layer.Dispose();
                    }

                    foreach (Layer layer in _level.Layers)
                        UnbindLayerEvents(layer);
                }

                _disposed = true;
            }
        }

        private void HistoryChangedHandler (object sender, EventArgs e)
        {
            CommandManager.Invalidate(CommandKey.Undo);
            CommandManager.Invalidate(CommandKey.Redo);
        }

        public Level Level
        {
            get { return _level; }
        }

        public IContentInfoPresenter InfoPresenter
        {
            get { return _info; }
        }

        public ZoomState Zoom
        {
            get { return _zoom; }
        }

        public GroupLayerPresenter RootLayer
        {
            get { return _rootLayer; }
        }

        public GroupLayerPresenter RootContentLayer
        {
            get { return _rootContentLayer; }
        }

        public LevelLayerPresenter SelectedLayer
        {
            get { return _selectedLayerRef; }
        }

        public TexturePool TexturePool
        {
            get { return _level.Project.TexturePool; }
        }

        public CommandHistory History
        {
            get { return _history; }
        }

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
        }

        public ILevelGeometry LevelGeometry { get; set; }

        public ILevelGeometry Geometry
        {
            get { return LevelGeometry; }
        }

        public IPointerResponder PointerEventResponder
        {
            get
            {
                return _selectedLayerRef != null
                    ? _selectedLayerRef.PointerEventResponder : null;
            }
        }

        public event EventHandler PointerEventResponderChanged;

        protected virtual void OnPointerEventResponderChanged (EventArgs e)
        {
            var ev = PointerEventResponderChanged;
            if (ev != null)
                ev(this, e);
        }

        private void InitializeLayerHierarchy ()
        {
            _rootContentLayer = new GroupLayerPresenter();
            _gridLayer = new GridLayerPresenter() {
                IsVisible = false,
            };

            _rootLayer = new GroupLayerPresenter();
            _rootLayer.Layers.Add(new WorkspaceLayerPresenter());
            _rootLayer.Layers.Add(_rootContentLayer);
            _rootLayer.Layers.Add(_gridLayer);
            _rootLayer.Layers.Add(new AnnotationLayerPresenter() {
                Annotations = _annotations,
            });
        }

        private void InitializeLayers ()
        {
            foreach (Layer layer in _level.Layers) {
                AddLayer(layer);
            }

            SelectLayer();
        }

        private void AddLayer (Layer layer)
        {
            LevelLayerPresenter layerp = LayerPresenterFactory.Default.Create(layer, this);
            layerp.Info = _info;

            _layerPresenters[layer.Name] = layerp;
            _rootContentLayer.Layers.Add(layerp);

            BindingHelper.TryBind<ITilePoolListPresenter>(layerp, _editor.Presentation.TilePoolList);
            BindingHelper.TryBind<ITileBrushManagerPresenter>(layerp, _editor.Presentation.TileBrushes);
            BindingHelper.TryBind<IObjectPoolCollectionPresenter>(layerp, _editor.Presentation.ObjectPoolCollection);

            BindLayerEvents(layer);
        }

        private void RemoveLayer (string layerName)
        {
            if (layerName == null || !_layerPresenters.ContainsKey(layerName))
                return;

            LevelLayerPresenter layerp = _layerPresenters[layerName];
            _layerPresenters.Remove(layerName);
            _rootContentLayer.Layers.Remove(layerp);

            UnbindLayerEvents(layerp);
            layerp.Dispose();
        }

        private void SelectLayer ()
        {
            SelectLayer(null);

            foreach (Layer layer in _level.Layers) {
                SelectLayer(layer.Name);
                return;
            }
        }

        private void SelectLayer (string layerName)
        {
            if (_selectedLayer == layerName)
                return;

            if (_selectedLayerRef != null) {
                UnbindSelectedLayerEvents(_selectedLayerRef);

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.RemoveCommandSubscriber(comLayer);
                }

                _selectedLayerRef.Deactivate();
            }

            if (layerName == null || !_layerPresenters.ContainsKey(layerName)) {
                _selectedLayer = null;
                _selectedLayerRef = null;

                InvalidateLayerCommands();
                RefreshGridVisibility();
                OnPointerEventResponderChanged(EventArgs.Empty);
                return;
            }

            _selectedLayer = layerName;
            _selectedLayerRef = _layerPresenters[_selectedLayer];

            _info.ActionUpdateCoordinates("");
            _gridLayer.IsVisible = false;

            if (_selectedLayerRef != null) {
                BindSelectedLayerEvents(_selectedLayerRef);

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.AddCommandSubscriber(comLayer);
                }

                if (_selectedLayerRef is TileGridLayerPresenter) {
                    _gridLayer.IsVisible = CommandManager.IsSelected(CommandKey.ViewGrid);
                    _gridLayer.GridSpacingX = (_selectedLayerRef as TileGridLayerPresenter).Layer.TileWidth;
                    _gridLayer.GridSpacingY = (_selectedLayerRef as TileGridLayerPresenter).Layer.TileHeight;
                }

                _selectedLayerRef.Activate();
            }

            InvalidateLayerCommands();
            OnPointerEventResponderChanged(EventArgs.Empty);

            _info.RefreshContentInfo();
        }

        private void BindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged += LayerNameChanged;
            }
        }

        private void UnbindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged -= LayerNameChanged;
            }
        }

        private void UnbindLayerEvents (LevelLayerPresenter layer)
        {
            UnbindSelectedLayerEvents(layer);
        }

        private void BindSelectedLayerEvents (LevelLayerPresenter layer)
        {
            if (layer != null) {
                layer.PointerEventResponderChanged += SelectedLayerPointerEventResponderChanged;
            }
        }

        private void UnbindSelectedLayerEvents (LevelLayerPresenter layer)
        {
            if (layer != null) {
                layer.PointerEventResponderChanged -= SelectedLayerPointerEventResponderChanged;
            }
        }

        private void SelectedLayerPointerEventResponderChanged (object sender, EventArgs e)
        {
            OnPointerEventResponderChanged(EventArgs.Empty);
        }

        private void LayerNameChanged (object sender, NameChangedEventArgs e)
        {
            if (_layerPresenters.ContainsKey(e.OldName)) {
                _layerPresenters[e.NewName] = _layerPresenters[e.OldName];
                _layerPresenters.Remove(e.OldName);
            }

            if (_selectedLayer == e.OldName)
                _selectedLayer = e.NewName;
        }

        #region Commands

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();
            _commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.Undo, CommandCanUndo, CommandUndo);
            _commandManager.Register(CommandKey.Redo, CommandCanRedo, CommandRedo);
            _commandManager.Register(CommandKey.LevelResize, CommandCanResize, CommandResize);
            _commandManager.Register(CommandKey.ViewZoomIn, CommandCanZoomIn, CommandZoomIn);
            _commandManager.Register(CommandKey.ViewZoomOut, CommandCanZoomOut, CommandZoomOut);
            _commandManager.Register(CommandKey.ViewZoomNormal, CommandCanZoomNormal, CommandZoomNormal);

            _commandManager.Register(CommandKey.NewTileLayer, CommandCanAddTileLayer, CommandAddTileLayer);
            _commandManager.Register(CommandKey.NewObjectLayer, CommandCanAddObjectLayer, CommandAddObjectLayer);
            _commandManager.Register(CommandKey.LayerClone, CommandCanCloneLayer, CommandCloneLayer);
            _commandManager.Register(CommandKey.LayerDelete, CommandCanDeleteLayer, CommandDeleteLayer);
            _commandManager.Register(CommandKey.LayerProperties, CommandCanLayerProperties, CommandLayerProperties);
            _commandManager.Register(CommandKey.LayerMoveTop, CommandCanMoveLayerTop, CommandMoveLayerTop);
            _commandManager.Register(CommandKey.LayerMoveUp, CommandCanMoveLayerUp, CommandMoveLayerUp);
            _commandManager.Register(CommandKey.LayerMoveDown, CommandCanMoveLayerDown, CommandMoveLayerDown);
            _commandManager.Register(CommandKey.LayerMoveBottom, CommandCanMoveLayerBottom, CommandMoveLayerBottom);

            _commandManager.RegisterToggle(CommandKey.ViewGrid, CommandCanToggleGrid, CommandToggleGrid);

            _commandManager.Perform(CommandKey.ViewGrid);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private void HandleCommandInvalidated (object sender, CommandSubscriberEventArgs e)
        {
            _editor.Presentation.DocumentTools.RefreshDocumentTools();
        }

        private bool CommandCanUndo ()
        {
            return History.CanUndo;
        }

        private void CommandUndo ()
        {
            if (CommandCanUndo()) {
                History.Undo();
            }
        }

        private bool CommandCanRedo ()
        {
            return History.CanRedo;
        }

        private void CommandRedo ()
        {
            if (CommandCanRedo()) {
                History.Redo();
            }
        }

        private bool CommandCanToggleGrid ()
        {
            return true;
        }

        private void CommandToggleGrid ()
        {
            RefreshGridVisibility();
        }

        private bool CommandCanResize ()
        {
            return true;
        }

        private void CommandResize ()
        {
            if (CommandCanResize()) {
                using (ResizeLevelForm form = new ResizeLevelForm(_level)) {
                    if (form.ShowDialog() == DialogResult.OK) {
                        _level.Resize(form.NewOriginX, form.NewOriginY, form.NewWidth, form.NewHeight);

                        //Intercept event instead
                        if (LevelGeometry != null)
                            LevelGeometry.LevelBounds = new Rectangle(
                                form.NewOriginX, form.NewOriginY,
                                form.NewWidth, form.NewHeight);

                        _history.Clear();
                    }
                }
            }
        }

        private bool CommandCanZoomIn ()
        {
            return _zoom.CanZoomIn;
        }

        private void CommandZoomIn ()
        {
            if (CommandCanZoomIn()) {
                _zoom.ZoomIn();
                InvalidateZoomCommands();
            }
        }

        private bool CommandCanZoomOut ()
        {
            return _zoom.CanZoomOut;
        }

        private void CommandZoomOut ()
        {
            if (CommandCanZoomOut()) {
                _zoom.ZoomOut();
                InvalidateZoomCommands();
            }
        }

        private bool CommandCanZoomNormal ()
        {
            return true;
        }

        private void CommandZoomNormal ()
        {
            if (CommandCanZoomNormal()) {
                _zoom.ZoomLevel = 1f;
                InvalidateZoomCommands();
            }
        }

        private bool CommandCanAddTileLayer ()
        {
            return true;
        }

        private void CommandAddTileLayer ()
        {
            if (CommandCanAddTileLayer()) {
                string name = FindDefaultLayerName("Tile Layer");

                using (TileLayerForm form = new TileLayerForm(_level, name)) {
                    foreach (Layer layer in _level.Layers)
                        form.ReservedNames.Add(layer.Name);

                    if (form.ShowDialog() == DialogResult.OK) {
                        _level.Layers.Add(form.Layer);

                        // Intercept event instead
                        AddLayer(form.Layer);

                        SelectLayer(name);

                        OnSyncLayerList(EventArgs.Empty);
                        OnSyncLayerSelection(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanAddObjectLayer ()
        {
            return true;
        }

        private void CommandAddObjectLayer ()
        {
            if (CommandCanAddObjectLayer()) {
                string name = FindDefaultLayerName("Object Layer");

                ObjectLayer layer = new ObjectLayer(name, _level);
                _level.Layers.Add(layer);

                // Intercept event instead
                AddLayer(layer);

                SelectLayer(name);

                OnSyncLayerList(EventArgs.Empty);
                OnSyncLayerSelection(EventArgs.Empty);
            }
        }

        private bool CommandCanCloneLayer ()
        {
            return SelectedLayer != null;
        }

        private void CommandCloneLayer ()
        {
            if (CommandCanCloneLayer() && _layerPresenters.ContainsKey(_selectedLayer)) {
                string name = FindCloneLayerName(_selectedLayer);

                Layer layer = null;
                if (_selectedLayerRef is TileLayerPresenter)
                    layer = new MultiTileGridLayer(name, SelectedLayer.Layer as MultiTileGridLayer);
                else if (_selectedLayerRef is ObjectLayerPresenter)
                    layer = new ObjectLayer(name, SelectedLayer.Layer as ObjectLayer);
                else
                    return;

                _level.Layers.Add(layer);

                // Intercept event instead
                AddLayer(layer);

                SelectLayer(name);

                OnSyncLayerList(EventArgs.Empty);
                OnSyncLayerSelection(EventArgs.Empty);
            }
        }

        private bool CommandCanDeleteLayer ()
        {
            return SelectedLayer != null;
        }

        private void CommandDeleteLayer ()
        {
            if (CommandCanDeleteLayer() && _layerPresenters.ContainsKey(_selectedLayer)) {
                _level.Layers.Remove(_selectedLayer);

                // Intercept event instead
                RemoveLayer(_selectedLayer);

                SelectLayer();

                OnSyncLayerList(EventArgs.Empty);
                OnSyncLayerSelection(EventArgs.Empty);
            }
        }

        private bool CommandCanLayerProperties ()
        {
            return SelectedLayer != null;
        }

        private void CommandLayerProperties ()
        {
            if (CommandCanLayerProperties()) {
                _editor.Presentation.PropertyList.Provider = SelectedLayer.Layer;
                _editor.ActivatePropertyPanel();
            }
        }

        private bool CommandCanMoveLayerTop ()
        {
            return CommandCanMoveLayerUp();
        }

        private void CommandMoveLayerTop ()
        {
            if (CommandCanMoveLayerTop()) {
                int index = _level.Layers.IndexOf(_selectedLayer);
                int count = _level.Layers.Count - 1;
                _level.Layers.ChangeIndexRelative(_selectedLayer, count - index);

                int pindex = _rootContentLayer.Layers.IndexOf(_selectedLayerRef);
                _rootContentLayer.Layers.Move(pindex, _rootContentLayer.Layers.Count - 1);

                InvalidateLayerViewCommands();

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private bool CommandCanMoveLayerUp ()
        {
            return (SelectedLayer != null 
                && _level.Layers.Contains(_selectedLayer) 
                && _level.Layers.IndexOf(_selectedLayer) < _level.Layers.Count - 1);
        }

        private void CommandMoveLayerUp ()
        {
            if (CommandCanMoveLayerUp()) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, 1);

                int index = _rootContentLayer.Layers.IndexOf(_selectedLayerRef);
                _rootContentLayer.Layers.Move(index, index + 1);

                InvalidateLayerViewCommands();

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private bool CommandCanMoveLayerDown ()
        {
            return (SelectedLayer != null 
                && _level.Layers.Contains(_selectedLayer)
                && _level.Layers.IndexOf(_selectedLayer) > 0);
        }

        private void CommandMoveLayerDown ()
        {
            if (CommandCanMoveLayerDown()) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, -1);

                int index = _rootContentLayer.Layers.IndexOf(_selectedLayerRef);
                _rootContentLayer.Layers.Move(index, index - 1);

                InvalidateLayerViewCommands();

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private bool CommandCanMoveLayerBottom ()
        {
            return CommandCanMoveLayerDown();
        }

        private void CommandMoveLayerBottom ()
        {
            if (CommandCanMoveLayerBottom()) {
                int index = _level.Layers.IndexOf(_selectedLayer);
                _level.Layers.ChangeIndexRelative(_selectedLayer, 0 - index);

                int pindex = _rootContentLayer.Layers.IndexOf(_selectedLayerRef);
                _rootContentLayer.Layers.Move(pindex, 0);

                InvalidateLayerViewCommands();

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private void InvalidateLayerViewCommands ()
        {
            CommandManager.Invalidate(CommandKey.LayerMoveTop);
            CommandManager.Invalidate(CommandKey.LayerMoveUp);
            CommandManager.Invalidate(CommandKey.LayerMoveDown);
            CommandManager.Invalidate(CommandKey.LayerMoveBottom);
        }

        private void InvalidateLayerCommands ()
        {
            InvalidateLayerViewCommands();

            CommandManager.Invalidate(CommandKey.LayerClone);
            CommandManager.Invalidate(CommandKey.LayerDelete);
            CommandManager.Invalidate(CommandKey.LayerProperties);
            CommandManager.Invalidate(CommandKey.LayerExportRaster);
            CommandManager.Invalidate(CommandKey.LevelResize);
        }

        private void InvalidateZoomCommands ()
        {
            CommandManager.Invalidate(CommandKey.ViewZoomIn);
            CommandManager.Invalidate(CommandKey.ViewZoomOut);
        }

        #endregion

        private void RefreshGridVisibility ()
        {
            if (_gridLayer != null) {
                if (_selectedLayerRef is TileGridLayerPresenter)
                    _gridLayer.IsVisible = CommandManager.IsSelected(CommandKey.ViewGrid);
                else
                    _gridLayer.IsVisible = false;
            }
        }

        private void ZoomStateLevelChanged (object sender, EventArgs e)
        {
            if (LevelGeometry != null)
                LevelGeometry.ZoomFactor = _zoom.ZoomLevel;
        }

        private string FindDefaultLayerName (string baseName)
        {
            List<string> names = new List<string>();
            foreach (Layer layer in _level.Layers) {
                names.Add(layer.Name);
            }

            int i = 0;
            while (true) {
                string name = baseName + " " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }

        private string FindCloneLayerName (string basename)
        {
            List<string> names = new List<string>();
            foreach (Layer layer in _level.Layers) {
                names.Add(layer.Name);
            }

            int i = 0;
            while (true) {
                string name = basename + " (" + ++i + ")";
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }




        public IEnumerable<LevelLayerPresenter> LayerList
        {
            get
            {
                foreach (LevelLayerPresenter layer in _rootContentLayer.Layers) {
                    if (layer != null)
                        yield return layer;
                }
            }
        }

        public event EventHandler SyncLayerList;
        public event EventHandler SyncLayerSelection;

        protected virtual void OnSyncLayerList (EventArgs e)
        {
            var ev = SyncLayerList;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnSyncLayerSelection (EventArgs e)
        {
            var ev = SyncLayerSelection;
            if (ev != null)
                ev(this, e);
        }

        public void ActionSelectLayer (string name)
        {
            SelectLayer(name);

            if (SelectedLayer != null)
                _editor.Presentation.PropertyList.Provider = SelectedLayer.Layer;

            OnSyncLayerSelection(EventArgs.Empty);
        }

        public void ActionShowHideLayer (string name, LayerVisibility visibility)
        {

        }
    }
}
