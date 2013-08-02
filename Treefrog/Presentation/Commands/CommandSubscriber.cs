using System;
using System.Collections.Generic;
using Treefrog.Utility;

namespace Treefrog.Presentation.Commands
{
    /*public class CommandCollectionEntry
    {
        public CommandKey Key { get; }
        public CommandCollection SubCollection { get; }
    }*/

    public class CommandKey : Symbol
    {
        public CommandKey (Symbol prototype)
            : base(prototype)
        { }

        public static SymbolPool<CommandKey> Registry = new SymbolPool<CommandKey>();

        public static readonly CommandKey Unknown = Registry.GenerateSymbol("Unknown");

        public static readonly CommandKey NewProject = Registry.GenerateSymbol("NewProject");
        public static readonly CommandKey OpenProject = Registry.GenerateSymbol("OpenProject");
        public static readonly CommandKey Save = Registry.GenerateSymbol("Save");
        public static readonly CommandKey SaveAs = Registry.GenerateSymbol("SaveAs");
        public static readonly CommandKey Exit = Registry.GenerateSymbol("Exit");

        public static readonly CommandKey Undo = Registry.GenerateSymbol("Undo");
        public static readonly CommandKey Redo = Registry.GenerateSymbol("Redo");
        public static readonly CommandKey Cut = Registry.GenerateSymbol("Cut");
        public static readonly CommandKey Copy = Registry.GenerateSymbol("Copy");
        public static readonly CommandKey Paste = Registry.GenerateSymbol("Paste");
        public static readonly CommandKey Delete = Registry.GenerateSymbol("Delete");
        public static readonly CommandKey SelectAll = Registry.GenerateSymbol("SelectAll");
        public static readonly CommandKey SelectNone = Registry.GenerateSymbol("SelectNone");

        public static readonly CommandKey ViewZoomNormal = Registry.GenerateSymbol("ViewZoomNormal");
        public static readonly CommandKey ViewZoomIn = Registry.GenerateSymbol("ViewZoomIn");
        public static readonly CommandKey ViewZoomOut = Registry.GenerateSymbol("ViewZoomOut");
        public static readonly CommandKey ViewGrid = Registry.GenerateSymbol("ViewGrid");

        public static readonly CommandKey TileToolSelect = Registry.GenerateSymbol("TileToolSelect");
        public static readonly CommandKey TileToolDraw = Registry.GenerateSymbol("TileToolDraw");
        public static readonly CommandKey TileToolErase = Registry.GenerateSymbol("TileToolErase");
        public static readonly CommandKey TileToolFill = Registry.GenerateSymbol("TileToolFill");

        public static readonly CommandKey ProjectAddLevel = Registry.GenerateSymbol("ProjectAddLevel");
        public static readonly CommandKey ProjectAddNewLibrary = Registry.GenerateSymbol("ProjectAddNewLibrary");
        public static readonly CommandKey ProjectAddExistingLibrary = Registry.GenerateSymbol("ProjectAddExistingLibrary");
        public static readonly CommandKey ProjectRemoveLibrary = Registry.GenerateSymbol("ProjectRemoveLibrary");
        public static readonly CommandKey ProjectSetLibraryDefault = Registry.GenerateSymbol("ProjectSetLibraryDefault");

        public static readonly CommandKey LevelOpen = Registry.GenerateSymbol("LevelOpen");
        public static readonly CommandKey LevelClose = Registry.GenerateSymbol("LevelClose");
        public static readonly CommandKey LevelCloseAllOther = Registry.GenerateSymbol("LevelCloseAllOther");
        public static readonly CommandKey LevelClone = Registry.GenerateSymbol("LevelClone");
        public static readonly CommandKey LevelDelete = Registry.GenerateSymbol("LevelDelete");
        public static readonly CommandKey LevelRename = Registry.GenerateSymbol("LevelRename");
        public static readonly CommandKey LevelResize = Registry.GenerateSymbol("LevelResize");
        public static readonly CommandKey LevelProperties = Registry.GenerateSymbol("LevelProperties");

        public static readonly CommandKey NewTileLayer = Registry.GenerateSymbol("NewTileLayer");
        public static readonly CommandKey NewObjectLayer = Registry.GenerateSymbol("NewObjectLayer");
        public static readonly CommandKey LayerEdit = Registry.GenerateSymbol("LayerEdit");
        public static readonly CommandKey LayerClone = Registry.GenerateSymbol("LayerClone");
        public static readonly CommandKey LayerDelete = Registry.GenerateSymbol("LayerDelete");
        public static readonly CommandKey LayerProperties = Registry.GenerateSymbol("LayerProperties");
        public static readonly CommandKey LayerMoveTop = Registry.GenerateSymbol("LayerMoveTop");
        public static readonly CommandKey LayerMoveUp = Registry.GenerateSymbol("LayerMoveUp");
        public static readonly CommandKey LayerMoveDown = Registry.GenerateSymbol("LayerMoveDown");
        public static readonly CommandKey LayerMoveBottom = Registry.GenerateSymbol("LayerMoveBottom");
        public static readonly CommandKey LayerShowCurrentOnly = Registry.GenerateSymbol("LayerShowCurrentOnly");
        public static readonly CommandKey LayerShowAll = Registry.GenerateSymbol("LayerShowAll");
        public static readonly CommandKey LayerShowNone = Registry.GenerateSymbol("LayerShowNone");
        public static readonly CommandKey LayerExportRaster = Registry.GenerateSymbol("LayerExportRaster");

        public static readonly CommandKey TileProperties = Registry.GenerateSymbol("TileProperties");
        public static readonly CommandKey TileDelete = Registry.GenerateSymbol("TileDelete");

        public static readonly CommandKey TilePoolExport = Registry.GenerateSymbol("TilePoolExport");
        public static readonly CommandKey TilePoolImportOver = Registry.GenerateSymbol("TilePoolImportOver");
        public static readonly CommandKey TilePoolImport = Registry.GenerateSymbol("TilePoolImport");
        public static readonly CommandKey TilePoolImportMerge = Registry.GenerateSymbol("TilePoolImportMerge");
        public static readonly CommandKey TilePoolDelete = Registry.GenerateSymbol("TilePoolDelete");
        public static readonly CommandKey TilePoolRename = Registry.GenerateSymbol("TilePoolRename");
        public static readonly CommandKey TilePoolProperties = Registry.GenerateSymbol("TilePoolProperties");

        public static readonly CommandKey NewStaticTileBrush = Registry.GenerateSymbol("NewStaticTileBrush");
        public static readonly CommandKey NewDynamicTileBrush = Registry.GenerateSymbol("NewDynamicTileBrush");
        public static readonly CommandKey TileBrushClone = Registry.GenerateSymbol("TileBrushClone");
        public static readonly CommandKey TileBrushDelete = Registry.GenerateSymbol("TileBrushDelete");
        public static readonly CommandKey TileBrushFilter = Registry.GenerateSymbol("TileBrushFilter");

        public static readonly CommandKey TileSelectionCreateBrush = Registry.GenerateSymbol("TileSelectionCreateBrush");
        public static readonly CommandKey TileSelectionPromoteLayer = Registry.GenerateSymbol("TileSelectionPromoteLayer");
        public static readonly CommandKey TileSelectionFloat = Registry.GenerateSymbol("TileSelectionFloat");
        public static readonly CommandKey TileSelectionDefloat = Registry.GenerateSymbol("TileSelectionDefloat");

        public static readonly CommandKey ObjectProtoImport = Registry.GenerateSymbol("ObjectProtoImport");
        public static readonly CommandKey ObjectProtoEdit = Registry.GenerateSymbol("ObjectProtoEdit");
        public static readonly CommandKey ObjectProtoDelete = Registry.GenerateSymbol("ObjectProtoDelete");
        public static readonly CommandKey ObjectProtoRename = Registry.GenerateSymbol("ObjectProtoRename");
        public static readonly CommandKey ObjectProtoClone = Registry.GenerateSymbol("ObjectProtoClone");
        public static readonly CommandKey ObjectProtoProperties = Registry.GenerateSymbol("ObjectProtoProperties");
        public static readonly CommandKey ObjectMoveTop = Registry.GenerateSymbol("ObjectMoveTop");
        public static readonly CommandKey ObjectMoveUp = Registry.GenerateSymbol("ObjectMoveUp");
        public static readonly CommandKey ObjectMoveDown = Registry.GenerateSymbol("ObjectMoveDown");
        public static readonly CommandKey ObjectMoveBottom = Registry.GenerateSymbol("ObjectMoveBottom");
        public static readonly CommandKey ObjectReferenceImage = Registry.GenerateSymbol("ObjectReferenceImage");
        public static readonly CommandKey ObjectReferenceMask = Registry.GenerateSymbol("ObjectReferenceMask");
        public static readonly CommandKey ObjectReferenceOrigin = Registry.GenerateSymbol("ObjectReferenceOrigin");
        public static readonly CommandKey ObjectSnappingNone = Registry.GenerateSymbol("ObjectSnappingNone");
        public static readonly CommandKey ObjectSnappingTopLeft = Registry.GenerateSymbol("ObjectSnappingTopLeft");
        public static readonly CommandKey ObjectSnappingTopRight = Registry.GenerateSymbol("ObjectSnappingTopRight");
        public static readonly CommandKey ObjectSnappingBottomLeft = Registry.GenerateSymbol("ObjectSnappingBottomLeft");
        public static readonly CommandKey ObjectSnappingBottomRight = Registry.GenerateSymbol("ObjectSnappingBottomRight");
        public static readonly CommandKey ObjectSnappingTop = Registry.GenerateSymbol("ObjectSnappingTop");
        public static readonly CommandKey ObjectSnappingBottom = Registry.GenerateSymbol("ObjectSnappingBottom");
        public static readonly CommandKey ObjectSnappingLeft = Registry.GenerateSymbol("ObjectSnappingLeft");
        public static readonly CommandKey ObjectSnappingRight = Registry.GenerateSymbol("ObjectSnappingRight");
        public static readonly CommandKey ObjectSnappingVert = Registry.GenerateSymbol("ObjectSnappingVert");
        public static readonly CommandKey ObjectSnappingHorz = Registry.GenerateSymbol("ObjectSnappingHorz");
        public static readonly CommandKey ObjectSnappingCenter = Registry.GenerateSymbol("ObjectSnappingCenter");

        public static readonly CommandKey ObjectProperties = Registry.GenerateSymbol("ObjectProperties");
    }

    public enum CommandToggleGroup
    {
        Unknown,
        TileTool,
        ObjectReference,
        ObjectSnapping,
    }

    public class CommandSubscriberEventArgs : EventArgs
    {
        public CommandKey CommandKey { get; private set; }

        public CommandSubscriberEventArgs (CommandKey key)
        {
            CommandKey = key;
        }
    }

    public interface ICommandSubscriber
    {
        CommandManager CommandManager { get; }
    }



    public class CommandManager
    {
        private class CommandHandler
        {
            public Func<object, bool> CanPerform { get; set; }
            public Action<object> Perform { get; set; }
            public CommandToggleGroup Group { get; set; }

            public CommandHandler () 
            {
                Group = CommandToggleGroup.Unknown;
            }
        }

        private class StatefulCommandHandler : CommandHandler
        {
            public bool Selected { get; set; }

            public StatefulCommandHandler ()
                : base()
            {
                Selected = false;
            }
        }

        private class CommandGroup
        {
            public CommandKey Selected { get; set; }
            public List<CommandKey> Members { get; set; }
            public CommandHandler Handler { get; set; }

            public CommandGroup ()
            {
                Members = new List<CommandKey>();
            }
        }

        private static Func<object, bool> DefaultGroupCheck = delegate { return true; };

        private static Func<object, bool> DefaultStatefulCheck = delegate { return true; };

        private Dictionary<CommandKey, CommandHandler> _handlers = new Dictionary<CommandKey, CommandHandler>();
        private Dictionary<CommandToggleGroup, CommandGroup> _toggleGroups = new Dictionary<CommandToggleGroup, CommandGroup>();

        public event EventHandler<CommandSubscriberEventArgs> CommandInvalidated;
        public event EventHandler ManagerInvalidated;

        protected virtual void OnCommandInvalidated (CommandSubscriberEventArgs e)
        {
            var ev = CommandInvalidated;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnManagerInvalidated (EventArgs e)
        {
            var ev = ManagerInvalidated;
            if (ev != null)
                ev(this, e);
        }

        public void RegisterToggleGroup (CommandToggleGroup group)
        {
            RegisterToggleGroup(group, null as Func<object, bool>, null as Action<object>);
        }

        public void RegisterToggleGroup (CommandToggleGroup group, Func<bool> performCheck, Action perform)
        {
            RegisterToggleGroup(group, (obj) => { return performCheck(); }, (obj) => { perform(); });
        }

        public void RegisterToggleGroup (CommandToggleGroup group, Func<object, bool> performCheck, Action<object> perform)
        {
            _toggleGroups[group] = new CommandGroup() {
                Selected = CommandKey.Unknown,
                Handler = new CommandHandler() {
                    CanPerform = performCheck ?? DefaultGroupCheck,
                    Perform = perform,
                },
            };
        }

        public void Register (CommandKey key, Func<bool> performCheck, Action perform)
        {
            Register(key, (obj) => { return performCheck(); }, (obj) => { perform(); });
        }

        public void Register (CommandKey key, Func<object, bool> performCheck, Action<object> perform)
        {
            _handlers[key] = new CommandHandler() {
                CanPerform = performCheck,
                Perform = perform,
            };
        }

        public void RegisterToggle (CommandKey key)
        {
            RegisterToggle(key, null as Func<object, bool>, null as Action<object>);
        }

        public void RegisterToggle (CommandKey key, Func<bool> performCheck, Action perform)
        {
            RegisterToggle(key, (obj) => { return performCheck(); }, (obj) => { perform(); });
        }

        public void RegisterToggle (CommandKey key, Func<object, bool> performCheck, Action<object> perform)
        {
            _handlers[key] = new StatefulCommandHandler() {
                CanPerform = performCheck ?? DefaultStatefulCheck,
                Perform = perform,
            };
        }

        public void RegisterToggle (CommandToggleGroup group, CommandKey key)
        {
            RegisterToggle(group, key, null as Func<object, bool>, null as Action<object>);
        }

        public void RegisterToggle (CommandToggleGroup group, CommandKey key, Func<bool> performCheck, Action perform)
        {
            RegisterToggle(group, key, (obj) => { return performCheck(); }, (obj) => { perform(); });
        }

        public void RegisterToggle (CommandToggleGroup group, CommandKey key, Func<object, bool> performCheck, Action<object> perform)
        {
            if (!_toggleGroups[group].Members.Contains(key))
                _toggleGroups[group].Members.Add(key);

            Register(key, performCheck, perform);

            _handlers[key].Group = group;
        }

        public void Unregister (CommandKey key)
        {
            _handlers.Remove(key);
        }

        public void Invalidate (CommandKey key)
        {
            OnCommandInvalidated(new CommandSubscriberEventArgs(key));
        }

        public void InvalidateManager ()
        {
            OnManagerInvalidated(EventArgs.Empty);
        }

        public void InvalidateGroup (CommandToggleGroup group)
        {
            if (!_toggleGroups.ContainsKey(group))
                return;

            foreach (CommandKey key in _toggleGroups[group].Members)
                Invalidate(key);
        }

        public virtual bool CanHandle (CommandKey key)
        {
            return _handlers.ContainsKey(key);
        }

        protected bool CanHandleGroup (CommandToggleGroup group)
        {
            return _toggleGroups.ContainsKey(group);
        }

        protected bool CanHandleGroup (CommandKey key)
        {
            if (!CanHandle(key))
                return false;

            return CanHandleGroup(_handlers[key].Group);
        }

        public bool CanPerform (CommandKey key)
        {
            return CanPerform(key, null);
        }

        public virtual bool CanPerform (CommandKey key, object param)
        {
            if (!CanHandle(key))
                return false;

            if (GroupCanPerform(key, param))
                return true;
            
            if (_handlers[key].CanPerform != null && _handlers[key].CanPerform(param))
                return true;

            return false;
        }

        protected virtual bool GroupCanPerform (CommandKey key, object param)
        {
            CommandHandler handler;
            if (!_handlers.TryGetValue(key, out handler))
                return false;

            CommandGroup group;
            if (!_toggleGroups.TryGetValue(handler.Group, out group))
                return false;

            if (group.Handler.CanPerform == null)
                return false;

            return group.Handler.CanPerform(param);
        }

        public void Perform (CommandKey key)
        {
            Perform(key, null);
        }

        public virtual void Perform (CommandKey key, object param)
        {
            if (CanHandleGroup(key))
                GroupPerform(key, param);
            else if (CanHandle(key)) {
                if (_handlers[key] is StatefulCommandHandler)
                    StatefulPerform(key, param);
                else if (_handlers[key].Perform != null)
                    _handlers[key].Perform(param);
            }
        }

        protected virtual void StatefulPerform (CommandKey key, object param)
        {
            CommandHandler handler;
            if (!_handlers.TryGetValue(key, out handler))
                return;

            StatefulCommandHandler stateHandler = handler as StatefulCommandHandler;
            if (stateHandler == null)
                return;

            stateHandler.Selected = !stateHandler.Selected;

            if (stateHandler.Perform != null)
                stateHandler.Perform(param);

            Invalidate(key);
        }

        protected virtual void GroupPerform (CommandKey key, object param)
        {
            CommandHandler handler;
            if (!_handlers.TryGetValue(key, out handler))
                return;

            CommandGroup group;
            if (!_toggleGroups.TryGetValue(handler.Group, out group))
                return;

            if (group.Selected == key)
                return;

            group.Selected = key;

            if (group.Handler.Perform != null)
                group.Handler.Perform(param);

            if (handler.Perform != null)
                handler.Perform(param);

            InvalidateGroup(handler.Group);
        }

        public virtual CommandKey SelectedCommand (CommandToggleGroup group)
        {
            if (_toggleGroups.ContainsKey(group))
                return _toggleGroups[group].Selected;

            return CommandKey.Unknown;
        }

        public virtual bool IsSelected (CommandKey key)
        {
            CommandHandler handler;
            if (!_handlers.TryGetValue(key, out handler))
                return false;

            StatefulCommandHandler stateHandler = handler as StatefulCommandHandler;
            if (stateHandler != null)
                return stateHandler.Selected;

            CommandGroup group;
            if (!_toggleGroups.TryGetValue(handler.Group, out group))
                return false;

            return group.Selected == key;
        }
    }

    public class ForwardingCommandManager : CommandManager
    {
        private List<ICommandSubscriber> _forwardManagers = new List<ICommandSubscriber>();

        public void AddCommandSubscriber (ICommandSubscriber manager)
        {
            if (manager == null)
                return;

            if (!_forwardManagers.Contains(manager)) {
                manager.CommandManager.CommandInvalidated += HandleCommandInvalidated;
                manager.CommandManager.ManagerInvalidated += HandleManagerInvalidated;

                _forwardManagers.Add(manager);
                InvalidateManager();
            }
        }

        public void RemoveCommandSubscriber (ICommandSubscriber manager)
        {
            if (_forwardManagers.Remove(manager)) {
                manager.CommandManager.CommandInvalidated -= HandleCommandInvalidated;
                manager.CommandManager.ManagerInvalidated -= HandleManagerInvalidated;

                InvalidateManager();
            }
        }

        public bool IsMulticast { get; set; }

        private void HandleManagerInvalidated (object sender, EventArgs e)
        {
            InvalidateManager();
        }

        private void HandleCommandInvalidated (object sender, CommandSubscriberEventArgs e)
        {
            Invalidate(e.CommandKey);
        }

        public override bool CanHandle (CommandKey key)
        {
            if (base.CanHandle(key))
                return true;

            foreach (ICommandSubscriber subscriber in _forwardManagers) {
                if (subscriber == null || subscriber.CommandManager == null)
                    continue;
                if (subscriber.CommandManager.CanHandle(key))
                    return true;
            }

            return false;
        }

        public override bool CanPerform (CommandKey key, object param)
        {
            if (base.CanHandle(key))
                return base.CanPerform(key, param);

            foreach (ICommandSubscriber subscriber in _forwardManagers) {
                if (subscriber == null || subscriber.CommandManager == null)
                    continue;
                if (subscriber.CommandManager.CanPerform(key, param))
                    return true;
            }

            return false;
        }

        public override void Perform (CommandKey key, object param)
        {
            if (base.CanHandle(key)) {
                base.Perform(key, param);
                return;
            }

            foreach (ICommandSubscriber subscriber in _forwardManagers) {
                if (subscriber == null || subscriber.CommandManager == null)
                    continue;
                if (!subscriber.CommandManager.CanPerform(key, param))
                    continue;

                subscriber.CommandManager.Perform(key, param);
                if (!IsMulticast)
                    break;
            }
        }

        public override CommandKey SelectedCommand (CommandToggleGroup group)
        {
            if (base.CanHandleGroup(group))
                return base.SelectedCommand(group);

            foreach (ICommandSubscriber subscriber in _forwardManagers) {
                if (subscriber == null || subscriber.CommandManager == null)
                    continue;
                CommandKey selected = subscriber.CommandManager.SelectedCommand(group);
                if (selected != CommandKey.Unknown)
                    return selected;
            }

            return CommandKey.Unknown;
        }

        public override bool IsSelected (CommandKey key)
        {
            if (base.CanHandle(key))
                return base.IsSelected(key);

            foreach (ICommandSubscriber subscriber in _forwardManagers) {
                if (subscriber == null || subscriber.CommandManager == null)
                    continue;
                if (subscriber.CommandManager.IsSelected(key))
                    return true;
            }

            return false;
        }
    }
}
