using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Treefrog.Framework.Compat;
using Treefrog.Framework.Model.Proxy;
using System.Collections;

namespace Treefrog.Framework.Model
{
    public abstract class TileBrushCollection : INamedResource, IResourceManager<TileBrush>
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
                catch (NameChangeException) {
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

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager<TileBrush>.ResourceAdded
        {
            add { _tileBrushResourceAdded += value; }
            remove { _tileBrushResourceAdded -= value; }
        }

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager<TileBrush>.ResourceRemoved
        {
            add { _tileBrushResourceRemoved += value; }
            remove { _tileBrushResourceRemoved -= value; }
        }

        event EventHandler<ResourceEventArgs<TileBrush>> IResourceManager<TileBrush>.ResourceModified
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
    }

    public class TileBrushCollection<T> : TileBrushCollection
        where T : TileBrush
    {
        protected TileBrushCollection (Guid uid, string name)
            : base(uid, name)
        {
            Brushes = new NamedResourceCollection<T>();

            Brushes.ResourceAdded += (s, e) => OnResourceAdded(e.Resource);
            Brushes.ResourceRemoved += (s, e) => OnResourceRemoved(e.Resource);
            Brushes.ResourceModified += (s, e) => OnResourceModified(e.Resource);
        }

        public TileBrushCollection (string name)
            : this(Guid.NewGuid(), name)
        { }

        public NamedResourceCollection<T> Brushes { get; private set; }

        public int Count
        {
            get { return Brushes.Count; }
        }

        protected override TileBrush GetBrushCore (Guid uid)
        {
            return GetBrush(uid);
        }

        public new T GetBrush (Guid uid)
        {
            foreach (T brush in Brushes) {
                if (brush.Uid == uid)
                    return brush;
            }

            return null;
        }

        protected override IEnumerator<TileBrush> GetTileBrushEnumerator ()
        {
            foreach (TileBrush brush in Brushes)
                yield return brush;
        }

        public static LibraryX.TileBrushCollectionX<TProxy> ToXProxy<TProxy> (TileBrushCollection<T> brushCollection, Func<T, TProxy> itemXmlFunc)
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
                Uid = brushCollection.Uid,
                Name = brushCollection.Name,
                Brushes = objects,
            };
        }
    }

    public class StaticTileBrushCollection : TileBrushCollection<StaticTileBrush>
    {
        public StaticTileBrushCollection (LibraryX.TileBrushCollectionX<LibraryX.StaticTileBrushX> proxy, TilePoolManager tileManager)
            : base(proxy.Uid, proxy.Name)
        {
            if (proxy.Brushes != null) {
                foreach (var brush in proxy.Brushes)
                    Brushes.Add(StaticTileBrush.FromXmlProxy(brush, tileManager));
            }
        }

        public static StaticTileBrushCollection FromXProxy (LibraryX.TileBrushCollectionX<LibraryX.StaticTileBrushX> proxy, TilePoolManager tileManager)
        {
            if (proxy == null)
                return null;

            return new StaticTileBrushCollection(proxy, tileManager);
        }

        public static LibraryX.TileBrushCollectionX<LibraryX.StaticTileBrushX> ToXProxy (StaticTileBrushCollection brushCollection)
        {
            return TileBrushCollection<StaticTileBrush>.ToXProxy<LibraryX.StaticTileBrushX>(brushCollection, StaticTileBrush.ToXmlProxyX);
        }
    }

    public class DynamicTileBrushCollection : TileBrushCollection<DynamicTileBrush>
    {
        public DynamicTileBrushCollection (LibraryX.TileBrushCollectionX<LibraryX.DynamicTileBrushX> proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
            : base(proxy.Uid, proxy.Name)
        {
            if (proxy.Brushes != null) {
                foreach (var brush in proxy.Brushes)
                    Brushes.Add(DynamicTileBrush.FromXmlProxy(brush, tileManager, registry));
            }
        }

        public static DynamicTileBrushCollection FromXProxy (LibraryX.TileBrushCollectionX<LibraryX.DynamicTileBrushX> proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
        {
            if (proxy == null)
                return null;

            return new DynamicTileBrushCollection(proxy, tileManager, registry);
        }

        public static LibraryX.TileBrushCollectionX<LibraryX.DynamicTileBrushX> ToXProxy (DynamicTileBrushCollection brushCollection)
        {
            return TileBrushCollection<DynamicTileBrush>.ToXProxy<LibraryX.DynamicTileBrushX>(brushCollection, DynamicTileBrush.ToXmlProxyX);
        }
    }
}
