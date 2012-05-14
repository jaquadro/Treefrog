using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.V2.ViewModel.ToolBars;
using Treefrog.V2.ViewModel.Menu;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using Treefrog.V2.ViewModel.Commands;

namespace Treefrog.V2.ViewModel
{
    public class EditorVM : ViewModelBase
    {
        private ProjectVM _project;

        private double _selectedZoom;

        public EditorVM ()
            : base()
        {
            _project = new ProjectVM();
            _standardMenu = new StandardMenuVM(this);
            _standardToolBar = new StandardToolBarVM(_standardMenu);
            _tileMenu = new TileMenuVM(this);
            _tileToolBar = new TileToolBarVM(_tileMenu);
            _propertyCollection = new PropertyCollectionVM();

            GalaSoft.MvvmLight.ServiceContainer.Default.AddService<PropertyManagerService>(_propertyCollection);

            _selectedZoom = 16;

            ActiveContent = _project.Documents.First();
        }

        public ProjectVM Project
        {
            get { return _project; }
        }

        #region Component Properties

        private StandardToolBarVM _standardToolBar;
        private StandardMenuVM _standardMenu;
        private TileMenuVM _tileMenu;
        private TileToolBarVM _tileToolBar;

        public StandardToolBarVM StandardToolBar
        {
            get { return _standardToolBar; }
        }

        public StandardMenuVM StandardMenu
        {
            get { return _standardMenu; }
        }

        public TileToolBarVM TileToolBar
        {
            get { return _tileToolBar; }
        }

        public TileMenuVM TileMenu
        {
            get { return _tileMenu; }
        }

        #endregion

        #region Content Properties

        private object _activeContent;
        private DocumentVM _activeDocument;

        public object ActiveContent
        {
            get { return _activeContent; }
            set
            {
                if (_activeContent != value) {
                    _activeContent = value;
                    RaisePropertyChanged("ActiveContent");

                    if (_activeContent is DocumentVM)
                        ActiveDocument = _activeContent as DocumentVM;
                }
            }
        }

        public DocumentVM ActiveDocument
        {
            get { return _activeDocument; }
            set
            {
                if (_activeDocument != value) {
                    if (_activeDocument != null)
                        _activeDocument.IsActive = false;

                    _activeDocument = value;
                    if (_activeDocument != null)
                        _activeDocument.IsActive = true;

                    RaisePropertyChanged("ActiveDocument");
                    RaisePropertyChanged("ZoomVisibility");
                    RaisePropertyChanged("CommandHistory");
                }
            }
        }

        #endregion

        private PropertyCollectionVM _propertyCollection;
        public PropertyCollectionVM PropertyProxy
        {
            get { return _propertyCollection; }
        }

        private string _title = "Treefrog";

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value) {
                    _title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        private string _projectFile = "";
        public string ProjectFile
        {
            get { return _projectFile; }
            set
            {
                if (_projectFile != value) {
                    _projectFile = value;
                    RaisePropertyChanged("ProjectFile");
                }
            }
        }

        public void OpenProject (string path)
        {
            if (path == null)
                return;

            using (FileStream stream = File.OpenRead(path)) {
                Treefrog.Framework.Model.Project project = Treefrog.Framework.Model.Project.Open(stream, null);
                _project = new ProjectVM(project);

                if (_project.Documents.Count > 0) {
                    ActiveContent = _project.Documents[0];
                    RaisePropertyChanged("ActiveContent");
                }

                RaisePropertyChanged("Project");
                Title = "Treefrog - " + path;
                ProjectFile = path;
            }
        }

        public void SaveProject (string path)
        {
            if (path == null || _project == null)
                return;

            using (FileStream stream = File.Create(path)) {
                _project.Project.Save(stream);
            }
        }

        private Dictionary<double, double> _zoomLevelMap = new Dictionary<double, double>
        {
            { 2, 0.12 },
            { 4, 0.25 },
            { 8, 0.50 },
            { 16, 1.0 },
            { 18, 2.0 },
            { 20, 3.0 },
            { 22, 4.0 },
            { 24, 5.0 },
            { 26, 6.0 },
            { 28, 7.0 },
            { 30, 8.0 },
        };

        public double ZoomMin
        {
            get { return ZoomLevels[0]; }
        }

        public double ZoomMax
        {
            get { return ZoomLevels[ZoomLevels.Count - 1]; }
        }

        public double SelectedZoom
        {
            get { return _selectedZoom; }
            set { 
                _selectedZoom = value;
                RaisePropertyChanged("ZoomText");

                if (_activeDocument != null)
                    _activeDocument.ZoomLevel = _zoomLevelMap[_selectedZoom];
            }
        }

        public DoubleCollection ZoomLevels
        {
            get { return new DoubleCollection(_zoomLevelMap.Keys); }
        }

        public string ZoomText
        {
            get { return string.Format("{0:F0}%", _zoomLevelMap[_selectedZoom] * 100); }
        }

        public Visibility ZoomVisibility
        {
            get
            {
                if (_activeDocument == null)
                    return Visibility.Collapsed;
                return _activeDocument.SupportsZoom ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public CommandHistory CommandHistory 
        {
            get
            {
                if (_activeDocument == null)
                    return null;
                return _activeDocument.CommandHistory;
            }
        }
    }
}
