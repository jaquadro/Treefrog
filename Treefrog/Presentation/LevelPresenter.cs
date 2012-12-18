using System;
using System.Linq;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation.Tools;
using Treefrog.Windows.Controls;
using Treefrog.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;
using Treefrog.Windows.Forms;
using Treefrog.Presentation.Controllers;

namespace Treefrog.Presentation
{
    public class SyncLayerEventArgs : EventArgs {
        public Layer PreviousLayer { get; private set; }
        public BaseControlLayer PreviousControlLayer { get; private set; }

        public SyncLayerEventArgs (Layer layer, BaseControlLayer clayer)
        {
            PreviousLayer = layer;
            PreviousControlLayer = clayer;
        }
    }

    public interface ITileLayerPresenter
    {

    }

    public interface ILevelPresenter : ICommandSubscriber
    {
        IContentInfoPresenter InfoPresenter { get; }

        LayerControl LayerControl { get; }

        CommandHistory History { get; }
        ObservableCollection<Annotation> Annotations { get; }

        Level Level { get; }

        event EventHandler<SyncLayerEventArgs> SyncCurrentLayer;

        IEditToolResponder EditToolResponder { get; }
    }

    public partial class LevelPresenter : ILevelPresenter
    {
        private EditorPresenter _editor;

        private Level _level;

        private LayerControl _layerControl;
        private PointerEventController _pointerController;

        private LevelInfoPresenter _info;

        private CommandHistory _history;
        private ObservableCollection<Annotation> _annotations = new ObservableCollection<Annotation>();

        public LevelPresenter (EditorPresenter editor, Level level)
        {
            _editor = editor;
            _level = level;

            _info = new LevelInfoPresenter(this);

            _layerControl = new LayerControl() {
                OriginX = _level.OriginX,
                OriginY = _level.OriginY,
                ReferenceWidth = _level.Width,
                ReferenceHeight = _level.Height,
            };
            _layerControl.MouseLeave += LayerControlMouseLeaveHandler;
            _layerControl.ControlInitialized += LayerControlInitialized;

            _pointerController = new PointerEventController(_layerControl);
            _layerControl.MouseDown += _pointerController.TargetMouseDown;
            _layerControl.MouseUp += _pointerController.TargetMouseUp;
            _layerControl.MouseMove += _pointerController.TargetMouseMove;
            _layerControl.MouseLeave += _pointerController.TargetMouseLeave;

            _layerControl.CanvasLayer = new CanvasLayer(_layerControl);
            _layerControl.AnnotationLayer = new AnnotationLayer(_layerControl);
            _layerControl.AnnotationLayer.Annotations = _annotations;

            _history = new CommandHistory();
            _history.HistoryChanged += HistoryChangedHandler;

            InitializeCommandManager();
            InitializeLayerListPresenter();
        }

        private void LayerControlInitialized (object sender, EventArgs e)
        {
            TilePoolTextureService poolService = new TilePoolTextureService(_editor.Project.TilePoolManager, _layerControl.GraphicsDeviceService);
            _layerControl.Services.AddService<TilePoolTextureService>(poolService);

            ObjectTextureService objPoolService = new ObjectTextureService(_editor.Project.ObjectPoolManager, _layerControl.GraphicsDeviceService);
            _layerControl.Services.AddService<ObjectTextureService>(objPoolService);
        }

        #region ILevelPResenter Members

        public IContentInfoPresenter InfoPresenter
        {
            get { return _info; }
        }

        public Level Level
        {
            get { return _level; }
        }

        public LayerControl LayerControl
        {
            get { return _layerControl; }
        }

        public CommandHistory History
        {
            get { return _history; }
        }

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
        }

        public event EventHandler<SyncLayerEventArgs> SyncCurrentLayer;

        protected virtual void OnSyncCurrentLayer (SyncLayerEventArgs e)
        {
            if (SyncCurrentLayer != null) {
                SyncCurrentLayer(this, e);
            }
        }

        #endregion

        #region Command Handling

        private ForwardingCommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new ForwardingCommandManager();
            _commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.Undo, CommandCanUndo, CommandUndo);
            _commandManager.Register(CommandKey.Redo, CommandCanRedo, CommandRedo);

            _commandManager.Register(CommandKey.NewTileLayer, CommandCanAddTileLayer, CommandAddTileLayer);
            _commandManager.Register(CommandKey.NewObjectLayer, CommandCanAddObjectLayer, CommandAddObjectLayer);
            _commandManager.Register(CommandKey.LayerClone, CommandCanCloneLayer, CommandCloneLayer);
            _commandManager.Register(CommandKey.LayerDelete, CommandCanDeleteLayer, CommandDeleteLayer);
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
            History.Undo();
        }

        private bool CommandCanRedo ()
        {
            return History.CanRedo;
        }

        private void CommandRedo ()
        {
            History.Redo();
        }

        private bool CommandCanToggleGrid ()
        {
            return true;
        }

        private void CommandToggleGrid ()
        {
            _layerControl.ShowGrid = _commandManager.IsSelected(CommandKey.ViewGrid);
        }

        #endregion
    }

    public partial class LevelPresenter : ILayerListPresenter
    {
        #region Fields

        private string _selectedLayer;
        private BaseControlLayer _selectedLayerRef;

        private Dictionary<string, BaseControlLayer> _controlLayers;

        #endregion

        private void InitializeLayerListPresenter ()
        {
            _controlLayers = new Dictionary<string, BaseControlLayer>();

            foreach (Layer layer in _level.Layers) {
                //MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, layer);
                Type controlType = ControlLayerFactory.Lookup(layer.GetType());
                if (controlType == null)
                    continue;

                BaseControlLayer clayer = Activator.CreateInstance(controlType, _layerControl, layer) as BaseControlLayer;
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;

                clayer.BindContentInfoController(_info);

                IPointerToolResponder pointerLayer = clayer as IPointerToolResponder;
                if (pointerLayer != null)
                    pointerLayer.BindLevelController(this);

                ObjectControlLayer objLayer = clayer as ObjectControlLayer;
                if (objLayer != null)
                    objLayer.BindObjectController(_editor.Presentation.ObjectPoolCollection);

                TileControlLayer tileLayer = clayer as TileControlLayer;
                if (tileLayer != null) {
                    tileLayer.BindObjectController(_editor.Presentation.TilePoolList);
                    tileLayer.BindTileBrushManager(_editor.Presentation.TileBrushes);
                }

                _controlLayers[layer.Name] = clayer;

                BindLayerEvents(layer);
            }

            SelectLayer();
        }

        private void BindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged += Layer_NameChanged;
            }
        }

        private void UnbindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged -= Layer_NameChanged;
            }
        }

        private bool CommandCanAddTileLayer ()
        {
            return true;
        }

        private void CommandAddTileLayer ()
        {
            string name = FindDefaultLayerName("Tile Layer");

            using (TileLayerForm form = new TileLayerForm(_level, name)) {
                foreach (Layer layer in _level.Layers)
                    form.ReservedNames.Add(layer.Name);

                if (form.ShowDialog() == DialogResult.OK) {
                    BindLayerEvents(form.Layer);
                    _level.Layers.Add(form.Layer);

                    MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, form.Layer);
                    clayer.ShouldDrawContent = LayerCondition.Always;
                    clayer.ShouldDrawGrid = LayerCondition.Selected;
                    clayer.ShouldRespondToInput = LayerCondition.Selected;

                    clayer.BindContentInfoController(_info);

                    IPointerToolResponder pointerLayer = clayer as IPointerToolResponder;
                    if (pointerLayer != null)
                        pointerLayer.BindLevelController(this);

                    TileControlLayer tileLayer = clayer as TileControlLayer;
                    if (tileLayer != null) {
                        tileLayer.BindObjectController(_editor.Presentation.TilePoolList);
                        tileLayer.BindTileBrushManager(_editor.Presentation.TileBrushes);
                    }

                    _controlLayers[name] = clayer;

                    SelectLayer(name);

                    OnSyncLayerActions(EventArgs.Empty);
                    OnSyncLayerList(EventArgs.Empty);
                    OnSyncLayerSelection(EventArgs.Empty);
                }
            }
        }

        private bool CommandCanAddObjectLayer ()
        {
            return true;
        }

        private void CommandAddObjectLayer ()
        {
            string name = FindDefaultLayerName("Object Layer");

            ObjectLayer layer = new ObjectLayer(name, _level);

            BindLayerEvents(layer);

            _level.Layers.Add(layer);

            ObjectControlLayer clayer = new ObjectControlLayer(_layerControl, layer);
            clayer.ShouldDrawContent = LayerCondition.Always;
            clayer.ShouldDrawGrid = LayerCondition.Selected;
            clayer.ShouldRespondToInput = LayerCondition.Selected;
            clayer.BindLevelController(this);
            clayer.BindObjectController(_editor.Presentation.ObjectPoolCollection);

            _controlLayers[name] = clayer;

            SelectLayer(name);

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        private bool CommandCanCloneLayer ()
        {
            return SelectedLayer != null;
        }

        private void CommandCloneLayer ()
        {
            if (CommandCanCloneLayer() && _controlLayers.ContainsKey(_selectedLayer)) {
                string name = FindCloneLayerName(SelectedLayer.Name);

                MultiTileGridLayer layer = new MultiTileGridLayer(name, SelectedLayer as MultiTileGridLayer);

                BindLayerEvents(layer);

                _level.Layers.Add(layer);

                MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, layer);
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;

                _controlLayers[name] = clayer;

                SelectLayer(name);

                OnSyncLayerActions(EventArgs.Empty);
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
            if (CommandCanDeleteLayer() && _controlLayers.ContainsKey(_selectedLayer)) {
                UnbindLayerEvents(_level.Layers[_selectedLayer]);

                _level.Layers.Remove(_selectedLayer);
                _layerControl.RemoveLayer(_controlLayers[_selectedLayer]);
                _controlLayers.Remove(_selectedLayer);

                SelectLayer();
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
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
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], count - index);

                InvalidateLayerViewCommands();
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        private bool CommandCanMoveLayerUp ()
        {
            return (SelectedLayer != null && _level.Layers.IndexOf(_selectedLayer) < _level.Layers.Count - 1);
        }

        private void CommandMoveLayerUp ()
        {
            if (CommandCanMoveLayerUp()) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, 1);
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], 1);

                InvalidateLayerViewCommands();
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        private bool CommandCanMoveLayerDown ()
        {
            return (SelectedLayer != null && _level.Layers.IndexOf(_selectedLayer) > 0);
        }

        private void CommandMoveLayerDown ()
        {
            if (CommandCanMoveLayerDown()) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, -1);
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], -1);

                InvalidateLayerViewCommands();
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
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
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], 0 - index);

                InvalidateLayerViewCommands();
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        private void InvalidateLayerViewCommands ()
        {
            CommandManager.Invalidate(CommandKey.LayerMoveTop);
            CommandManager.Invalidate(CommandKey.LayerMoveUp);
            CommandManager.Invalidate(CommandKey.LayerMoveDown);
            CommandManager.Invalidate(CommandKey.LayerMoveBottom);
        }

        #region Properties

        public bool CanShowSelectedLayerProperties
        {
            get { return SelectedLayer != null; }
        }

        public IEnumerable<Layer> LayerList
        {
            get { return _level.Layers; }
        }

        public Layer SelectedLayer
        {
            get
            {
                return (_selectedLayer != null && _level.Layers.Contains(_selectedLayer))
                    ? _level.Layers[_selectedLayer]
                    : null;
            }
        }

        public BaseControlLayer SelectedControlLayer
        {
            get
            {
                return (SelectedLayer != null && _controlLayers.ContainsKey(_selectedLayer))
                    ? _controlLayers[_selectedLayer]
                    : null;
            }
        }

        public IEditToolResponder EditToolResponder
        {
            get
            {
                return (SelectedControlLayer != null)
                    ? SelectedControlLayer.EditToolResponder
                    : null;
            }
        }

        #endregion

        #region Events

        public event EventHandler SyncLayerActions;

        public event EventHandler SyncLayerList;

        public event EventHandler SyncLayerSelection;

        #endregion

        #region Event Dispatchers

        protected void OnSyncLayerActions (EventArgs e)
        {
            if (SyncLayerActions != null) {
                SyncLayerActions(this, e);
            }
        }

        protected void OnSyncLayerList (EventArgs e)
        {
            if (SyncLayerList != null) {
                SyncLayerList(this, e);
            }
        }

        protected void OnSyncLayerSelection (EventArgs e)
        {
            if (SyncLayerSelection != null) {
                SyncLayerSelection(this, e);
            }

            //_editor.Presentation.LevelTools.RefreshLevelTools();
        }

        #endregion

        private void Layer_NameChanged (object sender, NameChangedEventArgs e) 
        {
            if (_controlLayers.ContainsKey(e.OldName)) {
                _controlLayers[e.NewName] = _controlLayers[e.OldName];
                _controlLayers.Remove(e.OldName);
            }

            if (_selectedLayer == e.OldName)
                _selectedLayer = e.NewName;

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        #region View Action API

        public void ActionSelectLayer (string name)
        {
            SelectLayer(name);

            _editor.Presentation.PropertyList.Provider = SelectedLayer;

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        public void ActionShowSelectedLayerProperties ()
        {
            if (CanShowSelectedLayerProperties) {
                _editor.Presentation.PropertyList.Provider = SelectedLayer;
            }
        }

        public void ActionShowHideLayer (string name, LayerVisibility visibility)
        {
            if (name != null && _controlLayers.ContainsKey(name)) {
                BaseControlLayer layer = _controlLayers[name];
                layer.Visible = (visibility == LayerVisibility.Show);
            }
        }

        #endregion

        private void SelectLayer ()
        {
            SelectLayer(null);

            foreach (Layer layer in _level.Layers) {
                SelectLayer(layer.Name);
                return;
            }
        }

        private void SelectLayer (string layer)
        {
            Layer prevLayer = SelectedLayer;
            BaseControlLayer prevControlLayer = SelectedControlLayer;

            if (_selectedLayer == layer) {
                return;
            }

            // Unbind previously selected layer if necessary
            if (_selectedLayerRef != null) {
                _selectedLayerRef.Deactivate();
                _selectedLayerRef.Selected = false;

                TileControlLayer tileLayer = _selectedLayerRef as TileControlLayer;
                if (tileLayer != null) {
                    tileLayer.MouseTileMove -= LayerMouseMoveHandler;
                }

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.RemoveCommandSubscriber(comLayer);
                }
            }

            _selectedLayer = null;
            _selectedLayerRef = null;
            _pointerController.Responder = null;

            _info.ActionUpdateCoordinates("");

            // Bind new layer
            if (layer != null && _controlLayers.ContainsKey(layer)) {
                _selectedLayer = layer;
                _selectedLayerRef = _controlLayers[layer];

                _selectedLayerRef.Activate();
                _selectedLayerRef.Selected = true;
                _selectedLayerRef.ApplyScrollAttributes();

                TileControlLayer tileLayer = _selectedLayerRef as TileControlLayer;
                if (tileLayer != null) {
                    tileLayer.MouseTileMove += LayerMouseMoveHandler;
                }

                ICommandSubscriber comLayer = _selectedLayerRef as ICommandSubscriber;
                if (comLayer != null) {
                    _commandManager.AddCommandSubscriber(comLayer);
                }

                if (!_level.Layers.Contains(layer)) {
                    throw new InvalidOperationException("Selected a ControlLayer with no corresponding model Layer!  Selected name: " + layer);
                }

                _pointerController.Responder = SelectedControlLayer as IPointerToolResponder;
            }

            OnSyncCurrentLayer(new SyncLayerEventArgs(prevLayer, prevControlLayer));

            _info.RefreshContentInfo();
        }

        private void LayerMouseMoveHandler (object sender, TileMouseEventArgs e)
        {
            _info.ActionUpdateCoordinates(e.TileLocation.X + ", " + e.TileLocation.Y);
        }

        private void LayerControlMouseLeaveHandler (object sender, EventArgs e)
        {
            _info.ActionUpdateCoordinates("");
        }

        private void HistoryChangedHandler (object sender, EventArgs e)
        {
            //_editor.Presentation.DocumentTools.RefreshDocumentTools();
            CommandManager.Invalidate(CommandKey.Undo);
            CommandManager.Invalidate(CommandKey.Redo);
        }

        public void RefreshLayerList ()
        {
            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
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
    }
}
