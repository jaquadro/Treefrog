using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Commands;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Tools
{
    public class ObjectPointerTool : PointerTool
    {
        private ObjectPoolManagerService _poolManager;

        private CommandHistory _history;
        private ObjectLayer _layer;

        private Size _gridSize;
        private SnappingManager _snapManager;

        public ObjectPointerTool (CommandHistory history, ObjectLayer layer, Size gridSize)
        {
            _history = history;
            _layer = layer;
            _gridSize = gridSize;
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ViewportVM viewport)
        {
            _snapManager = new SnappingManager(GetSnappingSourceOrigin(), GetSnappingSourceBounds(), GridSize);
        }

        protected override void PointerPositionCore (PointerEventInfo info, ViewportVM viewport)
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

        protected CommandHistory History
        {
            get { return _history; }
        }

        protected Size GridSize
        {
            get { return _gridSize; }
        }

        protected SnappingManager SnapManager
        {
            get { return _snapManager; }
        }

        protected ObjectPoolManagerService PoolManagerService
        {
            get
            {
                if (_poolManager == null)
                    _poolManager = ServiceContainer.Default.GetService<ObjectPoolManagerService>();
                return _poolManager;
            }
        }

        protected ObjectClass ActiveObjectClass
        {
            get
            {
                if (PoolManagerService == null)
                    return null;

                ObjectPoolItemVM activeClass = PoolManagerService.ActiveObjectClass;
                if (activeClass == null)
                    return null;
                return activeClass.ObjectClass;
            }
        }

        protected ObjectSnappingTarget SnappingTarget
        {
            get
            {
                if (PoolManagerService == null)
                    return ObjectSnappingTarget.None;
                return PoolManagerService.SelectedSnappingTarget;
            }
        }

        protected ObjectSnappingSource SnappingSource
        {
            get
            {
                if (PoolManagerService == null)
                    return ObjectSnappingSource.ImageBounds;
                return PoolManagerService.SelectedSnappingSource;
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
            return GetSnappingSourceBounds(ActiveObjectClass);
        }

        protected Rectangle GetSnappingSourceBounds (ObjectClass objClass)
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
    }
}
