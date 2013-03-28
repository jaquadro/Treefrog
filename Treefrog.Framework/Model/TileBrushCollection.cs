using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System.Collections;

namespace Treefrog.Framework.Model
{
    public abstract class TileBrushCollection : INamedResource, IResourceManager2<TileBrush>
    {
        private string _name;

        protected TileBrushCollection (Guid uid, string name)
        {
            Uid = uid;
            _name = name;
        }

        public Guid Uid { get; private set; }

        public TileBrush GetBrush (Guid uid)
        {
            return GetBrushCore(uid);
        }

        protected abstract TileBrush GetBrushCore (Guid uid);

        public event EventHandler<NameChangingEventArgs> NameChanging;
        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanging (NameChangingEventArgs e)
        {
            var ev = NameChanging;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            var ev = NameChanged;
            if (ev != null)
                ev(this, e);
        }

        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public bool TrySetName (string name)
        {
            if (Name != name) {
                try {
                    OnNameChanging(new NameChangingEventArgs(Name, name));
                }
                catch (KeyProviderException) {
                    return false;
                }

                NameChangedEventArgs e = new NameChangedEventArgs(Name, name);
                Name = name;

                OnNameChanged(e);
                OnModified(EventArgs.Empty);
            }

            return true;
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            var ev = Modified;
            if ((ev = Modified) != null)
                ev(this, e);
        }

        #region Resource Manager Explicit Interface

        private EventHandler<ResourceEventArgs<TileBrush>> _tileBrushResourceAdded;
        private EventHandler<ResourceEventArgs<TileBrush>> _tileBrushResourceRemoved;
        private EventHandler<ResourceEventArgs<TileBrush>> _tileBrushResourceModified;

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager2<TileBrush>.ResourceAdded
        {
            add { _tileBrushResourceAdded += value; }
            remove { _tileBrushResourceAdded -= value; }
        }

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager2<TileBrush>.ResourceRemoved
        {
            add { _tileBrushResourceRemoved += value; }
            remove { _tileBrushResourceRemoved -= value; }
        }

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager2<TileBrush>.ResourceModified
        {
            add { _tileBrushResourceModified += value; }
            remove { _tileBrushResourceModified -= value; }
        }

        IEnumerator<TileBrush> System.Collections.Generic.IEnumerable<TileBrush>.GetEnumerator ()
        {
            return GetTileBrushEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetTileBrushEnumerator();
        }

        protected virtual void OnResourceAdded (TileBrush resource)
        {
            var ev = _tileBrushResourceAdded;
            if (ev != null)
                ev(this, new ResourceEventArgs<TileBrush>(resource));
        }

        protected virtual void OnResourceRemoved (TileBrush resource)
        {
            var ev = _tileBrushResourceRemoved;
            if (ev != null)
                ev(this, new ResourceEventArgs<TileBrush>(resource));
        }

        protected virtual void OnResourceModified (TileBrush resource)
        {
            var ev = _tileBrushResourceModified;
            if (ev != null)
                ev(this, new ResourceEventArgs<TileBrush>(resource));
        }

        protected virtual IEnumerator<TileBrush> GetTileBrushEnumerator ()
        {
            yield break;
        }

        #endregion

        //#endregion

        /*#region INotifyPropertyChanged Members

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

        #endregion*/
    }

    public class TileBrushCollection<T> : TileBrushCollection
        where T : TileBrush
    {
        private TileBrushManager _manager;
        private NamedResourceCollection<T> _brushes;
        //private NamedObservableCollection<T> _brushes;

        protected TileBrushCollection (string name)
            : base(Guid.NewGuid(), name)
        {
            _brushes = new NamedResourceCollection<T>();
            //_brushes = new NamedObservableCollection<T>();
            //_brushes.CollectionChanged += HandleCollectionChanged;

            _brushes.ResourceAdded += (s, e) => OnResourceAdded(e.Resource);
            _brushes.ResourceRemoved += (s, e) => OnResourceRemoved(e.Resource);
            _brushes.ResourceModified += (s, e) => OnResourceModified(e.Resource);
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

        /*public void AddBrush (T brush)
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
        }*/

        public NamedResourceCollection<T> Brushes
        {
            get { return _brushes; }
        }

        protected override IEnumerator<TileBrush> GetTileBrushEnumerator ()
        {
            foreach (TileBrush brush in Brushes)
                yield return brush;
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

        public static TileBrushCollection<T> FromXmlProxy<TProxy> (LibraryX.TileBrushCollectionX<TProxy> proxy, TileBrushCollection<T> brushCollection, Func<TProxy, T> itemXmlFunc)
            where TProxy : LibraryX.TileBrushX
        {
            if (proxy == null)
                return null;

            if (proxy.Brushes != null) {
                foreach (TProxy brush in proxy.Brushes) {
                    T inst = itemXmlFunc(brush);
                    brushCollection.Brushes.Add(inst);
                }
            }

            return brushCollection;
        }
    }
}
