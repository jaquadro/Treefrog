using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Editor.Model;

namespace Editor
{
    public partial class TilesetView : UserControl, IFormView, ITileToolbarSubscriber
    {
        private Form _parentForm;

        private Project _project;

        private TilePool _selectedPool;
        private TileSet1D _selectedPoolSet;

        private TileSet2D _selectedTileSet;

        private bool _poolReady;
        private bool _setReady;

        private CommandHistory _commandHistory;

        public event EventHandler<CommandHistoryEventArgs> CommandHistoryChanged;

        private DrawTool _drawTool;
        private EraseTool _eraseTool;
        private FillTool _fillTool;

        public TilesetView ()
        {
            InitializeComponent();

            _commandHistory = new CommandHistory();
            _commandHistory.HistoryChanged += CommandHistoryChangedHandler;
        }

        public TilesetView (Form parentForm, Project project)
            : this()
        {
            _parentForm = parentForm;
            _project = project;

            // Setup TilePool Control (Sidebar)

            tilepoolControl.MainForm = parentForm;
            tilepoolControl.ControlInitializing += SetupTilePoolControl;

            // Setup TileSet Control (Main)

            tilesetControl.MainForm = parentForm;
            tilesetControl.ControlInitializing += SetupTileSetControl;

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            toolStripButton1.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus16.png"));
            toolStripButton2.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus16.png"));
            toolStripButton3.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.pencil16.png"));

            toolButtonSelect.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.cursor16.png"));
            toolButtonDraw.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.paint-brush16.png"));
            toolButtonErase.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.eraser16.png"));
            toolButtonFill.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.paint-can16.png"));
            toolButtonStamp.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.stamp16.png"));

            // Tools

            _drawTool = new DrawTool(tilesetControl, tilepoolControl, _project.TileSets["Default"], _commandHistory);
            _eraseTool = new EraseTool(tilesetControl, _project.TileSets["Default"], _commandHistory);
            _fillTool = new FillTool(tilesetControl, tilepoolControl, _project.TileSets["Default"], _commandHistory);

            // XXX
            _drawTool.Enabled = true;
        }

        private void CommandHistoryChangedHandler (object sender, CommandHistoryEventArgs e)
        {
            OnCommandHistoryChanged(e);
        }

        private void SetupTilePoolControl (object sender, EventArgs e)
        {
            using (FileStream fs = File.OpenRead(@"E:\Workspace\Managed\Treefrog\Tilesets\jungle_tiles.png")) {
                //_defaultPool = TilePool.Import(_registry, fs, 16, 16, 1, 1);
                //_defaultPoolSet = TileSet1D.CreatePoolSet("Default", _defaultPool);

                _selectedPool = TilePool.Import("Jungle", _project.Registry, fs, 16, 16, 1, 1);
                _selectedPoolSet = TileSet1D.CreatePoolSet("JungleSet", _selectedPool);

                _project.TilePools.Add(_selectedPool);

                tilepoolControl.TileSource = _selectedPoolSet;
            }

            tilepoolControl.BackColor = Color.SlateGray;
            tilepoolControl.CanSelectRange = false;
            tilepoolControl.CanSelectDisjoint = false;
            tilepoolControl.Mode = TileControlMode.Select;

            foreach (TilePool pool in _project.TilePools) {
                toolStripComboBox1.Items.Add(pool.Name);
            }
            toolStripComboBox1.SelectedText = _selectedPool.Name;

            _poolReady = true;
            Experimental();
        }

        private void SetupTileSetControl (object sender, EventArgs e)
        {
            _selectedTileSet = _project.TileSets["Default"];

            tilesetControl.TileSource = _selectedTileSet;
            tilesetControl.Mode = TileControlMode.Click;

            _setReady = true;
            Experimental();
        }

        private void Experimental ()
        {
            if (!_poolReady || !_setReady) {
                return;
            }

            tilesetControl.MouseTileMove += MouseTileMoveHandler;
            tilesetControl.MouseLeave += MouseLeaveHandler;
            
            tilepoolControl.TileSelected += TilePoolTileSelectedHandler;
        }

        private void MouseTileMoveHandler (object sender, TileMouseEventArgs e)
        {
            Form1 parent = _parentForm as Form1;

            parent.StatusCoord.Text = e.TileLocation.X + ", " + e.TileLocation.Y;
        }

        private void MouseLeaveHandler (object sender, EventArgs e)
        {
            Form1 parent = _parentForm as Form1;

            parent.StatusCoord.Text = "";
        }

        private Tile _selectedPoolTile;

        private void TilePoolTileSelectedHandler (object sender, TileEventArgs e)
        {
            _selectedPoolTile = e.Tile;
        }

        private void TileSetMouseTileClickHandler (object sender, TileMouseEventArgs e)
        {
            if (e.Location.X < 0 || e.Location.X >= _selectedTileSet.TilesWide)
                return;
            if (e.Location.Y < 0 || e.Location.Y >= _selectedTileSet.TilesHigh)
                return;

            _selectedTileSet[e.TileLocation] = _selectedPoolTile;
        }

        #region IFormView Members

        public Control Control
        {
            get { return this; }
        }

        public float Zoom
        {
            get { return tilesetControl.Zoom; }
            set { tilesetControl.Zoom = value; }
        }

        public void Display ()
        {
            OnCommandHistoryChanged(new CommandHistoryEventArgs(_commandHistory));
        }

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        

        public void Copy ()
        {
            throw new NotImplementedException();
        }

        public void Cut ()
        {
            throw new NotImplementedException();
        }

        public void Paste ()
        {
            throw new NotImplementedException();
        }

        public void Undo ()
        {
            if (_commandHistory.CanUndo) {
                _commandHistory.Undo();
                OnCommandHistoryChanged();
            }
        }

        public void Redo ()
        {
            if (_commandHistory.CanRedo) {
                _commandHistory.Redo();
                OnCommandHistoryChanged();
            }
        }

        protected virtual void OnClipboardChanged (ClipboardEventArgs e)
        {
            if (ClipboardChanged != null) {
                ClipboardChanged(this, e);
            }
        }

        private void OnCommandHistoryChanged ()
        {
            OnCommandHistoryChanged(new CommandHistoryEventArgs(_commandHistory));
        }

        protected virtual void OnCommandHistoryChanged (CommandHistoryEventArgs e)
        {
            if (CommandHistoryChanged != null) {
                CommandHistoryChanged(this, e);
            }
        }

        #endregion

        #region ITileToolbarSubscriber Members

        private TileToolMode _toolMode = TileToolMode.Draw;

        public TileToolMode TileToolMode
        {
            get
            {
                return _toolMode;
            }
            set
            {
                _toolMode = value;

                _drawTool.Enabled = false;
                _eraseTool.Enabled = false;
                _fillTool.Enabled = false;

                switch (_toolMode) {
                    case Editor.TileToolMode.Select:
                    case Editor.TileToolMode.Stamp:
                        tilesetControl.Mode = TileControlMode.Select;
                        break;
                    case Editor.TileToolMode.Draw:
                        tilesetControl.Mode = TileControlMode.Click;
                        _drawTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Erase:
                        tilesetControl.Mode = TileControlMode.Click;
                        _eraseTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Fill:
                        tilesetControl.Mode = TileControlMode.Click;
                        _fillTool.Enabled = true;
                        break;
                }
            }
        }

        #endregion
    }

}
