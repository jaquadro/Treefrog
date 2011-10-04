using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Amphibian.Drawing;

namespace Editor
{
    /*public class EditorViewport
    {
        private AmphibianGameControl _canvas;
        private HScrollBar _hScrollBar;
        private VScrollBar _vScrollBar;

        private Map _map;
        private Texture2D _tileBrush;

        private float _zoom = 1f;

        public EditorViewport (Form1 form, Map map)
        {
            _canvas = form.Canvas;
            //_hScrollBar = form.CanvasHScroll;
            //_vScrollBar = form.CanvasVScroll;

            _map = map;

            Load();
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                BuildTileBrush();
                Resize();
            }
        }

        protected void Load ()
        {
            BuildTileBrush();
            Resize();

            _canvas.Resize += ResizeHandler;
        }

        public void Draw ()
        {
            Vector2 offset = new Vector2(-_hScrollBar.Value * _zoom, -_vScrollBar.Value * _zoom);
            if (_canvas.Width > _map.PixelsWide * _zoom) {
                offset.X = (_canvas.Width - _map.PixelsWide * _zoom) / 2;
            }
            if (_canvas.Height > _map.PixelsHigh * _zoom) {
                offset.Y = (_canvas.Height - _map.PixelsHigh * _zoom) / 2;
            }

            Matrix tranf = Matrix.CreateTranslation(offset.X, offset.Y, 0);

            _canvas.SpriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, tranf);

            DrawGrid();

            _canvas.SpriteBatch.End();
        }

        private void ResizeHandler (object sender, EventArgs e)
        {
            Resize();
        }

        private void Resize ()
        {

            _hScrollBar.Minimum = 0;
            _vScrollBar.Minimum = 0;
            _hScrollBar.Maximum = (_canvas.Width > _map.PixelsWide * _zoom) ? 0 
                : (int)(_map.PixelsWide * _zoom) - _canvas.Width + (int)(_map.TileWidth * _zoom * 4) - (int)(1 * _zoom);
            _vScrollBar.Maximum = (_canvas.Height > _map.PixelsHigh * _zoom) ? 0 
                : (int)(_map.PixelsHigh * _zoom) - _canvas.Height + (int)(_map.TileHeight * _zoom * 4) - (int)(1 * _zoom);

            _hScrollBar.Maximum = (int)(_hScrollBar.Maximum / _zoom);
            _vScrollBar.Maximum = (int)(_vScrollBar.Maximum / _zoom);

            _hScrollBar.SmallChange = _map.TileWidth / 2;
            _vScrollBar.SmallChange = _map.TileHeight / 2;
            _hScrollBar.LargeChange = _map.TileWidth * 4;
            _vScrollBar.LargeChange = _map.TileHeight * 4;

            _hScrollBar.Value = Math.Min(_hScrollBar.Value, Math.Max(0, _hScrollBar.Maximum - _hScrollBar.LargeChange));
            _vScrollBar.Value = Math.Min(_vScrollBar.Value, Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange));

            if (_hScrollBar.Maximum <= 0) {
                _hScrollBar.Maximum = 0;
                _hScrollBar.Value = 0;
                _hScrollBar.Enabled = false;
            }
            else {
                _hScrollBar.Enabled = true;
            }

            if (_vScrollBar.Maximum <= 0) {
                _vScrollBar.Maximum = 0;
                _vScrollBar.Value = 0;
                _vScrollBar.Enabled = false;
            }
            else {
                _vScrollBar.Enabled = true;
            }
        }

        private void DrawGrid ()
        {
            int offX = _hScrollBar.Value % _map.TileWidth;
            int offY = _vScrollBar.Value % _map.TileHeight;
            int startX = (int)(_hScrollBar.Value * _zoom) - (int)(offX * _zoom);
            int startY = (int)(_vScrollBar.Value * _zoom) - (int)(offY * _zoom);
            int tileX = (int)Math.Ceiling((Math.Min(_canvas.Width, _map.PixelsWide * _zoom) + (offX * _zoom)) / (float)(_map.TileWidth * _zoom));
            int tileY = (int)Math.Ceiling((Math.Min(_canvas.Height, _map.PixelsHigh * _zoom) + (offY * _zoom)) / (float)(_map.TileHeight * _zoom));

            for (int x = 0; x < tileX; x++) {
                for (int y = 0; y < tileY; y++) {
                    Vector2 pos = new Vector2(startX + x * _map.TileWidth * _zoom, startY + y * _map.TileHeight * _zoom);
                    _canvas.SpriteBatch.Draw(_tileBrush, pos, Color.White);
                }
            }

            int roomPixelsWide = (int)(_map.RoomWidth * _map.TileWidth * _zoom);
            int roomPixelsHigh = (int)(_map.RoomHeight * _map.TileHeight * _zoom);

            int lineX = ((startX + roomPixelsWide - 1) / roomPixelsWide) * roomPixelsWide;
            int lineY = ((startY + roomPixelsHigh - 1) / roomPixelsHigh) * roomPixelsHigh;

            for (int x = lineX; x < lineX + Math.Min(_canvas.Width, _map.PixelsWide * _zoom + 1); x += roomPixelsWide) {
                Primitives2D.DrawLine(_canvas.SpriteBatch, new Vector2(x + 1, startY), new Vector2(x + 1, startY + tileY * _map.TileHeight * _zoom), Color.Black);
            }
            for (int y = lineY; y < lineY + Math.Min(_canvas.Height, _map.PixelsHigh * _zoom + 1); y += roomPixelsHigh) {
                Primitives2D.DrawLine(_canvas.SpriteBatch, new Vector2(startX, y), new Vector2(startX + tileX * _map.TileWidth * _zoom, y), Color.Black);
            }
        }

        private void BuildTileBrush ()
        {
            int x = (int)(_map.TileWidth * _zoom);
            int y = (int)(_map.TileHeight * _zoom);

            Color[] colors = new Color[x * y];
            for (int i = 0; i < x; i++) {
                if (i % 4 != 2) {
                    colors[i] = new Color(0, 0, 0, 0.5f);
                }
            }
            for (int i = 0; i < y; i++) {
                if (i % 4 != 2) {
                    colors[i * x] = new Color(0, 0, 0, 0.5f);
                }
            }

            _tileBrush = new Texture2D(_canvas.SpriteBatch.GraphicsDevice, x, y, false, SurfaceFormat.Color);
            _tileBrush.SetData(colors);
        }
    }*/
}
