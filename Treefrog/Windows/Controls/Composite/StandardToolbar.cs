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

        private IStandardToolsPresenter _stdController;
        private IDocumentToolsPresenter _docController;

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
                _tbNewProject, _tbNewItem, _tbOpen, _tbSave, new ToolStripSeparator(),
                _tbCut, _tbCopy, _tbPaste, new ToolStripSeparator(),
                _tbUndo, _tbRedo, new ToolStripSeparator(),
                _tbCompile,
            });

            _commandMap = new Mapper<CommandKey, ToolStripButton>() {
                { CommandKey.Undo, _tbUndo },
                { CommandKey.Redo, _tbRedo },
                { CommandKey.Cut, _tbCut },
                { CommandKey.Copy, _tbCopy },
                { CommandKey.Paste, _tbPaste },
            };

            foreach (ToolStripButton item in _commandMap.Values)
                item.Click += BoundButtonClickHandler;

            ResetStandardComponent();

            _tbNewProject.Click += ButtonNewClickHandler;
            _tbOpen.Click += ButtonOpenClickHandler;
            _tbSave.Click += ButtonSaveClickHandler;

            /*_tbUndo.Click += ButtonUndoClickHandler;
            _tbRedo.Click += ButtonRedoClickHandler;
            _tbCut.Click += ButtonCut_Click;
            _tbCopy.Click += ButtonCopy_Click;
            _tbPaste.Click += ButtonPaste_Click;*/

            _tbCompile.Click += ButtonCompileClickHandler;
        }

        public void BindStandardToolsController (IStandardToolsPresenter controller)
        {
            if (_stdController == controller) {
                return;
            }

            if (_stdController != null) {
                _stdController.SyncStandardToolsActions -= SyncStandardToolsActionsHandler;
            }

            _stdController = controller;

            if (_stdController != null) {
                _stdController.SyncStandardToolsActions += SyncStandardToolsActionsHandler;

                _stdController.RefreshStandardTools();
            }
            else {
                ResetStandardComponent();
            }
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

        public void BindDocumentToolsController (IDocumentToolsPresenter controller)
        {
            if (_docController == controller) {
                return;
            }

            if (_docController != null) {
                _docController.SyncDocumentToolsActions -= SyncDocumentToolsActionsHandler;
            }

            _docController = controller;

            if (_docController != null) {
                _docController.SyncDocumentToolsActions += SyncDocumentToolsActionsHandler;

                _docController.RefreshDocumentTools();
            }
            /*else {
                ResetDocumentComponent();
            }*/
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

        private void ResetStandardComponent ()
        {
            _tbNewProject.Enabled = false;
            _tbNewItem.Enabled = false;
            _tbOpen.Enabled = false;
            _tbSave.Enabled = false;
        }

        /*private void ResetDocumentComponent ()
        {
            _tbCut.Enabled = false;
            _tbCopy.Enabled = false;
            _tbPaste.Enabled = false;
            _tbUndo.Enabled = false;
            _tbRedo.Enabled = false;
        }*/

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

        private void ButtonNewClickHandler (object sender, EventArgs e)
        {
            if (_stdController != null)
                _stdController.ActionCreateProject();
        }

        private void ButtonOpenClickHandler (object sender, EventArgs e)
        {
            if (_stdController != null) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open Project File";
                ofd.Filter = "Treefrog Project Files|*.tlp";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    _stdController.ActionOpenProject(ofd.FileName);
                }
            }
        }

        private void ButtonSaveClickHandler (object sender, EventArgs e)
        {
            if (_stdController != null) {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Title = "Save Project File";
                ofd.Filter = "Treefrog Project Files|*.tlp";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    _stdController.ActionSaveProject(ofd.FileName);
                }
            }
        }

        /*private void ButtonUndoClickHandler (object sender, EventArgs e)
        {
            if (_docController != null)
                _docController.ActionUndo();
        }

        private void ButtonRedoClickHandler (object sender, EventArgs e)
        {
            if (_docController != null)
                _docController.ActionRedo();
        }

        private void ButtonCut_Click (object sender, EventArgs e)
        {
            if (_docController != null)
                _docController.ActionCut();
        }

        private void ButtonCopy_Click (object sender, EventArgs e)
        {
            if (_docController != null)
                _docController.ActionCopy();
        }

        private void ButtonPaste_Click (object sender, EventArgs e)
        {
            if (_docController != null)
                _docController.ActionPaste();
        }*/

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

        private void SyncStandardToolsActionsHandler (object sender, EventArgs e)
        {
            if (_stdController != null) {
                _tbNewProject.Enabled = _stdController.CanCreateProject;
                _tbOpen.Enabled = _stdController.CanOpenProject;
                _tbSave.Enabled = _stdController.CavSaveProject;
            }
        }

        private void SyncDocumentToolsActionsHandler (object sender, EventArgs e)
        {
            if (_docController != null) {
                _tbCut.Enabled = _docController.CanCut;
                _tbCopy.Enabled = _docController.CanCopy;
                _tbPaste.Enabled = _docController.CanPaste;
                _tbUndo.Enabled = _docController.CanUndo;
                _tbRedo.Enabled = _docController.CanRedo;
            }
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
