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
using System.Windows.Shapes;
using Treefrog.ViewModel.Dialogs;

namespace Treefrog.View.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportObjectFromFileDialog.xaml
    /// </summary>
    public partial class ImportObjectFromFileDialog : Window
    {
        public ImportObjectFromFileDialog ()
        {
            InitializeComponent();
            this.DataContextChanged += DataContextChangedHandler;
        }

        private void DataContextChangedHandler (object sender, DependencyPropertyChangedEventArgs e)
        {
            IDialogViewModel d = e.NewValue as IDialogViewModel;
            if (d == null)
                return;

            d.CloseRequested += CloseRequestedHandler;
        }

        private void CloseRequestedHandler (object sender, EventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
