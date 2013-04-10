using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Presentation.Commands;

namespace Treefrog.Windows.Controls.Composite
{
    class TileToolbar
    {
        private ToolStrip _strip;

        private ToolStripButton _tbSelect;
        private ToolStripButton _tbDraw;
        private ToolStripButton _tbErase;
        private ToolStripButton _tbFill;

        private ToolStripButton _tbFlipH;
        private ToolStripButton _tbFlipV;
        private ToolStripButton _tbRotateLeft;
        private ToolStripButton _tbRotateRight;

        private CommandManager _commandManager;

        private Dictionary<CommandKey, ToolStripButton> _commandButtonMap;

        public TileToolbar ()
        {
            // Construction

            _tbSelect = CreateButton("Select Tool (C)", Properties.Resources.Cursor);
            _tbDraw = CreateButton("Draw Tool (D)", Properties.Resources.PaintBrush);
            _tbErase = CreateButton("Erase Tool (E)", Properties.Resources.Eraser);
            _tbFill = CreateButton("Fill Tool (F)", Properties.Resources.PaintCan);

            _tbFlipH = CreateButton("Flip Horizontally", Properties.Resources.LayerFlip);
            _tbFlipV = CreateButton("Flip Vertically", Properties.Resources.LayerFlipVertical);
            _tbRotateLeft = CreateButton("Rotate Left 90 Degrees", Properties.Resources.LayerRotateLeft);
            _tbRotateRight = CreateButton("Rotate Right 90 Degrees", Properties.Resources.LayerRotate);

            _strip = new ToolStrip();
            _strip.Items.AddRange(new ToolStripItem[] {
                _tbSelect, _tbDraw, _tbErase, _tbFill, new ToolStripSeparator(),
                _tbFlipH, _tbFlipV, _tbRotateLeft, _tbRotateRight
            });

            _tbSelect.Click += SelectButtonClickHandler;
            _tbDraw.Click += DrawButtonClickHandler;
            _tbErase.Click += EraseButtonClickHandler;
            _tbFill.Click += FillButtonClickHandler;

            _commandButtonMap = new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.TileToolSelect, _tbSelect },
                { CommandKey.TileToolDraw, _tbDraw },
                { CommandKey.TileToolErase, _tbErase },
                { CommandKey.TileToolFill, _tbFill },
            };

            ResetComponent();
        }

        public void BindCommandManager (CommandManager commandManager)
        {
            if (_commandManager != null) {
                _commandManager.CommandInvalidated -= HandleCommandInvalidated;
                _commandManager.ManagerInvalidated -= HandleManagerInvalidated;
            }

            _commandManager = commandManager;
            if (_commandManager != null) {
                _commandManager.CommandInvalidated += HandleCommandInvalidated;
                _commandManager.ManagerInvalidated += HandleManagerInvalidated;
            }

            ResetComponent();
        }

        private bool CanPerformCommand (CommandKey key)
        {
            return _commandManager != null && _commandManager.CanPerform(key);
        }

        private void PerformCommand (CommandKey key)
        {
            if (_commandManager.CanPerform(key))
                _commandManager.Perform(key);
        }

        private bool IsCommandSelected (CommandKey key)
        {
            return _commandManager != null && _commandManager.IsSelected(key);
        }

        private void HandleCommandInvalidated (object sender, CommandSubscriberEventArgs e)
        {
            Invalidate(e.CommandKey);
        }

        private void HandleManagerInvalidated (object sender, EventArgs e)
        {
            ResetComponent();
        }

        private void Invalidate (CommandKey key)
        {
            ToolStripButton button;
            if (_commandButtonMap.TryGetValue(key, out button)) {
                button.Enabled = CanPerformCommand(key);
                button.Checked = IsCommandSelected(key);
            }
        }

        private void ResetComponent ()
        {
            foreach (CommandKey key in _commandButtonMap.Keys)
                Invalidate(key);

            _tbFlipH.Enabled = false;
            _tbFlipV.Enabled = false;
            _tbRotateLeft.Enabled = false;
            _tbRotateRight.Enabled = false;
        }

        public ToolStrip Strip
        {
            get { return _strip; }
        }

        private void SelectButtonClickHandler (object sender, EventArgs e)
        {
            PerformCommand(CommandKey.TileToolSelect);
        }

        private void DrawButtonClickHandler (object sender, EventArgs e)
        {
            PerformCommand(CommandKey.TileToolDraw);
        }

        private void EraseButtonClickHandler (object sender, EventArgs e)
        {
            PerformCommand(CommandKey.TileToolErase);
        }

        private void FillButtonClickHandler (object sender, EventArgs e)
        {
            PerformCommand(CommandKey.TileToolFill);
        }

        private ToolStripButton CreateButton (string text, Image resource)
        {
            ToolStripButton button = new ToolStripButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new System.Drawing.Size(22, 22);
            button.Text = text;
            button.Image = resource;

            return button;
        }
    }
}
