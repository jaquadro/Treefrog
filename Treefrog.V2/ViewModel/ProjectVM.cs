using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework;
using System.IO;
using System.Drawing;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;

namespace Treefrog.V2.ViewModel
{
    public class ProjectVM : ViewModelBase
    {
        private Project _project;

        private TilePoolCollectionVM _tilePoolCollectionVM;
        private ObjectPoolCollectionVM _objectPoolCollectionVM;
        private ObservableCollection<DocumentVM> _documents;

        public ProjectVM ()
            : base()
        {
            _project = new Project();
            _project.Modified += HandleProjectModified;
            _project.Levels.ResourceAdded += Project_LevelAdded;

            ObjectPool objPool = new ObjectPool("Default");

            String path = @"E:\Workspace\Image Projects\Graphic Rips\Paper Mario\Environment Textures\Individual\Boo's Mansion";
            foreach (string name in Directory.EnumerateFiles(path, "*.bmp")) {
                Console.WriteLine(Path.Combine(path, name));
                TextureResource tres = TextureResourceBitmapExt.CreateTextureResource(Path.Combine(path, name));
                ObjectClass objClass = new ObjectClass(name, tres);
                objPool.AddObject(objClass);
            }
            _project.ObjectPools.Add(objPool);

            _objectPoolCollectionVM = new ObjectPoolCollectionVM(_project.ObjectPools);

            _tilePoolCollectionVM = new TilePoolCollectionVM(_project.TilePoolManager);
            _documents = new ObservableCollection<DocumentVM>();

            Level lv = new Level("Level 1", 16, 16, 40, 30)
            {
                Project = _project
            };

            Layer layer = new MultiTileGridLayer("Tile Layer 1", 16, 16, 40, 30)
            {
                Level = lv
            };
            lv.Layers.Add(layer);

            ObjectLayer layer2 = new ObjectLayer("Object Layer 1", 16 * 40, 16 * 30)
            {
                Level = lv
            };
            lv.Layers.Add(layer2);

            layer2.AddObject(new ObjectInstance(objPool.Objects.First(), 128, 96));

            _project.Levels.Add(lv);

            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<TilePoolManagerService>(_tilePoolCollectionVM);
            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<ObjectPoolManagerService>(_objectPoolCollectionVM);
        }

        public ProjectVM (Project project)
            : base()
        {
            LoadProject(project);
        }

        public TilePoolCollectionVM TilePoolCollection
        {
            get { return _tilePoolCollectionVM; }
        }

        public ObjectPoolCollectionVM ObjectPoolCollection
        {
            get { return _objectPoolCollectionVM; }
        }

        public ObservableCollection<DocumentVM> Documents
        {
            get { return _documents; }
        }

        private DocumentVM _currentDocument;
        public DocumentVM CurrentDocument
        {
            get { return _currentDocument; }
            set
            {
                if (_currentDocument != value) {
                    _currentDocument = value;
                    RaisePropertyChanged("CurrentDocument");
                }
            }
        }

        private bool _projectModified;
        public bool IsProjectModified
        {
            get { return _projectModified; }
            set
            {
                if (_projectModified != value) {
                    _projectModified = value;
                    RaisePropertyChanged("IsProjectModified");
                }
            }
        }

        private void HandleProjectModified (object sender, EventArgs e)
        {
            IsProjectModified = true;
        }

        private void Project_LevelAdded (object sender, NamedResourceEventArgs<Level> e)
        {
            _documents.Add(new LevelDocumentVM(e.Resource));
        }

        // XXX: Temporary
        public Project Project
        {
            get { return _project; }
        }

        private void LoadProject (Project project)
        {
            _project = project;
            _project.Modified += HandleProjectModified;
            _project.Levels.ResourceAdded += Project_LevelAdded;

            _tilePoolCollectionVM = new TilePoolCollectionVM(project.TilePoolManager);
            _documents = new ObservableCollection<DocumentVM>();

            foreach (Level level in project.Levels) {
                _documents.Add(new LevelDocumentVM(level));
            }

            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<TilePoolManagerService>(_tilePoolCollectionVM);
            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<ObjectPoolManagerService>(_objectPoolCollectionVM);
        }
    }
}
