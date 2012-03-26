using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;

namespace AvalonDock.Layout
{
    public static class Extentions
    {
        public static IEnumerable<ILayoutElement> Descendents(this ILayoutElement element)
        {
            var container = element as ILayoutContainer;
            if (container != null)
            {
                foreach (var childElement in container.Children)
                {
                    yield return childElement;
                    foreach (var childChildElement in childElement.Descendents())
                        yield return childChildElement;
                }
            }
        }

        public static T FindParent<T>(this ILayoutElement element) where T : ILayoutContainer
        { 
            var parent = element.Parent;
            while (parent != null &&
                !(parent is T))
                parent = parent.Parent;


            return (T)parent;
        }

        public static bool ContainsChildOfType<T>(this ILayoutContainer element)
        {
            foreach (var childElement in element.Descendents())
                if (childElement is T)
                    return true;

            return false;
        }

        public static bool ContainsChildOfType<T, S>(this ILayoutContainer container)
        {
            foreach (var childElement in container.Descendents())
                if (childElement is T || childElement is S)
                    return true;

            return false;
        }

        public static bool IsOfType<T, S>(this ILayoutContainer container)
        {
            return container is T || container is S;
        }

        public static AnchorSide GetSide(this ILayoutElement element)
        {
            var parentContainer = element.Parent as ILayoutOrientableGroup;
            if (parentContainer != null)
            {
                if (!parentContainer.ContainsChildOfType<LayoutDocumentPaneGroup, LayoutDocumentPane>())
                    return GetSide(parentContainer);

                foreach (var childElement in parentContainer.Children)
                {
                    if (childElement == element)
                        return parentContainer.Orientation == System.Windows.Controls.Orientation.Horizontal ?
                            AnchorSide.Left : AnchorSide.Top;

                    var childElementAsContainer = childElement as ILayoutContainer;
                    if (childElementAsContainer != null &&
                        (childElementAsContainer.IsOfType<LayoutDocumentPane, LayoutDocumentPaneGroup>() ||
                        childElementAsContainer.ContainsChildOfType<LayoutDocumentPane, LayoutAnchorablePaneGroup>()))
                    {
                        return parentContainer.Orientation == System.Windows.Controls.Orientation.Horizontal ?
                           AnchorSide.Right : AnchorSide.Bottom;
                    }
                }
            }

            Debug.Fail("Unable to find the side for an element, possible layout problem!");
            return AnchorSide.Right;
        }
    }
}
