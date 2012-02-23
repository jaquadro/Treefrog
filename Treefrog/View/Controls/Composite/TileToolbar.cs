using System;
using System.Windows.Forms;
using System.Reflection;
using Treefrog.Presentation;
using System.Drawing;

namespace Treefrog.View.Controls.Composite
{
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

        private ILevelToolsPresenter _controller;

        public TileToolbar ()
        {
            // Construction

            _assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _tbSelect = CreateButton("Select Tool (C)", "Treefrog.Icons.cursor16.png");
            _tbDraw = CreateButton("Draw Tool (D)", "Treefrog.Icons.paint-brush16.png");
            _tbErase = CreateButton("Erase Tool (E)", "Treefrog.Icons.eraser16.png");
            _tbFill = CreateButton("Fill Tool (F)", "Treefrog.Icons.paint-can16.png");
            _tbStamp = CreateButton("Stamp Tool (S)", "Treefrog.Icons.stamp16.png");

            _tbFlipH = CreateButton("Flip Horizontally", "Treefrog.Icons.layer-flip16.png");
            _tbFlipV = CreateButton("Flip Vertically", "Treefrog.Icons.layer-flip-vertical16.png");
            _tbRotateLeft = CreateButton("Rotate Left 90 Degrees", "Treefrog.Icons.layer-rotate-left16.png");
            _tbRotateRight = CreateButton("Rotate Right 90 Degrees", "Treefrog.Icons.layer-rotate16.png");

            _strip = new ToolStrip();
            _strip.Items.AddRange(new ToolStripItem[] {
                _tbSelect, _tbDraw, _tbErase, _tbFill, _tbStamp, new ToolStripSeparator(),
                _tbFlipH, _tbFlipV, _tbRotateLeft, _tbRotateRight
            });

            ResetComponent();

            _tbSelect.Click += SelectButtonClickHandler;
            _tbDraw.Click += DrawButtonClickHandler;
            _tbErase.Click += EraseButtonClickHandler;
            _tbFill.Click += FillButtonClickHandler;
            _tbStamp.Click += StampButtonClickHandler;

            //ToolMode = TileToolMode.Draw;
        }

        public void BindController (ILevelToolsPresenter controller)
        {
            if (_controller == controller) {
                return;
            }

            if (_controller != null) {
                _controller.SyncLevelToolsActions -= SyncLevelToolsActionsHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncLevelToolsActions += SyncLevelToolsActionsHandler;

                _controller.RefreshLevelTools();
            }
            else {
                ResetComponent();
            }
        }

        private void ResetComponent ()
        {
            _tbSelect.Enabled = false;
            _tbDraw.Enabled = false;
            _tbErase.Enabled = false;
            _tbFill.Enabled = false;
            _tbStamp.Enabled = false;

            _tbFlipH.Enabled = false;
            _tbFlipV.Enabled = false;
            _tbRotateLeft.Enabled = false;
            _tbRotateRight.Enabled = false;
        }

        private void SyncLevelToolsActionsHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                _tbSelect.Enabled = _controller.CanSelect;
                _tbDraw.Enabled = _controller.CanDraw;
                _tbErase.Enabled = _controller.CanErase;
                _tbFill.Enabled = _controller.CanFill;
                _tbStamp.Enabled = _controller.CanStamp;

                _tbSelect.Checked = false;
                _tbDraw.Checked = false;
                _tbErase.Checked = false;
                _tbStamp.Checked = false;
                _tbFill.Checked = false;

                switch (_controller.ActiveTileTool) {
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
            }
        }

        public ToolStrip Strip
        {
            get { return _strip; }
        }

        private void SelectButtonClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionToggleSelect();
        }

        private void DrawButtonClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionToggleDraw();
        }

        private void EraseButtonClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionToggleErase();
        }

        private void FillButtonClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionToggleFill();
        }

        private void StampButtonClickHandler (object sender, EventArgs e)
        {
            if (_controller != null)
                _controller.ActionToggleStamp();
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
