using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Treefrog.Windows.Controllers
{
    public class ValidationController : IDisposable
    {
        private class ControlRecord
        {
            public Control Control { get; set; }
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
                    if (msg != null)
                        errorProvider.SetError(control, msg);
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
                    ErrorProvider = errorProvider,
                    ValidatedHandler = validatedHandler,
                    TextChangedHandler = textChangedHandler,
                };
                record.Link();

                _controls.Add(record);
                _validateFuncs.Add(validationFunc);
            }
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
    }
}
