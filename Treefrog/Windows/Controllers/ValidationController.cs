using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Linq;

namespace Treefrog.Windows.Controllers
{
    public class ValidationController : IDisposable
    {
        private class ControlRecord
        {
            public Control Control { get; set; }
            public Func<string> ValidationFunc { get; set; }
            public ErrorProvider ErrorProvider { get; set; }
            public EventHandler ValidatedHandler { get; set; }
            public EventHandler TextChangedHandler { get; set; }

            public void Link ()
            {
                Control.Validated += ValidatedHandler;
                Control.TextChanged += TextChangedHandler;
            }

            public void Unlink ()
            {
                Control.Validated -= ValidatedHandler;
                Control.TextChanged -= TextChangedHandler;
            }
        }

        private List<ControlRecord> _controls = new List<ControlRecord>();
        private List<Func<string>> _validateFuncs = new List<Func<string>>();

        private bool _disposed;

        public void Dispose ()
        {
            Dispose(true);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    foreach (ControlRecord record in _controls)
                        record.Unlink();
                }

                _disposed = true;
            }
        }

        public Button OKButton { get; set; }
        public Button ApplyButton { get; set; }

        public void Validate ()
        {
            EnableButtons(ValidateForm());
        }

        public bool ValidateForm ()
        {
            foreach (Func<string> func in _validateFuncs) {
                if (func() != null)
                    return false;
            }

            return true;
        }

        public void RegisterControl (Control control, Func<string> validationFunc)
        {
            if (control != null && validationFunc != null) {
                ErrorProvider errorProvider = new ErrorProvider();

                EventHandler validatedHandler = (sender, e) => {
                    string msg = validationFunc();
                    if (msg != null) {
                        errorProvider.SetIconPadding(control, DetermineErrorIconPadding(control));
                        errorProvider.SetError(control, msg);
                    }
                    else
                        errorProvider.SetError(control, String.Empty);
                    Validate();
                };

                EventHandler textChangedHandler = (sender, e) => {
                    if (validationFunc() == null)
                        errorProvider.SetError(control, String.Empty);
                    Validate();
                };

                ControlRecord record = new ControlRecord() {
                    Control = control,
                    ValidationFunc = validationFunc,
                    ErrorProvider = errorProvider,
                    ValidatedHandler = validatedHandler,
                    TextChangedHandler = textChangedHandler,
                };
                record.Link();

                _controls.Add(record);
                _validateFuncs.Add(validationFunc);
            }
        }

        public void UnregisterControl (Control control)
        {
            if (control != null) {
                ControlRecord record = _controls.Find(r => { return r.Control == control; });
                if (record != null) {
                    _validateFuncs.Remove(record.ValidationFunc);
                    _controls.Remove(record);

                    record.Unlink();
                }
            }
        }

        public void UnregisterValidationFunc (Func<string> validationFunc)
        {
            _validateFuncs.Remove(validationFunc);
        }

        public void RegisterValidationFunc (Func<string> validationFunc)
        {
            if (validationFunc != null)
                _validateFuncs.Add(validationFunc);
        }

        private void EnableButtons (bool enable)
        {
            if (OKButton != null)
                OKButton.Enabled = enable;
            if (ApplyButton != null)
                ApplyButton.Enabled = enable;
        }

        private int DetermineErrorIconPadding (Control control)
        {
            if (control is TextBox)
                return -18;
            if (control is NumericUpDown)
                return -32;

            return -18;
        }

        public static Func<string> ValidateNonEmptyName (string fieldName, TextBox control, IEnumerable<string> reservedNames)
        {
            return () => {
                string txt = (control.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(txt))
                    return fieldName + " must not be empty";
                if (reservedNames != null && reservedNames.Contains(txt))
                    return fieldName + " conflicts with another name";
                return null;
            };
        }

        public static Func<string> ValidateNumericUpDownFunc (string fieldName, NumericUpDown control)
        {
            return () => {
                if (control.Value < control.Minimum || control.Value > control.Maximum)
                    return fieldName + " must be in range [" + control.Minimum + ", " + control.Maximum + "].";
                else
                    return null;
            };
        }

        public static Func<string> ValidateGreater (string fieldName, NumericUpDown control, string refName, NumericUpDown reference)
        {
            return () => {
                if (control.Value <= reference.Value)
                    return fieldName + " must be greater than " + refName;
                else
                    return ValidateNumericUpDownFunc(fieldName, control)();
            };
        }

        public static Func<string> ValidateGreaterEq (string fieldName, NumericUpDown control, string refName, NumericUpDown reference)
        {
            return () => {
                if (control.Value < reference.Value)
                    return fieldName + " must be greater than or equal to " + refName;
                else
                    return ValidateNumericUpDownFunc(fieldName, control)();
            };
        }

        public static Func<string> ValidateLess (string fieldName, NumericUpDown control, string refName, NumericUpDown reference)
        {
            return () => {
                if (control.Value >= reference.Value)
                    return fieldName + " must be less than " + refName;
                else
                    return ValidateNumericUpDownFunc(fieldName, control)();
            };
        }

        public static Func<string> ValidateLessEq (string fieldName, NumericUpDown control, string refName, NumericUpDown reference)
        {
            return () => {
                if (control.Value > reference.Value)
                    return fieldName + " must be less than or equal to " + refName;
                else
                    return ValidateNumericUpDownFunc(fieldName, control)();
            };
        }
    }
}
