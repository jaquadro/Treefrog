using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework.Imaging
{
    public class TextureResource
    {
        private const int _bytesPerPixel = 4;

        private readonly int _width;
        private readonly int _height;
        private readonly byte[] _data;

        public TextureResource (int width, int height)
        {
            _width = width;
            _height = height;
            _data = new byte[width * height * _bytesPerPixel];
        }

        public TextureResource (Size size)
            : this(size.Width, size.Height)
        {
        }

        public TextureResource (int width, int height, byte[] data)
            : this(width, height, data, 0)
        {
        }

        public TextureResource (Size size, byte[] data)
            : this(size.Width, size.Height, data, 0)
        {
        }

        public TextureResource (int width, int height, byte[] data, int dataOffset)
            : this(width, height)
        {
            if (DataLength > data.Length + dataOffset)
                throw new ArgumentException("source data too small for image size");

            Array.Copy(data, dataOffset, _data, 0, DataLength);
        }

        public TextureResource (Size size, byte[] data, int dataOffset)
            : this(size.Width, size.Height, data, dataOffset)
        {
        }

        public int Height
        {
            get { return _height; }
        }

        public int Width
        {
            get { return _width; }
        }

        public byte[] RawData
        {
            get { return _data; }
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(0, 0, _width, _height); }
        }

        public Size Size
        {
            get { return new Size(_width, _height); }
        }

        public int ScanlineSize
        {
            get { return _width * _bytesPerPixel; }
        }

        private int DataLength
        {
            get { return _width * _height * _bytesPerPixel; }
        }

        public TextureResource Crop (Rectangle region)
        {
            Rectangle rect = ClampRectangle(region, Bounds);

            if (Rectangle.IsAreaNegativeOrEmpty(rect))
                return new TextureResource(0, 0);

            TextureResource sub = new TextureResource(rect.Width, rect.Height);

            int priScan = ScanlineSize;
            int subScan = sub.ScanlineSize;

            int sourceOffset = rect.Y * priScan + rect.X * _bytesPerPixel;
            for (int y = 0; y < rect.Height; y++) {
                int sourceIndex = sourceOffset + y * priScan;
                int destIndex = y * subScan;
                Array.Copy(_data, sourceIndex, sub._data, destIndex, subScan);
            }

            return sub;
        }

        public void Set (TextureResource data, Point location)
        {
            Rectangle rect = ClampRectangle(new Rectangle(location, data.Size), Bounds);

            if (Rectangle.IsAreaNegativeOrEmpty(rect))
                return;

            int dataScan = data.ScanlineSize;
            int clampScan = rect.Width * _bytesPerPixel;

            int sourceOffset = (rect.Y - location.Y) * dataScan + (rect.X - location.X) * _bytesPerPixel;
            int targetOffset = rect.Y * ScanlineSize + rect.X * _bytesPerPixel;
            for (int y = 0; y < rect.Height; y++) {
                int sourceIndex = sourceOffset + y * dataScan;
                int destIndex = targetOffset + y * ScanlineSize;
                Array.Copy(data._data, sourceIndex, _data, destIndex, clampScan);
            }
        }

        public Color this[int x, int y]
        {
            get
            {
                int index = y * ScanlineSize + x * _bytesPerPixel;
                return new Color(_data[index + 0], _data[index + 1], _data[index + 2], _data[index + 3]);
            }

            set
            {
                int index = y * ScanlineSize + x * _bytesPerPixel;
                _data[index + 0] = value.R;
                _data[index + 1] = value.G;
                _data[index + 2] = value.B;
                _data[index + 3] = value.A;
            }
        }

        public void Apply (Func<Color, Color> pixelFunc, Rectangle region)
        {
            Rectangle rect = ClampRectangle(region, Bounds);

            if (Rectangle.IsAreaNegativeOrEmpty(rect))
                return;

            int sourceOffset = rect.Y * ScanlineSize + rect.X * _bytesPerPixel;
            for (int y = 0; y < rect.Height; y++) {
                int lineIndex = sourceOffset + y * ScanlineSize;
                for (int x = 0; x < rect.Width; x++) {
                    int index = lineIndex + x * _bytesPerPixel;
                    Color result = pixelFunc(new Color(
                        _data[index + 0], 
                        _data[index + 1], 
                        _data[index + 2], 
                        _data[index + 3]));
                    _data[index + 0] = result.R;
                    _data[index + 1] = result.G;
                    _data[index + 2] = result.B;
                    _data[index + 3] = result.A;
                }
            }
        }

        public void Apply (Func<Color, int, int, Color> pixelFunc, Rectangle region)
        {
            Rectangle rect = ClampRectangle(region, Bounds);

            if (Rectangle.IsAreaNegativeOrEmpty(rect))
                return;

            int sourceOffset = rect.Y * ScanlineSize + rect.X * _bytesPerPixel;
            for (int y = 0; y < rect.Height; y++) {
                int lineIndex = sourceOffset + y * ScanlineSize;
                for (int x = 0; x < rect.Width; x++) {
                    int index = lineIndex + x * _bytesPerPixel;
                    Color result = pixelFunc(new Color(
                        _data[index + 0],
                        _data[index + 1],
                        _data[index + 2],
                        _data[index + 3]), rect.X + x, rect.Y + y);
                    _data[index + 0] = result.R;
                    _data[index + 1] = result.G;
                    _data[index + 2] = result.B;
                    _data[index + 3] = result.A;
                }
            }
        }

        public void Apply (Func<Color, Color> pixelFunc)
        {
            Apply(pixelFunc, Bounds);
        }

        public void Apply (Func<Color, int, int, Color> pixelFunc)
        {
            Apply(pixelFunc, Bounds);
        }

        public void Clear (Rectangle region)
        {
            Apply(c => { return Colors.Transparent; }, region);
        }

        private Rectangle ClampRectangle (Rectangle rect, Rectangle bounds)
        {
            if (rect.X < bounds.X) {
                rect.Width += rect.X;
                rect.X = bounds.X;
            }
            if (rect.Y < bounds.Y) {
                rect.Height += rect.Y;
                rect.Y = bounds.Y;
            }

            if (rect.X + rect.Width > bounds.X + bounds.Width)
                rect.Width = bounds.X + bounds.Width - rect.X;
            if (rect.Y + rect.Height > bounds.Y + bounds.Height)
                rect.Height = bounds.Y + bounds.Height - rect.Y;

            return rect;
        }
    }
}

namespace Treefrog.Aux
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;

    using TextureResource = Treefrog.Framework.Imaging.TextureResource;

    public static class TextureResourceBitmapExt
    {
        public static TextureResource CreateTextureResource (Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (image.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ArgumentException("image must be in 32bppArgb format");

            TextureResource tex = new TextureResource(image.Width, image.Height);

            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptr, tex.RawData, 0, tex.RawData.Length);

            image.UnlockBits(bmpData);

            return tex;
        }

        public static Bitmap CreateBitmap (this TextureResource self)
        {
            Bitmap bmp = new Bitmap(self.Width, self.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0, self.Width, self.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(self.RawData, 0, ptr, self.RawData.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}

namespace Treefrog.Aux
{
    using Microsoft.Xna.Framework.Graphics;

    using TextureResource = Treefrog.Framework.Imaging.TextureResource;

    public static class TextureResourceXnaExt
    {
        public static TextureResource CreateTextureResource (Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (texture.Format != SurfaceFormat.Color)
                throw new ArgumentException("texture must be in Color format");

            TextureResource tex = new TextureResource(texture.Width, texture.Height);
            texture.GetData(tex.RawData);

            return tex;
        }

        public static Texture2D CreateTexture (this TextureResource self, GraphicsDevice device)
        {
            Texture2D texture = new Texture2D(device, self.Width, self.Height, false, SurfaceFormat.Color);
            texture.SetData(self.RawData);

            return texture;
        }
    }
}