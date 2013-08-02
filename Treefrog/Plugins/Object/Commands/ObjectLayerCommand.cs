using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Plugins.Object.Commands
{
    public abstract class ObjectLayerCommand : Command
    {
        private ObjectSelectionManager _selectionManager;

        public ObjectLayerCommand ()
        { }

        public ObjectLayerCommand (ObjectSelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
        }

        protected void SetSelectedObjects (List<ObjectInstance> objects)
        {
            if (_selectionManager != null) {
                _selectionManager.ClearSelection();
                _selectionManager.AddObjectsToSelection(objects);
            }
            
        }

        protected void AddSelectedObjects (List<ObjectInstance> objects)
        {
            if (_selectionManager != null) {
                _selectionManager.AddObjectsToSelection(objects);
            }
        }

        protected void RemoveSelectedObjects (List<ObjectInstance> objects)
        {
            if (_selectionManager != null) {
                _selectionManager.RemoveObjectsFromSelection(objects);
            }
        }
    }
}
