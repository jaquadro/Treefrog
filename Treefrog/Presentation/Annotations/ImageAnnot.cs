using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.Presentation.Annotations
{
    public class ImageAnnot : Annotation
    {
        private TextureResource _image;

        public Point Position { get; set; }
        //public Point End { get; set; }

        public Color BlendColor { get; set; }

        public TextureResource Image
        {
            get { return _image; }
            set
            {
                if (_image != value) {
                    _image = value;
                    OnImageInvalidated(EventArgs.Empty);
                }
            }
        }

        public void MoveTo (Point location)
        {
            int diffX = location.X - Position.X;
            int diffY = location.Y - Position.Y;
            MoveBy(diffX, diffY);
        }

        public void MoveBy (int diffX, int diffY)
        {
            Position = new Point(Position.X + diffX, Position.Y + diffY);
            //End = new Point(End.X + diffX, End.Y + diffY);
        }

        public event EventHandler ImageInvalidated;

        protected virtual void OnImageInvalidated (EventArgs e)
        {
            var ev = ImageInvalidated;
            if (ev != null)
                ev(this, e);
        }
    }
}
