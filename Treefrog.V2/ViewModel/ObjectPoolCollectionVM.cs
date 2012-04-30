using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using GalaSoft.MvvmLight;

namespace Treefrog.V2.ViewModel
{
    public interface ObjectPoolManagerService
    {
        ObjectPoolVM ActiveObjectPool { get; }
        ObjectPoolItemVM ActiveObjectClass { get; }
    }


    public class ObjectPoolItemVM : ViewModelBase
    {
        private ObjectClass _objClass;

        public ObjectPoolItemVM (ObjectClass objClass)
        {
            _objClass = objClass;
        }

        public ObjectClass ObjectClass
        {
            get { return _objClass; }
        }

        public string Name
        {
            get { return _objClass.Name; }
        }

        public TextureResource ImageSource
        {
            get { return _objClass.Image; }
        }

        public int ImageWidth
        {
            get { return _objClass.ImageBounds.Width; }
        }

        public int ImageHeight
        {
            get { return _objClass.ImageBounds.Height; }
        }
    }

    public class ObjectPoolVM : ViewModelBase
    {
        private ObjectPool _objectPool;
        private ObservableCollection<ObjectPoolItemVM> _vms;

        public ObjectPoolVM (ObjectPool objectPool)
        {
            _objectPool = objectPool;
            _vms = new ObservableCollection<ObjectPoolItemVM>();

            foreach (ObjectClass objClass in _objectPool) {
                _vms.Add(new ObjectPoolItemVM(objClass));
            }
        }

        public ObjectPool ObjectPool
        {
            get { return _objectPool; }
        }

        public ObservableCollection<ObjectPoolItemVM> ObjectClasses
        {
            get { return _vms; }
        }

        private ObjectPoolItemVM _selected;

        public ObjectPoolItemVM SelectedObjectClass
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("SelectedObjectClass");

                    /*if (_selected != null) {
                        PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _selected.Tile;
                    }*/
                }
            }
        }

        public string Name
        {
            get { return _objectPool.Name; }
        }
    }

    public class ObjectPoolCollectionVM : ViewModelBase, ObjectPoolManagerService
    {
        private NamedResourceCollection<ObjectPool> _pools;
        private ObservableCollection<ObjectPoolVM> _vms;

        public ObjectPoolCollectionVM ()
        {
            _vms = new ObservableCollection<ObjectPoolVM>();
        }

        public ObjectPoolCollectionVM (NamedResourceCollection<ObjectPool> pools)
        {
            _pools = pools;
            _vms = new ObservableCollection<ObjectPoolVM>();

            foreach (ObjectPool pool in _pools) {
                _vms.Add(new ObjectPoolVM(pool));
            }

            if (_pools.Count > 0)
                ActiveObjectPool = _vms.First();

            //manager.Pools.ResourceAdded += HandlePoolAdded;
            //manager.Pools.ResourceRemoved += HandlePoolRemoved;
        }

        public ObservableCollection<ObjectPoolVM> ObjectPools
        {
            get { return _vms; }
        }

        public bool HasTilePools
        {
            get { return _pools.Count > 0; }
        }

        private ObjectPoolVM _selected;

        public ObjectPoolVM ActiveObjectPool
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("ActiveObjectPool");
                    //RefreshCommandState();

                    /*if (_selected != null) {
                        PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _manager.Pools[_selected.Name];
                    }*/
                }
            }
        }

        public ObjectPoolItemVM ActiveObjectClass
        {
            get
            {
                ObjectPoolVM pool = ActiveObjectPool;
                if (pool != null)
                    return pool.SelectedObjectClass;
                return null;
            }
        }
    }
}
