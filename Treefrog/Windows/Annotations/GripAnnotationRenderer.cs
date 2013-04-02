using LilyPath;
using Microsoft.Xna.Framework;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Windows.Annotations
{
    public class GripAnnotRenderer : DrawAnnotationRenderer
    {
        private GripAnnot _data;

        public GripAnnotRenderer (GripAnnot data)
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
            float size = _data.Size;

            Rectangle rect = new Rectangle((int)(center.X - size), (int)(center.Y - size), (int)(size * 2), (int)(size * 2));

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
