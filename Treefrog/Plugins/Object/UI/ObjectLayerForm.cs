using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Windows.Controllers;
using Treefrog.Framework.Model;

namespace Treefrog.Plugins.Object.UI
{
    public partial class ObjectLayerForm : Form
    {
        private ObjectLayer _layer;
        private Level _level;

        private ValidationController _validateController;

        public ObjectLayerForm (Level level, string name)
        {
            InitializeForm();

            Text = "New Object Layer";

            _level = level;
            _nameField.Text = name;

            _validateController.Validate();
        }

        public ObjectLayerForm (ObjectLayer layer)
        {
            InitializeForm();

            Text = "Edit Object Layer";

            _layer = layer;
            _nameField.Text = _layer.Name;
            _opacityField.Value = (decimal)_layer.Opacity;
            _gridWidthField.Value = _layer.GridWidth;
            _gridHeightField.Value = _layer.GridHeight;
            _gridColorButton.Color = Color.FromArgb(255, _layer.GridColor.R, _layer.GridColor.G, _layer.GridColor.B);

            _validateController.Validate();
        }

        private void InitializeForm ()
        {
            InitializeComponent();

            _validateController = new ValidationController() {
                OKButton = _okButton,
            };

            _validateController.RegisterControl(_nameField, ValidateName);
            _validateController.RegisterControl(_opacityField,
                ValidationController.ValidateNumericUpDownFunc("Opacity", _opacityField));
            _validateController.RegisterControl(_gridWidthField,
                ValidationController.ValidateNumericUpDownFunc("Grid Width", _gridWidthField));
            _validateController.RegisterControl(_gridHeightField,
                ValidationController.ValidateNumericUpDownFunc("Grid Height", _gridHeightField));
        }

        public ObjectLayer Layer
        {
            get { return _layer; }
        }

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        private string ValidateName ()
        {
            string txt = _nameField.Text.Trim();

            if (String.IsNullOrEmpty(txt))
                return "Name field must be non-empty.";
            else if (_reservedNames.Contains(txt))
                return "A layer with this name already exists.";
            else
                return null;
        }

        private void _opacitySlider_Scroll (object sender, EventArgs e)
        {
            decimal value = (decimal)_opacitySlider.Value / (decimal)_opacitySlider.Maximum;
            if (_opacityField.Value != value)
                _opacityField.Value = value;

            _validateController.Validate();
        }

        private void _opacityField_ValueChanged (object sender, EventArgs e)
        {
            int value = (int)(_opacityField.Value * 100);
            if (_opacitySlider.Value != value)
                _opacitySlider.Value = value;

            _validateController.Validate();
        }

        private void ApplyNew ()
        {
            _layer = new ObjectLayer(
                _nameField.Text.Trim(),
                _level);

            _layer.Opacity = (float)_opacityField.Value;
            _layer.GridWidth = (int)_gridWidthField.Value;
            _layer.GridHeight = (int)_gridHeightField.Value;
            _layer.GridColor = new Treefrog.Framework.Imaging.Color(_gridColorButton.Color.R, _gridColorButton.Color.G, _gridColorButton.Color.B, 128);
        }

        private void ApplyModify ()
        {
            _layer.Name = _nameField.Text.Trim();
            _layer.Opacity = (float)_opacityField.Value;
            _layer.GridWidth = (int)_gridWidthField.Value;
            _layer.GridHeight = (int)_gridHeightField.Value;
            _layer.GridColor = new Treefrog.Framework.Imaging.Color(_gridColorButton.Color.R, _gridColorButton.Color.G, _gridColorButton.Color.B, 128);
        }

        private void _okButton_Click (object sender, EventArgs e)
        {
            if (!_validateController.ValidateForm())
                return;

            if (_level != null)
                ApplyNew();
            else
                ApplyModify();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void _cancelButton_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void _gridColorButton_Click (object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog() {
                SolidColorOnly = true,
                Color = _gridColorButton.Color,
                FullOpen = true,
            };

            DialogResult result = cd.ShowDialog(this);

            _gridColorButton.Color = cd.Color;
        }
    }
}
