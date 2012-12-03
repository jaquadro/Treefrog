using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;

namespace Treefrog.Windows.Forms
{
    public partial class ImportObject : Form
    {
        public ImportObject ()
        {
            InitializeComponent();
        }

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
                    _name = value;
                    string error = ValidateObjectName();
                    if (error != null)
                        MessageBox.Show(error);
                }
                /*if (_name != value) {
                    TestValidState(() => {
                        _name = value;
                        RaisePropertyChanged("ObjectName");
                    });
                }*/
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
                    _sourceFile = value;
                    _textSource.Text = _sourceFile;
                    LoadObjectPreview(value);

                    string error = ValidateSourceFile();
                    if (error != null)
                        MessageBox.Show(error);
                }
                /*if (_sourceFile != value) {
                    TestValidState(() => {
                        _sourceFile = value;
                        LoadObjectPreview(value);

                        RaisePropertyChanged("SourceFile");
                    });
                }*/
            }
        }

        private void OpenSourceFile ()
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "Image Files (*.bmp,*.gif,*.jpg,*.png)|*.bmp;*.gif;*.jpg;*.jpeg;*.png|All files (*.*)|*.*";
            dlg.FilterIndex = 0;
            dlg.RestoreDirectory = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                SourceFile = dlg.FileName;
            }
        }

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
                    _maskLeft = value;
                    string error = ValidateMaskLeft();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        _maskLeft = value;
                        RaisePropertyChanged("MaskLeft");
                        RaisePreviewProperties();
                    });*/
                }
            }
        }

        public int? MaskTop
        {
            get { return _maskTop; }
            set
            {
                if (_maskTop != value) {
                    _maskTop = value;
                    string error = ValidateMaskTop();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        _maskTop = value;
                        RaisePropertyChanged("MaskTop");
                        RaisePreviewProperties();
                    });*/
                }
            }
        }

        public int? MaskRight
        {
            get { return _maskRight; }
            set
            {
                if (_maskRight != value) {
                    _maskRight = value;
                    string error = ValidateMaskRight();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        _maskRight = value;
                        RaisePropertyChanged("MaskRight");
                        RaisePreviewProperties();
                    });*/
                }
            }
        }

        public int? MaskBottom
        {
            get { return _maskBottom; }
            set
            {
                if (_maskBottom != value) {
                    _maskBottom = value;
                    string error = ValidateMaskBottom();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        _maskBottom = value;
                        RaisePropertyChanged("MaskBottom");
                        RaisePreviewProperties();
                    });*/
                }
            }
        }

        public int? OriginX
        {
            get { return _originX; }
            set
            {
                if (_originX != value) {
                    UpdateOrigin(value, _originY);
                    string error = ValidateOriginX();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        UpdateOrigin(value, _originY);
                    });*/
                }
            }
        }

        public int? OriginY
        {
            get { return _originY; }
            set
            {
                if (_originY != value) {
                    UpdateOrigin(_originX, value);
                    string error = ValidateOriginY();
                    if (error != null)
                        MessageBox.Show(error);
                    /*TestValidState(() => {
                        UpdateOrigin(_originX, value);
                    });*/
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

            UpdateMaskPropertyFields();
            //RaiseMaskProperties();
            //RaisePreviewProperties();
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

            //if (valid != IsValid)
            //    RaisePropertyChanged("IsValid");
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

            //RaisePreviewProperties();
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
                UpdateMaskPropertyFields();
                //RaisePreviewProperties();
                //RaiseMaskProperties();
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

        /*private void RaiseMaskProperties ()
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
        }*/

        private void UpdateMaskPropertyFields ()
        {
            _textOriginX.Text = (_originX ?? 0).ToString();
            _textOriginY.Text = (_originY ?? 0).ToString();
            _textMaskLeft.Text = (_maskLeft ?? 0).ToString();
            _textMaskTop.Text = (_maskTop ?? 0).ToString();
            _textMaskRight.Text = (_maskRight ?? 0).ToString();
            _textMaskBottom.Text = (_maskBottom ?? 0).ToString();
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

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {

        }

        private void _buttonBrowse_Click (object sender, EventArgs e)
        {
            OpenSourceFile();
        }

        private void _textObjectName_TextChanged (object sender, EventArgs e)
        {
            ObjectName = _textObjectName.Text;
        }

        private void _textMaskLeft_TextChanged (object sender, EventArgs e)
        {
            try {
                MaskLeft = Convert.ToInt32(_textMaskLeft.Text);
            }
            catch (FormatException) { }
        }

        private void _textMaskTop_TextChanged (object sender, EventArgs e)
        {
            try {
                MaskTop = Convert.ToInt32(_textMaskTop.Text);
            }
            catch (FormatException) { }
        }

        private void _textMaskRight_TextChanged (object sender, EventArgs e)
        {
            try {
                MaskRight = Convert.ToInt32(_textMaskRight.Text);
            }
            catch (FormatException) { }
        }

        private void _textMaskBottom_TextChanged (object sender, EventArgs e)
        {
            try {
                MaskBottom = Convert.ToInt32(_textMaskBottom.Text);
            }
            catch (FormatException) { }
        }

        private void _textOriginX_TextChanged (object sender, EventArgs e)
        {
            try {
                OriginX = Convert.ToInt32(_textOriginX.Text);
            }
            catch (FormatException) { }
        }

        private void _textOriginY_TextChanged (object sender, EventArgs e)
        {
            try {
                OriginY = Convert.ToInt32(_textOriginY.Text);
            }
            catch (FormatException) { }
        }
    }
}
