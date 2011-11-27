using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Treefrog.Presentation;

namespace Treefrog.View.Controls.Composite
{
    public class StandardMenu
    {
        private Assembly _assembly;

        private IEditorPresenter _controller;

        private MenuStrip _menuStrip;

        private ToolStripMenuItem _fileStrip;
        private ToolStripMenuItem _editStrip;
        private ToolStripMenuItem _viewStrip;
        private ToolStripMenuItem _helpStrip;

        private ToolStripMenuItem _fileNew;
        private ToolStripMenuItem _fileOpen;
        private ToolStripMenuItem _fileSave;
        private ToolStripMenuItem _fileSaveAs;
        private ToolStripMenuItem _fileExit;

        private ToolStripMenuItem _editUndo;
        private ToolStripMenuItem _editRedo;
        private ToolStripMenuItem _editCut;
        private ToolStripMenuItem _editCopy;
        private ToolStripMenuItem _editPaste;
        private ToolStripMenuItem _editDelete;
        private ToolStripMenuItem _editSelectAll;
        private ToolStripMenuItem _editSelectNone;

        public StandardMenu () {
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _menuStrip = new MenuStrip();

            _fileStrip = CreateMenuItem("&File");
            _fileNew = CreateMenuItem("&New Project...", "Editor.Icons._16.applications-blue--asterisk.png", Keys.Control | Keys.N);
            _fileOpen = CreateMenuItem("&Open Project...", "Editor.Icons.folder-horizontal-open16.png", Keys.Control | Keys.O);
            _fileSave = CreateMenuItem("&Save Project", "Editor.Icons._16.disk.png", Keys.Control | Keys.S);
            _fileSaveAs = CreateMenuItem("Save Project &As...", "Editor.Icons._16.disk--pencil.png");
            _fileExit = CreateMenuItem("E&xit", Keys.Alt | Keys.F4);

            _editStrip = CreateMenuItem("&Edit");
            _editUndo = CreateMenuItem("&Undo", "Editor.Icons._16.arrow-turn-180-left.png", Keys.Control | Keys.Z);
            _editRedo = CreateMenuItem("&Redo", "Editor.Icons._16.arrow-turn.png", Keys.Control | Keys.Y);
            _editCut = CreateMenuItem("Cu&t", "Editor.Icons._16.scissors.png", Keys.Control | Keys.X);
            _editCopy = CreateMenuItem("&Copy", "Editor.Icons._16.documents.png", Keys.Control | Keys.C);
            _editPaste = CreateMenuItem("&Paste", "Editor.Icons._16.clipboard-paste.png", Keys.Control | Keys.V);
            _editDelete = CreateMenuItem("&Delete", "Editor.Icons._16.cross.png", Keys.Delete);
            _editSelectAll = CreateMenuItem("Select &All", "Editor.Icons._16.selection-select.png", Keys.Control | Keys.A);
            _editSelectNone = CreateMenuItem("Select &None", "Editor.Icons._16.selection.png", Keys.Control | Keys.Shift | Keys.A);

            _viewStrip = CreateMenuItem("&View");

            _helpStrip = CreateMenuItem("&Help");

            _fileStrip.DropDownItems.AddRange(new ToolStripItem[] {
                _fileNew, _fileOpen, _fileSave, _fileSaveAs, new ToolStripSeparator(),
                _fileExit,
            });

            _editStrip.DropDownItems.AddRange(new ToolStripItem[] {
                _editUndo, _editRedo, new ToolStripSeparator(),
                _editCut, _editCopy, _editPaste, _editDelete, new ToolStripSeparator(),
                _editSelectAll, _editSelectNone,
            });

            _menuStrip.Items.AddRange(new ToolStripItem[] {
                _fileStrip, _editStrip, _viewStrip, _helpStrip,
            });

            ResetStandardComponent();
            ResetDocumentComponent();

            // Wire Menu Items

            _fileOpen.Click += MenuOpenClickHandler;
            _fileSave.Click += MenuSaveClickHandler;

            _editUndo.Click += MenuUndoClickHandler;
            _editRedo.Click += MenuRedoClickHandler;
        }

        public void BindController (IEditorPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                IStandardToolsPresenter stdTools = _controller.Presentation.StandardTools;
                IDocumentToolsPresenter docTools = _controller.Presentation.DocumentTools;

                if (stdTools != null)
                    stdTools.SyncStandardToolsActions -= SyncStandardToolsActionsHandler;
                if (docTools != null)
                    docTools.SyncDocumentToolsActions -= SyncDocumentToolsActionsHandler;
            }

            _controller = controller;

            if (_controller != null) {
                IStandardToolsPresenter stdTools = _controller.Presentation.StandardTools;
                IDocumentToolsPresenter docTools = _controller.Presentation.DocumentTools;

                if (stdTools != null) {
                    stdTools.SyncStandardToolsActions += SyncStandardToolsActionsHandler;
                    stdTools.RefreshStandardTools();
                }
                if (docTools != null) {
                    docTools.SyncDocumentToolsActions += SyncDocumentToolsActionsHandler;
                    docTools.RefreshDocumentTools();
                }
            }
            else {
                ResetStandardComponent();
                ResetDocumentComponent();
            }
        }

        private void ResetStandardComponent ()
        {

        }

        private void ResetDocumentComponent ()
        {
            _editCut.Enabled = false;
            _editCopy.Enabled = false;
            _editPaste.Enabled = false;
            _editUndo.Enabled = false;
            _editRedo.Enabled = false;
            _editDelete.Enabled = false;
            _editSelectAll.Enabled = false;
            _editSelectNone.Enabled = false;
        }

        public MenuStrip Strip
        {
            get { return _menuStrip; }
        }

        private void MenuOpenClickHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Open Project File";
                ofd.Filter = "Treefrog Project Files|*.tlp";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    _controller.Presentation.StandardTools.ActionOpenProject(ofd.FileName);
                }
            }
        }

        private void MenuSaveClickHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.Title = "Save Project File";
                ofd.Filter = "Treefrog Project Files|*.tlp";
                ofd.OverwritePrompt = true;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    _controller.Presentation.StandardTools.ActionSaveProject(ofd.FileName);
                }
            }
        }

        private void MenuUndoClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionUndo();
        }

        private void MenuRedoClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionRedo();
        }

        private void SyncStandardToolsActionsHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            IStandardToolsPresenter stdTools = _controller.Presentation.StandardTools;
            if (stdTools != null) {
                //_tbNewProject.Enabled = _stdController.CanCreateProject;
                //_tbOpen.Enabled = _stdController.CanOpenProject;
                //_tbSave.Enabled = _stdController.CavSaveProject;
            }
        }

        private void SyncDocumentToolsActionsHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            IDocumentToolsPresenter docTools = _controller.Presentation.DocumentTools;
            if (docTools != null) {
                _editCut.Enabled = docTools.CanCut;
                _editCopy.Enabled = docTools.CanCopy;
                _editPaste.Enabled = docTools.CanPaste;
                _editUndo.Enabled = docTools.CanUndo;
                _editRedo.Enabled = docTools.CanRedo;
            }
        }

        private ToolStripMenuItem CreateMenuItem (string text)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            return item;
        }

        private ToolStripMenuItem CreateMenuItem (string text, Keys keys)
        {
            ToolStripMenuItem item = CreateMenuItem(text);
            item.ShortcutKeys = keys;
            return item;
        }

        private ToolStripMenuItem CreateMenuItem (string text, string resource)
        {
            ToolStripMenuItem item = CreateMenuItem(text);
            item.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));
            return item;
        }

        private ToolStripMenuItem CreateMenuItem (string text, string resource, Keys keys)
        {
            ToolStripMenuItem item = CreateMenuItem(text, resource);
            item.ShortcutKeys = keys;
            return item;
        }
    }
}
