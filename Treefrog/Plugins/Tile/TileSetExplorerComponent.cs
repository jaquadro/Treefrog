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
using Treefrog.Plugins.Tiles;

namespace Treefrog.Plugins.Tiles
{
    class TileSetExplorerComponent : ProjectExplorerComponent, IBindable<TilePoolListPresenter>
    {
        private enum EventBindings
        {
            TilePoolAdded,
            TilePoolRemoved,
            TilePoolModified,
        }

        private TilePoolListPresenter _conroller;
        private ITilePoolManager _tilePoolManager;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<TilePool>>> _tilePoolEventBindings;

        public Guid ProjectTilesetsTag { get; private set; }

        public ITilePoolManager TileSetManager
        {
            get { return _conroller.TilePoolManager; }
        }

        public TileSetExplorerComponent ()
        {
            _tilePoolEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<TilePool>>>() {
                { EventBindings.TilePoolAdded, (s, e) => OnTilePoolAdded(new ResourceEventArgs<TilePool>(e.Resource)) },
                { EventBindings.TilePoolRemoved, (s, e) => OnTilePoolRemoved(new ResourceEventArgs<TilePool>(e.Resource)) },
                { EventBindings.TilePoolModified, (s, e) => OnTilePoolModified(new ResourceEventArgs<TilePool>(e.Resource)) },
            };

            ProjectTilesetsTag = Guid.NewGuid();
        }

        public void Bind (TilePoolListPresenter controller)
        {
            if (_conroller == controller)
                return;

            if (_conroller != null) {
                _conroller.SyncTilePoolManager -= SyncTilePoolManager;
            }

            _conroller = controller;

            if (_conroller != null) {
                _conroller.SyncTilePoolManager += SyncTilePoolManager;
                Bind(_conroller.TilePoolManager);
            }
            else
                Bind((TilePoolManager)null);
        }

        private void SyncTilePoolManager (object sender, EventArgs e)
        {
            Bind(_conroller.TilePoolManager);
        }

        private void Bind (ITilePoolManager manager)
        {
            if (_tilePoolManager == manager)
                return;

            if (_tilePoolManager != null) {
                _tilePoolManager.PoolAdded -= _tilePoolEventBindings[EventBindings.TilePoolAdded];
                _tilePoolManager.PoolRemoved -= _tilePoolEventBindings[EventBindings.TilePoolRemoved];
                _tilePoolManager.PoolModified -= _tilePoolEventBindings[EventBindings.TilePoolModified];
            }

            _tilePoolManager = manager;

            if (_tilePoolManager != null) {
                _tilePoolManager.PoolAdded += _tilePoolEventBindings[EventBindings.TilePoolAdded];
                _tilePoolManager.PoolRemoved += _tilePoolEventBindings[EventBindings.TilePoolRemoved];
                _tilePoolManager.PoolModified += _tilePoolEventBindings[EventBindings.TilePoolModified];
            }
        }

        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolAdded;
        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolRemoved;
        public event EventHandler<ResourceEventArgs<TilePool>> TilePoolModified;

        protected virtual void OnTilePoolAdded (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTilePoolRemoved (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnTilePoolModified (ResourceEventArgs<TilePool> e)
        {
            var ev = TilePoolModified;
            if (ev != null)
                ev(this, e);
        }

        public CommandManager CommandManager
        {
            get { return _conroller.CommandManager; }
        }

        public void DefaultAction (Guid uid)
        {
            //if (_conroller.TilePoolManager.Contains(uid))
            //    _conroller.CommandManager.Perform(CommandKey.TilePoolEdit, uid);
        }

        public CommandMenu Menu (Guid uid)
        {
            if (_conroller.TilePoolManager.Contains(uid))
                return TileSetMenu(uid);

            return new CommandMenu("");
        }

        public CommandMenu TileSetMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.TilePoolDelete, uid),
                    new CommandMenuEntry(CommandKey.TilePoolRename, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.TilePoolProperties, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.TilePoolExport, uid),
                    new CommandMenuEntry(CommandKey.TilePoolImportOver, uid),
                },
            });
        }
    }

    class TileSetExplorerPanelComponent : ProjectPanelComponent, IBindable<TileSetExplorerComponent>
    {
        private TileSetExplorerComponent _controller;
        private TreeNode _tileNode;

        public TileSetExplorerPanelComponent (TreeNode rootNode)
            : base(rootNode)
        {
            Reset();
        }

        private void Reset ()
        {
            _tileNode = new TreeNode("TileSets", ProjectPanel.IconIndex.FolderTiles, ProjectPanel.IconIndex.FolderTiles);

            RootNode.Nodes.Add(_tileNode);
        }

        public void Bind (TileSetExplorerComponent controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.TilePoolAdded -= TilePoolAddedHandler;
                _controller.TilePoolRemoved -= TilePoolRemovedHandler;
                _controller.TilePoolModified -= TilePoolModifiedHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.TilePoolAdded += TilePoolAddedHandler;
                _controller.TilePoolRemoved += TilePoolRemovedHandler;
                _controller.TilePoolModified += TilePoolModifiedHandler;

                _tileNode.Tag = _controller.ProjectTilesetsTag;
            }
        }

        protected override bool ControllerValid
        {
            get { return _controller != null && _controller.TileSetManager != null; }
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
            SyncNode(_tileNode, (node) => AddResources<TilePool>(_tileNode, _controller.TileSetManager.Pools, ProjectPanel.IconIndex.TileGroup));
        }

        private void TilePoolAddedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            AddResource(_tileNode, e.Resource, ProjectPanel.IconIndex.TileGroup);

            // TODO: Library syncing

            /*Library library = _controller.Project.LibraryManager.Libraries.First(lib => {
                return lib.TilePoolManager.Pools.Contains(e.Uid);
            });

            List<TreeNode> nodeList;
            if (library == null || !NodeMap.TryGetValue(library.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList)
                AddResource(node.Nodes["Tilesets"], e.Resource, ProjectPanel.IconIndex.TileGroup);*/
        }

        private void TilePoolRemovedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            RemoveResource(e.Resource);
        }

        private void TilePoolModifiedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            ModifyResource(e.Resource);
        }
    }
}
