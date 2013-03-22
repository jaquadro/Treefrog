using System;
using System.Collections.Generic;

namespace Treefrog.Framework
{
    public class OrderedResourceCollection<T> : ResourceCollection<T>
        where T : IResource
    {
        private List<Guid> _order;

        public event EventHandler<OrderedResourceEventArgs<T>> ResourceOrderChanged;

        public OrderedResourceCollection ()
            : base()
        {
            _order = new List<Guid>();
        }

        public int IndexOf (Guid uid)
        {
            if (!_order.Contains(uid)) {
                throw new ArgumentException("No resource with the given uid", "uid");
            }

            return _order.IndexOf(uid);
        }

        protected override void AddCore (T item)
        {
            _order.Add(item.Uid);

            base.AddCore(item);
        }

        protected override void RemoveCore (T item)
        {
            base.RemoveCore(item);

            _order.Remove(item.Uid);
        }

        protected virtual void OnResourceOrderChanged (OrderedResourceEventArgs<T> e)
        {
            var ev = ResourceOrderChanged;
            if (ev != null)
                ev(this, e);
        }

        public override IEnumerator<T> GetEnumerator ()
        {
            foreach (Guid uid in _order)
                yield return this[uid];
        }

        public void ChangeIndexRelative (Guid uid, int offset)
        {
            if (offset == 0)
                return;

            if (!_order.Contains(uid))
                throw new ArgumentException("No resource with the given uid", "uid");

            int index = _order.IndexOf(uid);
            if (index + offset < 0 || index + offset >= _order.Count) {
                throw new ArgumentOutOfRangeException("offset", "The relative offset results in an invalid index for this item");
            }

            _order.Remove(uid);
            _order.Insert(index + offset, uid);

            OnResourceOrderChanged(new OrderedResourceEventArgs<T>(this[uid], index));
            OnModified(EventArgs.Empty);
        }
    }
}
