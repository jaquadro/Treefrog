
namespace Treefrog.Pipeline.ImagePacker
{
    public class Alias
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int[] Splits { get; set; }
        public int[] Pads { get; set; }

        public Alias (Rect rect)
        {
            Name = rect.Name;
            Index = rect.Index;
            Splits = rect.Splits;
            Pads = rect.Pads;
        }

        public void Apply (Rect rect)
        {
            rect.Name = Name;
            rect.Index = Index;
            rect.Splits = Splits;
            rect.Pads = Pads;
        }
    }
}
