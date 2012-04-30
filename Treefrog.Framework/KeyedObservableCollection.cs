using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System;

namespace Treefrog.Framework
{
    public interface IKeyProvider<TKey>
    {
        TKey GetKey ();
    }

    public class KeyProviderObservableCollection<TKey, TItem> : KeyedObservableCollection<TKey, TItem>
        where TItem : IKeyProvider<TKey>
    {
        protected override TKey GetKeyForItem (TItem item)
        {
            return item.GetKey();
        }
    }

    public abstract class KeyedObservableCollection<TKey, TItem> : KeyedCollection<TKey, TItem>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;

            public void Dispose ()
            {
                _busyCount--;
            }

            public void Enter ()
            {
                _busyCount++;
            }

            public bool Busy
            {
                get { return (_busyCount > 0); }
            }
        }

        private SimpleMonitor _monitor;

        protected KeyedObservableCollection ()
            : base()
        {
            _monitor = new SimpleMonitor();
        }

        protected KeyedObservableCollection (IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            _monitor = new SimpleMonitor();
        }

        protected KeyedObservableCollection (IEqualityComparer<TKey> comparer, int dictionaryCreationThreshold)
            : base(comparer, dictionaryCreationThreshold)
        {
            _monitor = new SimpleMonitor();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected IDisposable BlockReentrancy ()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void CheckReentrancy ()
        {
            if (_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
                throw new InvalidOperationException("KeyedObservableCollection Reentrancy is not allowed.");
        }

        protected override void ClearItems ()
        {
            CheckReentrancy();
            base.ClearItems();
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionReset();
        }

        protected override void InsertItem (int index, TItem item)
        {
            CheckReentrancy();
            base.InsertItem(index, item);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void Move (int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        protected virtual void MoveItem (int oldIndex, int newIndex)
        {
            CheckReentrancy();
            TItem item = base[oldIndex];
            base.RemoveItem(oldIndex);
            base.InsertItem(newIndex, item);
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
        }

        protected override void RemoveItem (int index)
        {
            CheckReentrancy();
            TItem item = base[index];
            base.RemoveItem(index);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        protected override void SetItem (int index, TItem item)
        {
            CheckReentrancy();
            TItem oldItem = base[index];
            base.SetItem(index, item);
            OnPropertyChanged("Item[]");
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
        }

        protected virtual void OnCollectionChanged (NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null) {
                using (BlockReentrancy()) {
                    CollectionChanged(this, e);
                }
            }
        }

        private void OnCollectionChanged (NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged (NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged (NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset ()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        private void OnPropertyChanged (string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
