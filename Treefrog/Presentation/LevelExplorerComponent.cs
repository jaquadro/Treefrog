using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Extensibility;
using Treefrog.Framework;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Utility;
using Treefrog.Windows.Panels;

namespace Treefrog.Presentation
{
    class LevelExplorerComponent : ProjectExplorerComponent, IBindable<EditorPresenter>
    {
        private enum EventBindings
        {
            LevelAdded,
            LevelRemoved,
            LevelModified,
        }

        private EditorPresenter _controller;
        private Project _project;

        private Dictionary<EventBindings, EventHandler<ResourceEventArgs<Level>>> _levelEventBindings;

        public Guid ProjectLevelsTag { get; private set; }

        public LevelExplorerComponent ()
        {
            _levelEventBindings = new Dictionary<EventBindings, EventHandler<ResourceEventArgs<Level>>>() {
                { EventBindings.LevelAdded, (s, e) => OnLevelAdded(new ResourceEventArgs<Level>(e.Resource)) },
                { EventBindings.LevelRemoved, (s, e) => OnLevelRemoved(new ResourceEventArgs<Level>(e.Resource)) },
                { EventBindings.LevelModified, (s, e) => OnLevelModified(new ResourceEventArgs<Level>(e.Resource)) },
            };

            ProjectLevelsTag = Guid.NewGuid();
        }

        public Project Project
        {
            get { return (_controller != null) ? _controller.Project : null; }
        }

        public void Bind (EditorPresenter controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.SyncCurrentProject -= SyncCurrentProject;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.SyncCurrentProject += SyncCurrentProject;
                Bind(_controller.Project);
            }
            else
                Bind((Project)null);
        }

        private void SyncCurrentProject (object sender, EventArgs e)
        {
            Bind(_controller.Project);
        }

        private void Bind (Project project)
        {
            if (_project == project)
                return;

            if (_project != null) {
                _project.Levels.ResourceAdded -= _levelEventBindings[EventBindings.LevelAdded];
                _project.Levels.ResourceRemoved -= _levelEventBindings[EventBindings.LevelRemoved];
                _project.Levels.ResourceModified -= _levelEventBindings[EventBindings.LevelModified];
            }

            _project = project;

            if (_project != null) {
                _project.Levels.ResourceAdded += _levelEventBindings[EventBindings.LevelAdded];
                _project.Levels.ResourceRemoved += _levelEventBindings[EventBindings.LevelRemoved];
                _project.Levels.ResourceModified += _levelEventBindings[EventBindings.LevelModified];
            }
        }

        public event EventHandler<ResourceEventArgs<Level>> LevelAdded;
        public event EventHandler<ResourceEventArgs<Level>> LevelRemoved;
        public event EventHandler<ResourceEventArgs<Level>> LevelModified;

        protected virtual void OnLevelAdded (ResourceEventArgs<Level> e)
        {
            var ev = LevelAdded;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLevelRemoved (ResourceEventArgs<Level> e)
        {
            var ev = LevelRemoved;
            if (ev != null)
                ev(this, e);
        }

        protected virtual void OnLevelModified (ResourceEventArgs<Level> e)
        {
            var ev = LevelModified;
            if (ev != null)
                ev(this, e);
        }

        public CommandManager CommandManager
        {
            get { return (_controller != null) ? _controller.CommandManager : null; }
        }

        public void DefaultAction (Guid uid)
        {
            if (_controller != null && _controller.Project.Levels.Contains(uid))
                _controller.OpenLevel(uid);
        }

        public CommandMenu Menu (Guid uid)
        {
            if (_controller != null && _controller.Project.Levels.Contains(uid))
                return LevelMenu(uid);

            return new CommandMenu("");
        }

        public CommandMenu LevelMenu (Guid uid)
        {
            return new CommandMenu("", new List<CommandMenuGroup>() {
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelOpen, uid) { Default = true },
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelClone, uid), 
                    new CommandMenuEntry(CommandKey.LevelDelete, uid),
                    new CommandMenuEntry(CommandKey.LevelRename, uid),
                },
                new CommandMenuGroup() {
                    new CommandMenuEntry(CommandKey.LevelProperties, uid),
                },
            });
        }
    }

    class LevelPanelComponent : ProjectPanelComponent, IBindable<LevelExplorerComponent>
    {
        private LevelExplorerComponent _controller;
        private TreeNode _levelNode;

        public LevelPanelComponent ()
        { }

        protected override void InitializeCore ()
        {
            Reset();
        }

        private void Reset ()
        {
            _levelNode = new TreeNode("Levels", ProjectPanel.IconIndex.FolderLevels, ProjectPanel.IconIndex.FolderLevels);

            RootNode.Nodes.Add(_levelNode);
        }

        public void Bind (LevelExplorerComponent controller)
        {
            if (_controller == controller)
                return;

            if (_controller != null) {
                _controller.LevelAdded -= LevelAddedHandler;
                _controller.LevelRemoved -= LevelRemovedHandler;
                _controller.LevelModified -= LevelModifiedHandler;
            }

            _controller = controller;

            if (_controller != null) {
                _controller.LevelAdded += LevelAddedHandler;
                _controller.LevelRemoved += LevelRemovedHandler;
                _controller.LevelModified += LevelModifiedHandler;

                _levelNode.Tag = _controller.ProjectLevelsTag;
            }
        }

        protected override bool ControllerValid
        {
            get { return _controller != null; }
        }

        protected override CommandManager CommandManager
        {
            get { return _controller.CommandManager; }
        }

        protected override CommandMenu GetCommandMenu (Guid tag)
        {
            return _controller.Menu(tag);
        }

        public override void DefaultAction (Guid tag)
        {
            _controller.DefaultAction(tag);
        }

        public override void Sync ()
        {
            SyncNode(_levelNode, (node) => AddResources<Level>(_levelNode, _controller.Project.Levels, ProjectPanel.IconIndex.Level, (subNode, r) => {
                //subNode.ContextMenuStrip = CommandMenuBuilder.BuildContextMenu(_controller.LevelMenu(r.Uid));
            }));
        }

        private void LevelAddedHandler (object sender, ResourceEventArgs<Level> e)
        {
            AddResource(_levelNode, e.Resource, ProjectPanel.IconIndex.Level);
        }

        private void LevelRemovedHandler (object sender, ResourceEventArgs<Level> e)
        {
            RemoveResource(e.Resource);
        }

        private void LevelModifiedHandler (object sender, ResourceEventArgs<Level> e)
        {
            ModifyResource(e.Resource);
        }
    }
}
