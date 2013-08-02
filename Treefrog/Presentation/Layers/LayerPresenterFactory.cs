using Treefrog.Framework.Model;
using Treefrog.Utility;
using Treefrog.Plugins.Object.Layers;

namespace Treefrog.Presentation.Layers
{
    public class LayerPresenterFactory : DependentTypeFactory<Layer, LevelLayerPresenter, ILayerContext>
    {
        public static LayerPresenterFactory Default { get; private set; }

        static LayerPresenterFactory ()
        {
            Default = new LayerPresenterFactory();

            Default.Register<TileLayer, TileLayerPresenter>((layer, context) => {
                return new TileLayerPresenter(context, layer as TileLayer);
            });
            Default.Register<TileGridLayer, TileGridLayerPresenter>((layer, context) => {
                return new TileGridLayerPresenter(context, layer as TileGridLayer);
            });
            Default.Register<MultiTileGridLayer, TileGridLayerPresenter>((layer, context) => {
                return new TileGridLayerPresenter(context, layer as TileGridLayer);
            });
            //Default.Register<ObjectLayer, ObjectLayerPresenter>((layer, context) => {
            //    return new ObjectLayerPresenter(context, layer as ObjectLayer);
            //});
        }
    }
}
