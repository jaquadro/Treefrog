using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Xna.Framework.Graphics;

using XRectangle = Microsoft.Xna.Framework.Rectangle;

namespace Treefrog.Framework
{
    /// <summary>
    /// Represents an XNA texture with fallback to System.Drawing.Bitmap if there is no
    /// access to a GraphicsDevice context.
    /// </summary>
    public class TextureSource
    {
        int _width;
        int _height;

        private Bitmap _image;
        private Texture2D _texture;

        public TextureSource (int width, int height)
        {
            _width = width;
            _height = height;
            _image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public TextureSource (int width, int height, GraphicsDevice device)
        {
            _width = width;
            _height = height;

            if (device != null)
                _texture = new Texture2D(device, width, height, false, SurfaceFormat.Color);
            else
                _image = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public TextureSource (Bitmap bitmap)
        {
            _width = bitmap.Width;
            _height = bitmap.Height;
            _image = bitmap;
        }

        public TextureSource (Texture2D texture)
        {
            _width = texture.Width;
            _height = texture.Height;
            _texture = texture;
        }

        public int Height
        {
            get { return _height; }
        }

        public int Width
        {
            get { return _width; }
        }

        public Bitmap Bitmap
        {
            get { return _image; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
        }

        public void Initialize (GraphicsDevice device)
        {
            if (_texture != null && _texture.GraphicsDevice == device)
                return;

            byte[] data = new byte[_width * _height * 4];
            GetData(data);

            _texture = new Texture2D(device, _width, _height, false, SurfaceFormat.Color);
            if (_image != null) {
                _image.Dispose();
                _image = null;
            }

            SetData(data);
        }

        public void GetData (byte[] data)
        {
            if (data.Length != _width * _height * 4)
                throw new ArgumentException("Length of buffer is invalid for the source image data.", "data");

            if (_texture != null) {
                _texture.GetData(data);
            }
            else if (_image != null) {
                Rectangle rect = new Rectangle(0, 0, _image.Width, _image.Height);
                BitmapData bmpData = _image.LockBits(rect, ImageLockMode.ReadOnly, _image.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, data.Length);

                _image.UnlockBits(bmpData);
            }
        }

        public void GetData (byte[] data, XRectangle rect)
        {
            GetData(data, rect, 0);
        }

        public void GetData (byte[] data, XRectangle rect, int startIndex)
        {
            int elems = rect.Width * rect.Height * 4;

            if (startIndex + elems > data.Length)
                throw new ArgumentOutOfRangeException("startIndex", "Not enough space left in data to copy image.");
            if (rect.Left < 0 || rect.Top < 0 || rect.Right > _width || rect.Bottom > _height)
                throw new ArgumentException("Region not within source image bounds.", "rect");

            if (_texture != null) {
                _texture.GetData(0, rect, data, startIndex, elems);
            }
            else if (_image != null) {
                Rectangle srect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                BitmapData bmpData = _image.LockBits(srect, ImageLockMode.ReadOnly, _image.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(ptr, data, startIndex, elems);

                _image.UnlockBits(bmpData);
            }
        }

        public void SetData (byte[] data)
        {
            if (data.Length != _width * _height * 4)
                throw new ArgumentException("Length of buffer is invalid for the source image data.", "data");

            if (_texture != null) {
                _texture.SetData(data);
            }
            else if (_image != null) {
                Rectangle rect = new Rectangle(0, 0, _image.Width, _image.Height);
                BitmapData bmpData = _image.LockBits(rect, ImageLockMode.WriteOnly, _image.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, data.Length);

                _image.UnlockBits(bmpData);
            }
        }

        public void SetData (byte[] data, XRectangle rect)
        {
            SetData(data, rect, 0);
        }

        public void SetData (byte[] data, XRectangle rect, int startIndex)
        {
            int elems = rect.Width * rect.Height * 4;

            if (startIndex + elems > data.Length)
                throw new ArgumentOutOfRangeException("startIndex", "Not enough space left in data to copy image.");
            if (rect.Left < 0 || rect.Top < 0 || rect.Right > _width || rect.Bottom > _height)
                throw new ArgumentException("Region not within source image bounds.", "rect");

            if (_texture != null) {
                _texture.SetData(0, rect, data, startIndex, elems);
            }
            else if (_image != null) {
                Rectangle srect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                BitmapData bmpData = _image.LockBits(srect, ImageLockMode.WriteOnly, _image.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                System.Runtime.InteropServices.Marshal.Copy(data, startIndex, ptr, elems);

                _image.UnlockBits(bmpData);
            }
        }

        public Bitmap CopyBitmap ()
        {
            Bitmap bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);

            byte[] data = new byte[_width * _height * 4];
            GetData(data);

            Rectangle rect = new Rectangle(0, 0, _width, _height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, data.Length);

            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public Texture2D CopyTexture ()
        {
            if (_texture == null)
                return null;

            return CopyTexture(_texture.GraphicsDevice);
        }

        public Texture2D CopyTexture (GraphicsDevice device)
        {
            byte[] data = new byte[_width * _height * 4];
            GetData(data);

            Texture2D texture = new Texture2D(device, _width, _height, false, SurfaceFormat.Color);
            texture.SetData(data);

            return texture;
        }
    }
}
