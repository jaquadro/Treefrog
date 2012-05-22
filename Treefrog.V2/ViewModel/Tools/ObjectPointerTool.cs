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

        protected override void PointerPositionCore (PointerEventInfo info)
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

        private Point GetSnappingSourceOrigin ()
        {
            if (ActiveObjectClass == null)
                return Point.Zero;

            return ActiveObjectClass.Origin;
        }

        private Rectangle GetSnappingSourceBounds ()
        {
            if (ActiveObjectClass == null)
                return Rectangle.Empty;

            switch (SnappingSource) {
                case ObjectSnappingSource.ImageBounds:
                    return ActiveObjectClass.ImageBounds;
                case ObjectSnappingSource.MaskBounds:
                    return ActiveObjectClass.MaskBounds;
                case ObjectSnappingSource.Origin:
                    return new Rectangle(ActiveObjectClass.Origin, Size.Zero);
                default:
                    return Rectangle.Empty;
            }
        }
    }
}
