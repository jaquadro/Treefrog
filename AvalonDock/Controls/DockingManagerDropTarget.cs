using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal class DockingManagerDropTarget : DropTarget<DockingManager>
    {
        internal DockingManagerDropTarget(DockingManager manager, Rect detectionRect, DropTargetType type)
            : base(manager, detectionRect, type)
        {
            _manager = manager;
        }

        DockingManager _manager;

        protected override void Drop(LayoutAnchorableFloatingWindow floatingWindow)
        {
            switch (Type)
            {
                case DropTargetType.DockingManagerDockLeft:
                    #region DropTargetType.DockingManagerDockLeft
                    {
                        if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                            _manager.Layout.RootPanel.Children.Count == 1)
                            _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

                        if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)
                            { 
                                for (int i = 0; i < layoutAnchorablePaneGroup.Children.Count; i++)
                                    _manager.Layout.RootPanel.Children.Insert(i, layoutAnchorablePaneGroup.Children[i]);
                            }
                            else
                                _manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutPanel()
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal
                            };

                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);
                            newOrientedPanel.Children.Add(_manager.Layout.RootPanel);

                            _manager.Layout.RootPanel = newOrientedPanel;
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DockingManagerDockRight:
                    #region DropTargetType.DockingManagerDockRight
                    {
                        if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Horizontal &&
                            _manager.Layout.RootPanel.Children.Count == 1)
                            _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Horizontal;

                        if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Horizontal)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Horizontal)
                            {
                                for (int i = 0; i < layoutAnchorablePaneGroup.Children.Count; i++)
                                    _manager.Layout.RootPanel.Children.Add(layoutAnchorablePaneGroup.Children[i]);
                            }
                            else
                                _manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutPanel()
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal
                            };

                            newOrientedPanel.Children.Add(_manager.Layout.RootPanel);
                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);

                            _manager.Layout.RootPanel = newOrientedPanel;
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DockingManagerDockTop:
                    #region DropTargetType.DockingManagerDockTop
                    {
                        if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
                            _manager.Layout.RootPanel.Children.Count == 1)
                            _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

                        if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)
                            {
                                for (int i = 0; i < layoutAnchorablePaneGroup.Children.Count; i++)
                                    _manager.Layout.RootPanel.Children.Insert(i, layoutAnchorablePaneGroup.Children[i]);
                            }
                            else
                                _manager.Layout.RootPanel.Children.Insert(0, floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutPanel()
                            {
                                Orientation = System.Windows.Controls.Orientation.Vertical
                            };

                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);
                            newOrientedPanel.Children.Add(_manager.Layout.RootPanel);

                            _manager.Layout.RootPanel = newOrientedPanel;
                        }
                    }
                    break;
                    #endregion
                case DropTargetType.DockingManagerDockBottom:
                    #region DropTargetType.DockingManagerDockBottom
                    {
                        if (_manager.Layout.RootPanel.Orientation != System.Windows.Controls.Orientation.Vertical &&
                            _manager.Layout.RootPanel.Children.Count == 1)
                            _manager.Layout.RootPanel.Orientation = System.Windows.Controls.Orientation.Vertical;

                        if (_manager.Layout.RootPanel.Orientation == System.Windows.Controls.Orientation.Vertical)
                        {
                            var layoutAnchorablePaneGroup = floatingWindow.RootPanel as LayoutAnchorablePaneGroup;
                            if (layoutAnchorablePaneGroup != null &&
                                layoutAnchorablePaneGroup.Orientation == System.Windows.Controls.Orientation.Vertical)
                            {
                                for (int i = 0; i < layoutAnchorablePaneGroup.Children.Count; i++)
                                    _manager.Layout.RootPanel.Children.Add(layoutAnchorablePaneGroup.Children[i]);
                            }
                            else
                                _manager.Layout.RootPanel.Children.Add(floatingWindow.RootPanel);
                        }
                        else
                        {
                            var newOrientedPanel = new LayoutPanel()
                            {
                                Orientation = System.Windows.Controls.Orientation.Vertical
                            };

                            newOrientedPanel.Children.Add(_manager.Layout.RootPanel);
                            newOrientedPanel.Children.Add(floatingWindow.RootPanel);

                            _manager.Layout.RootPanel = newOrientedPanel;
                        }
                    }
                    break;
                    #endregion
            }


            base.Drop(floatingWindow);
        }

        public override System.Windows.Media.Geometry GetPreviewPath(OverlayWindow overlayWindow, LayoutFloatingWindow floatingWindowModel)
        {
            var anchorableFloatingWindowModel = floatingWindowModel as LayoutAnchorableFloatingWindow;
            var layoutAnchorablePane = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElement;
            var layoutAnchorablePaneWithActualSize = anchorableFloatingWindowModel.RootPanel as ILayoutPositionableElementWithActualSize;

            var targetScreenRect = TargetElement.GetScreenArea();

            switch (Type)
            {
                case DropTargetType.DockingManagerDockLeft:
                    {
                        var previewBoxRect = new Rect(
                            targetScreenRect.Left - overlayWindow.Left,
                            targetScreenRect.Top - overlayWindow.Top,
                            layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth,
                            targetScreenRect.Height);

                        return new RectangleGeometry(previewBoxRect);
                    }
                case DropTargetType.DockingManagerDockTop:
                    {
                        var previewBoxRect = new Rect(
                            targetScreenRect.Left - overlayWindow.Left,
                            targetScreenRect.Top - overlayWindow.Top,
                            targetScreenRect.Width,
                            layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight);

                        return new RectangleGeometry(previewBoxRect);
                    }
                case DropTargetType.DockingManagerDockRight:
                    {
                        var previewBoxRect = new Rect(
                            targetScreenRect.Right - overlayWindow.Left - (layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth),
                            targetScreenRect.Top - overlayWindow.Top,
                            layoutAnchorablePane.DockWidth.IsAbsolute ? layoutAnchorablePane.DockWidth.Value : layoutAnchorablePaneWithActualSize.ActualWidth,
                            targetScreenRect.Height);

                        return new RectangleGeometry(previewBoxRect);
                    }
                case DropTargetType.DockingManagerDockBottom:
                    {
                        var previewBoxRect = new Rect(
                            targetScreenRect.Left - overlayWindow.Left,
                            targetScreenRect.Bottom - overlayWindow.Top - (layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight),
                            targetScreenRect.Width,
                            layoutAnchorablePane.DockHeight.IsAbsolute ? layoutAnchorablePane.DockHeight.Value : layoutAnchorablePaneWithActualSize.ActualHeight);

                        return new RectangleGeometry(previewBoxRect);
                    }
            }


            throw new InvalidOperationException();
        }
    }
}
