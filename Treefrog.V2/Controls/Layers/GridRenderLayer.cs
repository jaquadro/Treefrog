﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows;

using XEffect = Microsoft.Xna.Framework.Graphics.Effect;
using System.IO;

namespace Treefrog.Controls.Layers
{
    public class GridRenderLayer : RenderLayer
    {
        private const int MaxBrushSize = 256;

        private int _tileHeight = 16;
        private int _tileWidth = 16;
        private Color _gridColor = new Color(0, 0, 0, .5f);

        private Texture2D _tileGridBrush;
        private Texture2D _tileGridBrushRight;
        private Texture2D _tileGridBrushBottom;

        private int _tileBrushWidth;
        private int _tileBrushHeight;

        public GridRenderLayer ()
        {
        }

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            //return;

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

            for (int x = tileRegion.Left; x < tileRegion.Right; x += _tileBrushWidth) {
                for (int y = tileRegion.Top; y < tileRegion.Bottom; y += _tileBrushHeight) {
                    Vector2 pos = new Vector2(x * _tileWidth * (float)ZoomFactor, y * _tileHeight * (float)ZoomFactor);
                    Rectangle sourceRect = new Rectangle(0, 0,
                        (int)(Math.Min(_tileBrushWidth, tileRegion.Right - x) * _tileWidth * ZoomFactor),
                        (int)(Math.Min(_tileBrushHeight, tileRegion.Bottom - y) * _tileHeight * ZoomFactor)
                        );

                    spriteBatch.Draw(_tileGridBrush, pos, sourceRect, Color.White);
                }
            }

            for (int x = tileRegion.Left; x < tileRegion.Right; x += _tileBrushWidth) {
                Vector2 pos = new Vector2(x * _tileWidth * (float)ZoomFactor, tileRegion.Bottom * _tileHeight * (float)ZoomFactor);
                Rectangle sourceRect = new Rectangle(0, 0,
                    (int)(Math.Min(_tileBrushWidth, tileRegion.Right - x) * _tileWidth * ZoomFactor),
                    _tileGridBrushBottom.Height
                    );

                spriteBatch.Draw(_tileGridBrushBottom, pos, sourceRect, Color.White);
            }

            for (int y = tileRegion.Top; y < tileRegion.Bottom; y += _tileBrushHeight) {
                Vector2 pos = new Vector2(tileRegion.Right * _tileWidth * (float)ZoomFactor, y * _tileHeight * (float)ZoomFactor);
                Rectangle sourceRect = new Rectangle(0, 0,
                    _tileGridBrushRight.Width,
                    (int)(Math.Min(_tileBrushHeight, tileRegion.Bottom - y) * _tileHeight * ZoomFactor)
                    );

                spriteBatch.Draw(_tileGridBrushRight, pos, sourceRect, Color.White);
            }

            EndDraw(spriteBatch, offset);
        }

        private int CalcBrushDimension (int tileDim, int maxDim, double zoomFactor)
        {
            int dim = 1;
            while ((dim + 1) * tileDim * zoomFactor <= maxDim) {
                dim++;
            }

            return dim;
        }

        private void BuildTileBrush (GraphicsDevice device)
        {
            int tilesAcross = CalcBrushDimension(_tileWidth, MaxBrushSize, ZoomFactor);
            int tilesDown = CalcBrushDimension(_tileHeight, MaxBrushSize, ZoomFactor);

            _tileBrushWidth = tilesAcross;
            _tileBrushHeight = tilesDown;

            int zTileWidth = (int)(_tileWidth * ZoomFactor);
            int zTileHeight = (int)(_tileHeight * ZoomFactor);

            int x = (int)(tilesAcross * zTileWidth);
            int y = (int)(tilesDown * zTileHeight);

            Color[] colors = new Color[x * y];
            Color[] right = new Color[1 * y];
            Color[] bottom = new Color[x * 1];

            for (int h = 0; h < tilesDown; h++) {
                for (int i = 0; i < x; i++) {
                    if (i % 4 != 2) {
                        colors[h * x * zTileHeight + i] = _gridColor;
                        bottom[i] = _gridColor;
                    }
                }
            }

            for (int w = 0; w < tilesAcross; w++) {
                for (int i = 0; i < y; i++) {
                    if (i % 4 != 2) {
                        colors[i * x + w * zTileWidth] = _gridColor;
                        right[i] = _gridColor;
                    }
                }
            }

            _tileGridBrush = new Texture2D(device, x, y, false, SurfaceFormat.Color);
            _tileGridBrush.SetData(colors);

            _tileGridBrushRight = new Texture2D(device, 1, y, false, SurfaceFormat.Color);
            _tileGridBrushRight.SetData(right);

            _tileGridBrushBottom = new Texture2D(device, x, 1, false, SurfaceFormat.Color);
            _tileGridBrushBottom.SetData(bottom);
        }

        protected override void OnZoomFactorChanged (DependencyPropertyChangedEventArgs e)
        {
            _tileGridBrush = null;
        }
    }
}
