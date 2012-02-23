using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework
{
    public class NamedResourceEventArgs<T> : EventArgs
        where T : INamedResource
    {
        //public string Name { get; private set; }
        public T Resource { get; private set; }

        public NamedResourceEventArgs (T resource)
        {
            //Name = resource.Name;
            Resource = resource;
        }

        //public NamedResourceEventArgs (T resource)
        //{
            //Name = name;
        //    Resource = resource;
        //}
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

    public class OrderedResourceEventArgs<T> : NamedResourceEventArgs<T>
        where T : INamedResource
    {
        public int Order { get; private set; }

        public OrderedResourceEventArgs (T resource, int order)
            : base(resource)
        {
            Order = order;
        }
    }
}
