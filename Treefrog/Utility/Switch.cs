using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Utility
{
    static class Switch
    {
        public static Switch<T> On<T> (T o)
        {
            return new Switch<T>(o);
        }

        public static Switch<T, R> On<T, R> (T o)
        {
            return new Switch<T, R>(o);
        }
    }

    class Switch<T>
    {
        public Switch (T o)
        {
            Object = o;
        }

        public T Object { get; private set; }
    }

    class Switch<T, R>
    {
        public Switch (T o)
        {
            Object = o;
        }

        public T Object { get; private set; }
        public bool HasValue { get; private set; }
        public R Value { get; private set; }

        public void Set (R value)
        {
            Value = value;
            HasValue = true;
        }
    }

    static class SwitchExt
    {
        public static Switch<T> Case<T> (this Switch<T> s, T t, Action<T> a)
        {
            return Case(s, x => object.Equals(x, t), a);
        }

        public static Switch<T> Case<T> (this Switch<T> s, Func<T, bool> c, Action<T> a)
        {
            if (s == null) {
                return null;
            }
            else if (c(s.Object)) {
                a(s.Object);
                return null;
            }

            return s;
        }

        public static void Default<T> (this Switch<T> s, Action<T> a)
        {
            if (s != null) {
                a(s.Object);
            }
        }

        public static Switch<T, R> Case<T, R> (this Switch<T, R> s, T t, Func<T, R> f)
        {
            return Case<T, R>(s, x => object.Equals(x, t), f);
        }

        public static Switch<T, R> Case<T, R> (this Switch<T, R> s, T t, R r)
        {
            return Case<T, R>(s, x => object.Equals(x, t), r);
        }

        public static Switch<T, R> Case<T, R> (this Switch<T, R> s, Func<T, bool> c, Func<T, R> f)
        {
            if (!s.HasValue && c(s.Object)) {
                s.Set(f(s.Object));
            }

            return s;
        }

        public static Switch<T, R> Case<T, R> (this Switch<T, R> s, Func<T, bool> c, R r)
        {
            if (!s.HasValue && c(s.Object)) {
                s.Set(r);
            }

            return s;
        }

        public static R Default<T, R> (this Switch<T, R> s, Func<T, R> f)
        {
            if (!s.HasValue) {
                s.Set(f(s.Object));
            }

            return s.Value;
        }

        public static R Default<T, R> (this Switch<T, R> s, R r)
        {
            if (!s.HasValue) {
                s.Set(r);
            }

            return s.Value;
        }
    }
}
