using System;

namespace Treefrog.Framework
{
    public interface IResource
    {
        Guid Uid { get; }

        event EventHandler Modified;
    }

    public class ResourceEventArgs : EventArgs
    {
        public static new ResourceEventArgs Empty = new ResourceEventArgs(Guid.Empty);

        public Guid Uid { get; private set; }

        public ResourceEventArgs (Guid uid)
        {
            Uid = uid;
        }
    }

    public class ResourceEventArgs<T> : ResourceEventArgs
        where T : IResource
    {
        public T Resource { get; private set; }

        public ResourceEventArgs (T resource)
            : base(resource.Uid)
        {
            Resource = resource;
        }
    }
}
