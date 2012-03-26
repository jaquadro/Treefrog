using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public class LayoutPanelControl : LayoutGridControl<ILayoutPanelElement>, ILayoutControl
    {
        internal LayoutPanelControl(LayoutPanel model)
            :base(model, model.Orientation)
        {
            _model = model;
        }

        LayoutPanel _model;

        protected override void FixChildrenDockLengths()
        {
            #region Setup DockWidth/Height for children
            if (_model.Orientation == Orientation.Horizontal)
            {
                if (_model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>())
                {
                    for (int i = 0; i < _model.Children.Count; i++)
                    {
                        var childContainerModel = _model.Children[i] as ILayoutContainer;
                        var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;

                        if (childContainerModel != null &&
                            (childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
                             childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
                        {
                            childPositionableModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
                        }
                        else if (childPositionableModel != null && childPositionableModel.DockWidth.IsStar)
                        {
                            var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
                            childPositionableModel.DockWidth = new GridLength(Math.Max(childPositionableModelWidthActualSize.ActualWidth, childPositionableModel.DockMinWidth), GridUnitType.Pixel);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _model.Children.Count; i++)
                    {
                        var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;
                        if (!childPositionableModel.DockWidth.IsStar)
                        {
                            childPositionableModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
                        }
                    }
                }
            }
            else
            {
                if (_model.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>())
                {
                    for (int i = 0; i < _model.Children.Count; i++)
                    {
                        var childContainerModel = _model.Children[i] as ILayoutContainer;
                        var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;

                        if (childContainerModel != null &&
                            (childContainerModel.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
                             childContainerModel.ContainsChildOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>()))
                        {
                            childPositionableModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
                        }
                        else if (childPositionableModel != null && childPositionableModel.DockHeight.IsStar)
                        {
                            var childPositionableModelWidthActualSize = childPositionableModel as ILayoutPositionableElementWithActualSize;
                            childPositionableModel.DockHeight = new GridLength(Math.Max(childPositionableModelWidthActualSize.ActualHeight, childPositionableModel.DockMinHeight), GridUnitType.Pixel);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _model.Children.Count; i++)
                    {
                        var childPositionableModel = _model.Children[i] as ILayoutPositionableElement;
                        if (!childPositionableModel.DockHeight.IsStar)
                        {
                            childPositionableModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
                        }
                    }
                }
            }
            #endregion
        }

    }
}
