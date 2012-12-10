﻿using System;
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

namespace Treefrog.Windows.Panels
{
    public partial class TileBrushPanel : UserControl
    {
        private ITileBrushManagerPresenter _controller;

        public TileBrushPanel ()
        {
            InitializeComponent();

            ResetComponent();

            // Load form elements

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            _buttonRemove.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.paint-brush--minus.png"));
            _buttonAdd.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.paint-brush--plus.png"));
            _buttonFilter.Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.funnel.png"));

            ToolStripMenuItem buttonAddDynamic = new ToolStripMenuItem("New Dynamic Brush...") {
                Image = Image.FromStream(assembly.GetManifestResourceStream("Treefrog.Icons._16.table-dynamic.png")),
            };

            _buttonAdd.DropDownItems.AddRange(new ToolStripItem[] {
                buttonAddDynamic,
            });

            // Wire Events

            _listView.ItemSelectionChanged += ListViewSelectionChangedHandler;
            _listView.MouseClick += ListViewItemActivateHandler;
        }

        public void BindController (ITileBrushManagerPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncTileBrushManager -= SyncTileBrushManagerHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncTileBrushManager += SyncTileBrushManagerHandler;
            }
            else {
                ResetComponent();
            }
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
                    _controller.ActionSelectBrush(-1);
                else
                    _controller.ActionSelectBrush((int)e.Item.Tag);
            }
        }

        private void ListViewItemActivateHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                foreach (ListViewItem item in _listView.SelectedItems)
                    _controller.ActionSelectBrush((int)item.Tag);
            }
        }

        private void SyncTileBrushManagerHandler (object sender, EventArgs e)
        {
            if (_controller != null) {
                ImageList imgList = BuildImageList();
                PopulateList(imgList);
            }
        }

        private void ResetComponent ()
        {
            _filterSelection.Items.Clear();
            _filterSelection.Text = "";

            //_buttonAdd.Enabled = false;
            _buttonRemove.Enabled = false;
        }

        private void PopulateList (ImageList imgList)
        {
            _listView.Clear();
            _listView.LargeImageList = imgList;

            foreach (TileBrush brush in _controller.TileBrushManager.Brushes) {
                _listView.Items.Add(new ListViewItem(brush.Name, brush.Id.ToString()) { Tag = brush.Id });
            }
        }

        private ImageList BuildImageList ()
        {
            if (_controller == null || _controller.TileBrushManager == null)
                return null;

            ImageList imgList = new ImageList();
            imgList.ImageSize = new Size(64, 64);
            imgList.ColorDepth = ColorDepth.Depth32Bit;

            foreach (DynamicBrush brush in _controller.TileBrushManager.DynamicBrushes) {
                imgList.Images.Add(brush.Id.ToString(), CreateCenteredBitmap(brush.BrushClass.MakePreview(64, 64), 64, 64));
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
            Bitmap dest = new Bitmap(width, height, source.PixelFormat);
            int x = Math.Max(0, (width - source.Width) / 2);
            int y = Math.Max(0, (height - source.Height) / 2);
            int w = Math.Min(width, source.Width);
            int h = Math.Min(height, source.Height);

            using (Graphics g = Graphics.FromImage(dest)) {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                g.DrawImage(source, x, y, w, h);
            }

            return dest;
        }
    }
}