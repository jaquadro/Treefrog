using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework;

namespace Treefrog.V2.ViewModel
{
    public class ProjectVM : ViewModelBase
    {
        private Project _project;

        private TilePoolCollectionVM _tilePoolCollectionVM;
        private ObservableCollection<DocumentVM> _documents;

        public ProjectVM ()
            : base()
        {
            _project = new Project();
            _project.Levels.ResourceAdded += Project_LevelAdded;

            _tilePoolCollectionVM = new TilePoolCollectionVM();
            _documents = new ObservableCollection<DocumentVM>();

            Level lv = new Level("Level X");
            lv.Layers.Add(new MultiTileGridLayer("Tile Layer 1", 16, 16, 16, 16));
            _documents.Add(new LevelDocumentVM(lv));
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
            _project.Levels.ResourceAdded += Project_LevelAdded;

            _tilePoolCollectionVM = new TilePoolCollectionVM(project.TilePools);
            _documents = new ObservableCollection<DocumentVM>();

            foreach (Level level in project.Levels) {
                _documents.Add(new LevelDocumentVM(level));
            }

            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<TilePoolManagerService>(_tilePoolCollectionVM);
        }
    }
}
