using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Treefrog.Presentation;
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

        private Assembly _assembly;

        private CommandManager _commandManager;
        private Mapper<CommandKey, ToolStripButton> _commandMap = new Mapper<CommandKey, ToolStripButton>();

        public StandardToolbar ()
        {
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _tbNewProject = CreateButton("New Project (Ctrl+N)", "Treefrog.Icons._16.applications-blue--asterisk.png");
            _tbNewItem = CreateDropDownButton("Add New Item", "Treefrog.Icons._16.map--asterisk.png");
            _tbNewItem.DropDownItems.AddRange(new ToolStripItem[] {
                DropDownMenuItem("Add New Level", "Treefrog.Icons._16.map--asterisk.png"),
                DropDownMenuItem("Add New Tile Pool", "Treefrog.Icons.color-swatch16.png"),
            });
            _tbOpen = CreateButton("Open Project (Ctrl+O)", "Treefrog.Icons.folder-horizontal-open16.png");
            _tbSave = CreateButton("Save Project (Ctrl+S)", "Treefrog.Icons.disk16.png");
            _tbCut = CreateButton("Cut (Ctrl+X)", "Treefrog.Icons.scissors16.png");
            _tbCopy = CreateButton("Copy (Ctrl+C)", "Treefrog.Icons.document-copy16.png");
            _tbPaste = CreateButton("Paste (Ctrl+V)", "Treefrog.Icons.clipboard-paste16.png");
            _tbUndo = CreateButton("Undo (Ctrl+Z)", "Treefrog.Icons.arrow-turn-180-left16.png");
            _tbRedo = CreateButton("Redo (Ctrl+Y)", "Treefrog.Icons.arrow-turn16.png");
            _tbCompile = CreateButton("Compile", "Treefrog.Icons._16.compile.png");

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

        private ToolStripButton CreateButton (string text, string resource)
        {
            ToolStripButton button = new ToolStripButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new System.Drawing.Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripSplitButton CreateSplitButton (string text, string resource)
        {
            ToolStripSplitButton button = new ToolStripSplitButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripDropDownButton CreateDropDownButton (string text, string resource)
        {
            ToolStripDropDownButton button = new ToolStripDropDownButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }

        private ToolStripMenuItem DropDownMenuItem (string text, string resource)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();

            item.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            item.Text = text;
            item.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return item;
        }
    }
}
