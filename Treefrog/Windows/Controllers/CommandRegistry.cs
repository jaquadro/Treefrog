using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Treefrog.Presentation.Commands;
using System.Windows.Forms;
using System.Drawing;

namespace Treefrog.Windows.Controllers
{
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
                                Tag = entry.Key,
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

        static Assembly _assembly;
        static CommandRegistry _default;

        static CommandRegistry ()
        {
            _default = new CommandRegistry();

            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Register(CommandKey.NewProject, "&New Project...", 
                resource: "Treefrog.Icons._16.applications-blue--asterisk.png", shortcut: Keys.Control | Keys.N);
            Register(CommandKey.OpenProject, "&Open Project...", 
                resource: "Treefrog.Icons.folder-horizontal-open16.png", shortcut: Keys.Control | Keys.O);
            Register(CommandKey.Save, "&Save Project", 
                resource: "Treefrog.Icons._16.disk.png", shortcut: Keys.Control | Keys.S);
            Register(CommandKey.SaveAs, "Save Project &As...", 
                resource: "Treefrog.Icons._16.disk--pencil.png", shortcut: Keys.Control | Keys.Shift | Keys.S);
            Register(CommandKey.Exit, "E&xit", 
                shortcut: Keys.Alt | Keys.F4);

            Register(CommandKey.Undo, "&Undo",
                resource: "Treefrog.Icons._16.arrow-turn-180-left.png", shortcut: Keys.Control | Keys.Z);
            Register(CommandKey.Redo, "&Redo",
                resource: "Treefrog.Icons._16.arrow-turn.png", shortcut: Keys.Control | Keys.Y);
            Register(CommandKey.Cut, "Cu&t",
                resource: "Treefrog.Icons._16.scissors.png", shortcut: Keys.Control | Keys.X);
            Register(CommandKey.Copy, "&Copy",
                resource: "Treefrog.Icons._16.documents.png", shortcut: Keys.Control | Keys.C);
            Register(CommandKey.Paste, "&Paste",
                resource: "Treefrog.Icons._16.clipboard-paste.png", shortcut: Keys.Control | Keys.V);
            Register(CommandKey.Delete, "&Delete",
                resource: "Treefrog.Icons._16.cross.png", shortcut: Keys.Delete);
            Register(CommandKey.SelectAll, "Select &All",
                resource: "Treefrog.Icons._16.selection-select.png", shortcut: Keys.Control | Keys.A);
            Register(CommandKey.SelectNone, "Select &None",
                resource: "Treefrog.Icons._16.selection.png", shortcut: Keys.Control | Keys.Shift | Keys.A);

            Register(CommandKey.ViewZoomNormal, "Zoom &1:1", "Treefrog.Icons._16.magnifier-zoom-actual.png");
            Register(CommandKey.ViewZoomIn, "Zoom &In",
                resource: "Treefrog.Icons._16.magnifier-zoom-in.png", shortcut: Keys.Control | Keys.Add, shortcutDisplay: "Ctrl+Plus");
            Register(CommandKey.ViewZoomOut, "Zoom &Out",
                resource: "Treefrog.Icons._16.magnifier-zoom-out.png", shortcut: Keys.Control | Keys.Subtract, shortcutDisplay: "Ctrl+Minus");
            Register(CommandKey.ViewGrid, "Show &Grid", 
                resource: "Treefrog.Icons._16.grid.png", shortcut: Keys.Control | Keys.G);

            Register(CommandKey.ProjectAddLevel, "New &Level...", 
                resource: "Treefrog.Icons._16.map--asterisk.png");

            Register(CommandKey.LevelClose, "&Close",
                shortcut: Keys.Control | Keys.F4);
            Register(CommandKey.LevelCloseAllOther, "Close &All But This");
            Register(CommandKey.LevelRename, "Re&name");
            Register(CommandKey.LevelResize, "&Resize...", 
                resource: "Treefrog.Icons._16.arrow-resize-135.png", shortcut: Keys.Control | Keys.R);
            Register(CommandKey.LevelProperties, "&Properties",
                resource: "Treefrog.Icons._16.tags.png");

            Register(CommandKey.NewTileLayer, "New &Tile Layer",
                resource: "Treefrog.Icons._16.grid.png");
            Register(CommandKey.NewObjectLayer, "New &Object Layer",
                resource: "Treefrog.Icons._16.game.png");
            Register(CommandKey.LayerClone, "D&uplicate",
                resource: "Treefrog.Icons._16.layers.png");
            Register(CommandKey.LayerDelete, "&Delete",
                resource: "Treefrog.Icons._16.layer--minus.png");
            Register(CommandKey.LayerProperties, "&Properties",
                resource: "Treefrog.Icons._16.tags.png");
            Register(CommandKey.LayerMoveTop, "Bring to &Top",
                resource: "Treefrog.Icons._16.arrow-skip-090.png");
            Register(CommandKey.LayerMoveUp, "Move &Up",
                resource: "Treefrog.Icons._16.arrow-090.png");
            Register(CommandKey.LayerMoveDown, "Move &Down",
                resource: "Treefrog.Icons._16.arrow-270.png");
            Register(CommandKey.LayerMoveBottom, "Send to &Bottom",
                resource: "Treefrog.Icons._16.arrow-skip-270.png");
            Register(CommandKey.LayerShowCurrentOnly, "Show &Current Layer Only");
            Register(CommandKey.LayerShowAll, "&All");
            Register(CommandKey.LayerShowNone, "&None");
            Register(CommandKey.LayerExportRaster, "E&xport Raster Image");

            Register(CommandKey.NewStaticTileBrush, "New &Static Brush...",
                resource: "Treefrog.Icons._16.stamp.png");
            Register(CommandKey.NewDynamicTileBrush, "New D&ynamic Brush...",
                resource: "Treefrog.Icons._16.table-dynamic.png");
            Register(CommandKey.TileBrushClone, "D&uplicate");
            Register(CommandKey.TileBrushDelete, "&Delete",
                resource: "Treefrog.Icons._16.paint-brush--minus.png");
            Register(CommandKey.TileSelectionCreateBrush, "Create &Brush from Selection",
                resource: "Treefrog.Icons._16.paint-brush--plus.png");
            Register(CommandKey.TileSelectionPromoteLayer, "Promote to &Layer",
                resource: "Treefrog.Icons._16.layer-select.png");
            Register(CommandKey.TileSelectionFloat, "&Float");
            Register(CommandKey.TileSelectionDefloat, "&Defloat");
            Register(CommandKey.TilePoolExport, "&Export Raw Tileset...",
                resource: "Treefrog.Icons._16.drive-download.png");
            Register(CommandKey.TilePoolImportOver, "&Import Raw Tileset...",
                resource: "Treefrog.Icons._16.drive-upload.png");

            Register(CommandKey.ObjectProtoImport, "Import Object from Image...", 
                resource: "Treefrog.Icons._16.game--plus.png");
            Register(CommandKey.ObjectProtoClone, "D&uplicate");
            Register(CommandKey.ObjectProtoDelete, "&Delete", 
                resource: "Treefrog.Icons._16.game--minus.png");
            Register(CommandKey.ObjectProtoProperties, "&Properties", 
                resource: "Treefrog.Icons._16.tags.png");
            Register(CommandKey.ObjectMoveTop, "Bring to &Front", 
                resource: "Treefrog.Icons._16.arrow-skip-090.png");
            Register(CommandKey.ObjectMoveUp, "Move &Forward", 
                resource: "Treefrog.Icons._16.arrow-090.png");
            Register(CommandKey.ObjectMoveDown, "Move &Backward", 
                resource: "Treefrog.Icons._16.arrow-270.png");
            Register(CommandKey.ObjectMoveBottom, "Send to &Back", 
                resource: "Treefrog.Icons._16.arrow-skip-270.png");
            Register(CommandKey.ObjectReferenceImage, "&Image Bounds", 
                resource: "Treefrog.Icons._16.snap-borders.png");
            Register(CommandKey.ObjectReferenceMask, "&Mask Bounds", 
                resource: "Treefrog.Icons._16.snap-bounds.png");
            Register(CommandKey.ObjectReferenceOrigin, "&Origin", 
                resource: "Treefrog.Icons._16.snap-origin.png");
            Register(CommandKey.ObjectSnappingNone, "&None", 
                resource: "Treefrog.Icons._16.snap-none.png");
            Register(CommandKey.ObjectSnappingTopLeft, "To&p Left", 
                resource: "Treefrog.Icons._16.snap-topleft.png");
            Register(CommandKey.ObjectSnappingTopRight, "Top R&ight", 
                resource: "Treefrog.Icons._16.snap-topright.png");
            Register(CommandKey.ObjectSnappingBottomLeft, "Botto&m Left", 
                resource: "Treefrog.Icons._16.snap-bottomleft.png");
            Register(CommandKey.ObjectSnappingBottomRight, "Bottom Ri&ght", 
                resource: "Treefrog.Icons._16.snap-bottomright.png");
            Register(CommandKey.ObjectSnappingTop, "&Top", 
                resource: "Treefrog.Icons._16.snap-top.png");
            Register(CommandKey.ObjectSnappingBottom, "&Bottom", 
                resource: "Treefrog.Icons._16.snap-bottom.png");
            Register(CommandKey.ObjectSnappingLeft, "&Left", 
                resource: "Treefrog.Icons._16.snap-left.png");
            Register(CommandKey.ObjectSnappingRight, "&Right", 
                resource: "Treefrog.Icons._16.snap-right.png");
            Register(CommandKey.ObjectSnappingVert, "Center &Vertical", 
                resource: "Treefrog.Icons._16.snap-horizontal.png");
            Register(CommandKey.ObjectSnappingHorz, "Center &Horizontal", 
                resource: "Treefrog.Icons._16.snap-vertical.png");
            Register(CommandKey.ObjectSnappingCenter, "&Center", 
                resource: "Treefrog.Icons._16.snap-center.png");
            Register(CommandKey.ObjectProperties, "&Properties",
                resource: "Treefrog.Icons._16.tags.png");
        }

        private static void Register (CommandKey key, string displayName, string resource = null, Keys shortcut = Keys.None, string description = null, string shortcutDisplay = null)
        {
            CommandRecord record = new CommandRecord(key) {
                DisplayName = displayName,
                Description = description,
                Shortcut = shortcut,
                ShortcutDisplay = shortcutDisplay,
            };

            if (resource != null)
                record.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            _default.Register(record);
        }
    }
}
