using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;

namespace Treefrog.Framework.Model
{
    public abstract class TileBrushCollection : IKeyProvider<string>, INotifyPropertyChanged
    {
        private string _name;

        protected TileBrushCollection (string name)
        {
            _name = name;
        }

        public TileBrush GetBrush (Guid uid)
        {
            return GetBrushCore(uid);
        }

        protected abstract TileBrush GetBrushCore (Guid uid);

        #region IKeyProvider<string> Members

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;
        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        public string GetKey ()
        {
            return Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
                RaisePropertyChanged("Name");
            }

            return true;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        protected void RaisePropertyChanged (string name)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(name));
        }

        #endregion
    }

    public class TileBrushCollection<T> : TileBrushCollection, IEnumerable<T>
        where T : TileBrush
    {
        private TileBrushManager _manager;
        private NamedObservableCollection<T> _brushes;
        private Dictionary<Guid, T> _brushIndex;

        protected TileBrushCollection (string name)
            : base(name)
        {
            _brushes = new NamedObservableCollection<T>();
            _brushes.CollectionChanged += HandleCollectionChanged;
        }

        public TileBrushCollection (string name, TileBrushManager manager)
            : this(name)
        {
            _manager = manager;
        }

        public int Count
        {
            get { return _brushes.Count; }
        }

        protected override TileBrush GetBrushCore (Guid uid)
        {
            return GetBrush(uid);
        }

        public new T GetBrush (Guid uid)
        {
            foreach (T brush in _brushes) {
                if (brush.Uid == uid)
                    return brush;
            }

            return null;
        }

        public void AddBrush (T brush)
        {
            Guid uid = _manager.TakeKey();
            AddBrush(brush, uid);
        }

        public void AddBrush (T brush, Guid uid)
        {
            if (_brushes.Contains(brush.Name))
                throw new ArgumentException("Brush collection already contains a brush with the same name as brush.");

            brush.Uid = uid;

            _brushes.Add(brush);

            _manager.LinkItemKey(uid, this);
        }

        public void RemoveBrush (string name)
        {
            if (_brushes.Contains(name)) {
                T objClass = _brushes[name];
                _manager.UnlinkItemKey(objClass.Uid);

                _brushes.Remove(name);
            }
        }

        public NamedObservableCollection<T> Brushes
        {
            get { return _brushes; }
        }

        private void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Brushes");
        }

        #region IEnumerable<ObjectClass> Members

        public IEnumerator<T> GetEnumerator ()
        {
            return _brushes.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _brushes.GetEnumerator();
        }

        #endregion

        [Obsolete]
        public static TileBrushCollectionXmlProxy<TProxy> ToXmlProxy<TProxy> (TileBrushCollection<T> brushCollection, Func<T, TProxy> itemXmlFunc)
            where TProxy : TileBrushXmlProxy
        {
            if (brushCollection == null)
                return null;

            List<TProxy> objects = new List<TProxy>();
            foreach (T brush in brushCollection.Brushes) {
                TProxy brushProxy = itemXmlFunc(brush);
                objects.Add(brushProxy);
            }

            return new TileBrushCollectionXmlProxy<TProxy>() {
                Name = brushCollection.Name,
                Brushes = objects,
            };
        }

        public static LibraryX.TileBrushCollectionX<TProxy> ToXmlProxyX<TProxy> (TileBrushCollection<T> brushCollection, Func<T, TProxy> itemXmlFunc)
            where TProxy : LibraryX.TileBrushX
        {
            if (brushCollection == null)
                return null;

            List<TProxy> objects = new List<TProxy>();
            foreach (T brush in brushCollection.Brushes) {
                TProxy brushProxy = itemXmlFunc(brush);
                objects.Add(brushProxy);
            }

            return new LibraryX.TileBrushCollectionX<TProxy>() {
                Name = brushCollection.Name,
                Brushes = objects,
            };
        }

        [Obsolete]
        public static TileBrushCollection<T> FromXmlProxy<TProxy> (TileBrushCollectionXmlProxy<TProxy> proxy, TileBrushCollection<T> brushCollection, Func<TProxy, T> itemXmlFunc)
            where TProxy : TileBrushXmlProxy
        {
            if (proxy == null)
                return null;

            foreach (TProxy brush in proxy.Brushes) {
                T inst = itemXmlFunc(brush);
                //brushCollection.AddBrush(inst, brush.Id);
            }

            return brushCollection;
        }

        public static TileBrushCollection<T> FromXmlProxy<TProxy> (LibraryX.TileBrushCollectionX<TProxy> proxy, TileBrushCollection<T> brushCollection, Func<TProxy, T> itemXmlFunc)
            where TProxy : LibraryX.TileBrushX
        {
            if (proxy == null)
                return null;

            if (proxy.Brushes != null) {
                foreach (TProxy brush in proxy.Brushes) {
                    T inst = itemXmlFunc(brush);
                    brushCollection.AddBrush(inst, brush.Uid);
                }
            }

            return brushCollection;
        }
    }
}
