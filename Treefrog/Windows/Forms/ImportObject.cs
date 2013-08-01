using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;
using Treefrog.Windows.Controllers;
using Treefrog.Windows.Controls;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation;
using Treefrog.Framework.Model;

using Xna = Microsoft.Xna.Framework;
using LilyPath.Brushes;
using LilyPath;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Windows.Forms
{
    public partial class ImportObject : Form
    {
        private ValidationController _validateController;

        public ImportObject ()
        {
            InitializeForm();

            _validateController.Validate();
        }

        public ImportObject (ObjectClass obj)
        {
            InitializeForm();

            base.Text = "Edit Object";

            _sourceImage = obj.Image.Crop(obj.Image.Bounds);
            _sourceFileValid = true;
            _name = obj.Name;

            _originX = obj.Origin.X;
            _originY = obj.Origin.Y;
            _maskLeft = obj.MaskBounds.Left;
            _maskTop = obj.MaskBounds.Top;
            _maskRight = obj.MaskBounds.Right;
            _maskBottom = obj.MaskBounds.Bottom;

            UpdateMaskPropertyFields();

            _validateController.UnregisterControl(_textSource);
            _validateController.Validate();
        }

        private void InitializeForm ()
        {
            InitializeComponent();

            _validateController = new ValidationController() {
                OKButton = _buttonOK,
            };

            _validateController.RegisterControl(_textObjectName, ValidateObjectName);
            _validateController.RegisterControl(_textSource, ValidateSourceFile);
            _validateController.RegisterControl(_numMaskLeft,
                ValidationController.ValidateLessEq("Left Mask Bound", _numMaskLeft, "Right Mask Bound", _numMaskRight));
            _validateController.RegisterControl(_numMaskRight,
                ValidationController.ValidateGreaterEq("Right Mask Bound", _numMaskRight, "Left Mask Bound", _numMaskLeft));
            _validateController.RegisterControl(_numMaskTop,
                ValidationController.ValidateLessEq("Top Mask Bound", _numMaskTop, "Bottom Mask Bound", _numMaskBottom));
            _validateController.RegisterControl(_numMaskBottom,
                ValidationController.ValidateGreaterEq("Bottom Mask Bound", _numMaskBottom, "Top Mask Bound", _numMaskTop));
            _validateController.RegisterControl(_numOriginX,
                ValidationController.ValidateNumericUpDownFunc("Origin X", _numOriginX));
            _validateController.RegisterControl(_numOriginY,
                ValidationController.ValidateNumericUpDownFunc("Origin Y", _numOriginY));

            InitializeObjectPreview();
        }

        protected override void OnClosed (EventArgs e)
        {
            if (_objectBrush != null) {
                _objectBrush.Dispose();
                _objectBrush = null;
            }

            if (_maskPen != null) {
                _maskPen.Brush.Dispose();
                _maskPen.Dispose();
                _maskPen = null;
            }

            _drawControl.Dispose();

            base.OnClosed(e);
        }

        protected override void OnLoad(EventArgs e)
        {
 	         base.OnLoad(e);
            
            _textObjectName.Text = _name ?? FindDefaultName("Object");

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

        private string _name = null;

        public string ObjectName
        {
            get { return _name; }
            set
            {
                value = value != null ? value.Trim() : "";
                if (_name != value) {
                    _name = value;
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
                    _sourceFile = value;
                }
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
                _textSource.Text = dlg.FileName;
                SourceFile = dlg.FileName;
                ResetObjectPreview();
                
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
                    && ValidateSourceFile() == null;
            }
        }

        private string ValidateObjectName ()
        {
            string txt = _textObjectName.Text.Trim();

            if (string.IsNullOrWhiteSpace(txt))
                return "Object Name must not be empty";
            if (_reservedNames.Contains(txt))
                return "Object Name conflicts with another Object";
            return null;
        }

        private string ValidateSourceFile ()
        {
            string txt = _textSource.Text.Trim();

            if (string.IsNullOrWhiteSpace(txt))
                return "Source File must not be empty";

            if (!_sourceFileValid)
                return "Invalid Source File slected";
            return null;
        }

        #endregion

        #region Object Preview Management

        private LilyPathControl _drawControl;

        private bool _sourceFileValid = false;
        private TextureResource _sourceImage;

        public TextureResource SourceImage
        {
            get { return _sourceImage; }
        }

        private void InitializeObjectPreview ()
        {
            _drawControl = new LilyPathControl();
            _drawControl.Dock = DockStyle.Fill;
            _drawControl.DrawAction = DrawObjectAction;

            panel1.Controls.Add(_drawControl);
        }

        private void ClearObjectPreiew ()
        {
            _sourceImage = null;
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
            }
            catch (Exception) {
                _sourceFileValid = false;
            }
        }

        private TextureBrush _objectBrush;
        private Pen _maskPen;
        

        private void DrawObjectAction (DrawBatch drawBatch)
        {
            if (_sourceImage == null)
                return;

            drawBatch.GraphicsDevice.ScissorRectangle = Xna.Rectangle.Empty;

            if (_objectBrush == null)
                _objectBrush = new TextureBrush(_sourceImage.CreateTexture(_drawControl.GraphicsDevice)) { OwnsTexture = true };
            if (_maskPen == null)
                _maskPen = new Pen(new CheckerBrush(_drawControl.GraphicsDevice, Xna.Color.Black, Xna.Color.White, 4, .75f), true);

            int originX = (_drawControl.Width - _sourceImage.Width) / 2;
            int originY = (_drawControl.Height - _sourceImage.Height) / 2;

            _objectBrush.Transform = Xna.Matrix.CreateTranslation(-(float)originX / _sourceImage.Width, -(float)originY / _sourceImage.Height, 0);

            drawBatch.Begin();

            drawBatch.FillRectangle(_objectBrush, new Xna.Rectangle(originX, originY, _sourceImage.Width, _sourceImage.Height));
            drawBatch.DrawRectangle(_maskPen, new Microsoft.Xna.Framework.Rectangle(
                originX - 1 + (_maskLeft ?? 0) + (_originX ?? 0), 
                originY - 1 + (_maskTop ?? 0) + (_originY ?? 0), 
                1 + (_maskRight - _maskLeft) ?? 0, 
                1 + (_maskBottom - _maskTop) ?? 0));
            
            drawBatch.FillCircle(Brush.White, new Xna.Vector2(originX + _originX ?? 0, originY + _originY ?? 0), 4, 12);
            drawBatch.FillCircle(Brush.Black, new Xna.Vector2(originX + _originX ?? 0, originY + _originY ?? 0), 3, 12);

            drawBatch.End();
        }

        private void ResetObjectPreview ()
        {
            if (_objectBrush != null) {
                _objectBrush.Dispose();
                _objectBrush = null;
            }

            LoadObjectPreview(_sourceFile);

            if (ValidateSourceFile() != null)
                ClearObjectPreiew();
        }

        private void UpdateMaskPropertyFields ()
        {
            if (_numOriginX.Value != (_originX ?? 0))
                _numOriginX.Value = _originX ?? 0;
            if (_numOriginY.Value != (_originY ?? 0))
                _numOriginY.Value = _originY ?? 0;
            if (_numMaskLeft.Value != (_maskLeft ?? 0))
                _numMaskLeft.Value = _maskLeft ?? 0;
            if (_numMaskRight.Value != (_maskRight ?? 0))
                _numMaskRight.Value = _maskRight ?? 0;
            if (_numMaskTop.Value != (_maskTop ?? 0))
                _numMaskTop.Value = _maskTop ?? 0;
            if (_numMaskBottom.Value != (_maskBottom ?? 0))
                _numMaskBottom.Value = _maskBottom ?? 0;
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
            if (!_validateController.ValidateForm())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void _buttonBrowse_Click (object sender, EventArgs e)
        {
            OpenSourceFile();
        }

        private void _textObjectName_TextChanged (object sender, EventArgs e)
        {
            ObjectName = _textObjectName.Text;
        }

        private void _numMaskLeft_ValueChanged (object sender, EventArgs e)
        {
            MaskLeft = (int)_numMaskLeft.Value;
        }

        private void _numMaskRight_ValueChanged (object sender, EventArgs e)
        {
            MaskRight = (int)_numMaskRight.Value;
        }

        private void _numMaskTop_ValueChanged (object sender, EventArgs e)
        {
            MaskTop = (int)_numMaskTop.Value;
        }

        private void _numMaskBottom_ValueChanged (object sender, EventArgs e)
        {
            MaskBottom = (int)_numMaskBottom.Value;
        }

        private void _numOriginX_ValueChanged (object sender, EventArgs e)
        {
            OriginX = (int)_numOriginX.Value;
        }

        private void _numOriginY_ValueChanged (object sender, EventArgs e)
        {
            OriginY = (int)_numOriginY.Value;
        }

        private string FindDefaultName (string baseName)
        {
            int i = 0;
            while (true) {
                string name = baseName + " " + ++i;
                if (_reservedNames.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
