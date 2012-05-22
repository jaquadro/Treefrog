using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Utility
{
    public class OrderedToObservableAdapter<TPrimary, TDependent>
        where TPrimary : INamedResource
    {
        private OrderedResourceCollection<TPrimary> _primary;
        private ObservableCollection<TDependent> _dependent;

        private Dictionary<TPrimary, TDependent> _map;

        private Func<TPrimary, TDependent> _addFunc;

        public OrderedToObservableAdapter (Func<TPrimary, TDependent> addFunc)
        {
            _dependent = new ObservableCollection<TDependent>();
            _addFunc = addFunc;

            _map = new Dictionary<TPrimary, TDependent>();
        }

        public OrderedToObservableAdapter (Func<TPrimary, TDependent> addFunc, OrderedResourceCollection<TPrimary> primary)
            : this(addFunc)
        {
            Primary = primary;
        }

        public OrderedResourceCollection<TPrimary> Primary
        {
            get { return _primary; }
            set
            {
                if (_primary != value) {
                    _dependent.Clear();

                    if (_primary != null) {
                        _primary.ResourceAdded -= HandleResourceAdded;
                        _primary.ResourceRemoved -= HandleResourceRemoved;
                        _primary.ResourceOrderChanged -= HandleResourceOrderChanged;
                    }

                    _primary = value;
                    if (_primary != null) {
                        _primary.ResourceAdded += HandleResourceAdded;
                        _primary.ResourceRemoved += HandleResourceRemoved;
                        _primary.ResourceOrderChanged += HandleResourceOrderChanged;

                        foreach (TPrimary item in _primary) {
                            _map[item] = _addFunc(item);
                            _dependent.Add(_map[item]);
                        }
                    }
                }
            }
        }

        public ObservableCollection<TDependent> Dependent
        {
            get { return _dependent; }
        }

        public TDependent Lookup (TPrimary value)
        {
            TDependent result;
            if (_map.TryGetValue(value, out result))
                return result;
            return default(TDependent);
        }

        private void HandleResourceAdded (object sender, NamedResourceEventArgs<TPrimary> e)
        {
            _map[e.Resource] = _addFunc(e.Resource);
            _dependent.Add(_map[e.Resource]);
        }

        private void HandleResourceRemoved (object sender, NamedResourceEventArgs<TPrimary> e)
        {
            _dependent.Remove(_map[e.Resource]);
            _map.Remove(e.Resource);
        }

        private void HandleResourceOrderChanged (object sender, OrderedResourceEventArgs<TPrimary> e)
        {
            int oldIndex = e.Order;
            int newIndex = _primary.IndexOf(e.Resource.Name);

            _dependent.Move(oldIndex, newIndex);
        }
    }
}
