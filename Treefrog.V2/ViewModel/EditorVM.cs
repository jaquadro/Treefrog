using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.V2.ViewModel.ToolBars;
using Treefrog.V2.ViewModel.Menu;
using System.IO;

namespace Treefrog.V2.ViewModel
{
    public class EditorVM : ViewModelBase
    {
        private ProjectVM _project;
        private StandardToolBarVM _standardToolBar;
        private StandardMenuVM _standardMenu;
        private TileMenuVM _tileMenu;
        private TileToolBarVM _tileToolBar;

        public EditorVM ()
            : base()
        {
            _project = new ProjectVM();
            _standardMenu = new StandardMenuVM(this);
            _standardToolBar = new StandardToolBarVM(_standardMenu);
            _tileMenu = new TileMenuVM(this);
            _tileToolBar = new TileToolBarVM(_tileMenu);
        }

        public ProjectVM Project
        {
            get { return _project; }
        }

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

        public string Title
        {
            get { return "Treefrog"; }
        }

        private object _ac;
        public object ActiveContent
        {
            get { return _ac; }
            set { _ac = value; }
        }

        public void OpenProject (string path)
        {
            if (path == null)
                return;

            using (FileStream stream = File.OpenRead(path)) {
                Treefrog.Framework.Model.Project project = Treefrog.Framework.Model.Project.Open(stream, null);
                _project = new ProjectVM(project);

                RaisePropertyChanged("Project");
            }
        }
    }
}
