using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Views;
using Treefrog.Framework.Model;
using Editor.Model.Controls;

namespace Editor
{
    public interface IEditorPanel
    {
        void Deactivate ();
        void Activate (LevelState level, PanelProperties properties);

        PanelProperties PanelProperties { get; }
    }

    public class PanelProperties
    {

    }

    public class LevelState
    {
        #region Fields

        private EditorState _editor;

        private Project _project;
        private Level _level;

        private LayerControl _layerControl;
        private CommandHistory _commandHistory;

        private Dictionary<Type, PanelProperties> _panelProperties;

        #endregion

        public LevelState (EditorState editor, Project project, Level level)
        {
            _editor = editor;
            _project = project;
            _level = level;

            _layerControl = new LayerControl();
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
            get { return _layerControl; }
        }

        public CommandHistory CommandHistory
        {
            get { return _commandHistory; }
        }

        #endregion

        public void Initialize ()
        {
            foreach (Layer layer in _level.Layers) {
                MultiTileControlLayer clayer = new MultiTileControlLayer(_layerControl, layer);
                clayer.ShouldDrawContent = LayerCondition.Always;
                clayer.ShouldDrawGrid = LayerCondition.Selected;
                clayer.ShouldRespondToInput = LayerCondition.Selected;
            }
        }

        public void Activate ()
        {
            _editor.LayerPanel.Activate(this, _panelProperties[_editor.LayerPanel.GetType()]);
        }

        public void Deactivate ()
        {
            _panelProperties[_editor.LayerPanel.GetType()] = _editor.LayerPanel.PanelProperties;
            _panelProperties[_editor.PropertyPanel.GetType()] = _editor.PropertyPanel.PanelProperties;
            _panelProperties[_editor.TilePoolPanel.GetType()] = _editor.TilePoolPanel.PanelProperties;
        }
    }

    public class EditorState
    {
        #region Fields

        private Project _project;

        private Dictionary<string, LevelState> _levels;

        // Tab Control Areas

        private TabControl _tabControlMain;
        private TabControl _tabControlLeftUpper;
        private TabControl _tabControlLeftLower;

        // Tab Control Pages

        private TabPage _tabPageLayers;
        private TabPage _tabPageProperties;
        private TabPage _tabPageTilePools;

        private List<TabPage> _contentPages;

        // Panels

        private LayerPane _panelLayers;
        private PropertyPane _panelProperties;
        private TilePoolPane _panelTilePools;

        #endregion

        #region Constructors

        public EditorState ()
        {
            
        }

        #endregion

        #region Properties

        public LayerPane LayerPanel
        {
            get { return _panelLayers; }
        }

        public PropertyPane PropertyPanel
        {
            get { return _panelProperties; }
        }

        public TilePoolPane TilePoolPanel
        {
            get { return _panelTilePools; }
        }

        #endregion

        public void UnloadProject ()
        {
            _levels.Clear();

            _contentPages.Clear();
            _tabControlMain.TabPages.Clear();
        }

        public void LoadProject (Project project)
        {
            _project = project;

            _levels = new Dictionary<string, LevelState>();
            foreach (Level level in _project.Levels) {
                _levels[level.Name] = new LevelState(this, project, level);
            }
        }
    }
}
