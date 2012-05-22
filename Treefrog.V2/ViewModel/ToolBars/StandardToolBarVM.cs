using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.ViewModel.Menu;

namespace Treefrog.ViewModel.ToolBars
{
    public class StandardToolBarVM
    {
        private StandardMenuVM _menu;

        public StandardToolBarVM (StandardMenuVM menu)
        {
            _menu = menu;
        }

        public StandardMenuVM Menu
        {
            get { return _menu; }
        }
    }
}
