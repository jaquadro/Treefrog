using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public class LayoutAnchorablePaneControl : TabControl, ILayoutControl, ILogicalChildrenContainer
    {
        static LayoutAnchorablePaneControl()
        {
            FocusableProperty.OverrideMetadata(typeof(LayoutAnchorablePaneControl), new FrameworkPropertyMetadata(false));
        }

        public LayoutAnchorablePaneControl(LayoutAnchorablePane model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            _model = model;
            
            SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });

            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        void OnLayoutUpdated(object sender, EventArgs e)
        {
            var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
            //if (Orientation == System.Windows.Controls.Orientation.Horizontal)
            modelWithAtcualSize.ActualWidth = ActualWidth;
            //else
            modelWithAtcualSize.ActualHeight = ActualHeight;
        }

        //protected override DependencyObject GetContainerForItemOverride()
        //{
        //    return new LayoutAnchorableTabItem();
        //}
        List<object> _logicalChildren = new List<object>();
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return _logicalChildren.GetEnumerator(); // _model.Children.Select(a => a.Content).GetEnumerator();
            }
        }

        LayoutAnchorablePane _model;

        public ILayoutElement Model
        {
            get { return _model; }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            _model.SelectedContent.IsActive = true;
            
            base.OnGotKeyboardFocus(e);
        }

        void ILogicalChildrenContainer.InternalAddLogicalChild(object element)
        {
            if (_logicalChildren.Contains(element))
                throw new InvalidOperationException();

            System.Diagnostics.Debug.WriteLine("[{0}]InternalAddLogicalChild({1})", this, element);
            _logicalChildren.Add(element);
            AddLogicalChild(element);
        }

        void ILogicalChildrenContainer.InternalRemoveLogicalChild(object element)
        {
            if (!_logicalChildren.Contains(element))
                throw new InvalidOperationException();
            System.Diagnostics.Debug.WriteLine("[{0}]InternalRemoveLogicalChild({1})", this, element);
            _logicalChildren.Remove(element); 
            RemoveLogicalChild(element);
        }
    }
}