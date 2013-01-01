using System;
using System.IO;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Presentation.Layers;
using Treefrog.Windows.Controls;
using Treefrog.Windows.Layers;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Treefrog.Windows.Forms
{
    public partial class ImportTilePool : Form
    {
        Project _project;

        LayerGraphicsControl _layerControl;

        TexturePool _localTexturePool;
        TilePoolManager _localManager;
        Stream _fileStream;

        private GroupLayerPresenter _rootLayer;
        private TileSetLayerPresenter _previewLayer;

        private bool _useTransColor;
        private Color _transColor;

        int _width;
        int _height;

        public ImportTilePool (Project project)
        {
            InitializeComponent();

            _project = project;

            _localTexturePool = new TexturePool();
            _localManager = new TilePoolManager(_localTexturePool);

            _buttonOK.Enabled = false;

            _layerControl = new LayerGraphicsControl();
            _layerControl.Dock = DockStyle.Fill;
            _layerControl.WidthSynced = true;
            _layerControl.CanvasAlignment = CanvasAlignment.UpperLeft;
            _layerControl.TextureCache.SourcePool = _localManager.TexturePool;

            _rootLayer = new GroupLayerPresenter();
            _layerControl.RootLayer = new GroupLayer(_rootLayer);

            _previewPanel.Controls.Add(_layerControl);

            _message.Text = "";

            _buttonTransColor.Click += ButtonTransColorClickHandler;
            _checkboxTransColor.Click += CheckboxTransColorClickHandler;
            _layerControl.MouseDown += PreviewControlClickHandler;
        }

        private void _buttonBrowse_Click (object sender, EventArgs e)
        {
            if (_fileStream != null) {
                _fileStream.Close();
                _fileStream = null;
            }

            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Filter = "Image Files (*.bmp,*.gif,*.jpg,*.png)|*.bmp;*.gif;*.jpg;*.jpeg;*.png|All files (*.*)|*.*";
            dlg.FilterIndex = 0;
            dlg.RestoreDirectory = true;
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                try {
                    _fileStream = dlg.OpenFile();
                    if (_fileStream != null) {
                        _textPath.Text = dlg.FileName;

                        FileInfo();

                        LoadFile();
                        CheckValid();
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void _buttonOK_Click (object sender, EventArgs e)
        {
            TilePool pool = LoadFile(_project.TilePoolManager);
            if (pool != null) {
                if (_checkboxTransColor.Checked) {
                    System.Drawing.Color c = _buttonTransColor.Color;
                }
                _project.TilePoolManager.MergePool(pool.Name, pool);
            }

            Close();
        }

        private void _buttonCancel_Click (object sender, EventArgs e)
        {
            if (_fileStream != null) {
                _fileStream.Close();
            }
        }

        private TilePool LoadFile ()
        {
            return LoadFile(_localManager);
        }

        private void FileInfo ()
        {
            TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(_fileStream);

            _width = resource.Width;
            _height = resource.Height;

            _fileStream.Position = 0;
        }

        private TilePool _previewPool;
        private TextureResource _originalResource;

        private TilePool LoadFile (TilePoolManager manager)
        {
            if (_fileStream == null) {
                return null;
            }

            if (_fileStream.Position != 0) {
                _fileStream.Position = 0;
            }

            _localManager.Reset();

            TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(_fileStream);
            TilePool.TileImportOptions options = new TilePool.TileImportOptions()
            {
                TileHeight = (int)_numTileHeight.Value,
                TileWidth = (int)_numTileWidth.Value,
                SpaceX = (int)_numXSpacing.Value,
                SpaceY = (int)_numYSpacing.Value,
                MarginX = (int)_numXMargin.Value,
                MarginY = (int)_numYMargin.Value,
                ImportPolicty = TileImportPolicy.SetUnique,
            };

            _previewPool = _localManager.ImportTilePool(_textName.Text, resource, options);
            _originalResource = _previewPool.TileSource.Crop(_previewPool.TileSource.Bounds);

            if (_useTransColor)
                SetTransparentColor();

            // Update preview window

            if (_previewLayer != null)
                _previewLayer.Dispose();

            Model.TileSetLayer layer = new Model.TileSetLayer(_previewPool.Name, _previewPool);
            _previewLayer = new TileSetLayerPresenter(layer) {
                LevelGeometry = _layerControl.LevelGeometry,
            };

            _rootLayer.Layers.Clear();
            _rootLayer.Layers.Add(_previewLayer);

            // Update stats

            _countTilesHigh.Text = ((_height + (int)_numYSpacing.Value) / ((int)_numTileHeight.Value + (int)_numYSpacing.Value + (int)_numYMargin.Value)).ToString();
            _countTilesWide.Text = ((_width + (int)_numXSpacing.Value) / ((int)_numTileWidth.Value + (int)_numXSpacing.Value + (int)_numXMargin.Value)).ToString();
            _countUniqueTiles.Text = _previewPool.Count.ToString();

            return _previewPool;
        }

        private void SetTransparentColor ()
        {
            if (_previewPool != null) {
                SetTransparentColor(_previewPool.TileSource);
                _previewPool.ReplaceTexture(_previewPool.TileSource);
            }
        }

        private void SetTransparentColor (TextureResource resource)
        {
            ClearTransparentColor(resource);
            resource.Apply(c => {
                if (c.Equals(_transColor))
                    return Colors.Transparent;
                else
                    return c;
            });
        }

        private void ClearTransparentColor ()
        {
            if (_previewPool != null) {
                ClearTransparentColor(_previewPool.TileSource);
                _previewPool.ReplaceTexture(_previewPool.TileSource);
            }
        }

        private void ClearTransparentColor (TextureResource resource)
        {
            resource.Set(_originalResource, Point.Zero);
        }

        private void ButtonTransColorClickHandler (object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog() {
                SolidColorOnly = true,
                Color = _buttonTransColor.Color,
                FullOpen = true,
            };

            DialogResult result = cd.ShowDialog(this);

            _buttonTransColor.Color = cd.Color;
            _transColor = new Color(cd.Color.R, cd.Color.G, cd.Color.B);

            if (_useTransColor)
                SetTransparentColor();
        }

        private void CheckboxTransColorClickHandler (object sender, EventArgs e)
        {
            _useTransColor = _checkboxTransColor.Checked;
            if (_useTransColor)
                SetTransparentColor();
            else
                ClearTransparentColor();
        }

        private void PreviewControlClickHandler (object sender, MouseEventArgs e)
        {
            XnaColor color = _layerControl.GetPixel(e.X, e.Y);

            _buttonTransColor.Color = System.Drawing.Color.FromArgb(255, color.R, color.G, color.B);
            _transColor = new Color(color.R, color.G, color.B);

            if (_useTransColor)
                SetTransparentColor();
        }

        private void _numTileHeight_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numTileWidth_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numXSpacing_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numYSpacing_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numXMargin_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _numYMargin_ValueChanged (object sender, EventArgs e)
        {
            LoadFile();
        }

        private void _textName_TextChanged (object sender, EventArgs e)
        {
            CheckValid();
        }

        private void CheckValid ()
        {
            string txt = _textName.Text.Trim();
            if (txt.Length > 0 && !_project.TilePools.Contains(txt) && _fileStream != null) {
                _buttonOK.Enabled = true;
            }
            else {
                _buttonOK.Enabled = false;
            }

            if (_project.TilePools.Contains(txt)) {
                _message.Text = "A resouce with the given name already exists.";
            }
            else {
                _message.Text = "";
            }
        }

        private void TranspButtonPaintHandler (object sender, PaintEventArgs e)
        {
            
        }
    }
}
