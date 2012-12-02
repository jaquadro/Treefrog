using System;
using System.Collections.Generic;
using System.Text;

namespace Treefrog.Framework.Compat
{
    public interface INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
