using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;
using System.Diagnostics;

namespace AvalonDock.Controls
{
    public class LayoutDocumentTabItem : Control
    {
        static LayoutDocumentTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentTabItem), new FrameworkPropertyMetadata(typeof(LayoutDocumentTabItem)));
        }

        public LayoutDocumentTabItem()
        {
            //DataContextChanged += new DependencyPropertyChangedEventHandler(OnDataContextChanged);
        }

        #region Model

        /// <summary>
        /// Model Dependency Property
        /// </summary>
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(LayoutContent), typeof(LayoutDocumentTabItem),
                new FrameworkPropertyMetadata((LayoutContent)null,
                    new PropertyChangedCallback(OnModelChanged)));

        /// <summary>
        /// Gets or sets the Model property.  This dependency property 
        /// indicates the layout content model attached to the tab item.
        /// </summary>
        public LayoutContent Model
        {
            get { return (LayoutContent)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Model property.
        /// </summary>
        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutDocumentTabItem)d).OnModelChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Model property.
        /// </summary>
        protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateLogicalParent();
        }

        #endregion



        //LayoutContent GetModel()
        //{
        //    return Model;
        //}

        //void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var parentPaneControl = this.FindVisualAncestor<LayoutDocumentPaneControl>();
        //    if (e.OldValue != null && parentPaneControl != null)
        //    {
        //        ((ILogicalChildrenContainer)parentPaneControl).InternalRemoveLogicalChild(e.OldValue);
        //    }

        //    if (e.NewValue != null && parentPaneControl != null)
        //    {
        //        ((ILogicalChildrenContainer)parentPaneControl).InternalAddLogicalChild(e.NewValue);
        //    }
        //}

        //protected override void OnVisualParentChanged(DependencyObject oldParent)
        //{
        //    base.OnVisualParentChanged(oldParent);

        //    var contentModel = GetModel();

        //    if (oldParent != null && contentModel != null)
        //    {
        //        var oldParentPaneControl = oldParent.FindVisualAncestor<LayoutDocumentPaneControl>();
        //        if (oldParentPaneControl != null)
        //        {
        //            ((ILogicalChildrenContainer)oldParentPaneControl).InternalRemoveLogicalChild(contentModel.Content);
        //        }
        //    }

        //    if (contentModel != null && contentModel.Content != null && contentModel.Content is DependencyObject)
        //    {
        //        var oldLogicalParentPaneControl = LogicalTreeHelper.GetParent(contentModel.Content as DependencyObject)
        //            as ILogicalChildrenContainer;
        //        if (oldLogicalParentPaneControl != null)
        //            oldLogicalParentPaneControl.InternalRemoveLogicalChild(contentModel.Content);
        //    }

        //    var parentPaneControl = this.FindVisualAncestor<LayoutDocumentPaneControl>();
        //    if (contentModel != null && parentPaneControl != null && contentModel.Content != null)
        //    {
        //        ((ILogicalChildrenContainer)parentPaneControl).InternalAddLogicalChild(contentModel.Content);
        //    }
        //}

        void UpdateLogicalParent()
        {
            //var contentModel = GetModel();

            //if (oldParent != null && contentModel != null)
            //{
            //    var oldParentPaneControl = oldParent.FindVisualAncestor<LayoutDocumentPaneControl>();
            //    if (oldParentPaneControl != null)
            //    {
            //        ((ILogicalChildrenContainer)oldParentPaneControl).InternalRemoveLogicalChild(contentModel.Content);
            //    }
            //}

            if (Model != null && Model.Content != null && Model.Content is DependencyObject)
            {
                var oldLogicalParentPaneControl = LogicalTreeHelper.GetParent(Model.Content as DependencyObject)
                    as ILogicalChildrenContainer;
                if (oldLogicalParentPaneControl != null)
                    oldLogicalParentPaneControl.InternalRemoveLogicalChild(Model.Content);
            }

            var parentPaneControl = this.FindVisualAncestor<LayoutDocumentPaneControl>();
            if (Model != null && parentPaneControl != null && Model.Content != null)
            {
                ((ILogicalChildrenContainer)parentPaneControl).InternalAddLogicalChild(Model.Content);
            }        
        }

        bool _isMouseDown = false;
        static LayoutDocumentTabItem _draggingItem = null;

        internal static bool IsDraggingItem()
        {
            return _draggingItem != null;
        }

        internal static LayoutDocumentTabItem GetDraggingItem()
        {
            return _draggingItem;
        }
        internal static void ResetDraggingItem()
        {
            _draggingItem = null;
        }



        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            _isMouseDown = true;
            _draggingItem = this;
        }

        protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isMouseDown = false;
                _draggingItem = null;
            }
        }

        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            _isMouseDown = false;

            base.OnMouseLeftButtonUp(e);

            Model.IsActive = true;
            //FocusElementManager.SetFocusOnLastElement(Model);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isMouseDown && e.LeftButton == MouseButtonState.Pressed)
            {
                _draggingItem = this;
            }

            _isMouseDown = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_draggingItem != null &&
                _draggingItem != this &&
                e.LeftButton == MouseButtonState.Pressed)
            {
                Debug.WriteLine("Dragging item from {0} to {1}", _draggingItem, this);

                var model = Model;
                var container = model.Parent as ILayoutContainer;
                var containerPane = model.Parent as ILayoutPane;
                var childrenList = container.Children.ToList();
                containerPane.MoveChild(childrenList.IndexOf(_draggingItem.Model), childrenList.IndexOf(model));
            }
        }

        public override string ToString()
        {
            return string.Format("TabItem({0})", Model.Title);
            //return base.ToString();
        }

    }
}
