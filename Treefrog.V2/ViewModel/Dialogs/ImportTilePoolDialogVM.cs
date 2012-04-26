using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Treefrog.Aux;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;

namespace Treefrog.V2.ViewModel.Dialogs
{
    public class ImportTilePoolDialogVM : ViewModelBase, IDataErrorInfo, IDialogViewModel
    {
        #region Pool Name

        #region Reserved Names

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        #endregion

        private string _name = "";

        public string TilePoolName
        {
            get { return _name; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_name != value) {
                    TestValidState(() =>
                    {
                        _name = value;
                        RaisePropertyChanged("TilePoolName");
                    });
                }
            }
        }

        #endregion

        #region Source File

        private string _sourceFile = "";

        public string SourceFile
        {
            get { return _sourceFile; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_sourceFile != value) {
                    TestValidState(() =>
                    {
                        _sourceFile = value;
                        RaisePropertyChanged("SourceFile");

                        LoadTilePreview(value);
                    });
                }
            }
        }

        #region Open Source File Command

        private RelayCommand _openSourceFileCommand;

        public ICommand OpenSourceFileCommand
        {
            get
            {
                if (_openSourceFileCommand == null)
                    _openSourceFileCommand = new RelayCommand(OnOpenSourceFile, CanOpenSourceFile);
                return _openSourceFileCommand;
            }
        }

        private bool CanOpenSourceFile ()
        {
            return true;
        }

        private void OnOpenSourceFile ()
        {
            IOService service = ServiceProvider.GetService<IOService>();
            if (service != null) {
                string path = service.OpenFileDialog("");

                SourceFile = path;
            }
        }

        #endregion

        #endregion

        #region Transparency

        private bool _useTransColor;
        private Color _transColor = Colors.Transparent;

        public bool UseTransparentColor
        {
            get { return _useTransColor; }
            set
            {
                if (_useTransColor != value) {
                    _useTransColor = value;
                    RaisePropertyChanged("UseTransparentColor");
                    ResetTilePreview();
                }
            }
        }

        public Color TransparentColor
        {
            get { return _transColor; }
            set
            {
                if (!_transColor.Equals(value)) {
                    _transColor = value;
                    RaisePropertyChanged("TransparentColor");
                    ResetTilePreview();
                }
            }
        }

        #endregion

        #region Tile Parameters

        #region Min, Max Parameter Values

        private int _minTileHeight = 1;
        private int _maxTileHeight = 512;
        private int _minTileWidth = 1;
        private int _maxTileWidth = 512;
        private int _minTileSpaceX = 0;
        private int _maxTileSpaceX = 512;
        private int _minTileSpaceY = 0;
        private int _maxTileSpaceY = 512;
        private int _minTileMarginX = 0;
        private int _maxTileMarginX = 1 << 16;
        private int _minTileMarginY = 0;
        private int _maxTileMarginY = 1 << 16;

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

        public int MinTileSpaceX
        {
            get { return _minTileSpaceX; }
        }

        public int MaxTileSpaceX
        {
            get { return _maxTileSpaceX; }
        }

        public int MinTileSpaceY
        {
            get { return _minTileSpaceY; }
        }

        public int MaxTileSpaceY
        {
            get { return _maxTileSpaceY; }
        }

        public int MinTileMarginX
        {
            get { return _minTileMarginX; }
        }

        public int MaxTileMarginX
        {
            get { return _maxTileMarginX; }
        }

        public int MinTileMarginY
        {
            get { return _minTileMarginY; }
        }

        public int MaxTileMarginY
        {
            get { return _maxTileMarginY; }
        }

        #endregion

        private int? _tileHeight = 16;
        private int? _tileWidth = 16;
        private int? _tileSpaceX = 1;
        private int? _tileSpaceY = 1;
        private int? _tileMarginX = 0;
        private int? _tileMarginY = 0;

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
                        ResetTilePreview();
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
                        ResetTilePreview();
                    });
                }
            }
        }

        public int? TileSpaceX
        {
            get { return _tileSpaceX; }
            set
            {
                if (_tileSpaceX != value) {
                    TestValidState(() =>
                    {
                        _tileSpaceX = value;
                        RaisePropertyChanged("TileSpaceX");
                        ResetTilePreview();
                    });
                }
            }
        }

        public int? TileSpaceY
        {
            get { return _tileSpaceY; }
            set
            {
                if (_tileSpaceY != value) {
                    TestValidState(() =>
                    {
                        _tileSpaceY = value;
                        RaisePropertyChanged("TileSpaceY");
                        ResetTilePreview();
                    });
                }
            }
        }

        public int? TileMarginX
        {
            get { return _tileMarginX; }
            set
            {
                if (_tileMarginX != value) {
                    TestValidState(() =>
                    {
                        _tileMarginX = value;
                        RaisePropertyChanged("TileMarginX");
                        ResetTilePreview();
                    });
                }
            }
        }

        public int? TileMarginY
        {
            get { return _tileMarginY; }
            set
            {
                if (_tileMarginY != value) {
                    TestValidState(() =>
                    {
                        _tileMarginY = value;
                        RaisePropertyChanged("TileMarginY");
                        ResetTilePreview();
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
                return ValidateTilePoolName() == null
                    && ValidateSourceFile() == null
                    && ValidateTileHeight() == null
                    && ValidateTileWidth() == null
                    && ValidateTileSpaceX() == null
                    && ValidateTileSpaceY() == null
                    && ValidateTileMarginX() == null
                    && ValidateTileMarginY() == null;
            }
        }

        private string ValidateTilePoolName ()
        {
            if (string.IsNullOrWhiteSpace(_name))
                return "Tile Pool Name must not be empty";
            if (_reservedNames.Contains(_name))
                return "Tile Pool Name conflicts with another Tile Pool";
            return null;
        }

        private string ValidateSourceFile ()
        {
            if (string.IsNullOrWhiteSpace(_sourceFile))
                return "Source File must not be empty";
            if (!_sourceFileValid)
                return "Invalid Source File slected";
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

        private string ValidateTileSpaceX ()
        {
            if (_tileSpaceX == null || _tileSpaceX < _minTileSpaceX || _tileSpaceX > _maxTileSpaceX)
                return string.Format("Tile X Spacing must be in range [{0} - {1}]", _minTileSpaceX, _maxTileSpaceX);
            return null;
        }

        private string ValidateTileSpaceY ()
        {
            if (_tileSpaceY == null || _tileSpaceY < _minTileSpaceY || _tileSpaceY > _maxTileSpaceY)
                return string.Format("Tile Y Spacing must be in range [{0} - {1}]", _minTileSpaceY, _maxTileSpaceY);
            return null;
        }

        private string ValidateTileMarginX ()
        {
            if (_tileMarginX == null || _tileMarginX < _minTileMarginX || _tileMarginX > _maxTileMarginX)
                return string.Format("Tile X Margin must be in range [{0} - {1}]", _minTileMarginX, _maxTileMarginX);
            return null;
        }

        private string ValidateTileMarginY ()
        {
            if (_tileMarginY == null || _tileMarginY < _minTileMarginY || _tileMarginY > _maxTileMarginY)
                return string.Format("Tile Y Margin must be in range [{0} - {1}]", _minTileMarginY, _maxTileMarginY);
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

        #region OK Command

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
                    case "TilePoolName":
                        return ValidateTilePoolName();
                    case "SourceFile":
                        return ValidateSourceFile();
                    case "TileHeight":
                        return ValidateTileHeight();
                    case "TileWidth":
                        return ValidateTileWidth();
                    case "TileSpaceX":
                        return ValidateTileSpaceX();
                    case "TileSpaceY":
                        return ValidateTileSpaceY();
                    case "TileMarginX":
                        return ValidateTileMarginX();
                    case "TileMarginY":
                        return ValidateTileMarginY();
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region Tile Preview Management

        private bool _sourceFileValid = false;

        private TilePoolManager _manager;
        private TilePool _tilePool;
        private TilePoolVM _poolVM;

        public ObservableCollection<TilePoolItemVM> Tiles
        {
            get { return _poolVM.Tiles; }
        }

        private void ClearTilePreview ()
        {
            _poolVM = null;
            _tilePool = null;
            _manager = null;

            RaisePropertyChanged("Tiles");
        }

        private void LoadTilePreview (String path)
        {
            try {
                TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(path);
                if (_useTransColor) {
                    resource.Apply(c =>
                    {
                        if (c.Equals(_transColor))
                            return Colors.Transparent;
                        else
                            return c;
                    });
                }

                TilePool.TileImportOptions options = new TilePool.TileImportOptions()
                {
                    TileHeight = _tileHeight ?? 16,
                    TileWidth = _tileWidth ?? 16,
                    SpaceX = _tileSpaceX ?? 0,
                    SpaceY = _tileSpaceY ?? 0,
                    MarginX = _tileMarginX ?? 0,
                    MarginY = _tileMarginY ?? 0,
                    ImportPolicty = TileImportPolicy.ImprotAll,
                };
                _manager = new TilePoolManager();
                _manager.ImportTilePool("preview", resource, options);
                _tilePool = _manager.Pools["preview"];

                _poolVM = new TilePoolVM(_tilePool);

                _sourceFileValid = true;
                RaisePropertyChanged("Tiles");
            }
            catch (Exception) {
                _sourceFileValid = false;
            }
        }

        private void ResetTilePreview ()
        {
            if (!IsValid)
                ClearTilePreview();

            if (_sourceFileValid)
                LoadTilePreview(_sourceFile);
        }

        #endregion
    }
}
