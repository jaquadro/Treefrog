using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    public class LayoutDocumentPaneGroupControl : LayoutGridControl<ILayoutDocumentPane>, ILayoutControl
    {
        internal LayoutDocumentPaneGroupControl(LayoutDocumentPaneGroup model)
            :base(model, model.Orientation)
        {
            _model = model;
        }

        LayoutDocumentPaneGroup _model;

        protected override void FixChildrenDockLengths()
        {
            #region Setup DockWidth/Height for children
            if (_model.Orientation == Orientation.Horizontal)
            {
                for (int i = 0; i < _model.Children.Count; i++)
                {
                    var childModel = _model.Children[i] as ILayoutPositionableElement;
                    if (!childModel.DockWidth.IsStar)
                    {
                        childModel.DockWidth = new GridLength(1.0, GridUnitType.Star);
                    }
                }
            }
            else
            {
                for (int i = 0; i < _model.Children.Count; i++)
                {
                    var childModel = _model.Children[i] as ILayoutPositionableElement;
                    if (!childModel.DockHeight.IsStar)
                    {
                        childModel.DockHeight = new GridLength(1.0, GridUnitType.Star);
                    }
                }
            }
            #endregion
        }

    }
}
