using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
{
    public abstract class TileTransform
    {
        public abstract byte[] Transform (byte[] data, int width, int height);

        public abstract byte[] InverseTransform (byte[] data, int width, int height);
    }

    public class Rotate90Transform : TileTransform
    {
        public override byte[] Transform (byte[] data, int width, int height)
        {
            byte[] tdata = new byte[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = (width - x) * height + y;
                    tdata[tindex] = data[dindex];
                }
            }

            return data;
        }

        public override byte[] InverseTransform (byte[] data, int width, int height)
        {
            byte[] tdata = new byte[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = x * height + (height - y);
                    tdata[tindex] = data[dindex];
                }
            }

            return data;
        }
    }

    public class FlipHTransform : TileTransform
    {
        public override byte[] Transform (byte[] data, int width, int height)
        {
            byte[] tdata = new byte[width * height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int dindex = y * width + x;
                    int tindex = y * width + (width - x);
                    tdata[tindex] = data[dindex];
                }
            }

            return data;
        }

        public override byte[] InverseTransform (byte[] data, int width, int height)
        {
            return Transform(data, width, height);
        }
    }
}
