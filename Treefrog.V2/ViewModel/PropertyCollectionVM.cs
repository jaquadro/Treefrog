using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.ComponentModel;
using GalaSoft.MvvmLight;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Treefrog.ViewModel.Dialogs;
using Treefrog.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace Treefrog.ViewModel
{
    public class PropertyProviderAdapter : ICustomTypeDescriptor
    {
        private static Attribute[] _emptyAttribute = new Attribute[0];

        private IPropertyProvider _provider;

        public PropertyProviderAdapter (IPropertyProvider provider)
        {
            _provider = provider;
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes ()
        {
            return new AttributeCollection(new Attribute[0]);
        }

        public string GetClassName ()
        {
            return _provider.PropertyProviderName;
        }

        public string GetComponentName ()
        {
            return string.Empty;
        }

        public TypeConverter GetConverter ()
        {
            return null;
        }

        public EventDescriptor GetDefaultEvent ()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty ()
        {
            return null;
        }

        public object GetEditor (Type editorBaseType)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents (Attribute[] attributes)
        {
            return new EventDescriptorCollection(new EventDescriptor[0]);
        }

        public EventDescriptorCollection GetEvents ()
        {
            return new EventDescriptorCollection(new EventDescriptor[0]);
        }

        public PropertyDescriptorCollection GetProperties (Attribute[] attributes)
        {
            Attribute[] sysAttr = MergeArrays(attributes, new Attribute[] {
                new CategoryAttribute("System")
            });
            Attribute[] userAttr = MergeArrays(attributes, new Attribute[] {
                new CategoryAttribute("User Defined")
            });

            DynamicPropertyDescriptor[] properties = MergeArrays(_provider.PredefinedProperties.Select(prop =>
                    new DynamicPropertyDescriptor(_provider, prop.Name, GetPropertyObject(prop).GetType(), sysAttr)).ToArray(),
                _provider.CustomProperties.Select(prop =>
                    new DynamicPropertyDescriptor(_provider, prop.Name, GetPropertyObject(prop).GetType(), userAttr)).ToArray()
                );

            return new PropertyDescriptorCollection(properties);
        }

        public PropertyDescriptorCollection GetProperties ()
        {
            return GetProperties(_emptyAttribute);
        }

        public object GetPropertyOwner (PropertyDescriptor pd)
        {
            return _provider;
        }

        #endregion

        private static T[] MergeArrays<T> (params T[][] arrs)
        {
            int count = 0;
            foreach (Array item in arrs)
                count += item.Length;

            T[] merge = new T[count];
            for (int i = 0, index = 0; i < arrs.Length; index += arrs[i].Length, i++)
                arrs[i].CopyTo(merge, index);

            return merge;
        }

        private static object GetPropertyObject (Property property)
        {
            if (property is NumberProperty)
                return (property as NumberProperty).Value;
            else if (property is StringProperty)
                return (property as StringProperty).Value;
            else if (property is BoolProperty)
                return (property as BoolProperty).Value;
            else
                return null;
        }

        private class DynamicPropertyDescriptor : PropertyDescriptor
        {
            private readonly IPropertyProvider _provider;
            private readonly Type _propertyType;

            public DynamicPropertyDescriptor (IPropertyProvider provider, string propertyName, Type propertyType, Attribute[] attributes)
                : base(propertyName, attributes)
            {
                _provider = provider;
                _propertyType = propertyType;
            }

            public override bool CanResetValue (object component)
            {
                return true;
            }

            public override object GetValue (object component)
            {
                return GetPropertyObject(_provider.LookupProperty(Name));
            }

            public override void ResetValue (object component)
            {
            }

            public override void SetValue (object component, object value)
            {
                Property prop = _provider.LookupProperty(Name);
                if (prop != null) {
                    if (prop is NumberProperty && value is float)
                        (prop as NumberProperty).Value = (float)value;
                    else if (prop is StringProperty && value is string)
                        (prop as StringProperty).Value = (string)value;
                    else if (prop is BoolProperty && value is bool)
                        (prop as BoolProperty).Value = (bool)value;
                }
            }

            public override bool ShouldSerializeValue (object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(PropertyProviderAdapter); }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return _propertyType; }
            }
        }
    }

    public interface PropertyManagerService
    {
        IPropertyProvider ActiveProvider { get; set; }
    }

    public class PropertyCollectionVM : ViewModelBase, PropertyManagerService
    {
        private PropertyProviderAdapter _adapter;
        private IPropertyProvider _provider;

        public PropertyCollectionVM ()
        {
        }

        public IPropertyProvider ActiveProvider
        {
            get { return _provider; }
            set
            {
                if (_provider != value) {
                    _provider = value;
                    _adapter = new PropertyProviderAdapter(_provider);
                    RaisePropertyChanged("PropertyObject");
                }
            }
        }

        public Object PropertyObject
        {
            get { return _adapter; }
        }

        #region Commands

        #region Add New Property

        private RelayCommand _addPropertyCommand;

        public ICommand AddPropertyCommand
        {
            get
            {
                if (_addPropertyCommand == null)
                    _addPropertyCommand = new RelayCommand(OnAddProperty, CanAddProperty);
                return _addPropertyCommand;
            }
        }

        public bool IsAddPropertyEnabled
        {
            get { return CanAddProperty(); }
        }

        private bool CanAddProperty ()
        {
            return ActiveProvider != null;
        }

        private void OnAddProperty ()
        {
            if (CanAddProperty()) {
                if (_provider == null)
                    return;

                NewPropertyDialogVM vm = new NewPropertyDialogVM();
                foreach (Property prop in _provider.PredefinedProperties)
                    vm.ReservedNames.Add(prop.Name);
                foreach (Property prop in _provider.CustomProperties)
                    vm.ReservedNames.Add(prop.Name);

                BlockingDialogMessage message = new BlockingDialogMessage(this, vm);
                Messenger.Default.Send(message);

                if (message.DialogResult == true) {
                    Property prop = null;
                    switch (vm.SelectedType) {
                        case "String":
                            prop = new StringProperty(vm.PropertyName, "");
                            break;
                        case "Number":
                            prop = new NumberProperty(vm.PropertyName, 0f);
                            break;
                        case "Boolean Flag":
                            prop = new BoolProperty(vm.PropertyName, false);
                            break;
                        default:
                            return;
                    }

                    _provider.CustomProperties.Add(prop);

                    _adapter = new PropertyProviderAdapter(_provider);
                    RaisePropertyChanged("PropertyObject");
                }
            }
        }

        #endregion

        #endregion
    }
}
