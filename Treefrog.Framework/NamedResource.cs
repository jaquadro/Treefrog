using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class IdChangedEventArgs : EventArgs
    {
        public int OldId { get; private set; }
        public int NewId { get; private set; }

        public IdChangedEventArgs (int oldId, int newId)
        {
            OldId = oldId;
            NewId = newId;
        }
    }

    public class NameChangedEventArgs : EventArgs
    {
        public string OldName { get; private set; }
        public string NewName { get; private set; }

        public NameChangedEventArgs (string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    public class NameChangingEventArgs : EventArgs
    {
        public string OldName { get; private set; }
        public string NewName { get; private set; }

        public bool Cancel { get; set; }

        public NameChangingEventArgs (string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    /// <summary>
    /// An interface that treats objects as named resources and provides events for signalling changes to the resource.
    /// </summary>
    public interface INamedResource
    {
        /// <summary>
        /// Gets the name of the resource.
        /// </summary>
        string Name { get; }

        event EventHandler<NameChangingEventArgs> NameChanging;

        /// <summary>
        /// Occurs when the resource's name is changed.
        /// </summary>
        event EventHandler<NameChangedEventArgs> NameChanged;

        /// <summary>
        /// Occurs when the resource's content is modified, excluding changes to its name.
        /// </summary>
        event EventHandler Modified;
    }
}
