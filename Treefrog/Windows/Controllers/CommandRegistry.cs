using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Presentation.Commands;

namespace Treefrog.Windows.Controllers
{
    public struct CommandMenuTag
    {
        private readonly CommandKey _key;
        private readonly object _param;

        public CommandKey Key
        {
            get { return _key; }
        }

        public object Param
        {
            get { return _param; }
        }

        public CommandMenuTag (CommandKey key)
            : this(key, null)
        { }

        public CommandMenuTag (CommandKey key, object param)
        {
            _key = key;
            _param = param;
        }
    }

    public static class CommandMenuBuilder
    {
        public static ToolStripMenuItem BuildToolStripMenu (CommandMenu menu)
        {
            return BuildToolStripMenu(menu, CommandRegistry.Default);
        }

        public static ToolStripMenuItem BuildToolStripMenu (CommandMenu menu, CommandRegistry registry)
        {
            ToolStripMenuItem strip = new ToolStripMenuItem(menu.Name);
            if (menu.Groups == null)
                return strip;

            PopulateItems(strip.DropDownItems, menu.Groups, registry);
            return strip;
        }

        public static ContextMenuStrip BuildContextMenu (CommandMenu menu)
        {
            return BuildContextMenu(menu, CommandRegistry.Default);
        }

        public static ContextMenuStrip BuildContextMenu (CommandMenu menu, CommandRegistry registry)
        {
            ContextMenuStrip strip = new ContextMenuStrip();
            if (menu.Groups == null)
                return strip;

            PopulateItems(strip.Items, menu.Groups, registry);
            return strip;
        }

        private static void PopulateItems (ToolStripItemCollection items, IEnumerable<CommandMenuGroup> groups, CommandRegistry registry)
        {
            List<CommandMenuGroup> groupList = groups as List<CommandMenuGroup>;
            if (groupList == null)
                groupList = new List<CommandMenuGroup>(groups);

            foreach (CommandMenuGroup group in groupList) {
                if (group != groupList[0])
                    items.Add(new ToolStripSeparator());

                foreach (CommandMenuEntry entry in group) {
                    if (entry.SubMenu != null)
                        items.Add(BuildToolStripMenu(entry.SubMenu, registry));
                    else {
                        CommandRecord record = registry.Lookup(entry.Key);
                        if (record != null) {
                            ToolStripMenuItem menuItem = new ToolStripMenuItem() {
                                Tag = new CommandMenuTag(entry.Key, entry.Param),
                                Text = record.DisplayName,
                                Image = record.Image,
                                ShortcutKeys = record.Shortcut,
                                ShortcutKeyDisplayString = record.ShortcutDisplay,
                                ToolTipText = record.Description,
                            };

                            items.Add(menuItem);
                        }
                    }
                }
            }
        }

        /*public static MenuItem BuildMenu (CommandMenu menu, CommandRegistry registry)
        {
            MenuItem item = new MenuItem(menu.Name);
            if (menu.Groups == null)
                return item;

            List<CommandMenuGroup> groups = new List<CommandMenuGroup>(menu.Groups);
            foreach (CommandMenuGroup group in groups) {
                if (group != groups[0])
                    item.MenuItems.Add();

                foreach (CommandMenuEntry entry in group) {
                    if (entry.SubMenu != null)
                        item.DropDownItems.Add(BuildToolStripMenu(entry.SubMenu, registry));
                    else {
                        CommandRecord record = registry.Lookup(entry.Key);
                        if (record != null) {
                            ToolStripMenuItem menuItem = new ToolStripMenuItem() {
                                Tag = entry.Key,
                                Text = record.DisplayName,
                                Image = record.Image,
                                ShortcutKeys = record.Shortcut,
                                ShortcutKeyDisplayString = record.ShortcutDisplay,
                                ToolTipText = record.Description,
                            };

                            item.DropDownItems.Add(menuItem);
                        }
                    }
                }
            }

            return item;
        }*/
    }

    public class CommandRecord
    {
        public CommandKey Key { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public Image Image { get; set; }
        public Keys Shortcut { get; set; }
        public string ShortcutDisplay { get; set; }

        public CommandRecord (CommandKey key)
        {
            Key = key;
        }
    }

    public class CommandRegistry
    {
        private Dictionary<CommandKey, CommandRecord> _registry = new Dictionary<CommandKey, CommandRecord>();

        public CommandRegistry ()
        { }

        public IEnumerable<CommandRecord> RegisteredCommands
        {
            get
            {
                foreach (KeyValuePair<CommandKey, CommandRecord> kv in _registry)
                    yield return kv.Value;
            }
        }

        public CommandRecord Lookup (CommandKey key)
        {
            CommandRecord inst;
            if (_registry.TryGetValue(key, out inst))
                return inst;
            else
                return null;
        }

        public void Register (CommandRecord inst)
        {
            _registry[inst.Key] = inst;
        }

        public static CommandRegistry Default
        {
            get { return _default; }
        }

        static CommandRegistry _default;

        static CommandRegistry ()
        {
            _default = new CommandRegistry();

            Register(CommandKey.NewProject, "&New Project...", 
                resource: Properties.Resources.ApplicationsBlueAst, shortcut: Keys.Control | Keys.N);
            Register(CommandKey.OpenProject, "&Open Project...", 
                resource: Properties.Resources.FolderHorizontalOpen, shortcut: Keys.Control | Keys.O);
            Register(CommandKey.Save, "&Save Project",
                resource: Properties.Resources.Disk, shortcut: Keys.Control | Keys.S);
            Register(CommandKey.SaveAs, "Save Project &As...",
                resource: Properties.Resources.DiskPencil, shortcut: Keys.Control | Keys.Shift | Keys.S);
            Register(CommandKey.Exit, "E&xit", 
                shortcut: Keys.Alt | Keys.F4);

            Register(CommandKey.Undo, "&Undo",
                resource: Properties.Resources.ArrowTurn180Left, shortcut: Keys.Control | Keys.Z);
            Register(CommandKey.Redo, "&Redo",
                resource: Properties.Resources.ArrowTurn, shortcut: Keys.Control | Keys.Y);
            Register(CommandKey.Cut, "Cu&t",
                resource: Properties.Resources.Scissors, shortcut: Keys.Control | Keys.X);
            Register(CommandKey.Copy, "&Copy",
                resource: Properties.Resources.Documents, shortcut: Keys.Control | Keys.C);
            Register(CommandKey.Paste, "&Paste",
                resource: Properties.Resources.ClipboardPaste, shortcut: Keys.Control | Keys.V);
            Register(CommandKey.Delete, "&Delete",
                resource: Properties.Resources.Cross, shortcut: Keys.Delete);
            Register(CommandKey.SelectAll, "Select &All",
                resource: Properties.Resources.SelectionSelect, shortcut: Keys.Control | Keys.A);
            Register(CommandKey.SelectNone, "Select &None",
                resource: Properties.Resources.Selection, shortcut: Keys.Control | Keys.Shift | Keys.A);

            Register(CommandKey.ViewZoomNormal, "Zoom &1:1", 
                resource: Properties.Resources.MagnifierZoomActual);
            Register(CommandKey.ViewZoomIn, "Zoom &In",
                resource: Properties.Resources.MagnifierZoomIn, shortcut: Keys.Control | Keys.Add, shortcutDisplay: "Ctrl+Plus");
            Register(CommandKey.ViewZoomOut, "Zoom &Out",
                resource: Properties.Resources.MagnifierZoomOut, shortcut: Keys.Control | Keys.Subtract, shortcutDisplay: "Ctrl+Minus");
            Register(CommandKey.ViewGrid, "Show &Grid",
                resource: Properties.Resources.Grid, shortcut: Keys.Control | Keys.G);

            Register(CommandKey.ProjectAddLevel, "New &Level...",
                resource: Properties.Resources.MapAsterisk);

            Register(CommandKey.LevelOpen, "&Open");
            Register(CommandKey.LevelClose, "&Close",
                shortcut: Keys.Control | Keys.F4);
            Register(CommandKey.LevelCloseAllOther, "Close &All But This");
            Register(CommandKey.LevelClone, "D&uplicate",
                resource: Properties.Resources.Maps);
            Register(CommandKey.LevelDelete, "&Delete",
                resource: Properties.Resources.Cross);
            Register(CommandKey.LevelRename, "Re&name");
            Register(CommandKey.LevelResize, "&Resize...",
                resource: Properties.Resources.ArrowResize135, shortcut: Keys.Control | Keys.R);
            Register(CommandKey.LevelProperties, "&Properties",
                resource: Properties.Resources.Tags);

            Register(CommandKey.NewTileLayer, "New &Tile Layer",
                resource: Properties.Resources.Grid);
            Register(CommandKey.NewObjectLayer, "New &Object Layer",
                resource: Properties.Resources.Game);
            Register(CommandKey.LayerClone, "D&uplicate",
                resource: Properties.Resources.Layers);
            Register(CommandKey.LayerDelete, "&Delete",
                resource: Properties.Resources.LayerMinus);
            Register(CommandKey.LayerProperties, "&Properties",
                resource: Properties.Resources.Tags);
            Register(CommandKey.LayerMoveTop, "Bring to &Top",
                resource: Properties.Resources.ArrowSkip90);
            Register(CommandKey.LayerMoveUp, "Move &Up",
                resource: Properties.Resources.Arrow90);
            Register(CommandKey.LayerMoveDown, "Move &Down",
                resource: Properties.Resources.Arrow270);
            Register(CommandKey.LayerMoveBottom, "Send to &Bottom",
                resource: Properties.Resources.ArrowSkip270);
            Register(CommandKey.LayerShowCurrentOnly, "Show &Current Layer Only");
            Register(CommandKey.LayerShowAll, "&All");
            Register(CommandKey.LayerShowNone, "&None");
            Register(CommandKey.LayerExportRaster, "E&xport Raster Image");

            Register(CommandKey.NewStaticTileBrush, "New &Static Brush...",
                resource: Properties.Resources.Stamp);
            Register(CommandKey.NewDynamicTileBrush, "New D&ynamic Brush...",
                resource: Properties.Resources.TableDynamic);
            Register(CommandKey.TileBrushClone, "D&uplicate");
            Register(CommandKey.TileBrushDelete, "&Delete",
                resource: Properties.Resources.PaintBrushMinus);
            Register(CommandKey.TileSelectionCreateBrush, "Create &Brush from Selection",
                resource: Properties.Resources.PaintBrushPlus);
            Register(CommandKey.TileSelectionPromoteLayer, "Promote to &Layer",
                resource: Properties.Resources.LayerSelect);
            Register(CommandKey.TileSelectionFloat, "&Float");
            Register(CommandKey.TileSelectionDefloat, "&Defloat");
            Register(CommandKey.TilePoolDelete, "&Delete",
                resource: Properties.Resources.Cross);
            Register(CommandKey.TilePoolProperties, "&Properties",
                resource: Properties.Resources.Tags);
            Register(CommandKey.TilePoolExport, "&Export Raw Tileset...",
                resource: Properties.Resources.DriveDownload);
            Register(CommandKey.TilePoolImportOver, "&Import Raw Tileset...",
                resource: Properties.Resources.DriveUpload);

            Register(CommandKey.ObjectProtoImport, "&Import Object from Image...",
                resource: Properties.Resources.GamePlus);
            Register(CommandKey.ObjectProtoEdit, "&Edit...");
            Register(CommandKey.ObjectProtoClone, "D&uplicate");
            Register(CommandKey.ObjectProtoDelete, "&Delete",
                resource: Properties.Resources.GameMinus);
            Register(CommandKey.ObjectProtoRename, "&Rename");
            Register(CommandKey.ObjectProtoProperties, "&Properties",
                resource: Properties.Resources.Tags);
            Register(CommandKey.ObjectMoveTop, "Bring to &Front",
                resource: Properties.Resources.ArrowSkip90);
            Register(CommandKey.ObjectMoveUp, "Move &Forward",
                resource: Properties.Resources.Arrow90);
            Register(CommandKey.ObjectMoveDown, "Move &Backward",
                resource: Properties.Resources.Arrow270);
            Register(CommandKey.ObjectMoveBottom, "Send to &Back",
                resource: Properties.Resources.ArrowSkip270);
            Register(CommandKey.ObjectReferenceImage, "&Image Bounds",
                resource: Properties.Resources.SnapBorders);
            Register(CommandKey.ObjectReferenceMask, "&Mask Bounds",
                resource: Properties.Resources.SnapBounds);
            Register(CommandKey.ObjectReferenceOrigin, "&Origin",
                resource: Properties.Resources.SnapOrigin);
            Register(CommandKey.ObjectSnappingNone, "&None",
                resource: Properties.Resources.SnapNone);
            Register(CommandKey.ObjectSnappingTopLeft, "To&p Left",
                resource: Properties.Resources.SnapTopLeft);
            Register(CommandKey.ObjectSnappingTopRight, "Top R&ight",
                resource: Properties.Resources.SnapTopRight);
            Register(CommandKey.ObjectSnappingBottomLeft, "Botto&m Left",
                resource: Properties.Resources.SnapBottomLeft);
            Register(CommandKey.ObjectSnappingBottomRight, "Bottom Ri&ght",
                resource: Properties.Resources.SnapBottomRight);
            Register(CommandKey.ObjectSnappingTop, "&Top",
                resource: Properties.Resources.SnapTop);
            Register(CommandKey.ObjectSnappingBottom, "&Bottom",
                resource: Properties.Resources.SnapBottom);
            Register(CommandKey.ObjectSnappingLeft, "&Left",
                resource: Properties.Resources.SnapLeft);
            Register(CommandKey.ObjectSnappingRight, "&Right",
                resource: Properties.Resources.SnapRight);
            Register(CommandKey.ObjectSnappingVert, "Center &Vertical",
                resource: Properties.Resources.SnapHorizontal);
            Register(CommandKey.ObjectSnappingHorz, "Center &Horizontal",
                resource: Properties.Resources.SnapVertical);
            Register(CommandKey.ObjectSnappingCenter, "&Center",
                resource: Properties.Resources.SnapCenter);
            Register(CommandKey.ObjectProperties, "&Properties",
                resource: Properties.Resources.Tags);
        }

        private static void Register (CommandKey key, string displayName, Image resource = null, Keys shortcut = Keys.None, string description = null, string shortcutDisplay = null)
        {
            CommandRecord record = new CommandRecord(key) {
                DisplayName = displayName,
                Description = description,
                Shortcut = shortcut,
                ShortcutDisplay = shortcutDisplay,
            };

            if (resource != null)
                record.Image = resource;

            _default.Register(record);
        }
    }
}
