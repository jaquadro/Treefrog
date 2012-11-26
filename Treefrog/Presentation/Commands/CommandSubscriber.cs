using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Presentation.Commands
{
    public enum CommandKey
    {
        Undo,
        Redo,
        Cut,
        Copy,
        Paste,
        Delete,
        SelectAll,
        SelectNone,
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
            public Func<bool> CanPerform { get; set; }
            public Action Perform { get; set; }
        }

        private Dictionary<CommandKey, CommandHandler> _handlers = new Dictionary<CommandKey, CommandHandler>();

        public event EventHandler<CommandSubscriberEventArgs> CommandInvalidated;

        protected virtual void OnCommandInvalidated (CommandSubscriberEventArgs e)
        {
            var ev = CommandInvalidated;
            if (ev != null)
                ev(this, e);
        }

        public void Register (CommandKey key, Func<bool> performCheck, Action perform)
        {
            _handlers[key] = new CommandHandler() {
                CanPerform = performCheck,
                Perform = perform,
            };
        }

        public void Unregister (CommandKey key)
        {
            _handlers.Remove(key);
        }

        public void Invalidate (CommandKey key)
        {
            OnCommandInvalidated(new CommandSubscriberEventArgs(key));
        }

        public virtual bool CanHandle (CommandKey key)
        {
            return _handlers.ContainsKey(key);
        }

        public virtual bool CanPerform (CommandKey key)
        {
            if (!CanHandle(key))
                return false;

            if (_handlers[key].CanPerform == null)
                return false;

            return _handlers[key].CanPerform();
        }

        public virtual void Perform (CommandKey key)
        {
            if (!CanHandle(key))
                return;

            if (_handlers[key].Perform == null)
                return;

            _handlers[key].Perform();
        }
    }
}
