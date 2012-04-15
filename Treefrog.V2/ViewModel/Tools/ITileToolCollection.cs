using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.V2.ViewModel.Tools
{
    public enum TileTool
    {
        Select,
        Draw,
        Erase,
        Fill,
        Stamp,
    }

    public interface ITileToolCollection : IToolCollection
    {
        bool SelectEnabled { get; }
        bool DrawEnabled { get; }
        bool EraseEnabled { get; }
        bool FillEnabled { get; }
        bool StampEnabled { get; }

        //bool FlipHEnabled { get; }
        //bool FlipVEnabled { get; }
        //bool RotateLeftEnabled { get; }
        //bool RotateRightEnabled { get; }

        TileTool SelectedTool { get; set; }
    }
}
