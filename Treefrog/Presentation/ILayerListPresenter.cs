using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public enum LayerVisibility {
        Hide,
        Show,
    }

    public interface ILayerListPresenter : ICommandSubscriber
    {
        //bool CanAddLayer { get; }
        //bool CanRemoveSelectedLayer { get; }
        //bool CanCloneSelectedLayer { get; }
        //bool CanMoveSelectedLayerUp { get; }
        //bool CanMoveSelectedLayerDown { get; }
        bool CanShowSelectedLayerProperties { get; }

        IEnumerable<Layer> LayerList { get; }
        Layer SelectedLayer { get; }

        event EventHandler SyncLayerActions;
        event EventHandler SyncLayerList;
        event EventHandler SyncLayerSelection;

        //void ActionAddLayer ();
        //void ActionRemoveSelectedLayer ();
        //void ActionCloneSelectedLayer ();
        //void ActionMoveSelectedLayerUp ();
        //void ActionMoveSelectedLayerDown ();
        void ActionSelectLayer (string name);
        void ActionShowSelectedLayerProperties ();
        void ActionShowHideLayer (string name, LayerVisibility visibility);

        void RefreshLayerList ();
    }
}
