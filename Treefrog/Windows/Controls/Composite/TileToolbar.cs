using System;
using System.Windows.Forms;
using System.Reflection;
using Treefrog.Presentation;
using System.Drawing;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;

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

        private Assembly _assembly;

        private CommandManager _commandManager;

        private Dictionary<CommandKey, ToolStripButton> _commandButtonMap;

        public TileToolbar ()
        {
            // Construction

            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _tbSelect = CreateButton("Select Tool (C)", "Treefrog.Icons.cursor16.png");
            _tbDraw = CreateButton("Draw Tool (D)", "Treefrog.Icons.paint-brush16.png");
            _tbErase = CreateButton("Erase Tool (E)", "Treefrog.Icons.eraser16.png");
            _tbFill = CreateButton("Fill Tool (F)", "Treefrog.Icons.paint-can16.png");

            _tbFlipH = CreateButton("Flip Horizontally", "Treefrog.Icons.layer-flip16.png");
            _tbFlipV = CreateButton("Flip Vertically", "Treefrog.Icons.layer-flip-vertical16.png");
            _tbRotateLeft = CreateButton("Rotate Left 90 Degrees", "Treefrog.Icons.layer-rotate-left16.png");
            _tbRotateRight = CreateButton("Rotate Right 90 Degrees", "Treefrog.Icons.layer-rotate16.png");

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

        private ToolStripButton CreateButton (string text, string resource)
        {
            ToolStripButton button = new ToolStripButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new System.Drawing.Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }
    }
}
