using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Layers
{
    public class GridLayerPresenter : LayerPresenter
    {
        public int GridSpacingX { get; set; }
        public int GridSpacingY { get; set; }

        public Color GridColor { get; set; }
    }
}
