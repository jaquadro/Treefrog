using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Layers
{
    public struct DrawCommand
    {
        public string Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Rectangle DestRect { get; set; }
        public Color BlendColor { get; set; }
    }

    public abstract class RenderLayerPresenter : LayerPresenter
    {
        public abstract IEnumerable<DrawCommand> RenderCommands { get; }
    }
}
