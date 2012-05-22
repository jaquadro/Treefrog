using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Treefrog.ViewModel.Dialogs
{
    public class NewLevelDialogVM : ViewModelBase, IDataErrorInfo, IDialogViewModel
    {
        #region Level Name

        #region Reserved Names

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        #endregion

        private string _levelName = "";

        public string LevelName
        {
            get { return _levelName; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_levelName != value) {
                    TestValidState(() =>
                    {
                        _levelName = value;
                        RaisePropertyChanged("LevelName");
                    });
                }
            }
        }

        #endregion

        #region Tile and Level Values

        #region Min, Max Tile and Level Limits

        private int _minLevelHeight = 1;
        private int _maxLevelHeight = 1 << 16;
        private int _minLevelWidth = 1;
        private int _maxLevelWidth = 1 << 16;
        private int _minTileHeight = 1;
        private int _maxTileHeight = 512;
        private int _minTileWidth = 1;
        private int _maxTileWidth = 512;

        public int MinLevelHeight
        {
            get { return _minLevelHeight; }
        }

        public int MaxLevelHeight
        {
            get { return _maxLevelHeight; }
        }

        public int MinLevelWidth
        {
            get { return _minLevelWidth; }
        }

        public int MaxLevelWidth
        {
            get { return _maxLevelWidth; }
        }

        public int MinTileHeight
        {
            get { return _minTileHeight; }
        }

        public int MaxTileHeight
        {
            get { return _maxTileHeight; }
        }

        public int MinTileWidth
        {
            get { return _minTileWidth; }
        }

        public int MaxTileWidth
        {
            get { return _maxTileWidth; }
        }

        #endregion

        private int? _levelHeight = 30;
        private int? _levelWidth = 50;
        private int? _tileHeight = 16;
        private int? _tileWidth = 16;

        public int? LevelHeight
        {
            get { return _levelHeight; }
            set
            {
                if (_levelHeight != value) {
                    TestValidState(() =>
                    {
                        _levelHeight = value;
                        RaisePropertyChanged("LevelHeight");
                    });
                }
            }
        }

        public int? LevelWidth
        {
            get { return _levelWidth; }
            set
            {
                if (_levelWidth != value) {
                    TestValidState(() =>
                    {
                        _levelWidth = value;
                        RaisePropertyChanged("LevelWidth");
                    });
                }
            }
        }

        public int? TileHeight
        {
            get { return _tileHeight; }
            set
            {
                if (_tileHeight != value) {
                    TestValidState(() =>
                    {
                        _tileHeight = value;
                        RaisePropertyChanged("TileHeight");
                    });
                }
            }
        }

        public int? TileWidth
        {
            get { return _tileWidth; }
            set
            {
                if (_tileWidth != value) {
                    TestValidState(() =>
                    {
                        _tileWidth = value;
                        RaisePropertyChanged("TileWidth");
                    });
                }
            }
        }

        #endregion

        #region Validation

        public bool IsValid
        {
            get
            {
                return ValidateLevelName() == null
                    && ValidateLevelHeight() == null
                    && ValidateLevelWidth() == null
                    && ValidateTileHeight() == null
                    && ValidateTileWidth() == null;
            }
        }

        private string ValidateLevelName ()
        {
            if (string.IsNullOrWhiteSpace(_levelName))
                return "Level Name must not be empty";
            if (_reservedNames.Contains(_levelName))
                return "Level Name conflicts with another Level";
            return null;
        }

        private string ValidateLevelHeight ()
        {
            if (_levelHeight == null || _levelHeight < _minLevelHeight || _levelHeight > _maxLevelHeight)
                return string.Format("Level Height must be in range [{0} - {1}]", _minLevelHeight, _maxLevelHeight);
            return null;
        }

        private string ValidateLevelWidth ()
        {
            if (_levelWidth == null || _levelWidth < _minLevelWidth || _levelWidth > _maxLevelWidth)
                return string.Format("Level Width must be in range [{0} - {1}]", _minLevelWidth, _maxLevelWidth);
            return null;
        }

        private string ValidateTileHeight ()
        {
            if (_tileHeight == null || _tileHeight < _minTileHeight || _tileHeight > _maxTileHeight)
                return string.Format("Tile Height must be in range [{0} - {1}]", _minTileHeight, _maxTileHeight);
            return null;
        }

        private string ValidateTileWidth ()
        {
            if (_tileWidth == null || _tileWidth < _minTileWidth || _tileWidth > _maxTileWidth)
                return string.Format("Tile Width must be in range [{0} - {1}]", _minTileWidth, _maxTileWidth);
            return null;
        }

        private void TestValidState (Action act)
        {
            bool valid = IsValid;

            act();

            if (valid != IsValid)
                RaisePropertyChanged("IsValid");
        }

        #endregion

        #region Commands

        public event EventHandler CloseRequested = (s, e) => { };

        protected virtual void OnCloseRequested (EventArgs e)
        {
            CloseRequested(this, e);
        }

        private RelayCommand _okayCommand;

        public ICommand OkayCommand
        {
            get
            {
                if (_okayCommand == null)
                    _okayCommand = new RelayCommand(OnExecuteOkay, CanExecuteOkay);
                return _okayCommand;
            }
        }

        private bool CanExecuteOkay ()
        {
            return true;
        }

        private void OnExecuteOkay ()
        {
            OnCloseRequested(EventArgs.Empty);
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName) {
                    case "LevelName":
                        return ValidateLevelName();
                    case "LevelHeight":
                        return ValidateLevelHeight();
                    case "LevelWidth":
                        return ValidateLevelWidth();
                    case "TileHeight":
                        return ValidateTileHeight();
                    case "TileWidth":
                        return ValidateTileWidth();
                    default:
                        return null;
                }
            }
        }

        #endregion
    }
}
