using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;

namespace Treefrog.Framework
{
    public abstract class TileTransform
    {
        public abstract TextureResource Transform (TextureResource data, int width, int height);

        public abstract TextureResource InverseTransform (TextureResource data, int width, int height);
    }

    public class Rotate90Transform : TileTransform
    {
        public override TextureResource Transform (TextureResource data, int width, int height)
        {
            //byte[] tdata = new byte[width * height];

            TextureResource transData = new TextureResource(width, height);
            transData.Apply((c, x, y) =>
            {
                return data[height - x, width - y];
            });

            /*for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = (width - x) * height + y;
                    tdata[tindex] = data.RawData[dindex];
                }
            }*/

            return transData;
        }

        public override TextureResource InverseTransform (TextureResource data, int width, int height)
        {
            //byte[] tdata = new byte[width * height];

            TextureResource transData = new TextureResource(width, height);
            transData.Apply((c, x, y) =>
            {
                return data[y, x];
            });

            /*for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = x * height + (height - y);
                    tdata[tindex] = data[dindex];
                }
            }*/

            return transData;
        }
    }

    public class FlipHTransform : TileTransform
    {
        public override TextureResource Transform (TextureResource data, int width, int height)
        {
            //byte[] tdata = new byte[width * height];

            TextureResource transData = new TextureResource(width, height);
            transData.Apply((c, x, y) =>
            {
                return data[width - x, y];
            });

            /*for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = y * width + (width - x);
                    tdata[tindex] = data[dindex];
                }
            }*/

            return transData;
        }

        public override TextureResource InverseTransform (TextureResource data, int width, int height)
        {
            return Transform(data, width, height);
        }
    }
}
