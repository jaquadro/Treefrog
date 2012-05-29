using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Treefrog.Framework.Model;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight;
using Treefrog.ViewModel.Dialogs;
using Treefrog.Messages;
using Treefrog.ViewModel.Commands;
using System.ComponentModel;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Menu
{
    public class StandardMenuVM : ViewModelBase
    {
        EditorVM _editor;
        DocumentVM _document;

        public StandardMenuVM (EditorVM editor)
        {
            _editor = editor;
            _editor.PropertyChanged += HandleEditorPropertyChanged;

            SetDocument(_editor.ActiveDocument);
        }

        private void HandleEditorPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "ActiveDocument":
                    SetDocument(_editor.ActiveDocument);
                    break;
            }
        }

        private void SetDocument (DocumentVM document)
        {
            if (_document != null) {
                _document.CanUndoChanged -= HandleDocumentCanUndoChanged;
                _document.CanRedoChanged -= HandleDocumentCanRedoChanged;
                _document.CanCutChanged -= HandleDocumentCanCutChanged;
                _document.CanCopyChanged -= HandleDocumentCanCopyChanged;
                _document.CanPasteChanged -= HandleDocumentCanPasteChanged;
                _document.CanDeleteChanged -= HandleDocumentCanDeleteChanged;
                _document.CanSelectAllChanged -= HandleDocumentCanSelectAllChanged;
                _document.CanSelectNoneChanged -= HandleDocumentCanSelectNoneChanged;
            }

            _document = document;

            if (_document != null) {
                _document.CanUndoChanged += HandleDocumentCanUndoChanged;
                _document.CanRedoChanged += HandleDocumentCanRedoChanged;
                _document.CanDeleteChanged += HandleDocumentCanDeleteChanged;
                _document.CanCutChanged += HandleDocumentCanCutChanged;
                _document.CanCopyChanged += HandleDocumentCanCopyChanged;
                _document.CanPasteChanged += HandleDocumentCanPasteChanged;
                _document.CanSelectAllChanged += HandleDocumentCanSelectAllChanged;
                _document.CanSelectNoneChanged += HandleDocumentCanSelectNoneChanged;
            }
        }

        private void HandleDocumentCanUndoChanged (object sender, EventArgs e)
        {
            if (_undoCommand != null)
                _undoCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanRedoChanged (object sender, EventArgs e)
        {
            if (_redoCommand != null)
                _redoCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanCutChanged (object sender, EventArgs e)
        {
            if (_cutCommand != null)
                _cutCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanCopyChanged (object sender, EventArgs e)
        {
            if (_copyCommand != null)
                _copyCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanPasteChanged (object sender, EventArgs e)
        {
            if (_pasteCommand != null)
                _pasteCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanDeleteChanged (object sender, EventArgs e)
        {
            if (_deleteCommand != null)
                _deleteCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanSelectAllChanged (object sender, EventArgs e)
        {
            if (_selectAllCommand != null)
                _selectAllCommand.RaiseCanExecuteChanged();
        }

        private void HandleDocumentCanSelectNoneChanged (object sender, EventArgs e)
        {
            if (_selectNoneCommand != null)
                _selectNoneCommand.RaiseCanExecuteChanged();
        }

        private void HandleHistoryChanged (object sender, EventArgs e)
        {
            if (_undoCommand != null)
                _undoCommand.RaiseCanExecuteChanged();
            if (_redoCommand != null)
                _redoCommand.RaiseCanExecuteChanged();
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
            try {
                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.OpenFileDialog(new OpenFileOptions()
                    {
                        Filter = "Treefrog Projects|*.tlp|All Files|*",
                        FilterIndex = 0,
                    });

                    _editor.OpenProject(path);
                }
            }
            catch {
                IMessageService service = ServiceProvider.GetService<IMessageService>();
                if (service != null) {
                    service.ShowMessage(new MessageInfo()
                    {
                        Message = "Error opening requested file.",
                        Type = MessageType.Warning
                    });
                }
            }
        }

        #endregion

        #region Save Project Command

        private RelayCommand _saveProjectCommand;

        public ICommand SaveProjectCommand
        {
            get
            {
                if (_saveProjectCommand == null)
                    _saveProjectCommand = new RelayCommand(OnSaveProject, CanSaveProject);
                return _saveProjectCommand;
            }
        }

        private bool CanSaveProject ()
        {
            return _editor.Project != null;
        }

        private void OnSaveProject ()
        {
            if (String.IsNullOrEmpty(_editor.ProjectFile)) {
                IOService service = ServiceProvider.GetService<IOService>();
                if (service != null) {
                    string path = service.SaveFileDialog(new SaveFileOptions()
                    {
                        Filter = "Treefrog Projects|*.tlp|All Files|*",
                        FilterIndex = 0,
                    });

                    _editor.SaveProject(path);
                }
            }
            else {
                _editor.SaveProject(_editor.ProjectFile);
            }
        }

        #endregion

        #region Save Project As Command

        private RelayCommand _saveProjectAsCommand;

        public ICommand SaveProjectAsCommand
        {
            get
            {
                if (_saveProjectAsCommand == null)
                    _saveProjectAsCommand = new RelayCommand(OnSaveProjectAs, CanSaveProjectAs);
                return _saveProjectAsCommand;
            }
        }

        private bool CanSaveProjectAs ()
        {
            return _editor.Project != null;
        }

        private void OnSaveProjectAs ()
        {
            IOService service = ServiceProvider.GetService<IOService>();
            if (service != null) {
                string path = service.SaveFileDialog(new SaveFileOptions()
                {
                    Filter = "Treefrog Projects|*.tlp|All Files|*",
                    FilterIndex = 0,
                });

                _editor.SaveProject(path);
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

        #region Undo Command

        private RelayCommand _undoCommand;

        public ICommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                    _undoCommand = new RelayCommand(OnUndo, CanUndo);
                return _undoCommand;
            }
        }

        private bool CanUndo ()
        {
            if (_document == null)
                return false;
            return _document.CanUndo;
        }

        private void OnUndo ()
        {
            if (_document == null)
                return;
            _document.Undo();
            _undoCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Redo Command

        private RelayCommand _redoCommand;

        public ICommand RedoCommand
        {
            get
            {
                if (_redoCommand == null)
                    _redoCommand = new RelayCommand(OnRedo, CanRedo);
                return _redoCommand;
            }
        }

        private bool CanRedo ()
        {
            if (_document == null)
                return false;
            return _document.CanRedo;
        }

        private void OnRedo ()
        {
            if (_document == null)
                return;
            _document.Redo();
            _redoCommand.RaiseCanExecuteChanged();
        }

        #endregion

        #region Cut Command

        private RelayCommand _cutCommand;

        public ICommand CutCommand
        {
            get
            {
                if (_cutCommand == null)
                    _cutCommand = new RelayCommand(OnCut, CanCut);
                return _cutCommand;
            }
        }

        private bool CanCut ()
        {
            if (_document == null)
                return false;
            return _document.CanCut;
        }

        private void OnCut ()
        {
            if (_document == null)
                return;
            _document.Cut();
        }

        #endregion

        #region Copy Command

        private RelayCommand _copyCommand;

        public ICommand CopyCommand
        {
            get
            {
                if (_copyCommand == null)
                    _copyCommand = new RelayCommand(OnCopy, CanCopy);
                return _copyCommand;
            }
        }

        private bool CanCopy ()
        {
            if (_document == null)
                return false;
            return _document.CanCopy;
        }

        private void OnCopy ()
        {
            if (_document == null)
                return;
            _document.Copy();
        }

        #endregion

        #region Paste Command

        private RelayCommand _pasteCommand;

        public ICommand PasteCommand
        {
            get
            {
                if (_pasteCommand == null)
                    _pasteCommand = new RelayCommand(OnPaste, CanPaste);
                return _pasteCommand;
            }
        }

        private bool CanPaste ()
        {
            if (_document == null)
                return false;
            return _document.CanPaste;
        }

        private void OnPaste ()
        {
            if (_document == null)
                return;
            _document.Paste();
        }

        #endregion

        #region Delete Command

        private RelayCommand _deleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                if (_deleteCommand == null)
                    _deleteCommand = new RelayCommand(OnDelete, CanDelete);
                return _deleteCommand;
            }
        }

        private bool CanDelete ()
        {
            if (_document == null)
                return false;
            return _document.CanDelete;
        }

        private void OnDelete ()
        {
            if (_document == null)
                return;
            _document.Delete();
        }

        #endregion

        #region Select All Command

        private RelayCommand _selectAllCommand;

        public ICommand SelectAllCommand
        {
            get
            {
                if (_selectAllCommand == null)
                    _selectAllCommand = new RelayCommand(OnSelectAll, CanSelectAll);
                return _selectAllCommand;
            }
        }

        private bool CanSelectAll ()
        {
            if (_document == null)
                return false;
            return _document.CanSelectAll;
        }

        private void OnSelectAll ()
        {
            if (_document == null)
                return;
            _document.SelectAll();
        }

        #endregion

        #region SelectNone Command

        private RelayCommand _selectNoneCommand;

        public ICommand SelectNoneCommand
        {
            get
            {
                if (_selectNoneCommand == null)
                    _selectNoneCommand = new RelayCommand(OnSelectNone, CanSelectNone);
                return _selectNoneCommand;
            }
        }

        private bool CanSelectNone ()
        {
            if (_document == null)
                return false;
            return _document.CanSelectNone;
        }

        private void OnSelectNone ()
        {
            if (_document == null)
                return;
            _document.SelectNone();
        }

        #endregion
    }
}
