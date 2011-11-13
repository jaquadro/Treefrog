using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Editor.Model.Controls;

namespace Editor.A.Presentation
{
    public interface ITilePoolListPresenter
    {
        bool CanAddTilePool { get; }
        bool CanRemoveSelectedTilePool { get; }

        IEnumerable<TilePool> TilePoolList { get; }
        TilePool SelectedTilePool { get; }
        Tile SelectedTile { get; }                      // Send to ITilePoolPresenter

        event EventHandler SyncTilePoolActions;
        event EventHandler SyncTilePoolList;
        event EventHandler SyncTilePoolControl;         // Send to ITilePoolPresenter

        void ActionImportTilePool ();
        void ActionRemoveSelectedTilePool ();
        void ActionSelectTilePool (string name);
        void ActionSelectTile (Tile tile);              // Send to ITilePoolPresenter

        void RefreshTilePoolList ();
    }
}
