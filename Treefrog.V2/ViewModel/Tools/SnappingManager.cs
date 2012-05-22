using Treefrog.Framework.Imaging;

namespace Treefrog.ViewModel.Tools
{
    public class SnappingManager
    {
        private int _gridX;
        private int _gridY;

        private Point _origin;
        private Rectangle _bounds;

        public SnappingManager (Point subjectOrigin, Rectangle subjectBounds, Size gridSize)
        {
            _gridX = gridSize.Width;
            _gridY = gridSize.Height;

            _origin = subjectOrigin;
            _bounds = subjectBounds;
        }

        public Point Translate (Point coord, ObjectSnappingTarget mode)
        {
            switch (mode) {
                case ObjectSnappingTarget.TopLeft:
                    return new Point(SnapXLeft(coord.X), SnapYTop(coord.Y));
                case ObjectSnappingTarget.TopRight:
                    return new Point(SnapXRight(coord.X), SnapYTop(coord.Y));
                case ObjectSnappingTarget.BottomLeft:
                    return new Point(SnapXLeft(coord.X), SnapYBottom(coord.Y));
                case ObjectSnappingTarget.BottomRight:
                    return new Point(SnapXRight(coord.X), SnapYBottom(coord.Y));
                case ObjectSnappingTarget.Top:
                    return new Point(coord.X, SnapYTop(coord.Y));
                case ObjectSnappingTarget.Bottom:
                    return new Point(coord.X, SnapYBottom(coord.Y));
                case ObjectSnappingTarget.Left:
                    return new Point(SnapXLeft(coord.X), coord.Y);
                case ObjectSnappingTarget.Right:
                    return new Point(SnapXRight(coord.X), coord.Y);
                case ObjectSnappingTarget.CenterHorizontal:
                    return new Point(coord.X, SnapYCenter(coord.Y));
                case ObjectSnappingTarget.CenterVertical:
                    return new Point(SnapXCenter(coord.X), coord.Y);
                case ObjectSnappingTarget.Center:
                    return new Point(SnapXCenter(coord.X), SnapYCenter(coord.Y));
                case ObjectSnappingTarget.None:
                default:
                    return coord;
            }
        }

        private int SnapXLeft (int x)
        {
            return (int)(x / _gridX) * _gridX + (_origin.X - _bounds.Left);
        }

        private int SnapXRight (int x)
        {
            return (int)(x / _gridX + 1) * _gridX + (_origin.X - _bounds.Right);
        }

        private int SnapXCenter (int x)
        {
            return (SnapXLeft(x) + SnapXRight(x)) / 2;
        }

        private int SnapYTop (int y)
        {
            return (int)(y / _gridY) * _gridY + (_origin.Y - _bounds.Top);
        }

        private int SnapYBottom (int y)
        {
            return (int)(y / _gridY + 1) * _gridY + (_origin.Y - _bounds.Bottom);
        }

        private int SnapYCenter (int y)
        {
            return (SnapYTop(y) + SnapYBottom(y)) / 2;
        }
    }
}
