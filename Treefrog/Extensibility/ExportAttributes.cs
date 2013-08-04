using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Render.Layers;

namespace Treefrog.Extensibility
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method)]
    public class LevelLayerPresenterExportAttribute : ExportAttribute
    {
        public LevelLayerPresenterExportAttribute ()
            : base(typeof(Func<Layer, ILayerContext, LevelLayerPresenter>))
        { }

        public Type LayerType { get; set; }
        public Type TargetType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method)]
    public class LayerFromPresenterExportAttribute : ExportAttribute
    {
        public LayerFromPresenterExportAttribute ()
            : base(typeof(Func<LayerPresenter, string, Layer>))
        { }

        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method)]
    public class CanvasLayerExportAttribute : ExportAttribute
    {
        public CanvasLayerExportAttribute ()
            : base(typeof(Func<LayerPresenter, CanvasLayer>))
        { }

        public Type LayerType { get; set; }
        public Type TargetType { get; set; }
    }
}
