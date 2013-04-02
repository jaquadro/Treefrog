using System;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Annotations
{
    public class CircleAnnot : DrawAnnotation
    {
        public CircleAnnot ()
        { }

        public CircleAnnot (Point center)
        {
            Center = center;
        }

        public CircleAnnot (Point center, float radius)
            : this(center)
        {
            Radius = radius;
        }

        public Point Center { get; set; }
        public float Radius { get; set; }

        public void MoveTo (Point location)
        {
            Center = location;
        }

        public void MoveBy (int diffX, int diffY)
        {
            Center = new Point(Center.X + diffX, Center.Y + diffY);
        }

        public void SizeToBound (Rectangle bound)
        {
            Center = bound.Center;
            Radius = (float)Math.Sqrt(bound.Width * bound.Width + bound.Height * bound.Height) / 2;
        }
    }
}
