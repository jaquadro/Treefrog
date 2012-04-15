using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace AvalonDock.Layout
{
    [Serializable]
    public class LayoutAnchorable : LayoutContent
    {
        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    RaisePropertyChanging("IsVisible");
                    _isVisible = value;
                    UpdateParentVisibility();
                    RaisePropertyChanged("IsVisible");
                }
                    
            }
        }

        protected override void OnParentChanged(ILayoutContainer oldValue, ILayoutContainer newValue)
        {
            UpdateParentVisibility(); 
            base.OnParentChanged(oldValue, newValue);
        }

        void UpdateParentVisibility()
        {
            var parentPane = Parent as ILayoutElementWithVisibility;
            if (parentPane != null)
                parentPane.ComputeVisibility();
        }

        #endregion

        #region AutoHideWidth

        private double _autohideWidth = 0.0;
        public double AutoHideWidth
        {
            get { return _autohideWidth; }
            set
            {
                if (_autohideWidth != value)
                {
                    RaisePropertyChanging("AutoHideWidth");
                    _autohideWidth = value;
                    RaisePropertyChanged("AutoHideWidth");
                }
            }
        }

        #endregion

        #region AutoHideMinWidth

        private double _autohideMinWidth = 25.0;
        public double AutoHideMinWidth
        {
            get { return _autohideMinWidth; }
            set
            {
                if (_autohideMinWidth != value)
                {
                    RaisePropertyChanging("AutoHideMinWidth");
                    _autohideMinWidth = value;
                    RaisePropertyChanged("AutoHideMinWidth");
                }
            }
        }

        #endregion

        #region AutoHideHeight

        private double _autohideHeight = 0.0;
        public double AutoHideHeight
        {
            get { return _autohideHeight; }
            set
            {
                if (_autohideHeight != value)
                {
                    RaisePropertyChanging("AutoHideHeight");
                    _autohideHeight = value;
                    RaisePropertyChanged("AutoHideHeight");
                }
            }
        }

        #endregion

        #region AutoHideMinHeight

        private double _autohideMinHeight = 0.0;
        public double AutoHideMinHeight
        {
            get { return _autohideMinHeight; }
            set
            {
                if (_autohideMinHeight != value)
                {
                    RaisePropertyChanging("AutoHideMinHeight");
                    _autohideMinHeight = value;
                    RaisePropertyChanged("AutoHideMinHeight");
                }
            }
        }

        #endregion
        
        /// <summary>
        /// Hide this contents
        /// </summary>
        /// <remarks>Add this content to <see cref="ILayoutRoot.Hidden"/> collection of parent root.</remarks>
        public void Hide()
        {
            if (IsHidden)
            {
                IsSelected = true;
                IsActive = true;
                return;
            }
            RaisePropertyChanging("IsHidden");
            if (Parent is ILayoutPane)
            {
                PreviousContainer = Parent as ILayoutPane;
                PreviousContainerIndex = (Parent as ILayoutContentSelector).SelectedContentIndex;
            }
            Root.Hidden.Add(this);
            RaisePropertyChanged("IsHidden");
        }

        /// <summary>
        /// Show the content
        /// </summary>
        /// <remarks>Try to show the content where it was previously hidden.</remarks>
        public void Show()
        {
            if (!IsHidden)
                return;

            RaisePropertyChanging("IsHidden");

            bool added = false;
            var root = Root;
            if (root != null && root.Manager != null)
            {
                if (root.Manager.LayoutUpdateStrategy != null)
                    added = root.Manager.LayoutUpdateStrategy.BeforeInsertAnchorable(this, PreviousContainer);
            }

            if (!added && PreviousContainer != null)
            {
                var previousContainerAsLayoutGroup = PreviousContainer as ILayoutGroup;
                previousContainerAsLayoutGroup.InsertChildAt(PreviousContainerIndex, this);
                IsSelected = true;
                IsActive = true;
            }
            
            if (!added && root != null && root.Manager != null)
            {
                if (root.Manager.LayoutUpdateStrategy != null)
                    root.Manager.LayoutUpdateStrategy.InsertAnchorable(this, PreviousContainer);
            }
            

            RaisePropertyChanged("IsHidden");
        }

        [XmlIgnore]
        public bool IsHidden
        {
            get
            {
                return Parent is LayoutRoot;
            }
        }
    }
}
