using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
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
