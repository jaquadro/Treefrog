using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Editor.A.Presentation
{
    public interface ILayerListPresenter
    {
        bool CanAddLayer { get; }
        bool CanRemoveSelectedLayer { get; }
        bool CanCloneSelectedLayer { get; }
        bool CanMoveSelectedLayerUp { get; }
        bool CanMoveSelectedLayerDown { get; }
        bool CanShowSelectedLayerProperties { get; }

        IEnumerable<Layer> LayerList { get; }
        Layer SelectedLayer { get; }

        event EventHandler SyncLayerActions;
        event EventHandler SyncLayerList;
        event EventHandler SyncLayerSelection;

        void ActionAddLayer ();
        void ActionRemoveSelectedLayer ();
        void ActionCloneSelectedLayer ();
        void ActionMoveSelectedLayerUp ();
        void ActionMoveSelectedLayerDown ();
        void ActionSelectLayer (string name);
        void ActionShowSelectedLayerProperties ();

        void RefreshLayerList ();
    }
}
