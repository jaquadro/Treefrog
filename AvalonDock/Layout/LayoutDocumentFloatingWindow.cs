using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Diagnostics;

namespace AvalonDock.Layout
{
    [ContentProperty("RootDocument")]
    [Serializable]
    public class LayoutDocumentFloatingWindow : LayoutFloatingWindow
    {
        public LayoutDocumentFloatingWindow()
        {

        }

        #region RootDocument

        private LayoutDocument _rootDocument = null;
        public LayoutDocument RootDocument
        {
            get { return _rootDocument; }
            set
            {
                if (_rootDocument != value)
                {
                    RaisePropertyChanging("RootDocument");
                    _rootDocument = value;
                    if (_rootDocument != null)
                        _rootDocument.Parent = this;
                    RaisePropertyChanged("RootDocument");

                    if (RootDocumentChanged != null)
                        RootDocumentChanged(this, EventArgs.Empty);
                }
            }
        }


        public event EventHandler RootDocumentChanged;

        #endregion

        public override IEnumerable<ILayoutElement> Children
        {
            get { yield return RootDocument; }
        }

        public override void RemoveChild(ILayoutElement element)
        {
            Debug.Assert(element == RootDocument && element != null);
            RootDocument = null;
        }

        public override int ChildrenCount
        {
            get { return 1; }
        }
    }

}
