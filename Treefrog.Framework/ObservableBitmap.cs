using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace Treefrog.Framework
{
    public class BitmapChangedEventArgs : EventArgs
    {
        public Rectangle Region { get; private set; }

        public BitmapChangedEventArgs (Rectangle region)
        {
            Region = region;
        }
    }

    public class ObservableBitmap
    {
        private Bitmap _bitmap;

        public ObservableBitmap (Bitmap bitmap)
        {
            _bitmap = new Bitmap(bitmap);
        }

        public Bitmap Bitmap
        {
            get { return _bitmap; }
        }

        public event EventHandler<BitmapChangedEventArgs> BitmapModified = (s, e) => { };

        protected virtual void OnBitmapModified (BitmapChangedEventArgs e)
        {
            BitmapModified(this, e);
        }

        public void Clear (Brush brush)
        {
            Clear(brush, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
        }

        public void Clear (Brush brush, Rectangle region)
        {
            using (Graphics g = Graphics.FromImage(_bitmap)) {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(brush, region);
            }

            Rectangle clampedDest = ClampRect(region, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
            if (clampedDest.Width > 0 && clampedDest.Height > 0)
                OnBitmapModified(new BitmapChangedEventArgs(clampedDest));
        }

        public void SetFrom (Bitmap source)
        {
            _bitmap = new Bitmap(source);
            OnBitmapModified(new BitmapChangedEventArgs(new Rectangle(0, 0, source.Width, source.Height)));
        }

        public void CopyFrom (Bitmap source)
        {
            CopyFrom(source, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
        }

        public void CopyFrom (Bitmap source, Rectangle destRect)
        {
            CopyFrom(source, destRect, new Rectangle(0, 0, source.Width, source.Height));
        }

        public void CopyFrom (Bitmap source, Rectangle destRect, Rectangle sourceRect)
        {
            if (_bitmap.PixelFormat != source.PixelFormat)
                throw new ArgumentException("Incompatible pixel format", "source");

            using (Graphics g = Graphics.FromImage(_bitmap)) {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(source, destRect, sourceRect, GraphicsUnit.Pixel);
            }

            Rectangle clampedDest = ClampRect(destRect, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
            if (clampedDest.Width > 0 && clampedDest.Height > 0)
                OnBitmapModified(new BitmapChangedEventArgs(clampedDest));
        }

        public void CopyFrom (byte[] source)
        {
            CopyFrom(source, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
        }

        public void CopyFrom (byte[] source, Rectangle destRect)
        {
            CopyFrom(source, destRect, 0);
        }

        public void CopyFrom (byte[] source, Rectangle destRect, int startIndex)
        {
            int length = destRect.Width * destRect.Height * BytesPerPixel(_bitmap.PixelFormat);
            if (source.Length - startIndex < length)
                throw new ArgumentException("Destination rectangle invalid size for supplied data");

            using (Bitmap bmp = new Bitmap(destRect.Width, destRect.Height, _bitmap.PixelFormat)) {
                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(source, startIndex, data.Scan0, length);
                bmp.UnlockBits(data);

                CopyFrom(bmp, destRect);
            }
        }

        public void CopyTo (Bitmap dest)
        {
            CopyTo(dest, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
        }

        public void CopyTo (Bitmap dest, Rectangle sourceRect)
        {
            CopyTo(dest, sourceRect, new Rectangle(0, 0, dest.Width, dest.Height));
        }

        public void CopyTo (Bitmap dest, Rectangle sourceRect, Rectangle destRect)
        {
            if (_bitmap.PixelFormat != dest.PixelFormat)
                throw new ArgumentException("Incompatible pixel format", "dest");

            using (Graphics g = Graphics.FromImage(dest)) {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(_bitmap, destRect, sourceRect, GraphicsUnit.Pixel);
            }
        }

        public void CopyTo (byte[] dest)
        {
            CopyTo(dest, new Rectangle(0, 0, _bitmap.Width, _bitmap.Height));
        }

        public void CopyTo (byte[] dest, Rectangle sourceRect)
        {
            CopyTo(dest, sourceRect, 0);
        }

        public void CopyTo (byte[] dest, Rectangle sourceRect, int startIndex)
        {
            int length = sourceRect.Width * sourceRect.Height * BytesPerPixel(_bitmap.PixelFormat);
            if (dest.Length - startIndex < length)
                throw new ArgumentException("Source rectangle invalid size for supplied data");

            using (Bitmap bmp = new Bitmap(sourceRect.Width, sourceRect.Height, _bitmap.PixelFormat)) {
                CopyTo(bmp, sourceRect);

                BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                Marshal.Copy(data.Scan0, dest, startIndex, length);
                bmp.UnlockBits(data);
            }
        }

        private int BytesPerPixel (PixelFormat pf)
        {
            return (int)Math.Ceiling(Image.GetPixelFormatSize(pf) / 8.0);
        }

        private Rectangle ClampRect (Rectangle rect, Rectangle bounds)
        {
            if (rect.X + rect.Width > bounds.Width)
                rect.Width = bounds.Width - rect.X;
            if (rect.Y + rect.Height > bounds.Height)
                rect.Height = bounds.Height - rect.Y;

            if (rect.X < 0) {
                rect.Width += rect.X;
                rect.X = 0;
            }
            if (rect.Y < 0) {
                rect.Height += rect.Y;
                rect.Y = 0;
            }

            if (rect.Width < 0)
                rect.Width = 0;
            if (rect.Height < 0)
                rect.Height = 0;

            return rect;
        }
    }
}
