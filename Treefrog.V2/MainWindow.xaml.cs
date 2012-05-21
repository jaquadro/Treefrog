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
using Treefrog.V2.Messages;
using Treefrog.V2.ViewModel.Dialogs;
using Treefrog.V2.View.Dialogs;
using Treefrog.Framework;

namespace Treefrog.V2
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
        }

        private void DialogMessageHandler (BlockingDialogMessage message)
        {
            if (message.DialogVM is NewLevelDialogVM) {
                message.DialogResult = new NewLevelDialog() { DataContext = message.DialogVM }.ShowDialog();
            }
            else if (message.DialogVM is ImportTilePoolDialogVM) {
                message.DialogResult = new ImportTilePoolDialog() { DataContext = message.DialogVM }.ShowDialog();
            }
        }
    }
}
