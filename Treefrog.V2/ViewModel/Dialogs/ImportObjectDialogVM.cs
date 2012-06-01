using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Treefrog.Framework;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;

namespace Treefrog.ViewModel.Dialogs
{
    public class ImportObjectDialogVM : ViewModelBase, IDataErrorInfo, IDialogViewModel
    {
        #region Object Name

        #region Reserved Names

        private List<string> _reservedNames = new List<string>();

        public List<string> ReservedNames
        {
            get { return _reservedNames; }
            set { _reservedNames = value; }
        }

        #endregion

        private string _name = "";

        public string ObjectName
        {
            get { return _name; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_name != value) {
                    TestValidState(() =>
                    {
                        _name = value;
                        RaisePropertyChanged("ObjectName");
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
                        LoadObjectPreview(value);

                        RaisePropertyChanged("SourceFile");
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
                string path = service.OpenFileDialog(new OpenFileOptions()
                {
                    Filter = "Image Files|*.png;*.gif;*.bmp|All Files|*",
                    FilterIndex = 0,
                });

                SourceFile = path;
            }
        }

        #endregion

        #endregion

        #region Object Parameters

        private int? _maskLeft = 0;
        private int? _maskTop = 0;
        private int? _maskRight = 0;
        private int? _maskBottom = 0;
        private int? _originX = 0;
        private int? _originY = 0;

        public int? MaskLeft
        {
            get { return _maskLeft; }
            set
            {
                if (_maskLeft != value) {
                    TestValidState(() =>
                    {
                        _maskLeft = value;
                        RaisePropertyChanged("MaskLeft");
                        RaisePreviewProperties();
                    });
                }
            }
        }

        public int? MaskTop
        {
            get { return _maskTop; }
            set
            {
                if (_maskTop != value) {
                    TestValidState(() =>
                    {
                        _maskTop = value;
                        RaisePropertyChanged("MaskTop");
                        RaisePreviewProperties();
                    });
                }
            }
        }

        public int? MaskRight
        {
            get { return _maskRight; }
            set
            {
                if (_maskRight != value) {
                    TestValidState(() =>
                    {
                        _maskRight = value;
                        RaisePropertyChanged("MaskRight");
                        RaisePreviewProperties();
                    });
                }
            }
        }

        public int? MaskBottom
        {
            get { return _maskBottom; }
            set
            {
                if (_maskBottom != value) {
                    TestValidState(() =>
                    {
                        _maskBottom = value;
                        RaisePropertyChanged("MaskBottom");
                        RaisePreviewProperties();
                    });
                }
            }
        }

        public int? OriginX
        {
            get { return _originX; }
            set
            {
                if (_originX != value) {
                    TestValidState(() =>
                    {
                        UpdateOrigin(value, _originY);
                    });
                }
            }
        }

        public int? OriginY
        {
            get { return _originY; }
            set
            {
                if (_originY != value) {
                    TestValidState(() =>
                    {
                        UpdateOrigin(_originX, value);
                    });
                }
            }
        }

        private void UpdateOrigin (int? x, int? y)
        {
            int oldX = _originX ?? 0;
            int oldY = _originY ?? 0;

            int diffX = (x ?? 0) - oldX;
            int diffY = (y ?? 0) - oldY;

            _originX = x;
            _originY = y;
            _maskLeft -= diffX;
            _maskTop -= diffY;
            _maskRight -= diffX;
            _maskBottom -= diffY;

            RaiseMaskProperties();
            RaisePreviewProperties();
        }

        #endregion

        #region Validation

        public bool IsValid
        {
            get
            {
                return ValidateObjectName() == null
                    && ValidateSourceFile() == null
                    && ValidateMaskLeft() == null
                    && ValidateMaskTop() == null
                    && ValidateMaskRight() == null
                    && ValidateMaskBottom() == null
                    && ValidateOriginX() == null
                    && ValidateOriginY() == null;
            }
        }

        private string ValidateObjectName ()
        {
            if (string.IsNullOrWhiteSpace(_name))
                return "Object Name must not be empty";
            if (_reservedNames.Contains(_name))
                return "Object Name conflicts with another Object";
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

        private string ValidateMaskLeft ()
        {
            if (_maskLeft == null || _maskLeft > (_maskRight ?? 0))
                return "Mask must define a positive area";
            return null;
        }

        private string ValidateMaskTop ()
        {
            if (_maskTop == null || _maskTop > (_maskBottom ?? 0))
                return "Mask must define a positive area";
            return null;
        }

        private string ValidateMaskRight ()
        {
            if (_maskRight == null || _maskRight < (_maskLeft ?? 0))
                return "Mask must define a positive area";
            return null;
        }

        private string ValidateMaskBottom ()
        {
            if (_maskBottom == null || _maskBottom < (_maskTop ?? 0))
                return "Mask must define a positive area";
            return null;
        }

        private string ValidateOriginX ()
        {
            if (_originX == null)
                return "Invalid origin";
            return null;
        }

        private string ValidateOriginY ()
        {
            if (_originY == null)
                return "Invalid origin";
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
                    case "ObjectName":
                        return ValidateObjectName();
                    case "SourceFile":
                        return ValidateSourceFile();
                    case "MaskLeft":
                        return ValidateMaskLeft();
                    case "MaskTop":
                        return ValidateMaskTop();
                    case "MaskRight":
                        return ValidateMaskRight();
                    case "MaskBottom":
                        return ValidateMaskBottom();
                    case "OriginX":
                        return ValidateOriginX();
                    case "OriginY":
                        return ValidateOriginY();
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region Object Preview Management

        private bool _sourceFileValid = false;
        private TextureResource _sourceImage;

        public TextureResource SourceImage
        {
            get { return _sourceImage; }
        }

        private void ClearObjectPreiew ()
        {
            _sourceImage = null;

            RaisePreviewProperties();
        }

        private void LoadObjectPreview (String path)
        {
            try {
                _sourceImage = TextureResourceBitmapExt.CreateTextureResource(path);

                _originX = 0;
                _originY = 0;
                _maskLeft = 0;
                _maskTop = 0;
                _maskRight = _sourceImage.Width;
                _maskBottom = _sourceImage.Height;

                _sourceFileValid = true;
                RaisePreviewProperties();
                RaiseMaskProperties();
            }
            catch (Exception) {
                _sourceFileValid = false;
            }
        }

        private void ResetObjectPreview ()
        {
            if (!IsValid)
                ClearObjectPreiew();

            if (_sourceFileValid)
                LoadObjectPreview(_sourceFile);
        }

        private void RaiseMaskProperties ()
        {
            RaisePropertyChanged("OriginX");
            RaisePropertyChanged("OriginY");
            RaisePropertyChanged("MaskLeft");
            RaisePropertyChanged("MaskTop");
            RaisePropertyChanged("MaskRight");
            RaisePropertyChanged("MaskBottom");
        }

        private void RaisePreviewProperties ()
        {
            RaisePropertyChanged("PreviewCanvasWidth");
            RaisePropertyChanged("PreviewCanvasHeight");
            RaisePropertyChanged("PreviewImageX");
            RaisePropertyChanged("PreviewImageY");
            RaisePropertyChanged("PreviewImageWidth");
            RaisePropertyChanged("PreviewImageHeight");
            RaisePropertyChanged("PreviewMaskX");
            RaisePropertyChanged("PreviewMaskY");
            RaisePropertyChanged("PreviewMaskWidth");
            RaisePropertyChanged("PreviewMaskHeight");
            RaisePropertyChanged("PreviewOriginX");
            RaisePropertyChanged("PreviewOriginY");
            RaisePropertyChanged("SourceImage");
        }

        public int PreviewCanvasWidth
        {
            get { return 200; }
        }

        public int PreviewCanvasHeight
        {
            get { return 200; }
        }

        public int PreviewImageWidth 
        {
            get { return _sourceImage != null ? _sourceImage.Width : 0; }
        }

        public int PreviewImageHeight 
        {
            get { return _sourceImage != null ? _sourceImage.Height : 0; }
        }

        public int PreviewImageX
        {
            get { return PreviewCanvasWidth / 2 - PreviewImageWidth / 2; }
        }

        public int PreviewImageY
        {
            get { return PreviewCanvasHeight / 2 - PreviewImageHeight / 2; }
        }

        public int PreviewOriginX
        {
            get { return (PreviewImageX + (_originX ?? 0)) - 3; }
        }

        public int PreviewOriginY
        {
            get { return (PreviewImageY + (_originY ?? 0)) - 3; }
        }

        public int PreviewMaskX
        {
            get { return (PreviewImageX + (_originX ?? 0)) + (_maskLeft ?? 0); }
        }

        public int PreviewMaskY
        {
            get { return (PreviewImageY + (_originY ?? 0)) + (_maskTop ?? 0); }
        }

        public int PreviewMaskWidth
        {
            get { return (_maskRight ?? 0) - (_maskLeft ?? 0); }
        }

        public int PreviewMaskHeight
        {
            get { return (_maskBottom ?? 0) - (_maskTop ?? 0); }
        }

        #endregion
    }
}
