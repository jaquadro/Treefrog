using System;
using System.Collections.Generic;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel.Commands
{
    public abstract class ObjectLayerCommand : Command
    {
        private WeakReference _toolRef;

        public ObjectLayerCommand ()
        { }

        public ObjectLayerCommand (ObjectSelectTool selectTool)
        {
            if (selectTool != null)
                _toolRef = new WeakReference(selectTool);
        }

        protected void SetSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.SelectObjects(objects);
                }
            }
        }

        protected void AddSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.AddObjectsToSelection(objects);
                }
            }
        }

        protected void RemoveSelectedObjects (List<ObjectInstance> objects)
        {
            if (_toolRef != null) {
                ObjectSelectTool tool = _toolRef.Target as ObjectSelectTool;
                if (tool != null && !tool.IsCancelled) {
                    tool.RemoveObjectsFromSelection(objects);
                }
            }
        }
    }
}
