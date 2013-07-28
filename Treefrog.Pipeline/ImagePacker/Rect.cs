using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Treefrog.Pipeline.ImagePacker
{
    public class Rect
    {
        public string Name { get; set; }
        public Bitmap Image { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Index { get; set; }
        public bool Rotated { get; set; }
        public ISet<Alias> Aliases { get; private set; }
        public int[] Splits { get; set; }
        public int[] Pads { get; set; }
        public bool CanRotate { get; set; }

        internal int Score1 { get; set; }
        internal int Score2 { get; set; }

        public Rect (Bitmap source, int left, int top, int newWidth, int newHeight)
            : this()
        {
            Image = new Bitmap(source);
            OffsetX = left;
            OffsetY = top;
            OriginalWidth = source.Width;
            OriginalHeight = source.Height;
            Width = newWidth;
            Height = newHeight;
        }

        public Rect ()
        {
            Aliases = new HashSet<Alias>();
            CanRotate = true;
        }

        public Rect (Rect rect)
            : this()
        {
            SetSize(rect);
        }

        private void SetSize (Rect rect)
        {
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }

        internal void Set (Rect rect)
        {
            Name = rect.Name;
            Image = rect.Image;
            OffsetX = rect.OffsetX;
            OffsetY = rect.OffsetY;
            OriginalWidth = rect.OriginalWidth;
            OriginalHeight = rect.OriginalHeight;
            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
            Index = rect.Index;
            Rotated = rect.Rotated;
            Aliases = rect.Aliases;
            Splits = rect.Splits;
            Pads = rect.Pads;
            CanRotate = rect.CanRotate;

            Score1 = rect.Score1;
            Score2 = rect.Score2;
        }

        public override bool Equals (object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;

            Rect other = obj as Rect;
            if (Name != other.Name)
                return false;

            return true;
        }

        public override string ToString ()
        {
            return string.Format("[{0},{1} {2}x{3}]", X, Y, Width, Height);
        }

        public static string GetAliasName (string name, bool flattenPaths)
        {
            return flattenPaths ? Path.GetFileName(name) : name;
        }
    }
}
