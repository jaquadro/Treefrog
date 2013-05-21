using Treefrog.Framework.Model;
using System;

namespace Treefrog.Framework
{
    public abstract class PropertyConverter
    {
        public abstract Property Pack (string name, object value);
        public abstract object Unpack (Property property);
    }

    public class RasterModePropertyConverter : PropertyConverter
    {
        public override Property Pack (string name, object value)
        {
            if (!(value is RasterMode))
                return new StringProperty(name, "");
            return new StringProperty(name, ((RasterMode)value).ToString());
        }

        public override object Unpack (Property property)
        {
            StringProperty sp = property as StringProperty;
            switch (sp.Value) {
                case "Point":
                    return Model.RasterMode.Point;
                default:
                    return Model.RasterMode.Linear;
            }
        }
    }

    public class RadToDegPropertyConverter : PropertyConverter
    {
        public override Property Pack (string name, object value)
        {
            float v = Convert.ToSingle(value);
            return new NumberProperty(name, MathEx.RadToDeg(v));
        }

        public override object Unpack (Property property)
        {
            NumberProperty np = property as NumberProperty;
            if (np == null)
                return 0f;
            return MathEx.DegToRad(np.Value);
        }
    }
}
