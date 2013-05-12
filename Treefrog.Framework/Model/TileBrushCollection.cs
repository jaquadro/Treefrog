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
        private readonly Guid _uid;
        private readonly ResourceName _name;

        protected TileBrushCollection (Guid uid, string name)
        {
            _uid = uid;
            _name = new ResourceName(this, name);
        }

        public Guid Uid
        {
            get { return _uid; }
        }

        public TileBrush GetBrush (Guid uid)
        {
            return GetBrushCore(uid);
        }

        protected abstract TileBrush GetBrushCore (Guid uid);

        public bool IsModified { get; private set; }

        public virtual void ResetModified ()
        {
            IsModified = false;
            foreach (TileBrush brush in this)
                brush.ResetModified();
        }

        public event EventHandler Modified;

        protected virtual void OnModified (EventArgs e)
        {
            if (!IsModified) {
                IsModified = true;
                var ev = Modified;
                if (ev != null)
                    ev(this, e);
            }
        }

        #region Name Interface

        public event EventHandler<NameChangingEventArgs> NameChanging
        {
            add { _name.NameChanging += value; }
            remove { _name.NameChanging -= value; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged
        {
            add { _name.NameChanged += value; }
            remove { _name.NameChanged -= value; }
        }

        public string Name
        {
            get { return _name.Name; }
        }

        public bool TrySetName (string name)
        {
            bool result = _name.TrySetName(name);
            if (result)
                OnModified(EventArgs.Empty);

            return result;
        }

        #endregion

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

        IResourceCollection<TileBrush> IResourceManager<TileBrush>.Collection
        {
            get { return Collection; }
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

        protected virtual IResourceCollection<TileBrush> Collection
        {
            get { return null; }
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

            Brushes.Modified += (s, e) => OnModified(EventArgs.Empty);
            Brushes.ResourceAdded += (s, e) => OnResourceAdded(e.Resource);
            Brushes.ResourceRemoved += (s, e) => OnResourceRemoved(e.Resource);
            Brushes.ResourceModified += (s, e) => OnResourceModified(e.Resource);
        }

        public TileBrushCollection (string name)
            : this(Guid.NewGuid(), name)
        { }

        public NamedResourceCollection<T> Brushes { get; private set; }

        /*protected override IResourceCollection<TileBrush> Collection
        {
            get { return Brushes; }
        }*/

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
                    Brushes.Add(StaticTileBrush.FromXProxy(brush, tileManager));
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
            return TileBrushCollection<StaticTileBrush>.ToXProxy<LibraryX.StaticTileBrushX>(brushCollection, StaticTileBrush.ToXProxy);
        }
    }

    public class DynamicTileBrushCollection : TileBrushCollection<DynamicTileBrush>
    {
        public DynamicTileBrushCollection (LibraryX.TileBrushCollectionX<LibraryX.DynamicTileBrushX> proxy, TilePoolManager tileManager, DynamicTileBrushClassRegistry registry)
            : base(proxy.Uid, proxy.Name)
        {
            if (proxy.Brushes != null) {
                foreach (var brush in proxy.Brushes)
                    Brushes.Add(DynamicTileBrush.FromXProxy(brush, tileManager, registry));
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
            return TileBrushCollection<DynamicTileBrush>.ToXProxy<LibraryX.DynamicTileBrushX>(brushCollection, DynamicTileBrush.ToXProxy);
        }
    }
}
