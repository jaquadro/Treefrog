using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows;

namespace Treefrog.V2.Controls.Layers
{
    public class GridRenderLayer : RenderLayer
    {
        private int _tileHeight = 16;
        private int _tileWidth = 16;
        private Color _gridColor = new Color(0, 0, 0, .5f);

        private Texture2D _tileGridBrush;
        private Texture2D _tileGridBrushRight;
        private Texture2D _tileGridBrushBottom;

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            return;

            if (_tileGridBrush == null)
                BuildTileBrush(spriteBatch.GraphicsDevice);

            Vector offset = BeginDraw(spriteBatch);

            Rect region = VisibleRegion;
            Rectangle tileRegion = new Rectangle(
                (int)(region.X / _tileWidth),
                (int)(region.Y / _tileHeight),
                (int)(region.Width + region.X % _tileWidth + _tileWidth - 1) / _tileWidth,
                (int)(region.Height + region.Y % _tileHeight + _tileHeight - 1) / _tileHeight
                );

            for (int x = tileRegion.X; x <= tileRegion.X + tileRegion.Width; x++) {
                for (int y = tileRegion.Y; y <= tileRegion.Y + tileRegion.Height; y++) {
                    Vector2 pos = new Vector2(x * _tileWidth * (float)ZoomFactor, y * _tileHeight * (float)ZoomFactor);

                    if (x < tileRegion.Right && y < tileRegion.Bottom)
                        spriteBatch.Draw(_tileGridBrush, pos, Color.White);
                    else if (x == tileRegion.Right && y < tileRegion.Bottom)
                        spriteBatch.Draw(_tileGridBrushRight, pos, Color.White);
                    else if (x < tileRegion.Right && y == tileRegion.Bottom)
                        spriteBatch.Draw(_tileGridBrushBottom, pos, Color.White);
                }
            }

            EndDraw(spriteBatch, offset);
        }

        private void BuildTileBrush (GraphicsDevice device)
        {
            int x = (int)(_tileWidth * ZoomFactor);
            int y = (int)(_tileHeight * ZoomFactor);

            Color[] colors = new Color[x * y];
            Color[] right = new Color[x * y];
            Color[] bottom = new Color[x * y];
            for (int i = 0; i < x; i++) {
                if (i % 4 != 2) {
                    colors[i] = _gridColor;
                    bottom[i] = _gridColor;
                }
            }
            for (int i = 0; i < y; i++) {
                if (i % 4 != 2) {
                    colors[i * x] = _gridColor;
                    right[i * x] = _gridColor;
                }
            }

            _tileGridBrush = new Texture2D(device, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            _tileGridBrushRight = new Texture2D(device, x, y, false, SurfaceFormat.Color);
            _tileGridBrushRight.SetData(right);

            _tileGridBrushBottom = new Texture2D(device, x, y, false, SurfaceFormat.Color);
            _tileGridBrushBottom.SetData(bottom);
        }

        protected override void OnZoomFactorChanged (DependencyPropertyChangedEventArgs e)
        {
            _tileGridBrush = null;
        }
    }
}
