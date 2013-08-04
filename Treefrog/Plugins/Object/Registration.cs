using Treefrog.Extensibility;
using Treefrog.Framework.Model;
using Treefrog.Plugins.Object.Layers;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Render.Layers;

namespace Treefrog.Plugins.Object
{
    public static class Registration
    {
        [LevelLayerPresenterExport(LayerType = typeof(ObjectLayer), TargetType = typeof(ObjectLayerPresenter))]
        public static LevelLayerPresenter CreateObjectLayerPresenter (Layer layer, ILayerContext context)
        {
            return new ObjectLayerPresenter(context, layer as ObjectLayer);
        }

        [LayerFromPresenterExport(SourceType = typeof(ObjectLayerPresenter), TargetType = typeof(ObjectLayer))]
        public static Layer CreateObjectLayerPresenter (LayerPresenter layer, string name)
        {
            return new ObjectLayer(name, (layer as ObjectLayerPresenter).Layer as ObjectLayer);
        }

        [CanvasLayerExport(LayerType = typeof(ObjectLayerPresenter), TargetType = typeof(LevelRenderLayer))]
        public static CanvasLayer CreateObjectCanvasLayer (LayerPresenter layer) 
        {
            return new LevelRenderLayer(layer as LevelLayerPresenter);
        }
    }
}
