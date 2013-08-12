using Treefrog.Extensibility;
using Treefrog.Framework.Model;
using Treefrog.Plugins.Tiles.Layers;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Render.Layers;

namespace Treefrog.Plugins.Tiles
{
    public static class Registration
    {
        // Layer Presenter Creation

        [LevelLayerPresenterExport(LayerType = typeof(TileLayer), TargetType = typeof(TileLayerPresenter))]
        public static LevelLayerPresenter CreateTileLayerPresenter1 (Layer layer, ILayerContext context)
        {
            return new TileLayerPresenter(context, layer as TileLayer);
        }

        [LevelLayerPresenterExport(LayerType = typeof(TileGridLayer), TargetType = typeof(TileGridLayerPresenter))]
        public static LevelLayerPresenter CreateTileLayerPresenter2 (Layer layer, ILayerContext context)
        {
            return new TileGridLayerPresenter(context, layer as TileGridLayer);
        }

        [LevelLayerPresenterExport(LayerType = typeof(MultiTileGridLayer), TargetType = typeof(TileGridLayerPresenter))]
        public static LevelLayerPresenter CreateTileLayerPresenter3 (Layer layer, ILayerContext context)
        {
            return new TileGridLayerPresenter(context, layer as TileGridLayer);
        }

        // Layer Creation

        [LayerFromPresenterExport(SourceType = typeof(TileGridLayerPresenter), TargetType = typeof(MultiTileGridLayer))]
        public static Layer CreateTileLayer (LayerPresenter layer, string name)
        {
            return new MultiTileGridLayer(name, (layer as TileGridLayerPresenter).Layer as MultiTileGridLayer);
        }

        // Canvas Layer Creation

        [CanvasLayerExport(LayerType = typeof(TileLayerPresenter), TargetType = typeof(LevelRenderLayer))]
        public static CanvasLayer CreateTileCanvasLayer1 (LayerPresenter layer) 
        {
            return new LevelRenderLayer(layer as LevelLayerPresenter);
        }

        [CanvasLayerExport(LayerType = typeof(TileSetLayerPresenter), TargetType = typeof(TileSetRenderLayer))]
        public static CanvasLayer CreateTileCanvasLayer2 (LayerPresenter layer)
        {
            return new TileSetRenderLayer(layer as TileSetLayerPresenter);
        }

        [CanvasLayerExport(LayerType = typeof(TileGridLayerPresenter), TargetType = typeof(LevelRenderLayer))]
        public static CanvasLayer CreateTileCanvasLayer3 (LayerPresenter layer)
        {
            return new LevelRenderLayer(layer as LevelLayerPresenter);
        }
    }
}
