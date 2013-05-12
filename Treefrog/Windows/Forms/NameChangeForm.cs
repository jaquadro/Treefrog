using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Forms
{
    public partial class NameChangeForm : Form
    {
        private ValidationController _validateController;

        public NameChangeForm (string name)
        {
            InitializeComponent();

            Name = name;
            OriginalName = name;
            ReservedNames = new List<string>();

            _validateController = new ValidationController() {
                OKButton = _buttonOK,
            };

            _validateController.RegisterControl(_fieldName, 
                ValidationController.ValidateNonEmptyName("Name", _fieldName, ReservedNameEnumerator));
        }

        public NameChangeForm (string name, string title)
            : this(name)
        {
            Text = title;
        }

        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad(e);
            _fieldName.Text = Name;
        }

        private string OriginalName { get; set; }

        public new string Name { get; set; }

        public List<string> ReservedNames { get; set; }

        private IEnumerable<string> ReservedNameEnumerator
        {
            get
            {
                foreach (string s in ReservedNames) {
                    if (s != OriginalName)
                        yield return s;
                }
            }
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            if (!_validateController.ValidateForm())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void _fieldName_TextChanged (object sender, EventArgs e)
        {
            Name = _fieldName.Text;
        }
    }
}
