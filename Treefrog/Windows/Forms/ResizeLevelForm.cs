using System;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Windows.Controls;

namespace Treefrog.Windows.Forms
{
    public partial class ResizeLevelForm : Form
    {
        private Level _level;

        private int _newWidth;
        private int _newHeight;
        private int _left;
        private int _right;
        private int _top;
        private int _bottom;

        public ResizeLevelForm (Level level)
        {
            InitializeComponent();

            _level = level;

            NewWidth = level.Width;
            NewHeight = level.Height;

            _alignmentControl.SourceIcon = Properties.Resources.Map;
            _alignmentControl.OldSize = new Size(_level.Width, _level.Height);
            _alignmentControl.NewSize = new Size(_level.Width, _level.Height);
            _alignmentControl.Alignment = Alignment.TopLeft;
            _alignmentControl.AlignmentChanged += _alignmentControl_AlignmentChanged;

            _oldWidthLabel.Text = _level.Width.ToString();
            _oldHeightLabel.Text = _level.Height.ToString();

            CalculatePositionValues(_alignmentControl.Alignment);
        }

        private void CalculatePositionValues (Alignment alignment)
        {
            switch (alignment) {
                case Alignment.TopLeft:
                case Alignment.Left:
                case Alignment.BottomLeft:
                    PosLeft = 0;
                    break;
                case Alignment.TopRight:
                case Alignment.Right:
                case Alignment.BottomRight:
                    PosRight = 0;
                    break;
                case Alignment.Top:
                case Alignment.Center:
                case Alignment.Bottom:
                    PosLeft = (_newWidth - _level.Width) / 2;
                    break;
                default:
                    PosLeft = PosLeft;
                    break;
            }

            switch (alignment) {
                case Alignment.TopLeft:
                case Alignment.Top:
                case Alignment.TopRight:
                    PosTop = 0;
                    break;
                case Alignment.BottomLeft:
                case Alignment.Bottom:
                case Alignment.BottomRight:
                    PosBottom = 0;
                    break;
                case Alignment.Left:
                case Alignment.Center:
                case Alignment.Right:
                    PosTop = (_newHeight - _level.Height) / 2;
                    break;
                default:
                    PosTop = PosTop;
                    break;
            }
        }

        private bool DisableCalculateAlignment { get; set; }

        private void CalculateAlignment ()
        {
            if (DisableCalculateAlignment || !LeftRightConsistent || !TopBottomConsistent)
                return;

            if (PosTop == 0) {
                if (PosLeft == 0)
                    _alignmentControl.Alignment = Alignment.TopLeft;
                else if (PosLeft == PosRight)
                    _alignmentControl.Alignment = Alignment.Top;
                else if (PosRight == 0)
                    _alignmentControl.Alignment = Alignment.TopRight;
                else
                    _alignmentControl.Alignment = Alignment.None;
            }
            else if (PosTop == PosBottom) {
                if (PosLeft == 0)
                    _alignmentControl.Alignment = Alignment.Left;
                else if (PosLeft == PosRight)
                    _alignmentControl.Alignment = Alignment.Center;
                else if (PosRight == 0)
                    _alignmentControl.Alignment = Alignment.Right;
                else
                    _alignmentControl.Alignment = Alignment.None;
            }
            else if (PosBottom == 0) {
                if (PosLeft == 0)
                    _alignmentControl.Alignment = Alignment.BottomLeft;
                else if (PosLeft == PosRight)
                    _alignmentControl.Alignment = Alignment.Bottom;
                else if (PosRight == 0)
                    _alignmentControl.Alignment = Alignment.BottomRight;
                else
                    _alignmentControl.Alignment = Alignment.None;
            }
            else
                _alignmentControl.Alignment = Alignment.None;
        }

        public int NewOriginX
        {
            get { return _level.OriginX - _left; }
        }

        public int NewOriginY
        {
            get { return _level.OriginY - _top; }
        }

        public int NewWidth
        {
            get { return _newWidth; }
            private set
            {
                if (_newWidth != value) {
                    _newWidth = value;
                    _alignmentControl.NewSize = new Size(_newWidth, _newHeight);
                    CalculatePositionValues(_alignmentControl.Alignment);
                }
                if (_newWidth != _fieldWidth.Value)
                    _fieldWidth.Value = _newWidth;
            }
        }

        public  int NewHeight
        {
            get { return _newHeight; }
            private set
            {
                if (_newHeight != value) {
                    _newHeight = value;
                    _alignmentControl.NewSize = new Size(_newWidth, _newHeight);
                    CalculatePositionValues(_alignmentControl.Alignment);
                }
                if (_newHeight != _fieldHeight.Value)
                    _fieldHeight.Value = _newHeight;
            }
        }

        private bool LeftRightConsistent
        {
            get { return _newWidth == _level.Width + _left + _right; }
        }

        private bool TopBottomConsistent
        {
            get { return _newHeight == _level.Height + _top + _bottom; }
        }

        private int PosLeft
        {
            get { return _left; }
            set
            {
                if (_left != value) {
                    _left = value;
                    CalculateAlignment();
                }
                if (!LeftRightConsistent)
                    PosRight = (_newWidth - _level.Width) - _left;
                if (_left != _fieldLeft.Value)
                    _fieldLeft.Value = _left;
            }
        }

        private int PosRight
        {
            get { return _right; }
            set
            {
                if (_right != value) {
                    _right = value;
                    CalculateAlignment();
                }
                if (!LeftRightConsistent)
                    PosLeft = (_newWidth - _level.Width) - _right;
                if (_right != _fieldRight.Value)
                    _fieldRight.Value = _right;
            }
        }

        private int PosTop
        {
            get { return _top; }
            set
            {
                if (_top != value) {
                    _top = value;
                    CalculateAlignment();
                }
                if (!TopBottomConsistent)
                    PosBottom = (_newHeight - _level.Height) - _top;
                if (_top != _fieldTop.Value)
                    _fieldTop.Value = _top;
            }
        }

        private int PosBottom
        {
            get { return _bottom; }
            set
            {
                if (_bottom != value) {
                    _bottom = value;
                    CalculateAlignment();
                }
                if (!TopBottomConsistent)
                    PosTop = (_newHeight - _level.Height) - _bottom;
                if (_bottom != _fieldBottom.Value)
                    _fieldBottom.Value = _bottom;
            }
        }

        private void _alignmentControl_AlignmentChanged (object sender, EventArgs e)
        {
            DisableCalculateAlignment = true;
            CalculatePositionValues(_alignmentControl.Alignment);
            DisableCalculateAlignment = false;
        }

        private void _fieldWidth_ValueChanged (object sender, EventArgs e)
        {
            NewWidth = (int)_fieldWidth.Value;
        }

        private void _fieldHeight_ValueChanged (object sender, EventArgs e)
        {
            NewHeight = (int)_fieldHeight.Value;
        }

        private void _fieldTop_ValueChanged (object sender, EventArgs e)
        {
            PosTop = (int)_fieldTop.Value;
        }

        private void _fieldBottom_ValueChanged (object sender, EventArgs e)
        {
            PosBottom = (int)_fieldBottom.Value;
        }

        private void _fieldLeft_ValueChanged (object sender, EventArgs e)
        {
            PosLeft = (int)_fieldLeft.Value;
        }

        private void _fieldRight_ValueChanged (object sender, EventArgs e)
        {
            PosRight = (int)_fieldRight.Value;
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
