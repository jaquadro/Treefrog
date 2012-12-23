using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Treefrog.Windows.Controls
{
    public enum Alignment
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    public partial class AlignmentControl : UserControl
    {
        private Alignment _alignment;
        private Size _oldSize;
        private Size _newSize;

        private Image _arrowTopLeft;
        private Image _arrowTop;
        private Image _arrowTopRight;
        private Image _arrowRight;
        private Image _arrowBottomRight;
        private Image _arrowBottom;
        private Image _arrowBottomLeft;
        private Image _arrowLeft;

        private Dictionary<Alignment, Action> _alignmentActions;

        public AlignmentControl ()
        {
            InitializeComponent();

            _alignmentActions = new Dictionary<Alignment, Action>() {
                { Alignment.None, SetAlignmentNone },
                { Alignment.TopLeft, SetAlignmentTopLeft },
                { Alignment.Top, SetAlignmentTop },
                { Alignment.TopRight, SetAlignmentTopRight },
                { Alignment.Left, SetAlignmentLeft },
                { Alignment.Center, SetAlignmentCenter },
                { Alignment.Right, SetAlignmentRight },
                { Alignment.BottomLeft, SetAlignmentBottomLeft },
                { Alignment.Bottom, SetAlignmentBottom },
                { Alignment.BottomRight, SetAlignmentBottomRight },
            };

            _arrowTopLeft = _arrowImages.Images[0];
            _arrowTop = _arrowImages.Images[1];
            _arrowTopRight = _arrowImages.Images[2];
            _arrowLeft = _arrowImages.Images[3];
            _arrowRight = _arrowImages.Images[4];
            _arrowBottomLeft = _arrowImages.Images[5];
            _arrowBottom = _arrowImages.Images[6];
            _arrowBottomRight = _arrowImages.Images[7];
        }

        public Image SourceIcon { get; set; }

        public Alignment Alignment
        {
            get { return _alignment; }
            set
            {
                if (_alignment != value) {
                    _alignment = value;
                    OnAlignmentChanged(EventArgs.Empty);
                }

                RefreshAlignment();
            }
        }

        public Size OldSize
        {
            get { return _oldSize; }
            set
            {
                _oldSize = value;
                RefreshAlignment();
            }
        }

        public Size NewSize
        {
            get { return _newSize; }
            set
            {
                _newSize = value;
                RefreshAlignment();
            }
        }

        public event EventHandler AlignmentChanged;

        protected virtual void OnAlignmentChanged (EventArgs e)
        {
            if (AlignmentChanged != null)
                AlignmentChanged(this, e);
        }

        private void RefreshAlignment ()
        {
            if (_alignmentActions.ContainsKey(_alignment))
                _alignmentActions[_alignment]();
        }

        private void ResetButtons ()
        {
            _buttonAlignBottom.Image = null;
            _buttonAlignBottomLeft.Image = null;
            _buttonAlignBottomRight.Image = null;
            _buttonAlignCenter.Image = null;
            _buttonAlignLeft.Image = null;
            _buttonAlignRight.Image = null;
            _buttonAlignTop.Image = null;
            _buttonAlignTopLeft.Image = null;
            _buttonAlignTopRight.Image = null;
        }

        private void SetAlignmentNone ()
        {
            ResetButtons();
        }

        private void SetAlignmentTopLeft ()
        {
            ResetButtons();

            _buttonAlignTopLeft.Image = SourceIcon;
            _buttonAlignTop.Image = GetImageRight();
            _buttonAlignLeft.Image = GetImageBottom();
            _buttonAlignCenter.Image = GetImageBottomRight();
        }

        private void SetAlignmentTop ()
        {
            ResetButtons();

            _buttonAlignTopLeft.Image = GetImageLeft();
            _buttonAlignTop.Image = SourceIcon;
            _buttonAlignTopRight.Image = GetImageRight();
            _buttonAlignLeft.Image = GetImageBottomLeft();
            _buttonAlignCenter.Image = GetImageBottom();
            _buttonAlignRight.Image = GetImageBottomRight();
        }

        private void SetAlignmentTopRight ()
        {
            ResetButtons();

            _buttonAlignTop.Image = GetImageLeft();
            _buttonAlignTopRight.Image = SourceIcon;
            _buttonAlignCenter.Image = GetImageBottomLeft();
            _buttonAlignRight.Image = GetImageBottom();
        }

        private void SetAlignmentLeft ()
        {
            ResetButtons();

            _buttonAlignTopLeft.Image = GetImageTop();
            _buttonAlignTop.Image = GetImageTopRight();
            _buttonAlignLeft.Image = SourceIcon;
            _buttonAlignCenter.Image = GetImageRight();
            _buttonAlignBottomLeft.Image = GetImageBottom();
            _buttonAlignBottom.Image = GetImageBottomRight();
        }

        private void SetAlignmentCenter ()
        {
            ResetButtons();

            _buttonAlignTopLeft.Image = GetImageTopLeft();
            _buttonAlignTop.Image = GetImageTop();
            _buttonAlignTopRight.Image = GetImageTopRight();
            _buttonAlignLeft.Image = GetImageLeft();
            _buttonAlignCenter.Image = SourceIcon;
            _buttonAlignRight.Image = GetImageRight();
            _buttonAlignBottomLeft.Image = GetImageBottomLeft();
            _buttonAlignBottom.Image = GetImageBottom();
            _buttonAlignBottomRight.Image = GetImageBottomRight();
        }

        private void SetAlignmentRight ()
        {
            ResetButtons();

            _buttonAlignTop.Image = GetImageTopLeft();
            _buttonAlignTopRight.Image = GetImageTop();
            _buttonAlignCenter.Image = GetImageLeft();
            _buttonAlignRight.Image = SourceIcon;
            _buttonAlignBottom.Image = GetImageBottomLeft();
            _buttonAlignBottomRight.Image = GetImageBottom();
        }

        private void SetAlignmentBottomLeft ()
        {
            ResetButtons();

            _buttonAlignLeft.Image = GetImageTop();
            _buttonAlignCenter.Image = GetImageTopRight();
            _buttonAlignBottomLeft.Image = SourceIcon;
            _buttonAlignBottom.Image = GetImageRight();
        }

        private void SetAlignmentBottom ()
        {
            ResetButtons();

            _buttonAlignLeft.Image = GetImageTopLeft();
            _buttonAlignCenter.Image = GetImageTop();
            _buttonAlignRight.Image = GetImageTopRight();
            _buttonAlignBottomLeft.Image = GetImageLeft();
            _buttonAlignBottom.Image = SourceIcon;
            _buttonAlignBottomRight.Image = GetImageRight();
        }

        private void SetAlignmentBottomRight ()
        {
            ResetButtons();

            _buttonAlignCenter.Image = GetImageTopLeft();
            _buttonAlignRight.Image = GetImageTop();
            _buttonAlignBottom.Image = GetImageLeft();
            _buttonAlignBottomRight.Image = SourceIcon;
        }

        private Image GetImageLeft ()
        {
            if (OldSize.Width == NewSize.Width)
                return null;

            return (OldSize.Width > NewSize.Width) ? _arrowRight : _arrowLeft;
        }

        private Image GetImageRight ()
        {
            if (OldSize.Width == NewSize.Width)
                return null;

            return (OldSize.Width > NewSize.Width) ? _arrowLeft : _arrowRight;
        }

        private Image GetImageTop ()
        {
            if (OldSize.Height == NewSize.Height)
                return null;

            return (OldSize.Height > NewSize.Height) ? _arrowBottom : _arrowTop;
        }

        private Image GetImageBottom ()
        {
            if (OldSize.Height == NewSize.Height)
                return null;

            return (OldSize.Height > NewSize.Height) ? _arrowTop : _arrowBottom;
        }

        private Image GetImageTopLeft ()
        {
            if (OldSize.Height == NewSize.Height || OldSize.Width == NewSize.Width)
                return null;

            if (OldSize.Width > NewSize.Width)
                return (OldSize.Height > NewSize.Height) ? _arrowBottomRight : _arrowTopRight;
            else
                return (OldSize.Height > NewSize.Height) ? _arrowBottomLeft : _arrowTopLeft;
        }

        private Image GetImageTopRight ()
        {
            if (OldSize.Height == NewSize.Height || OldSize.Width == NewSize.Width)
                return null;

            if (OldSize.Width > NewSize.Width)
                return (OldSize.Height > NewSize.Height) ? _arrowBottomLeft : _arrowTopLeft;
            else
                return (OldSize.Height > NewSize.Height) ? _arrowBottomRight : _arrowTopRight;
        }

        private Image GetImageBottomLeft ()
        {
            if (OldSize.Height == NewSize.Height || OldSize.Width == NewSize.Width)
                return null;

            if (OldSize.Width > NewSize.Width)
                return (OldSize.Height > NewSize.Height) ? _arrowTopRight : _arrowBottomRight;
            else
                return (OldSize.Height > NewSize.Height) ? _arrowTopLeft : _arrowBottomLeft;
        }

        private Image GetImageBottomRight ()
        {
            if (OldSize.Height == NewSize.Height || OldSize.Width == NewSize.Width)
                return null;

            if (OldSize.Width > NewSize.Width)
                return (OldSize.Height > NewSize.Height) ? _arrowTopLeft : _arrowBottomLeft;
            else
                return (OldSize.Height > NewSize.Height) ? _arrowTopRight : _arrowBottomRight;
        }

        private void _buttonAlignTopLeft_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.TopLeft;
        }

        private void _buttonAlignTop_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.Top;
        }

        private void _buttonAlignTopRight_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.TopRight;
        }

        private void _buttonAlignLeft_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.Left;
        }

        private void _buttonAlignCenter_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.Center;
        }

        private void _buttonAlignRight_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.Right;
        }

        private void _buttonAlignBottomLeft_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.BottomLeft;
        }

        private void _buttonAlignBottom_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.Bottom;
        }

        private void _buttonAlignBottomRight_Click (object sender, EventArgs e)
        {
            Alignment = Alignment.BottomRight;
        }
    }
}
