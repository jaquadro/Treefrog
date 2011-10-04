using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace Editor
{
    public enum TileToolMode
    {
        Select,
        Draw,
        Erase,
        Fill,
        Stamp
    }

    public class TileToolModeEventArgs : EventArgs
    {
        public TileToolMode TileToolMode { get; private set; }

        public TileToolModeEventArgs (TileToolMode mode)
        {
            TileToolMode = mode;
        }
    }

    class TileToolbar
    {
        private ToolStrip _strip;

        private ToolStripButton _tbSelect;
        private ToolStripButton _tbDraw;
        private ToolStripButton _tbErase;
        private ToolStripButton _tbFill;
        private ToolStripButton _tbStamp;

        private ToolStripButton _tbFlipH;
        private ToolStripButton _tbFlipV;
        private ToolStripButton _tbRotateLeft;
        private ToolStripButton _tbRotateRight;

        private Assembly _assembly;

        private TileToolMode _toolMode;

        public event EventHandler<TileToolModeEventArgs> ToolModeChanged;

        public TileToolbar ()
        {
            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _tbSelect = CreateButton("Select Tool (C)", "Editor.Icons.cursor16.png");
            _tbDraw = CreateButton("Draw Tool (D)", "Editor.Icons.paint-brush16.png");
            _tbErase = CreateButton("Erase Tool (E)", "Editor.Icons.eraser16.png");
            _tbFill = CreateButton("Fill Tool (F)", "Editor.Icons.paint-can16.png");
            _tbStamp = CreateButton("Stamp Tool (S)", "Editor.Icons.stamp16.png");

            _tbFlipH = CreateButton("Flip Horizontally", "Editor.Icons.layer-flip16.png");
            _tbFlipV = CreateButton("Flip Vertically", "Editor.Icons.layer-flip-vertical16.png");
            _tbRotateLeft = CreateButton("Rotate Left 90 Degrees", "Editor.Icons.layer-rotate-left16.png");
            _tbRotateRight = CreateButton("Rotate Right 90 Degrees", "Editor.Icons.layer-rotate16.png");

            _strip = new ToolStrip();
            _strip.Items.AddRange(new ToolStripItem[] {
                _tbSelect, _tbDraw, _tbErase, _tbFill, _tbStamp, new ToolStripSeparator(),
                _tbFlipH, _tbFlipV, _tbRotateLeft, _tbRotateRight
            });

            _tbSelect.Click += SelectButtonClickHandler;
            _tbDraw.Click += DrawButtonClickHandler;
            _tbErase.Click += EraseButtonClickHandler;
            _tbFill.Click += FillButtonClickHandler;
            _tbStamp.Click += StampButtonClickHandler;

            ToolMode = TileToolMode.Draw;
        }

        public ToolStrip Strip
        {
            get { return _strip; }
        }

        public ToolStripButton ButtonSelect
        {
            get { return _tbSelect; }
        }

        public ToolStripButton ButtonDraw
        {
            get { return _tbDraw; }
        }

        public ToolStripButton ButtonErase
        {
            get { return _tbErase; }
        }

        public ToolStripButton ButtonFill
        {
            get { return _tbFill; }
        }

        public ToolStripButton ButtonStamp
        {
            get { return _tbStamp; }
        }

        public ToolStripButton ButtonFlipH
        {
            get { return _tbFlipH; }
        }

        public ToolStripButton ButtonFlipV
        {
            get { return _tbFlipV; }
        }

        public ToolStripButton ButtonRotateLeft
        {
            get { return _tbRotateLeft; }
        }

        public ToolStripButton ButtonRotateRight
        {
            get { return _tbRotateRight; }
        }

        public TileToolMode ToolMode
        {
            get { return _toolMode; }
            set
            {
                if (_toolMode == value)
                    return;

                _toolMode = value;

                if (_tbSelect.Checked) 
                    _tbSelect.Checked = false;
                if (_tbDraw.Checked)
                    _tbDraw.Checked = false;
                if (_tbErase.Checked)
                    _tbErase.Checked = false;
                if (_tbFill.Checked)
                    _tbFill.Checked = false;
                if (_tbStamp.Checked)
                    _tbStamp.Checked = false;

                switch (value) {
                    case TileToolMode.Select:
                        _tbSelect.Checked = true;
                        break;
                    case TileToolMode.Draw:
                        _tbDraw.Checked = true;
                        break;
                    case TileToolMode.Erase:
                        _tbErase.Checked = true;
                        break;
                    case TileToolMode.Fill:
                        _tbFill.Checked = true;
                        break;
                    case TileToolMode.Stamp:
                        _tbStamp.Checked = true;
                        break;
                }

                OnTileModeChanged(new TileToolModeEventArgs(_toolMode));
            }
        }

        protected void OnTileModeChanged (TileToolModeEventArgs e)
        {
            if (ToolModeChanged != null) {
                ToolModeChanged(this, e);
            }
        }

        private void SelectButtonClickHandler (object sender, EventArgs e)
        {
            ToolMode = TileToolMode.Select;
        }

        private void DrawButtonClickHandler (object sender, EventArgs e)
        {
            ToolMode = TileToolMode.Draw;
        }

        private void EraseButtonClickHandler (object sender, EventArgs e)
        {
            ToolMode = TileToolMode.Erase;
        }

        private void FillButtonClickHandler (object sender, EventArgs e)
        {
            ToolMode = TileToolMode.Fill;
        }

        private void StampButtonClickHandler (object sender, EventArgs e)
        {
            ToolMode = TileToolMode.Stamp;
        }

        private ToolStripButton CreateButton (string text, string resource)
        {
            ToolStripButton button = new ToolStripButton();

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            button.Size = new System.Drawing.Size(22, 22);
            button.Text = text;
            button.Image = Image.FromStream(_assembly.GetManifestResourceStream(resource));

            return button;
        }
    }
}
