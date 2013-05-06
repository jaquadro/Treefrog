using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Presentation.Tools;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Forms;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Aux;

namespace Treefrog.Presentation
{
    public class SyncObjectPoolEventArgs : EventArgs
    {
        public ObjectPool PreviousObjectPool { get; private set; }

        public SyncObjectPoolEventArgs (ObjectPool objectPool)
        {
            PreviousObjectPool = objectPool;
        }
    }

    public class SyncObjectEventArgs : EventArgs
    {
        public ObjectClass PreviousObject { get; private set; }

        public SyncObjectEventArgs (ObjectClass objectClass)
        {
            PreviousObject = objectClass;
        }
    }

    public class ObjectPoolCollectionPresenter : ICommandSubscriber
    {
        private IEditorPresenter _editor;

        private Guid _selectedPool;
        private ObjectPool _selectedPoolRef;
        private IObjectPoolManager _poolManager;

        private Dictionary<Guid, ObjectClass> _selectedObjects;

        public ObjectPoolCollectionPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += SyncCurrentProjectHandler;

            InitializeCommandManager();
        }

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            _selectedObjects = new Dictionary<Guid, ObjectClass>();

            if (_editor.Project != null)
                BindObjectPoolManager(_editor.Project.ObjectPoolManager);
            else
                BindObjectPoolManager(null);

            //_selectedObject = null;
            //_selectedObjectRef = null;

            //_editor.Project.ObjectPoolManager.Pools.ResourceRemapped += ObjectPool_NameChanged;

            //SelectObjectPool();

            //OnSyncObjectPoolManager(EventArgs.Empty);
            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);

            InvalidateObjectProtoCommands();
        }

        public void BindObjectPoolManager (IObjectPoolManager manager)
        {
            if (_poolManager != null) {
                _poolManager.Pools.ResourceAdded -= ObjectPoolAdded;
                _poolManager.Pools.ResourceRemoved -= ObjectPoolRemoved;
                //_poolManager.Pools.ResourceRemapped -= TilePoolRemapped;
                //_poolManager.Pools.CollectionChanged -= TilePoolManagerChanged;
            }

            _poolManager = manager;
            if (_poolManager != null) {
                _poolManager.Pools.ResourceAdded += ObjectPoolAdded;
                _poolManager.Pools.ResourceRemoved += ObjectPoolRemoved;
                //_poolManager.Pools.ResourceRemapped += TilePoolRemapped;
                //_poolManager.Pools.CollectionChanged += TilePoolManagerChanged;

                InitializePoolPresenters();
            }
            else {
                ClearPoolPresenters();
            }

            OnSyncObjectPoolManager(EventArgs.Empty);
        }

        private void ClearPoolPresenters ()
        {
            _selectedPool = Guid.Empty;
            _selectedPoolRef = null;

            OnSyncObjectPoolCollection(EventArgs.Empty);
        }

        private void InitializePoolPresenters ()
        {
            ClearPoolPresenters();
            SelectObjectPool();

            //OnObjectPoolSelectionChanged(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
        }

        private void AddPoolPresenter (ObjectPool pool)
        {
            SelectObjectPool(pool.Uid);
            OnSyncObjectPoolCollection(EventArgs.Empty);
        }

        private void RemovePoolPresenter (Guid poolUid)
        {
            if (_selectedPool == poolUid)
                SelectObjectPool();
            OnSyncObjectPoolCollection(EventArgs.Empty);
        }

        private void ObjectPoolAdded (object sender, ResourceEventArgs<ObjectPool> e)
        {
            AddPoolPresenter(e.Resource);
        }

        private void ObjectPoolRemoved (object sender, ResourceEventArgs<ObjectPool> e)
        {
            RemovePoolPresenter(e.Resource.Uid);
        }

        #region Command Handling

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();
            //_commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.ObjectProtoImport, CommandCanImportObject, CommandImportObject);
            _commandManager.Register(CommandKey.ObjectProtoEdit, CommandCanEditObject, CommandEditObject);
            _commandManager.Register(CommandKey.ObjectProtoDelete, CommandCanRemoveObject, CommandRemoveObject);
            _commandManager.Register(CommandKey.ObjectProtoRename, CommandCanRename, CommandRename);
            _commandManager.Register(CommandKey.ObjectProtoProperties, CommandCanObjectProperties, CommandObjectProperties);

            _commandManager.RegisterToggleGroup(CommandToggleGroup.ObjectReference);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectReference, CommandKey.ObjectReferenceImage);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectReference, CommandKey.ObjectReferenceMask);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectReference, CommandKey.ObjectReferenceOrigin);

            _commandManager.RegisterToggleGroup(CommandToggleGroup.ObjectSnapping);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingBottom);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingBottomLeft);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingBottomRight);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingCenter);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingHorz);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingLeft);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingNone);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingRight);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingTop);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingTopLeft);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingTopRight);
            _commandManager.RegisterToggle(CommandToggleGroup.ObjectSnapping, CommandKey.ObjectSnappingVert);

            _commandManager.Perform(CommandKey.ObjectReferenceImage);
            _commandManager.Perform(CommandKey.ObjectSnappingNone);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanImportObject ()
        {
            return _selectedPool != null;
        }

        private void CommandImportObject ()
        {
            if (CommandCanImportObject()) {
                using (ImportObject form = new ImportObject()) {
                    foreach (ObjectClass objClass in SelectedObjectPool.Objects)
                        form.ReservedNames.Add(objClass.Name);

                    if (form.ShowDialog() == DialogResult.OK) {
                        TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(form.SourceFile);
                        ObjectClass objClass = new ObjectClass(form.ObjectName) {
                            MaskBounds = new Rectangle(form.MaskLeft ?? 0, form.MaskTop ?? 0,
                                (form.MaskRight ?? 0) - (form.MaskLeft ?? 0), (form.MaskBottom ?? 0) - (form.MaskTop ?? 0)),
                            Origin = new Point(form.OriginX ?? 0, form.OriginY ?? 0),
                        };

                        SelectedObjectPool.AddObject(objClass);
                        objClass.Image = resource;

                        RefreshObjectPoolCollection();
                        OnSyncObjectPoolManager(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanEditObject ()
        {
            return SelectedObjectPool != null && SelectedObject != null;
        }

        private void CommandEditObject ()
        {
            if (CommandCanEditObject()) {
                using (ImportObject form = new ImportObject(SelectedObject)) {
                    foreach (ObjectClass objClass in SelectedObjectPool.Objects) {
                        if (objClass.Name != SelectedObject.Name)
                            form.ReservedNames.Add(objClass.Name);
                    }

                    if (form.ShowDialog() == DialogResult.OK) {
                        if (form.SourceImage != null)
                            SelectedObject.Image = form.SourceImage;
                        SelectedObject.TrySetName(form.ObjectName);
                        SelectedObject.MaskBounds = new Rectangle(form.MaskLeft ?? 0, form.MaskTop ?? 0,
                            (form.MaskRight ?? 0) - (form.MaskLeft ?? 0), (form.MaskBottom ?? 0) - (form.MaskTop ?? 0));
                        SelectedObject.Origin = new Point(form.OriginX ?? 0, form.OriginY ?? 0);

                        RefreshObjectPoolCollection();
                        OnSyncObjectPoolManager(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanRemoveObject ()
        {
            return SelectedObjectPool != null && SelectedObject != null;
        }

        private void CommandRemoveObject ()
        {
            if (CommandCanRemoveObject()) {
                SelectedObjectPool.RemoveObject(SelectedObject.Uid);

                RefreshObjectPoolCollection();
                InvalidateObjectProtoCommands();
                OnSyncObjectPoolManager(EventArgs.Empty);
            }
        }

        private bool CommandCanRename ()
        {
            return SelectedObjectPool != null && SelectedObject != null;
        }

        private void CommandRename ()
        {
            if (CommandCanRename()) {
                using (NameChangeForm form = new NameChangeForm(SelectedObject.Name)) {
                    foreach (ObjectClass objClass in SelectedObjectPool.Objects) {
                        if (objClass.Name != SelectedObject.Name)
                            form.ReservedNames.Add(objClass.Name);
                    }

                    if (form.ShowDialog() == DialogResult.OK) {
                        SelectedObject.TrySetName(form.Name);
                        OnSyncObjectPoolManager(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanObjectProperties ()
        {
            return SelectedObjectPool != null && SelectedObject != null;
        }

        private void CommandObjectProperties ()
        {
            if (CommandCanObjectProperties()) {
                _editor.Presentation.PropertyList.Provider = SelectedObject;
                _editor.ActivatePropertyPanel();
            }
        }

        private void InvalidateObjectProtoCommands ()
        {
            CommandManager.Invalidate(CommandKey.ObjectProtoImport);
            CommandManager.Invalidate(CommandKey.ObjectProtoEdit);
            CommandManager.Invalidate(CommandKey.ObjectProtoClone);
            CommandManager.Invalidate(CommandKey.ObjectProtoDelete);
            CommandManager.Invalidate(CommandKey.ObjectProtoRename);
            CommandManager.Invalidate(CommandKey.ObjectProtoProperties);
        }

        #endregion

        public void ActionEditObject (Guid uid)
        {
            SelectObject(uid);
            if (SelectedObject != null && SelectedObject.Uid == uid) {
                CommandEditObject();
            }
        }

        #region Properties

        public bool CanAddObjectPool
        {
            get { return true; }
        }

        public bool CanRemoveSelectedObjectPool
        {
            get { return SelectedObjectPool != null; }
        }

        public bool CanShowSelectedObjectPoolProperties
        {
            get { return SelectedObjectPool != null; }
        }

        public IObjectPoolManager ObjectPoolManager
        {
            get { return _editor.Project.ObjectPoolManager; }
        }

        public IEnumerable<ObjectPool> ObjectPoolCollection
        {
            get
            {
                foreach (ObjectPool pool in _editor.Project.ObjectPoolManager.Pools) {
                    yield return pool;
                }
            }
        }

        public ObjectPool SelectedObjectPool
        {
            get { return _selectedPoolRef; }
        }

        public ObjectClass SelectedObject
        {
            get {
                ObjectPool pool = SelectedObjectPool;
                return (pool != null && _selectedObjects.ContainsKey(_selectedPool))
                    ? _selectedObjects[_selectedPool]
                    : null;
            }
        }

        public ObjectSnappingSource SnappingReference
        {
            get
            { 
                switch(_commandManager.SelectedCommand(CommandToggleGroup.ObjectReference)) {
                    case CommandKey.ObjectReferenceImage: return ObjectSnappingSource.ImageBounds;
                    case CommandKey.ObjectReferenceMask: return ObjectSnappingSource.MaskBounds;
                    case CommandKey.ObjectReferenceOrigin: return ObjectSnappingSource.Origin;
                    default: return ObjectSnappingSource.ImageBounds;
                }
            }
        }

        public ObjectSnappingTarget SnappingTarget
        {
            get
            {
                switch (_commandManager.SelectedCommand(CommandToggleGroup.ObjectSnapping)) {
                    case CommandKey.ObjectSnappingBottom: return ObjectSnappingTarget.Bottom;
                    case CommandKey.ObjectSnappingBottomLeft: return ObjectSnappingTarget.BottomLeft;
                    case CommandKey.ObjectSnappingBottomRight: return ObjectSnappingTarget.BottomRight;
                    case CommandKey.ObjectSnappingCenter: return ObjectSnappingTarget.Center;
                    case CommandKey.ObjectSnappingHorz: return ObjectSnappingTarget.CenterHorizontal;
                    case CommandKey.ObjectSnappingLeft: return ObjectSnappingTarget.Left;
                    case CommandKey.ObjectSnappingNone: return ObjectSnappingTarget.None;
                    case CommandKey.ObjectSnappingRight: return ObjectSnappingTarget.Right;
                    case CommandKey.ObjectSnappingTop: return ObjectSnappingTarget.Top;
                    case CommandKey.ObjectSnappingTopLeft: return ObjectSnappingTarget.TopLeft;
                    case CommandKey.ObjectSnappingTopRight: return ObjectSnappingTarget.TopRight;
                    case CommandKey.ObjectSnappingVert: return ObjectSnappingTarget.CenterVertical;
                    default: return ObjectSnappingTarget.None;
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler SyncObjectPoolManager;

        public event EventHandler SyncObjectPoolActions;

        public event EventHandler SyncObjectPoolCollection;

        public event EventHandler SyncObjectPoolControl;

        public event EventHandler<SyncObjectPoolEventArgs> SyncCurrentObjectPool;

        public event EventHandler<SyncObjectEventArgs> SyncCurrentObject;

        public event EventHandler ObjectSelectionChanged;

        #endregion

        #region Event Dispatchers

        protected virtual void OnSyncObjectPoolManager (EventArgs e)
        {
            if (SyncObjectPoolManager != null) {
                SyncObjectPoolManager(this, e);
            }
        }

        protected virtual void OnSyncObjectPoolActions (EventArgs e)
        {
            if (SyncObjectPoolActions != null) {
                SyncObjectPoolActions(this, e);
            }
        }

        protected virtual void OnSyncObjectPoolCollection (EventArgs e)
        {
            if (SyncObjectPoolCollection != null) {
                SyncObjectPoolCollection(this, e);
            }
        }

        protected virtual void OnSyncObjectPoolControl (EventArgs e)
        {
            if (SyncObjectPoolControl != null) {
                SyncObjectPoolControl(this, e);
            }
        }

        protected virtual void OnSyncCurrentObjectPool (SyncObjectPoolEventArgs e)
        {
            if (SyncCurrentObjectPool != null) {
                SyncCurrentObjectPool(this, e);
            }
        }

        protected virtual void OnSyncCurrentObject (SyncObjectEventArgs e)
        {
            if (SyncCurrentObject != null) {
                SyncCurrentObject(this, e);
            }
        }

        protected virtual void OnObjectSelectionChanged (EventArgs e)
        {
            if (ObjectSelectionChanged != null) {
                ObjectSelectionChanged(this, e);
            }
        }

        #endregion

        /*private void ObjectPool_NameChanged (object sender, NamedResourceEventArgs<ObjectPool> e)
        {
            if (e.Resource != null && e.Resource.Name == _selectedPool) {
                SelectObjectPool(e.Resource.Name);
            }
        }*/

        #region View Action API

        // TODO: Create Object Pool Dialog
        public void ActionCreateObjectPool ()
        {

        }

        public void ActionRemoveSelectedObjectPool ()
        {
            if (_selectedPool != null && _editor.Project.ObjectPoolManager.Pools.Contains(_selectedPool))
                _editor.Project.ObjectPoolManager.Pools.Remove(_selectedPool);

            SelectObjectPool();

            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);
        }

        public void ActionSelectObjectPool (Guid objectPoolUid)
        {
            if (_selectedPool != objectPoolUid) {
                SelectObjectPool(objectPoolUid);

                OnSyncObjectPoolActions(EventArgs.Empty);
                OnSyncObjectPoolCollection(EventArgs.Empty);

                if (SelectedObjectPool != null)
                    _editor.Presentation.PropertyList.Provider = SelectedObjectPool;
            }
        }

        public void ActionShowObjectPoolProperties ()
        {
            if (SelectedObjectPool != null)
                _editor.Presentation.PropertyList.Provider = SelectedObjectPool;
        }
        
        // TODO: Import Object Dialog
        public void ActionImportObject ()
        {

        }

        public void ActionRemoveSelectedObject ()
        {
            if (SelectedObject != null && SelectedObjectPool.Objects.Contains(SelectedObject)) {
                SelectedObjectPool.Objects.Remove(SelectedObject.Uid);
                _selectedObjects.Remove(_selectedPool);

                InvalidateObjectProtoCommands();
            }

            //SelectObject(_selectedPool);

            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);
        }

        public void ActionSelectObject (Guid objectClassUid)
        {
            if (objectClassUid == Guid.Empty) {
                if (_selectedPool != null)
                    _selectedObjects.Remove(_selectedPool);

                OnSyncObjectPoolControl(EventArgs.Empty);
                OnObjectSelectionChanged(EventArgs.Empty);

                _editor.Presentation.PropertyList.Provider = null;
            }
            else if (SelectedObjectPool != null && SelectedObjectPool.Objects.Contains(objectClassUid)) {
                _selectedObjects[_selectedPool] = SelectedObjectPool.Objects[objectClassUid];

                OnSyncObjectPoolControl(EventArgs.Empty);
                OnObjectSelectionChanged(EventArgs.Empty);

                _editor.Presentation.PropertyList.Provider = SelectedObjectPool.Objects[objectClassUid];
            }

            InvalidateObjectProtoCommands();
        }

        public void RefreshObjectPoolCollection ()
        {
            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);
        }

        #endregion

        private void SelectObjectPool ()
        {
            SelectObjectPool(Guid.Empty);

            foreach (ObjectPool pool in _editor.Project.ObjectPoolManager.Pools) {
                SelectObjectPool(pool.Uid);
                return;
            }
        }

        private void SelectObjectPool (Guid objectPoolUid)
        {
            ObjectPool prevPool = _selectedPoolRef;

            if (objectPoolUid == _selectedPool)
                return;

            _selectedPool = Guid.Empty;
            _selectedPoolRef = null;

            // Bind new pool
            if (objectPoolUid != Guid.Empty && _editor.Project.ObjectPoolManager.Pools.Contains(objectPoolUid)) {
                _selectedPool = objectPoolUid;
                _selectedPoolRef = _editor.Project.ObjectPoolManager.Pools[objectPoolUid];
            }

            InvalidateObjectProtoCommands();

            OnSyncCurrentObjectPool(new SyncObjectPoolEventArgs(prevPool));
        }

        private void SelectObject (Guid objectPoolUid)
        {
            SelectObject(objectPoolUid, Guid.Empty);

            if (_editor.Project.ObjectPoolManager.Pools.Contains(objectPoolUid)) {
                foreach (ObjectClass objClass in _editor.Project.ObjectPoolManager.Pools[objectPoolUid].Objects) {
                    SelectObject(objectPoolUid, objClass.Uid);
                    return;
                }
            }
        }

        private void SelectObject (Guid objectPoolUid, Guid objectClassUid)
        {
            ObjectClass prevClass = _selectedObjects.ContainsKey(objectPoolUid)
                ? _selectedObjects[objectPoolUid]
                : null;

            if (prevClass != null && prevClass.Uid == objectClassUid)
                return;

            _selectedObjects.Remove(objectPoolUid);

            // Bind new object
            if (objectClassUid != null && _editor.Project.ObjectPoolManager.Pools.Contains(objectPoolUid)
                && _editor.Project.ObjectPoolManager.Pools[objectPoolUid].Objects.Contains(objectClassUid)) 
            {
                _selectedObjects[objectPoolUid] = _editor.Project.ObjectPoolManager.Pools[objectPoolUid].Objects[objectClassUid];
            }

            InvalidateObjectProtoCommands();

            OnSyncCurrentObject(new SyncObjectEventArgs(prevClass));
        }
    }
}
