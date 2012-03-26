using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.V2.ViewModel.Menu;

namespace Treefrog.V2.ViewModel.ToolBars
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
