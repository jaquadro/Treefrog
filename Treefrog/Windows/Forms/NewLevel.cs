using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Forms
{
    public partial class NewLevel : Form
    {
        private Project _project;
        private Level _level;

        private ValidationController _validateController;

        public NewLevel (Project project)
        {
            InitializeForm();

            _project = project;
            _name.Text = FindDefaultLevelName();

            _validateController.Validate();
        }

        public Level Level 
        {
            get { return _level; }
        }

        private void InitializeForm ()
        {
            InitializeComponent();

            _validateController = new ValidationController() {
                OKButton = _buttonOK,
            };

            _validateController.RegisterControl(_name, ValidateName);
            _validateController.RegisterControl(_originXField, 
                ValidationController.ValidateNumericUpDownFunc("Origin X", _originXField));
            _validateController.RegisterControl(_originYField, 
                ValidationController.ValidateNumericUpDownFunc("Origin Y", _originYField));
            _validateController.RegisterControl(_levelWidth, 
                ValidationController.ValidateNumericUpDownFunc("Level Width", _levelWidth));
            _validateController.RegisterControl(_levelHeight, 
                ValidationController.ValidateNumericUpDownFunc("Level Height", _levelHeight));
        }

        private void ApplyNew ()
        {
            int originX = (int)_originXField.Value;
            int originY = (int)_originYField.Value;
            int levelWidth = (int)_levelWidth.Value;
            int levelHeight = (int)_levelHeight.Value;

            _level = new Level(_name.Text.Trim(), originX, originY, levelWidth, levelHeight);
            //_level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", originX, originY, levelWidth, levelHeight));
            _project.Levels.Add(_level);
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            if (!_validateController.ValidateForm())
                return;

            ApplyNew();

            Close();
        }

        private string ValidateName ()
        {
            string txt = _name.Text.Trim();

            if (String.IsNullOrEmpty(txt))
                return "Name field must be non-empty.";
            else if (_project.Levels.Contains(txt))
                return "A layer with this name already exists.";
            else
                return null;
        }

        private string FindDefaultLevelName ()
        {
            List<string> names = new List<string>();
            foreach (Level level in _project.Levels) {
                names.Add(level.Name);
            }

            int i = 0;
            while (true) {
                string name = "Level " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
