
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
    }
}
