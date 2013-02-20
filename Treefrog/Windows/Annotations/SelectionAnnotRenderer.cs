using System;
using LilyPath;
using Microsoft.Xna.Framework;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Windows.Annotations
{
    public class SelectionAnnotRenderer : DrawAnnotationRenderer
    {
        private SelectionAnnot _data;

        public SelectionAnnotRenderer (SelectionAnnot data)
            : base(data)
        {
            _data = data;
        }

        public override void Render (DrawBatch drawBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            InitializeResources(drawBatch.GraphicsDevice);

            Rectangle rect = new Rectangle(
                (int)(Math.Min(_data.Start.X, _data.End.X) * zoomFactor),
                (int)(Math.Min(_data.Start.Y, _data.End.Y) * zoomFactor),
                (int)(Math.Abs(_data.End.X - _data.Start.X) * zoomFactor),
                (int)(Math.Abs(_data.End.Y - _data.Start.Y) * zoomFactor)
                );

            if (Fill != null)
                drawBatch.FillRectangle(rect, Fill);
            if (Outline != null)
                drawBatch.DrawRectangle(rect, Outline);
        }
    }
}
