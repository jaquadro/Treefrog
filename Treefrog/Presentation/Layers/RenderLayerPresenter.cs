using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Layers
{
    public struct DrawCommand
    {
        public Guid Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Rectangle DestRect { get; set; }
        public Color BlendColor { get; set; }
        public float Rotation { get; set; }
        public float OriginX { get; set; }
        public float OriginY { get; set; }
    }

    public abstract class RenderLayerPresenter : LayerPresenter
    {
        
    }
}
