using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using AvalonDock.Layout;
using Treefrog.ViewModel;

namespace Treefrog.View
{
    public class PanelsStyleSelector : StyleSelector
    {
        public Style LevelDocumentStyle { get; set; }

        public Style LayerPanelStyle { get; set; }

        public Style ObjectPanelStyle { get; set; }

        public Style TilePoolPanelStyle { get; set; }

        public Style PropertyPanelStyle { get; set; }

        public override Style SelectStyle (object item, DependencyObject container)
        {
            if (item is LevelDocumentVM)
                return LevelDocumentStyle;

            if (item is LayerCollectionVM)
                return LayerPanelStyle;

            if (item is ObjectPoolCollectionVM)
                return ObjectPanelStyle;

            if (item is TilePoolCollectionVM)
                return TilePoolPanelStyle;

            if (item is PropertyCollectionVM)
                return PropertyPanelStyle;

            return base.SelectStyle(item, container);
        }
    }
}
