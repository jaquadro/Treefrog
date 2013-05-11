using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Collections;

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

        event EventHandler<EventArgs> PropertyProviderNameChanged;

        PropertyManager PropertyManager { get; }

        //IEnumerable<Property> PredefinedProperties { get; }
        //IEnumerable<Property> CustomProperties { get; }

        PropertyCollection CustomProperties { get; }
        PredefinedPropertyCollection PredefinedProperties { get; }

        //event EventHandler<PropertyEventArgs> PropertyAdded;
        //event EventHandler<PropertyEventArgs> PropertyRemoved;
        //event EventHandler<PropertyEventArgs> PropertyModified;
        //event EventHandler<NameChangedEventArgs> PropertyRenamed;

        PropertyCategory LookupPropertyCategory (string name);
        Property LookupProperty (string name);

        //void AddCustomProperty (Property property);
        //void RemoveCustomProperty (string name);
    }
}
