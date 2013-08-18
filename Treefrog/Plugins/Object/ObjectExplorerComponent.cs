using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Treefrog.Extensibility;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Utility;
using Treefrog.Windows.Panels;

namespace Treefrog.Plugins.Object
{
    class ObjectExplorerComponent : ProjectExplorerComponent, IBindable<ObjectPoolCollectionPresenter>
    {
        private enum EventBindings
        {
            ObjectAdded,
            ObjectRemoved,
            ObjectModified,
        }

        private ObjectPoolCollectionPresenter _controller;
        private IObjectPoolManager _objectPoolManager;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<ObjectClass>>> _objectEventBindings;

        public Guid ProjectObjectsTag { get; private set; }

        public IObjectPoolManager ObjectManager
        {
            get { return (_controller != null) ? _controller.ObjectPoolManager : null; }
        }

        public ObjectExplorerComponent ()
        {
            _objectEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<ObjectClass>>>() {
                { EventBindings.ObjectAdded, (s, e) => OnObjectAdded(new ResourceEventArgs<ObjectClass>(e.Resource)) },
                { EventBindings.ObjectRemoved, (s, e) => OnObjectRemoved(new ResourceEventArgs<ObjectClass>(e.Resource)) },
                { EventBindings.ObjectModified, (s, e) => OnObjectModified(new ResourceEventArgs<ObjectClass>(e.Resource)) },
            };

            ProjectObjectsTag = Guid.NewGuid();
        }

        public void Bind (ObjectPoolCollectionPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncObjectPoolManager -= SyncObjectPoolManager;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncObjectPoolManager += SyncObjectPoolManager;
                Bind(_controller.ObjectPoolManager);
            }
            else
                Bind((IObjectPoolManager)null);
        }

        private void SyncObjectPoolManager (object sender, EventArgs e)
        {
            Bind(_controller.ObjectPoolManager);
        }

        private void Bind (IObjectPoolManager manager)
        {
            if (_objectPoolManager == manager)
                return;

            if (_objectPoolManager != null) {
                _objectPoolManager.PoolAdded -= ObjectPoolAddedHandler;
                _objectPoolManager.PoolRemoved -= ObjectPoolRemovedHandler;

                foreach (ObjectPool pool in _objectPoolManager.Pools)
                    UnhookObjectPool(pool);
            }

            _objectPoolManager = manager;

            if (_objectPoolManager != null) {
                _objectPoolManager.PoolAdded += ObjectPoolAddedHandler;
                _objectPoolManager.PoolRemoved += ObjectPoolRemovedHandler;

                foreach (ObjectPool pool in _objectPoolManager.Pools)
                    HookObjectPool(pool);
            }
        }

        private void ObjectPoolAddedHandler (object sender, ResourceEventArgs<ObjectPool> e)
        {
            HookObjectPool(e.Resource);
        }

        private void ObjectPoolRemovedHandler (object sender, ResourceEventArgs<ObjectPool> e)
        {
            UnhookObjectPool(e.Resource);
        }

        private void UnhookObjectPool (ObjectPool pool)
        {
            if (pool != null) {
                pool.Objects.ResourceAdded -= _objectEventBindings[EventBindings.ObjectAdded];
                pool.Objects.ResourceRemoved -= _objectEventBindings[EventBindings.ObjectRemoved];
                pool.Objects.ResourceModified -= _objectEventBindings[EventBindings.ObjectModified];
            }
        }

        private void HookObjectPool (ObjectPool pool)
        {
            if (pool != null) {
                pool.Objects.ResourceAdded += _objectEventBindings[EventBindings.ObjectAdded];
                pool.Objects.ResourceRemoved += _objectEventBindings[EventBindings.ObjectRemoved];
                pool.Objects.ResourceModified += _objectEventBindings[EventBindings.ObjectModified];
            }
        }

        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectAdded;
        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectRemoved;
        public event EventHandler<ResourceEventArgs<ObjectClass>> ObjectModified;

        protected virtual void OnObjectAdded (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnObjectRemoved (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnObjectModified (ResourceEventArgs<ObjectClass> e)
        {
            var ev = ObjectModified;
            if (ev != null)
                ev(this, e);
        }

        public CommandManager CommandManager
        {
            get { return (_controller != null) ? _controller.CommandManager : null; }
        }

        public void DefaultAction (Guid uid)
        {
            if (_controller != null && _controller.ObjectPoolManager.Contains(uid))
                _controller.CommandManager.Perform(CommandKey.ObjectProtoEdit, uid);
        }

        public CommandMenu Menu (Guid uid)
        {
            if (_controller != null && _controller.ObjectPoolManager.Contains(uid))
                return ObjectProtoMenu(uid);

            return new CommandMenu("");
        }

        public CommandMenu ObjectProtoMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoEdit, uid) { Default = true },
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoClone, uid),
                    new CommandMenuEntry(CommandKey.ObjectProtoDelete, uid),
                    new CommandMenuEntry(CommandKey.ObjectProtoRename, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.ObjectProtoProperties, uid),
                },
            });
        }
    }

    class ObjectExplorerPanelComponent : ProjectPanelComponent, IBindable<ObjectExplorerComponent>
    {
        private ObjectExplorerComponent _controller;
        private TreeNode _objectNode;

        public ObjectExplorerPanelComponent ()
        { }

        protected override void InitializeCore ()
        {
            Reset();
        }

        private void Reset ()
        {
            _objectNode = new TreeNode("Object Groups", ProjectPanel.IconIndex.FolderObjects, ProjectPanel.IconIndex.FolderObjects);

            RootNode.Nodes.Add(_objectNode);
        }

        public void Bind (ObjectExplorerComponent controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.ObjectAdded -= ObjectAddedHandler;
                _controller.ObjectRemoved -= ObjectRemovedHandler;
                _controller.ObjectModified -= ObjectModifiedHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.ObjectAdded += ObjectAddedHandler;
                _controller.ObjectRemoved += ObjectRemovedHandler;
                _controller.ObjectModified += ObjectModifiedHandler;

                _objectNode.Tag = _controller.ProjectObjectsTag;
            }
        }

        protected override bool ControllerValid
        {
            get { return _controller != null && _controller.ObjectManager != null; }
        }

        protected override CommandMenu GetCommandMenu (Guid tag)
        {
            return _controller.Menu(tag);
        }

        protected override CommandManager CommandManager
        {
            get { return _controller.CommandManager; }
        }

        public override void DefaultAction (Guid tag)
        {
            _controller.DefaultAction(tag);
        }

        public override void Sync ()
        {
            SyncNode(_objectNode, (node) => AddResources<ObjectPool>(_objectNode, _controller.ObjectManager.Pools, ProjectPanel.IconIndex.ObjectGroup, (subNode, r) => {
                AddResources<ObjectClass>(subNode, r.Objects, ProjectPanel.IconIndex.ObjectGroup);
            }));
        }

        private void ObjectAddedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            ObjectPool pool = _controller.ObjectManager.Pools.First(p => p.Objects.Contains(e.Uid));
            
            List<TreeNode> nodeList;
            if (pool == null || !NodeMap.TryGetValue(pool.Uid, out nodeList))
                return;

            foreach (TreeNode poolNode in nodeList)
                AddResource(poolNode, e.Resource, ProjectPanel.IconIndex.ObjectGroup);
        }

        private void ObjectRemovedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            RemoveResource(e.Resource);
        }

        private void ObjectModifiedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            ModifyResource(e.Resource);
        }
    }
}
