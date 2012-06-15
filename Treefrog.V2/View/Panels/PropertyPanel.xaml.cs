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

namespace Treefrog.View.Panels
{
    /// <summary>
    /// Interaction logic for PropertyPanel.xaml
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        static PropertyPanel ()
        {
            SelectedPropertyProperty = DependencyProperty.Register("SelectedProperty",
                typeof(string), typeof(PropertyPanel));
        }

        public PropertyPanel ()
        {
            InitializeComponent();
            this.DataContextChanged += HandleDataContextChanged;
        }

        private void HandleDataContextChanged (object sender, DependencyPropertyChangedEventArgs e)
        {
            Binding binding = new Binding();
            binding.Path = new PropertyPath("SelectedProperty");
            binding.Source = this.DataContext;
            binding.Mode = BindingMode.TwoWay;
            
            BindingOperations.SetBinding(this, SelectedPropertyProperty, binding);
        }

        public static readonly DependencyProperty SelectedPropertyProperty;

        public string SelectedProperty
        {
            get { return (string)this.GetValue(SelectedPropertyProperty); }
            set { this.SetValue(SelectedPropertyProperty, value); }
        }

        private void propertyGrid1_SelectedPropertyItemChanged (object sender, RoutedEventArgs e)
        {
            if (propertyGrid1.SelectedProperty == null)
                SelectedProperty = null;
            else
                SelectedProperty = propertyGrid1.SelectedProperty.Name;
        }
    }
}
