using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Framework.Model;

namespace Treefrog.Windows.Forms
{
    public partial class NewLevel : Form
    {
        private Project _project;
        private Level _level;

        public NewLevel (Project project)
        {
            InitializeComponent();

            _project = project;
            _name.Text = FindDefaultLevelName();

            CheckValid();
        }

        public Level Level 
        {
            get { return _level; }
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            int tileWidth = (int)_tileWidth.Value;
            int tileHeight = (int)_tileHeight.Value;
            int levelWidth = (int)_levelWidth.Value;
            int levelHeight = (int)_levelHeight.Value;

            _level = new Level(_name.Text.Trim(), tileWidth, tileHeight, levelWidth, levelHeight);
            _level.Layers.Add(new MultiTileGridLayer("Tile Layer 1", tileWidth, tileHeight, levelWidth, levelHeight));
            _project.Levels.Add(_level);

            Close();
        }

        private void CheckValid ()
        {
            string txt = _name.Text.Trim();
            if (txt.Length > 0 && !_project.Levels.Contains(txt)) {
                _buttonOK.Enabled = true;
            }
            else {
                _buttonOK.Enabled = false;
            }
        }

        private void _name_TextChanged (object sender, EventArgs e)
        {
            CheckValid();
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
