using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Diagnostics;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
    [Serializable]
    [ContentProperty("RootPanel")]
    public class LayoutAnchorableFloatingWindow : LayoutFloatingWindow, ILayoutElementWithVisibility
    {
        public LayoutAnchorableFloatingWindow()
        { 
        
        }

        #region RootPanel

        private LayoutAnchorablePaneGroup _rootPanel = null;
        public LayoutAnchorablePaneGroup RootPanel
        {
            get { return _rootPanel; }
            set
            {
                if (_rootPanel != value)
                {
                    RaisePropertyChanging("RootPanel");

                    if (_rootPanel != null)
                        _rootPanel.ChildrenCollectionChanged -= new EventHandler(_rootPanel_ChildrenCollectionChanged);

                    _rootPanel = value;
                    if (_rootPanel != null)
                        _rootPanel.Parent = this;

                    if (_rootPanel != null)
                        _rootPanel.ChildrenCollectionChanged += new EventHandler(_rootPanel_ChildrenCollectionChanged);

                    RaisePropertyChanged("RootPanel");
                    RaisePropertyChanged("IsSinglePane");
                    RaisePropertyChanged("SinglePane");

                    ((ILayoutElementWithVisibility)this).ComputeVisibility();
                }
            }
        }

        void _rootPanel_ChildrenCollectionChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsSinglePane");
            RaisePropertyChanged("SinglePane");
        }

        public bool IsSinglePane
        {
            get
            {
                //Debug.WriteLine("IsSinglePane={0}", RootPanel != null && RootPanel.ChildrenCount == 1);
                return RootPanel != null && RootPanel.ChildrenCount == 1;
            }
        }

        public ILayoutAnchorablePane SinglePane
        {
            get
            {
                return IsSinglePane ? RootPanel.Children[0] : null;
            }
        }

        #endregion

        public override IEnumerable<ILayoutElement> Children
        {
            get { yield return RootPanel; }
        }

        public override void RemoveChild(ILayoutElement element)
        {
            Debug.Assert(element == RootPanel && element != null);
            RootPanel = null;
        }

        public override void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement)
        {
            Debug.Assert(oldElement == RootPanel && oldElement != null);
            RootPanel = newElement as LayoutAnchorablePaneGroup;
        }

        public override int ChildrenCount
        {
            get { return 1; }
        }

        #region IsVisible
        [NonSerialized]
        private bool _isVisible = true;

        [XmlIgnore]
        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (_isVisible != value)
                {
                    RaisePropertyChanging("IsVisible");
                    _isVisible = value;
                    RaisePropertyChanged("IsVisible");
                    if (IsVisibleChanged != null)
                        IsVisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler IsVisibleChanged;

        #endregion

       
        void ILayoutElementWithVisibility.ComputeVisibility()
        {
            if (RootPanel != null)
                IsVisible = RootPanel.IsVisible;
        }
    }
}
