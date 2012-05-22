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
    public class PanelsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LevelDocumentTemplate { get; set; }

        public DataTemplate LayerPanelTemplate { get; set; }

        public DataTemplate ObjectPanelTemplate { get; set; }

        public DataTemplate TilePoolPanelTemplate { get; set; }

        public DataTemplate PropertyPanelTemplate { get; set; }

        public override DataTemplate SelectTemplate (object item, DependencyObject container)
        {
            if (item is LevelDocumentVM)
                return LevelDocumentTemplate;

            if (item is LayerCollectionVM)
                return LayerPanelTemplate;

            if (item is ObjectPoolCollectionVM)
                return ObjectPanelTemplate;

            if (item is TilePoolCollectionVM)
                return TilePoolPanelTemplate;

            if (item is PropertyCollectionVM)
                return PropertyPanelTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
