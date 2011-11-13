using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Editor.A.Presentation;
using Editor.Views;

namespace Editor
{
    public partial class Form1 : Form
    {
        private StandardToolbar _standardToolbar;
        private TileToolbar _tileToolbar;

        private Image _imgMinusCircle;
        private Image _imgMinusCircleClk;
        private Image _imgMinusCircleMo;

        private Image _imgPlusCircle;
        private Image _imgPlusCircleClk;
        private Image _imgPlusCircleMo;

        private TrackBar trackBarZoom;

        //private TilesetView _tilesetView;
        //private MapView _mapView;

        private Project _project;

        //private EditorState _editor;

        private EditorPresenter _editor;

        public Form1 ()
        {
            InitializeComponent();

            // Toolbars

            _standardToolbar = new StandardToolbar();
            /*_standardToolbar.ButtonUndo.Click += ButtonUndo;
            _standardToolbar.ButtonRedo.Click += ButtonRedo;
            _standardToolbar.ButtonSave.Click += ButtonSave;
            _standardToolbar.ButtonOpen.Click += ButtonOpen;*/

            _tileToolbar = new TileToolbar();
            //_tileToolbar.ToolModeChanged += TileToolModeChangedHandler;

            toolStripContainer1.TopToolStripPanel.Controls.AddRange(new Control[] {
                _standardToolbar.Strip, 
                _tileToolbar.Strip
            });

            // Other

            //_editor = new EditorState();

            //_editor.ContentActivated += EditorActivateContent;
            //_editor.ContentDeactivated += EditorDeactivateContent;

            //splitContainer1.Panel2.Controls.Add(_editor.MainTabControl);
            //splitContainer2.Panel1.Controls.Add(_editor.UpperLeftTabControl);
            //splitContainer2.Panel2.Controls.Add(_editor.LowerLeftTabControl);

            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(Handle, 128, 128);

            _project = new Project();
            _project.Initialize(gds.GraphicsDevice);
            //_project.SetupDefaults();

            //_tilesetView = new TilesetView(this, _project);
            //_tilesetView.Dock = DockStyle.Fill;

            Level level = new Level("Level 1", 16, 16, 30, 20);
            Property prop1 = new StringProperty("background", "dirt");
            Property prop2 = new StringProperty("music", "rocks.ogg");
            level.Properties.Add(prop1);
            level.Properties.Add(prop2);

            _project.Levels.Add(level);

            level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 30, 20));

            Level level2 = new Level("Level 2", 16, 16, 80, 30);
            _project.Levels.Add(level2);

            level2.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 80, 30));

            _editor = new EditorPresenter(_project);
            _editor.SyncContentTabs += SyncContentTabsHandler;
            _editor.SyncContentView += SyncContentViewHandler;

            _editor.RefreshEditor();

            //_editor.LoadProject(_project);

            //_mapView = new MapView(_project, "Level 1");
            //_mapView.Dock = DockStyle.Fill;

            //SwitchToView(_tilesetView);
            //SwitchToView(_mapView);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            statusCoord.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.status-loc.png"));
            statusCoord.ImageScaling = ToolStripItemImageScaling.None;

            redoToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-turn.png"));
            undoToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.arrow-turn-180-left.png"));

            cutToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.scissors.png"));
            copyToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.documents.png"));
            pasteToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.clipboard-paste.png"));
            deleteToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.cross.png"));

            selectAllToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.selection-select.png"));
            selectNoneToolStripMenuItem.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons._16.selection.png"));

            // TilesetView

            _imgMinusCircle = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus-circle16.png"));
            _imgMinusCircleClk = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus-circle-clk16.png"));
            _imgMinusCircleMo = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus-circle-mo16.png"));

            _imgPlusCircle = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus-circle16.png"));
            _imgPlusCircleClk = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus-circle-clk16.png"));
            _imgPlusCircleMo = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus-circle-mo16.png"));

            ToolStripButton tsb = new ToolStripButton();
            tsb.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.map16.png"));
            tsb.CheckOnClick = true;
            tsb.Margin = new Padding(0);
            tsb.Padding = new Padding(0);
            tsb.AutoSize = false;
            tsb.ImageScaling = ToolStripItemImageScaling.None;
            tsb.Width = 20;
            tsb.Height = 20;


            ToolStripItem tsbItem = tsb;
            this.menuBar.Items.Add(tsbItem);

            // Statusbar Zoom

            trackBarZoom = new TrackBar();
            trackBarZoom.AutoSize = false;
            trackBarZoom.Height = 22;
            trackBarZoom.TickStyle = TickStyle.None;
            trackBarZoom.Anchor = AnchorStyles.Right;
            trackBarZoom.Minimum = 0;
            trackBarZoom.Maximum = 7;
            trackBarZoom.Value = 2;
            trackBarZoom.BackColor = SystemColors.ControlLightLight;

            ToolStripItem zoomItem = new ToolStripControlHost(trackBarZoom);
            this.statusBar.Items.Insert(statusBar.Items.Count - 1, zoomItem);

            statusZoomIn.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus-circle16.png"));
            statusZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            statusZoomIn.Margin = new Padding(0, 0, 8, 0);

            statusZoomOut.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus-circle16.png"));
            statusZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;

            /*trackBarZoom.Scroll += trackBarZoom_Scroll;
            trackBarZoom.ValueChanged += trackBarZoom_ValueChanged;

            statusZoomOut.MouseEnter += buttonZoomOut_MouseEnter;
            statusZoomOut.MouseLeave += buttonZoomOut_MouseLeave;
            statusZoomOut.MouseDown += buttonZoomOut_MouseDown;
            statusZoomOut.MouseUp += buttonZoomOut_MouseUp;

            statusZoomIn.MouseEnter += buttonZoomIn_MouseEnter;
            statusZoomIn.MouseLeave += buttonZoomIn_MouseLeave;
            statusZoomIn.MouseDown += buttonZoomIn_MouseDown;
            statusZoomIn.MouseUp += buttonZoomIn_MouseUp;*/
        }

        private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            foreach (ILevelPresenter lp in _editor.OpenContent) {
                TabPage page = new TabPage("Level");
                tabControlEx1.TabPages.Add(page);

                LevelPanel lpanel = new LevelPanel();
                lpanel.BindController(lp);
                lpanel.Dock = DockStyle.Fill;

                page.Controls.Add(lpanel);
            }
        }

        private void SyncContentViewHandler (object sender, EventArgs e)
        {
            ILevelPresenter lp = _editor.CurrentLevel;

            foreach (TabPage page in tabControlEx1.TabPages) {
                if (page.Text == lp.LayerControl.Name) {
                    tabControlEx1.SelectedTab = page;
                }
            }

            if (_editor.CanShowLayerPanel)
                layerPane1.BindController(_editor.CurrentLayerListPresenter);

            if (_editor.CanShowTilePoolPanel)
                tilePoolPane1.BindController(_editor.CurrentTilePoolListPresenter);

            if (_editor.CanShowPropertyPanel)
                propertyPane1.BindController(_editor.CurrentPropertyListPresenter);

            _tileToolbar.BindController(_editor.CurrentLevelToolsPresenter);
        }

        /*public ToolStripStatusLabel Label
        {
            get { return toolStripStatusLabel1; }
        }

        public ToolStripStatusLabel StatusCoord
        {
            get { return statusCoord; }
        }

        private void splitContainer1_SplitterMoved (object sender, SplitterEventArgs e)
        {

        }

        // Zoom In

        public void buttonZoomIn_MouseEnter (object sender, EventArgs e)
        {
            statusZoomIn.Image = _imgPlusCircleMo;
        }

        public void buttonZoomIn_MouseLeave (object sender, EventArgs e)
        {
            statusZoomIn.Image = _imgPlusCircle;
        }

        public void buttonZoomIn_MouseDown (object sender, EventArgs e)
        {
            statusZoomIn.Image = _imgPlusCircleClk;
        }

        public void buttonZoomIn_MouseUp (object sender, EventArgs e)
        {
            statusZoomIn.Image = _imgPlusCircleMo;
            if (trackBarZoom.Value < trackBarZoom.Maximum) {
                trackBarZoom.Value++;
            }
        }

        // Zoom Out

        public void buttonZoomOut_MouseEnter (object sender, EventArgs e)
        {
            statusZoomOut.Image = _imgMinusCircleMo;
        }

        public void buttonZoomOut_MouseLeave (object sender, EventArgs e)
        {
            statusZoomOut.Image = _imgMinusCircle;
        }

        public void buttonZoomOut_MouseDown (object sender, EventArgs e)
        {
            statusZoomOut.Image = _imgMinusCircleClk;
        }

        public void buttonZoomOut_MouseUp (object sender, EventArgs e)
        {
            statusZoomOut.Image = _imgMinusCircleMo;
            if (trackBarZoom.Value > trackBarZoom.Minimum) {
                trackBarZoom.Value--;
            }
        }

        // Zoom Bar

        private void trackBarZoom_Scroll (object sender, EventArgs e)
        {

        }

        private void trackBarZoom_ValueChanged (object sender, EventArgs e)
        {
            if (_currentView == null)
                return;

            float zoom = 1f;

            switch (trackBarZoom.Value) {
                case 0: 
                    statusZoomText.Text = "25%";
                    zoom = 0.25f;
                    break;
                case 1: 
                    statusZoomText.Text = "50%";
                    zoom = 0.5f;
                    break;
                case 2: 
                    statusZoomText.Text = "100%";
                    zoom = 1f;
                    break;
                case 3: 
                    statusZoomText.Text = "200%";
                    zoom = 2f;
                    break;
                case 4: 
                    statusZoomText.Text = "300%";
                    zoom = 3f;
                    break;
                case 5: 
                    statusZoomText.Text = "400%";
                    zoom = 4f;
                    break;
                case 6: 
                    statusZoomText.Text = "600%";
                    zoom = 6f;
                    break;
                case 7: 
                    statusZoomText.Text = "800%";
                    zoom = 8f;
                    break;
            }

            //if (_editor.CurrentZoomableControl != null) {
            //    _editor.CurrentZoomableControl.Zoom = zoom;
            //}
        }

        private void ZoomableControlZoomChangedHandler (object sender, EventArgs e)
        {
            //if (_editor.CurrentZoomableControl != sender) {
            //    throw new Exception("Unexpected ZoomChanged event");
            //}

            //UpdateZoomState(_editor.CurrentZoomableControl.Zoom);
        }

        private void UpdateZoomState (float zoom)
        {
            List<float> valid = new List<float> { .25f, .5f, 1f, 2f, 3f, 4f, 6f, 8f };
            List<string> text = new List<string> { "25%", "50%", "100%", "200%", "300%", "400%", "600%", "800%" };

            if (!valid.Contains(zoom)) {
                foreach (float f in valid) {
                    if (zoom >= f) {
                        zoom = f;
                    }
                }
            }

            int index = valid.FindIndex((f) => { return f == zoom; });
            if (trackBarZoom != null) {
                trackBarZoom.Value = index;
            }
        }

        private void EditorActivateContent (object sender, EventArgs e)
        {
            //if (_editor.CurrentZoomableControl != null) {
            //    _editor.CurrentZoomableControl.ZoomChanged += ZoomableControlZoomChangedHandler;

            //    UpdateZoomState(_editor.CurrentZoomableControl.Zoom);
            //}
        }

        private void EditorDeactivateContent (object sender, EventArgs e)
        {
            
        }

        private void ButtonSave (object sender, EventArgs e)
        {
            using (FileStream fs = File.Open("test.tlp", FileMode.Truncate, FileAccess.Write)) {
                _project.Save(fs);
            }
        }

        private void ButtonOpen (object sender, EventArgs e)
        {
            GraphicsDeviceService gds = GraphicsDeviceService.AddRef(Handle, 128, 128);

            using (FileStream fs = File.Open("test.tlp", FileMode.Open, FileAccess.Read)) {
                _project = Project.Open(fs, gds.GraphicsDevice);
            }

            //_editor.UnloadProject();
            //_editor.LoadProject(_project);
        }

        private void ButtonUndo (object sender, EventArgs e)
        {
            if (_currentView != null) {
                _currentView.Undo();
            }
        }

        private void ButtonRedo (object sender, EventArgs e)
        {
            if (_currentView != null) {
                _currentView.Redo();
            }
        }

        private void TileToolModeChangedHandler (object sender, TileToolModeEventArgs e)
        {
            if (_currentView != null && _currentView is ITileToolbarSubscriber) {
                ITileToolbarSubscriber tts = _currentView as ITileToolbarSubscriber;

                if (tts.TileToolMode == e.TileToolMode) {
                    return;
                }

                tts.TileToolMode = e.TileToolMode;
            }
        }

        private void FormCommandHistoryChangedHandler (object sender, CommandHistoryEventArgs e)
        {
            _standardToolbar.ButtonRedo.Enabled = e.CommandHistory.CanRedo;
            _standardToolbar.ButtonUndo.Enabled = e.CommandHistory.CanUndo;
        }

        IFormView _currentView;

        private void SwitchToView (IFormView view)
        {
            if (_currentView != null) {
                toolStripContainer1.ContentPanel.Controls.Clear();

                _currentView.CommandHistoryChanged -= FormCommandHistoryChangedHandler;
            }

            toolStripContainer1.ContentPanel.Controls.Add(view.Control);

            _currentView = view;
            _currentView.CommandHistoryChanged += FormCommandHistoryChangedHandler;

            _currentView.Display();
        }*/
    }


    

    
}
