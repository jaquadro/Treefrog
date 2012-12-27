using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Treefrog.Utility
{
    public class ObservableCollectionAdapter<TPrimary, TDependent>
    {
        ObservableCollection<TPrimary> _primary;
        ObservableCollection<TDependent> _dependent;

        Func<TPrimary, TDependent> _addFunc;

        public ObservableCollectionAdapter (Func<TPrimary, TDependent> addFunc)
        {
            _addFunc = addFunc;
            _dependent = new ObservableCollection<TDependent>();
        }

        public ObservableCollectionAdapter (Func<TPrimary, TDependent> addFunc, ObservableCollection<TPrimary> primary)
            : this(addFunc)
        {
            Primary = primary;
        }

        public ObservableCollection<TPrimary> Primary
        {
            get { return _primary; }
            set
            {
                if (_primary != value) {
                    _dependent.Clear();

                    if (_primary != null)
                        _primary.CollectionChanged -= HandlePrimaryCollectionChanged;

                    _primary = value;
                    if (_primary != null) {
                        _primary.CollectionChanged += HandlePrimaryCollectionChanged;

                        foreach (TPrimary item in _primary)
                            _dependent.Add(_addFunc(item));
                    }
                }
            }
        }

        public ObservableCollection<TDependent> Dependent
        {
            get { return _dependent; }
        }

        private void HandlePrimaryCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    _dependent.Insert(e.NewStartingIndex, _addFunc(_primary[e.NewStartingIndex]));
                    break;
                case NotifyCollectionChangedAction.Move:
                    _dependent.Move(e.OldStartingIndex, e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _dependent.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    _dependent.RemoveAt(e.OldStartingIndex);
                    _dependent.Insert(e.OldStartingIndex, _addFunc(_primary[e.OldStartingIndex]));
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _dependent.Clear();
                    break;
            }
        }
    }
}
