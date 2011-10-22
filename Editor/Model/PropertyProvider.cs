using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public interface IPropertyProvider
    {
        string PropertyProviderName { get; }

        IEnumerable<Property> PredefinedProperties { get; }
        IEnumerable<Property> CustomProperties { get; }

        void AddCustomProperty (Property property);
        void RemoveCustomProperty (string name);
    }
}
