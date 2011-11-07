using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Editor.Forms;
using System.IO;
using Editor.Model;
using Editor.Model.Controls;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Editor.A.Presentation;

namespace Editor
{
    public partial class TilePoolPane : UserControl
    {
        #region Fields

        private ITilePoolListPresenter _controller;

        private TileSetControlLayer _tileLayer;

        #endregion

        #region Constructors

        public TilePoolPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.minus16.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.plus16.png"));
            _buttonEdit.Image = Image.FromStream(assembly.GetManifestResourceStream("Editor.Icons.pencil16.png"));

            // Setup control

            _tileControl.BackColor = Color.SlateGray;
            _tileControl.WidthSynced = true;
            _tileControl.Alignment = LayerControlAlignment.UpperLeft;

            _tileLayer = new TileSetControlLayer(_tileControl);
            _tileLayer.ShouldDrawContent = LayerCondition.Always;
            _tileLayer.ShouldDrawGrid = LayerCondition.Always;
            _tileLayer.ShouldRespondToInput = LayerCondition.Always;

            // Wire events

            importNewToolStripMenuItem.Click += ImportTilePoolClickedHandler;
            _buttonRemove.Click += RemoveTilePoolClickedHandler;

            _poolComboBox.SelectedIndexChanged += SelectTilePoolHandler;
        }

        #endregion

        public void BindController (ITilePoolListPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncTilePoolActions -= SyncTilePoolActionsHandler;
                _controller.SyncTilePoolList -= SyncTilePoolListHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncTilePoolActions += SyncTilePoolActionsHandler;
                _controller.SyncTilePoolList += SyncTilePoolListHandler;

                _controller.RefreshTilePoolList();
            }
            else {
                ResetComponent();
            }
        }

        #region Event Handlers

        private void ImportTilePoolClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionImportTilePool();
        }

        private void RemoveTilePoolClickedHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionRemoveSelectedTilePool();
        }

        private void SelectTilePoolHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionSelectTilePool((string)_poolComboBox.SelectedItem);
        }

        private void SyncTilePoolActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                _buttonAdd.Enabled = _controller.CanAddTilePool;
                _buttonRemove.Enabled = _controller.CanRemoveSelectedTilePool;
                _buttonEdit.Enabled = false;
            }
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            _tileLayer.Layer = null;

            foreach (TilePool pool in _controller.TilePoolList) {
                _poolComboBox.Items.Add(pool.Name);

                if (pool == _controller.SelectedTilePool) {
                    _poolComboBox.SelectedItem = pool.Name;

                    _tileLayer.Layer = new TileSetLayer(pool.Name, pool);
                }
            }
        }

        #endregion

        private void ResetComponent ()
        {
            _poolComboBox.Items.Clear();
            _poolComboBox.Text = "";

            if (_tileLayer != null) {
                _tileLayer.Layer = null;
            }

            _buttonAdd.Enabled = false;
            _buttonEdit.Enabled = false;
            _buttonRemove.Enabled = false;
        }
    }
}
