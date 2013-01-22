using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework;

namespace Treefrog.Presentation.Annotations
{
    public class SelectionAnnot : DrawAnnotation
    {
        public SelectionAnnot ()
            : this(new Point(0, 0))
        {
        }

        public SelectionAnnot (Point start)
        {
            Start = start;
            End = start;
        }

        public Point Start { get; set; }
        public Point End { get; set; }

        public void Normalize ()
        {
            Point newStart = new Point(Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y));
            Point newEnd = new Point(Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));

            Start = newStart;
            End = newEnd;
        }

        public void MoveTo (Point location)
        {
            int diffX = location.X - Start.X;
            int diffY = location.Y - Start.Y;
            MoveBy(diffX, diffY);
        }

        public void MoveBy (int diffX, int diffY)
        {
            Start = new Point(Start.X + diffX, Start.Y + diffY);
            End = new Point(End.X + diffX, End.Y + diffY);
        }
    }
}
