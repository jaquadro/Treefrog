using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework
{
    public class OrderedResourceCollection<T> : NamedResourceCollection<T>
        where T : INamedResource
    {
        private List<string> _order;

        public event EventHandler<OrderedResourceEventArgs<T>> ResourceOrderChanged;

        public OrderedResourceCollection ()
            : base()
        {
            _order = new List<string>();
        }

        public int IndexOf (string name)
        {
            if (!_order.Contains(name)) {
                throw new ArgumentException("No resource with the given name", "name");
            }

            return _order.IndexOf(name);
        }

        protected override void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            if (e.Resource != null) {
                _order.Add(e.Resource.Name);
                base.OnResourceAdded(e);
            }
        }

        protected override void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            if (e.Resource != null) {
                _order.Remove(e.Resource.Name);
                base.OnResourceRemoved(e);
            }
        }

        protected override void OnResourceRemapped (NamedResourceRemappedEventArgs<T> e)
        {
            int index = _order.IndexOf(e.OldName);
            _order.Insert(index, e.NewName);
            _order.Remove(e.OldName);

            base.OnResourceRemapped(e);
        }

        protected virtual void OnResourceOrderChanged (OrderedResourceEventArgs<T> e)
        {
            if (ResourceOrderChanged != null) {
                ResourceOrderChanged(this, e);
            }
            OnModified(EventArgs.Empty);
        }

        public override IEnumerator<T> GetEnumerator ()
        {
            foreach (string name in _order) {
                yield return this[name];
            }
        }

        public void ChangeIndexRelative (string name, int offset)
        {
            if (offset == 0) {
                return;
            }

            if (!_order.Contains(name)) {
                throw new ArgumentException("No resource with the given name", "name");
            }

            int index = _order.IndexOf(name);
            if (index + offset < 0 || index + offset >= _order.Count) {
                throw new ArgumentOutOfRangeException("offset", "The relative offset results in an invalid index for this item");
            }

            _order.Remove(name);
            _order.Insert(index + offset, name);

            OnResourceOrderChanged(new OrderedResourceEventArgs<T>(this[name], index));
        }
    }
}
