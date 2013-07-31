using System;
using System.Collections.Generic;

namespace Treefrog.Presentation.Commands
{
    /*public class CommandCollectionEntry
    {
        public CommandKey Key { get; }
        public CommandCollection SubCollection { get; }
    }*/

    public enum CommandKey
    {
        Unknown,

        NewProject,
        OpenProject,
        Save,
        SaveAs,
        Exit,

        Undo,
        Redo,
        Cut,
        Copy,
        Paste,
        Delete,
        SelectAll,
        SelectNone,

        ViewZoomNormal,
        ViewZoomIn,
        ViewZoomOut,
        ViewGrid,

        TileToolSelect,
        TileToolDraw,
        TileToolErase,
        TileToolFill,

        ProjectAddLevel,
        ProjectAddNewLibrary,
        ProjectAddExistingLibrary,
        ProjectRemoveLibrary,
        ProjectSetLibraryDefault,

        LevelOpen,
        LevelClose,
        LevelCloseAllOther,
        LevelClone,
        LevelDelete,
        LevelRename,
        LevelResize,
        LevelProperties,

        NewTileLayer,
        NewObjectLayer,
        LayerEdit,
        LayerClone,
        LayerDelete,
        LayerProperties,
        LayerMoveTop,
        LayerMoveUp,
        LayerMoveDown,
        LayerMoveBottom,
        LayerShowCurrentOnly,
        LayerShowAll,
        LayerShowNone,
        LayerExportRaster,

        TileProperties,

        TilePoolExport,
        TilePoolImportOver,
        TilePoolImport,
        TilePoolImportMerge,
        TilePoolDelete,
        TilePoolRename,
        TilePoolProperties,

        NewStaticTileBrush,
        NewDynamicTileBrush,
        TileBrushClone,
        TileBrushDelete,
        TileBrushFilter,

        TileSelectionCreateBrush,
        TileSelectionPromoteLayer,
        TileSelectionFloat,
        TileSelectionDefloat,

        ObjectProtoImport,
        ObjectProtoEdit,
        ObjectProtoDelete,
        ObjectProtoRename,
        ObjectProtoClone,
        ObjectProtoProperties,
        ObjectMoveTop,
        ObjectMoveUp,
        ObjectMoveDown,
        ObjectMoveBottom,
        ObjectReferenceImage,
        ObjectReferenceMask,
        ObjectReferenceOrigin,
        ObjectSnappingNone,
        ObjectSnappingTopLeft,
        ObjectSnappingTopRight,
        ObjectSnappingBottomLeft,
        ObjectSnappingBottomRight,
        ObjectSnappingTop,
        ObjectSnappingBottom,
        ObjectSnappingLeft,
        ObjectSnappingRight,
        ObjectSnappingVert,
        ObjectSnappingHorz,
        ObjectSnappingCenter,

        ObjectProperties,
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
