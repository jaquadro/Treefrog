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
    public interface IZoomable
    {
        float Zoom { get; set; }

        event EventHandler ZoomChanged;
    }

    public class DataReportEventArgs : EventArgs
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public DataReportEventArgs (string key, string value)
            : base()
        {
            Key = key;
            Value = value;
        }
    }

    public interface IDataReporter
    {
        event EventHandler<DataReportEventArgs> DataReport;
    }

    public interface IEditorPanel
    {
        void Deactivate ();
        //void Activate (LevelState level, PanelProperties properties);

        PanelProperties PanelProperties { get; }
    }

    public interface IProjectPanel : IEditorPanel
    {
        void Activate (EditorState level, PanelProperties properties);
    }

    public interface ILevelPanel : IEditorPanel
    {
        void Activate (LevelState level, PanelProperties properties);
    }

    public class PanelProperties
    {

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

        public Project Project
        {
            get { return _project; }
        }

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

        public IZoomable CurrentZoomableControl
        {
            get
            {
                if (_curLevel == null) {
                    return null;
                }

                return _curLevel.LevelPanel;
            }
        }

        #endregion

        #region Events

        public event EventHandler ProjectUnloaded;
        public event EventHandler ProjectLoaded;

        public event EventHandler ContentActivated;
        public event EventHandler ContentDeactivated;

        #endregion

        #region Event Dispatchers

        protected virtual void OnProjectLoaded (EventArgs e)
        {
            if (ProjectLoaded != null) {
                ProjectLoaded(this, e);
            }
        }

        protected virtual void OnProjectUnloaded (EventArgs e)
        {
            if (ProjectUnloaded != null) {
                ProjectUnloaded(this, e);
            }
        }

        protected virtual void OnContentActivated (EventArgs e)
        {
            if (ContentActivated != null) {
                ContentActivated(this, e);
            }
        }

        protected virtual void OnContentDeactivated (EventArgs e)
        {
            if (ContentDeactivated != null) {
                ContentDeactivated(this, e);
            }
        }

        #endregion

        public void UnloadProject ()
        {
            _levels.Clear();
            _curLevel = null;

            _contentPages.Clear();
            _tabControlMain.TabPages.Clear();

            //_panelLayers.Deactivate();
            _panelProperties.Deactivate();
            _panelTilePools.Deactivate();

            OnProjectUnloaded(EventArgs.Empty);
        }

        public void LoadProject (Project project)
        {
            _project = project;

            _levels = new Dictionary<string, LevelState>();
            foreach (Level level in _project.Levels) {
                LevelPanel panel = new LevelPanel();
                InitializePanelControl(panel);

                _levels[level.Name] = new LevelState(this, project, level, panel);
                if (_curLevel == null) {
                    _curLevel = _levels[level.Name];
                }

                TabPage levelPage = new TabPage(level.Name);
                InitializeTabPage(levelPage, panel);
                levelPage.Padding = new Padding(0);

                _contentPages.Add(levelPage);
                _tabControlMain.TabPages.Add(levelPage);
            }

            TilePoolPanel.Activate(this, null); // Of type 'ProjectPanel'

            if (_curLevel != null) {
                _curLevel.Activate();
                OnContentActivated(EventArgs.Empty);
            }

            OnProjectLoaded(EventArgs.Empty);
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
