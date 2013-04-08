using LilyPath;
using Microsoft.Xna.Framework;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Render.Annotations
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

            if (FillGlow != null)
                drawBatch.FillCircle(FillGlow, center, radius + 2);
            if (Fill != null)
                drawBatch.FillCircle(Fill, center, radius);
            if (OutlineGlow != null)
                drawBatch.DrawCircle(OutlineGlow, center, radius);
            if (Outline != null) {
                if (Outline is PrimitivePen)
                    drawBatch.DrawPrimitiveCircle(Outline, center, radius);
                else
                    drawBatch.DrawCircle(Outline, center, radius);
            }
        }
    }
}
