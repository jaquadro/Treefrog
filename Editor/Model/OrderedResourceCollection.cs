using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Editor.Model
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

        protected override void OnResourceAdded (NamedResourceEventArgs<T> e)
        {
            _order.Add(e.Name);

            base.OnResourceAdded(e);
        }

        protected override void OnResourceRemoved (NamedResourceEventArgs<T> e)
        {
            _order.Remove(e.Name);

            base.OnResourceRemoved(e);
        }

        protected override void OnResourceRemapped (NamedResourceEventArgs<T> e)
        {
            int index = _order.IndexOf(e.Name);
            _order.Insert(index, e.Resource.Name);
            _order.Remove(e.Name);

            base.OnResourceRemapped(e);
        }

        protected virtual void OnResourceOrderChanged (OrderedResourceEventArgs<T> e)
        {
            if (ResourceOrderChanged != null) {
                ResourceOrderChanged(this, e);
            }
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
