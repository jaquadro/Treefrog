using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace Treefrog.ViewModel.Layers
{
    public abstract class LayerVM : ViewModelBase
    {
        public abstract string LayerName { get; set; }

        public abstract bool IsVisible { get; set; }
    }
}
