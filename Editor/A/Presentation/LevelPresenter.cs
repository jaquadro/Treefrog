using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Editor.Model.Controls;

namespace Editor.A.Presentation
{
    public interface ITileLayerPresenter
    {

    }

    public interface ILevelPresenter
    {
        LayerControl LayerControl { get; }
    }

    public class LevelPresenter : ILevelPresenter, ILayerListPresenter
    {
        private EditorPresenter _editor;

        private Level _level;

        private LayerControl _layerControl;

        private DrawTool _drawTool;

        public LevelPresenter (EditorPresenter editor, Level level)
        {
            _editor = editor;
            _level = level;

            _layerControl = new LayerControl();
            _controlLayers = new Dictionary<string, BaseControlLayer>();

            foreach (Layer layer in _level.Layers) {
                MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, layer);
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;

                _controlLayers[layer.Name] = clayer;

                if (_selectedLayer == null) {
                    _selectedLayer = layer.Name;
                    clayer.Selected = true;
                }
            }

            _drawTool = new DrawTool(this);
            _drawTool.BindLevelToolsController(_editor.CurrentLevelToolsPresenter);
            _drawTool.BindTileSourceController(_editor.CurrentTilePoolListPresenter);
        }


        #region ILevelPResenter Members

        public LayerControl LayerControl
        {
            get { return _layerControl; }
        }

        #endregion

        #region ILayerListPresenter Members

        #region Fields

        private string _selectedLayer;
        private Dictionary<string, BaseControlLayer> _controlLayers;

        #endregion

        #region Properties

        public bool CanAddLayer
        {
            get { return true; }
        }

        public bool CanRemoveSelectedLayer
        {
            get { return SelectedLayer != null; }
        }

        public bool CanCloneSelectedLayer
        {
            get { return SelectedLayer != null; }
        }

        public bool CanMoveSelectedLayerUp
        {
            get { return (SelectedLayer != null && _level.Layers.IndexOf(_selectedLayer) < _level.Layers.Count - 1); }
        }

        public bool CanMoveSelectedLayerDown
        {
            get { return (SelectedLayer != null && _level.Layers.IndexOf(_selectedLayer) > 0); }
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

        #endregion

        #region Events

        public event EventHandler PreSyncLayerSelection;

        public event EventHandler SyncLayerActions;

        public event EventHandler SyncLayerList;

        public event EventHandler SyncLayerSelection;

        #endregion

        #region Event Dispatchers

        protected void OnPreSyncLayerSelection (EventArgs e)
        {
            if (PreSyncLayerSelection != null) {
                PreSyncLayerSelection(this, e);
            }
        }

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
        }

        #endregion

        #region View Action API

        public void ActionAddLayer ()
        {
            OnPreSyncLayerSelection(EventArgs.Empty);

            string name = FindDefaultLayerName();

            MultiTileGridLayer layer = new MultiTileGridLayer(name, _level.TileWidth, _level.TileHeight, _level.TilesWide, _level.TilesHigh);
            _level.Layers.Add(layer);

            MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, layer);
            clayer.ShouldDrawContent = LayerCondition.Always;
            clayer.ShouldDrawGrid = LayerCondition.Selected;
            clayer.ShouldRespondToInput = LayerCondition.Selected;

            _controlLayers[name] = clayer;

            foreach (MultiTileControlLayer cl in _controlLayers.Values) {
                cl.Selected = false;
            }

            _selectedLayer = name;
            clayer.Selected = true;

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        public void ActionRemoveSelectedLayer ()
        {
            if (CanRemoveSelectedLayer) {
                OnPreSyncLayerSelection(EventArgs.Empty);

                _level.Layers.Remove(_selectedLayer);
                _layerControl.RemoveLayer(_controlLayers[_selectedLayer]);
                _controlLayers.Remove(_selectedLayer);
            }

            foreach (MultiTileControlLayer cl in _controlLayers.Values) {
                _selectedLayer = cl.Layer.Name;
                cl.Selected = true;
                break;
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        public void ActionCloneSelectedLayer ()
        {
            throw new NotImplementedException();
        }

        public void ActionMoveSelectedLayerUp ()
        {
            if (CanMoveSelectedLayerUp) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, 1);
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], 1);
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        public void ActionMoveSelectedLayerDown ()
        {
            if (CanMoveSelectedLayerDown) {
                _level.Layers.ChangeIndexRelative(_selectedLayer, -1);
                _layerControl.ChangeLayerOrderRelative(_controlLayers[_selectedLayer], -1);
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        public void ActionSelectLayer (string name)
        {
            if (name != _selectedLayer && _level.Layers.Contains(name)) {
                OnPreSyncLayerSelection(EventArgs.Empty);
                _selectedLayer = name;
            }

            foreach (MultiTileControlLayer cl in _controlLayers.Values) {
                cl.Selected = (cl.Layer.Name != name);
            }

            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerSelection(EventArgs.Empty);
        }

        #endregion

        public void RefreshLayerList ()
        {
            OnSyncLayerActions(EventArgs.Empty);
            OnSyncLayerList(EventArgs.Empty);
        }

        private string FindDefaultLayerName ()
        {
            List<string> names = new List<string>();
            foreach (Layer layer in _level.Layers) {
                names.Add(layer.Name);
            }

            int i = 0;
            while (true) {
                string name = "Tile Layer " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }

        #endregion
    }
}
