using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.ViewModel.Dialogs
{
    public interface IDialogViewModel
    {
        event EventHandler CloseRequested;
    }
}
