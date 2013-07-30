using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows
{
    public partial class LayerPane : UserControl
    {
        private ILayerListPresenter _controller;
        private UICommandController _commandController;

        public LayerPane ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            _buttonAdd.Image = Properties.Resources.LayerPlus;
            _buttonRemove.Image = Properties.Resources.LayerMinus;
            _buttonUp.Image = Properties.Resources.Arrow90;
            _buttonDown.Image = Properties.Resources.Arrow270;
            _buttonCopy.Image = Properties.Resources.Layers;
            _buttonProperties.Image = Properties.Resources.Tags;

            _menuNewTileLayer.Image = Properties.Resources.Grid;
            _menuNewObjectLayer.Image = Properties.Resources.Game;

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.LayerDelete, _buttonRemove },
                { CommandKey.LayerClone, _buttonCopy },
                { CommandKey.LayerProperties, _buttonProperties },
                { CommandKey.LayerMoveUp, _buttonUp },
                { CommandKey.LayerMoveDown, _buttonDown },
            });
            _commandController.MapMenuItems(new Dictionary<CommandKey, ToolStripMenuItem>() {
                { CommandKey.NewTileLayer, _menuNewTileLayer },
                { CommandKey.NewObjectLayer, _menuNewObjectLayer },
            });

            // Wire events

            
        }

        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad(e);

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
            _listControl.ItemChecked += ItemCheckedHandler;

            _listControl.DoubleClick += (s, v) => {
                if (_controller != null)
                    _controller.CommandManager.Perform(CommandKey.LayerEdit);
            };
        }

        public void BindController (ILayerListPresenter controller) {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncLayerList -= SyncLayerListHandler;
                _controller.SyncLayerSelection -= SyncLayerSelectionHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncLayerList += SyncLayerListHandler;
                _controller.SyncLayerSelection += SyncLayerSelectionHandler;

                _commandController.BindCommandManager(_controller.CommandManager);

                SyncLayerList();
                SyncLayerSelection();
            }
            else {
                _commandController.BindCommandManager(null);

                ResetComponent();
            }
        }

        #region Event Handlers

        private void SelectedItemChangedHandler (object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected) {
                return;
            }

            if (_controller != null) {
                _controller.ActionSelectLayer((Guid)e.Item.Tag);
            }
        }

        private void ItemCheckedHandler (object sender, ItemCheckedEventArgs e)
        {
            if (_controller != null) {
                _controller.ActionShowHideLayer((Guid)e.Item.Tag, e.Item.Checked ? LayerVisibility.Show : LayerVisibility.Hide);
            }
        }

        private void SyncLayerList ()
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            _listControl.Items.Clear();

            if (_controller != null) {
                Stack<ListViewItem> items = new Stack<ListViewItem>();

                foreach (LevelLayerPresenter layer in _controller.LayerList) {
                    ListViewItem layerItem = new ListViewItem(layer.LayerName, 0) {
                        Name = layer.LayerName,
                        Checked = layer.IsVisible,
                        Tag = layer.Layer.Uid,
                    };

                    if (layer is ObjectLayerPresenter)
                        layerItem.ImageIndex = 1;

                    if (layer == _controller.SelectedLayer) {
                        layerItem.Selected = true;
                    }

                    items.Push(layerItem);
                }

                while (items.Count > 0) {
                    _listControl.Items.Add(items.Pop());
                }
            }

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
        }

        private void SyncLayerSelection ()
        {
            _listControl.ItemSelectionChanged -= SelectedItemChangedHandler;

            foreach (ListViewItem item in _listControl.Items) {
                if (_controller.SelectedLayer == null || item.Name != _controller.SelectedLayer.LayerName) {
                    item.Selected = false;
                }
                else {
                    item.Selected = true;
                }
            }

            _listControl.ItemSelectionChanged += SelectedItemChangedHandler;
        }

        private void SyncLayerListHandler (object sender, EventArgs e)
        {
            SyncLayerList();
        }

        private void SyncLayerSelectionHandler (object sender, EventArgs e)
        {
            SyncLayerSelection();
        }

        #endregion

        private void ResetComponent ()
        {
            _listControl.Items.Clear();

            _buttonProperties.Enabled = false;
        }
    }
}
