using Treefrog.Framework.Model;
using Treefrog.Utility;

namespace Treefrog.Presentation.Layers
{
    public class LayerPresenterFactory : DependentTypeFactory<Layer, LevelLayerPresenter, ILayerContext>
    {
        public static LayerPresenterFactory Default { get; private set; }

        static LayerPresenterFactory ()
        {
            Default = new LayerPresenterFactory();

            //Default.Register<TileLayer, TileLayerPresenter>((layer, context) => {
            //    return new TileLayerPresenter(context, layer as TileLayer);
            //});
            //Default.Register<TileGridLayer, TileGridLayerPresenter>((layer, context) => {
            //    return new TileGridLayerPresenter(context, layer as TileGridLayer);
            //});
            //Default.Register<MultiTileGridLayer, TileGridLayerPresenter>((layer, context) => {
            //    return new TileGridLayerPresenter(context, layer as TileGridLayer);
            //});
            //Default.Register<ObjectLayer, ObjectLayerPresenter>((layer, context) => {
            //    return new ObjectLayerPresenter(context, layer as ObjectLayer);
            //});
        }
    }

    public class LayerFromPresenterFactory : DependentTypeFactory<LevelLayerPresenter, Layer, string>
    {
        public static LayerFromPresenterFactory Default { get; private set; }

        static LayerFromPresenterFactory ()
        {
            Default = new LayerFromPresenterFactory();

            //Default.Register<TileGridLayerPresenter, MultiTileGridLayer>((layer, name) => {
            //    return new MultiTileGridLayer(name, layer.Layer as MultiTileGridLayer);
            //});
        }
    }
}
