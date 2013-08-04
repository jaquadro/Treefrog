using System;
using System.Reflection;
using System.Collections.Generic;

namespace Treefrog.Utility
{
    public interface IBindable<T>
    {
        void Bind (T controller);
    }

    public static class BindingHelper
    {
        public static bool TryBind<T> (object target, T controller)
        {
            IBindable<T> bindTarget = target as IBindable<T>;
            if (bindTarget != null) {
                bindTarget.Bind(controller);
                return true;
            }

            return false;
        }

        public static bool TryBind (object target, Type type, object controller)
        {
            Type bindTemplate = typeof(IBindable<>);
            Type bindType = bindTemplate.MakeGenericType(type);

            if (bindType.IsAssignableFrom(target.GetType())) {
                bindType.InvokeMember("Bind", BindingFlags.InvokeMethod, null, target, new object[] { controller });
                return true;
            }

            return false;
        }

        public static void TryBindAny (object target, IEnumerable<KeyValuePair<Type, object>> controllers)
        {
            foreach (var kv in controllers) {
                TryBind(target, kv.Key, kv.Value);
            }
        }
    }
}
