using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;

namespace Treefrog.V2.ViewModel.Annotations
{
    public abstract class Annotation
    {
    }

    public class SelectionAnnot : Annotation
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

        public Brush Fill { get; set; }
        public Pen Outline { get; set; }

        public void Normalize ()
        {
            Point newStart = new Point(Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y));
            Point newEnd = new Point(Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));

            Start = newStart;
            End = newEnd;
        }
    }
}
