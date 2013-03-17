using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Ionic.Zlib;
//using System.IO.Compression;

namespace Treefrog.Framework.Imaging
{
    public delegate Color PixelFunction (Color c);
    public delegate Color PixelFunctionXY (Color c, int x, int y);

    public class TextureResource
    {
        public class XmlProxy
        {
            private byte[] _data = new byte[0];

            [XmlAttribute]
            public int Width { get; set; }

            [XmlAttribute]
            public int Height { get; set; }

            [XmlElement(IsNullable = true)]
            public byte[] Data
            {
                get 
                {
                    using (MemoryStream inStr = new MemoryStream(_data)) {
                        using (MemoryStream outStr = new MemoryStream()) {
                            using (DeflateStream zStr = new DeflateStream(outStr, CompressionMode.Compress, true)) {
                                inStr.WriteTo(zStr);
                            }

                            byte[] data = new byte[outStr.Length];
                            Array.Copy(outStr.GetBuffer(), data, data.Length);
                            return data;
                        }
                    }
                }

                set
                {
                    using (MemoryStream inStr = new MemoryStream(value)) {
                        using (MemoryStream outStr = new MemoryStream()) {
                            using (DeflateStream zStr = new DeflateStream(inStr, CompressionMode.Decompress)) {
                                byte[] buf = new byte[4096];
                                int bytesRead = 0;
                                while ((bytesRead = zStr.Read(buf, 0, buf.Length)) > 0) {
                                    outStr.Write(buf, 0, bytesRead);
                                }
                                //zStr.CopyTo(outStr);
                            }

                            _data = new byte[outStr.Length];
                            Array.Copy(outStr.GetBuffer(), _data, _data.Length);
                        }
                    }
                }
            }

            [XmlIgnore]
            public byte[] RawData
            {
                get { return _data; }
                set { _data = value; }
            }
        }

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
            if (data == null)
                return;

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

        public void SetComposite (TextureResource data, Point location)
        {
            Rectangle rect = ClampRectangle(new Rectangle(location, data.Size), Bounds);

            if (Rectangle.IsAreaNegativeOrEmpty(rect))
                return;

            int dataScan = data.ScanlineSize;
            int clampScan = rect.Width * _bytesPerPixel;

            int sourceOffset = (rect.Y - location.Y) * dataScan + (rect.X - location.X) * _bytesPerPixel;
            int targetOffset = rect.Y * ScanlineSize + rect.X * _bytesPerPixel;
            for (int y = 0; y < rect.Height; y++) {
                for (int x = 0; x < rect.Width; x++) {
                    int sourceIndex = sourceOffset + y * dataScan + x * _bytesPerPixel;
                    int destIndex = targetOffset + y * ScanlineSize + x * _bytesPerPixel;

                    float alpha = data._data[sourceIndex + 3] / 255f;
                    _data[destIndex + 0] = (byte)(_data[destIndex + 0] * (1f - alpha) + data._data[sourceIndex + 0] * alpha);
                    _data[destIndex + 1] = (byte)(_data[destIndex + 1] * (1f - alpha) + data._data[sourceIndex + 1] * alpha);
                    _data[destIndex + 2] = (byte)(_data[destIndex + 2] * (1f - alpha) + data._data[sourceIndex + 2] * alpha);
                    _data[destIndex + 3] = (byte)Math.Min(255, _data[destIndex + 3] * (1f - alpha) + data._data[sourceIndex + 3]);
                }
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

        public void Apply (PixelFunction pixelFunc, Rectangle region)
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

        public void Apply (PixelFunctionXY pixelFunc, Rectangle region)
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

        public void Apply (PixelFunction pixelFunc)
        {
            Apply(pixelFunc, Bounds);
        }

        public void Apply (PixelFunctionXY pixelFunc)
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

        public static XmlProxy ToXmlProxy (TextureResource tex)
        {
            if (tex == null)
                return null;

            return new XmlProxy()
            {
                Width = tex._width,
                Height = tex._height,
                RawData = tex._data,
            };
        }

        public static TextureResource FromXmlProxy (XmlProxy proxy)
        {
            if (proxy == null)
                return null;

            return new TextureResource(proxy.Width, proxy.Height, proxy.RawData, 0);
        }
    }
}

namespace Treefrog.Aux
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;

    using TextureResource = Treefrog.Framework.Imaging.TextureResource;
    using TFColor = Treefrog.Framework.Imaging.Color;


    public static class TextureResourceBitmapExt
    {
        public static TextureResource CreateTextureResource (String path)
        {
            using (Bitmap bmp = new Bitmap(path)) {
                if (bmp == null)
                    throw new ArgumentException("Could not create a Bitmap from the supplied path.", "path");
                return CreateTextureResource(bmp);
            }
        }

        public static TextureResource CreateTextureResource (Stream stream)
        {
            using (Bitmap bmp = new Bitmap(stream)) {
                if (bmp == null)
                    throw new ArgumentException("Could not create a Bitmap from the supplied stream.", "stream");
                return CreateTextureResource(bmp);
            }
        }

        public static TextureResource CreateTextureResource (Bitmap image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            TextureResource tex = new TextureResource(image.Width, image.Height);

            if (image.PixelFormat != PixelFormat.Format32bppArgb) {
                using (Bitmap compatImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb)) {
                    compatImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    using (Graphics g = Graphics.FromImage(compatImage)) {
                        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                        g.DrawImage(image, new Point(0, 0));
                    }

                    Rectangle rect = new Rectangle(0, 0, compatImage.Width, compatImage.Height);
                    BitmapData bmpData = compatImage.LockBits(rect, ImageLockMode.ReadOnly, compatImage.PixelFormat);

                    IntPtr ptr = bmpData.Scan0;
                    System.Runtime.InteropServices.Marshal.Copy(ptr, tex.RawData, 0, tex.RawData.Length);

                    compatImage.UnlockBits(bmpData);
                }
            }
            else {

                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(ptr, tex.RawData, 0, tex.RawData.Length);

                image.UnlockBits(bmpData);
            }

            tex.Apply(c => { return new TFColor(c.B, c.G, c.R, c.A); });

            return tex;
        }

        public static Bitmap CreateBitmap (this TextureResource self)
        {
            if (self == null)
                return null;

            TextureResource tex = self.Crop(self.Bounds);

            tex.Apply(c => { return new TFColor(c.B, c.G, c.R, c.A); });

            Bitmap bmp = new Bitmap(tex.Width, tex.Height, PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0, tex.Width, tex.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(tex.RawData, 0, ptr, tex.RawData.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }
    }
}