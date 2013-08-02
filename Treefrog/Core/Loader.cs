using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Treefrog.Presentation.Layers;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using System.ComponentModel.Composition.Hosting;
using Treefrog.Plugins.Object.Layers;

namespace Treefrog.Core
{
    internal class Loader
    {
        [Import]
        LayerPresenterFactoryLoader _layerPresenterFactoryLoader;

        public void Compose ()
        {
            AssemblyCatalog catalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            CompositionContainer container = new CompositionContainer(catalog);
            container.SatisfyImportsOnce(this);

            _layerPresenterFactoryLoader.CompleteLoading();
        }
    }

    public interface ILayerPresenterDesc
    {
        Type LayerType { get; }
        Type PresenterType { get; }
        Func<Layer, ILayerContext, LevelLayerPresenter> Create { get; }
    }

    [Export(typeof(ILayerPresenterDesc))]
    internal class ObjectLayerPresenterDesc : ILayerPresenterDesc
    {
        public Type LayerType {
            get { return typeof(ObjectLayer); }
        }

        public Type PresenterType {
            get { return typeof(ObjectLayerPresenter); }
        }

        public Func<Layer, ILayerContext, LevelLayerPresenter> Create {
            get
            {
                return (layer, context) => {
                    return new ObjectLayerPresenter(context, layer as ObjectLayer);
                };
            }
        }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method)]
    public class TypeFactoryExportAttribute : ExportAttribute
    {
        public TypeFactoryExportAttribute ()
            : base(typeof(Func<Layer, ILayerContext, LevelLayerPresenter>))
        { }

        public Type LayerType { get; set; }
        public Type TargetType { get; set; }
    }

    public static class FactoryRegistrants
    {
        [TypeFactoryExport(LayerType = typeof(ObjectLayer), TargetType = typeof(ObjectLayerPresenter))]
        public static LevelLayerPresenter CreateObjectLayerPresenter (Layer layer, ILayerContext context)
        {
            return new ObjectLayerPresenter(context, layer as ObjectLayer);
        }
    }

    public interface ITypeFactoryMetadata
    {
        Type LayerType { get; }
        Type TargetType { get; }
    }

    [Export]
    internal class LayerPresenterFactoryLoader
    {
        //[ImportMany]
        //List<ILayerPresenterDesc> _registrants;

        [ImportMany]
        List<Lazy<Func<Layer, ILayerContext, LevelLayerPresenter>, ITypeFactoryMetadata>> _registrants;

        public void CompleteLoading ()
        {
            foreach (var entry in _registrants)
                LayerPresenterFactory.Default.Register(entry.Metadata.LayerType, entry.Metadata.TargetType, entry.Value);
                //LayerPresenterFactory.Default.Register(entry.LayerType, entry.PresenterType, entry.Create);
        }
    }
}
