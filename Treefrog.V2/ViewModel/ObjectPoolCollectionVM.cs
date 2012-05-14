using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Treefrog.V2.ViewModel
{
    public interface ObjectPoolManagerService : INotifyPropertyChanged
    {
        ObjectPoolVM ActiveObjectPool { get; }
        ObjectPoolItemVM ActiveObjectClass { get; }

        ObjectSnappingSource SelectedSnappingSource { get; }
        ObjectSnappingTarget SelectedSnappingTarget { get; }
    }


    public class ObjectPoolItemVM : ViewModelBase
    {
        private ObjectPoolVM _parent;
        private ObjectClass _objClass;

        public ObjectPoolItemVM (ObjectPoolVM parent, ObjectClass objClass)
        {
            _parent = parent;
            _objClass = objClass;
        }

        public ObjectPoolVM Parent
        {
            get { return _parent; }
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
        private ObjectPoolCollectionVM _parent;
        private ObjectPool _objectPool;
        private ObservableCollection<ObjectPoolItemVM> _vms;

        public ObjectPoolVM (ObjectPoolCollectionVM parent, ObjectPool objectPool)
        {
            _parent = parent;
            _objectPool = objectPool;
            _vms = new ObservableCollection<ObjectPoolItemVM>();

            foreach (ObjectClass objClass in _objectPool) {
                _vms.Add(new ObjectPoolItemVM(this, objClass));
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

        public ObjectPoolCollectionVM Parent
        {
            get { return _parent; }
        }
    }

    public class ObjectPoolCollectionVM : ViewModelBase, ObjectPoolManagerService, IDisposable
    {
        private ObjectPoolManager _manager;
        private ObservableCollection<ObjectPoolVM> _vms;

        public ObjectPoolCollectionVM ()
        {
            _vms = new ObservableCollection<ObjectPoolVM>();
        }

        public ObjectPoolCollectionVM (ObjectPoolManager manager)
        {
            _manager = manager;
            _vms = new ObservableCollection<ObjectPoolVM>();

            foreach (ObjectPool pool in manager.Pools) {
                _vms.Add(new ObjectPoolVM(this, pool));
            }

            if (_vms.Count > 0)
                ActiveObjectPool = _vms.First();

            //manager.Pools.ResourceAdded += HandlePoolAdded;
            //manager.Pools.ResourceRemoved += HandlePoolRemoved;
        }

        public void Dispose ()
        {
            if (_manager != null) {
                //_manager.Pools.ResourceAdded -= HandlePoolAdded;
                //_manager.Pools.ResourceRemoved -= HandlePoolRemoved;
                _manager = null;

                ChangeActiveObjectPool(null);

                _vms.Clear();

                //RefreshCommandState();
            }
        }

        public ObservableCollection<ObjectPoolVM> ObjectPools
        {
            get { return _vms; }
        }

        public bool HasObjectPools
        {
            get { return _manager.Pools.Count > 0; }
        }

        /*private ObjectPoolVM _selected;

        public ObjectPoolVM ActiveObjectPool
        {
            get { return _selected; }
            set
            {
                if (_selected != value) {
                    _selected = value;
                    RaisePropertyChanged("ActiveObjectPool");
                    //RefreshCommandState();

                    if (_selected != null) {
                        PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                        service.ActiveProvider = _manager.Pools[_selected.Name];
                    }
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
        }*/

        #region ObjectPoolManagerService

        private ObjectPoolVM _activeObjectPool;

        public ObjectPoolVM ActiveObjectPool
        {
            get { return _activeObjectPool; }
            set
            {
                if (_activeObjectPool != value) {
                    ChangeActiveObjectPool(value);
                    //RefreshCommandState();

                    //if (_activeObjectPool != null) {
                    //    PropertyManagerService service = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<PropertyManagerService>();
                    //    service.ActiveProvider = _manager.Pools[_activeObjectPool.Name];
                    //}
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

        private void ChangeActiveObjectPool (ObjectPoolVM newPool)
        {
            if (_activeObjectPool != null)
                _activeObjectPool.PropertyChanged -= HandleActiveObjectPoolPropertyChanged;

            _activeObjectPool = newPool;
            if (_activeObjectPool != null)
                _activeObjectPool.PropertyChanged += HandleActiveObjectPoolPropertyChanged;

            RaisePropertyChanged("ActiveObjectPool");
            RaisePropertyChanged("ActiveObjectClass");
        }

        private void HandleActiveObjectPoolPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedObjectClass")
                RaisePropertyChanged("ActiveObjectClass");
        }

        #endregion

        // --- Split below into separate view-oriented VM?

        PreviewSize _previewSize = PreviewSize.Medium;
        ObjectSnappingSource _snappingSource = ObjectSnappingSource.ImageBounds;
        ObjectSnappingTarget _snappingTarget = ObjectSnappingTarget.None;

        public PreviewSize PreviewSize
        {
            get { return _previewSize; }
            set
            {
                if (_previewSize != value) {
                    _previewSize = value;
                    RaisePropertyChanged("PreviewSize");
                }
            }
        }

        public ObjectSnappingSource SelectedSnappingSource
        {
            get { return _snappingSource; }
            set
            {
                if (_snappingSource != value) {
                    _snappingSource = value;
                    RaisePropertyChanged("SelectedSnappingSource");
                }
            }
        }

        public ObjectSnappingTarget SelectedSnappingTarget
        {
            get { return _snappingTarget; }
            set
            {
                if (_snappingTarget != value) {
                    _snappingTarget = value;
                    RaisePropertyChanged("SelectedSnappingTarget");
                }
            }
        }

        #region Commands

        #region Preview Size Command

        private RelayCommand<PreviewSize> _previewCommand;

        public ICommand PreviewSizeCommand
        {
            get
            {
                if (_previewCommand == null)
                    _previewCommand = new RelayCommand<PreviewSize>(OnPreviewSize, CanPreviewSize);
                return _previewCommand;
            }
        }

        public bool IsPreviewSizeEnabled
        {
            get { return true; }
        }

        private bool CanPreviewSize (PreviewSize value)
        {
            return true;
        }

        private void OnPreviewSize (PreviewSize value)
        {
            PreviewSize = value;
        }

        #endregion

        #region Snapping Source Command

        private RelayCommand<ObjectSnappingSource> _selectSnappingSourceCommand;

        public ICommand SelectObjectSourceCommand
        {
            get
            {
                if (_selectSnappingSourceCommand == null)
                    _selectSnappingSourceCommand = new RelayCommand<ObjectSnappingSource>(OnSelectSnappingSource, CanSelectSnappingSource);
                return _selectSnappingSourceCommand;
            }
        }

        public bool IsSelectSnappingSourceEnabled
        {
            get { return true; }
        }

        private bool CanSelectSnappingSource (ObjectSnappingSource value)
        {
            return true;
        }

        private void OnSelectSnappingSource (ObjectSnappingSource value)
        {
            SelectedSnappingSource = value;
        }

        #endregion

        #region Snapping Target Command

        private RelayCommand<ObjectSnappingTarget> _selectSnappingTargetCommand;

        public ICommand SelectSnappingTargetCommand
        {
            get
            {
                if (_selectSnappingTargetCommand == null)
                    _selectSnappingTargetCommand = new RelayCommand<ObjectSnappingTarget>(OnSelectSnappingTarget, CanSelectSnappingTarget);
                return _selectSnappingTargetCommand;
            }
        }

        public bool IsSelectSnappingTargetEnabled
        {
            get { return true; }
        }

        private bool CanSelectSnappingTarget (ObjectSnappingTarget value)
        {
            return true;
        }

        private void OnSelectSnappingTarget (ObjectSnappingTarget value)
        {
            SelectedSnappingTarget = value;
        }

        #endregion

        #endregion
    }

    public enum PreviewSize
    {
        Large,
        Medium,
        Small,
    }

    public enum ObjectSnappingSource
    {
        ImageBounds,
        MaskBounds,
        Origin,
    }

    public enum ObjectSnappingTarget
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right,
        CenterHorizontal,
        CenterVertical,
        Center,
    }
}
