using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Treefrog.Runtime
{
    public class Property
    {
        private bool? _cacheBool;
        private int? _cacheInt;
        private float? _cacheFloat;

        #region Constructors

        private Property (string name)
        {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            Name = name;
        }

        public Property (string name, string value)
            : this(name)
        {
            Value = value ?? string.Empty;
        }

        public Property (string name, bool value)
            : this(name)
        {
            _cacheBool = value;
            Value = value.ToString();
        }

        public Property (string name, int value)
            : this(name)
        {
            _cacheInt = value;
            Value = value.ToString();
        }

        public Property (string name, float value)
            : this(name)
        {
            _cacheFloat = value;
            Value = value.ToString();
        }

        public Property (Property property)
            : this(property.Name)
        {
            Value = property.Value;
        }

        #endregion

        public string Name { get; private set; }

        public string Value { get; private set; }

        #region Conversion Operators

        public static explicit operator bool (Property p)
        {
            if (!p._cacheBool.HasValue) {
                p._cacheBool = bool.Parse(p.Value);
            }
            return p._cacheBool.Value;
        }

        public static explicit operator int (Property p)
        {
            if (!p._cacheInt.HasValue) {
                p._cacheInt = int.Parse(p.Value, CultureInfo.InvariantCulture);
            }
            return p._cacheInt.Value;
        }

        public static explicit operator float (Property p)
        {
            if (!p._cacheFloat.HasValue) {
                p._cacheFloat = float.Parse(p.Value, CultureInfo.InvariantCulture);
            }
            return p._cacheFloat.Value;
        }

        public static explicit operator string (Property p)
        {
            return p.Value;
        }

        #endregion
    }
}
