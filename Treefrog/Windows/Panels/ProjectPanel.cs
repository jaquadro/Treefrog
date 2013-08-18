using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Treefrog.Extensibility;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Utility;
using Treefrog.Windows.Controllers;

namespace Treefrog.Windows.Panels
{
    public partial class ProjectPanel : UserControl
    {
        public static class IconIndex
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

        private TreeNode _libraryRoot;

        private Dictionary<Guid, List<TreeNode>> _nodeMap = new Dictionary<Guid, List<TreeNode>>();

        private UICommandController _commandController;

        public ProjectPanel ()
        {
            InitializeComponent();

            _tree.NodeMouseDoubleClick += TreeNodeDoubleClickHandler;
            _tree.NodeMouseClick += TreeNodeClickHandler;

            _commandController = new UICommandController();

            ComponentManager = new InstanceRegistry<ProjectPanelComponent>();
            ComponentManager.InstanceRegistered += (s, e) => {
                e.Instance.Initialize(_rootNode);

                if (_controller != null)
                    BindingHelper.TryBindAny(e.Instance, _controller.Components.Select(c => new KeyValuePair<Type, object>(c.Key, c.Value)));
            };

            ResetComponent();
        }

        public InstanceRegistry<ProjectPanelComponent> ComponentManager { get; private set; }

        private void ResetComponent ()
        {
            _tree.Nodes.Clear();

            _rootNode = new TreeNode("Project", IconIndex.Application, IconIndex.Application);

            foreach (var component in ComponentManager.RegisteredInstances)
                component.Reset(_rootNode);

            _tree.Nodes.Add(_rootNode);

            _libraryRoot = new TreeNode("Libraries", IconIndex.LibraryGroup, IconIndex.LibraryGroup);

            _tree.Nodes.Add(_libraryRoot);

            _rootNode.Expand();
        }

        public void BindController (ProjectExplorerPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.ProjectReset -= ProjectResetHandler;
                _controller.DefaultLibraryChanged -= DefaultLibraryChangedHandler;

                _controller.LibraryAdded -= LibraryAddedHandler;
                _controller.LibraryRemoved -= LibraryRemovedHandler;
                _controller.LibraryModified -= LibraryModifiedHandler;

                foreach (var component in ComponentManager.RegisteredInstances)
                    BindingHelper.TryBindAny(component, _controller.Components.Select(c => new KeyValuePair<Type, object>(c.Key, null)));
            }

            _controller = controller;

            if (_controller != null) {
                _commandController.BindCommandManager(_controller.CommandManager);

                _controller.ProjectReset += ProjectResetHandler;
                _controller.DefaultLibraryChanged += DefaultLibraryChangedHandler;

                _controller.LibraryAdded += LibraryAddedHandler;
                _controller.LibraryRemoved += LibraryRemovedHandler;
                _controller.LibraryModified += LibraryModifiedHandler;

                _rootNode.Tag = _controller.ProjectManagerTag;
                _libraryRoot.Tag = _controller.LibraryManagerTag;

                foreach (var component in ComponentManager.RegisteredInstances)
                    BindingHelper.TryBindAny(component, _controller.Components.Select(c => new KeyValuePair<Type, object>(c.Key, c.Value)));

                SyncAll();
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

        private void DefaultLibraryChangedHandler (object sender, EventArgs e)
        {
            foreach (TreeNode node in _libraryRoot.Nodes) {
                if (!(node.Tag is Guid))
                    continue;

                Guid libraryUid = (Guid)node.Tag;
                Library library = _controller.Project.LibraryManager.Libraries[libraryUid];

                if (library == _controller.Project.DefaultLibrary) {
                    node.ImageIndex = IconIndex.LibraryDefault;
                    node.SelectedImageIndex = IconIndex.LibraryDefault;
                }
                else {
                    node.ImageIndex = IconIndex.Library;
                    node.SelectedImageIndex = IconIndex.Library;
                }
            }
        }

        private void LibraryAddedHandler (object sender, ResourceEventArgs<Library> e)
        {
            AddResource(_libraryRoot, e.Resource, IconIndex.Library, (subNode, r) => {
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
            });

            if (_libraryRoot.Nodes.Count > 0)
                _libraryRoot.Expand();
        }

        private void LibraryRemovedHandler (object sender, ResourceEventArgs<Library> e)
        {

        }

        private void LibraryModifiedHandler (object sender, ResourceEventArgs<Library> e)
        {

        }

        private void SyncAll ()
        {
            SyncLibraries();

            foreach (var component in ComponentManager.RegisteredInstances)
                component.Sync();
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

            ProjectPanelComponent targetComponent = ComponentManager.RegisteredInstances.FirstOrDefault(c => c.ShouldHandle(tag));
            if (targetComponent != null) {
                targetComponent.DefaultAction(tag);
                return;
            }

            // TODO: Remove when fully on components
            _controller.DefaultAction(tag);
        }

        private void TreeNodeClickHandler (object sender, TreeNodeMouseClickEventArgs e)
        {
            _tree.SelectedNode = e.Node;

            if (_controller == null || !(e.Node.Tag is Guid))
                return;

            Guid tag = (Guid)e.Node.Tag;

            if (e.Button == MouseButtons.Right) {
                ProjectPanelComponent targetComponent = ComponentManager.RegisteredInstances.FirstOrDefault(c => c.ShouldHandle(tag));
                if (targetComponent != null) {
                    targetComponent.ShowContextMenu(_commandController, e.Location, tag);
                    return;
                }

                // TODO: Remove when fully on components
                ContextMenuStrip contextMenu = CommandMenuBuilder.BuildContextMenu(_controller.Menu(tag));

                _commandController.BindCommandManager(_controller.CommandManager);
                _commandController.Clear();
                _commandController.MapMenuItems(contextMenu.Items);

                contextMenu.Show(_tree, e.Location);
            }
        }
    }

    
}
