using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Treefrog.Extensibility;
//using Treefrog.Plugins.Object.Layers;
//using Treefrog.Plugins.Object;

namespace Treefrog.Presentation
{
    public interface ILayerContext
    {
        ILevelGeometry Geometry { get; }
        CommandHistory History { get; }
        ObservableCollection<Annotation> Annotations { get; }

        void SetPropertyProvider (IPropertyProvider provider);
        void ActivatePropertyProvider (IPropertyProvider provider);
        void ActivateContextMenu (CommandMenu menu, Point location);
    }

    public class ContextMenuEventArgs : EventArgs
    {
        public CommandMenu Menu { get; set; }
        public Point Location { get; set; }

        public ContextMenuEventArgs (CommandMenu menu, Point location)
        {
            Menu = menu;
            Location = location;
        }
    }

    public abstract class ContentPresenter
    {
        public abstract Guid Uid { get; }
        public abstract string Name { get; }
    }

    public class LevelPresenter : ContentPresenter, IDisposable, ILayerContext, IPointerResponderProvider, ICommandSubscriber, ILayerListPresenter
    {
        private bool _disposed;
        private EditorPresenter _editor;
        private PresenterManager _pm;
        private Level _level;
        private LevelInfoPresenter _info;
        private ZoomState _zoom;

        private GroupLayerPresenter _rootLayer;
        private GroupLayerPresenter _rootContentLayer;
        private GridLayerPresenter _gridLayer;

        private Guid _selectedLayer;
        private LevelLayerPresenter _selectedLayerRef;

        private Dictionary<Guid, LevelLayerPresenter> _layerPresenters;

        private CommandHistory _history;
        private ObservableCollection<Annotation> _annotations;

        public LevelPresenter (PresenterManager pm, EditorPresenter editor, Level level)
        {
            _pm = pm;
            _pm.InstanceRegistered += PresenterRegsitered;
            _pm.InstanceUnregistered += PresenterUnregistered;

            _editor = editor;
            _level = level;

            _zoom = new ZoomState();
            _zoom.ZoomLevelChanged += ZoomStateLevelChanged;

            _info = new LevelInfoPresenter(this);

            _layerPresenters = new Dictionary<Guid, LevelLayerPresenter>();

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
                    _pm.InstanceRegistered -= PresenterRegsitered;
                    _pm.InstanceUnregistered -= PresenterUnregistered;

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

        private void PresenterRegsitered (object sender, InstanceRegistryEventArgs<Presenter> e)
        {
            foreach (LevelLayerPresenter layer in _layerPresenters.Values)
                BindingHelper.TryBind(layer, e.Type, e.Instance);
        }

        private void PresenterUnregistered (object sender, InstanceRegistryEventArgs<Presenter> e)
        {
            foreach (LevelLayerPresenter layer in _layerPresenters.Values)
                BindingHelper.TryBind(layer, e.Type, null);
        }

        private IPropertyProvider _currentPropertyProvider;

        public void Activate ()
        {
            _editor.Presentation.PropertyList.Provider = _currentPropertyProvider;
        }

        public void Deactivate ()
        {

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

        public override Guid Uid
        {
            get { return _level.Uid; }
        }

        public override string Name
        {
            get { return _level.Name; }
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

        public ITexturePool TexturePool
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

        public event EventHandler LevelGeometryInvalidated;

        protected virtual void OnLevelGeometryInvalidated (EventArgs e)
        {
            var ev = LevelGeometryInvalidated;
            if (ev != null)
                ev(this, e);
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

            _layerPresenters[layer.Uid] = layerp;
            _rootContentLayer.Layers.Add(layerp);

            BindingHelper.TryBind<TilePoolListPresenter>(layerp, _editor.Presentation.TilePoolList);
            BindingHelper.TryBind<TileBrushManagerPresenter>(layerp, _editor.Presentation.TileBrushes);
            //BindingHelper.TryBind<ObjectPoolCollectionPresenter>(layerp, _editor.Presentation.ObjectPoolCollection);

            BindingHelper.TryBindAny(layerp, _pm.Select(kv => new KeyValuePair<Type, object>(kv.Key, kv.Value)));

            BindLayerEvents(layer);
        }

        private void RemoveLayer (Guid layerUid)
        {
            if (layerUid == null || !_layerPresenters.ContainsKey(layerUid))
                return;

            LevelLayerPresenter layerp = _layerPresenters[layerUid];
            _layerPresenters.Remove(layerUid);
            _rootContentLayer.Layers.Remove(layerp);

            UnbindLayerEvents(layerp);
            layerp.Dispose();
        }

        private void SelectLayer ()
        {
            SelectLayer(Guid.Empty);

            foreach (Layer layer in _level.Layers) {
                SelectLayer(layer.Uid);
                return;
            }
        }

        private void SelectLayer (Guid layerUid)
        {
            if (_selectedLayer == layerUid)
                return;

            if (_selectedLayerRef != null) {
                UnbindSelectedLayerEvents(_selectedLayerRef);

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.RemoveCommandSubscriber(comLayer);
                }

                _selectedLayerRef.Deactivate();
            }

            if (layerUid == null || !_layerPresenters.ContainsKey(layerUid)) {
                _selectedLayer = Guid.Empty;
                _selectedLayerRef = null;

                InvalidateLayerCommands();
                RefreshGridVisibility();
                OnPointerEventResponderChanged(EventArgs.Empty);
                return;
            }

            _selectedLayer = layerUid;
            _selectedLayerRef = _layerPresenters[_selectedLayer];

            _info.ActionUpdateCoordinates("");
            _gridLayer.IsVisible = true;

            if (_selectedLayerRef != null) {
                BindSelectedLayerEvents(_selectedLayerRef);

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.AddCommandSubscriber(comLayer);
                }

                if (_selectedLayerRef is LevelLayerPresenter) {
                    //_gridLayer.IsVisible = CommandManager.IsSelected(CommandKey.ViewGrid);
                    _gridLayer.GridColor = (_selectedLayerRef as LevelLayerPresenter).Layer.GridColor;
                    _gridLayer.GridSpacingX = (_selectedLayerRef as LevelLayerPresenter).Layer.GridWidth;
                    _gridLayer.GridSpacingY = (_selectedLayerRef as LevelLayerPresenter).Layer.GridHeight;
                }

                _selectedLayerRef.Activate();
            }

            InvalidateLayerCommands();
            OnPointerEventResponderChanged(EventArgs.Empty);

            _info.RefreshContentInfo();
        }

        private void BindLayerEvents (Layer layer)
        { }

        private void UnbindLayerEvents (Layer layer)
        { }

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

        #region Commands

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();
            //_commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.Undo, CommandCanUndo, CommandUndo);
            _commandManager.Register(CommandKey.Redo, CommandCanRedo, CommandRedo);
            _commandManager.Register(CommandKey.LevelRename, CommandCanRename, CommandRename);
            _commandManager.Register(CommandKey.LevelResize, CommandCanResize, CommandResize);
            _commandManager.Register(CommandKey.LevelProperties, CommandCanLevelProperties, CommandLevelProperties);
            _commandManager.Register(CommandKey.ViewZoomIn, CommandCanZoomIn, CommandZoomIn);
            _commandManager.Register(CommandKey.ViewZoomOut, CommandCanZoomOut, CommandZoomOut);
            _commandManager.Register(CommandKey.ViewZoomNormal, CommandCanZoomNormal, CommandZoomNormal);

            _commandManager.Register(CommandKey.NewTileLayer, CommandCanAddTileLayer, CommandAddTileLayer);
            _commandManager.Register(CommandKey.NewObjectLayer, CommandCanAddObjectLayer, CommandAddObjectLayer);
            _commandManager.Register(CommandKey.LayerEdit, CommandCanEditLayer, CommandEditLayer);
            _commandManager.Register(CommandKey.LayerClone, CommandCanCloneLayer, CommandCloneLayer);
            _commandManager.Register(CommandKey.LayerDelete, CommandCanDeleteLayer, CommandDeleteLayer);
            _commandManager.Register(CommandKey.LayerProperties, CommandCanLayerProperties, CommandLayerProperties);
            _commandManager.Register(CommandKey.LayerMoveTop, CommandCanMoveLayerTop, CommandMoveLayerTop);
            _commandManager.Register(CommandKey.LayerMoveUp, CommandCanMoveLayerUp, CommandMoveLayerUp);
            _commandManager.Register(CommandKey.LayerMoveDown, CommandCanMoveLayerDown, CommandMoveLayerDown);
            _commandManager.Register(CommandKey.LayerMoveBottom, CommandCanMoveLayerBottom, CommandMoveLayerBottom);
            _commandManager.Register(CommandKey.LayerShowAll, CommandCanShowAll, CommandShowAll);
            _commandManager.Register(CommandKey.LayerShowNone, CommandCanShowNone, CommandShowNone);
            _commandManager.Register(CommandKey.LayerShowCurrentOnly, CommandCanShowSelectedOnly, CommandShowSelectedOnly);

            _commandManager.RegisterToggle(CommandKey.ViewGrid, CommandCanToggleGrid, CommandToggleGrid);

            _commandManager.Perform(CommandKey.ViewGrid);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        /*private void HandleCommandInvalidated (object sender, CommandSubscriberEventArgs e)
        {
            _editor.Presentation.DocumentTools.RefreshDocumentTools();
        }*/

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

        private bool CommandCanRename ()
        {
            return true;
        }

        private void CommandRename ()
        {
            if (CommandCanRename()) {
                using (NameChangeForm form = new NameChangeForm(_level.Name)) {
                    foreach (Level lev in _level.Project.Levels)
                        form.ReservedNames.Add(lev.Name);

                    if (form.ShowDialog() == DialogResult.OK) 
                        _level.TrySetName(form.Name);
                }
            }
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
                                _level.OriginX, _level.OriginY,
                                _level.Width, _level.Height);

                        _history.Clear();
                    }
                }
            }
        }

        private bool CommandCanLevelProperties ()
        {
            return true;
        }

        private void CommandLevelProperties ()
        {
            if (CommandCanLevelProperties()) {
                ActivatePropertyProvider(_level);
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

                        SelectLayer(form.Layer.Uid);

                        OnSyncLayerList(EventArgs.Empty);
                        OnSyncLayerSelection(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanEditLayer ()
        {
            return SelectedLayer != null && (
                SelectedLayer.Layer is MultiTileGridLayer ||
                SelectedLayer.Layer is ObjectLayer
                );
        }

        private void CommandEditLayer ()
        {
            if (CommandCanEditLayer()) {
                if (SelectedLayer.Layer is MultiTileGridLayer) {
                    using (TileLayerForm form = new TileLayerForm(SelectedLayer.Layer as MultiTileGridLayer)) {
                        foreach (Layer layer in _level.Layers) {
                            if (layer.Name != SelectedLayer.Layer.Name)
                                form.ReservedNames.Add(layer.Name);
                        }

                        if (form.ShowDialog() == DialogResult.OK) {
                            _gridLayer.GridColor = SelectedLayer.Layer.GridColor;
                            OnSyncLayerList(EventArgs.Empty);
                        }
                    }
                }
                else if (SelectedLayer.Layer is ObjectLayer) {
                    using (ObjectLayerForm form = new ObjectLayerForm(SelectedLayer.Layer as ObjectLayer)) {
                        foreach (Layer layer in _level.Layers) {
                            if (layer.Name != SelectedLayer.Layer.Name)
                                form.ReservedNames.Add(layer.Name);
                        }

                        if (form.ShowDialog() == DialogResult.OK) {
                            _gridLayer.GridSpacingX = SelectedLayer.Layer.GridWidth;
                            _gridLayer.GridSpacingY = SelectedLayer.Layer.GridHeight;
                            _gridLayer.GridColor = SelectedLayer.Layer.GridColor;
                            OnSyncLayerList(EventArgs.Empty);
                        }
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

                SelectLayer(layer.Uid);

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
                string name = FindCloneLayerName(SelectedLayer.LayerName);

                /*Layer layer = null;
                if (_selectedLayerRef is TileLayerPresenter)
                    layer = new MultiTileGridLayer(name, SelectedLayer.Layer as MultiTileGridLayer);
                else if (_selectedLayerRef is ObjectLayerPresenter)
                    layer = new ObjectLayer(name, SelectedLayer.Layer as ObjectLayer);
                else
                    return;*/

                Layer layer = LayerFromPresenterFactory.Default.Create(_selectedLayerRef, name);
                if (layer == null)
                    return;

                _level.Layers.Add(layer);

                // Intercept event instead
                AddLayer(layer);

                SelectLayer(layer.Uid);

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

        private bool CommandCanShowAll ()
        {
            return true;
        }

        private void CommandShowAll ()
        {
            if (CommandCanShowAll()) {
                foreach (Layer layer in _level.Layers) {
                    if (!layer.IsVisible)
                        layer.IsVisible = true;
                }

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private bool CommandCanShowNone ()
        {
            return true;
        }

        private void CommandShowNone ()
        {
            if (CommandCanShowNone()) {
                foreach (Layer layer in _level.Layers) {
                    if (layer.IsVisible)
                        layer.IsVisible = false;
                }

                OnSyncLayerList(EventArgs.Empty);
            }
        }

        private bool CommandCanShowSelectedOnly ()
        {
            return SelectedLayer != null;
        }

        private void CommandShowSelectedOnly ()
        {
            if (CommandCanShowSelectedOnly()) {
                foreach (Layer layer in _level.Layers) {
                    if (layer != SelectedLayer.Layer && layer.IsVisible)
                        layer.IsVisible = false;
                }

                if (SelectedLayer.Layer.IsVisible == false)
                    SelectedLayer.Layer.IsVisible = true;

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

            CommandManager.Invalidate(CommandKey.LayerShowCurrentOnly);
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

        public void ActionSelectLayer (Guid layerUid)
        {
            SelectLayer(layerUid);

            if (SelectedLayer != null)
                _editor.Presentation.PropertyList.Provider = SelectedLayer.Layer;

            OnSyncLayerSelection(EventArgs.Empty);
        }

        public void ActionShowHideLayer (Guid layerUid, LayerVisibility visibility)
        {
            if (!_level.Layers.Contains(layerUid))
                return;

            _level.Layers[layerUid].IsVisible = (visibility == LayerVisibility.Show);
        }

        public void ActionInvalidateLevelGeometry ()
        {
            OnLevelGeometryInvalidated(EventArgs.Empty);
        }

        public void SetPropertyProvider (IPropertyProvider provider)
        {
            _currentPropertyProvider = provider;
            _editor.Presentation.PropertyList.Provider = provider;
        }

        public void ActivatePropertyProvider (IPropertyProvider provider)
        {
            _currentPropertyProvider = provider;
            _editor.Presentation.PropertyList.Provider = provider;
            _editor.ActivatePropertyPanel();
        }

        public event EventHandler<ContextMenuEventArgs> ContextMenuActivated;

        protected virtual void OnContextMenuActivated (ContextMenuEventArgs e)
        {
            var ev = ContextMenuActivated;
            if (ev != null)
                ev(this, e);
        }

        public void ActivateContextMenu (CommandMenu menu, Point location)
        {
            OnContextMenuActivated(new ContextMenuEventArgs(menu, location));
        }
    }
}
