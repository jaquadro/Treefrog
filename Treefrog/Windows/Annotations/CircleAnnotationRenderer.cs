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

            Vector2 center = new Vector2((int)(_data.Center.X * zoomFactor), (int)(_data.Center.Y * zoomFactor));
            float radius = _data.Radius * zoomFactor;

            //if (Fill != null)
            //    drawBatch.FillRectangle(rect, Fill);
            if (Outline != null) {
                if (Outline is PrimitivePen)
                    drawBatch.DrawPrimitiveCircle(Outline, center, radius);
                else
                    drawBatch.DrawCircle(Outline, center, radius);
            }
        }
    }
}
