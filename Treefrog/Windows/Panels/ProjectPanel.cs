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

        public ProjectPanel ()
        {
            InitializeComponent();

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

        /*public void BindController (IEditorPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncContentTabs -= SyncContentTabsHandler;
                _controller.Presentation.ObjectPoolCollection.SyncObjectPoolCollection -= SyncObjectPoolCollectionHandler;
                _controller.Presentation.TilePoolList.SyncTilePoolList -= SyncTilePoolListHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncContentTabs += SyncContentTabsHandler;
                _controller.Presentation.ObjectPoolCollection.SyncObjectPoolCollection += SyncObjectPoolCollectionHandler;
                _controller.Presentation.TilePoolList.SyncTilePoolList += SyncTilePoolListHandler;

                SyncAll();
            }
            else {
                ResetComponent();
            }
        }*/

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
            }

            _controller = controller;

            if (_controller != null) {
                _controller.ProjectReset += ProjectResetHandler;

                _controller.LibraryAdded += LibraryAddedHandler;
                _controller.LibraryRemoved += LibraryRemovedHandler;
                _controller.LibraryModified += LibraryModifiedHandler;

                _controller.LevelAdded += LevelAddedHandler;
                _controller.LevelRemoved += LevelRemovedHandler;
                _controller.LevelModified += LevelModifiedHandler;
            }
            else
                ResetComponent();
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
            if (_controller == null)
                return;

            _levelNode.Nodes.Clear();

            if (_controller.Project == null)
                return;

            foreach (Level level in _controller.Project.Levels)
                AddLevel(level);

            if (_levelNode.Nodes.Count > 0)
                _levelNode.Expand();
        }

        private void AddLevel (Level level)
        {
            TreeNode levelNode = new TreeNode(level.Name, IconIndex.Level, IconIndex.Level) {
                Tag = level.Uid,
            };

            _levelNode.Nodes.Add(levelNode);
        }

        private void RemoveLevel (Level level)
        {
            foreach (TreeNode node in _levelNode.Nodes) {
                if ((Guid)node.Tag == level.Uid) {
                    _levelNode.Nodes.Remove(node);
                    break;
                }
            }
        }

        private void SyncLevel (Level level)
        {
            foreach (TreeNode node in _levelNode.Nodes) {
                if ((Guid)node.Tag == level.Uid) {
                    if (node.Text != level.Name)
                        node.Text = level.Name;
                    break;
                }
            }
        }

        private void LevelAddedHandler (object sender, ResourceEventArgs<Level> e)
        {
            AddLevel(e.Resource);
        }

        private void LevelRemovedHandler (object sender, ResourceEventArgs<Level> e)
        {
            RemoveLevel(e.Resource);
        }

        private void LevelModifiedHandler (object sender, ResourceEventArgs<Level> e)
        {
            SyncLevel(e.Resource);
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

                TreeNode tileSets = new TreeNode("Tilesets", IconIndex.FolderTiles, IconIndex.FolderTiles);
                TreeNode objects = new TreeNode("Objects", IconIndex.FolderObjects, IconIndex.FolderObjects);

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

        private void SyncObjectPools ()
        {
            SyncNode(_objectNode, (node) => AddResources<ObjectPool>(_objectNode, _controller.Project.ObjectPoolManager.Pools, IconIndex.ObjectGroup, (subNode, r) => {
                AddResources<ObjectClass>(subNode, r.Objects, IconIndex.ObjectGroup);
            }));
        }

        private void SyncNode (TreeNode node, Action<TreeNode> action)
        {
            if (_controller == null)
                return;

            node.Nodes.Clear();

            if (_controller.Project == null)
                return;

            action(node);
        }

        private void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon)
            where T : INamedResource
        {
            AddResources<T>(node, resources, icon, null);
        }

        private void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon, Action<TreeNode, T> subNodeAction)
            where T : INamedResource
        {
            foreach (T resource in resources) {
                TreeNode subNode = new TreeNode(resource.Name, icon, icon) {
                    Tag = resource.Uid,
                };

                if (subNodeAction != null)
                    subNodeAction(subNode, resource);

                node.Nodes.Add(subNode);
            }
        }

        /*private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            _levelNode.Nodes.Clear();

            foreach (Level level in _controller.Project.Levels) {
                TreeNode node = new TreeNode(level.Name, IconIndex.Level, IconIndex.Level);

                _levelNode.Nodes.Add(node);
            }

            SyncLibraryListHandler(null, EventArgs.Empty);
        }

        private void SyncObjectPoolCollectionHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            _objectNode.Nodes.Clear();

            foreach (ObjectPool pool in _controller.Presentation.ObjectPoolCollection.ObjectPoolCollection) {
                TreeNode node = new TreeNode(pool.Name, IconIndex.ObjectGroup, IconIndex.ObjectGroup);

                _objectNode.Nodes.Add(node);
            }
        }

        private void SyncTilePoolListHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            _tileNode.Nodes.Clear();

            foreach (TilePoolPresenter pool in _controller.Presentation.TilePoolList.TilePoolList) {
                TreeNode node = new TreeNode(pool.TilePool.Name, IconIndex.TileGroup, IconIndex.TileGroup);

                _tileNode.Nodes.Add(node);
            }
        }*/
    }
}
