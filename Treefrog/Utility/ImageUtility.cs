using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Treefrog.Aux;
using TextureResource = Treefrog.Framework.Imaging.TextureResource;

namespace Treefrog.Utility
{
    internal static class ImageUtility
    {
        public static Bitmap CreateCenteredBitmap (TextureResource source, int width, int height)
        {
            using (Bitmap tmp = source.CreateBitmap()) {
                return CreateCenteredBitmap(tmp, width, height);
            }
        }

        public static Bitmap CreateCenteredBitmap (Bitmap source, int width, int height)
        {
            if (source == null)
                return null;

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

                x = (width - (int)(scale * source.Width)) / 2;
                y = (height - (int)(scale * source.Height)) / 2;

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
