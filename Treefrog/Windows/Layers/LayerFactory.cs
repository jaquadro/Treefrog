using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Layers;
using Treefrog.Utility;

namespace Treefrog.Windows.Layers
{
    public class LayerFactory : DependentTypeFactory<LayerPresenter, CanvasLayer>
    {
        public static LayerFactory Default { get; private set; }

        static LayerFactory ()
        {
            Default = new LayerFactory();

            Default.Register<LayerPresenter, CanvasLayer>();
            Default.Register<WorkspaceLayerPresenter, WorkspaceLayer>();
            Default.Register<GroupLayerPresenter, GroupLayer>(layer => {
                return new GroupLayer() { Model = layer as GroupLayerPresenter };
            });
            Default.Register<LocalRenderLayerPresenter, LocalRenderLayer>(layer => {
                return new LocalRenderLayer() { Model = layer as LocalRenderLayerPresenter };
            });
            Default.Register<LevelLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Default.Register<TileLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Default.Register<TileSetLayerPresenter, TileSetLayer>(layer => {
                return new TileSetLayer() { Model = layer as TileSetLayerPresenter };
            });
            Default.Register<TileGridLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Default.Register<ObjectLayerPresenter, RenderLayer>(layer => {
                return new RenderLayer() { Model = layer as LevelLayerPresenter };
            });
            Default.Register<AnnotationLayerPresenter, AnnotationLayer>(layer => {
                return new AnnotationLayer() { Model = layer as AnnotationLayerPresenter };
            });
            Default.Register<GridLayerPresenter, GridLayer>(layer => {
                return new GridLayer() { Model = layer as GridLayerPresenter };
            });
        }
    }
}
