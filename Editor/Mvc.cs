using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Editor.Model.Controls;
using Editor.Views;

namespace Editor
{
    public class LayerPresentation
    {
        #region Fields

        private LevelPresentation _level;

        private string _activeLayer;
        private Dictionary<string, BaseControlLayer> _controlLayers;

        #endregion

        #region Constructors

        public LayerPresentation (LevelPresentation parent)
        {
            _level = parent;
            _controlLayers = new Dictionary<string, BaseControlLayer>();

            _level.LayerControlChanged += LevelLayerControlChangedHandler;
        }

        #endregion

        #region Properties

        public Layer CurrentLayer 
        {
            get
            {
                if (!_level.Level.Layers.Contains(_activeLayer)) {
                    return null;
                }
                return _level.Level.Layers[_activeLayer];
            }

            set
            {
                if (!_level.Level.Layers.Contains(value)) {
                    throw new Exception("Tried to set a current layer that does not exist.");
                }

                if (_activeLayer != value.Name) {
                    _activeLayer = value.Name;
                    OnCurrentLayerChanged(EventArgs.Empty);
                }
            }
        }

        public BaseControlLayer CurrentControl
        {
            get
            {
                if (!_controlLayers.ContainsKey(_activeLayer)) {
                    return null;
                }
                return _controlLayers[_activeLayer];
            }
        }

        #endregion

        #region Events

        public event EventHandler CurrentLayerChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnCurrentLayerChanged (EventArgs e)
        {
            if (CurrentLayerChanged != null) {
                CurrentLayerChanged(this, e);
            }
        }

        #endregion

        #region EventHandlers

        private void LevelLayerControlChangedHandler (object sender, EventArgs e)
        {
            _controlLayers.Clear();

            foreach (Layer layer in _level.Level.Layers) {
                MultiTileControlLayer clayer = new MultiTileControlLayer(_level.LayerControl, layer);
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;

                _controlLayers[layer.Name] = clayer;
            }
        }

        #endregion
    }

    public class TilePoolPresentation
    {
        #region Fields

        private EditorPresentation _editor;

        private TileSetControlLayer _controlLayer;

        private string _activePool;

        #endregion

        #region Constructors

        public TilePoolPresentation (EditorPresentation parent)
        {
            _editor.TilePoolLayerControlChanged += TilePoolLayerControlChangedHandler;
        }

        #endregion

        #region Properties

        public TilePool CurrentTilePool
        {
            get
            {
                if (!_editor.Project.TilePools.Contains(_activePool)) {
                    return null;
                }
                return _editor.Project.TilePools[_activePool];
            }

            set
            {
                if (!_editor.Project.TilePools.Contains(value)) {
                    throw new Exception("Tried to set a current tile pool that does not exist.");
                }

                if (_activePool != value.Name) {
                    _activePool = value.Name;
                    OnCurrentTilePoolChanged(EventArgs.Empty);
                }
            }
        }

        public TileSetControlLayer CurrentControl
        {
            get { return _controlLayer; }
        }

        #endregion

        #region Events

        public event EventHandler CurrentTilePoolChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnCurrentTilePoolChanged (EventArgs e)
        {
            if (CurrentTilePoolChanged != null) {
                CurrentTilePoolChanged(this, e);
            }
        }

        #endregion

        #region Event Handlers

        private void TilePoolLayerControlChangedHandler (object sender, EventArgs e)
        {
            foreach (BaseControlLayer control in _editor.TilePoolLayerControl.ControlLayers) {
                _controlLayer = control as TileSetControlLayer;
                if (_controlLayer == null) {
                    throw new Exception("Expected TileSetControlLayer in TilePool LayerControl");
                }
                break;
            }
        }

        #endregion
    }

    public class LevelPresentation
    {
        #region Fields

        private EditorPresentation _editor;
        private LayerPresentation _layer;
        private TilePoolPresentation _tilePool;

        private Level _level;

        private LayerControl _control;

        #endregion

        #region Constructors

        public LevelPresentation (EditorPresentation parent, Level level)
        {
            _editor = parent;
            _level = level;
        }

        #endregion

        #region Properties

        public Level Level
        {
            get { return _level; }
        }

        public LayerControl LayerControl
        {
            get { return _control; }
            set
            {
                if (_control != value) {
                    _control = value;
                    OnLayerControlChanged(EventArgs.Empty);
                }
            }
        }

        public LayerPresentation LayerInfo
        {
            get { return _layer; }
        }

        public TilePoolPresentation TilePoolInfo
        {
            get { return _tilePool; }
        }

        public float Zoom
        {
            get { return _control.Zoom; }
        }

        #endregion

        #region Events

        public event EventHandler LayerControlChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnLayerControlChanged (EventArgs e)
        {
            if (LayerControlChanged != null) {
                LayerControlChanged(this, e);
            }
        }

        #endregion
    }

    public class EditorPresentation
    {
        #region Fields

        private Project _project;

        private Dictionary<string, LevelPresentation> _levels;

        private LayerControl _tilePoolControl;

        private string _activeLevel;

        #endregion

        #region Constructors

        public EditorPresentation (Project project)
        {
            _project = project;
            _levels = new Dictionary<string, LevelPresentation>();

            foreach (Level level in project.Levels) {
                _levels[level.Name] = new LevelPresentation(this, level);
            }
        }

        #endregion

        #region Properties

        public Project Project 
        {
            get { return _project; }
        }

        public LevelPresentation CurrentLevel 
        {
            get
            {
                if (_levels == null || !_levels.ContainsKey(_activeLevel)) {
                    return null;
                }
                return _levels[_activeLevel];
            }

            set
            {
                if (!_levels.ContainsValue(value)) {
                    throw new ArgumentException("Tried to set current level that does not exist in the level collection.");
                }

                if (_activeLevel != value.Level.Name) {
                    _activeLevel = value.Level.Name;
                    OnCurrentLevelChanged(EventArgs.Empty);
                }
            }
        }

        public LayerControl TilePoolLayerControl
        {
            get { return _tilePoolControl; }
            set
            {
                if (_tilePoolControl != value) {
                    _tilePoolControl = value;
                    OnTilePoolLayerControlChanged(EventArgs.Empty);
                }
            }
        }
        
        public IDictionary<string, LevelPresentation> Levels
        {
            get { return _levels; }
        }

        #endregion

        #region Events

        public event EventHandler CurrentLevelChanged;
        public event EventHandler TilePoolLayerControlChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnCurrentLevelChanged (EventArgs e)
        {
            if (CurrentLevelChanged != null) {
                CurrentLevelChanged(this, e);
            }
        }

        protected virtual void OnTilePoolLayerControlChanged (EventArgs e)
        {
            if (TilePoolLayerControlChanged != null) {
                TilePoolLayerControlChanged(this, e);
            }
        }

        #endregion
    }

    /*public class LevelController
    {
        private LevelPresentation _levelModel;
        private LevelPanel _levelView;

        private void LayerAddHandler (object sender, EventArgs e);
        private void LayerRemoveHandler (object sender, EventArgs e);
        private void LayerSelectionChangedHandler (object sender, EventArgs e);
        private void LayerMoveUpHandler (object sender, EventArgs e);
        private void LayerMoveDownHandler (object sender, EventArgs e);

        private void TilePoolSelectionChangedHandler (object sender, EventArgs e);
        private void TilePoolTileMouseClickHandler (object sender, TileMouseEventArgs e);

        private void LevelTileMouseClickHandler (object sender, TileMouseEventArgs e);  // Tools
        private void LevelTileMouseDownHandler (object sender, TileMouseEventArgs e); // Tools
        private void LevelTileMouseUpHandler (object sender, TileMouseEventArgs e); // Tools
        private void LevelTileMouseMoveHandler (object sender, TileMouseEventArgs e); // Tools, Reporting

        private void LevelZoomChangedHandler (object sender, EventArgs e);
    }

    public class EditorController
    {
        private EditorPresentation _editorModel;
        private Form1 _editorView;

        public EditorController (Form1 view) {
            _editorView = view;
        }

        private void TilePoolAddHandler (object sender, EventArgs e);
        private void TilePoolRemoveHandler (object sender, EventArgs e);
    }*/
}
