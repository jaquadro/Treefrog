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

    public interface IObjectPoolCollectionPresenter : ICommandSubscriber
    {
        bool CanAddObjectPool { get; }
        bool CanRemoveSelectedObjectPool { get; }
        bool CanShowSelectedObjectPoolProperties { get; }

        ObjectPoolManager ObjectPoolManager { get; }

        IEnumerable<ObjectPool> ObjectPoolCollection { get; }
        ObjectPool SelectedObjectPool { get; }
        ObjectClass SelectedObject { get; }                          // Send to IObjectPoolPresenter

        ObjectSnappingSource SnappingReference { get; }
        ObjectSnappingTarget SnappingTarget { get; }

        event EventHandler SyncObjectPoolManager;
        event EventHandler SyncObjectPoolActions;
        event EventHandler SyncObjectPoolCollection;
        event EventHandler SyncObjectPoolControl;               // Send to IObjectPoolPresenter
        event EventHandler ObjectSelectionChanged;

        event EventHandler<SyncObjectPoolEventArgs> SyncCurrentObjectPool;
        event EventHandler<SyncObjectEventArgs> SyncCurrentObject; // Send to IObjectPoolPresenter

        void ActionCreateObjectPool ();
        void ActionRemoveSelectedObjectPool ();
        void ActionSelectObjectPool (string name);
        void ActionShowObjectPoolProperties ();

        void ActionImportObject ();                             // Send to IObjectPoolPresenter
        void ActionRemoveSelectedObject ();                     // Send to IObjectPoolPresenter
        void ActionSelectObject (string objectClass);      // Send to IObjectPoolPresenter

        void RefreshObjectPoolCollection ();
    }

    public class ObjectPoolCollectionPresenter : IObjectPoolCollectionPresenter
    {
        private IEditorPresenter _editor;

        private string _selectedPool;
        private ObjectPool _selectedPoolRef;

        private Dictionary<string, ObjectClass> _selectedObjects;

        //private string _selectedObject;
        //private ObjectClass _selectedObjectRef;

        public ObjectPoolCollectionPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += SyncCurrentProjectHandler;

            InitializeCommandManager();
        }

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            _selectedObjects = new Dictionary<string, ObjectClass>();

            //_selectedObject = null;
            //_selectedObjectRef = null;

            //_editor.Project.ObjectPoolManager.Pools.ResourceRemapped += ObjectPool_NameChanged;

            SelectObjectPool();

            OnSyncObjectPoolManager(EventArgs.Empty);
            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);
        }

        #region Command Handling

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();
            //_commandManager.CommandInvalidated += HandleCommandInvalidated;

            _commandManager.Register(CommandKey.ObjectProtoImport, CommandCanImportObject, CommandImportObject);
            _commandManager.Register(CommandKey.ObjectProtoDelete, CommandCanRemoveObject, CommandRemoveObject);

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
                ImportObject form = new ImportObject();
                foreach (ObjectClass objClass in  SelectedObjectPool.Objects)
                    form.ReservedNames.Add(objClass.Name);

                if (form.ShowDialog() == DialogResult.OK) {
                    TextureResource resource = TextureResourceBitmapExt.CreateTextureResource(form.SourceFile);
                    ObjectClass objClass = new ObjectClass(form.ObjectName) {
                        MaskBounds = new Rectangle(form.MaskLeft ?? 0, form.MaskTop ?? 0,
                            form.MaskRight ?? 0 - form.MaskLeft ?? 0, form.MaskBottom ?? 0 - form.MaskTop ?? 0),
                        Origin = new Point(form.OriginX ?? 0, form.OriginY ?? 0),
                    };

                    SelectedObjectPool.AddObject(objClass);
                    objClass.Image = resource;

                    RefreshObjectPoolCollection();
                    OnSyncObjectPoolManager(EventArgs.Empty);
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
                SelectedObjectPool.RemoveObject(SelectedObject.Name);

                RefreshObjectPoolCollection();
                OnSyncObjectPoolManager(EventArgs.Empty);
            }
        }

        #endregion

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

        public ObjectPoolManager ObjectPoolManager
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

        public void ActionSelectObjectPool (string name)
        {
            if (_selectedPool != name) {
                SelectObjectPool(name);

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
                SelectedObjectPool.Objects.Remove(SelectedObject);
                _selectedObjects.Remove(_selectedPool);
            }

            SelectObject(_selectedPool);

            OnSyncObjectPoolActions(EventArgs.Empty);
            OnSyncObjectPoolCollection(EventArgs.Empty);
            OnSyncObjectPoolControl(EventArgs.Empty);
        }

        public void ActionSelectObject (string objectClass)
        {
            if (objectClass == null) {
                if (_selectedPool != null)
                    _selectedObjects.Remove(_selectedPool);

                OnSyncObjectPoolControl(EventArgs.Empty);
                OnObjectSelectionChanged(EventArgs.Empty);

                _editor.Presentation.PropertyList.Provider = null;
            }
            else if (SelectedObjectPool != null && SelectedObjectPool.Objects.Contains(objectClass)) {
                _selectedObjects[_selectedPool] = SelectedObjectPool.Objects[objectClass];

                OnSyncObjectPoolControl(EventArgs.Empty);
                OnObjectSelectionChanged(EventArgs.Empty);

                //_editor.Presentation.PropertyList.Provider = SelectedObjectPool.Objects[objectClass];
            }
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
            SelectObjectPool(null);

            foreach (ObjectPool pool in _editor.Project.ObjectPoolManager.Pools) {
                SelectObjectPool(pool.Name);
                return;
            }
        }

        private void SelectObjectPool (string objectPool)
        {
            ObjectPool prevPool = _selectedPoolRef;

            if (objectPool == _selectedPool)
                return;

            _selectedPool = null;
            _selectedPoolRef = null;

            // Bind new pool
            if (objectPool != null && _editor.Project.ObjectPoolManager.Pools.Contains(objectPool)) {
                _selectedPool = objectPool;
                _selectedPoolRef = _editor.Project.ObjectPoolManager.Pools[objectPool];
            }

            OnSyncCurrentObjectPool(new SyncObjectPoolEventArgs(prevPool));
        }

        private void SelectObject (string objectPool)
        {
            SelectObject(objectPool, null);

            if (_editor.Project.ObjectPoolManager.Pools.Contains(objectPool)) {
                foreach (ObjectClass objClass in _editor.Project.ObjectPoolManager.Pools[objectPool].Objects) {
                    SelectObject(objectPool, objClass.Name);
                    return;
                }
            }
        }

        private void SelectObject (string objectPool, string objectClass)
        {
            ObjectClass prevClass = _selectedObjects.ContainsKey(objectPool)
                ? _selectedObjects[objectPool]
                : null;

            if (prevClass.Name == objectClass)
                return;

            _selectedObjects.Remove(objectPool);

            // Bind new object
            if (objectClass != null && _editor.Project.ObjectPoolManager.Pools.Contains(objectPool)
                && _editor.Project.ObjectPoolManager.Pools[objectPool].Objects.Contains(objectClass)) 
            {
                _selectedObjects[objectPool] = _editor.Project.ObjectPoolManager.Pools[objectPool].Objects[objectClass];
            }

            OnSyncCurrentObject(new SyncObjectEventArgs(prevClass));
        }
    }
}
