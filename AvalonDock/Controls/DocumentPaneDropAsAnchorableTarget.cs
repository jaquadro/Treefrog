using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal class DocumentPaneDropAsAnchorableTarget : DropTarget<LayoutDocumentPaneControl>
    {
        internal DocumentPaneDropAsAnchorableTarget(LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type)
            : base(paneControl, detectionRect, type)
        {
            _targetPane = paneControl;
        }

        internal DocumentPaneDropAsAnchorableTarget(LayoutDocumentPaneControl paneControl, Rect detectionRect, DropTargetType type, int tabIndex)
            : base(paneControl, detectionRect, type)
        {
            _targetPane = paneControl;
            _tabIndex = tabIndex;
        }


        LayoutDocumentPaneControl _targetPane;

        int _tabIndex = -1;

        protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
        {
            ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;

            switch (Type)
            {
                case DropTargetType.DocumentPaneDockAsAnchorableBottom:
                    #region DropTargetType.DocumentPaneDockAsAnchorableBottom
                    {
                        var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;

                        if (parentGroup != null)
                        {
                            var parentGroupPanel = parentGroup.Parent as LayoutPanel;
                            if (parentGroupPanel != null &&
                                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
                            {
                                parentGroupPanel.Children.Insert(
                                    parentGroupPanel.IndexOfChild(parentGroup) + 1,
                                    floatingWindow.RootPanel);
                            }
                            else
                            {
                                var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
                                parentGroupPanel.ReplaceChild(parentGroup, newParentPanel);
                                newParentPanel.Children.Add(parentGroup);
                                newParentPanel.Children.Add(floatingWindow.RootPanel);
                            }
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DocumentPaneDockAsAnchorableTop:
                    #region DropTargetType.DocumentPaneDockAsAnchorableTop
                    {
                        var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;

                        if (parentGroup != null)
                        {
                            var parentGroupPanel = parentGroup.Parent as LayoutPanel;
                            if (parentGroupPanel != null &&
                                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
                            {
                                parentGroupPanel.Children.Insert(
                                    parentGroupPanel.IndexOfChild(parentGroup),
                                    floatingWindow.RootPanel);
                            }
                            else
                            {
                                var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };
                                parentGroupPanel.ReplaceChild(parentGroup, newParentPanel);
                                newParentPanel.Children.Add(floatingWindow.RootPanel);
                                newParentPanel.Children.Add(parentGroup);
                            }
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DocumentPaneDockAsAnchorableLeft:
                    #region DropTargetType.DocumentPaneDockAsAnchorableLeft
                    {
                        var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;

                        if (parentGroup != null)
                        {
                            var parentGroupPanel = parentGroup.Parent as LayoutPanel;
                            if (parentGroupPanel != null &&
                                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
                            {
                                parentGroupPanel.Children.Insert(
                                    parentGroupPanel.IndexOfChild(parentGroup),
                                    floatingWindow.RootPanel);
                            }
                            else
                            {
                                var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
                                parentGroupPanel.ReplaceChild(parentGroup, newParentPanel);
                                newParentPanel.Children.Add(floatingWindow.RootPanel);
                                newParentPanel.Children.Add(parentGroup);
                            }
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DocumentPaneDockAsAnchorableRight:
                    #region DropTargetType.DocumentPaneDockAsAnchorableRight
                    {
                        var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;

                        if (parentGroup != null)
                        {
                            var parentGroupPanel = parentGroup.Parent as LayoutPanel;
                            if (parentGroupPanel != null &&
                                parentGroupPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
                            {
                                parentGroupPanel.Children.Insert(
                                    parentGroupPanel.IndexOfChild(parentGroup) + 1,
                                    floatingWindow.RootPanel);
                            }
                            else
                            {
                                var newParentPanel = new LayoutPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal };
                                parentGroupPanel.ReplaceChild(parentGroup, newParentPanel);
                                newParentPanel.Children.Add(parentGroup);
                                newParentPanel.Children.Add(floatingWindow.RootPanel);
                            }
                        }
                    }
                    break;
                    #endregion
            }

            base.Drop(floatingWindow);
        }

        public override System.Windows.Media.Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel)
        {
            ILayoutDocumentPane targetModel = _targetPane.Model as ILayoutDocumentPane;
            var parentGroup = targetModel.Parent as LayoutDocumentPaneGroup;
            var manager = targetModel.Root.Manager;
            var documentPaneGroupControl = manager.FindLogicalChildren<LayoutDocumentPaneGroupControl>().First(d => d.Model == parentGroup);

            switch (Type)
            {
                case DropTargetType.DocumentPaneDockAsAnchorableBottom:
                    {
                        var targetScreenRect = documentPaneGroupControl.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
                        targetScreenRect.Offset(0.0, targetScreenRect.Height - targetScreenRect.Height / 3.0);
                        targetScreenRect.Height /= 3.0;
                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.DocumentPaneDockAsAnchorableTop:
                    {
                        var targetScreenRect = documentPaneGroupControl.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
                        targetScreenRect.Height /= 3.0;
                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.DocumentPaneDockAsAnchorableRight:
                    {
                        var targetScreenRect = documentPaneGroupControl.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
                        targetScreenRect.Offset(targetScreenRect.Width - targetScreenRect.Width / 3.0, 0.0);
                        targetScreenRect.Width /= 3.0;
                        return new RectangleGeometry(targetScreenRect);
                    }
                case DropTargetType.DocumentPaneDockAsAnchorableLeft:
                    {
                        var targetScreenRect = documentPaneGroupControl.GetScreenArea();
                        targetScreenRect.Offset(-overlayWindow.Left, -overlayWindow.Top);
                        targetScreenRect.Width /= 3.0;
                        return new RectangleGeometry(targetScreenRect);
                    }
            }

            return null;
        }

    }
}
