using System;

namespace Treefrog.Framework
{
    public enum PropertyCategory
    {
        None,
        Predefined,
        Custom,
        Inherited,
    }

    public interface IPropertyProvider
    {
        string PropertyProviderName { get; }
        PropertyManager PropertyManager { get; }

        event EventHandler<EventArgs> PropertyProviderNameChanged;
    }
}
