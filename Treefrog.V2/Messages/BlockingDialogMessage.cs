using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.V2.ViewModel.Dialogs;
using GalaSoft.MvvmLight.Messaging;

namespace Treefrog.V2.Messages
{
    public class BlockingDialogMessage : MessageBase
    {
        public BlockingDialogMessage (IDialogViewModel dialogVM)
        {
            DialogVM = dialogVM;
        }

        public BlockingDialogMessage (object sender, IDialogViewModel dialogVM)
            : base(sender)
        {
            DialogVM = dialogVM;
        }

        public BlockingDialogMessage (object sender, object target, IDialogViewModel dialogVM)
            : base(sender, target)
        {
            DialogVM = dialogVM;
        }

        public bool? DialogResult { get; set; }
        public IDialogViewModel DialogVM { get; protected set; }
    }
}
