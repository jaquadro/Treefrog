using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Views;
using Treefrog.Framework.Model;
using Editor.Model.Controls;
using System.Drawing;

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

            _panelProperties[_editor.LayerPanel.GetType()] = null;
            _panelProperties[_editor.PropertyPanel.GetType()] = null;
            _panelProperties[_editor.TilePoolPanel.GetType()] = null;
        }

        public void Activate ()
        {
            _editor.LayerPanel.Activate(this, _panelProperties[_editor.LayerPanel.GetType()]);  // Of type 'LevelPanel'
            _editor.PropertyPanel.Activate(this, _panelProperties[_editor.PropertyPanel.GetType()]); // Of type 'Panel'
            _editor.TilePoolPanel.Activate(this, _panelProperties[_editor.TilePoolPanel.GetType()]); // Of type 'ProjectPanel'
        }

        public void Deactivate ()
        {
            _panelProperties[_editor.LayerPanel.GetType()] = _editor.LayerPanel.PanelProperties;
            _panelProperties[_editor.PropertyPanel.GetType()] = _editor.PropertyPanel.PanelProperties;
            _panelProperties[_editor.TilePoolPanel.GetType()] = _editor.TilePoolPanel.PanelProperties;
        }
    }

    public enum EditorContentType
    {
        None,
        Level,
    }

    public class EditorState
    {
        #region Fields

        private Project _project;

        private Dictionary<string, LevelState> _levels;

        private LevelState _curLevel;

        // Tab Control Areas

        private TabControl _tabControlMain;
        private TabControl _tabControlUpperLeft;
        private TabControl _tabControlLowerLeft;

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
            Initialize();

            _contentPages = new List<TabPage>();
        }

        #endregion

        #region Properties

        public TabControl MainTabControl
        {
            get { return _tabControlMain; }
        }

        public TabControl UpperLeftTabControl
        {
            get { return _tabControlUpperLeft; }
        }

        public TabControl LowerLeftTabControl
        {
            get { return _tabControlLowerLeft; }
        }

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

        public LevelState CurrentLevel
        {
            get { return _curLevel; }
        }

        public EditorContentType CurrentContentType
        {
            get
            {
                if (_curLevel == null) {
                    return EditorContentType.None;
                }
                else {
                    return EditorContentType.Level;
                }
            }
        }

        #endregion

        public void UnloadProject ()
        {
            _levels.Clear();
            _curLevel = null;

            _contentPages.Clear();
            _tabControlMain.TabPages.Clear();
        }

        public void LoadProject (Project project)
        {
            _project = project;

            _levels = new Dictionary<string, LevelState>();
            foreach (Level level in _project.Levels) {
                _levels[level.Name] = new LevelState(this, project, level);
                if (_curLevel == null) {
                    _curLevel = _levels[level.Name];
                }
            }

            if (_curLevel != null) {
                _curLevel.Activate();
            }
        }

        #region Form Initialization

        private void Initialize ()
        {
            // Create Compont Panels

            _panelLayers = new LayerPane();
            _panelProperties = new PropertyPane();
            _panelTilePools = new TilePoolPane();

            InitializePanelControl(_panelLayers);
            InitializePanelControl(_panelProperties);
            InitializePanelControl(_panelTilePools);

            // Create Various Tab Pages

            _tabPageLayers = new TabPage("Layers");
            InitializeTabPage(_tabPageLayers, _panelLayers);

            _tabPageProperties = new TabPage("Properties");
            InitializeTabPage(_tabPageProperties, _panelProperties);

            _tabPageTilePools = new TabPage("Tile Pools");
            InitializeTabPage(_tabPageTilePools, _panelTilePools);

            // Setup Tab Controls

            _tabControlMain = new TabControlEx();
            _tabControlUpperLeft = new TabControl();
            _tabControlLowerLeft = new TabControl();

            InitializeTabControl(_tabControlMain);
            InitializeTabControl(_tabControlUpperLeft);
            InitializeTabControl(_tabControlLowerLeft);

            _tabControlUpperLeft.TabPages.Add(_tabPageTilePools);
            _tabControlLowerLeft.TabPages.Add(_tabPageLayers);
            _tabControlLowerLeft.TabPages.Add(_tabPageProperties);
        }

        private void InitializeTabControl (TabControl control)
        {
            control.Dock = DockStyle.Fill;
            control.Location = new Point(0, 0);
            control.Margin = new Padding(0);
            control.SelectedIndex = 0;
            control.TabStop = false;
        }

        private void InitializeTabPage (TabPage page, Control control)
        {
            page.Margin = new Padding(0);
            page.Padding = new Padding(0, 0, 2, 1);
            page.UseVisualStyleBackColor = true;
            page.Controls.Add(control);
        }

        private void InitializePanelControl (Control control)
        {
            control.Dock = DockStyle.Fill;
            control.Location = new Point(0, 0);
            control.Margin = new Padding(0);
            control.TabStop = false;
        }

        #endregion
    }
}
