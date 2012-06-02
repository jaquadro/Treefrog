using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Treefrog.Messages;
using Treefrog.ViewModel.Dialogs;
using Treefrog.View.Dialogs;
using Treefrog.Framework;
using Treefrog.Controls.Hooks;

namespace Treefrog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow ()
        {
            InitializeComponent();

            Messenger.Default.Register<BlockingDialogMessage>(this, act => DialogMessageHandler(act));

            ServiceContainer.Default.AddService<IOService>(new DefaultIOService());
            ServiceContainer.Default.AddService<IMessageService>(new MessageService());

            MouseHook.Attach();
        }

        private void DialogMessageHandler (BlockingDialogMessage message)
        {
            if (message.DialogVM is NewLevelDialogVM) {
                message.DialogResult = new NewLevelDialog() { DataContext = message.DialogVM }.ShowDialog();
            }
            else if (message.DialogVM is ImportTilePoolDialogVM) {
                message.DialogResult = new ImportTilePoolDialog() { DataContext = message.DialogVM }.ShowDialog();
            }
            else if (message.DialogVM is ImportObjectDialogVM) {
                message.DialogResult = new ImportObjectFromFileDialog() { DataContext = message.DialogVM }.ShowDialog();
            }
        }
    }
}
