using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model.Controls;

namespace Editor.Views
{
    public partial class LevelPanel : UserControl
    {
        public LevelPanel ()
        {
            InitializeComponent();
        }

        public LayerControl LayerControl
        {
            get { return layerControl1; }
        }
    }
}
