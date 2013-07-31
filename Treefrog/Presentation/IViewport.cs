using System.Drawing;

namespace Treefrog.Presentation
{
    public interface IViewport
    {
        Point Offset { get; set; }
        Size Viewport { get; }
        Size Limit { get; }
        float ZoomFactor { get; }

        Rectangle VisibleRegion { get; }
    }

    public enum CanvasAlignment
    {
        Center,
        Left,
        Right,
        Upper,
        Lower,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    public interface ILevelGeometry
    {
        Treefrog.Framework.Imaging.Point ScrollPosition { get; set; }

        /// <summary>
        /// Bounding rectangle of the level.
        /// </summary>
        Treefrog.Framework.Imaging.Rectangle LevelBounds { get; set; }

        /// <summary>
        /// Bounding rectangle of control viewport.
        /// </summary>
        Treefrog.Framework.Imaging.Rectangle ViewportBounds { get; }

        /// <summary>
        /// Bounding rectangle of the zoom-adjusted visible canvas area.
        /// </summary>
        Treefrog.Framework.Imaging.Rectangle VisibleBounds { get; }
        
        /// <summary>
        /// Bounding rectangle of the zoom-adjusted canvas relative to the viewport origin.
        /// </summary>
        Treefrog.Framework.Imaging.Rectangle CanvasBounds { get; }

        /// <summary>
        /// Alignment of zoom-adjusted canvas content within viewport if smaller.
        /// </summary>
        CanvasAlignment CanvasAlignment { get; }

        /// <summary>
        /// Zoom factor currently in effect.
        /// </summary>
        float ZoomFactor { get; set; }
    }
}
