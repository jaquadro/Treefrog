using System;

namespace Treefrog.Framework
{
    internal class ResourceName
    {
        public ResourceName (object parent)
        {
            Parent = parent;
        }

        public ResourceName (object parent, string name)
            : this(parent)
        {
            Name = name;
        }

        public object Parent { get; private set; }
        public string Name { get; private set; }

        public event EventHandler<NameChangingEventArgs> NameChanging;
        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            var ev = NameChanging;
            if (ev != null)
                ev(Parent, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            var ev = NameChanged;
            if (ev != null)
                ev(Parent, e);
        }

        public bool TrySetName (string name)
        {
            if (Name != name) {
                try {
                    OnNameChanging(new NameChangingEventArgs(Name, name));
                }
                catch (NameChangeException) {
                    return false;
                }

                NameChangedEventArgs e = new NameChangedEventArgs(Name, name);
                Name = name;

                OnNameChanged(e);
            }

            return true;
        }
    }
}
