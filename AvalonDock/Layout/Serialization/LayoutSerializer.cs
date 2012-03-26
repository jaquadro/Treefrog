using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AvalonDock.Controls;
using System.Windows;

namespace AvalonDock.Layout.Serialization
{
    public abstract class LayoutSerializer
    {
        DockingManager _manager;

        public LayoutSerializer(DockingManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _manager = manager;
        }

        public DockingManager Manager
        {
            get { return _manager; }
        }

        public event EventHandler<LayoutSerializationCallbackEventArgs> LayoutSerializationCallback;

        protected virtual void FixupLayout(LayoutRoot layout)
        {
            //fix container panes
            foreach (var lcToAttach in layout.Descendents().OfType<LayoutContent>().Where(lc => lc.PreviousContainerId != null))
            {
                var paneContainerToAttach = layout.Descendents().OfType<ILayoutPaneSerializable>().FirstOrDefault(lps => lps.Id == lcToAttach.PreviousContainerId);
                if (paneContainerToAttach == null)
                    throw new ArgumentException(string.Format("Unable to find a pane with id ='{0}'", lcToAttach.PreviousContainerId));

                lcToAttach.PreviousContainer = paneContainerToAttach as ILayoutPane;
            }

            //now fix the content of the layoutcontents
            foreach (var lcToFix in layout.Descendents().OfType<LayoutContent>().Where(lc => lc.Content == null))
            {
                if (LayoutSerializationCallback != null)
                { 
                    var args = new LayoutSerializationCallbackEventArgs(lcToFix);
                    LayoutSerializationCallback(this, args);
                    lcToFix.Content = args.Content;
                }
            }

        }

        protected void StartDeserialization()
        {
            Manager.SuspendDocumentsSourceBinding = true;
        }

        protected void EndDeserialization()
        {
            Manager.SuspendDocumentsSourceBinding = false;
        }
    }
}
