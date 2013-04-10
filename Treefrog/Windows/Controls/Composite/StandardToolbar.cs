using System;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Presentation.Commands;
using Treefrog.Utility;

namespace Treefrog.Windows.Controls.Composite
{
    class StandardToolbar
    {
        private ToolStrip _strip;

        private ToolStripButton _tbNewProject;
        private ToolStripDropDownButton _tbNewItem;
        private ToolStripButton _tbOpen;
        private ToolStripButton _tbSave;

        private ToolStripButton _tbCut;
        private ToolStripButton _tbCopy;
        private ToolStripButton _tbPaste;

        private ToolStripButton _tbUndo;
        private ToolStripButton _tbRedo;

        private ToolStripButton _tbCompile;

        private CommandManager _commandManager;
        private Mapper<CommandKey, ToolStripButton> _commandMap = new Mapper<CommandKey, ToolStripButton>();

        public StandardToolbar ()
        {
            _tbNewProject = CreateButton("New Project (Ctrl+N)", Properties.Resources.ApplicationsBlueAst);
            _tbNewItem = CreateDropDownButton("Add New Item", Properties.Resources.MapAsterisk);
            _tbNewItem.DropDownItems.AddRange(new ToolStripItem[] {
                DropDownMenuItem("Add New Level", Properties.Resources.MapAsterisk),
                DropDownMenuItem("Add New Tile Pool", Properties.Resources.ColorSwatch),
            });
            _tbOpen = CreateButton("Open Project (Ctrl+O)", Properties.Resources.FolderHorizontalOpen);
            _tbSave = CreateButton("Save Project (Ctrl+S)", Properties.Resources.Disk);
            _tbCut = CreateButton("Cut (Ctrl+X)", Properties.Resources.Scissors);
            _tbCopy = CreateButton("Copy (Ctrl+C)", Properties.Resources.Documents);
            _tbPaste = CreateButton("Paste (Ctrl+V)", Properties.Resources.ClipboardPaste);
            _tbUndo = CreateButton("Undo (Ctrl+Z)", Properties.Resources.ArrowTurn180Left);
            _tbRedo = CreateButton("Redo (Ctrl+Y)", Properties.Resources.ArrowTurn);
            _tbCompile = CreateButton("Compile", Properties.Resources.Compile);

            _strip = new ToolStrip();
            _strip.Items.AddRange(new ToolStripItem[] {
                _tbNewProject, _tbOpen, _tbSave, new ToolStripSeparator(),
                _tbCut, _tbCopy, _tbPaste, new ToolStripSeparator(),
                _tbUndo, _tbRedo, new ToolStripSeparator(),
                _tbCompile,
            });

            _commandMap = new Mapper<CommandKey, ToolStripButton>() {
                { CommandKey.NewProject, _tbNewProject },
                { CommandKey.OpenProject, _tbOpen },
                { CommandKey.Save, _tbSave },

                { CommandKey.Undo, _tbUndo },
                { CommandKey.Redo, _tbRedo },
                { CommandKey.Cut, _tbCut },
                { CommandKey.Copy, _tbCopy },
                { CommandKey.Paste, _tbPaste },
            };

            foreach (ToolStripButton item in _commandMap.Values)
                item.Click += BoundButtonClickHandler;

            _tbCompile.Enabled = false;
            _tbCompile.Click += ButtonCompileClickHandler;
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
            if (_commandMap.ContainsKey(key)) {
                ToolStripButton item = _commandMap[key];
                item.Enabled = CanPerformCommand(key);
                item.Checked = IsCommandSelected(key);
            }
        }

        private void ResetComponent ()
        {
            foreach (CommandKey key in _commandMap.Keys)
                Invalidate(key);
        }

        public ToolStrip Strip
        {
            get { return _strip; }
        }

        private void BoundButtonClickHandler (object sender, EventArgs e)
        {
            ToolStripButton item = sender as ToolStripButton;
            if (_commandManager != null && _commandMap.ContainsValue(item))
                _commandManager.Perform(_commandMap[item]);
        }

        private void ButtonCompileClickHandler (object sender, EventArgs e)
        {
            /*string path = Path.Combine(Environment.CurrentDirectory, "pcaves.tlp");
            string contentPath = Path.Combine(Environment.CurrentDirectory, "Content");

            ContentBuilder builder = new ContentBuilder();
            builder.Clear();
            builder.Add(path, "pcaves", "TlpImporter", "TlpProcessor");

            string buildError = builder.Build();

            if (!string.IsNullOrEmpty(buildError)) {
                MessageBox.Show(buildError, "Error");
            }
            else {
                foreach (string filename in Directory.EnumerateFiles(builder.OutputDirectory)) {
                    File.Copy(filename, Path.Combine(contentPath, Path.GetFileName(filename)), true);
                }
            }*/
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

        private ToolStripSplitButton CreateSplitButton (string text, Image resource)
        {
            ToolStripSplitButton button = new ToolStripSplitButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = resource;

            return button;
        }

        private ToolStripDropDownButton CreateDropDownButton (string text, Image resource)
        {
            ToolStripDropDownButton button = new ToolStripDropDownButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = resource;

            return button;
        }

        private ToolStripMenuItem DropDownMenuItem (string text, Image resource)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();

            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            item.Text = text;
            item.Image = resource;

            return item;
        }
    }
}
