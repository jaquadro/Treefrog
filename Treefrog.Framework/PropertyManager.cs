using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Framework.Model.Collections;

namespace Treefrog.Framework
{
    public class PropertyManager
    {
        private PropertyClassManager _classManager;
        private object _inst;

        private PropertyCollection _custom;
        private IPropertyProvider _customParent;

        internal PropertyManager (PropertyClassManager classManager, object inst)
        {
            _classManager = classManager;
            _inst = inst;

            _custom = new PropertyCollection(_classManager.RegisteredNames);
        }

        internal IPropertyProvider PropertyParent
        {
            get { return _customParent; }
            set { _customParent = value; }
        }

        public IEnumerable<Property> SpecialProperties
        {
            get
            {
                foreach (string name in _classManager.RegisteredNames)
                    yield return _classManager.LookupProperty(_inst, name);
            }
        }

        public PropertyCollection CustomProperties
        {
            get { return _custom; }
        }

        public PropertyCollection InheritedProperties
        {
            get
            {
                if (_customParent != null)
                    return _customParent.CustomProperties;
                return null;
            }
        }

        public bool HasPropertyParent
        {
            get { return _customParent != null; }
        }

        public PropertyCategory LookupCategory (string propertyName)
        {
            if (_classManager.ContainsDefinition(propertyName))
                return PropertyCategory.Predefined;
            if (_custom.Contains(propertyName))
                return PropertyCategory.Custom;
            if (_customParent != null && _customParent.CustomProperties.Contains(propertyName))
                return PropertyCategory.Inherited;

            return PropertyCategory.None;
        }

        public Property LookupProperty (string propertyName)
        {
            if (_classManager.ContainsDefinition(propertyName))
                return _classManager.LookupProperty(_inst, propertyName);

            if (_custom.Contains(propertyName))
                return _custom[propertyName];

            if (_customParent != null && _customParent.CustomProperties.Contains(propertyName))
                return _customParent.CustomProperties[propertyName];

            return null;
        }

        public bool IsReadOnly (string propertyName)
        {
            if (_classManager.ContainsDefinition(propertyName))
                return _classManager.IsReadOnly(propertyName);

            if (_custom.Contains(propertyName))
                return false;

            return true;
        }
    }
}
