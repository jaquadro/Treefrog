using System;

namespace Treefrog.Framework
{
    public class NamedResourceEventArgs<T> : ResourceEventArgs<T>
        where T : INamedResource
    {
        public NamedResourceEventArgs (T resource)
            : base(resource)
        { }
    }

    public class NamedResourceRemappedEventArgs<T> : NamedResourceEventArgs<T>
        where T : INamedResource
    {
        public string OldName { get; private set; }
        public string NewName { get; private set; }

        public NamedResourceRemappedEventArgs (T resource, string oldName, string newName)
            : base(resource)
        {
            OldName = oldName;
            NewName = newName;
        }
    }

    public class OrderedResourceEventArgs<T> : ResourceEventArgs<T>
        where T : IResource
    {
        public int Order { get; private set; }

        public OrderedResourceEventArgs (T resource, int order)
            : base(resource)
        {
            Order = order;
        }
    }
}
