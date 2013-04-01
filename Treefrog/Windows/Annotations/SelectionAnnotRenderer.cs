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

            if (FillGlow != null)
                drawBatch.FillRectangle(FillGlow, new Rectangle(rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2));
            if (Fill != null)
                drawBatch.FillRectangle(Fill, rect);
            if (OutlineGlow != null)
                drawBatch.DrawRectangle(OutlineGlow, rect);
            if (Outline != null) {
                if (Outline is PrimitivePen)
                    drawBatch.DrawPrimitiveRectangle(Outline, rect);
                else
                    drawBatch.DrawRectangle(Outline, rect);
            }
        }
    }
}
