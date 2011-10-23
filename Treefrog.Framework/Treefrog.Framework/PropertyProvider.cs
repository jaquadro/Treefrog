using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    using Model;

    public enum PropertyCategory
    {
        None,
        Predefined,
        Custom
    }

    public interface IPropertyProvider
    {
        string PropertyProviderName { get; }

        IEnumerable<Property> PredefinedProperties { get; }
        IEnumerable<Property> CustomProperties { get; }

        PropertyCategory LookupPropertyCategory (string name);
        Property LookupProperty (string name);

        void AddCustomProperty (Property property);
        void RemoveCustomProperty (string name);
    }
}
