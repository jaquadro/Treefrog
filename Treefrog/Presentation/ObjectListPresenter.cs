using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Treefrog.Presentation
{
    public interface IObjectPoolListPresenter
    {
        bool CanCreateObjectPool { get; }
        bool CanRemoveObjectPool { get; }
        bool CanShowObjectSetProperties { get; }

        IEnumerable<ObjectPool> ObjectPoolList { get; }
        ObjectPool SelectedObjectPool { get; }

        void ActionCreateObjectPool ();
        void ActionRemoveSelectedObjectPool ();
        void ActionSelectObjectPool (string name);
        void ActionShowObjectPoolProperties ();

        void RefreshObjectPoolList ();
    }

    public interface IObjectPoolPresenter
    {
        bool CanCreateObjectClass { get; }
        bool CanRemoveObjectClass { get; }
        bool CanShowObjectClassProperties { get; }

        IEnumerable<ObjectClass> ObjectClassList { get; }
        ObjectClass SelectedObjectClass { get; }

        event EventHandler ObjectAdded;
        event EventHandler ObjectRemoved;
        event EventHandler ObjectSelectionChanged;

        void ActionCreateObjectClass ();
        void ActionRemoveSeletedObjectClass ();
        void ActionSelectObjectClass (string name);
        void ActionShowObjectClassProperties ();

        void RefreshObjectClass ();
    }

    class ObjectListPresenter
    {
    }

    public class ObjectPoolPresenter : IObjectPoolPresenter
    {
        #region Fields

        private IEditorPresenter _editor;

        private ObjectPool _pool;

        private string _selectedObject;
        private ObjectClass _selectedObjectRef;

        private Dictionary<string, ObjectClass> _selectedObjects;

        #endregion

        #region Constructors

        public ObjectPoolPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += SyncCurrentProjectHandler;
        }

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            _selectedObjects = new Dictionary<string, ObjectClass>();

            //_editor.Project.TilePools.ResourceRemapped += TilePool_NameChanged;

            //SelectTilePool();

            //OnSyncTilePoolActions(EventArgs.Empty);
            //OnSyncTilePoolList(EventArgs.Empty);
            //OnSyncTilePoolControl(EventArgs.Empty);
        }

        #endregion


        #region IObjectPoolPresenter Members

        public bool CanCreateObjectClass
        {
            get { return true; }
        }

        public bool CanRemoveObjectClass
        {
            get { return _selectedObjectRef != null; }
        }

        public bool CanShowObjectClassProperties
        {
            get { return _selectedObjectRef != null; }
        }

        public IEnumerable<ObjectClass> ObjectClassList
        {
            get
            {
                if (_pool == null)
                    yield break;

                foreach (ObjectClass obj in _pool)
                    yield return obj;
            }
        }

        public ObjectClass SelectedObjectClass
        {
            get { return _selectedObjectRef; }
        }

        public void ActionCreateObjectClass ()
        {
            throw new NotImplementedException();
        }

        public void ActionRemoveSeletedObjectClass ()
        {
            throw new NotImplementedException();
        }

        public void ActionSelectObjectClass (string name)
        {
            throw new NotImplementedException();
        }

        public void ActionShowObjectClassProperties ()
        {
            throw new NotImplementedException();
        }

        public void RefreshObjectClass ()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IObjectPoolPresenter Members


        public event EventHandler ObjectAdded;

        public event EventHandler ObjectRemoved;

        public event EventHandler ObjectSelectionChanged;

        #endregion
    }
}
