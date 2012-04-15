using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.ComponentModel;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
    [ContentProperty("Children")]
    [Serializable]
    public class LayoutAnchorablePane : LayoutPositionableGroup<LayoutAnchorable>, ILayoutAnchorablePane, ILayoutPositionableElement, ILayoutContentSelector, ILayoutPaneSerializable
    {
        public LayoutAnchorablePane()
        {
        }

        public LayoutAnchorablePane(LayoutAnchorable anchorable)
        {
            Children.Add(anchorable);
        }

        protected override bool GetVisibility()
        {
            return Children.Count > 0 && Children.Any(c => c.IsVisible);
        }

        #region SelectedContentIndex

        private int _selectedIndex = -1;
        public int SelectedContentIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value < 0 ||
                    value >= Children.Count)
                    value = -1;

                if (_selectedIndex != value)
                {
                    RaisePropertyChanging("SelectedContentIndex");
                    RaisePropertyChanging("SelectedContent");
                    if (_selectedIndex >= 0 &&
                        _selectedIndex < Children.Count)
                        Children[_selectedIndex].IsSelected = false;

                    _selectedIndex = value;

                    if (_selectedIndex >= 0 &&
                        _selectedIndex < Children.Count)
                        Children[_selectedIndex].IsSelected = true;

                    RaisePropertyChanged("SelectedContentIndex");
                    RaisePropertyChanged("SelectedContent");
                }
            }
        }

        public LayoutContent SelectedContent
        {
            get
            { 
                return _selectedIndex == -1 ? null : Children[_selectedIndex]; 
            }
        }
        #endregion

        protected override void OnChildrenCollectionChanged()
        {
            if (SelectedContentIndex >= ChildrenCount)
                SelectedContentIndex = Children.Count - 1;

            if (SelectedContentIndex == -1 && ChildrenCount > 0)
                SelectedContentIndex = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i].IsSelected)
                {
                    SelectedContentIndex = i;
                    break;
                }
            }

            base.OnChildrenCollectionChanged();
        }

        public int IndexOf(LayoutContent content)
        {
            var anchorableChild = content as LayoutAnchorable;
            if (anchorableChild == null)
                return -1;

            return Children.IndexOf(anchorableChild);
        }

        protected override void OnIsVisibleChanged()
        {
            UpdateParentVisibility();
            base.OnIsVisibleChanged();
        }

        void UpdateParentVisibility()
        {
            var parentPane = Parent as ILayoutElementWithVisibility;
            if (parentPane != null)
                parentPane.ComputeVisibility();
        }

        public bool IsDirectlyHostedInFloatingWindow
        {
            get
            {
                return Parent != null && Parent.ChildrenCount == 1 && Parent.Parent is LayoutFloatingWindow;
            }
        }

        protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            var oldGroup = oldValue as ILayoutGroup;
            if (oldGroup != null)
                oldGroup.ChildrenCollectionChanged -= new EventHandler(OnParentChildrenCollectionChanged);
            
            RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");

            var newGroup = newValue as ILayoutGroup;
            if (newGroup != null)
                newGroup.ChildrenCollectionChanged += new EventHandler(OnParentChildrenCollectionChanged);

            base.OnParentChanged(oldValue, newValue);
        }

        void OnParentChildrenCollectionChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("IsDirectlyHostedInFloatingWindow");
        }

        string _id;

        string ILayoutPaneSerializable.Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            if (_id != null)
                writer.WriteAttributeString("Id", _id);

            base.WriteXml(writer);
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Id"))
                _id = reader.Value;

            base.ReadXml(reader);
        }
    }
}
