using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Treefrog.Framework;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Controllers;

namespace Treefrog.Extensibility
{
    public abstract class ProjectExplorerComponent
    { }

    public abstract class ProjectPanelComponent
    {
        protected ProjectPanelComponent (TreeNode rootNode)
        {
            RootNode = rootNode;

            NodeMap = new Dictionary<Guid, List<TreeNode>>();
        }

        protected TreeNode RootNode { get; private set; }

        protected abstract bool ControllerValid { get; }

        protected Dictionary<Guid, List<TreeNode>> NodeMap { get; private set; }

        public virtual bool ShouldHandle (Guid tag)
        {
            return NodeMap.ContainsKey(tag);
        }

        public void ShowContextMenu (UICommandController commandController, Point location, Guid tag)
        {
            ContextMenuStrip contextMenu = CommandMenuBuilder.BuildContextMenu(GetCommandMenu(tag));

            commandController.BindCommandManager(CommandManager);
            commandController.Clear();
            commandController.MapMenuItems(contextMenu.Items);

            contextMenu.Show(RootNode.TreeView, location);
        }

        protected virtual CommandMenu GetCommandMenu (Guid tag)
        {
            return new CommandMenu("");
        }

        protected virtual CommandManager CommandManager
        {
            get { return null; }
        }

        public virtual void DefaultAction (Guid tag)
        { }

        public virtual void Sync ()
        { }

        protected void SyncNode (TreeNode node, Action<TreeNode> action)
        {
            ClearNodeCache(node);
            node.Nodes.Clear();

            if (!ControllerValid)
                return;

            action(node);
        }

        protected void ClearNodeCache (TreeNode node)
        {
            if (node == null)
                return;

            if (node.Tag is Guid) {
                Guid uid = (Guid)node.Tag;
                if (NodeMap.ContainsKey(uid)) {
                    NodeMap[uid].Remove(node);
                    if (NodeMap[uid].Count == 0)
                        NodeMap.Remove(uid);
                }
            }

            foreach (TreeNode subNode in node.Nodes)
                ClearNodeCache(subNode);
        }

        protected void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon)
            where T : INamedResource
        {
            foreach (T resource in resources)
                AddResource(node, resource, icon, null);
        }

        protected void AddResources<T> (TreeNode node, IResourceCollection<T> resources, int icon, Action<TreeNode, T> subNodeAction)
            where T : INamedResource
        {
            foreach (T resource in resources)
                AddResource(node, resource, icon, subNodeAction);
        }

        protected void AddResource<T> (TreeNode node, T resource, int icon)
            where T : INamedResource
        {
            AddResource(node, resource, icon, null);
        }

        protected void AddResource<T> (TreeNode node, T resource, int icon, Action<TreeNode, T> subNodeAction)
            where T : INamedResource
        {
            TreeNode subNode = new TreeNode(resource.Name, icon, icon) {
                Tag = resource.Uid,
            };

            if (subNodeAction != null)
                subNodeAction(subNode, resource);

            node.Nodes.Add(subNode);

            if (!NodeMap.ContainsKey(resource.Uid))
                NodeMap[resource.Uid] = new List<TreeNode>();
            if (!NodeMap[resource.Uid].Contains(subNode))
                NodeMap[resource.Uid].Add(subNode);
        }

        protected void RemoveResource<T> (T resource)
            where T : INamedResource
        {
            RemoveResource(resource, null);
        }

        protected void RemoveResource<T> (T resource, Action<TreeNode, T> nodeAction)
            where T : INamedResource
        {
            List<TreeNode> nodeList;
            if (!NodeMap.TryGetValue(resource.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList.ToArray()) {
                if (nodeAction != null)
                    nodeAction(node, resource);

                ClearNodeCache(node);
                node.Remove();
            }
        }

        protected void ModifyResource<T> (T resource)
            where T : INamedResource
        {
            ModifyResource(resource, null);
        }

        protected void ModifyResource<T> (T resource, Action<TreeNode, T> nodeAction)
            where T : INamedResource
        {
            List<TreeNode> nodeList;
            if (!NodeMap.TryGetValue(resource.Uid, out nodeList))
                return;

            foreach (TreeNode node in nodeList.ToArray()) {
                if (node.Text != resource.Name)
                    node.Text = resource.Name;

                if (nodeAction != null)
                    nodeAction(node, resource);
            }
        }
    }
}
