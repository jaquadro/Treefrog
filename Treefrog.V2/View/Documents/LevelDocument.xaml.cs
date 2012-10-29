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
using Treefrog.ViewModel;
using System.ComponentModel;

namespace Treefrog.View.Documents
{
    /// <summary>
    /// Interaction logic for LevelDocument.xaml
    /// </summary>
    public partial class LevelDocument : UserControl
    {
        public static readonly DependencyProperty ViewportProperty;

        static LevelDocument ()
        {
            ViewportProperty = DependencyProperty.Register("Viewport",
                typeof(ViewportVM), typeof(LevelDocument), new PropertyMetadata(new ViewportVM(), HandleViewportChanged));
        }

        public LevelDocument ()
        {
            InitializeComponent();
        }

        public ViewportVM Viewport
        {
            get { return (ViewportVM)this.GetValue(ViewportProperty); }
            set { this.SetValue(ViewportProperty, value); }
        }

        private static void HandleViewportChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LevelDocument doc = d as LevelDocument;

            ViewportVM viewport = e.OldValue as ViewportVM;
            if (viewport != null)
                viewport.PropertyChanged -= doc.HandleViewportPropertyChanged;

            viewport = e.NewValue as ViewportVM;
            if (viewport != null)
                viewport.PropertyChanged += doc.HandleViewportPropertyChanged;            
        }

        private void HandleViewportPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Offset")
                SyncViewportOffset();
        }

        private void SyncViewportOffset ()
        {
            if (Viewport.Offset.X != scrollViewer1.HorizontalOffset)
                scrollViewer1.ScrollToHorizontalOffset(Viewport.Offset.X);
            if (Viewport.Offset.Y != scrollViewer1.VerticalOffset)
                scrollViewer1.ScrollToVerticalOffset(Viewport.Offset.Y);
        }
    }
}
