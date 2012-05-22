using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Menu;

namespace Treefrog.ViewModel.ToolBars
{
    public class TileToolBarVM
    {
        private TileMenuVM _menu;

        public TileToolBarVM (TileMenuVM menu)
        {
            _menu = menu;
        }

        public TileMenuVM Menu
        {
            get { return _menu; }
        }
    }
}
