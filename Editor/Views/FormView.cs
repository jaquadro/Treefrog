using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
    public interface IFormView
    {
        Control Control { get; }
        float Zoom { get; set; }

        void Display ();

        // Events

        event EventHandler<ClipboardEventArgs> ClipboardChanged;
        event EventHandler<CommandHistoryEventArgs> CommandHistoryChanged;

        // Toolbar Handlers

        void Undo ();
        void Redo ();

        void Copy ();
        void Cut ();
        void Paste ();
    }

    public interface ITileToolbarSubscriber
    {
        TileToolMode TileToolMode { get; set; }
    }
}
