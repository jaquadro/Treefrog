using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AvalonDock.Layout
{
    internal interface ILayoutPositionableElement : ILayoutElement, ILayoutElementForFloatingWindow
    {
        GridLength DockWidth
        {
            get;
            set;
        }

        GridLength DockHeight
        {
            get;
            set;
        }

        double DockMinWidth { get; set; }
        double DockMinHeight { get; set; }



        bool IsVisible { get; }
    }


    internal interface ILayoutPositionableElementWithActualSize
    {
        double ActualWidth { get; set; }
        double ActualHeight { get; set; }
    }

    internal interface ILayoutElementForFloatingWindow
    {
        double FloatingWidth { get; set; }
        double FloatingHeight { get; set; }
        double FloatingLeft { get; set; }
        double FloatingTop { get; set; }
        bool IsMaximized { get; set; }
    }
}
