using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Controls.Composite
{
    public class StandardMenu
    {
        private IEditorPresenter _controller;

        private UICommandController _commandController;

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

/*        private ToolStripMenuItem _levelsExportRaster;

        private ToolStripMenuItem _objectsGroups;
        private ToolStripMenuItem _objectsAlign;
        private ToolStripMenuItem _objectsAlignTop;
        private ToolStripMenuItem _objectsAlignBottom;
        private ToolStripMenuItem _objectsAlignLeft;
        private ToolStripMenuItem _objectsAlignRight;
        private ToolStripMenuItem _objectsAlignCenterHorz;
        private ToolStripMenuItem _objectsAlignCenterVert;*/

        public StandardMenu () 
        {
            _menuStrip = new MenuStrip();

            _fileStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&File", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.NewProject, CommandKey.OpenProject, CommandKey.Save, CommandKey.SaveAs,
                },
                new CommandMenuGroup() {
                    CommandKey.Exit,
                },
            }));

            _editStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Edit", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.Undo, CommandKey.Redo,
                },
                new CommandMenuGroup() {
                    CommandKey.Cut, CommandKey.Copy, CommandKey.Paste, CommandKey.Delete,
                },
                new CommandMenuGroup() {
                    CommandKey.SelectAll, CommandKey.SelectNone,
                },
            }));

            _viewStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&View", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.ViewZoomNormal, CommandKey.ViewZoomIn, CommandKey.ViewZoomOut,
                },
                new CommandMenuGroup() {
                    CommandKey.ViewGrid,
                },
            }));

            _projectStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Project", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.ProjectAddLevel,
                },
            }));

            _levelStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Level", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.LevelResize,
                },
            }));

            _layerStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("La&yers", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    CommandKey.NewTileLayer, CommandKey.NewObjectLayer,
                },
                new CommandMenuGroup() {
                    CommandKey.LayerClone, CommandKey.LayerDelete, CommandKey.LayerProperties,
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(new CommandMenu("&Arrange", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.LayerMoveTop, CommandKey.LayerMoveUp, CommandKey.LayerMoveDown, CommandKey.LayerMoveBottom,
                        },
                    })),
                    new CommandMenuEntry(new CommandMenu("&View", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.LayerShowCurrentOnly, CommandKey.LayerShowAll, CommandKey.LayerShowNone,
                        },
                    })),
                },
                new CommandMenuGroup() {
                    CommandKey.LayerExportRaster,
                },
            }));

            _tileStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Tiles", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(new CommandMenu("&Brushes", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.NewStaticTileBrush, CommandKey.NewDynamicTileBrush,
                        },
                        new CommandMenuGroup() {
                            CommandKey.TileBrushClone, CommandKey.TileBrushDelete,
                        },
                    })),
                    new CommandMenuEntry(new CommandMenu("&Selections", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.TileSelectionCreateBrush, CommandKey.TileSelectionPromoteLayer,
                        },
                        new CommandMenuGroup() {
                            CommandKey.TileSelectionFloat, CommandKey.TileSelectionDefloat,
                        },
                    })),
                },
                new CommandMenuGroup() {
                    CommandKey.TilePoolExport, CommandKey.TilePoolImportOver,
                },
            }));

            _objectStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Objects", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(new CommandMenu("Object &Prototypes", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.ObjectProtoImport
                        },
                        new CommandMenuGroup() {
                            CommandKey.ObjectProtoEdit, CommandKey.ObjectProtoClone, CommandKey.ObjectProtoDelete, 
                            CommandKey.ObjectProtoRename, 
                        },
                        new CommandMenuGroup() {
                            CommandKey.ObjectProtoProperties,
                        },
                    })),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(new CommandMenu("&Arrange", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.ObjectMoveTop, CommandKey.ObjectMoveUp, CommandKey.ObjectMoveDown, CommandKey.ObjectMoveBottom,
                        },
                    })),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(new CommandMenu("&Reference Point", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.ObjectReferenceImage, CommandKey.ObjectReferenceMask, CommandKey.ObjectReferenceOrigin,
                        },
                    })),
                    new CommandMenuEntry(new CommandMenu("&Snapping", new List<CommandMenuGroup>() {
                        new CommandMenuGroup() {
                            CommandKey.ObjectSnappingNone, CommandKey.ObjectSnappingTopLeft, CommandKey.ObjectSnappingTopRight,
                            CommandKey.ObjectSnappingBottomLeft, CommandKey.ObjectSnappingBottomRight, CommandKey.ObjectSnappingTop,
                            CommandKey.ObjectSnappingBottom, CommandKey.ObjectSnappingLeft, CommandKey.ObjectSnappingRight,
                            CommandKey.ObjectSnappingVert, CommandKey.ObjectSnappingHorz, CommandKey.ObjectSnappingCenter,
                        },
                    })),
                },
            }));

            _helpStrip = CommandMenuBuilder.BuildToolStripMenu(new CommandMenu("&Help"));

            _commandController = new UICommandController();
            _commandController.MapMenuItems(new List<ToolStripMenuItem>() {
                _fileStrip, _editStrip, _viewStrip, _projectStrip, _levelStrip, _layerStrip, _tileStrip, _objectStrip, _helpStrip
            });

            _menuStrip.Items.AddRange(new ToolStripItem[] {
                _fileStrip, _editStrip, _viewStrip, _projectStrip, _levelStrip, _layerStrip,
                _tileStrip, _objectStrip, _helpStrip,
            });
        }

        public void BindCommandManager (CommandManager commandManager)
        {
            _commandController.BindCommandManager(commandManager);
        }

        public void BindController (IEditorPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            _controller = controller;
        }

        public MenuStrip Strip
        {
            get { return _menuStrip; }
        }
    }
}
