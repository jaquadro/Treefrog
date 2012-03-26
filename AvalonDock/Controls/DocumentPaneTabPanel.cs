using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public class DocumentPaneTabPanel : Panel
    {

        protected override Size MeasureOverride(Size availableSize)
        {
            var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != System.Windows.Visibility.Collapsed); 
            
            Size desideredSize = new Size();
            foreach (FrameworkElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    desideredSize.Width += child.DesiredSize.Width;

                desideredSize.Height = Math.Max(desideredSize.Height, child.DesiredSize.Height);
            }

            return new Size(Math.Min(desideredSize.Width, availableSize.Width), desideredSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var visibleChildren = Children.Cast<UIElement>().Where(ch => ch.Visibility != System.Windows.Visibility.Collapsed);
            double offset = 0.0;
            bool skipAllOthers = false;
            foreach (FrameworkElement doc in visibleChildren)
            {
                if (skipAllOthers || offset + doc.DesiredSize.Width > finalSize.Width)
                {
                    doc.Visibility = System.Windows.Visibility.Hidden;
                    skipAllOthers = true;
                }
                else
                {
                    doc.Visibility = System.Windows.Visibility.Visible;
                    doc.Arrange(new Rect(offset, 0.0, doc.DesiredSize.Width, finalSize.Height));
                    offset += doc.ActualWidth + doc.Margin.Left + doc.Margin.Right;
                }
            }

            return finalSize;

        }


        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                LayoutDocumentTabItem.IsDraggingItem())
            {
                var contentModel = LayoutDocumentTabItem.GetDraggingItem().Model;
                var manager = contentModel.Root.Manager;
                LayoutDocumentTabItem.ResetDraggingItem();

                
                manager.StartDraggingFloatingWindowForContent(contentModel);

            }

            base.OnMouseLeave(e);
        }
    }
}
