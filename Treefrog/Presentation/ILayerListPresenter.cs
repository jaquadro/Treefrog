using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;

namespace Treefrog.Presentation
{
    public enum LayerVisibility {
        Hide,
        Show,
    }

    public interface ILayerListPresenter : ICommandSubscriber
    {
        IEnumerable<LevelLayerPresenter> LayerList { get; }
        LevelLayerPresenter SelectedLayer { get; }

        event EventHandler SyncLayerList;
        event EventHandler SyncLayerSelection;

        void ActionSelectLayer (string name);
        void ActionShowHideLayer (string name, LayerVisibility visibility);
    }
}
