using System;

namespace Treefrog.Framework
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SpecialPropertyAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Readonly { get; set; }
        public Type Converter { get; set; }
    }
}
