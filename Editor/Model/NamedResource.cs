using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
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

    public interface INamedResource
    {
        //int Id { get; }
        string Name { get; }

        //event EventHandler<IdChangedEventArgs> IdChanged;
        event EventHandler<NameChangedEventArgs> NameChanged;
    }
}
