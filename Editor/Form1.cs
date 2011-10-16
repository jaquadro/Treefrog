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
using Editor.Model;


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
        private MapView _mapView;

        private Project _project;

        public Form1 ()
        {
            InitializeComponent();

            // Toolbars

            _standardToolbar = new StandardToolbar();
            _standardToolbar.ButtonUndo.Click += ButtonUndo;
            _standardToolbar.ButtonRedo.Click += ButtonRedo;
            _standardToolbar.ButtonSave.Click += ButtonSave;

            _tileToolbar = new TileToolbar();
            _tileToolbar.ToolModeChanged += TileToolModeChangedHandler;

            toolStripContainer1.TopToolStripPanel.Controls.AddRange(new Control[] {
                _standardToolbar.Strip, 
                _tileToolbar.Strip
            });

            // Other

            _project = new Project();
            _project.Initialize(Handle);
            //_project.SetupDefaults();

            //_tilesetView = new TilesetView(this, _project);
            //_tilesetView.Dock = DockStyle.Fill;

            Level level = new Level("Level 1", 16, 16, 30, 20);

            _project.Levels.Add(level);

            _mapView = new MapView(_project, "Level 1");
            _mapView.Dock = DockStyle.Fill;

            //SwitchToView(_tilesetView);
            SwitchToView(_mapView);

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            statusCoord.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.status-loc.png"));
            statusCoord.ImageScaling = ToolStripItemImageScaling.None;

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

            trackBarZoom.Scroll += trackBarZoom_Scroll;
            trackBarZoom.ValueChanged += trackBarZoom_ValueChanged;

            statusZoomOut.MouseEnter += buttonZoomOut_MouseEnter;
            statusZoomOut.MouseLeave += buttonZoomOut_MouseLeave;
            statusZoomOut.MouseDown += buttonZoomOut_MouseDown;
            statusZoomOut.MouseUp += buttonZoomOut_MouseUp;

            statusZoomIn.MouseEnter += buttonZoomIn_MouseEnter;
            statusZoomIn.MouseLeave += buttonZoomIn_MouseLeave;
            statusZoomIn.MouseDown += buttonZoomIn_MouseDown;
            statusZoomIn.MouseUp += buttonZoomIn_MouseUp;


        }

        public ToolStripStatusLabel Label
        {
            get { return toolStripStatusLabel1; }
        }

        public ToolStripStatusLabel StatusCoord
        {
            get { return statusCoord; }
        }

        /*private void amphibianGameControl1_MouseMove (object sender, MouseEventArgs e)
        {
            canvas.mousex = e.X;
            canvas.mousey = e.Y;
        }*/

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

            switch (trackBarZoom.Value) {
                case 0: 
                    statusZoomText.Text = "25%";
                    _currentView.Zoom = 0.25f;
                    break;
                case 1: 
                    statusZoomText.Text = "50%";
                    _currentView.Zoom = 0.5f;
                    break;
                case 2: 
                    statusZoomText.Text = "100%";
                    _currentView.Zoom = 1f;
                    break;
                case 3: 
                    statusZoomText.Text = "200%";
                    _currentView.Zoom = 2f;
                    break;
                case 4: 
                    statusZoomText.Text = "300%";
                    _currentView.Zoom = 3f;
                    break;
                case 5: 
                    statusZoomText.Text = "400%";
                    _currentView.Zoom = 4f;
                    break;
                case 6: 
                    statusZoomText.Text = "600%";
                    _currentView.Zoom = 6f;
                    break;
                case 7: 
                    statusZoomText.Text = "800%";
                    _currentView.Zoom = 8f;
                    break;
            }
        }

        private void ButtonSave (object sender, EventArgs e)
        {
            using (FileStream fs = File.OpenWrite("test.tlp")) {
                _project.Save(fs);
            }
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
        }
    }


    

    
}
