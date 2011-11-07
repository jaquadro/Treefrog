using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model.Controls;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Runtime.InteropServices;

namespace Editor
{
    public partial class MapView : UserControl, IFormView, ITileToolbarSubscriber
    {
        private Project _project;
        private Level _level;

        // XXX: Move to individual level object
        private CommandHistory _commandHistory;

        private DrawTool _drawTool;
        //private EraseTool _eraseTool;
        //private FillTool _fillTool;

        // TODO: Arbitrary number of layers
        private MultiTileGridLayer _layer;
        private MultiTileControlLayer _tileLayer;

        #region Constructors

        public MapView ()
        {
            InitializeComponent();

            _commandHistory = new CommandHistory();
            _commandHistory.HistoryChanged += CommandHistoryChangedHandler;
        }

        public MapView (Project project, string level) 
            : this()
        {
            _project = project;
            _level = _project.Levels[level];

            //_tilePoolPane.SetupDefault(_project);

            //_layerPane.SetupDefault(_project, level, tilemapControl);

            //propertyPane1.PropertyProvider = _level;

            /*_tileLayer = new MultiTileControlLayer(tilemapControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Always;*/

            //_layer = new MultiTileGridLayer("Default", 16, 16, 30, 20);
            //_tileLayer.Layer = _layer;

            //_layerPane.SelectedLayerChanged += LayerSelectionChangedHandler;

            // Tools

            //_drawTool = new DrawTool(_tileLayer, _tilePoolPane.TileLayer, _commandHistory);
            //_eraseTool = new EraseTool(_tileLayer, _project.TileSets["Default"], _commandHistory);
            //_fillTool = new FillTool(tilesetControl, _tilePoolPane.TileLayer, _project.TileSets["Default"], _commandHistory);

            // XXX
            //_drawTool.Enabled = true;
        }

        public MapView (EditorState editor)
            : this()
        {

        }

        #endregion

        #region Event Handlers

        private void LayerSelectionChangedHandler (object sender, EventArgs e)
        {
            // TODO: Eventually we'll have more than tile layers
            /*_layer = _layerPane.SelectedLayer as MultiTileGridLayer;
            _tileLayer = _layerPane.SelectedControlLayer as MultiTileControlLayer;

            if (_drawTool != null) {
                _drawTool.Dispose();
            }

            _drawTool = new DrawTool(_tileLayer, _tilePoolPane.TileLayer, _commandHistory);
            _drawTool.Enabled = true;

            if (_layer != null) {
                tilemapControl.SetScrollSmallChange(ScrollOrientation.HorizontalScroll, _layer.TileWidth);
                tilemapControl.SetScrollSmallChange(ScrollOrientation.VerticalScroll, _layer.TileHeight);
                tilemapControl.SetScrollLargeChange(ScrollOrientation.HorizontalScroll, _layer.TileWidth * 4);
                tilemapControl.SetScrollLargeChange(ScrollOrientation.VerticalScroll, _layer.TileHeight * 4);
            }*/
        }

        #endregion

        private void CommandHistoryChangedHandler (object sender, CommandHistoryEventArgs e)
        {
            OnCommandHistoryChanged(e);
        }

        #region IFormView Members

        public Control Control
        {
            get { return this; }
        }

        public float Zoom
        {
            get { return tilemapControl.Zoom; }
            set { tilemapControl.Zoom = value; }
        }

        public void Display ()
        {
            OnCommandHistoryChanged(new CommandHistoryEventArgs(_commandHistory));
        }

        public event EventHandler<ClipboardEventArgs> ClipboardChanged;

        public event EventHandler<CommandHistoryEventArgs> CommandHistoryChanged;

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
                //_eraseTool.Enabled = false;
                //_fillTool.Enabled = false;

                switch (_toolMode) {
                    case Editor.TileToolMode.Select:
                    case Editor.TileToolMode.Stamp:
                        //tilesetControl.Mode = TileControlMode.Select;
                        break;
                    case Editor.TileToolMode.Draw:
                        //tilesetControl.Mode = TileControlMode.Click;
                        _drawTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Erase:
                        //tilesetControl.Mode = TileControlMode.Click;
                        //_eraseTool.Enabled = true;
                        break;
                    case Editor.TileToolMode.Fill:
                        //tilesetControl.Mode = TileControlMode.Click;
                        //_fillTool.Enabled = true;
                        break;
                }
            }
        }

        #endregion
    }

    
}
