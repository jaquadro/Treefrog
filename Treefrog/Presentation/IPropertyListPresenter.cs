using System;
using System.Collections.Generic;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Extensibility;

namespace Treefrog.Presentation
{
    public class PropertyListPresenter : Presenter
    {
        IPropertyProvider _provider;

        string _selectedProperty;

        public PropertyListPresenter (PresenterManager pm)
            : base(pm)
        { }

        public IPropertyProvider Provider
        {
            get { return _provider; }
            set
            {
                if (_provider != null) {
                    _provider.PropertyProviderNameChanged -= PropertyProvider_PropertyProviderNameChanged;
                }

                _provider = value;
                _selectedProperty = null;

                if (value != null) {
                    _provider.PropertyProviderNameChanged += PropertyProvider_PropertyProviderNameChanged;
                }

                RefreshPropertyList();
            }
        }

        private void PropertyProvider_PropertyProviderNameChanged (object sender, EventArgs e)
        {
            OnSyncPropertyContainer(EventArgs.Empty);
        }

        #region IPropertyListPresenter Members

        #region Properties

        public bool CanAddProperty
        {
            get { return _provider != null; }
        }

        public bool CanRemoveSelectedProperty
        {
            get { return _provider != null && _provider.PropertyManager.LookupCategory(_selectedProperty) == PropertyCategory.Custom; }
        }

        public bool CanRenameSelectedProperty
        {
            get { return _provider != null && _provider.PropertyManager.LookupCategory(_selectedProperty) == PropertyCategory.Custom; }
        }

        public bool CanEditSelectedProperty
        {
            get { return _provider != null && !_provider.PropertyManager.IsReadOnly(_selectedProperty); }
        }

        public string ProviderName 
        {
            get 
            { 
                if (_provider == null) {
                    return "";
                }
                return _provider.PropertyProviderName;
            }
        }

        public IEnumerable<Property> PredefinedProperties
        {
            get 
            {
                if (_provider == null) {
                    yield break;
                }

                foreach (Property property in _provider.PropertyManager.SpecialProperties) {
                    yield return property;
                }
            }
        }

        public IEnumerable<Property> InheritedProperties
        {
            get
            {
                if (_provider == null || _provider.PropertyManager.InheritedProperties == null) {
                    yield break;
                }

                foreach (Property property in _provider.PropertyManager.InheritedProperties) {
                    if (!_provider.PropertyManager.CustomProperties.Contains(property.Name))
                        yield return property;
                }
            }
        }

        public IEnumerable<Property> CustomProperties
        {
            get
            {
                if (_provider == null) {
                    yield break;
                }

                foreach (Property property in _provider.PropertyManager.CustomProperties) {
                    yield return property;
                }
            }
        }

        public Property SelectedProperty
        {
            get
            {
                if (_provider != null) {
                    return _provider.PropertyManager.LookupProperty(_selectedProperty);
                }
                return null;
            }
        }

        #endregion

        #region Events

        public event EventHandler SyncPropertyContainer;

        public event EventHandler SyncPropertyActions;

        public event EventHandler SyncPropertyList;

        public event EventHandler SyncPropertySelection;

        #endregion

        #region Event Dispatchers

        protected void OnSyncPropertyContainer (EventArgs e)
        {
            if (SyncPropertyContainer != null) {
                SyncPropertyContainer(this, e);
            }
        }

        protected void OnSyncPropertyActions (EventArgs e)
        {
            if (SyncPropertyActions != null) {
                SyncPropertyActions(this, e);
            }
        }

        protected void OnSyncPropertyList (EventArgs e)
        {
            if (SyncPropertyList != null) {
                SyncPropertyList(this, e);
            }
        }

        protected void OnSyncPropertySelection (EventArgs e)
        {
            if (SyncPropertySelection != null) {
                SyncPropertySelection(this, e);
            }
        }

        #endregion

        #region View Action API

        public void ActionAddCustomProperty ()
        {
            if (CanAddProperty) {
                Property property = new StringProperty(FindDefaultPropertyName(), "");
                _provider.PropertyManager.CustomProperties.Add(property);

                _selectedProperty = property.Name;
            }

            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        public void ActionRemoveSelectedProperty ()
        {
            if (CanRemoveSelectedProperty) {
                _provider.PropertyManager.CustomProperties.Remove(_selectedProperty);
            }

            _selectedProperty = null;

            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        public void ActionRenameSelectedProperty (string name)
        {
            if (CanRenameSelectedProperty && _provider.PropertyManager.LookupProperty(name) == null) {
                Property property = _provider.PropertyManager.LookupProperty(_selectedProperty);
                if (!property.TrySetName(name))
                    return;

                _selectedProperty = name;

                OnSyncPropertyActions(EventArgs.Empty);
                OnSyncPropertyList(EventArgs.Empty);
            }
        }

        public void ActionEditSelectedProperty (string value)
        {
            if (CanEditSelectedProperty) {
                Property property = _provider.PropertyManager.LookupProperty(_selectedProperty);

                if (_provider.PropertyManager.LookupCategory(_selectedProperty) == PropertyCategory.Inherited) {
                    property = property.Clone() as Property;
                    _provider.PropertyManager.CustomProperties.Add(property);
                }

                property.Parse(value);
                OnSyncPropertyContainer(EventArgs.Empty);
                OnSyncPropertyList(EventArgs.Empty);
            }
        }

        public void ActionSelectProperty (string name)
        {
            if (_provider != null && _provider.PropertyManager.LookupProperty(name) != null) {
                _selectedProperty = name;

                OnSyncPropertyActions(EventArgs.Empty);
                OnSyncPropertySelection(EventArgs.Empty);
            }
        }

        #endregion

        public void RefreshPropertyList ()
        {
            OnSyncPropertyContainer(EventArgs.Empty);
            OnSyncPropertyActions(EventArgs.Empty);
            OnSyncPropertyList(EventArgs.Empty);
        }

        private string FindDefaultPropertyName ()
        {
            List<string> names = new List<string>();
            foreach (Property property in _provider.PropertyManager.SpecialProperties)
                names.Add(property.Name);

            if (_provider.PropertyManager.InheritedProperties != null) {
                foreach (Property property in _provider.PropertyManager.InheritedProperties)
                    names.Add(property.Name);
            }

            foreach (Property property in _provider.PropertyManager.CustomProperties)
                names.Add(property.Name);

            int i = 0;
            while (true) {
                string name = "Property " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }

        #endregion
    }
}
