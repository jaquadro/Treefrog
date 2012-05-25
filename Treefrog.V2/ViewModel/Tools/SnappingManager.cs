using Treefrog.Framework.Imaging;
using System;

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
            int negAdj = (_gridX - (_origin.X - _bounds.Left) % _gridX) % _gridX;
            int posAdj = (_origin.X - _bounds.Left) % _gridX;
            int originSnap = (x / _gridX) * _gridX;

            return ClosestValue(x, new int[] {
                originSnap - negAdj,
                originSnap + posAdj,
                originSnap + _gridX - negAdj,
                originSnap + _gridX + posAdj,
            });
        }

        private int SnapXRight (int x)
        {
            int negAdj = (_origin.X - _bounds.Right) % _gridX;
            int posAdj = (_gridX - (_origin.X - _bounds.Right) % _gridX) % _gridX;
            int originSnap = (x / _gridX) * _gridX;

            return ClosestValue(x, new int[] {
                originSnap - negAdj,
                originSnap + posAdj,
                originSnap + _gridX - negAdj,
                originSnap + _gridX + posAdj,
            });
        }

        private int SnapXCenter (int x)
        {
            int negAdjLeft = (_gridX - (_origin.X - _bounds.Left) % _gridX) %_gridX;
            int posAdjRight = (_gridX - (_origin.X - _bounds.Right) % _gridX) % _gridX;
            int originSnap = (x / _gridX) * _gridX;

            return ClosestValue(x, new int[] {
                originSnap + (posAdjRight - negAdjLeft) / 2,
                originSnap + _gridX + (posAdjRight - negAdjLeft) / 2,
            });
        }

        private int SnapYTop (int y)
        {
            int negAdj = (_gridY - (_origin.Y - _bounds.Top) % _gridY) % _gridY;
            int posAdj = (_origin.Y - _bounds.Top) % _gridY;
            int originSnap = (y / _gridY) * _gridY;

            return ClosestValue(y, new int[] {
                originSnap + negAdj,
                originSnap + posAdj,
                originSnap + _gridY + negAdj,
                originSnap + _gridY + posAdj,
            });
        }

        private int SnapYBottom (int y)
        {
            int negAdj = (_origin.Y - _bounds.Bottom) % _gridY;
            int posAdj = (_gridY - (_origin.Y - _bounds.Bottom) % _gridY) % _gridY;
            int originSnap = (y / _gridY) * _gridY;

            return ClosestValue(y, new int[] {
                originSnap + negAdj,
                originSnap + posAdj,
                originSnap + _gridY + negAdj,
                originSnap + _gridY + posAdj,
            });
        }

        private int SnapYCenter (int y)
        {
            int negAdjTop = (_gridY - (_origin.Y - _bounds.Top) % _gridY) % _gridY;
            int posAdjBottom = (_gridY - (_origin.Y - _bounds.Bottom) % _gridY) % _gridY;
            int originSnap = (y / _gridY) * _gridY;

            return ClosestValue(y, new int[] {
                originSnap + (posAdjBottom - negAdjTop) / 2,
                originSnap + _gridY + (posAdjBottom - negAdjTop) / 2,
            });
        }

        private int ClosestValue (int refVal, int[] candidates)
        {
            int minError = int.MaxValue;
            int minCandidate = 0;

            foreach (int candidate in candidates) {
                int error = Math.Abs(candidate - refVal);
                if (error < minError) {
                    minError = error;
                    minCandidate = candidate;
                }
            }

            return minCandidate;
        }
    }
}
