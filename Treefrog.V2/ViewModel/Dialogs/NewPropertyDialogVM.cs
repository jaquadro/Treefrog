using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Treefrog.ViewModel.Dialogs
{
    public class NewPropertyDialogVM : ViewModelBase, IDataErrorInfo, IDialogViewModel
    {
        #region Property Name

        #region Reserved Names

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        #endregion

        private string _propertyName = "";

        public string PropertyName
        {
            get { return _propertyName; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_propertyName != value) {
                    TestValidState(() =>
                    {
                        _propertyName = value;
                        RaisePropertyChanged("PropertyName");
                    });
                }
            }
        }

        #endregion

        #region Property Type

        private string _selectedType = "String";
        ObservableCollection<string> _propertyTypes = new ObservableCollection<string>()
        {
            "String", "Number", "Boolean Flag", "Color",
        };

        public ObservableCollection<string> PropertyTypes
        {
            get { return _propertyTypes; }
        }

        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_selectedType != value) {
                    TestValidState(() =>
                    {
                        _selectedType = value;
                        RaisePropertyChanged("SelectedType");
                    });
                }
            }
        }

        #endregion

        #region Validation

        public bool IsValid
        {
            get
            {
                return ValidatePropertyName() == null
                    && ValidatePropertyType() == null;
            }
        }

        private string ValidatePropertyName ()
        {
            if (string.IsNullOrWhiteSpace(_propertyName))
                return "Level Name must not be empty";
            if (_reservedNames.Contains(_propertyName))
                return "Level Name conflicts with another Level";
            return null;
        }

        private string ValidatePropertyType ()
        {
            if (!_propertyTypes.Contains(_selectedType))
                return "Invalid Property Type";
            return null;
        }

        private void TestValidState (Action act)
        {
            bool valid = IsValid;

            act();

            if (valid != IsValid)
                RaisePropertyChanged("IsValid");
        }

        #endregion

        #region Commands

        public event EventHandler CloseRequested = (s, e) => { };

        protected virtual void OnCloseRequested (EventArgs e)
        {
            CloseRequested(this, e);
        }

        private RelayCommand _okayCommand;

        public ICommand OkayCommand
        {
            get
            {
                if (_okayCommand == null)
                    _okayCommand = new RelayCommand(OnExecuteOkay, CanExecuteOkay);
                return _okayCommand;
            }
        }

        private bool CanExecuteOkay ()
        {
            return true;
        }

        private void OnExecuteOkay ()
        {
            OnCloseRequested(EventArgs.Empty);
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName) {
                    case "PropertyName":
                        return ValidatePropertyName();
                    case "SelectedType":
                        return ValidatePropertyType();
                    default:
                        return null;
                }
            }
        }

        #endregion
    }
}
