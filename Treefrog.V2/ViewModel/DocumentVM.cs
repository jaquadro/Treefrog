using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;

namespace Treefrog.V2.ViewModel
{
    public abstract class DocumentVM : ViewModelBase
    {
        public abstract string Name { get; }
    }
}
