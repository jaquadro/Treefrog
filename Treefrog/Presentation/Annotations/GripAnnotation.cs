using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Annotations
{
    public class GripAnnot : DrawAnnotation
    {
        public GripAnnot (Point center)
        {
            Center = center;
        }

        public GripAnnot (Point center, float size)
            : this(center)
        {
            Size = size;
        }

        public Point Center { get; set; }
        public float Size { get; set; }

        public void MoveTo (Point location)
        {
            Center = location;
        }

        public void MoveBy (int diffX, int diffY)
        {
            Center = new Point(Center.X + diffX, Center.Y + diffY);
        }
    }
}
