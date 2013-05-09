using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Presentation;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Panels
{
    public partial class ProjectPanel : UserControl
    {
        private static class IconIndex
        {
            public static int Application = 0;
            public static int Folder = 1;
            public static int FolderLevels = 2;
            public static int FolderObjects = 3;
            public static int FolderTiles = 4;
            public static int FolderLayers = 5;
            public static int ObjectGroup = 6;
            public static int TileGroup = 7;
            public static int Level = 8;
            public static int Library = 9;
            public static int LibraryDefault = 10;
            public static int LibraryGroup = 11;
        }

        private ProjectExplorerPresenter _controller;

        private TreeNode _rootNode;
        private TreeNode _levelNode;
        private TreeNode _objectNode;
        private TreeNode _tileNode;

        private TreeNode _libraryRoot;

        private Dictionary<Guid, List<TreeNode>> _nodeMap = new Dictionary<Guid, List<TreeNode>>();

        private UICommandController _commandController;

        public ProjectPanel ()
        {
            InitializeComponent();

            _tree.NodeMouseDoubleClick += TreeNodeDoubleClickHandler;
            _tree.NodeMouseClick += TreeNodeClickHandler;

            _commandController = new UICommandController();

            ResetComponent();
        }

        private void ResetComponent ()
        {
            _tree.Nodes.Clear();

            _rootNode = new TreeNode("Project", IconIndex.Application, IconIndex.Application);
            _levelNode = new TreeNode("Levels", IconIndex.FolderLevels, IconIndex.FolderLevels);
            _tileNode = new TreeNode("Tilesets", IconIndex.FolderTiles, IconIndex.FolderTiles);
            _objectNode = new TreeNode("Object Groups", IconIndex.FolderObjects, IconIndex.FolderObjects);

            _rootNode.Nodes.AddRange(new TreeNode[] {
                _levelNode, _tileNode, _objectNode,
            });

            _tree.Nodes.Add(_rootNode);

            _libraryRoot = new TreeNode("Libraries", IconIndex.LibraryGroup, IconIndex.LibraryGroup);

            _tree.Nodes.Add(_libraryRoot);

            _rootNode.Expand();
            _levelNode.Expand();
        }

        public void BindController (ProjectExplorerPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.ProjectReset -= ProjectResetHandler;

                _controller.LibraryAdded -= LibraryAddedHandler;
                _controller.LibraryRemoved -= LibraryRemovedHandler;
                _controller.LibraryModified -= LibraryModifiedHandler;

                _controller.LevelAdded -= LevelAddedHandler;
                _controller.LevelRemoved -= LevelRemovedHandler;
                _controller.LevelModified -= LevelModifiedHandler;

                _controller.ObjectAdded -= ObjectAddedHandler;
                _controller.ObjectRemoved -= ObjectRemovedHandler;
                _controller.ObjectModified -= ObjectModifiedHandler;

                _controller.TilePoolAdded -= TilePoolAddedHandler;
                _controller.TilePoolRemoved -= TilePoolRemovedHandler;
                _controller.TilePoolModified -= TilePoolModifiedHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _commandController.BindCommandManager(_controller.CommandManager);

                _controller.ProjectReset += ProjectResetHandler;

                _controller.LibraryAdded += LibraryAddedHandler;
                _controller.LibraryRemoved += LibraryRemovedHandler;
                _controller.LibraryModified += LibraryModifiedHandler;

                _controller.LevelAdded += LevelAddedHandler;
                _controller.LevelRemoved += LevelRemovedHandler;
                _controller.LevelModified += LevelModifiedHandler;

                _controller.ObjectAdded += ObjectAddedHandler;
                _controller.ObjectRemoved += ObjectRemovedHandler;
                _controller.ObjectModified += ObjectModifiedHandler;

                _controller.TilePoolAdded += TilePoolAddedHandler;
                _controller.TilePoolRemoved += TilePoolRemovedHandler;
                _controller.TilePoolModified += TilePoolModifiedHandler;
            }
            else {
                _commandController.BindCommandManager(null);

                ResetComponent();
            }
        }

        private void ProjectResetHandler (object sender, EventArgs e)
        {
            SyncAll();
        }

        private void LibraryAddedHandler (object sender, ResourceEventArgs<Library> e)
        {

        }

        private void LibraryRemovedHandler (object sender, ResourceEventArgs<Library> e)
        {

        }

        private void LibraryModifiedHandler (object sender, ResourceEventArgs<Library> e)
        {

        }

        private void SyncLevels ()
        {
            SyncNode(_levelNode, (node) => AddResources<Level>(_levelNode, _controller.Project.Levels, IconIndex.Level, (subNode, r) => {
                //subNode.ContextMenuStrip = CommandMenuBuilder.BuildContextMenu(_controller.LevelMenu(r.Uid));
            }));
        }

        private void LevelAddedHandler (object sender, ResourceEventArgs<Level> e)
        {
            AddResource(_levelNode, e.Resource, IconIndex.Level);
        }

        private void LevelRemovedHandler (object sender, ResourceEventArgs<Level> e)
        {
            RemoveResource(e.Resource);
        }

        private void LevelModifiedHandler (object sender, ResourceEventArgs<Level> e)
        {
            ModifyResource(e.Resource);
        }

        private void SyncAll ()
        {
            SyncLibraries();
            SyncLevels();
            SyncTilePools();
            SyncObjectPools();
        }

        private void SyncLibraries ()
        {
            SyncNode(_libraryRoot, (node) => AddResources<Library>(_libraryRoot, _controller.Project.LibraryManager.Libraries, IconIndex.Library, (subNode, r) => {
                if (r == _controller.Project.DefaultLibrary) {
                    subNode.ImageIndex = IconIndex.LibraryDefault;
                    subNode.SelectedImageIndex = IconIndex.LibraryDefault;
                }

                TreeNode tileSets = new TreeNode("Tilesets", IconIndex.FolderTiles, IconIndex.FolderTiles) { Name = "Tilesets" };
                TreeNode objects = new TreeNode("Objects", IconIndex.FolderObjects, IconIndex.FolderObjects) { Name = "Objects" };

                subNode.Nodes.Add(tileSets);
                subNode.Nodes.Add(objects);

                AddResources<TilePool>(tileSets, r.TilePoolManager.Pools, IconIndex.TileGroup);
                AddResources<ObjectPool>(objects, r.ObjectPoolManager.Pools, IconIndex.ObjectGroup, (objSubNode, r2) => {
                    AddResources<ObjectClass>(objSubNode, r2.Objects, IconIndex.ObjectGroup);
                });
            }));

            if (_libraryRoot.Nodes.Count > 0)
                _libraryRoot.Expand();
        }

        private void SyncTilePools ()
        {
            SyncNode(_tileNode, (node) => AddResources<TilePool>(_tileNode, _controller.Project.TilePoolManager.Pools, IconIndex.TileGroup));
        }

        private void TilePoolAddedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            AddResource(_tileNode, e.Resource, IconIndex.TileGroup);

            Library library = _controller.Project.LibraryManager.Libraries.First(lib => {
                return lib.TilePoolManager.Pools.Contains(e.Uid);
            });

            List<TreeNode> nodeList;
            if (library == null || !_nodeMap.TryGetValue(library.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList)
                AddResource(node.Nodes["Tilesets"], e.Resource, IconIndex.TileGroup);
        }

        private void TilePoolRemovedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            RemoveResource(e.Resource);
        }

        private void TilePoolModifiedHandler (object sender, ResourceEventArgs<TilePool> e)
        {
            ModifyResource(e.Resource);
        }

        private void SyncObjectPools ()
        {
            SyncNode(_objectNode, (node) => AddResources<ObjectPool>(_objectNode, _controller.Project.ObjectPoolManager.Pools, IconIndex.ObjectGroup, (subNode, r) => {
                AddResources<ObjectClass>(subNode, r.Objects, IconIndex.ObjectGroup);
            }));
        }

        private void ObjectAddedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            ObjectPool pool = _controller.Project.ObjectPoolManager.Pools.First(p => {
                return p.Objects.Contains(e.Uid);
            });

            List<TreeNode> nodeList;
            if (pool == null || !_nodeMap.TryGetValue(pool.Uid, out nodeList))
                return;

            foreach (TreeNode poolNode in nodeList)
                AddResource(poolNode, e.Resource, IconIndex.ObjectGroup);
        }

        private void ObjectRemovedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            RemoveResource(e.Resource);
        }

        private void ObjectModifiedHandler (object sender, ResourceEventArgs<ObjectClass> e)
        {
            ModifyResource(e.Resource);
        }

        private void SyncNode (TreeNode node, Action<TreeNode> action)
        {
            if (_controller == null)
                return;

            ClearNodeCache(node);
            node.Nodes.Clear();

            if (_controller.Project == null)
                return;

            action(node);
        }

        private void ClearNodeCache (TreeNode node)
        {
            if (node == null)
                return;

            if (node.Tag is Guid) {
                Guid uid = (Guid)node.Tag;
                if (_nodeMap.ContainsKey(uid)) {
                    _nodeMap[uid].Remove(node);
                    if (_nodeMap[uid].Count == 0)
                        _nodeMap.Remove(uid);
                }
            }

            foreach (TreeNode subNode in node.Nodes)
                ClearNodeCache(subNode);
        }

        private void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon)
            where T : INamedResource
        {
            foreach (T resource in resources)
                AddResource(node, resource, icon, null);
        }

        private void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon, Action<TreeNode, T> subNodeAction)
            where T : INamedResource
        {
            foreach (T resource in resources)
                AddResource(node, resource, icon, subNodeAction);
        }

        private void AddResource<T> (TreeNode node, T resource, int icon)
            where T : INamedResource
        {
            AddResource(node, resource, icon, null);
        }

        private void AddResource<T> (TreeNode node, T resource, int icon, Action<TreeNode, T> subNodeAction)
            where T : INamedResource
        {
            TreeNode subNode = new TreeNode(resource.Name, icon, icon) {
                Tag = resource.Uid,
            };

            if (subNodeAction != null)
                subNodeAction(subNode, resource);

            node.Nodes.Add(subNode);

            if (!_nodeMap.ContainsKey(resource.Uid))
                _nodeMap[resource.Uid] = new List<TreeNode>();
            if (!_nodeMap[resource.Uid].Contains(subNode))
                _nodeMap[resource.Uid].Add(subNode);
        }

        private void RemoveResource<T> (T resource)
            where T : INamedResource
        {
            RemoveResource(resource, null);
        }

        private void RemoveResource<T> (T resource, Action<TreeNode, T> nodeAction)
            where T : INamedResource
        {
            List<TreeNode> nodeList;
            if (!_nodeMap.TryGetValue(resource.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList.ToArray()) {
                if (nodeAction != null)
                    nodeAction(node, resource);

                ClearNodeCache(node);
                node.Remove();
            }
        }

        private void ModifyResource<T> (T resource)
            where T : INamedResource
        {
           ModifyResource(resource, null);
        }

        private void ModifyResource<T> (T resource, Action<TreeNode, T> nodeAction)
            where T : INamedResource
        {
            List<TreeNode> nodeList;
            if (!_nodeMap.TryGetValue(resource.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList.ToArray()) {
                if (node.Text != resource.Name)
                    node.Text = resource.Name;

                if (nodeAction != null)
                    nodeAction(node, resource);
            }
        }

        private void TreeNodeDoubleClickHandler (object sender, TreeNodeMouseClickEventArgs e)
        {
            if (_controller == null || !(e.Node.Tag is Guid))
                return;

            Guid tag = (Guid)e.Node.Tag;

            _controller.DefaultAction(tag);
        }

        private void TreeNodeClickHandler (object sender, TreeNodeMouseClickEventArgs e)
        {
            _tree.SelectedNode = e.Node;

            if (_controller == null || !(e.Node.Tag is Guid))
                return;

            Guid tag = (Guid)e.Node.Tag;

            if (e.Button == MouseButtons.Right) {
                ContextMenuStrip contextMenu = CommandMenuBuilder.BuildContextMenu(_controller.Menu(tag));

                _commandController.Clear();
                _commandController.MapMenuItems(contextMenu.Items);

                Point location = new Point(Cursor.Position.X + _tree.Location.X, Cursor.Position.Y + _tree.Location.Y);
                Point p = _tree.PointToClient(location);

                contextMenu.Show(_tree, e.Location);

                //_commandController.Clear();
            }
        }
    }
}
