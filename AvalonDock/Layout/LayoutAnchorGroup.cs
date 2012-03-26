using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Markup;

namespace AvalonDock.Layout
{
    [ContentProperty("Children")]
    [Serializable]
    public class LayoutAnchorGroup : LayoutGroup<LayoutAnchorable>
    {
        public LayoutAnchorGroup()
        {
        }

        protected override bool GetVisibility()
        {
            return Children.Count > 0;
        }


        #region PreviousContainer

        private LayoutAnchorablePane _previousContainer = null;
        public LayoutAnchorablePane PreviousContainer
        {
            get { return _previousContainer; }
            internal set
            {
                if (_previousContainer != value)
                {
                    _previousContainer = value;
                    RaisePropertyChanged("PreviousContainer");
                }
            }
        }

        #endregion

    }
}
