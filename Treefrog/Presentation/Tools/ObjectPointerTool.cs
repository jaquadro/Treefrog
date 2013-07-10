using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation.Commands;
using Treefrog.Framework.Model;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation.Tools
{
    public class ObjectPointerTool : PointerTool
    {
        //private ObjectPoolManagerService _poolManager;

        private ObjectPoolCollectionPresenter _objectPool;
        private ILayerContext _layerContext;

        //private CommandHistory _history;
        private ObjectLayer _layer;

        private Size _gridSize;
        private SnappingManager _snapManager;

        public ObjectPointerTool (ILayerContext layerContext, ObjectLayer layer, Size gridSize)
        {
            _layerContext = layerContext;
            //_history = history;
            _layer = layer;
            _gridSize = gridSize;
        }

        protected override void DisposeManaged ()
        {
            BindObjectSourceController(null);

            base.DisposeManaged();
        }

        public void BindObjectSourceController (ObjectPoolCollectionPresenter controller)
        {
            if (_objectPool == controller)
                return;

            if (_objectPool != null) {
                _objectPool.ObjectSelectionChanged -= ObjectSelectionChanged;
            }

            _objectPool = controller;

            if (_objectPool != null) {
                _objectPool.ObjectSelectionChanged += ObjectSelectionChanged;
            }
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            _snapManager = new SnappingManager(GetSnappingSourceOrigin(), GetSnappingSourceBounds(), GridSize);
        }

        protected override void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            if (_snapManager == null)
                _snapManager = new SnappingManager(GetSnappingSourceOrigin(), GetSnappingSourceBounds(), GridSize);
        }

        protected override void PointerLeaveFieldCore ()
        {
            _snapManager = null;
        }

        protected ObjectLayer Layer
        {
            get { return _layer; }
        }

        protected ILayerContext LayerContext
        {
            get { return _layerContext; }
        }

        protected CommandHistory History
        {
            get { return _layerContext.History; }
        }

        protected Size GridSize
        {
            get { return _gridSize; }
        }

        protected SnappingManager SnapManager
        {
            get { return _snapManager; }
        }

        private void ObjectSelectionChanged (object sender, EventArgs e)
        {
            OnObjectSelectionChanged(EventArgs.Empty);
        }

        protected virtual void OnObjectSelectionChanged (EventArgs e)
        { }

        /*protected ObjectPoolManagerService PoolManagerService
        {
            get
            {
                if (_poolManager == null)
                    _poolManager = ServiceContainer.Default.GetService<ObjectPoolManagerService>();
                return _poolManager;
            }
        }*/

        protected ObjectClass ActiveObjectClass
        {
            get
            {
                /*if (PoolManagerService == null)
                    return null;

                ObjectPoolItemVM activeClass = PoolManagerService.ActiveObjectClass;
                if (activeClass == null)
                    return null;
                return activeClass.ObjectClass;*/

                if (_objectPool == null)
                    return null;

                return _objectPool.SelectedObject;
            }
        }

        protected ObjectSnappingTarget SnappingTarget
        {
            get
            {
                if (_objectPool != null)
                    return _objectPool.SnappingTarget;
                return ObjectSnappingTarget.None;
            }
        }

        protected ObjectSnappingSource SnappingSource
        {
            get
            {
                if (_objectPool != null)
                    return _objectPool.SnappingReference;
                return ObjectSnappingSource.ImageBounds;
            }
        }

        protected SnappingManager GetSnappingManager (ObjectClass objClass)
        {
            return new SnappingManager(GetSnappingSourceOrigin(objClass), GetSnappingSourceBounds(objClass), GridSize);
        }

        private Point GetSnappingSourceOrigin ()
        {
            return GetSnappingSourceOrigin(ActiveObjectClass);
        }

        protected Point GetSnappingSourceOrigin (ObjectClass objClass)
        {
            if (objClass == null)
                return Point.Zero;

            return objClass.Origin;
        }

        private Rectangle GetSnappingSourceBounds ()
        {
            if (ActiveObjectClass is RasterObjectClass)
                return GetSnappingSourceBounds(ActiveObjectClass as RasterObjectClass);
            else
                return GetSnappingSourceBounds(ActiveObjectClass);
        }

        protected Rectangle GetSnappingSourceBounds (RasterObjectClass objClass)
        {
            if (objClass == null)
                return Rectangle.Empty;

            switch (SnappingSource) {
                case ObjectSnappingSource.ImageBounds:
                    return objClass.ImageBounds;
                case ObjectSnappingSource.MaskBounds:
                    return objClass.MaskBounds;
                case ObjectSnappingSource.Origin:
                    return new Rectangle(objClass.Origin, Size.Zero);
                default:
                    return Rectangle.Empty;
            }
        }

        protected Rectangle GetSnappingSourceBounds (ObjectClass objClass)
        {
            if (objClass == null)
                return Rectangle.Empty;

            return new Rectangle(objClass.Origin, Size.Zero);
        }
    }
}
