using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using System.Collections.Generic;
using Treefrog.Utility;

namespace Treefrog.Windows.Controls.Composite
{
    public class StandardMenu
    {
        private Assembly _assembly;

        private IEditorPresenter _controller;

        private CommandManager _commandManager;
        private Mapper<CommandKey, ToolStripMenuItem> _commandMap = new Mapper<CommandKey, ToolStripMenuItem>();

        private MenuStrip _menuStrip;

        private ToolStripMenuItem _fileStrip;
        private ToolStripMenuItem _editStrip;
        private ToolStripMenuItem _viewStrip;
        private ToolStripMenuItem _projectStrip;
        private ToolStripMenuItem _levelStrip;
        private ToolStripMenuItem _layerStrip;
        private ToolStripMenuItem _tileStrip;
        private ToolStripMenuItem _objectStrip;
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

        private ToolStripMenuItem _levelsNew;

        private ToolStripMenuItem _layersNewTile;
        private ToolStripMenuItem _layersNewObject;
        private ToolStripMenuItem _layersClone;
        private ToolStripMenuItem _layersDelete;
        private ToolStripMenuItem _layersProperties;
        private ToolStripMenuItem _layersArrange;
        private ToolStripMenuItem _layersArrangeMoveTop;
        private ToolStripMenuItem _layersArrangeMoveUp;
        private ToolStripMenuItem _layersArrangeMoveDown;
        private ToolStripMenuItem _layersArrangeMoveBottom;
        private ToolStripMenuItem _layersView;
        private ToolStripMenuItem _layersViewShowCurrentOnly;
        private ToolStripMenuItem _layersViewShowAll;
        private ToolStripMenuItem _layersViewShowNone;
        private ToolStripMenuItem _layersExportRaster;

        private ToolStripMenuItem _tilesExport;
        private ToolStripMenuItem _tilesImportOver;

        private ToolStripMenuItem _objectsGroups;
        private ToolStripMenuItem _objectsProtos;
        private ToolStripMenuItem _objectsProtosImport;
        private ToolStripMenuItem _objectsProtosClone;
        private ToolStripMenuItem _objectsProtosDelete;
        private ToolStripMenuItem _objectsProtosProperties;
        private ToolStripMenuItem _objectsAlign;
        private ToolStripMenuItem _objectsAlignTop;
        private ToolStripMenuItem _objectsAlignBottom;
        private ToolStripMenuItem _objectsAlignLeft;
        private ToolStripMenuItem _objectsAlignRight;
        private ToolStripMenuItem _objectsAlignCenterHorz;
        private ToolStripMenuItem _objectsAlignCenterVert;
        private ToolStripMenuItem _objectsArrange;
        private ToolStripMenuItem _objectsArrangeMoveTop;
        private ToolStripMenuItem _objectsArrangeMoveUp;
        private ToolStripMenuItem _objectsArrangeMoveDown;
        private ToolStripMenuItem _objectsArrangeMoveBottom;
        private ToolStripMenuItem _objectsReference;
        private ToolStripMenuItem _objectsReferenceImageBounds;
        private ToolStripMenuItem _objectsReferenceMaskBounds;
        private ToolStripMenuItem _objectsReferenceOrigin;
        private ToolStripMenuItem _objectsSnapping;
        private ToolStripMenuItem _objectsSnappingNone;
        private ToolStripMenuItem _objectsSnappingTopLeft;
        private ToolStripMenuItem _objectsSnappingTopRight;
        private ToolStripMenuItem _objectsSnappingBottomLeft;
        private ToolStripMenuItem _objectsSnappingBottomRight;
        private ToolStripMenuItem _objectsSnappingTop;
        private ToolStripMenuItem _objectsSnappingBottom;
        private ToolStripMenuItem _objectsSnappingLeft;
        private ToolStripMenuItem _objectsSnappingRight;
        private ToolStripMenuItem _objectsSnappingCenterHorz;
        private ToolStripMenuItem _objectsSnappingCenterVert;
        private ToolStripMenuItem _objectsSnappingCenter;

        public StandardMenu () {
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _menuStrip = new MenuStrip();

            _fileStrip = CreateMenuItem("&File");
            _fileNew = CreateMenuItem("&New Project...", "Treefrog.Icons._16.applications-blue--asterisk.png", Keys.Control | Keys.N);
            _fileOpen = CreateMenuItem("&Open Project...", "Treefrog.Icons.folder-horizontal-open16.png", Keys.Control | Keys.O);
            _fileSave = CreateMenuItem("&Save Project", "Treefrog.Icons._16.disk.png", Keys.Control | Keys.S);
            _fileSaveAs = CreateMenuItem("Save Project &As...", "Treefrog.Icons._16.disk--pencil.png");
            _fileExit = CreateMenuItem("E&xit", Keys.Alt | Keys.F4);

            _editStrip = CreateMenuItem("&Edit");
            _editUndo = CreateMenuItem("&Undo", "Treefrog.Icons._16.arrow-turn-180-left.png", Keys.Control | Keys.Z);
            _editRedo = CreateMenuItem("&Redo", "Treefrog.Icons._16.arrow-turn.png", Keys.Control | Keys.Y);
            _editCut = CreateMenuItem("Cu&t", "Treefrog.Icons._16.scissors.png", Keys.Control | Keys.X);
            _editCopy = CreateMenuItem("&Copy", "Treefrog.Icons._16.documents.png", Keys.Control | Keys.C);
            _editPaste = CreateMenuItem("&Paste", "Treefrog.Icons._16.clipboard-paste.png", Keys.Control | Keys.V);
            _editDelete = CreateMenuItem("&Delete", "Treefrog.Icons._16.cross.png", Keys.Delete);
            _editSelectAll = CreateMenuItem("Select &All", "Treefrog.Icons._16.selection-select.png", Keys.Control | Keys.A);
            _editSelectNone = CreateMenuItem("Select &None", "Treefrog.Icons._16.selection.png", Keys.Control | Keys.Shift | Keys.A);

            _viewStrip = CreateMenuItem("&View");

            _projectStrip = CreateMenuItem("&Project");

            _levelStrip = CreateMenuItem("&Levels");

            _layerStrip = CreateMenuItem("La&yers");
            _layersNewTile = CreateMenuItem("New &Tile Layer", "Treefrog.Icons._16.grid.png");
            _layersNewObject = CreateMenuItem("New &Object Layer", "Treefrog.Icons._16.game.png");
            _layersClone = CreateMenuItem("D&uplicate", "Treefrog.Icons._16.layers.png");
            _layersDelete = CreateMenuItem("&Delete", "Treefrog.Icons._16.layer--minus.png");
            _layersProperties = CreateMenuItem("&Properties", "Treefrog.Icons._16.tags.png");
            _layersArrange = CreateMenuItem("&Arrange");
            _layersArrangeMoveTop = CreateMenuItem("Bring to &Top", "Treefrog.Icons._16.arrow-skip-090.png");
            _layersArrangeMoveUp = CreateMenuItem("Move &Up", "Treefrog.Icons._16.arrow-090.png");
            _layersArrangeMoveDown = CreateMenuItem("Move &Down", "Treefrog.Icons._16.arrow-270.png");
            _layersArrangeMoveBottom = CreateMenuItem("Send to &Bottom", "Treefrog.Icons._16.arrow-skip-270.png");
            _layersView = CreateMenuItem("&View");
            _layersViewShowCurrentOnly = CreateMenuItem("Show &Current Layer Only");
            _layersViewShowAll = CreateMenuItem("&All");
            _layersViewShowNone = CreateMenuItem("&None");

            _tileStrip = CreateMenuItem("&Tiles");
            _tilesExport = CreateMenuItem("&Export", "Treefrog.Icons._16.drive-download.png");
            _tilesImportOver = CreateMenuItem("&Import Over", "Treefrog.Icons._16.drive-upload.png");

            _objectStrip = CreateMenuItem("&Objects");
            _objectsProtos = CreateMenuItem("Object &Prototypes");
            _objectsProtosImport = CreateMenuItem("Import Object from Image", "Treefrog.Icons._16.game--plus.png");
            _objectsProtosClone = CreateMenuItem("D&uplicate");
            _objectsProtosDelete = CreateMenuItem("&Delete", "Treefrog.Icons._16.game--minus.png");
            _objectsProtosProperties = CreateMenuItem("&Properties", "Treefrog.Icons._16.tags.png");
            _objectsReference = CreateMenuItem("&Reference Point");
            _objectsReferenceImageBounds = CreateMenuItem("&Image Bounds", "Treefrog.Icons._16.snap-borders.png");
            _objectsReferenceMaskBounds = CreateMenuItem("&Mask Bounds", "Treefrog.Icons._16.snap-bounds.png");
            _objectsReferenceOrigin = CreateMenuItem("&Origin", "Treefrog.Icons._16.snap-origin.png");
            _objectsSnapping = CreateMenuItem("&Snapping");
            _objectsSnappingNone = CreateMenuItem("&None", "Treefrog.Icons._16.snap-none.png");
            _objectsSnappingTopLeft = CreateMenuItem("To&p Left", "Treefrog.Icons._16.snap-topleft.png");
            _objectsSnappingTopRight = CreateMenuItem("Top R&ight", "Treefrog.Icons._16.snap-topright.png");
            _objectsSnappingBottomLeft = CreateMenuItem("Botto&m Left", "Treefrog.Icons._16.snap-bottomleft.png");
            _objectsSnappingBottomRight = CreateMenuItem("Bottom Ri&ght", "Treefrog.Icons._16.snap-bottomright.png");
            _objectsSnappingTop = CreateMenuItem("&Top", "Treefrog.Icons._16.snap-top.png");
            _objectsSnappingBottom = CreateMenuItem("&Bottom", "Treefrog.Icons._16.snap-bottom.png");
            _objectsSnappingLeft = CreateMenuItem("&Left", "Treefrog.Icons._16.snap-left.png");
            _objectsSnappingRight = CreateMenuItem("&Right", "Treefrog.Icons._16.snap-right.png");
            _objectsSnappingCenterVert = CreateMenuItem("Center &Vertical", "Treefrog.Icons._16.snap-horizontal.png");
            _objectsSnappingCenterHorz = CreateMenuItem("Center &Horizontal", "Treefrog.Icons._16.snap-vertical.png");
            _objectsSnappingCenter = CreateMenuItem("&Center", "Treefrog.Icons._16.snap-center.png");

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

            _layerStrip.DropDownItems.AddRange(new ToolStripItem[] {
                _layersNewTile, _layersNewObject, new ToolStripSeparator(),
                _layersClone, _layersDelete, _layersProperties, new ToolStripSeparator(),
                _layersArrange, _layersView,
            });

            _layersArrange.DropDownItems.AddRange(new ToolStripItem[] {
                _layersArrangeMoveTop, _layersArrangeMoveUp, _layersArrangeMoveDown, _layersArrangeMoveBottom,
            });

            _layersView.DropDownItems.AddRange(new ToolStripItem[] {
                _layersViewShowCurrentOnly, _layersViewShowAll, _layersViewShowNone,
            });

            _tileStrip.DropDownItems.AddRange(new ToolStripItem[] {
                _tilesExport, _tilesImportOver,
            });

            _objectStrip.DropDownItems.AddRange(new ToolStripItem[] {
                _objectsProtos, new ToolStripSeparator(),
                _objectsReference, _objectsSnapping,
            });

            _objectsProtos.DropDownItems.AddRange(new ToolStripItem[] {
                _objectsProtosImport, new ToolStripSeparator(),
                _objectsProtosClone, _objectsProtosDelete, _objectsProtosProperties,
            });

            _objectsReference.DropDownItems.AddRange(new ToolStripItem[] {
                _objectsReferenceImageBounds, _objectsReferenceMaskBounds, _objectsReferenceOrigin,
            });

            _objectsSnapping.DropDownItems.AddRange(new ToolStripItem[] {
                _objectsSnappingNone, 
                _objectsSnappingTopLeft, _objectsSnappingTopRight, _objectsSnappingBottomLeft, _objectsSnappingBottomRight,
                _objectsSnappingTop, _objectsSnappingBottom, _objectsSnappingLeft, _objectsSnappingRight,
                _objectsSnappingCenterVert, _objectsSnappingCenterHorz, _objectsSnappingCenter,
            });

            _menuStrip.Items.AddRange(new ToolStripItem[] {
                _fileStrip, _editStrip, _viewStrip, _projectStrip, _levelStrip, _layerStrip,
                _tileStrip, _objectStrip, _helpStrip,
            });

            _commandMap = new Mapper<CommandKey, ToolStripMenuItem>() {
                { CommandKey.Undo, _editUndo },
                { CommandKey.Redo, _editRedo },
                { CommandKey.Cut, _editCut },
                { CommandKey.Copy, _editCopy },
                { CommandKey.Paste, _editPaste },
                { CommandKey.Delete, _editDelete },
                { CommandKey.SelectAll, _editSelectAll },
                { CommandKey.SelectNone, _editSelectNone },

                { CommandKey.NewTileLayer, _layersNewTile },
                { CommandKey.NewObjectLayer, _layersNewObject },
                { CommandKey.LayerClone, _layersClone },
                { CommandKey.LayerDelete, _layersDelete },
                { CommandKey.LayerProperties, _layersProperties },
                { CommandKey.LayerMoveTop, _layersArrangeMoveTop },
                { CommandKey.LayerMoveUp, _layersArrangeMoveUp },
                { CommandKey.LayerMoveDown, _layersArrangeMoveDown },
                { CommandKey.LayerMoveBottom, _layersArrangeMoveBottom },
                { CommandKey.LayerShowCurrentOnly, _layersViewShowCurrentOnly },
                { CommandKey.LayerShowAll, _layersViewShowAll },
                { CommandKey.LayerShowNone, _layersViewShowNone },

                { CommandKey.TilePoolExport, _tilesExport },
                { CommandKey.TilePoolImportOver, _tilesImportOver },

                { CommandKey.ObjectProtoImport, _objectsProtosImport },
                { CommandKey.ObjectProtoClone, _objectsProtosClone },
                { CommandKey.ObjectProtoDelete, _objectsProtosDelete },
                { CommandKey.ObjectProtoProperties, _objectsProtosProperties },
                { CommandKey.ObjectReferenceImage, _objectsReferenceImageBounds },
                { CommandKey.ObjectReferenceMask, _objectsReferenceMaskBounds },
                { CommandKey.ObjectReferenceOrigin, _objectsReferenceOrigin },
                { CommandKey.ObjectSnappingNone, _objectsSnappingNone },
                { CommandKey.ObjectSnappingTopLeft, _objectsSnappingTopLeft },
                { CommandKey.ObjectSnappingTopRight, _objectsSnappingTopRight },
                { CommandKey.ObjectSnappingBottomLeft, _objectsSnappingBottomLeft },
                { CommandKey.ObjectSnappingBottomRight, _objectsSnappingBottomRight },
                { CommandKey.ObjectSnappingTop, _objectsSnappingTop },
                { CommandKey.ObjectSnappingBottom, _objectsSnappingBottom },
                { CommandKey.ObjectSnappingLeft, _objectsSnappingLeft },
                { CommandKey.ObjectSnappingRight, _objectsSnappingRight },
                { CommandKey.ObjectSnappingVert, _objectsSnappingCenterVert },
                { CommandKey.ObjectSnappingHorz, _objectsSnappingCenterHorz },
                { CommandKey.ObjectSnappingCenter, _objectsSnappingCenter },
            };

            foreach (ToolStripMenuItem item in _commandMap.Values)
                item.Click += BoundMenuItemClickHandler;

            ResetStandardComponent();
            ResetDocumentComponent();

            // Wire Menu Items

            _fileOpen.Click += MenuOpenClickHandler;
            _fileSave.Click += MenuSaveClickHandler;

            /*_editUndo.Click += MenuUndoClickHandler;
            _editRedo.Click += MenuRedoClickHandler;
            _editCut.Click += MenuCut_Click;
            _editCopy.Click += MenuCopy_Click;
            _editPaste.Click += MenuPaste_Click;
            _editDelete.Click += MenuDelete_Click;
            _editSelectAll.Click += MenuSelectAll_Click;
            _editSelectNone.Click += MenuSelectNone_Click;*/
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
                ToolStripMenuItem item = _commandMap[key];
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

        private void BoundMenuItemClickHandler (object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (_commandManager != null && _commandMap.ContainsValue(item))
                _commandManager.Perform(_commandMap[item]);
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

        private void MenuCut_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionCut();
        }

        private void MenuCopy_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionCopy();
        }

        private void MenuPaste_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionPaste();
        }

        private void MenuDelete_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionDelete();
        }

        private void MenuSelectAll_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionSelectAll();
        }

        private void MenuSelectNone_Click (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.Presentation.DocumentTools.ActionUnselectAll();
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

            /*IDocumentToolsPresenter docTools = _controller.Presentation.DocumentTools;
            if (docTools != null) {
                _editCut.Enabled = docTools.CanCut;
                _editCopy.Enabled = docTools.CanCopy;
                _editPaste.Enabled = docTools.CanPaste;
                _editDelete.Enabled = docTools.CanDelete;
                _editSelectAll.Enabled = docTools.CanSelectAll;
                _editSelectNone.Enabled = docTools.CanUnselectAll;
                _editUndo.Enabled = docTools.CanUndo;
                _editRedo.Enabled = docTools.CanRedo;
            }*/
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
