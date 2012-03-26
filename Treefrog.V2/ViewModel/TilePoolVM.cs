using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using Treefrog.Framework.Model;
using Treefrog.V2.Controls.Xna;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Treefrog.Framework;
using System.IO;
using Treefrog.V2.ViewModel.Menu;
using Treefrog.V2.ViewModel.ToolBars;

namespace Treefrog.V2.ViewModel
{
    public class TilePoolVM : ViewModelBase
    {
        private TilePool _tilePool;

        public TilePoolVM (TilePool tilePool)
        {
            _tilePool = tilePool;
        }

        public TilePool TilePool
        {
            get { return _tilePool; }
        }

        public string Name
        {
            get { return _tilePool.Name; }
        }
    }
}
