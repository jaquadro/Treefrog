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
        }

        private IEditorPresenter _controller;
        private IObjectPoolCollectionPresenter _objectController;
        private ITilePoolListPresenter _tileController;

        private TreeNode _rootNode;
        private TreeNode _levelNode;
        private TreeNode _objectNode;
        private TreeNode _tileNode;

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

            _rootNode.Expand();
            _levelNode.Expand();
        }

        public void BindController (IEditorPresenter controller)
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
        }

        private void SyncAll ()
        {
            SyncContentTabsHandler(null, EventArgs.Empty);
            SyncObjectPoolCollectionHandler(null, EventArgs.Empty);
            SyncTilePoolListHandler(null, EventArgs.Empty);
        }

        private void SyncContentTabsHandler (object sender, EventArgs e)
        {
            if (_controller == null)
                return;

            _levelNode.Nodes.Clear();

            foreach (Level level in _controller.Project.Levels) {
                TreeNode node = new TreeNode(level.Name, IconIndex.Level, IconIndex.Level);

                _levelNode.Nodes.Add(node);
            }
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

            foreach (TilePool pool in _controller.Presentation.TilePoolList.TilePoolList) {
                TreeNode node = new TreeNode(pool.Name, IconIndex.TileGroup, IconIndex.TileGroup);

                _tileNode.Nodes.Add(node);
            }
        }
    }
}
