using Treefrog.Presentation.Layers;
using Treefrog.Utility;

namespace Treefrog.Render.Layers
{
    
    public class LayerFactory : DependentTypeFactory<LayerPresenter, CanvasLayer>
    {
        public static LayerFactory Default { get; private set; }

        static LayerFactory ()
        {
            Default = new LayerFactory();

            Default.Register<LayerPresenter, CanvasLayer>();
            Default.Register<WorkspaceLayerPresenter, WorkspaceRenderLayer>(layer => {
                return new WorkspaceRenderLayer(layer as WorkspaceLayerPresenter);
            });
            Default.Register<GroupLayerPresenter, GroupLayer>(layer => {
                return new GroupLayer(layer as GroupLayerPresenter);
            });
            Default.Register<LocalRenderLayerPresenter, LocalRenderLayer>(layer => {
                return new LocalRenderLayer(layer as LocalRenderLayerPresenter);
            });
            Default.Register<LevelLayerPresenter, LevelRenderLayer>(layer => {
                return new LevelRenderLayer(layer as LevelLayerPresenter);
            });
            //Default.Register<TileLayerPresenter, LevelRenderLayer>(layer => {
            //    return new LevelRenderLayer(layer as LevelLayerPresenter);
            //});
            //Default.Register<TileSetLayerPresenter, TileSetRenderLayer>(layer => {
            //    return new TileSetRenderLayer(layer as TileSetLayerPresenter);
            //});
            //Default.Register<TileGridLayerPresenter, LevelRenderLayer>(layer => {
            //    return new LevelRenderLayer(layer as LevelLayerPresenter);
            //});
            //Default.Register<ObjectLayerPresenter, LevelRenderLayer>(layer => {
            //    return new LevelRenderLayer(layer as LevelLayerPresenter);
            //});
            Default.Register<AnnotationLayerPresenter, AnnotationRenderLayer>(layer => {
                return new AnnotationRenderLayer(layer as AnnotationLayerPresenter);
            });
            Default.Register<GridLayerPresenter, GridRenderLayer>(layer => {
                return new GridRenderLayer(layer as GridLayerPresenter);
            });
        }
    }
}
