using System.Collections.Generic;

namespace Treefrog.Pipeline.ImagePacker
{
    public class Page
    {
        public string ImageName { get; set; }
        public List<Rect> OutputRects { get; set; }
        public List<Rect> RemainingRects { get; set; }
        public float Occupancy { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
