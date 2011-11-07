using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Editor.A.Presentation
{
    public interface ITilePoolListPresenter
    {
        bool CanAddTilePool { get; }
        bool CanRemoveSelectedTilePool { get; }

        IEnumerable<TilePool> TilePoolList { get; }
        TilePool SelectedTilePool { get; }

        event EventHandler SyncTilePoolActions;
        event EventHandler SyncTilePoolList;

        void ActionImportTilePool ();
        void ActionRemoveSelectedTilePool ();
        void ActionSelectTilePool (string name);

        void RefreshTilePoolList ();
    }
}
