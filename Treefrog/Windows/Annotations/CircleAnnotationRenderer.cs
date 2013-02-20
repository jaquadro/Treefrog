using LilyPath;
using Microsoft.Xna.Framework;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Windows.Annotations
{
    public class CircleAnnotRenderer : DrawAnnotationRenderer
    {
        private CircleAnnot _data;

        public CircleAnnotRenderer (CircleAnnot data)
            : base(data)
        {
            _data = data;
        }

        public override void Render (DrawBatch drawBatch, float zoomFactor)
        {
            if (IsDisposed)
                return;

            InitializeResources(drawBatch.GraphicsDevice);

            Point center = new Point((int)(_data.Center.X * zoomFactor), (int)(_data.Center.Y * zoomFactor));
            float radius = _data.Radius * zoomFactor;

            //if (Fill != null)
            //    drawBatch.FillRectangle(rect, Fill);
            if (Outline != null) {
                if (Outline is PrimitivePen)
                    drawBatch.DrawPrimitiveCircle(center, radius, Outline);
                else
                    drawBatch.DrawCircle(center, radius, Outline);
            }
        }
    }
}
