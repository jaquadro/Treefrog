using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Treefrog.Framework.Model;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Treefrog.V2.ViewModel.Dialogs;
using Treefrog.V2.Messages;

namespace Treefrog.V2.ViewModel.Menu
{
    public class StandardMenuVM : ViewModelBase
    {
        EditorVM _editor;

        public StandardMenuVM (EditorVM editor)
        {
            _editor = editor;
        }

        #region Open Project Command

        private RelayCommand _openProjectCommand;

        public ICommand OpenProjectCommand
        {
            get
            {
                if (_openProjectCommand == null)
                    _openProjectCommand = new RelayCommand(OnOpenProject, CanOpenProject);
                return _openProjectCommand;
            }
        }

        private bool CanOpenProject ()
        {
            return true;
        }

        private void OnOpenProject ()
        {
            IOService service = ServiceProvider.GetService<IOService>();
            if (service != null) {
                string path = service.OpenFileDialog("");

                _editor.OpenProject(path);
            }
        }

        #endregion

        #region New Level Command

        private RelayCommand _newLevelCommand;

        public ICommand NewLevelCommand
        {
            get
            {
                if (_newLevelCommand == null)
                    _newLevelCommand = new RelayCommand(OnNewLevel, CanNewLevel);
                return _newLevelCommand;
            }
        }

        private bool CanNewLevel ()
        {
            return true;
        }

        private void OnNewLevel ()
        {
            NewLevelDialogVM vm = new NewLevelDialogVM();
            foreach (Level lev in _editor.Project.Project.Levels)
                vm.ReservedNames.Add(lev.Name);

            BlockingDialogMessage message = new BlockingDialogMessage(this, vm);
            Messenger.Default.Send(message);

            if (message.DialogResult == true) {
                Level lev = new Level(vm.LevelName, vm.TileWidth ?? 0, vm.TileHeight ?? 0, vm.LevelWidth ?? 0, vm.LevelHeight ?? 0);
                _editor.Project.Project.Levels.Add(lev);
            }
        }

        #endregion
    }
}
