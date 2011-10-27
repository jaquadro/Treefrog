using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Editor.Views;
using Editor.Model.Controls;

namespace Editor
{
    public class LayerPresentation
    {
        public Layer CurrentLayer { get; }
        public BaseControlLayer CurrentControl { get; }
    }

    public class TilePoolPresentation
    {
        public TilePool CurrentTilePool { get; }
        public TileSetControlLayer CurrentControl { get; }
    }

    public class LevelPresentation
    {
        public Level Level { get; }
        public LayerControl LayerControl { get; }

        public LayerPresentation LayerInfo { get; }
        public TilePoolPresentation TilePoolInfo { get; }

        public float Zoom { get; }
    }

    public class ProjectPresentation
    {
        public Project Project { get; }

        public LevelPresentation CurrentLevel { get; }
    }

    public class LevelController
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

    public class ProjectController
    {

    }

    public class LevelState
    {
        #region Fields

        private EditorState _editor;

        private Project _project;
        private Level _level;

        private CommandHistory _commandHistory;

        private Dictionary<Type, PanelProperties> _panelProperties;

        private LevelPanel _levelPanel;

        #endregion

        public LevelState (EditorState editor, Project project, Level level, LevelPanel panel)
        {
            _editor = editor;
            _project = project;
            _level = level;
            _levelPanel = panel;

            _commandHistory = new CommandHistory();

            _panelProperties = new Dictionary<Type, PanelProperties>();

            Initialize();
        }

        #region Properties

        public Project Project
        {
            get { return _project; }
        }

        public Level Level
        {
            get { return _level; }
        }

        public LayerControl LayerControl
        {
            get { return _levelPanel.LayerControl; }
        }

        public LevelPanel LevelPanel
        {
            get { return _levelPanel; }
        }

        public CommandHistory CommandHistory
        {
            get { return _commandHistory; }
        }

        #endregion

        #region Event Handlers

        private void SelectedLayerChangedHandler (object sender, EventArgs e)
        {

        }

        #endregion

        public void Initialize ()
        {
            foreach (Layer layer in _level.Layers) {
                MultiTileControlLayer clayer = new MultiTileControlLayer(_levelPanel.LayerControl, layer);
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;
            }

            _panelProperties[_editor.LayerPanel.GetType()] = null;
            _panelProperties[_editor.PropertyPanel.GetType()] = null;
            _panelProperties[_editor.TilePoolPanel.GetType()] = null;
        }

        public void Activate ()
        {
            _editor.LayerPanel.Activate(this, _panelProperties[_editor.LayerPanel.GetType()]);  // Of type 'LevelPanel'
            _editor.PropertyPanel.Activate(this, _panelProperties[_editor.PropertyPanel.GetType()]); // Of type 'Panel'
        }

        public void Deactivate ()
        {
            _panelProperties[_editor.LayerPanel.GetType()] = _editor.LayerPanel.PanelProperties;
            _panelProperties[_editor.PropertyPanel.GetType()] = _editor.PropertyPanel.PanelProperties;
        }
    }
}
