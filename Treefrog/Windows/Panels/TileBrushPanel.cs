using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Presentation;
using Treefrog.Framework.Model;

using TextureResource = Treefrog.Framework.Imaging.TextureResource;
using Treefrog.Windows.Forms;
using Treefrog.Presentation.Commands;
using Treefrog.Utility;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Panels
{
    public partial class TileBrushPanel : UserControl
    {
        private ITileBrushManagerPresenter _controller;
        private ITilePoolListPresenter _tileController;

        private UICommandController _commandController;

        public TileBrushPanel ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.paint-brush--minus.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.paint-brush--plus.png"));
            _buttonFilter.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.funnel.png"));

            ToolStripMenuItem buttonAddStatic = new ToolStripMenuItem("New Static Brush...") {
                Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.stamp.png")),
            };
            ToolStripMenuItem buttonAddDynamic = new ToolStripMenuItem("New Dynamic Brush...") {
                Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.table-dynamic.png")),
            };

            _buttonAdd.DropDownItems.AddRange(new ToolStripItem[] {
                buttonAddStatic, buttonAddDynamic,
            });

            _commandController = new UICommandController();
            _commandController.MapButtons(new Dictionary<CommandKey, ToolStripButton>() {
                { CommandKey.TileBrushDelete, _buttonRemove },
            });
            _commandController.MapMenuItems(new Dictionary<CommandKey, ToolStripMenuItem>() {
                { CommandKey.NewStaticTileBrush, buttonAddStatic },
                { CommandKey.NewDynamicTileBrush, buttonAddDynamic },
            });

            // Wire Events

            _listView.ItemSelectionChanged += ListViewSelectionChangedHandler;
            _listView.MouseClick += ListViewItemActivateHandler;
            _listView.MouseDoubleClick += ListViewMouseDoubleClick;
        }

        public void BindController (ITileBrushManagerPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncTileBrushManager -= SyncTileBrushManagerHandler;
                _controller.SyncTileBrushCollection -= SyncTileBrushCollectionHandler;
                _controller.SyncCurrentBrush -= SyncCurrentBrushHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncTileBrushManager += SyncTileBrushManagerHandler;
                _controller.SyncTileBrushCollection += SyncTileBrushCollectionHandler;
                _controller.SyncCurrentBrush += SyncCurrentBrushHandler;

                _commandController.BindCommandManager(_controller.CommandManager);
            }
            else {
                _commandController.BindCommandManager(null);
            }
        }

        public void BindTileController (ITilePoolListPresenter controller)
        {
            _tileController = controller;
        }

        protected override void OnSizeChanged (EventArgs e)
        {
            base.OnSizeChanged(e);

            toolStrip1.CanOverflow = false;

            int width = toolStrip1.Width - _buttonAdd.Width - _buttonRemove.Width - _buttonFilter.Width - toolStripSeparator1.Width - toolStrip1.Padding.Horizontal - _buttonAdd.Margin.Horizontal - _buttonRemove.Margin.Horizontal - _buttonFilter.Margin.Horizontal - toolStripSeparator1.Margin.Horizontal - _filterSelection.Margin.Horizontal - 1;
            _filterSelection.Size = new Size(width, _filterSelection.Height);
        }

        private void ListViewSelectionChangedHandler (object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (_controller != null) {
                if (!e.IsSelected)
                    _controller.ActionSelectBrush(Guid.Empty);
                else
                    _controller.ActionSelectBrush((Guid)e.Item.Tag);
            }
        }

        private void ListViewItemActivateHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                foreach (ListViewItem item in _listView.SelectedItems) {
                    _controller.ActionSelectBrush((Guid)item.Tag);
                    return;
                }
            }
        }

        private void ListViewMouseDoubleClick (object sender, MouseEventArgs e)
        {
            if (_controller != null) {
                foreach (ListViewItem item in _listView.SelectedItems) {
                    _controller.ActionEditBrush((Guid)item.Tag);
                    return;
                }
            }
        }

        private void SyncTileBrushManagerHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                ImageList imgList = BuildImageList();
                PopulateList(imgList);
            }
        }

        private void SyncTileBrushCollectionHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                ImageList imgList = BuildImageList();
                PopulateList(imgList);
            }
        }

        private void SyncCurrentBrushHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                if (_controller.SelectedBrush == null)
                    _listView.SelectedItems.Clear();
                else if (_listView.SelectedItems.Count == 0 || (Guid)_listView.SelectedItems[0].Tag != _controller.SelectedBrush.Uid) {
                    _listView.SelectedItems.Clear();
                    foreach (ListViewItem item in _listView.Items) {
                        if (_controller.SelectedBrush.Uid == (Guid)item.Tag)
                            item.Selected = true;
                    }
                }
            }
        }

        private void ResetComponent ()
        {
            _filterSelection.Items.Clear();
            _filterSelection.Text = "";
        }

        private void PopulateList (ImageList imgList)
        {
            _listView.Clear();
            _listView.LargeImageList = imgList;

            foreach (TileBrush brush in _controller.TileBrushManager.Brushes) {
                _listView.Items.Add(new ListViewItem(brush.Name, brush.Uid.ToString()) { Tag = brush.Uid });
            }
        }

        private ImageList BuildImageList ()
        {
            if (_controller == null || _controller.TileBrushManager == null)
                return null;

            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(64, 64);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            foreach (DynamicTileBrush brush in _controller.TileBrushManager.DynamicBrushes) {
                imgList.Images.Add(brush.Uid.ToString(), CreateCenteredBitmap(brush.MakePreview(64, 64), 64, 64));
            }
            foreach (StaticTileBrush brush in _controller.TileBrushManager.StaticBrushes) {
                imgList.Images.Add(brush.Uid.ToString(), CreateCenteredBitmap(brush.MakePreview(64, 64), 64, 64));
            }

            return imgList;
        }

        private Bitmap CreateCenteredBitmap (TextureResource source, int width, int height)
        {
            using (Bitmap tmp = source.CreateBitmap()) {
                return CreateCenteredBitmap(tmp, width, height);
            }
        }

        private Bitmap CreateCenteredBitmap (Bitmap source, int width, int height)
        {
            if (source == null)
                return new Bitmap(width, height);

            Bitmap dest = new Bitmap(width, height, source.PixelFormat);
            int x = Math.Max(0, (width - source.Width) / 2);
            int y = Math.Max(0, (height - source.Height) / 2);
            int w = Math.Min(width, source.Width);
            int h = Math.Min(height, source.Height);

            Rectangle srcRect = new Rectangle(Point.Empty, source.Size);
            Point[] destPoints = new Point[] {
                new Point(x, y), new Point(x + w, y), new Point(x, y + h),
            };
            Rectangle destRect = new Rectangle(x, y, w, h);

            if (source.Width > width || source.Height > height) {
                double aspectRatio = source.Width * 1.0 / source.Height;
                double scale = (aspectRatio > 1)
                    ? (width * 1.0 / source.Width) : (height * 1.0 / source.Height);

                destRect = new Rectangle(x, y, (int)(scale * source.Width), (int)(scale * source.Height));
            }

            using (Graphics g = Graphics.FromImage(dest)) {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(source, destRect);
            }

            return dest;
        }
    }
}
