using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvalonDock.Layout
{
    public interface ILayoutUpdateStrategy
    {
        bool BeforeInsertAnchorable(
            LayoutAnchorable anchorableToShow,
            ILayoutContainer destinationContainer);

        bool InsertAnchorable(
                    LayoutAnchorable anchorableToShow,
                    ILayoutContainer destinationContainer);
    }
}
